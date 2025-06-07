using UnityEngine;
using System.Collections;

public partial class FishingLogic
{

    private void ResetLineState()
    {
        controller.FloatAnimation.HideFloat();
        controller.SetReeling(false);
        controller.SetFloatCast(false);

        if (controller.sessionManager?.CurrentSession != null)
        {
            controller.sessionManager.EndSession();
        }
    }

}