using UnityEngine;
using System.Collections;

public class BiteController : MonoBehaviour
{
    [SerializeField] private FishingController fishingController;
    [SerializeField] private FloatAnimation floatAnimation;

    [Header("Bite Settings")]
    [SerializeField] private float defaultBiteDuration = 3f;
    [SerializeField] private float defaultBiteSpeed = 1f;
    [SerializeField] private float pullTimeout = 3f;

    private Coroutine biteSequenceCoroutine;
    private Coroutine pullMonitorCoroutine;

    private Fish currentFish;
    private IBiteBehavior currentBiteBehavior;
    
    void Start()
    {
        // Відкладаємо підписку, щоб переконатися що все ініціалізовано
        StartCoroutine(DelayedSubscription());
    }
    
    private IEnumerator DelayedSubscription()
    {
        // Чекаємо поки все ініціалізується
        yield return new WaitForSeconds(0.5f);
        
        SubscribeToFishingEvents();
        
        Debug.Log("🔧 BiteController підписаний на події");
    }
    
    void OnDestroy()
    {
        UnsubscribeFromFishingEvents();
    }
    
    private void SubscribeToFishingEvents()
    {
        // Підписуємося безпосередньо на SessionManager
        if (fishingController.sessionManager != null)
        {
            Debug.Log("✅ Підписка на події SessionManager");
            // Створюємо підписку через SessionManager
            fishingController.sessionManager.OnStateChanged += HandleStateChanged;
        }
        
        // Також підписуємося на Event Bus для отримання риби
        FishingEventBus.Instance.OnFishSpawned += HandleFishSpawned;
    }
    
    private void UnsubscribeFromFishingEvents()
    {
        if (fishingController.sessionManager != null)
        {
            fishingController.sessionManager.OnStateChanged -= HandleStateChanged;
        }
        
        if (FishingEventBus.Instance != null)
        {
            FishingEventBus.Instance.OnFishSpawned -= HandleFishSpawned;
        }
    }
    
    private void HandleStateChanged(FishingState state)
    {
        if (state == FishingState.Biting && currentFish != null)
        {
            StartBite(currentFish);
        }
    }
    
    private void HandleFishSpawned(Fish fish)
    {
        currentFish = fish;
        Debug.Log($"🐟 BiteController отримав рибу: {fish.FishType}");
        
        // Запускаємо клювання через деякий час
        StartCoroutine(DelayedBiteStart(fish));
    }
    
    private IEnumerator DelayedBiteStart(Fish fish)
    {
        // Чекаємо рандомний час перед клюванням (1-3 секунди)
        float delay = Random.Range(1f, 3f);
        yield return new WaitForSeconds(delay);
        
        // Перевіряємо чи можна клювати
        if (fishingController.IsFloatCast && 
            !fishingController.IsFishBiting && 
            !fishingController.IsHooked)
        {
            StartBite(fish);
        }
    }
    
    public void StartBite(Fish fish)
    {
        if (fish == null) return;
        
        currentFish = fish;
        currentBiteBehavior = fish.GetBiteBehavior();
        
        Debug.Log($"🎣 BiteController: Риба {fish.FishType} почала клювати!");
        
        StopAllCoroutines();
        
        var inputHandler = new BiteInputHandler(fishingController);
        var sequence = new BiteSequence(
            fishingController,
            floatAnimation,
            currentFish,
            inputHandler,
            OnFishHooked,
            OnBiteMissed
        );

        biteSequenceCoroutine = StartCoroutine(sequence.Run(defaultBiteDuration, defaultBiteSpeed));
    }

    private void OnFishHooked()
    {
        Debug.Log("✅ BiteController: Риба підсічена!");
        fishingController.SetHooked(true);
        fishingController.SetFishBiting(false);

        var pullMonitor = new BitePullMonitor(fishingController, pullTimeout, OnFishLost);
        pullMonitorCoroutine = StartCoroutine(pullMonitor.Monitor());
    }

    private void OnBiteMissed()
    {
        Debug.Log("❌ BiteController: Гравець не встиг підсікти");
        fishingController.SetFishBiting(false);
        TryRebite();
    }

    private void OnFishLost()
    {
        Debug.Log("🐟 BiteController: Риба втрачена через бездіяльність");
        fishingController.SetHooked(false);
        TryRebite();
    }

    private void TryRebite()
    {
        StopAllCoroutines();

        var rebiteHandler = new BiteRebiteHandler(
            currentBiteBehavior,
            () => StartBite(currentFish),
            () => {
                Debug.Log("💨 BiteController: Риба остаточно втекла");
                NotifyFishEscaped();
            }
        );

        StartCoroutine(rebiteHandler.TryRebite());
    }
    
    private void NotifyFishEscaped()
    {
        var session = fishingController.sessionManager.CurrentSession;
        session?.CompleteFishing(FishingResult.MissedBite);
    }
}