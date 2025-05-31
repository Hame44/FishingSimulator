using System;
using UnityEngine;

[System.Serializable]
public class CaughtFishData
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string FishType { get; set; }
    public float Weight { get; set; }
    public System.DateTime CaughtAt { get; set; }
}