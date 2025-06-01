using System;

public interface ISessionManager
{
    FishingSession CurrentSession { get; }
    Player CurrentPlayer { get; }
    
    void StartSession(Player player);
    void EndSession();
    void ResetSession();
    
    event Action<FishingState> OnStateChanged;
    event Action<FishingResult, Fish> OnFishingComplete;
}