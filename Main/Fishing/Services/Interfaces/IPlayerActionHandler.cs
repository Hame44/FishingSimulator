public interface IPlayerActionHandler
{
    void HandleAction(FishingAction action, FishingSession session);
}