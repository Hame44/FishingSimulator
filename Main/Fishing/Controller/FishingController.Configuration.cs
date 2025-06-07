using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public partial class FishingController 
{
    
    [Header("Visual Elements")]
    public GameObject floatObject;
    public Transform shore;
    public LineRenderer fishingLine;
    
    [Header("Animation Settings")]
    public float floatBobSpeed = 2f;
    public float floatBobIntensity = 0.02f;
    public float biteBobIntensity = 0.15f;
    public float floatSubmergeDepth = 0.1f;
    
    [Header("Fishing Parameters")]
    public float pullSpeed = 2f;
}