using UnityEngine;
using UnityEngine.UI;
using System.Collections;
// TODO: натяг волосіні ( зверху slide bar)
public partial class FishingController : MonoBehaviour
{
    
    void Awake()
    {
        InitializeCachedDelays();
        InitializeComponents();
    }
    
    void Start()
    {
        InitializeServices();
        // CreatePlayer();
        SetupInitialState();
        StartCoroutine(floatAnimation.BaseBobbing());
    }
    
    void Update()
    {
        if (fishingLogic !=null)
        {
            fishingLogic.UpdateGameLogic();
        }
        // visualEffects.UpdateVisualEffects();
        // uiManager.UpdateUI();
    }
    
    
    public void CastLine()
    {
        if (!IsFloatCast && !fishingLogic.IsProcessingAction())
        {
            StartCoroutine(fishingLogic.CastLineCoroutine());
        }
    }
    
    public void HookingFish() => fishingLogic.Hook();
    public void PullingLine() => fishingLogic.PullLine();
    
}