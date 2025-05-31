using UnityEngine;
using System.Collections;

public partial class FishingGameLogic
{
    private void HandlePrematureHook()
    {
        controller.StartCoroutine(ShowTemporaryMessage("Передчасно! Чекайте поклювки...", 2f));
    }
    
    private void HandleFighting(FishingSession session)
    {
        if (session.State == FishingState.Fighting && controller.IsReeling)
        {
            // Логіка боротьби обробляється в PullFish()
            // HandleHookPullClick();
        }
    }
    
    private void CheckForCompletion(FishingSession session)
    {
        if (session.State == FishingState.Caught || session.State == FishingState.Escaped)
        {
            controller.StartCoroutine(HandleCompletion(session.State));
        }
    }
    
    private void CalculatePullProgress(Fish fish)
    {
        float fishResistance = fish.Strength / controller.CurrentPlayer.Strength;
        float basePullSpeed = controller.pullSpeed * Time.deltaTime;
        float adjustedPullSpeed = basePullSpeed / (1f + fishResistance * 0.5f);
        
        // Риба може опиратися
        if (UnityEngine.Random.value < fishResistance * 0.3f)
        {
            adjustedPullSpeed *= 0.2f;
            if (UnityEngine.Random.value < 0.1f)
            {
                adjustedPullSpeed = -adjustedPullSpeed * 0.5f; // Риба тягне назад
            }
        }
        
        float newDistance = controller.CurrentFishDistance - adjustedPullSpeed;
        controller.SetCurrentFishDistance(Mathf.Max(0, newDistance));

        if (Time.time % 0.5f < 0.1f) // Кожні 0.5 секунд
        {
            Debug.Log($"🐟 Дистанція риби: {controller.CurrentFishDistance:F2}м (швидкість: {adjustedPullSpeed:F3}м/с)");
        }
    }
    
    private void UpdateFishPosition()
    {
                if (controller.floatObject == null || controller.shore == null) return;
        
        // Розраховуємо позицію поплавка на основі дистанції до риби
        float distanceRatio = controller.CurrentFishDistance / controller.castDistance;
        
        // Поплавок рухається від початкової позиції до берега
        Vector3 startPos = controller.Animator.FloatTargetPosition; // Позиція закидання
        Vector3 endPos = controller.shore.position; // Позиція берега
        
        Vector3 newFloatPosition = Vector3.Lerp(endPos, startPos, distanceRatio);
        
        // Додаємо ефект боротьби - поплавок трясеться
        Vector3 fightOffset = new Vector3(
            UnityEngine.Random.Range(-0.05f, 0.05f),
            UnityEngine.Random.Range(-0.03f, 0.03f),
            0
        ) * controller.TensionLevel;
        
        controller.floatObject.transform.position = newFloatPosition + fightOffset;
        
        // Оновлюємо анімацію у FishingAnimator
        controller.Animator.AnimateFighting(distanceRatio, controller.TensionLevel);
    }
    
    private void CheckCatchCompletion()
    {
        if (controller.CurrentFishDistance <= 0.1f)
        {
            CompleteCatch();
        }
    }
    
    private void CompleteCatch()
    {
        var session = controller.FishingService.GetCurrentSession();
        if (session != null)
        {
            session.CompleteFishing(FishingResult.Success);
        }
        
        controller.SetReeling(false);
        controller.UIManager.UpdateStatusText("caught");
        StopFightSequence();
        controller.StartCoroutine(ResetAfterCompletion());
    }
    
    private void ReleaseFish()
    {
        var session = controller.FishingService.GetCurrentSession();
        if (session != null)
        {
            session.CompleteFishing(FishingResult.FishEscaped);
        }
        
        controller.UIManager.UpdateStatusText("escaped");
        StopFightSequence();
        controller.StartCoroutine(ResetAfterCompletion());
    }
    
    private void StopFightSequence()
    {
        if (controller.FightCoroutine != null)
        {
            controller.StopCoroutine(controller.FightCoroutine);
            controller.SetFightCoroutine(null);
        }
        
        controller.SetReeling(false);
        controller.UIManager.HideProgressBar();
    }
    
    private IEnumerator ReelInEmptyLine()
    {
        controller.UIManager.UpdateStatusText("Витягуємо порожню вудку...");
        controller.SetReeling(true);
        
        float reelTime = 2f;
        float elapsed = 0f;
        Vector3 startPos = controller.floatObject.transform.position;
        
        while (elapsed < reelTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / reelTime;
            
            Vector3 currentPos = Vector3.Lerp(startPos, controller.shore.position, progress);
            controller.floatObject.transform.position = currentPos;
            
            yield return null;
        }
        
        controller.Animator.ResetFloat();
        controller.VisualEffects.HideFishingLine();
        controller.SetReeling(false);
        controller.UIManager.UpdateStatusText("ready");
        controller.UIManager.UpdateButtonStates();
    }
    
    private IEnumerator HandleCompletion(FishingState completionState)
    {
        StopFightSequence();
        
        string message = completionState == FishingState.Caught ? "caught" : "escaped";
        controller.UIManager.UpdateStatusText(message);
        
        yield return controller.MediumDelay;
        
        yield return controller.StartCoroutine(ResetAfterCompletion());
    }
    
    private IEnumerator ResetAfterCompletion()
    {
        controller.Animator.ResetFloat();
        controller.VisualEffects.HideFishingLine();
        controller.SetReeling(false);
        controller.SetHooked(false);
        controller.SetFishBiting(false);
        
        yield return new WaitForSeconds(1f);
        
        controller.UIManager.UpdateStatusText("ready");
        controller.UIManager.UpdateButtonStates();
    }
    
    private IEnumerator ShowTemporaryMessage(string message, float duration)
    {
        controller.UIManager.UpdateStatusText(message);
        yield return new WaitForSeconds(duration);
        controller.UIManager.UpdateStatusText("waiting");
    }
    
    private IEnumerator FightSequenceCoroutine()
    {
        while (ShouldContinueFight())
        {
            controller.SetFightTimer(controller.FightTimer + Time.deltaTime);
            UpdateTension();
            controller.UIManager.UpdateProgressBar();
            
            if (CheckLineBroken())
            {
                HandleLineBroken();
                yield break;
            }
            
            yield return null;
        }
        
        HandleFightTimeout();
        controller.SetFightCoroutine(null);
    }
    
    private bool ShouldContinueFight()
    {
        return controller.IsReeling && 
               controller.CurrentFishDistance > 0.1f && 
               controller.FightTimer < controller.maxFightTime;
    }
    
    private void UpdateTension()
    {
        var session = controller.FishingService.GetCurrentSession();
        if (session?.CurrentFish == null) return;
        
        float fishStrength = session.CurrentFish.Strength;
        float distanceFactor = controller.CurrentFishDistance / controller.castDistance;
        float tension = (fishStrength * distanceFactor) / controller.CurrentPlayer.Strength;
        controller.SetTensionLevel(Mathf.Clamp01(tension));
    }
    
    private bool CheckLineBroken()
    {
        return controller.TensionLevel > 0.9f && UnityEngine.Random.value < 0.05f;
    }
    
    private void HandleLineBroken()
    {
        controller.UIManager.UpdateStatusText("Леска порвалась!");
        StopFightSequence();
        controller.StartCoroutine(ResetAfterCompletion());
    }
    
    private void HandleFightTimeout()
    {
        if (controller.FightTimer >= controller.maxFightTime)
        {
            controller.UIManager.UpdateStatusText("Час боротьби вийшов! Риба втекла...");
            ReleaseFish();
        }
    }
}
