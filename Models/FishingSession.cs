using System;
using UnityEngine;

public enum FishingState
{
    Waiting,
    Biting,
    Hooked,
    Fighting,
    Caught,
    Escaped
}

public enum FishingResult
{
    Success,
    FishEscaped,
    LineBroken,
    RodBroken,
    RodPulledAway,
    MissedBite
}

public class FishingSession
{
    public Fish CurrentFish { get; private set; }
    public FishingState State { get; private set; }
    public float SessionTime { get; private set; }
    public bool IsActive { get; private set; }
    
    public event Action<Fish> OnFishBite;
    public event Action<FishingResult, Fish> OnFishingComplete;
    public event Action<FishingState> OnStateChanged;
    
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
        if (State != FishingState.Waiting)
        {
            Debug.LogWarning($"Cannot set fish in state: {State}");
            return;
        }
        
        CurrentFish = fish;
        State = FishingState.Biting;
        OnStateChanged?.Invoke(State);
        OnFishBite?.Invoke(fish);
        Debug.Log($"Fish is biting: {fish?.FishType}");
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
    
    public void Update(float deltaTime)
    {
        if (IsActive)
        {
            SessionTime += deltaTime;
        }
    }
}