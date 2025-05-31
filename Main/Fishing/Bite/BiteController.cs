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

    public void StartBite(Fish fish)
    {
        currentFish = fish;
        currentBiteBehavior = fish.GetBiteBehavior();

        StopAllCoroutines();

        var inputHandler = new BiteInputHandler(fishingController);
        var biteSequence = new BiteSequence(
            fishingController,
            floatAnimation,
            currentFish,
            inputHandler,
            OnFishHooked,
            OnBiteMissed
        );

        biteSequenceCoroutine = StartCoroutine(
            biteSequence.Run(defaultBiteDuration, defaultBiteSpeed)
        );
    }

    private void OnFishHooked()
    {
        Debug.Log("✅ Риба підсічена!");
        fishingController.IsHooked = true;
        fishingController.IsBiting = false;

        // Старт моніторингу: чи гравець тягне рибу
        var pullMonitor = new BitePullMonitor(fishingController, pullTimeout, OnFishLost);
        pullMonitorCoroutine = StartCoroutine(pullMonitor.Monitor());
    }

    private void OnBiteMissed()
    {
        Debug.Log("❌ Гравець не встиг підсікти");
        fishingController.IsBiting = false;

        TryRebite();
    }

    private void OnFishLost()
    {
        Debug.Log("🐟 Риба втрачена через бездіяльність");
        fishingController.IsHooked = false;

        TryRebite();
    }

    private void TryRebite()
    {
        StopAllCoroutines();

        var rebiteHandler = new BiteRebiteHandler(
            currentBiteBehavior(),
            () => StartBite(currentFish),
            () => Debug.Log("💨 Риба остаточно втекла")
        );

        StartCoroutine(rebiteHandler.TryRebite());
    }
}
