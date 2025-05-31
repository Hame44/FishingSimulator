using UnityEngine;
using System;

public interface IFightBehavior
{
    bool TryEscape(float playerStrength, float rodDurability, float lineDurability);
    float GetRequiredStrength();
}

public class CarpFightBehavior : IFightBehavior
{
    private readonly Fish fish;

    public CarpFightBehavior(Fish fish)
    {
        this.fish = fish;
    }

    public bool TryEscape(float playerStrength, float rodDurability, float lineDurability)
    {
        float requiredStrength = GetRequiredStrength();
        
        // Різні способи втечі риби
        if (playerStrength < requiredStrength * 0.5f)
            return true; // риба вириває вудлище з рук
        
        if (lineDurability < fish.Strength * 0.3f)
            return true; // рвется леска
        
        if (rodDurability < fish.Strength * 0.2f)
            return true; // ломается удочка
        
        return UnityEngine.Random.value < fish.EscapeChance;
    }

    public float GetRequiredStrength()
    {
        return fish.Strength * UnityEngine.Random.Range(0.8f, 1.2f);
    }
}
