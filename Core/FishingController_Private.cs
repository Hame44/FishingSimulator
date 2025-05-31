using UnityEngine;

public partial class FishingController : MonoBehaviour
{
    private void SubscribeToServiceEvents()
    {
        var session = fishingService.GetCurrentSession();
        if (session != null)
        {
            session.OnStateChanged += gameLogic.OnFishingStateChanged;
            session.OnFishBite += gameLogic.OnFishBite;
            session.OnFishingComplete += gameLogic.OnFishingComplete;
        }
    }
    
    private void CleanupSubscriptions()
    {
        var session = fishingService?.GetCurrentSession();
        if (session != null)
        {
            session.OnStateChanged -= gameLogic.OnFishingStateChanged;
            session.OnFishBite -= gameLogic.OnFishBite;
            session.OnFishingComplete -= gameLogic.OnFishingComplete;
        }
    }
    
    private void CreatePlayer()
    {
        currentPlayer = new Player 
        { 
            Id = 1, 
            Name = "Рибалка",
            Strength = 10f,
            Experience = 0,
            Equipment = new Equipment
            {
                RodDurability = 100f,
                LineDurability = 100f,
                LineLength = 10f,
                FishingLuck = 1.2f
            }
        };
    }
    
    private void SetupInitialState()
    {
        uiManager.SetupUI();
        fishingAnimator.InitializeVisuals();
        visualEffects.InitializeLineRenderer();
        currentState = FishingState.Ready;
        uiManager.UpdateButtonStates();
        uiManager.UpdateStatusText("ready");
    }
}