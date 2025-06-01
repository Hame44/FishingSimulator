using UnityEngine;
using System.Collections;

public partial class FishingLogic
{
    
    private void PullFish()
    {
        var session = controller.FishingService?.GetCurrentSession();
        if (session?.State != FishingState.Fighting || !controller.IsReeling) return;
        
        var fish = session.CurrentFish;
        if (fish == null) return;
        
        CalculatePullProgress(fish);
        UpdateFishPosition();
        CheckCatchCompletion();
        
        // Логування прогресу
        if (Time.time % 1f < 0.1f) // Кожну секунду
        {
            float progress = (1f - controller.CurrentFishDistance / controller.castDistance) * 100f;
            Debug.Log($"🎣 Прогрес витягування: {progress:F0}% (дистанція: {controller.CurrentFishDistance:F1}м)");
        }
    }
    
    private void CalculatePullProgress(Fish fish)
    {
        float fishResistance = fish.Strength / controller.CurrentPlayer.Strength;
        float basePullSpeed = controller.pullSpeed * Time.deltaTime;
        float adjustedPullSpeed = basePullSpeed / (1f + fishResistance * 0.5f);
        
        // Риба може чинити опір
        if (UnityEngine.Random.value < fishResistance * 0.3f)
        {
            adjustedPullSpeed *= 0.2f; // Сповільнення
            
            if (UnityEngine.Random.value < 0.1f)
            {
                adjustedPullSpeed = -adjustedPullSpeed * 0.5f; // Риба тягне назад
            }
        }
        
        float newDistance = controller.CurrentFishDistance - adjustedPullSpeed;
        controller.SetCurrentFishDistance(Mathf.Max(0, newDistance));
    }
    
    private void UpdateFishPosition()
    {
        if (controller.floatObject == null || controller.shore == null) return;
        
        float distanceRatio = controller.CurrentFishDistance / controller.castDistance;
        
        Vector3 startPos = controller.Animator.FloatTargetPosition;
        Vector3 endPos = controller.shore.position;
        Vector3 newFloatPosition = Vector3.Lerp(endPos, startPos, distanceRatio);
        
        // Ефект тремтіння під час боротьби
        Vector3 fightOffset = new Vector3(
            UnityEngine.Random.Range(-0.05f, 0.05f),
            UnityEngine.Random.Range(-0.03f, 0.03f),
            0
        ) * controller.TensionLevel;
        
        controller.floatObject.transform.position = newFloatPosition + fightOffset;
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
        
        Debug.Log("🏆 Риба піймана!");
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
        return controller.TensionLevel > 0.9f && UnityEngine.Random.value < 0.02f;
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
}