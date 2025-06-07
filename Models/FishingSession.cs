using System;
using UnityEngine;

public enum FishingState
{
    Waiting,
    Biting,
    Hooked,
    Fighting,
    Caught,
    Escaped,
    Ready
}

public enum FishingResult
{
    Success,
    FishEscaped,
    LineBroken,
    RodBroken,
    RodPulledAway,
    EmptyReel,
    MissedBite
}

public class FishingSession
{
    public Fish CurrentFish { get; private set; }
    public FishingState State { get; private set; }
    public float SessionTime { get; private set; }
    public bool IsActive { get; private set; }
    public FishingResult? LastResult { get; private set; } // Додано

    public event Action<Fish> OnFishBite;
    public event Action<FishingResult, Fish> OnFishingComplete;
    public event Action<FishingState> OnStateChanged;


    public void TriggerFishBite(Fish fish)
    {
        OnFishBite?.Invoke(fish); // ✅ Можна викликати зсередини класу
    }

    public void StartSession()
    {
        State = FishingState.Waiting;
        IsActive = true;
        SessionTime = 0f;
        CurrentFish = null;
        OnStateChanged?.Invoke(State);
        Debug.Log("Fishing session started - Waiting for fish");
    }

    public void EndSession()
    {
        IsActive = false;
        State = FishingState.Waiting;
        CurrentFish = null;
        OnStateChanged?.Invoke(State);
        Debug.Log("Fishing session ended");
    }

    public void SetFish(Fish fish)
    {
        // ЗМІНЕНО: Дозволяємо встановлювати рибу в різних станах
        if (State == FishingState.Waiting || State == FishingState.Biting || State == FishingState.Fighting)
        {
            CurrentFish = fish;
            Debug.Log($"✅ Fish встановлена в сесії: {fish?.FishType}, стан залишається: {State}");

            // ВИПРАВЛЕНО: Не змінюємо стан, якщо він вже Fighting
            if (State == FishingState.Waiting)
            {
                State = FishingState.Biting;
                OnStateChanged?.Invoke(State);
                OnFishBite?.Invoke(fish);
            }
        }
        else
        {
            Debug.LogWarning($"❌ Cannot set fish in state: {State}");
        }
    }


    public bool TryHook()
    {
        if (State == FishingState.Biting && CurrentFish != null)
        {
            State = FishingState.Hooked;
            OnStateChanged?.Invoke(State);
            Debug.Log("Fish hooked successfully!");
            return true;
        }
        else if (State == FishingState.Waiting)
        {
            Debug.Log("Premature hook - no fish biting");
            return false;
        }

        Debug.LogWarning($"Cannot hook in state: {State}");
        return false;
    }

    public void StartFight()
    {
        if (State == FishingState.Hooked)
        {
            State = FishingState.Fighting;
            OnStateChanged?.Invoke(State);
            Debug.Log("Fight started!");
        }
        else
        {
            Debug.LogWarning($"Cannot start fight in state: {State}");
        }
    }

    public void CompleteFishing(FishingResult result)
    {
        LastResult = result;
        FishingState newState = result == FishingResult.Success ? FishingState.Caught : FishingState.Escaped;
        State = newState;
        OnStateChanged?.Invoke(State);
        OnFishingComplete?.Invoke(result, CurrentFish);

        Debug.Log($"Fishing completed: {result}, Fish: {CurrentFish?.FishType}");
    }

    public void ResetToWaiting()
    {
        CurrentFish = null;
        State = FishingState.Waiting;
        OnStateChanged?.Invoke(State);
        Debug.Log("Session reset to waiting");
    }

    public void setState(FishingState newState)
    {
        State = newState;
        OnStateChanged?.Invoke(State);
    }

    public void Update(float deltaTime)
    {
        if (IsActive)
        {
            SessionTime += deltaTime;
        }
    }
}