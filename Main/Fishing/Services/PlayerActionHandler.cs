using UnityEngine;

public class PlayerActionHandler : IPlayerActionHandler
{
    private readonly IActionValidator validator;
    private readonly IActionExecutor executor;

    public PlayerActionHandler()
    {
        validator = new ActionValidator();
        executor = new ActionExecutor();
    }

    public void HandleAction(FishingAction action, FishingSession session)
    {
        if (session == null)
        {
            Debug.LogWarning("❌ Немає активної сесії для виконання дії");
            return;
        }

        if (!validator.CanExecuteAction(action, session.State))
        {
            Debug.LogWarning($"❌ Дія {action} неможлива в стані {session.State}");
            return;
        }

        executor.ExecuteAction(action, session);
    }
}