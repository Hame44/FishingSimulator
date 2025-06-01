using System.Collections;
using UnityEngine;

public class BiteSequence
{
    private readonly FishingController fishingController;
    private readonly FloatAnimation floatAnimation;
    private readonly Fish currentFish;
    // private readonly IBiteBehavior currentBiteBehavior;
    private readonly BiteInputHandler inputHandler;
    private readonly System.Action onHooked;
    private readonly System.Action onMissed;

    public BiteSequence(
        FishingController fishingController,
        FloatAnimation floatAnimation,
        Fish currentFish,
        BiteInputHandler inputHandler,
        System.Action onHooked,
        System.Action onMissed)
    {
        this.fishingController = fishingController;
        this.floatAnimation = floatAnimation;
        this.currentFish = currentFish;
        // this.currentBiteBehavior = this.currentFish?.GetBiteBehavior();
        this.inputHandler = inputHandler;
        this.onHooked = onHooked;
        this.onMissed = onMissed;
    }

    public IEnumerator Run(float defaultDuration, float defaultSpeed)
    {
        fishingController.SetFishBiting(true);
        var biteBehavior = currentFish.GetBiteBehavior();
        float duration = biteBehavior?.BiteDuration ?? defaultDuration;
        // float duration = currentFish.currentBiteBehavior?.BiteDuration ?? defaultDuration;
        float speed = defaultSpeed;

        floatAnimation?.BiteBobbing(speed, duration);

        float timer = duration;

        floatAnimation?.BiteBobbing(speed, duration);

        Debug.Log($"🎣 Клювання розпочато на {duration:F1} секунд");

        while (timer > 0f && fishingController.IsFishBiting && !fishingController.IsHooked)
        {
            timer -= Time.deltaTime;

            if (inputHandler.Check())
            {
                onHooked?.Invoke();
                yield break;
            }

            yield return null;
        }

        if (fishingController.IsFishBiting && !fishingController.IsHooked)
        {
            onMissed?.Invoke();
        }
    }
}