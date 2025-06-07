using System.Collections;
using UnityEngine;

public class BiteSequence
{
    private readonly FishingController fishingController;
    private readonly FloatAnimation floatAnimation;
    private readonly Fish currentFish;
    private readonly System.Action onHooked;
    private readonly System.Action onMissed;

    public BiteSequence(
        FishingController fishingController,
        FloatAnimation floatAnimation,
        Fish currentFish,
        System.Action onHooked,
        System.Action onMissed)
    {
        this.fishingController = fishingController;
        this.floatAnimation = floatAnimation;
        this.currentFish = currentFish;
        this.onHooked = onHooked;
        this.onMissed = onMissed;
    }

    public IEnumerator Run(float defaultDuration, float defaultSpeed)
    {
        fishingController.SetFishBiting(true);
        var biteBehavior = currentFish.GetBiteBehavior();
        float duration = biteBehavior?.BiteDuration ?? defaultDuration;
        float speed = defaultSpeed;

        float timer = duration;
        Coroutine bobbingCoroutine = fishingController.StartCoroutine(floatAnimation.BiteBobbing(speed, duration));

        Debug.Log($"🎣 Клювання розпочато на {duration:F1} секунд");

        while (timer > 0f && fishingController.IsFishBiting && !fishingController.IsHooked)
        {
            timer -= Time.deltaTime;

            if (Input.GetMouseButtonDown(1)) // ПКМ
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