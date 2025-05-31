using UnityEngine;
using System.Collections;

public class FishingGameLogic
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
    
    public void HookOrPull()
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
    
    public void OnFishBite()
    {
        Debug.Log("Риба клює!");
    }
    
    public void OnFishingComplete(FishingResult result, Fish fish)
    {
        Debug.Log($"Риболовля завершена: {result}");
        controller.StartCoroutine(HandleCompletion(FishingState.Caught));
    }
}
