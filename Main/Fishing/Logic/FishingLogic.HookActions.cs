using UnityEngine;
using System.Collections;

public partial class FishingLogic
{
    
    private void HandleHookAction(FishingSession session)
    {
        switch (session.State)
        {
            case FishingState.Waiting:
                HandlePrematureHook();
                break;
                
            case FishingState.Biting:
                if (controller.IsFishBiting)
                {
                    HandleSuccessfulHook(session);
                }
                break;
                
            case FishingState.Hooked:
                StartFighting(session);
                break;
                
            case FishingState.Fighting:
                // Під час боротьби Hook означає одноразове тягнення
                PullFish();
                break;
                
            default:
                Debug.LogWarning($"❌ Hook неможливий в стані {session.State}");
                break;
        }
    }
    
    private void HandlePrematureHook()
    {
        controller.StartCoroutine(ShowTemporaryMessage("Передчасно! Чекайте поклювки...", 2f));
        Debug.Log("⏰ Передчасне підсікання!");
    }
    
    private void HandleSuccessfulHook(FishingSession session)
    {
        if (session.TryHook())
        {
            controller.SetHooked(true);
            controller.SetFishBiting(false);
            controller.VisualEffects.PlayHookEffect();
            controller.UIManager.UpdateStatusText("hooked");
            controller.UIManager.UpdateButtonStates();
            
            Debug.Log("🪝 Риба підсічена!");
        }
        else
        {
            Debug.Log("❌ Не вдалося підсікти рибу");
        }
    }
    
    private void StartFighting(FishingSession session)
    {
        session.StartFight();
        InitializeFightSequence();
        
        Debug.Log("⚔️ Почалася боротьба з рибою!");
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
    
    private IEnumerator ShowTemporaryMessage(string message, float duration)
    {
        controller.UIManager.UpdateStatusText(message);
        yield return new WaitForSeconds(duration);
        controller.UIManager.UpdateStatusText("waiting");
    }
    
}