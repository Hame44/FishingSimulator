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
        FishingEventBus.Instance.OnFishSpawned += HandleFishSpawned;
    }

    private void SetupInitialState()
    {
        if (floatAnimation != null)
        {
            floatAnimation.SetupFloatStartPosition();
        }

        CurrentState = FishingState.Ready;

        Debug.Log("✅ FishingController ініціалізовано повністю");
    }


    private void HandleFishSpawned(Fish fish)
    {
        Debug.Log($"🐟 Риба {fish.FishType} з'явилася!");

        StartCoroutine(DelayedBite(fish));
    }

    private IEnumerator DelayedBite(Fish fish)
    {
        float delay = UnityEngine.Random.Range(2f, 5f);
        yield return new WaitForSeconds(delay);


        if (IsFloatCast)
        {
            Debug.Log($"🎣 Риба {fish.FishType} починає клювати!");
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
        if (FishingEventBus.Instance != null)
        {
            FishingEventBus.Instance.OnFishSpawned -= HandleFishSpawned;
        }
    }
}