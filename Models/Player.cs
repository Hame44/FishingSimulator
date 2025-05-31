using System;
using UnityEngine;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; }
    public float Strength { get; set; }
    public int Experience { get; set; }
    public Equipment Equipment { get; set; }
    
    public void GainStrength(float fishWeight)
    {
        // Реалістична прогресія сили
        float strengthGain = fishWeight / (1 + Strength * 0.1f);
        Strength += strengthGain;
    }
    
    public void GainExperience(int exp)
    {
        Experience += exp;
    }
}