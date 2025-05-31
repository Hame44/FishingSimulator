using System.Collections;
using UnityEngine;

public class BiteRebiteHandler
{
    private readonly IBiteBehavior biteBehavior;
    private readonly System.Action onRebite;
    private readonly System.Action onEscape;

    public BiteRebiteHandler(IBiteBehavior behavior, System.Action onRebite, System.Action onEscape)
    {
        biteBehavior = behavior;
        this.onRebite = onRebite;
        this.onEscape = onEscape;
    }

    public IEnumerator TryRebite()
    {
        float chance = biteBehavior.RebiteChance;
        float delay = biteBehavior.RebiteDelay;

        if (Random.value < chance)
        {
            Debug.Log($"🔄 Риба клюне знову через {delay:F1}с");
            yield return new WaitForSeconds(delay);
            onRebite?.Invoke();
        }
        else
        {
            Debug.Log("🏃 Риба остаточно втекла");
            onEscape?.Invoke();
        }
    }
}
