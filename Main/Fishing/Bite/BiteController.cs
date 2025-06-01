using UnityEngine;
using System.Collections;

public class BiteController : MonoBehaviour
{
    [SerializeField] private FishingController fishingController;
    // ВИДАЛЕНО: [SerializeField] private FloatAnimation floatAnimation; - НЕ ПОТРІБНО!

    [Header("Bite Settings")]
    [SerializeField] private float defaultBiteDuration = 3f;
    [SerializeField] private float defaultBiteSpeed = 1f;
    [SerializeField] private float pullTimeout = 3f;

    private Coroutine biteSequenceCoroutine;
    private Coroutine pullMonitorCoroutine;

    private Fish currentFish;
    private IBiteBehavior currentBiteBehavior;
    
    // ДОДАНО: Властивість для отримання FloatAnimation
    private FloatAnimation FloatAnimation => fishingController?.floatAnimation;
    
    void Start()
    {
        StartCoroutine(DelayedSubscription());
    }
    
    private IEnumerator DelayedSubscription()
    {
        yield return new WaitForSeconds(0.5f);
        
        // ДОДАНО: Перевірка чи все ініціалізовано
        if (fishingController == null)
        {
            Debug.LogError("❌ BiteController: FishingController не встановлений в інспекторі!");
            yield break;
        }
        
        if (FloatAnimation == null)
        {
            Debug.LogError("❌ BiteController: FloatAnimation не ініціалізований в FishingController!");
            yield break;
        }
        
        SubscribeToFishingEvents();
        Debug.Log("🔧 BiteController підписаний на події");
    }
    
    void OnDestroy()
    {
        UnsubscribeFromFishingEvents();
    }
    
    private void SubscribeToFishingEvents()
    {
        if (fishingController.sessionManager != null)
        {
            Debug.Log("✅ Підписка на події SessionManager");
            fishingController.sessionManager.OnStateChanged += HandleStateChanged;
        }
        
        FishingEventBus.Instance.OnFishSpawned += HandleFishSpawned;
    }
    
    private void UnsubscribeFromFishingEvents()
    {
        if (fishingController?.sessionManager != null)
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
        StartCoroutine(DelayedBiteStart(fish));
    }
    
    private IEnumerator DelayedBiteStart(Fish fish)
    {
        float delay = Random.Range(1f, 3f);
        yield return new WaitForSeconds(delay);
        
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
        
        // ВИПРАВЛЕНО: Перевірка FloatAnimation
        if (FloatAnimation == null)
        {
            Debug.LogError("❌ FloatAnimation відсутній! Не можу розпочати клювання!");
            return;
        }
        
        currentFish = fish;
        currentBiteBehavior = fish.GetBiteBehavior();
        
        Debug.Log($"🎣 BiteController: Риба {fish.FishType} почала клювати!");
        
        StopAllCoroutines();
        
        var inputHandler = new BiteInputHandler(fishingController);
        var sequence = new BiteSequence(
            fishingController,
            FloatAnimation, // ВИПРАВЛЕНО: Використовуємо властивість
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