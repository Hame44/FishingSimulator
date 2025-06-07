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
    }
    
    void Update()
    {
        if (fishingLogic !=null)
        {
            fishingLogic.UpdateGameLogic();
        }
    }
    
     public void CastToPosition(Vector3 position)
    {
        if (!IsFloatCast && !fishingLogic.IsProcessingAction())
        {
            StartCoroutine(fishingLogic.CastToPositionCoroutine(position));
        }
    }
    
    public void HookingFish() => fishingLogic.Hook();
    public void PullingLine() => fishingLogic.PullLine();
    
}