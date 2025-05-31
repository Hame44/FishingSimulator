using UnityEngine;
using System;

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