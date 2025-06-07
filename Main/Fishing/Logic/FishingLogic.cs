using UnityEngine;
using System.Collections;

public partial class FishingLogic
{
    private FishingController controller;

    public FishingLogic(FishingController controller)
    {
        this.controller = controller;
    }

    public IEnumerator CastToPositionCoroutine(Vector3 targetPosition)
    {
        if (controller.IsFloatCast) yield break;

        controller.SetFloatCast(true);

        controller.FloatAnimation.ShowFloatAtPosition(targetPosition);

        StartFishingSession();

        Debug.Log($"🎣 Поплавок закинуто в позицію: {targetPosition}");
    }

    public void PullLine()
    {
        var session = controller.sessionManager?.CurrentSession;

        if (controller.IsHooked && session?.State == FishingState.Fighting)
        {
            PullFish();
        }
    }

    public void PullEmptyFloatStep()
    {
        if (!controller.IsFloatCast || controller.IsFishBiting || controller.IsHooked || controller.floatObject == null)
            return;

        Vector3 current = controller.floatObject.transform.position;
        Vector3 target = controller.shore.position;
        float speed = 5f;

        if (Vector3.Distance(current, target) > 0.05f)
        {
            Vector3 next = Vector3.MoveTowards(current, target, speed * Time.deltaTime);
            controller.floatObject.transform.position = next;
        }
        else
        {
            controller.FloatAnimation.HideFloat();
            controller.SetReeling(false);
            controller.SetFloatCast(false);
        }
    }

    public void Hook()
    {
        var session = controller.sessionManager?.CurrentSession;

        if (session == null)
        {
            HandlePrematureHook();
            return;
        }

        HandleHookAction(session);
    }

    public bool IsProcessingAction()
    {
        return controller.FightCoroutine != null || controller.IsReeling;
    }

    public void UpdateGameLogic()
    {
        var session = controller.sessionManager?.CurrentSession;
        if (session == null) return;

        UpdateSessionState(session);
        HandleActiveFighting(session);
        CheckForCompletion(session);
    }

    private void StartFishingSession()
    {
        if (controller.FishingService != null && controller.CurrentPlayer != null)
        {
            controller.FishingService.StartFishing(controller.CurrentPlayer);
        }
    }

    private void StartFloatBobbing()
    {
        if (controller.floatAnimation != null)
        {
            var bobCoroutine = controller.StartCoroutine(controller.floatAnimation.BaseBobbing());
            controller.SetFloatBobCoroutine(bobCoroutine);
        }
    }

    private void UpdateSessionState(FishingSession session)
    {
        if (controller.CurrentState != session.State)
        {
            controller.SetCurrentState(session.State);

            UpdateBitingState(session);
        }
    }

    private void UpdateBitingState(FishingSession session)
    {
        bool wasBiting = controller.IsFishBiting;
        bool isBiting = session.State == FishingState.Biting;
        controller.SetFishBiting(isBiting);

    }

    private void HandleActiveFighting(FishingSession session)
    {
        if (session.State == FishingState.Fighting && controller.IsReeling)
        {
            // Активна боротьба керується через UI або безпосередні виклики PullFish()
        }
    }

    private void CheckForCompletion(FishingSession session)
    {
        if (session.State == FishingState.Caught || session.State == FishingState.Escaped)
        {
            controller.StartCoroutine(HandleCompletion(session.State));
        }
    }

}