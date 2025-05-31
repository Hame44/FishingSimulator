using System;
using UnityEngine;

public class Carp : Fish
{
    public Carp(float weight)
    {
        FishType = "Carp";
        Weight = weight;
        Strength = weight * UnityEngine.Random.Range(1.2f, 1.8f);
        EscapeChance = 0.15f;
    }

    public override IBiteBehavior GetBiteBehavior()
    {
        return new CarpBiteBehavior();
    }

    public override IFightBehavior GetFightBehavior()
    {
        return new CarpFightBehavior(this);
    }
}