using System.Collections.Generic;
using UnityEngine;
using System;

public class CarpFactory : FishFactory
{
    public CarpFactory(float luckModifier = 1f)
    {
        this.luckModifier = luckModifier;
    }

    public override Fish CreateFish()
    {
        float baseWeight = GenerateWeight(0.5f, 5f);
        float finalWeight = baseWeight * luckModifier * UnityEngine.Random.Range(0.8f, 1.2f);
        return new Carp(finalWeight);
    }
}