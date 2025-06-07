using UnityEngine;

public class FishingResultHandler : IFishingResultHandler
{
    private readonly IPlayerRepository playerRepository;
    private readonly IFishRepository fishRepository;
    private readonly IRewardCalculator rewardCalculator;
    
    public FishingResultHandler(IPlayerRepository playerRepository, IFishRepository fishRepository)
    {
        this.playerRepository = playerRepository;
        this.fishRepository = fishRepository;
        this.rewardCalculator = new RewardCalculator();
    }
    
    public void HandleResult(FishingResult result, Fish fish, Player player)
    {
        switch (result)
        {
            case FishingResult.Success:
                HandleSuccessfulCatch(fish, player);
                break;
            case FishingResult.FishEscaped:
            case FishingResult.LineBroken:
            case FishingResult.RodBroken:
                HandleFailedCatch(result, fish);
                break;
            default:
                HandleOtherResult(result);
                break;
        }
    }
    
    private void HandleSuccessfulCatch(Fish fish, Player player)
    {
        SaveCaughtFish(fish, player);
        
        var rewards = rewardCalculator.CalculateRewards(fish);
        ApplyRewards(player, rewards);
        
        Debug.Log($"🏆 Успішно піймано {fish.FishType}! +{rewards.Experience} досвіду");
    }
    
    private void SaveCaughtFish(Fish fish, Player player)
    {
        var caughtFish = new CaughtFishData
        {
            PlayerId = player.Id,
            FishType = fish.FishType,
            Weight = fish.Weight,
            CaughtAt = System.DateTime.Now
        };
        
        fishRepository.SaveCaughtFish(caughtFish);
    }
    
    private void ApplyRewards(Player player, FishingRewards rewards)
    {
        player.GainStrength(rewards.Strength);
        player.GainExperience(rewards.Experience);
        
        playerRepository.UpdatePlayerStats(player.Id, rewards.Strength, rewards.Experience);
    }
    
    private void HandleFailedCatch(FishingResult result, Fish fish)
    {
        string message = GetFailureMessage(result);
        Debug.Log($"❌ {message}");
    }
    
    private void HandleOtherResult(FishingResult result)
    {
        Debug.Log($"ℹ️ Результат: {result}");
    }
    
    private string GetFailureMessage(FishingResult result)
    {
        return result switch
        {
            FishingResult.FishEscaped => "Риба втекла",
            FishingResult.LineBroken => "Порвалась леска",
            FishingResult.RodBroken => "Зламалась вудка",
            _ => "Невдача"
        };
    }
}