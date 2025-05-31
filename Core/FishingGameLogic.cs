using UnityEngine;
using System.Collections;

public class FishingGameLogic
{
    private FishingController controller;
    private FishingAnimator animator;
    private FishingUIManager uiManager;
    
    public FishingGameLogic(FishingController controller)
    {
        this.controller = controller;
        this.animator = new FishingAnimator(controller);
        this.uiManager = new FishingUIManager(controller);
    }
    
    public void UpdateGameLogic()
    {
        var session = controller.FishingService?.GetCurrentSession();
        if (session == null) return;
        
        if (session.State == FishingState.Fighting && controller.IsReeling)
        {
            PullFish();
        }
        
        if (session.State == FishingState.Caught || session.State == FishingState.Escaped)
        {
            if (session.State == FishingState.Escaped)
            {
                uiManager.UpdateStatusText("escaped");
            }
            ResetFishing();
        }
    }
    
    public IEnumerator CastLineCoroutine()
    {
        uiManager.UpdateStatusText("cast");
        
        yield return controller.StartCoroutine(animator.CastAnimation());
        
        controller.FishingService.StartFishing(controller.CurrentPlayer);
        uiManager.UpdateStatusText("waiting");
        uiManager.UpdateButtonStates();
        
        controller.StartCoroutine(animator.FloatBobbing());
        
    }
    
    public void HookOrPull()
    {
        if (IsProcessingAction()) return;
        
        var session = controller.FishingService.GetCurrentSession();
        if (session == null) return;
        
        switch (session.State)
        {
            case FishingState.Biting:
                PerformHook();
                break;
                
            case FishingState.Fighting:
                PerformPull();
                break;
                
            case FishingState.Waiting:
                PerformPrematureHook();
                break;
        }
    }
    
    private void PerformHook()
    {
        var session = controller.FishingService.GetCurrentSession();
        if (session != null && session.TryHook())
        {
        
            uiManager.UpdateStatusText("fighting");
            controller.StartCoroutine(DelayedFightStart());
        }
    }

    private IEnumerator DelayedFightStart()
    {
        yield return new WaitForSeconds(0.5f);
    
        var session = controller.FishingService.GetCurrentSession();
        if (session != null && session.State == FishingState.Hooked)
        {
            session.StartFight();
            StartFightSequence();
        }
    }
    
    private void PerformPull()
    {
        PullFish();
    }
    
    private void PerformPrematureHook()
    {
        uiManager.UpdateStatusText("waiting");
        controller.StartCoroutine(ShowTemporaryMessage("Передчасно! Чекайте поклювки...", 2f));
    }
    
    private void StartFightSequence()
    {
        controller.SetCurrentFishDistance(controller.castDistance);
        controller.SetReeling(true);
        controller.SetFightTimer(0f);
        controller.SetTensionLevel(0f);
        
        if (controller.progressBar != null)
        {
            controller.progressBar.gameObject.SetActive(true);
            controller.progressBar.value = 0f;
        }
        
        if (controller.FightCoroutine != null)
        {
            controller.StopCoroutine(controller.FightCoroutine);
        }
        controller.SetFightCoroutine(controller.StartCoroutine(FightSequenceCoroutine()));
    }
    
    private IEnumerator FightSequenceCoroutine()
    {
        while (controller.IsReeling && controller.CurrentFishDistance > 0.5f && controller.FightTimer < controller.maxFightTime)
        {
            controller.SetFightTimer(controller.FightTimer + Time.deltaTime);
            
            UpdateTension();
            uiManager.UpdateProgressBar();
            
            if (controller.TensionLevel > 0.9f)
            {
                if (Random.value < 0.1f)
                {
                    HandleLineBroken();
                    yield break;
                }
            }
            
            yield return null;
        }
        
        if (controller.FightTimer >= controller.maxFightTime)
        {
            HandleFishEscape("Час боротьби вийшов!");
        }
        
        controller.SetFightCoroutine(null);
    }
    
    private void UpdateTension()
    {
        var session = controller.FishingService.GetCurrentSession();
        if (session?.CurrentFish != null)
        {
            float fishStrength = session.CurrentFish.Strength;
            float distanceFactor = controller.CurrentFishDistance / controller.castDistance;
            controller.SetTensionLevel(Mathf.Clamp01((fishStrength * distanceFactor) / controller.CurrentPlayer.Strength));
        }
    }
    
