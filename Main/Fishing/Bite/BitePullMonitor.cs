using System.Collections;
using UnityEngine;

public class BitePullMonitor
{
    private readonly FishingController fishingController;
    private readonly float timeout;
    private readonly System.Action onFishLost;

    public BitePullMonitor(FishingController controller, float timeout, System.Action onFishLost)
    {
        fishingController = controller;
        this.timeout = timeout;
        this.onFishLost = onFishLost;
    }

    public IEnumerator Monitor()
    {
        float timer = 0f;

        while (fishingController.IsHooked)
        {
            bool isPulling = Input.GetMouseButton(1) || (fishingController?.IsReeling ?? false);

            timer = isPulling ? 0f : timer + Time.deltaTime;

            if (timer > timeout)
            {
                onFishLost?.Invoke();
                yield break;
            }

            yield return null;
        }
    }
}
