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
        }
    }
    
    private void HandleFighting(FishingSession session)
    {
        if (session.State == FishingState.Fighting && controller.IsReeling)
        {
            // Автоматичне тягнення не відбувається - тільки по кліку
        }
    }
    
    private void CheckForCompletion(FishingSession session)
    {
        if (session.State == FishingState.Caught || session.State == FishingState.Escaped)
        {
            controller.StartCoroutine(HandleCompletion(session.State));
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
                PullFish(session);
                break;
        }
    }
    
    private void HandlePrematureHook()
    {
        controller.StartCoroutine(ShowTemporaryMessage("Передчасно! Чекайте поклювки...", 2f));
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
    
    private void PullFish(FishingSession session)
    {
        if (controller.CurrentFishDistance <= 0 || !controller.IsReeling) return;
        
        var fish = session.CurrentFish;
        if (fish == null) return;
        
        CalculatePullProgress(fish);
        UpdateFishPosition();
        CheckCatchCompletion();
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
}