    private void PullFish()
    {
        if (controller.CurrentFishDistance > 0 && controller.IsReeling)
        {
            var session = controller.FishingService.GetCurrentSession();
            if (session?.CurrentFish == null) return;
        
            float fishResistance = session.CurrentFish.Strength / controller.CurrentPlayer.Strength;
            float basePullSpeed = controller.pullSpeed * Time.deltaTime;
            float adjustedPullSpeed = basePullSpeed / (1f + fishResistance * 0.5f);
        
            if (UnityEngine.Random.value < fishResistance * 0.3f)
            {
                adjustedPullSpeed *= 0.2f;
                if (UnityEngine.Random.value < 0.1f)
                {
                    adjustedPullSpeed = -adjustedPullSpeed * 0.5f;
                }
            }
        
            controller.SetCurrentFishDistance(Mathf.Max(0, controller.CurrentFishDistance - adjustedPullSpeed));
        
            float distanceRatio = controller.CurrentFishDistance / controller.castDistance;
            Vector3 targetPos = Vector3.Lerp(controller.shore.position, controller.FloatTargetPosition, distanceRatio);
        
            Vector3 fightOffset = new Vector3(
                UnityEngine.Random.Range(-0.1f, 0.1f),
                UnityEngine.Random.Range(-0.1f, 0.1f),
                0
            ) * controller.TensionLevel;
        
            if (controller.floatObject != null)
            {
                controller.floatObject.transform.position = targetPos + fightOffset;
            }
        
            uiManager.UpdateStatusText($"Тягнемо рибу! Відстань: {controller.CurrentFishDistance:F1}м (Опір: {fishResistance:F1})");
        
            if (controller.CurrentFishDistance <= 0.1f)
            {
                CompleteCatch();
            }
        }
    }
    
    // Event handlers
    public void OnFishingStateChanged(FishingState newState)
    {
        Debug.Log($"Fishing state changed to: {newState}");
        uiManager.UpdateButtonStates();
        
        switch (newState)
        {
            case FishingState.Biting:
                controller.SetFishBiting(true);
                controller.StartCoroutine(BiteTimerCoroutine());
                break;
            case FishingState.Fighting:
                controller.SetFishBiting(false);
                break;
            default:
                controller.SetFishBiting(false);
                break;
        }
    }
    
    public void OnFishBite(Fish fish)
    {
        uiManager.UpdateStatusText("biting");
    }
    
    public void OnFishingComplete(FishingResult result, Fish fish)
    {
        switch (result)
        {
            case FishingResult.Success:
                uiManager.UpdateStatusText($"Піймали {fish.FishType}! Вага: {fish.Weight:F2}кг");
                break;
            case FishingResult.FishEscaped:
                uiManager.UpdateStatusText("escaped");
                break;
            default:
                uiManager.UpdateStatusText("escaped");
                break;
        }
        
        controller.StartCoroutine(ResetAfterCompletion());
    }
    
    private IEnumerator BiteTimerCoroutine()
    {
        float biteTime = 3f;
        float elapsed = 0f;
        
        while (elapsed < biteTime && controller.IsFishBiting)
        {
            elapsed += Time.deltaTime;
            
            if (controller.timerText != null)
            {
                controller.timerText.text = $"Час: {(biteTime - elapsed):F1}с";
            }
            
            yield return null;
        }
        
        if (controller.timerText != null)
        {
            controller.timerText.text = "";
        }
    }
    
    private void CompleteCatch()
    {
        var session = controller.FishingService.GetCurrentSession();
        if (session?.CurrentFish != null)
        {
            uiManager.UpdateStatusText("caught");
        }
        
        StopFightSequence();
        ResetFishing();
    }
    
    private void HandleLineBroken()
    {
        uiManager.UpdateStatusText("Леска порвалась!");
        StopFightSequence();
        ResetFishing();
    }
    
    private void HandleFishEscape(string reason)
    {
        uiManager.UpdateStatusText($"Риба втекла! {reason}");
        StopFightSequence();
        ResetFishing();
    }
    
    private void StopFightSequence()
    {
        if (controller.FightCoroutine != null)
        {
            controller.StopCoroutine(controller.FightCoroutine);
            controller.SetFightCoroutine(null);
        }
        
        controller.SetReeling(false);
        controller.SetTensionLevel(0f);
        
        if (controller.progressBar != null)
        {
            controller.progressBar.gameObject.SetActive(false);
        }
    }
    
    private void ResetFishing()
    {
        controller.SetFloatCast(false);
        controller.SetFishBiting(false);
        
        StopFightSequence();
        
        if (controller.floatObject != null)
        {
            controller.floatObject.SetActive(false);
            controller.floatObject.transform.position = controller.FloatStartPosition;
        }
        
        if (controller.fishingLine != null)
        {
            controller.fishingLine.enabled = false;
            controller.fishingLine.material.color = controller.normalLineColor;
        }
        
        if (controller.timerText != null)
        {
            controller.timerText.text = "";
        }
        
        uiManager.UpdateButtonStates();
        controller.StartCoroutine(PrepareForNextCast());
    }
    
    private IEnumerator ResetAfterCompletion()
    {
        yield return controller.MediumDelay;
        ResetFishing();
    }
    
    private IEnumerator PrepareForNextCast()
    {
        yield return controller.MediumDelay;
        uiManager.UpdateStatusText("ready");
        uiManager.UpdateButtonStates();
    }
    
    private IEnumerator ShowTemporaryMessage(string message, float duration)
    {
        string originalMessage = controller.statusText?.text;
        uiManager.UpdateStatusText(message);
        
        yield return new WaitForSeconds(duration);
        
        if (!string.IsNullOrEmpty(originalMessage))
        {
            uiManager.UpdateStatusText(originalMessage);
        }
    }
    
    private bool IsProcessingAction()
    {
        return controller.FightCoroutine != null;
    }
}