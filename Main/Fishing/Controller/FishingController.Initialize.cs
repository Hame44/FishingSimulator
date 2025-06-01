using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public partial class FishingController 
{
    private void InitializeCachedDelays()
    {
        ShortDelay = new WaitForSeconds(0.1f);
        MediumDelay = new WaitForSeconds(2f);
    }
    
    private void InitializeComponents()
    {
        fishingLogic = new FishingLogic(this);
        floatAnimation = new FloatAnimation(this);
        
        // Ініціалізуємо SessionManager
        sessionManager = new SessionManager();
        
        CreatePlayer();
    }
    
    private void InitializeServices()
    {
        var serviceObject = FindOrCreateServiceObject();
        fishingService = GetOrAddFishingService(serviceObject);
        
        // Підписуємося на події після ініціалізації сервісів
        SubscribeToServiceEvents();
    }
    
    private void SubscribeToServiceEvents()
    {
        // Підписуємося на події FishingEventBus
        FishingEventBus.Instance.OnFishSpawned += HandleFishSpawned;
    }
    
    private void HandleFishSpawned(Fish fish)
    {
        Debug.Log($"🐟 Риба {fish.FishType} з'явилася!");
        
        // Запускаємо клювання через невеликий проміжок часу
        StartCoroutine(DelayedBite(fish));
    }
    
    private IEnumerator DelayedBite(Fish fish)
    {
        // Чекаємо 2-5 секунд перед початком клювання
        float delay = UnityEngine.Random.Range(2f, 5f);
        yield return new WaitForSeconds(delay);
        
        // Перевіряємо чи все ще можна клювати
        if (IsFloatCast && !IsFishBiting && !IsHooked && sessionManager.CurrentSession != null)
        {
            Debug.Log($"🎣 Риба {fish.FishType} починає клювати!");
            // sessionManager.CurrentSession.OnFishBite?.Invoke(fish);
            sessionManager.NotifyFishBite(fish);
        }
    }
    
    private GameObject FindOrCreateServiceObject()
    {
        var serviceObject = GameObject.Find("FishingService");
        return serviceObject ?? new GameObject("FishingService");
    }
    
    private FishingService GetOrAddFishingService(GameObject serviceObject)
    {
        return serviceObject.GetComponent<FishingService>() ?? 
               serviceObject.AddComponent<FishingService>();
    }
    
    private void SetupInitialState()
    {
        CurrentState = FishingState.Ready;
    }

    private void CreatePlayer()
    {
        currentPlayer = new Player 
        { 
            Id = 1, 
            Name = "Рибалка",
            Strength = 10f,
            Experience = 0,
            Equipment = new Equipment
            {
                RodDurability = 100f,
                LineDurability = 100f,
                LineLength = 10f,
                FishingLuck = 1.2f
            }
        };
    }
    
    void OnDestroy()
    {
        // Відписуємося від подій
        if (FishingEventBus.Instance != null)
        {
            FishingEventBus.Instance.OnFishSpawned -= HandleFishSpawned;
        }
    }
}