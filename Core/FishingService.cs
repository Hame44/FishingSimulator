using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FishingService : MonoBehaviour, IFishingService
{
    private IPlayerRepository playerRepository;
    private IFishRepository fishRepository;
    private Dictionary<string, FishFactory> fishFactories;
    private FishingSession currentSession;
    private Player currentPlayer;
    
    // Поточна корутіна для управління спавном риби
    private Coroutine fishSpawnCoroutine;
    private Coroutine biteCoroutine;
    
    void Awake()
    {
        playerRepository = new PlayerRepository("");
        fishRepository = new FishRepository("");
        fishFactories = new Dictionary<string, FishFactory>();
        
        InitializeFishFactories();
    }
    
    private void InitializeFishFactories()
    {
        fishFactories["Carp"] = new CarpFactory();
        fishFactories["Perch"] = new PerchFactory();
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
    
    public void HandlePlayerAction(FishingAction action)
    {
        if (currentSession == null || currentPlayer == null) 
        {
            Debug.LogWarning("No active fishing session");
            return;
        }
        
        Debug.Log($"Дія гравця: {action}, Поточний стан: {currentSession.State}");
        
        switch (action)
        {
            case FishingAction.Cast:
                HandleCastAction();
                break;
            case FishingAction.Hook:
                HandleHookAction();
                break;
            case FishingAction.Pull:
                HandlePullAction();
                break;
            case FishingAction.Release:
                HandleReleaseAction();
                break;
        }
    }
    
    private void HandleCastAction()
    {
        if (currentSession.State == FishingState.Waiting)
        {
            Debug.Log("Закидання вудки...");
            StartFishSpawnLogic();
        }
    }
    
    private void HandleHookAction()
    {
        if (currentSession.State == FishingState.Biting)
        {
            Debug.Log("Успішне підсікання!");
            bool hooked = currentSession.TryHook();
            if (hooked)
            {
                // Зупиняємо корутіну клювання
                if (biteCoroutine != null)
                {
                    StopCoroutine(biteCoroutine);
                    biteCoroutine = null;
                }
                currentSession.StartFight();
            }
        }
        else if (currentSession.State == FishingState.Waiting)
        {
            Debug.Log("Передчасне підсікання - штраф часу");
            // Затримка до наступної поклювки
            if (fishSpawnCoroutine != null)
            {
                StopCoroutine(fishSpawnCoroutine);
            }
            fishSpawnCoroutine = StartCoroutine(DelayNextFish(UnityEngine.Random.Range(10f, 20f)));
        }
        else
        {
            Debug.Log($"Підсікання неможливе в стані: {currentSession.State}");
        }
    }
    
    private void HandlePullAction()
    {
        if (currentSession.State == FishingState.Fighting)
        {
            Debug.Log("Процес боротьби з рибою...");
            ProcessFight();
        }
        else
        {
            Debug.Log($"Тягання неможливе в стані: {currentSession.State}");
        }
    }
    
    private void HandleReleaseAction()
    {
        if (currentSession.State == FishingState.Hooked || currentSession.State == FishingState.Fighting)
        {
            Debug.Log("Відпускаємо рибу");
            currentSession.CompleteFishing(FishingResult.FishEscaped);
        }
    }
    
    private void ProcessFight()
    {
        if (currentSession.CurrentFish == null) return;
        
        var fightBehavior = currentSession.CurrentFish.GetFightBehavior();
        bool fishEscaped = fightBehavior.TryEscape(
            currentPlayer.Strength,
            currentPlayer.Equipment.RodDurability,
            currentPlayer.Equipment.LineDurability
        );
        
        if (fishEscaped)
        {
            Debug.Log("Риба намагається втекти!");
            DetermineEscapeReason();
        }
        else
        {
            Debug.Log("Риба піймана!");
            currentSession.CompleteFishing(FishingResult.Success);
        }
    }
    
    private void DetermineEscapeReason()
    {
        var fish = currentSession.CurrentFish;
        var equipment = currentPlayer.Equipment;
        
        if (currentPlayer.Strength < fish.Strength * 0.5f)
        {
            Debug.Log("Риба вирвала вудку з рук!");
            currentSession.CompleteFishing(FishingResult.RodPulledAway);
        }
        else if (equipment.LineDurability < fish.Strength * 0.3f)
        {
            Debug.Log("Порвалась леска!");
            equipment.DamageLine(equipment.LineDurability);
            currentSession.CompleteFishing(FishingResult.LineBroken);
        }
        else if (equipment.RodDurability < fish.Strength * 0.2f)
        {
            Debug.Log("Зламалась вудка!");
            equipment.DamageRod(equipment.RodDurability);
            currentSession.CompleteFishing(FishingResult.RodBroken);
        }
        else
        {
            Debug.Log("Риба втекла...");
            currentSession.CompleteFishing(FishingResult.FishEscaped);
        }
    }
    
    private void StartFishSpawnLogic()
    {
        if (currentSession?.State == FishingState.Waiting)
        {
            if (fishSpawnCoroutine != null)
            {
                StopCoroutine(fishSpawnCoroutine);
            }
            fishSpawnCoroutine = StartCoroutine(SpawnFishCoroutine());
        }
    }
    
    private IEnumerator SpawnFishCoroutine()
    {
        // Чекаємо випадковий час до появи риби (реалістичні інтервали)
        float waitTime = UnityEngine.Random.Range(5f, 25f);
        Debug.Log($"Чекаємо рибу {waitTime:F1} секунд...");
        
        yield return new WaitForSeconds(waitTime);
        
        // Перевіряємо чи ще актуально
        if (currentSession?.State == FishingState.Waiting)
        {
            SpawnFish();
        }
        
        fishSpawnCoroutine = null;
    }
    
    private void SpawnFish()
    {
        if (currentSession?.State != FishingState.Waiting) return;
        
        // Випадковий вибір типу риби з різною ймовірністю
        string selectedType;
        float chance = UnityEngine.Random.value;
        
        if (chance < 0.7f) // 70% шанс на коропа
            selectedType = "Carp";
        else // 30% шанс на окуня
            selectedType = "Perch";
        
        if (fishFactories.TryGetValue(selectedType, out FishFactory factory))
        {
            UpdateFactoryLuckModifier(factory, currentPlayer.Equipment.FishingLuck);
            
            Fish newFish = factory.CreateFish();
            currentSession.SetFish(newFish);
            
            Debug.Log($"З'явилась риба: {newFish.FishType} ({newFish.Weight:F2}кг, сила: {newFish.Strength:F1})");
            
            // Запускаємо поведінку клювання
            var biteBehavior = newFish.GetBiteBehavior();
            biteCoroutine = StartCoroutine(ExecuteBiteCoroutine(biteBehavior));
        }
    }
    
    private IEnumerator ExecuteBiteCoroutine(IBiteBehavior biteBehavior)
    {
        Debug.Log($"{currentSession.CurrentFish.FishType} клює! Тривалість: {biteBehavior.BiteDuration:F1}с");
        
        // Виконуємо логіку початку клювання
        biteBehavior.ExecuteBite(
            () => Debug.Log("Клювання почалося!"),
            () => Debug.Log("Клювання закінчилося!")
        );
        
        // Чекаємо час клювання
        yield return new WaitForSeconds(biteBehavior.BiteDuration);
        
        // Перевіряємо чи гравець встиг засікти
        if (currentSession.State == FishingState.Biting)
        {
            Debug.Log("Гравець не встиг підсікти!");
            // Гравець не встиг засікти - перевіряємо повторне клювання
            CheckForRebite(biteBehavior);
        }
        
        biteCoroutine = null;
    }
    
    private void UpdateFactoryLuckModifier(FishFactory factory, float luck)
    {
        factory.SetLuckModifier(luck);
    }
    
    private void CheckForRebite(IBiteBehavior biteBehavior)
    {
        float rebiteChance = biteBehavior.RebiteChance * currentPlayer.Equipment.FishingLuck;
        
        if (UnityEngine.Random.value < rebiteChance)
        {
            Debug.Log($"Риба клюне знову через {biteBehavior.RebiteDelay:F1}с");
            // Повторне клювання через короткий час
            StartCoroutine(RebiteCoroutine(biteBehavior.RebiteDelay));
        }
        else
        {
            Debug.Log("Риба втекла. Чекаємо наступну...");
            // Риба втекла, чекаємо наступну
            currentSession.ResetToWaiting();
            fishSpawnCoroutine = StartCoroutine(DelayNextFish(UnityEngine.Random.Range(15f, 45f)));
        }
    }
    
    private IEnumerator RebiteCoroutine(float delay)
    {
        // Повертаємо стан до очікування але зберігаємо рибу
        Fish savedFish = currentSession.CurrentFish;
        currentSession.ResetToWaiting();
        
        yield return new WaitForSeconds(delay);
        
        // Повторне клювання тієї ж риби
        if (currentSession?.State == FishingState.Waiting && savedFish != null)
        {
            currentSession.SetFish(savedFish);
            var biteBehavior = savedFish.GetBiteBehavior();
            biteCoroutine = StartCoroutine(ExecuteBiteCoroutine(biteBehavior));
        }
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
    
    private void OnStateChanged(FishingState newState)
    {
        Debug.Log($"Fishing state changed to: {newState}");
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
        
        Debug.Log("Готовий до нового закидання!");
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
        
        Debug.Log($"Піймано {fish.FishType} вагою {fish.Weight:F2}кг! +" +
                 $"{strengthGain:F1} сили, +{expGain} досвіду");
    }
    
    private void HandleFailedCatch(FishingResult result, Fish fish)
    {
        string message = result switch
        {
            FishingResult.FishEscaped => "Риба втекла",
            FishingResult.LineBroken => "Порвалась леска",
            FishingResult.RodBroken => "Зламалась вудка",
            FishingResult.RodPulledAway => "Риба вирвала вудку",
            _ => "Невдача"
        };
        
        Debug.Log($"{message}. Риба: {fish?.FishType ?? "Невідома"} " +
                 $"({fish?.Weight:F2 ?? 0}кг)");
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