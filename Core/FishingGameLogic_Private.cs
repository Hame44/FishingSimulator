using UnityEngine;
using System.Collections;

public partial class FishingGameLogic
{
    private void CalculatePullProgress(Fish fish)
    {
        float fishResistance = fish.Strength / controller.CurrentPlayer.Strength;
        float basePullSpeed = controller.pullSpeed * Time.deltaTime;
        float adjustedPullSpeed = basePullSpeed / (1f + fishResistance * 0.5f);
        
        // Риба може опиратися
        if (UnityEngine.Random.value < fishResistance * 0.3f)
        {
            adjustedPullSpeed *= 0.2f;
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
        float distanceRatio = controller.CurrentFishDistance / controller.castDistance;
        controller.Animator.AnimateFighting(distanceRatio, controller.TensionLevel);
    }
    
    private void CheckCatchCompletion()
    {
        if (controller.CurrentFishDistance <= 0.1f)
        {
            CompleteCatch();
        }
    }
    
    private IEnumerator FightSequenceCoroutine()
    {
        while (ShouldContinueFight())
        {
            controller.SetFightTimer(controller.FightTimer + Time.deltaTime);
            UpdateTension();
            controller.UIManager.UpdateProgressBar();
            
            if (CheckLineBroken())
            {
                HandleLineBroken();
                yield break;
            }
            
            yield return null;
        }
        
        HandleFightTimeout();
        controller.SetFightCoroutine(null);
    }
    
    private bool ShouldContinueFight()
    {
        return controller.IsReeling && 
               controller.CurrentFishDistance > 0.5f && 
               controller.FightTimer < controller.maxFightTime;
    }
    
    private void UpdateTension()
    {
        var session = controller.FishingService.GetCurrentSession();
        if (session?.CurrentFish == null) return;
        
        float fishStrength = session.CurrentFish.Strength;
        float distanceFactor = controller.CurrentFishDistance / controller.castDistance;
        float tension = (fishStrength * distanceFactor) / controller.CurrentPlayer.Strength;
        controller.SetTensionLevel(Mathf.Clamp01(tension));
    }
    
    private bool CheckLineBroken()
    {
        return controller.TensionLevel > 0.9f && UnityEngine.Random.value < 0.05f;
    }
    
    private void HandleLineBroken()
    {
        controller.UIManager.UpdateStatusText("Леска порвалась!");
        StopFightSequence();
        controller.StartCoroutine(ResetAfterCompletion());
    }
    
    private void HandleFightTimeout()
    {
        if (controller.FightTimer >= controller.maxFightTime)
        {
            controller.UIManager.UpdateStatusText("Час боротьби вийшов!