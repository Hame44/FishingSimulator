using System;
using UnityEngine;

public class Perch : Fish
{
    public Perch(float weight)
    {
        FishType = "Perch";
        Weight = weight;
        Strength = weight * UnityEngine.Random.Range(0.8f, 1.4f);
        EscapeChance = 0.25f;
    }

    public override IBiteBehavior GetBiteBehavior()
    {
        return new PerchBiteBehavior();
    }

    public override IFightBehavior GetFightBehavior()
    {
        return new PerchFightBehavior(this);
    }
}