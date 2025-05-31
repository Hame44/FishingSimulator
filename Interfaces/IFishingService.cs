using System.Collections.Generic;
using UnityEngine;

public interface IFishingService
{
    void StartFishing(Player player);
    void StopFishing();
    void HandlePlayerAction(FishingAction action);
    FishingSession GetCurrentSession();
}