using System;
using UnityEngine;

// FishingSession.cs
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
    
    public void StartSession()
    {
        State = FishingState.Waiting;
        IsActive = true;
        SessionTime = 0f;
    }
    
    public void EndSession()
    {
        IsActive = false;
        State = FishingState.Waiting;
        CurrentFish = null;
    }
    
    public void SetFish(Fish fish)
    {
        CurrentFish = fish;
        State = FishingState.Biting;
        OnFishBite?.Invoke(fish);
    }
    
    public void Hook()
    {
        if (State == FishingState.Biting)
        {
            State = FishingState.Hooked;
        }
    }
    
    public void StartFight()
    {
        if (State == FishingState.Hooked)
        {
            State = FishingState.Fighting;
        }
    }
    
    public void CompleteFishing(FishingResult result)
    {
        State = result == FishingResult.Success ? FishingState.Caught : FishingState.Escaped;
        OnFishingComplete?.Invoke(result, CurrentFish);
    }
}