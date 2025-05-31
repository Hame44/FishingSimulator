using UnityEngine;
using System;

public class PerchBiteBehavior : IBiteBehavior
{
    public float BiteSpeed => 2f; // швидкий удар
    public float BiteDuration => UnityEngine.Random.Range(0.5f, 1f);
    public float RebiteChance => 0.8f; // великий шанс серії ударів
    public float RebiteDelay => UnityEngine.Random.Range(0.5f, 1.5f);

    public void ExecuteBite(Action onBiteStart, Action onBiteEnd)
    {
        onBiteStart?.Invoke();
        // Логіка клювання окуня
        onBiteEnd?.Invoke();
    }
}