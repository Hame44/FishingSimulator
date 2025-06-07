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
        // sessionManager = new SessionManager();
            if (sessionManager == null)
    {
        sessionManager = GetComponent<SessionManager>();
        if (sessionManager == null)
        {
            Debug.LogWarning("⚠️ SessionManager не знайдений на об'єкті! Створюємо новий...");
            sessionManager = gameObject.AddComponent<SessionManager>();
        }
    }
        IsReeling = false;
        
        CreatePlayer();
    }
    
    private void InitializeServices()
    {
        var serviceObject = FindOrCreateServiceObject();
        fishingService = GetOrAddFishingService(serviceObject);
        
        // Підписуємося на події після ініціалізації сервісів
        if (fishingService != null && sessionManager != null)
    {
        fishingService.SetSessionManager(sessionManager);
        Debug.Log("✅ SessionManager переданий до FishingService");
    }
    else
    {
        Debug.LogError($"❌ Помилка ініціалізації: FishingService={fishingService != null}, SessionManager={sessionManager != null}");
    }
    
    Debug.Log("✅ Сервіси ініціалізовано");
        SubscribeToServiceEvents();
    }
    
    private void SubscribeToServiceEvents()
    {
        // Підписуємося на події FishingEventBus
        FishingEventBus.Instance.OnFishSpawned += HandleFishSpawned;
    }

        private void SetupInitialState()
    {
        // Ініціалізуємо початкові позиції поплавка
        if (floatAnimation != null)
        {
            floatAnimation.SetupFloatStartPosition();
        }

        // Налаштовуємо UI кнопки - ЦЕ КРИТИЧНО ВАЖЛИВО!
        // SetupUIButtons();
        
        CurrentState = FishingState.Ready;
        
        Debug.Log("✅ FishingController ініціалізовано повністю");
    }

    // private void SetupUIButtons()
    // {
    //     // ЗМІНЕНО: Видаляємо налаштування кнопки закиду
    //     // Закидання тепер відбувається по кліку миші через WaterClickHandler
        
    //     if (hookPullButton != null)
    //     {
    //         hookPullButton.onClick.RemoveAllListeners();
    //         hookPullButton.onClick.AddListener(HookingFish);
    //     }

    //     if (releaseButton != null)
    //     {
    //         releaseButton.onClick.RemoveAllListeners();
    //         releaseButton.onClick.AddListener(PullingLine);
    //     }
        
    //     Debug.Log("🎯 UI кнопки налаштовані (без кнопки закиду)");
    // }
    
    private void HandleFishSpawned(Fish fish)
    {
        Debug.Log($"🐟 Риба {fish.FishType} з'явилася!");
        
        // Запускаємо клювання через невеликий проміжок часу
        // StopCoroutine(fishingAnimator.BaseBobbing());
        StartCoroutine(DelayedBite(fish));
    }
    
    private IEnumerator DelayedBite(Fish fish)
    {
        // Чекаємо 2-5 секунд перед початком клювання
        float delay = UnityEngine.Random.Range(2f, 5f);
        yield return new WaitForSeconds(delay);
        
        
        // Перевіряємо чи все ще можна клювати
        if (IsFloatCast)
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
    
    // private void SetupInitialState()
    // {
    //     CurrentState = FishingState.Ready;
    // }

    private void CreatePlayer()
    {
        currentPlayer = new Player 
        { 
            Id = 1, 
            Name = "Рибалка",
            Strength = 100f,
            Experience = 0,
            Equipment = new Equipment
            {
                RodDurability = 100f,
                LineDurability = 100f,
                LineLength = 100f,
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