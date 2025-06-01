using UnityEngine;
using System.Collections;

public partial class FishingLogic
{
    
    private IEnumerator HandleCompletion(FishingState completionState)
    {
        StopFightSequence();
        
        string message = completionState == FishingState.Caught ? "caught" : "escaped";
        controller.UIManager.UpdateStatusText(message);
        
        yield return controller.MediumDelay;
        yield return controller.StartCoroutine(ResetAfterCompletion());
    }
    
    private IEnumerator ResetAfterCompletion()
    {
        controller.Animator.ResetFloat();
        controller.VisualEffects.HideFishingLine();
        
        // Скидаємо всі стани
        ResetAllStates();
        
        yield return new WaitForSeconds(1f);
        
        controller.UIManager.UpdateStatusText("ready");
        controller.UIManager.UpdateButtonStates();
    }
    
    private void ResetAllStates()
    {
        controller.SetReeling(false);
        controller.SetHooked(false);
        controller.SetFishBiting(false);
        controller.SetFloatCast(false);
        controller.SetCurrentFishDistance(0f);
        controller.SetFightTimer(0f);
        controller.SetTensionLevel(0f);
        
        // Зупиняємо всі корутіни
        if (controller.FloatBobCoroutine != null)
        {
            controller.StopCoroutine(controller.FloatBobCoroutine);
            controller.SetFloatBobCoroutine(null);
        }
    }
}