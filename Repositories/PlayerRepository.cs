using System.Collections.Generic;
using UnityEngine;

public class PlayerRepository : IPlayerRepository
{
    private readonly string connectionString;

    public PlayerRepository() : this("")
    {
    }

    public PlayerRepository(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public PlayerData GetPlayer(int playerId)
    {
        // В реальному проекті - SQL запит до бази даних
        // Тут повертаємо тестові дані
        return new PlayerData
        {
            Id = playerId,
            Name = "Player",
            Strength = 10f,
            Experience = 0,
            RodDurability = 100f,
            LineDurability = 100f,
            LineLength = 10f,
            FishingLuck = 1f,
            LastPlayed = System.DateTime.Now
        };
    }

    public void SavePlayer(PlayerData player)
    {
        // SQL INSERT/UPDATE запит
        Debug.Log($"Saving player: {player.Name}, Strength: {player.Strength}");
    }

    public void UpdatePlayerStats(int playerId, float strengthGain, int expGain)
    {
        // SQL UPDATE запит
        Debug.Log($"Player {playerId} gained {strengthGain} strength and {expGain} experience");
    }
}