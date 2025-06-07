using System;
using UnityEngine;

public class Equipment
{
    public float RodDurability { get; set; }
    public float LineDurability { get; set; }
    public float LineLength { get; set; }
    public float FishingLuck { get; set; }

    public void DamageRod(float damage)
    {
        RodDurability -= damage;
        if (RodDurability < 0) RodDurability = 0;
    }

    public void DamageLine(float damage)
    {
        LineDurability -= damage;
        if (LineDurability < 0) LineDurability = 0;
    }
}