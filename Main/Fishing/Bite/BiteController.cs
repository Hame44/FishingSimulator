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
        // Відкладаємо підписку до наступного фрейму, щоб всі компоненти встигли ініціалізуватися
        StartCoroutine(DelayedSubscription());
    }
    
    void OnDestroy()
    {
        UnsubscribeFromFishingEvents();
    }
    
    private IEnumerator DelayedSubscription()
    {
        yield return null; // Чекаємо один фрейм
        SubscribeToFishingEvents();
    }
    
    private void SubscribeToFishingEvents()
    {
        if (fishingController?.sessionManager?.CurrentSession != null)
        {
            var session = fishingController.sessionManager.CurrentSession;
            session.OnFishBite += StartBite;
            Debug.Log("🔔 BiteController підписався на події");
        }
        else
        {
            Debug.LogWarning("⚠️ BiteController: sessionManager або CurrentSession недоступні");
            // Спробуємо підписатися пізніше
            StartCoroutine(RetrySubscription());
        }
    }
    
    private IEnumerator RetrySubscription()
    {
        int attempts = 0;
        while (attempts < 10 && (fishingController?.sessionManager?.CurrentSession == null))
        {
            yield return new WaitForSeconds(0.5f);
            attempts++;
        }
        
        if (fishingController?.sessionManager?.CurrentSession != null)
        {
            SubscribeToFishingEvents();
        }
        else
        {
            Debug.LogError("❌ BiteController: Не вдалося підписатися на події після 10 спроб");
        }
    }
    
    private void UnsubscribeFromFishingEvents()
    {
        var session = fishingController?.sessionManager?.CurrentSession;
        if (session != null)
        {
            session.OnFishBite -= StartBite;
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

        if (currentBiteBehavior != null)
        {
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
        else
        {
            NotifyFishEscaped();
        }
    }
    
    private void NotifyFishEscaped()
    {
        var session = fishingController?.sessionManager?.CurrentSession;
        session?.CompleteFishing(FishingResult.MissedBite);
    }
}