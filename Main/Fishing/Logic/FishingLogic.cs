using UnityEngine;
using System.Collections;

public partial class FishingLogic
{
    private FishingController controller;
    
    public FishingLogic(FishingController controller)
    {
        this.controller = controller;
    }
    

    // public IEnumerator CastLineCoroutine()
    // {
    //     if (controller.IsFloatCast) yield break;
        
    //     // controller.UIManager.UpdateStatusText("cast");
    //     controller.SetFloatCast(true);
        
    //     // Запускаємо анімацію закидання
    //     yield return controller.StartCoroutine(controller.FloatAnimation.CastAnimation());
        
    //     // Ініціалізуємо сесію риболовлі
    //     StartFishingSession();
        
    //     // Запускаємо базове покачування поплавка
    //     // StartFloatBobbing();
        
    //     // controller.UIManager.UpdateStatusText("waiting");
    //     // controller.UIManager.UpdateButtonStates();
        
    //     Debug.Log("🎣 Вудка закинута! Очікування риби...");
    // }
        public IEnumerator CastToPositionCoroutine(Vector3 targetPosition)
    {
        if (controller.IsFloatCast) yield break;
        
        controller.SetFloatCast(true);
        
        // Показуємо поплавок в цільовій позиції (без анімації поки що)
        controller.FloatAnimation.ShowFloatAtPosition(targetPosition);
        
        // Ініціалізуємо сесію риболовлі
        StartFishingSession();
        
        Debug.Log($"🎣 Поплавок закинуто в позицію: {targetPosition}");
    }
    
    public void PullLine()
    {
        var session = controller.sessionManager?.CurrentSession;
        
        if (controller.IsHooked && session?.State == FishingState.Fighting)
        {
            PullFish();
        }
        else if (controller.IsFloatCast && session == null)
        {
            controller.StartCoroutine(ReelInEmptyLine());
        }
        else
        {
            Debug.LogWarning("❌ Неможливо тягнути лінію в поточному стані");
        }
    }

    public void Hook()
    {
        var session = controller.sessionManager?.CurrentSession;
        
        if (session == null)
        {
            HandlePrematureHook();
            return;
        }
        
        HandleHookAction(session);
    }
    
    public bool IsProcessingAction()
    {
        return controller.FightCoroutine != null || controller.IsReeling;
    }

    public void UpdateGameLogic()
    {
        var session = controller.sessionManager?.CurrentSession;
        if (session == null) return;
        
        UpdateSessionState(session);
        HandleActiveFighting(session);
        CheckForCompletion(session);
    }
    
    private void StartFishingSession()
    {
        if (controller.FishingService != null && controller.CurrentPlayer != null)
        {
            controller.FishingService.StartFishing(controller.CurrentPlayer);
        }
    }
    
    private void StartFloatBobbing()
    {
        if (controller.floatAnimation != null)
        {
            var bobCoroutine = controller.StartCoroutine(controller.floatAnimation.BaseBobbing());
            controller.SetFloatBobCoroutine(bobCoroutine);
        }
    }
    
    private void UpdateSessionState(FishingSession session)
    {
        if (controller.CurrentState != session.State)
        {
            controller.SetCurrentState(session.State);
            
            UpdateBitingState(session);
        }
    }
    
    private void UpdateBitingState(FishingSession session)
    {
        bool wasBiting = controller.IsFishBiting;
        bool isBiting = session.State == FishingState.Biting;
        controller.SetFishBiting(isBiting);
        
    }
    
    private void HandleActiveFighting(FishingSession session)
    {
        if (session.State == FishingState.Fighting && controller.IsReeling)
        {
            // Активна боротьба керується через UI або безпосередні виклики PullFish()
        }
    }
    
    private void CheckForCompletion(FishingSession session)
    {
        if (session.State == FishingState.Caught || session.State == FishingState.Escaped)
        {
            controller.StartCoroutine(HandleCompletion(session.State));
        }
    }
    
}