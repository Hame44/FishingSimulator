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
