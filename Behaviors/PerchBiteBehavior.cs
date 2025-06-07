using UnityEngine;
using System;

public class PerchBiteBehavior : IBiteBehavior
{
    public float BiteSpeed => 4f; // швидкий удар
    public float BiteDuration => UnityEngine.Random.Range(0.8f, 1.5f);
    public float RebiteChance => 0.85f; // великий шанс серії ударів
    public float RebiteDelay => UnityEngine.Random.Range(1.5f, 2f);

    public void ExecuteBite(Action onBiteStart, Action onBiteEnd)
    {
        onBiteStart?.Invoke();
        // Логіка клювання окуня
        onBiteEnd?.Invoke();
    }
}