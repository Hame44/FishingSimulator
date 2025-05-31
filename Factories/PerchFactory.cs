using System.Collections.Generic;
using UnityEngine;
using System;

public class PerchFactory : FishFactory
{
    public PerchFactory(float luckModifier = 1f)
    {
        this.luckModifier = luckModifier;
    }

    public override Fish CreateFish()
    {
        float baseWeight = GenerateWeight(0.1f, 1.5f);
        float finalWeight = baseWeight * luckModifier * UnityEngine.Random.Range(0.9f, 1.1f);
        return new Perch(finalWeight);
    }
}