using System.Collections.Generic;
using UnityEngine;

public class FishRepository : IFishRepository
{
    private readonly string connectionString;

    public FishRepository() : this("")
    {
    }

    public FishRepository(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public void SaveCaughtFish(CaughtFishData fish)
    {
        // SQL INSERT запит
        Debug.Log($"Saved caught fish: {fish.FishType} ({fish.Weight}kg)");
    }

    public System.Collections.Generic.List<CaughtFishData> GetPlayerFishHistory(int playerId)
    {
        // SQL SELECT запит
        return new System.Collections.Generic.List<CaughtFishData>();
    }

    public System.Collections.Generic.Dictionary<string, int> GetFishingStatistics(int playerId)
    {
        // SQL запит з GROUP BY
        return new System.Collections.Generic.Dictionary<string, int>();
    }
}