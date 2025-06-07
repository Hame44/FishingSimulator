using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class FishFactory
{
    protected float luckModifier = 1f;

    public abstract Fish CreateFish();

    public void SetLuckModifier(float luck)
    {
        luckModifier = luck;
    }

    protected float GenerateWeight(float minWeight, float maxWeight)
    {
        return UnityEngine.Random.Range(minWeight, maxWeight);
    }
}