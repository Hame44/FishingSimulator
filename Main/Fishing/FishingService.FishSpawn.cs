using UnityEngine;
using System.Collections;

public partial class FishingService
{
    private void StartFishSpawnLogic()
    {
        if (fishSpawnCoroutine != null)
        {
            StopCoroutine(fishSpawnCoroutine);
        }
        
        fishSpawnCoroutine = StartCoroutine(SpawnFishWithDelay());
    }
    
    private IEnumerator SpawnFishWithDelay()
    {
        // Довгий інтервал між появою риби (5-25 секунд)
        float spawnDelay = UnityEngine.Random.Range(5f, 25f);
        Debug.Log($"🕐 Чекаємо появу риби {spawnDelay:F1} секунд...");
        
        yield return new WaitForSeconds(spawnDelay);
        
        if (currentSession?.State == FishingState.Waiting)
        {
            SpawnNewFish();
        }
        
        fishSpawnCoroutine = null;
    }
    
    private void SpawnNewFish()
    {
        // Вибираємо тип риби
        string fishType = SelectRandomFishType();
        
        // Створюємо рибу через Factory
        var fishFactory = GetFishFactory(fishType);
        if (fishFactory == null)
        {
            Debug.LogError($"❌ Не знайдено Factory для {fishType}");
            return;
        }
        
        Fish newFish = fishFactory.CreateFish();
        currentSession.SetFish(newFish);
        
        Debug.Log($"🐟 Нова риба з'явилась: {newFish.FishType} (вага: {newFish.Weight:F1}кг, сила: {newFish.Strength:F1})");
        
        // НЕ запускаємо клювання тут - це робить BiteController!
        // BiteController сам підпишеться на OnFishBite event
    }
    
    private string SelectRandomFishType()
    {
        float chance = UnityEngine.Random.value;
        return chance < 0.7f ? "Carp" : "Perch";
    }
    
    private FishFactory GetFishFactory(string fishType)
    {
        // Це має бути ін'єкція залежностей, але для простоти:
        return fishType switch
        {
            "Carp" => new CarpFactory(),
            "Perch" => new PerchFactory(),
            _ => null
        };
    }
    
    public void ScheduleNextFishAfterLongDelay()
    {
        if (fishSpawnCoroutine != null)
        {
            StopCoroutine(fishSpawnCoroutine);
        }
        
        float longDelay = UnityEngine.Random.Range(20f, 45f);
        Debug.log($"⏰ Наступна риба через {longDelay:F1} секунд (довгий інтервал)");
        
        fishSpawnCoroutine = StartCoroutine(DelayNextFish(longDelay));
    }
    
    private IEnumerator DelayNextFish(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (currentSession?.State == FishingState.Waiting)
        {
            StartFishSpawnLogic();
        }
        
        fishSpawnCoroutine = null;
    }
    
}