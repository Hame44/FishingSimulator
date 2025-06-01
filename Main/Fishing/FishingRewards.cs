using System;

[System.Serializable]
public class FishingRewards
{
    public float Strength { get; set; }
    public int Experience { get; set; }
    public int Money { get; set; }
    
    public FishingRewards(float strength, int experience, int money = 0)
    {
        Strength = strength;
        Experience = experience;
        Money = money;
    }
}