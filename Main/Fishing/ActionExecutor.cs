using UnityEngine;

public class ActionExecutor : IActionExecutor
{
    public void ExecuteAction(FishingAction action, FishingSession session)
    {
        switch (action)
        {
            case FishingAction.Hook:
                session.TryHook();
                break;
            case FishingAction.Pull:
                // Pull logic handled in FishingLogic
                Debug.Log("Pull action executed");
                break;
            default:
                Debug.LogWarning($"Action {action} not implemented in executor");
                break;
        }
    }
}