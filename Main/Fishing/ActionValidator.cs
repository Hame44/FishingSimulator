using UnityEngine;

public class ActionValidator : IActionValidator
{
    public bool CanExecuteAction(FishingAction action, FishingState currentState)
    {
        return action switch
        {
            FishingAction.Cast => currentState == FishingState.Waiting,
            FishingAction.Hook => currentState == FishingState.Biting || currentState == FishingState.Hooked,
            FishingAction.Pull => currentState == FishingState.Fighting,
            _ => false
        };
    }
}
