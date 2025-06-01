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
/// ^
/// |   PRIVATE METHODs
/// 
using UnityEngine;
using System.Collections;

public partial class FishingGameLogic
{
    private FishingController controller;
    
    public FishingGameLogic(FishingController controller)
    {
        this.controller = controller;
    }
    
    public void UpdateGameLogic()
    {
        var session = controller.FishingService?.GetCurrentSession();
        if (session == null) return;
        
        UpdateSessionState(session);
        HandleFighting(session);
        CheckForCompletion(session);
    }
    
    private void UpdateSessionState(FishingSession session)
    {
        if (controller.CurrentState != session.State)
        {
            controller.SetCurrentState(session.State);
            controller.UIManager.UpdateButtonStates();
            
            // Оновлюємо стан клювання
            bool wasBiting = controller.IsFishBiting;
            bool isBiting = session.State == FishingState.Biting;
            controller.SetFishBiting(isBiting);
            
            // Якщо почалося клювання, оновлюємо UI
            if (!wasBiting && isBiting)
            {
                controller.UIManager.UpdateStatusText("biting");
                controller.VisualEffects.PlayBiteEffect();
            }
        }
    }
    
    public IEnumerator CastLineCoroutine()
    {
        controller.UIManager.UpdateStatusText("cast");
        
        yield return controller.StartCoroutine(controller.Animator.CastAnimation());
        
        StartFishingSession();
        StartFloatBobbing();
    }
    
    private void StartFishingSession()
    {
        controller.FishingService.StartFishing(controller.CurrentPlayer);
        controller.UIManager.UpdateStatusText("waiting");
        controller.UIManager.UpdateButtonStates();
    }
    
    private void StartFloatBobbing()
    {
        var bobCoroutine = controller.StartCoroutine(controller.Animator.FloatBobbing());
        controller.SetFloatBobCoroutine(bobCoroutine);
    }
    
    public void Hook()
    {
        if (IsProcessingAction()) return;
        
        var session = controller.FishingService.GetCurrentSession();
        
        if (session == null)
        {
            HandleEmptyLine();
            return;
        }
        
        HandleSessionAction(session);
    }
    
    private void HandleEmptyLine()
    {
        if (controller.IsFloatCast)
        {
            controller.StartCoroutine(ReelInEmptyLine());
        }
    }
    
    private void HandleSessionAction(FishingSession session)
    {
        switch (session.State)
        {
            case FishingState.Waiting:
                HandlePrematureHook();
                break;
            case FishingState.Biting:
                HandleHook(session);
                break;
            case FishingState.Hooked:
                StartFighting(session);
                break;
            case FishingState.Fighting:
                // У стані Fighting не тягнемо автоматично
                controller.UIManager.UpdateStatusText("fighting");
                break;
        }
    }
    
    public void PullFish()
    {
        var session = controller.FishingService.GetCurrentSession();
        if (session?.State != FishingState.Fighting || !controller.IsReeling) return;
        
        var fish = session.CurrentFish;
        if (fish == null) return;
        
        CalculatePullProgress(fish);
        UpdateFishPosition();
        CheckCatchCompletion();

        if (Time.time % 1f < 0.1f) // Кожну секунду
        {
            float progress = (1f - controller.CurrentFishDistance / controller.castDistance) * 100f;
            Debug.Log($"🎣 Прогрес витягування: {progress:F0}% (дистанція: {controller.CurrentFishDistance:F1}м)");
        }
    }
    
    private void HandleHook(FishingSession session)
    {
        if (session.TryHook())
        {
            controller.SetHooked(true);
            controller.SetFishBiting(false);
            controller.VisualEffects.PlayBiteEffect();
            controller.UIManager.UpdateStatusText("hooked");
            controller.UIManager.UpdateButtonStates();
        }
    }
    
    private void StartFighting(FishingSession session)
    {
        session.StartFight();
        InitializeFightSequence();
    }
    
    private void InitializeFightSequence()
    {
        controller.SetCurrentFishDistance(controller.castDistance);
        controller.SetReeling(true);
        controller.SetFightTimer(0f);
        controller.SetTensionLevel(0f);
        
        controller.UIManager.ShowProgressBar();
        controller.UIManager.UpdateStatusText("fighting");
        
        var fightCoroutine = controller.StartCoroutine(FightSequenceCoroutine());
        controller.SetFightCoroutine(fightCoroutine);
    }
    
    public void ReleaseLine()
    {
        var session = controller.FishingService?.GetCurrentSession();
        
        if (session == null && controller.IsFloatCast)
        {
            controller.StartCoroutine(ReelInEmptyLine());
        }
        else if (session != null && (session.State == FishingState.Hooked || session.State == FishingState.Fighting))
        {
            ReleaseFish();
        }
    }
    
    public bool IsProcessingAction()
    {
        return controller.FightCoroutine != null;
    }
    
    // Event handlers
    public void OnFishingStateChanged(FishingState newState)
    {
        Debug.Log($"Стан риболовлі змінено на: {newState}");
    }
    
    public void OnFishBite(Fish fish)
    {
        Debug.Log($"Клює {fish?.FishType}!");
    }
    
    public void OnFishingComplete(FishingResult result, Fish fish)
    {
        Debug.Log($"Риболовля завершена: {result}");
        controller.StartCoroutine(HandleCompletion(FishingState.Caught));
    }
}
