using System;

public class FishingEventBus
{
    private static FishingEventBus _instance;
    public static FishingEventBus Instance => _instance ??= new FishingEventBus();

    // Events
    public event Action<Fish> OnFishSpawned;
    public event Action<Fish> OnFishBiteStarted;
    public event Action<Fish> OnFishHooked;
    public event Action<FishingResult, Fish> OnFishingCompleted;

    // Publishers
    public void PublishFishSpawned(Fish fish) => OnFishSpawned?.Invoke(fish);
    public void PublishFishBiteStarted(Fish fish) => OnFishBiteStarted?.Invoke(fish);
    public void PublishFishHooked(Fish fish) => OnFishHooked?.Invoke(fish);
    public void PublishFishingCompleted(FishingResult result, Fish fish) => OnFishingCompleted?.Invoke(result, fish);
}