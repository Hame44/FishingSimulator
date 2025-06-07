using UnityEngine;
using System.Collections;

public partial class FishingLogic
{

    public void PullFish()
    {
        var session = controller.sessionManager?.CurrentSession;
        if (session?.State != FishingState.Fighting || !controller.IsReeling) return;

        var fish = session.CurrentFish;
        if (fish == null) return;

        CalculatePullProgress(fish);
        UpdateFishPosition(); // ДОДАНО: Оновлюємо візуальну позицію поплавка
        CheckCatchCompletion();

        if (Time.time % 1f < 0.1f) // Кожну секунду
        {
            float progress = (100f - controller.CurrentFishDistance);
            Debug.Log($"🎣 Прогрес витягування: {progress:F0}% (дистанція: {controller.CurrentFishDistance:F1}м)");
        }
    }

    private void CalculatePullProgress(Fish fish)
    {
        float fishResistance = fish.Strength / controller.CurrentPlayer.Strength;
        float basePullSpeed = controller.pullSpeed * Time.deltaTime;
        float adjustedPullSpeed = basePullSpeed / (1f + fishResistance * 0.5f);

        // Риба може чинити опір
        if (UnityEngine.Random.value < fishResistance * 0.3f)
        {
            // adjustedPullSpeed *= 0.2f; // Сповільнення

            if (UnityEngine.Random.value < 0.1f)
            {
                adjustedPullSpeed = -adjustedPullSpeed * 0.5f; // Риба тягне назад
            }
        }

        float newDistance = controller.CurrentFishDistance - adjustedPullSpeed;
        controller.SetCurrentFishDistance(Mathf.Max(0, newDistance));
    }

    private void UpdateFishPosition()
    {
        if (controller.floatObject == null || controller.shore == null)
        {
            Debug.LogWarning("❌ floatObject або shore відсутні!");
            return;
        }

        Vector3 startPos = controller.FloatAnimation.FloatBasePosition;
        Vector3 endPos = controller.shore.position;

        Debug.Log($"🔧 UpdateFishPosition: startPos={startPos}, endPos={endPos}");

        float totalDistance = 100f;
        float pullProgress = 1f - (controller.CurrentFishDistance / totalDistance);
        pullProgress = Mathf.Clamp01(pullProgress);

        Vector3 newFloatPosition = Vector3.Lerp(startPos, endPos, pullProgress);


        Vector3 finalPosition = newFloatPosition;
        controller.floatObject.transform.position = finalPosition;

        Debug.Log($"🎣 Поплавок переміщено: {finalPosition}, прогрес: {pullProgress * 100:F0}%, дистанція: {controller.CurrentFishDistance:F1}");
    }

    private void CheckCatchCompletion()
    {
        if (controller.CurrentFishDistance <= 0.1f)
        {
            CompleteCatch();
        }
    }

    private void CompleteCatch()
    {
        var session = controller.sessionManager?.CurrentSession;
        if (session != null)
        {
            session.CompleteFishing(FishingResult.Success);
        }

        controller.SetReeling(false);
        StopFightSequence();

        controller.StartCoroutine(CompleteCatchAnimation());

        // Debug.Log("🏆 Риба піймана!");
    }

    private IEnumerator CompleteCatchAnimation()
    {
        Vector3 startPos = controller.floatObject.transform.position;
        Vector3 endPos = controller.shore.position;

        float animationTime = 1.5f;
        float elapsed = 0f;

        while (elapsed < animationTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationTime;

            Vector3 currentPos = Vector3.Lerp(startPos, endPos, progress);
            controller.floatObject.transform.position = currentPos;

            yield return null;
        }

        // Завершуємо анімацію
        // yield return controller.StartCoroutine(ResetAfterCompletion());
        ResetAllStates();
    }

    private IEnumerator FightSequenceCoroutine()
    {
        while (ShouldContinueFight())
        {

            yield return null;
        }

        // HandleFightTimeout();
        controller.SetFightCoroutine(null);
    }

    private bool ShouldContinueFight()
    {
        return controller.IsReeling &&
               controller.CurrentFishDistance > 0.1f;
    }


    private void StopFightSequence()
    {
        if (controller.FightCoroutine != null)
        {
            controller.StopCoroutine(controller.FightCoroutine);
            controller.SetFightCoroutine(null);
        }

        controller.SetReeling(false);
    }

    private void HandleLineBroken()
    {
        StopFightSequence();
        // controller.StartCoroutine(ResetAfterCompletion());
        ResetAllStates();
    }

}