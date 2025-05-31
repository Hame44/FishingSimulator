using System;
using UnityEngine;

public abstract class Fish
{
    public float Weight { get; protected set; }
    public float Strength { get; protected set; }
    public float EscapeChance { get; protected set; }
    public string FishType { get; protected set; }
    
    public abstract IBiteBehavior GetBiteBehavior();
    public abstract IFightBehavior GetFightBehavior();
}