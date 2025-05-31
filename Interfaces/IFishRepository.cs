using System.Collections.Generic;
using UnityEngine;

// IFishRepository.cs
public interface IFishRepository
{
    void SaveCaughtFish(CaughtFishData fish);
    System.Collections.Generic.List<CaughtFishData> GetPlayerFishHistory(int playerId);
    System.Collections.Generic.Dictionary<string, int> GetFishingStatistics(int playerId);
}