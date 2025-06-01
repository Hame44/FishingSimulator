using UnityEngine;
using System.Collections;

public partial class FishingService
{

    private void OnFishingComplete(FishingResult result, Fish fish)
    {
        Debug.Log($"🏁 Риболовля завершена: {result}");
        
        switch (result)
        {
            case FishingResult.Success:
                HandleSuccessfulCatch(fish);
                break;
            case FishingResult.FishEscaped:
            case FishingResult.LineBroken:
            case FishingResult.RodBroken:
            case FishingResult.RodPulledAway:
                HandleFailedCatch(result, fish);
                break;
            case FishingResult.MissedBite:
                HandleMissedBite(fish);
                break;
        }
        
        // Скидаємо сесію та плануємо наступну рибу
        StartCoroutine(ResetSessionAfterDelay(3f));
    }
    
    private void HandleSuccessfulCatch(Fish fish)
    {
        var caughtFish = new CaughtFishData
        {
            PlayerId = currentPlayer.Id,
            FishType = fish.FishType,
            Weight = fish.Weight,
            CaughtAt = System.DateTime.Now
        };
        
        fishRepository.SaveCaughtFish(caughtFish);
        
        // Нагороди за улов
        float strengthGain = fish.Weight * 0.1f;
        int expGain = Mathf.RoundToInt(fish.Weight * 15);
        
        currentPlayer.GainStrength(strengthGain);
        currentPlayer.GainExperience(expGain);
        
        playerRepository.UpdatePlayerStats(currentPlayer.Id, strengthGain, expGain);
        
        Debug.Log($"🏆 УСПІХ! Піймано {fish.FishType} вагою {fish.Weight:F2}кг!");
        Debug.Log($"📈 Нагороди: +{strengthGain:F1} сили, +{expGain} досвіду");
        
        // Після успішного улову - середній інтервал до наступної риби
        ScheduleNextFishAfterSuccess();
    }
    
    private void HandleFailedCatch(FishingResult result, Fish fish)
    {
        string message = result switch
        {
            FishingResult.FishEscaped => "🏃 Риба втекла",
            FishingResult.LineBroken => "💔 Порвалась леска",
            FishingResult.RodBroken => "💥 Зламалась вудка", 
            FishingResult.RodPulledAway => "💪 Риба вирвала вудку",
            _ => "❌ Невдача"
        };
        
        if (fish != null)
        {
            Debug.Log($"{message} - {fish.FishType} ({fish.Weight:F2}кг, сила: {fish.Strength:F1})");
        }
        
        // Після невдачі - довгий інтервал
        ScheduleNextFishAfterLongDelay();
    }
    
    private void HandleMissedBite(Fish fish)
    {
        Debug.Log($"⏰ Пропущена поклювка - {fish?.FishType}");
        
        // Пропущена поклювка = дуже довгий інтервал (гравець був неуважний)
        ScheduleNextFishAfterLongDelay();
    }
    
    private void ScheduleNextFishAfterSuccess()
    {
        float successDelay = UnityEngine.Random.Range(8f, 15f);
        Debug.Log($"🎉 Наступна риба після успіху через {successDelay:F1}сек");
        fishSpawnCoroutine = StartCoroutine(DelayNextFish(successDelay));
    }
    
    private IEnumerator ResetSessionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (currentSession != null)
        {
            currentSession.ResetToWaiting();
        }
    }
    
}