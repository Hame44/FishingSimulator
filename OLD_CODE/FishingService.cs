using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FishingService : MonoBehaviour, IFishingService
{
    private IPlayerRepository playerRepository;
    private IFishRepository fishRepository;
    private FishingSession currentSession;
    private Player currentPlayer;
    
    // Поточна корутіна для управління спавном риби
    private Coroutine fishSpawnCoroutine;
    private Coroutine biteCoroutine;

    private bool hasLoggedFightResult = false;
    private float lastFightLogTime = 0f;
    
    void Awake()
    {
        playerRepository = new PlayerRepository("");
        fishRepository = new FishRepository("");
        
    }
    
    public void StartFishing(Player player)
    {
        currentPlayer = player;
        
        // Створюємо нову сесію якщо її немає
        if (currentSession == null)
        {
            currentSession = new FishingSession();
            currentSession.OnFishingComplete += OnFishingComplete;
            currentSession.OnStateChanged += OnStateChanged;
        }
        
        currentSession.StartSession();
        Debug.Log("Риболовля почалася!");
        
        // Запускаємо логіку появи риби
        StartFishSpawnLogic();
    }
    
    public void StopFishing()
    {
        StopAllCoroutines();
        fishSpawnCoroutine = null;
        biteCoroutine = null;
        
        currentSession?.EndSession();
        Debug.Log("Риболовля зупинена");
    }
    
    private void DetermineEscapeReason()
    {
        var fish = currentSession.CurrentFish;
        var equipment = currentPlayer.Equipment;
        
        string escapeReason = "";
        FishingResult result = FishingResult.FishEscaped;
        
        if (currentPlayer.Strength < fish.Strength * 0.5f)
        {
            escapeReason = "💪 Риба вирвала вудку з рук! (Недостатньо сили)";
            result = FishingResult.RodPulledAway;
        }
        else if (equipment.LineDurability < fish.Strength * 0.3f)
        {
            escapeReason = "💔 Порвалась леска! (Низька міцність лески)";
            equipment.DamageLine(equipment.LineDurability);
            result = FishingResult.LineBroken;
        }
        else if (equipment.RodDurability < fish.Strength * 0.2f)
        {
            escapeReason = "💥 Зламалась вудка! (Низька міцність вудки)";
            equipment.DamageRod(equipment.RodDurability);
            result = FishingResult.RodBroken;
        }
        else
        {
            escapeReason = "🏃 Риба втекла через власну спритність...";
            result = FishingResult.FishEscaped;
        }
        
        Debug.Log($"❌ {escapeReason}");
        Debug.Log($"📊 Риба: {fish.FishType} ({fish.Weight:F1}кг, сила: {fish.Strength:F1}) vs Гравець (сила: {currentPlayer.Strength:F1})");
        
        currentSession.CompleteFishing(result);
    }
    
    
    
    private IEnumerator ExecuteBiteCoroutine(IBiteBehavior biteBehavior)
    {
        Debug.Log($"⏱️ {currentSession.CurrentFish.FishType} клює {biteBehavior.BiteDuration:F1} секунд!");
        
        biteBehavior.ExecuteBite(
            () => { /* Початок клювання */ },
            () => { /* Кінець клювання */ }
        );
        
        yield return new WaitForSeconds(biteBehavior.BiteDuration);
        
        if (currentSession.State == FishingState.Biting)
        {
            Debug.Log("⏰ Час клювання вийшов - гравець не встиг підсікти!");
            CheckForRebite(biteBehavior);
        }
        
        biteCoroutine = null;
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
    
    private void OnFishingComplete(FishingResult result, Fish fish)
    {
        Debug.Log($"Риболовля завершена: {result}");
        
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
        }
        
        // Очищуємо сесію для можливості нового закидання
        StartCoroutine(ResetSessionAfterDelay(3f));
    }
    
    private IEnumerator ResetSessionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (currentSession != null)
        {
            currentSession.ResetToWaiting();
        }
        
        // Debug.Log("Готовий до нового закидання!");
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
        
        // Оновлюємо статистику гравця
        float strengthGain = fish.Weight * 0.1f;
        int expGain = Mathf.RoundToInt(fish.Weight * 15);
        
        currentPlayer.GainStrength(strengthGain);
        currentPlayer.GainExperience(expGain);
        
        playerRepository.UpdatePlayerStats(currentPlayer.Id, strengthGain, expGain);
        
        Debug.Log($"🏆 УСПІХ! Піймано {fish.FishType} вагою {fish.Weight:F2}кг!");
        Debug.Log($"📈 Нагороди: +{strengthGain:F1} сили, +{expGain} досвіду");
        
        // Скидаємо флаг для наступного бою
        hasLoggedFightResult = false;
    }
    
    private void HandleFailedCatch(FishingResult result, Fish fish)
    {
        string message = result switch
        {
                        FishingResult.FishEscaped => "🏃 Риба втекла",
            FishingResult.LineBroken => "💔 Порвалась леска",
            FishingResult.RodBroken => "💥 Зламалась вудка", 
            FishingResult.RodPulledAway => "💪 Риба вирвала вудку",
            FishingResult.MissedBite => "⏰ Пропущена поклювка",
            FishingResult.EmptyReel => "🎣 Витягнули порожню лінію",
            _ => "❌ Невдача"
        };
        
        if (fish != null)
        {
            Debug.Log($"{message} - {fish.FishType} ({fish.Weight:F2}кг, сила: {fish.Strength:F1})");
        }
        else
        {
            Debug.Log(message);
        }
        
        // Скидаємо флаг для наступного бою
        hasLoggedFightResult = false;
    }
    
    public FishingSession GetCurrentSession()
    {
        return currentSession;
    }
    
    void Update()
    {
        currentSession?.Update(Time.deltaTime);
    }
}