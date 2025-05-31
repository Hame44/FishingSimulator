using UnityEngine;
using System;

public interface IFightBehavior
{
    bool TryEscape(float playerStrength, float rodDurability, float lineDurability);
    float GetRequiredStrength();
}

