using UnityEngine;
using System;

public interface IBiteBehavior
{
    float BiteSpeed { get; }
    float BiteDuration { get; }
    float RebiteChance { get; }
    float RebiteDelay { get; }
    void ExecuteBite(Action onBiteStart, Action onBiteEnd);
}

public class CarpBiteBehavior : IBiteBehavior
{
    public float BiteSpeed => 0.5f; // середня швидкість
    public float BiteDuration => UnityEngine.Random.Range(2f, 4f);
    public float RebiteChance => 0.2f; // невеликий шанс
    public float RebiteDelay => UnityEngine.Random.Range(3f, 5f);

    public void ExecuteBite(Action onBiteStart, Action onBiteEnd)
    {
        onBiteStart?.Invoke();
        // Логіка клювання коропа
        onBiteEnd?.Invoke();
    }
}

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