public interface IActionValidator
{
    bool CanExecuteAction(FishingAction action, FishingState currentState);
}