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
        SubscribeToFishingEvents();
    }
    
    void OnDestroy()
    {
        UnsubscribeFromFishingEvents();
    }
    
    private void SubscribeToFishingEvents()
    {
        var session = fishingController?.FishingService?.GetCurrentSession();
        if (session != null)
        {
            session.OnFishBite += StartBite; // Підписуємося на появу риби
        }
    }
    
    private void UnsubscribeFromFishingEvents()
    {
        var session = fishingController?.FishingService?.GetCurrentSession();
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
        fishingController.IsHooked = true;
        fishingController.IsBiting = false;

        var pullMonitor = new BitePullMonitor(fishingController, pullTimeout, OnFishLost);
        pullMonitorCoroutine = StartCoroutine(pullMonitor.Monitor());
    }

    private void OnBiteMissed()
    {
        Debug.Log("❌ BiteController: Гравець не встиг підсікти");
        fishingController.IsBiting = false;
        TryRebite();
    }

    private void OnFishLost()
    {
        Debug.Log("🐟 BiteController: Риба втрачена через бездіяльність");
        fishingController.IsHooked = false;

        TryRebite();
    }

    private void TryRebite()
    {
        StopAllCoroutines();

        var rebiteHandler = new BiteRebiteHandler(
            currentBiteBehavior,
            () => StartBite(currentFish), // Та ж риба клює знову
            () => {
                Debug.Log("💨 BiteController: Риба остаточно втекла");
                // Повідомляємо FishingService що ця риба втекла
                NotifyFishEscaped();
            }
        );

        StartCoroutine(rebiteHandler.TryRebite());
    }
    
    private void NotifyFishEscaped()
    {
        var session = fishingController?.FishingService?.GetCurrentSession();
        session?.CompleteFishing(FishingResult.MissedBite);
    }
}