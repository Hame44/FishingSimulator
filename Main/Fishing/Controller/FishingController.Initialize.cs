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
        
        // Підписуємося на події після ініціалізації всіх компонентів
        SubscribeToServiceEvents();
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
    
    private void SubscribeToServiceEvents()
    {
        if (fishingService != null)
        {
            // Підписуємося на події сервісу
            Debug.Log("🔔 Підписка на події FishingService");
        }
    }
    
    private void SetupInitialState()
    {
        // Ініціалізуємо початкові позиції поплавка
        if (floatAnimation != null)
        {
            floatAnimation.SetupFloatStartPosition();
        }

        // Налаштовуємо UI кнопки
        SetupUIButtons();
        
        CurrentState = FishingState.Ready;
        
        Debug.Log("✅ FishingController ініціалізовано");
    }

    private void SetupUIButtons()
    {
        // Прив'язуємо кнопки до методів
        if (castButton != null)
        {
            castButton.onClick.RemoveAllListeners();
            castButton.onClick.AddListener(CastLine);
            Debug.Log("🎯 Кнопка закиду налаштована");
        }
        else
        {
            Debug.LogWarning("⚠️ castButton не знайдена в Inspector!");
        }

        if (hookPullButton != null)
        {
            hookPullButton.onClick.RemoveAllListeners();
            hookPullButton.onClick.AddListener(HookingFish);
        }

        if (releaseButton != null)
        {
            releaseButton.onClick.RemoveAllListeners();
            releaseButton.onClick.AddListener(PullingLine);
        }
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
        
        Debug.Log($"👤 Гравець створений: {currentPlayer.Name}");
    }
}