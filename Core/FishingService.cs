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
    
    // Додаємо MonoBehaviour функціональність для корутин
    void Awake()
    {
        // Переносимо ініціалізацію в Awake
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
        currentSession = new FishingSession();
        currentSession.OnFishingComplete += OnFishingComplete;
        currentSession.StartSession();
        
        // Запускаємо логіку появи риби
        StartFishSpawnLogic();
    }
    
    public void StopFishing()
    {
        StopAllCoroutines(); // Зупиняємо всі корутини
        currentSession?.EndSession();
        currentSession = null;
    }
    
    public void HandlePlayerAction(FishingAction action)
    {
        if (currentSession == null) return;
        
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
            StartFishSpawnLogic();
        }
    }
    
    private void HandleHookAction()
    {
        if (currentSession.State == FishingState.Biting)
        {
            currentSession.Hook();
            currentSession.StartFight();
            ProcessFight();
        }
        else if (currentSession.State == FishingState.Waiting)
        {
            // Засікання не вчасно - затримка до наступної поклювки
            StartCoroutine(DelayNextFish(UnityEngine.Random.Range(15f, 30f)));
        }
    }
    
    private void HandlePullAction()
    {
        if (currentSession.State == FishingState.Fighting)
        {
            ProcessFight();
        }
    }
    
    private void HandleReleaseAction()
    {
        if (currentSession.State == FishingState.Hooked && currentSession.CurrentFish != null)
        {
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
            DetermineEscapeReason();
        }
        else
        {
            currentSession.CompleteFishing(FishingResult.Success);
        }
    }
    
    private void DetermineEscapeReason()
    {
        var fish = currentSession.CurrentFish;
        var equipment = currentPlayer.Equipment;
        
        if (currentPlayer.Strength < fish.Strength * 0.5f)
        {
            currentSession.CompleteFishing(FishingResult.RodPulledAway);
        }
        else if (equipment.LineDurability < fish.Strength * 0.3f)
        {
            equipment.DamageLine(equipment.LineDurability);
            currentSession.CompleteFishing(FishingResult.LineBroken);
        }
        else if (equipment.RodDurability < fish.Strength * 0.2f)
        {
            equipment.DamageRod(equipment.RodDurability);
            currentSession.CompleteFishing(FishingResult.RodBroken);
        }
        else
        {
            currentSession.CompleteFishing(FishingResult.FishEscaped);
        }
    }
    
    private void StartFishSpawnLogic()
    {
        StartCoroutine(SpawnFishCoroutine());
    }
    
    private IEnumerator SpawnFishCoroutine()
    {
        // Чекаємо випадковий час до появи риби
        float waitTime = UnityEngine.Random.Range(5f, 15f);
        yield return new WaitForSeconds(waitTime);
        
        SpawnFish();
    }
    
    private void SpawnFish()
    {
        if (currentSession?.State != FishingState.Waiting) return;
        
        // Випадковий вибір типу риби
        string[] fishTypes = { "Carp", "Perch" };
        string selectedType = fishTypes[UnityEngine.Random.Range(0, fishTypes.Length)];
        
        if (fishFactories.TryGetValue(selectedType, out FishFactory factory))
        {
            UpdateFactoryLuckModifier(factory, currentPlayer.Equipment.FishingLuck);
            
            Fish newFish = factory.CreateFish();
            currentSession.SetFish(newFish);
            
            // Запускаємо поведінку клювання
            var biteBehavior = newFish.GetBiteBehavior();
            StartCoroutine(ExecuteBiteCoroutine(biteBehavior));
        }
    }
    
    private IEnumerator ExecuteBiteCoroutine(IBiteBehavior biteBehavior)
    {
        Debug.Log($"{currentSession.CurrentFish.FishType} is biting!");
        
        // Чекаємо час клювання
        yield return new WaitForSeconds(biteBehavior.BiteDuration);
        
        // Перевіряємо чи гравець встиг засікти
        if (currentSession.State == FishingState.Biting)
        {
            // Гравець не встиг засікти - риба втекла
            CheckForRebite(biteBehavior);
        }
    }
    
    private void UpdateFactoryLuckModifier(FishFactory factory, float luck)
    {
        factory.SetLuckModifier(luck);
    }
    
    private void CheckForRebite(IBiteBehavior biteBehavior)
    {
        if (UnityEngine.Random.value < biteBehavior.RebiteChance)
        {
            // Повторне клювання через короткий час
            ScheduleRebite(biteBehavior.RebiteDelay);
        }
        else
        {
            // Довгий інтервал до наступної риби
            ScheduleNextFish(UnityEngine.Random.Range(10f, 30f));
        }
    }
    
    private void ScheduleRebite(float delay)
    {
        StartCoroutine(RebiteCoroutine(delay));
    }
    
    private IEnumerator RebiteCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (currentSession?.State == FishingState.Waiting && currentSession.CurrentFish != null)
        {
            var biteBehavior = currentSession.CurrentFish.GetBiteBehavior();
            StartCoroutine(ExecuteBiteCoroutine(biteBehavior));
        }
    }
    
    private void ScheduleNextFish(float delay)
    {
        StartCoroutine(DelayNextFish(delay));
    }
    
    private IEnumerator DelayNextFish(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartFishSpawnLogic();
    }
    
    private void OnFishingComplete(FishingResult result, Fish fish)
    {
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
        
        // Планування наступної риби
        ScheduleNextFish(UnityEngine.Random.Range(5f, 15f));
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
        
        currentPlayer.GainStrength(fish.Weight);
        currentPlayer.GainExperience(Mathf.RoundToInt(fish.Weight * 10));
        
        playerRepository.UpdatePlayerStats(
            currentPlayer.Id,
            fish.Weight,
            Mathf.RoundToInt(fish.Weight * 10)
        );
        
        Debug.Log($"Caught {fish.FishType} weighing {fish.Weight:F2}kg!");
    }
    
    private void HandleFailedCatch(FishingResult result, Fish fish)
    {
        Debug.Log($"Fishing failed: {result}. Fish: {fish?.FishType ?? "None"}");
    }
    
    public FishingSession GetCurrentSession()
    {
        return currentSession;
    }
}