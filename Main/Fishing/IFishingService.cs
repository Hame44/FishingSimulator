using System.Collections.Generic;
using UnityEngine;

public interface IFishingService
{
    FishingSession CurrentSession { get; }
    
    void StartFishing(Player player);
    void StopFishing();
    void HandlePlayerAction(FishingAction action);
    
    event Action<FishingState> OnStateChanged;
    event Action<FishingResult, Fish> OnFishingComplete;
}