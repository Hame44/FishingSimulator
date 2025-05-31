using System;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public float Strength { get; set; }
    public int Experience { get; set; }
    public float RodDurability { get; set; }
    public float LineDurability { get; set; }
    public float LineLength { get; set; }
    public float FishingLuck { get; set; }
    public System.DateTime LastPlayed { get; set; }
}