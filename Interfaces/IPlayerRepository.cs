using System.Collections.Generic;
using UnityEngine;

public interface IPlayerRepository
{
    PlayerData GetPlayer(int playerId);
    void SavePlayer(PlayerData player);
    void UpdatePlayerStats(int playerId, float strengthGain, int expGain);
}