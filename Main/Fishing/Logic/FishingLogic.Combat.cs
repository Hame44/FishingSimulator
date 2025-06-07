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
        
        // Логування прогресу
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
            adjustedPullSpeed *= 0.2f; // Сповільнення
            
            if (UnityEngine.Random.value < 0.1f)
            {
                adjustedPullSpeed = -adjustedPullSpeed * 0.5f; // Риба тягне назад
            }
        }
        
        float newDistance = controller.CurrentFishDistance - adjustedPullSpeed;
        controller.SetCurrentFishDistance(Mathf.Max(0, newDistance));
    }
    
    // ДОДАНО: Метод для візуального оновлення позиції поплавка
    private void UpdateFishPosition()
{
    if (controller.floatObject == null || controller.shore == null) 
    {
        Debug.LogWarning("❌ floatObject або shore відсутні!");
        return;
    }
    
    // Отримуємо початкову позицію (де був закинутий поплавок)
    Vector3 startPos = controller.FloatAnimation.FloatBasePosition;
    Vector3 endPos = controller.shore.position;
    
    Debug.Log($"🔧 UpdateFishPosition: startPos={startPos}, endPos={endPos}");
    
    // Розраховуємо прогрес витягування (від 0 до 1)
    float totalDistance = 100f;
    float pullProgress = 1f - (controller.CurrentFishDistance / totalDistance);
    pullProgress = Mathf.Clamp01(pullProgress);
    
    // Інтерполюємо позицію між початковою точкою і берегом
    Vector3 newFloatPosition = Vector3.Lerp(startPos, endPos, pullProgress);
    
    // Ефект тремтіння під час боротьби з рибою
    Vector3 fightOffset = new Vector3(
        UnityEngine.Random.Range(-0.08f, 0.08f),
        UnityEngine.Random.Range(-0.05f, 0.05f),
        0
    ) * (1f - pullProgress);
    
    Vector3 finalPosition = newFloatPosition + fightOffset;
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
        
        // ДОДАНО: Анімація завершення ловлі
        controller.StartCoroutine(CompleteCatchAnimation());
        
        Debug.Log("🏆 Риба піймана!");
    }
    
    // ДОДАНО: Анімація завершення ловлі
    private IEnumerator CompleteCatchAnimation()
    {
        // Плавно переміщуємо поплавок до берега
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
        yield return controller.StartCoroutine(ResetAfterCompletion());
    }
    
    private IEnumerator FightSequenceCoroutine()
    {
        while (ShouldContinueFight())
        {
            // controller.SetFightTimer(controller.FightTimer + Time.deltaTime);
            // UpdateTension();
            // controller.UIManager.UpdateProgressBar();
            
            // if (CheckLineBroken())
            // {
            //     HandleLineBroken();
            //     yield break;
            // }
            
            yield return null;
        }
        
        // HandleFightTimeout();
        controller.SetFightCoroutine(null);
    }
    
    private bool ShouldContinueFight()
    {
        return controller.IsReeling && 
               controller.CurrentFishDistance > 0.1f;
            //    controller.FightTimer < controller.maxFightTime;
    }
    
    // private void UpdateTension()
    // {
    //     var session = controller.sessionManager.CurrentSession;
    //     if (session?.CurrentFish == null) return;
        
    //     float fishStrength = session.CurrentFish.Strength;
    //     // float distanceFactor = controller.CurrentFishDistance / controller.castDistance;
    //     // float tension = (fishStrength * distanceFactor) / controller.CurrentPlayer.Strength;
    //     // controller.SetTensionLevel(Mathf.Clamp01(tension));
    // }
    
    // private bool CheckLineBroken()
    // {
    //     // return controller.TensionLevel > 0.9f && UnityEngine.Random.value < 0.02f;
    // }
    
    private void StopFightSequence()
    {
        if (controller.FightCoroutine != null)
        {
            controller.StopCoroutine(controller.FightCoroutine);
            controller.SetFightCoroutine(null);
        }
        
        controller.SetReeling(false);
        // controller.UIManager.HideProgressBar();
    }
    
    private void HandleLineBroken()
    {
        // controller.UIManager.UpdateStatusText("Леска порвалась!");
        StopFightSequence();
        controller.StartCoroutine(ResetAfterCompletion());
    }

}
    
    // private void HandleFightTimeout()
    // {
    //     if (controller.FightTimer >= controller.maxFightTime)
    //     {
    //         // controller.UIManager.UpdateStatusText("Час боротьби вийшов! Риба втекла...");
    //         ReleaseFish();
    //     }
    // }
    
    // private void ReleaseFish()
    // {
    //     var session = controller.FishingService.GetCurrentSession();
    //     if (session != null)
    //     {
    //         session.CompleteFishing(FishingResult.FishEscaped);
    //     }
        
    //     controller.UIManager.UpdateStatusText("escaped");
    //     StopFightSequence();
    //     controller.StartCoroutine(ResetAfterCompletion());
    // }