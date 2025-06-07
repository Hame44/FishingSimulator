using UnityEngine;
using System;

public class PerchFightBehavior : IFightBehavior
{
    private readonly Fish fish;

    public PerchFightBehavior(Fish fish)
    {
        this.fish = fish;
    }

    public bool TryEscape(float playerStrength, float rodDurability, float lineDurability)
    {
        float requiredStrength = GetRequiredStrength();

        // Окунь більш агресивний і швидший, ніж короп
        // Він частіше намагається втекти різкими ривками

        // Базова перевірка сили - окунь потребує менше сили для утримання
        if (playerStrength < requiredStrength * 0.4f)
            return true; // окунь легше вириває вудлище

        // Окунь менше пошкоджує лєску через меншу вагу
        if (lineDurability < fish.Strength * 0.2f)
            return true; // рвется леска

        // Окунь рідше ломає вудку через меншу силу
        if (rodDurability < fish.Strength * 0.15f)
            return true; // ломается удочка

        // Окунь має вищий базовий шанс втечі через агресивність
        float escapeModifier = 1.3f; // окунь на 30% частіше намагається втекти
        return UnityEngine.Random.value < (fish.EscapeChance * escapeModifier);
    }

    public float GetRequiredStrength()
    {
        // Окунь потребує менше сили через меншу вагу, але більше точності
        return fish.Strength * UnityEngine.Random.Range(0.6f, 1.0f);
    }
}