using UnityEngine;
using System;

public class SessionManager : MonoBehaviour, ISessionManager
{
    public FishingSession CurrentSession { get; private set; }
    public Player CurrentPlayer { get; private set; }

    public event Action<FishingState> OnStateChanged;
    public event Action<FishingResult, Fish> OnFishingComplete;

    public event Action<FishingSession> OnSessionStarted;

    public void NotifyFishBite(Fish fish)
    {
        CurrentSession?.TriggerFishBite(fish);
    }

    public void StartSession(Player player)
    {
        CurrentPlayer = player;

        if (CurrentSession == null)
        {
            // CreateNewSession();
            StartNewSession();
        }

        CurrentSession.StartSession();
        Debug.Log($"🎣 Сесія розпочата для {player.Name}");
    }


    public void StartNewSession()
    {
        if (CurrentSession != null && CurrentSession.IsActive)
        {
            Debug.Log("⚠️ Завершуємо попередню активну сесію");
            CurrentSession.EndSession();
        }

        CurrentSession = new FishingSession();
        CurrentSession.OnStateChanged += (state) => OnStateChanged?.Invoke(state);
        CurrentSession.OnFishingComplete += (result, fish) => OnFishingComplete?.Invoke(result, fish);

        Debug.Log($"✅ Нова сесія створена. Стан: {CurrentSession.State}, Активна: {CurrentSession.IsActive}");

        OnSessionStarted?.Invoke(CurrentSession);
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