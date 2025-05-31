using UnityEngine;

public partial class FishingController : MonoBehaviour
{
    // State setters (внутрішні методи)
    internal void SetFloatCast(bool value) => isFloatCast = value;
    internal void SetFishBiting(bool value) => isFishBiting = value;
    internal void SetReeling(bool value) => isReeling = value;
    internal void SetHooked(bool value) => isHooked = value;
    internal void SetFighting(bool value) => isFighting = value;
    internal void SetCurrentState(FishingState value) => currentState = value;
    internal void SetCurrentFishDistance(float value) => currentFishDistance = value;
    internal void SetFightTimer(float value) => fightTimer = value;
    internal void SetTensionLevel(float value) => tensionLevel = value;
    internal void SetFightCoroutine(Coroutine value) => fightCoroutine = value;
    internal void SetFloatBobCoroutine(Coroutine value) => floatBobCoroutine = value;
    internal void SetOriginalFloatPosition(Vector3 value) => originalFloatPosition = value;
    internal void SetFloatCastPosition(Vector3 value) => floatCastPosition = value;
    
    internal Coroutine FightCoroutine => fightCoroutine;
    internal Coroutine FloatBobCoroutine => floatBobCoroutine;
    
    private void InitializeComponents()
    {
        fishingAnimator = new FishingAnimator(this);
        uiManager = new FishingUIManager(this);
        gameLogic = new FishingGameLogic(this);
        visualEffects = new FishingVisualEffects(this);
    }
    
    private void InitializeServices()
    {
        GameObject serviceObject = GameObject.Find("FishingService");
        if (serviceObject == null)
        {
            serviceObject = new GameObject("FishingService");
        }
        
        fishingService = serviceObject.GetComponent<FishingService>();
        if (fishingService == null)
        {
            fishingService = serviceObject.AddComponent<FishingService>();
        }
        
        SubscribeToServiceEvents();
    }
}