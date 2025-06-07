using UnityEngine;
using System.Collections;

public class FishSpawner : MonoBehaviour, IFishSpawner
{
    // private readonly IFishFactoryProvider factoryProvider;
    private IFishFactoryProvider factoryProvider;
    private Coroutine spawnCoroutine;
    
    // public FishSpawner(IFishFactoryProvider factoryProvider)
    // {
    //     this.factoryProvider = factoryProvider;
    // }

    public void Initialize(IFishFactoryProvider factoryProvider)
    {
        this.factoryProvider = factoryProvider;
        Debug.Log("🔧 FishSpawner ініціалізовано");
    }
    
    public void StartSpawning()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
            
        spawnCoroutine = StartCoroutine(SpawnFishWithRandomDelay());
    }
    
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
    
    public void ScheduleNextFish(float delay)
    {
    StopSpawning();
    spawnCoroutine = StartCoroutine(DelayedSpawn(delay));
    }
    
    private IEnumerator SpawnFishWithRandomDelay()
    {
        // float spawnDelay = UnityEngine.Random.Range(5f, 25f);
        float spawnDelay = UnityEngine.Random.Range(5f, 10f);
        Debug.Log($"🕐 Очікування риби: {spawnDelay:F1}с");
        
        yield return new WaitForSeconds(spawnDelay);
        
        SpawnRandomFish();
        spawnCoroutine = null;
    }
    
    private IEnumerator DelayedSpawn(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartSpawning();
    }
    
    private void SpawnRandomFish()
    {
        var fishType = SelectRandomFishType();
        var factory = factoryProvider.GetFactory(fishType);
        
        if (factory != null)
        {
            var fish = factory.CreateFish();
            NotifyFishSpawned(fish);
        }
    }
    
    private string SelectRandomFishType()
    {
        return UnityEngine.Random.value < 0.4f ? "Carp" : "Perch";
    }
    
    private void NotifyFishSpawned(Fish fish)
    {
        // Через Event Bus або інший механізм повідомлення
        FishingEventBus.Instance.PublishFishSpawned(fish);
    }
}