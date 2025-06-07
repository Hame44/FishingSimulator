using UnityEngine;
using System.Collections;

public partial class FishingLogic
{

    private IEnumerator HandleCompletion(FishingState completionState)
    {
        StopFightSequence();
        controller.FloatAnimation.HideFloat();

        ResetAllStates();
        yield return null;
    }


    private void ResetAllStates()
    {
        controller.SetReeling(false);
        controller.SetHooked(false);
        controller.SetFishBiting(false);
        controller.SetFloatCast(false);
        controller.SetCurrentFishDistance(0f);

        if (controller.FloatBobCoroutine != null)
        {
            controller.StopCoroutine(controller.FloatBobCoroutine);
            controller.SetFloatBobCoroutine(null);
        }
    }
}