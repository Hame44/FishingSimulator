using UnityEngine;

public class RewardCalculator : IRewardCalculator
{
    public FishingRewards CalculateRewards(Fish fish)
    {
        float strengthGain = fish.Weight * 0.1f;
        int experienceGain = Mathf.RoundToInt(fish.Weight * 15f);
        int moneyGain = Mathf.RoundToInt(fish.Weight * 5f);
        
        return new FishingRewards(strengthGain, experienceGain, moneyGain);
    }
}