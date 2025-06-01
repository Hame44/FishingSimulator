using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public partial class FishingController 
{
    [Header("UI References")]
    public Button castButton;
    public Button hookPullButton;
    public Button releaseButton;
    public Text statusText;
    public Text playerStatsText;
    public Text instructionText;
    public Slider progressBar;
    
    [Header("Visual Elements")]
    public GameObject floatObject;
    public Transform waterSurface;
    public Transform shore;
    public LineRenderer fishingLine;
    
    [Header("Animation Settings")]
    public float floatBobSpeed = 2f;
    public float floatBobIntensity = 0.02f;
    public float biteBobIntensity = 0.15f;
    public float floatSubmergeDepth = 0.1f;
    public AnimationCurve castCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Fishing Parameters")]
    public float castDistance = 5f;
    public float pullSpeed = 2f;
}