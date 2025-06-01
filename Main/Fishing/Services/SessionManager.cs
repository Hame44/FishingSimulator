using UnityEngine;
using System;

public class SessionManager : ISessionManager
{
    public FishingSession CurrentSession { get; private set; }
    public Player CurrentPlayer { get; private set; }
    
    public event Action<FishingState> OnStateChanged;
    public event Action<FishingResult, Fish> OnFishingComplete;
    
    public void StartSession(Player player)
    {
        CurrentPlayer = player;
        
        if (CurrentSession == null)
        {
            CreateNewSession();
        }
        
        CurrentSession.StartSession();
        Debug.Log($"🎣 Сесія розпочата для {player.Name}");
    }
    
    public void EndSession()
    {
        CurrentSession?.EndSession();
        Debug.Log("🛑 Сесія завершена");
    }
    
    public void ResetSession()
    {
        CurrentSession?.ResetToWaiting();
    }
    
    private void CreateNewSession()
    {
        CurrentSession = new FishingSession();
        CurrentSession.OnStateChanged += (state) => OnStateChanged?.Invoke(state);
        CurrentSession.OnFishingComplete += (result, fish) => OnFishingComplete?.Invoke(result, fish);
    }
}