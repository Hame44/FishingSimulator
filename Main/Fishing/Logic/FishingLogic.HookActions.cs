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
                PullFish();
                break;
                
            default:
                Debug.LogWarning($"❌ Hook неможливий в стані {session.State}");
                break;
        }
    }
    
    private void HandlePrematureHook()
    {
        Debug.Log("⏰ Передчасне підсікання!");
    }
    
    private void HandleSuccessfulHook(FishingSession session)
    {
        if (session.TryHook())
        {
            controller.SetHooked(true);
            controller.SetFishBiting(false);
            
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
        controller.SetReeling(true);
        
        var fightCoroutine = controller.StartCoroutine(FightSequenceCoroutine());
        controller.SetFightCoroutine(fightCoroutine);
    }
    
}