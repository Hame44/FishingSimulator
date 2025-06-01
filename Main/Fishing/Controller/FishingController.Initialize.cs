using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public partial class FishingController 
{
    private void InitializeCachedDelays()
    {
        ShortDelay = new WaitForSeconds(0.1f);
        MediumDelay = new WaitForSeconds(2f);
    }
    
    private void InitializeComponents()
    {
        // fishingAnimator = new FishingAnimator(this);
        // uiManager = new FishingUIManager(this);
        fishingLogic = new FishingLogic(this);
        // visualEffects = new FishingVisualEffects(this);
        floatAnimation = new FloatAnimation(this);

        CreatePlayer();
    }
    
    private void InitializeServices()
    {
        var serviceObject = FindOrCreateServiceObject();
        fishingService = GetOrAddFishingService(serviceObject);
        // SubscribeToServiceEvents();
    }
    
    private GameObject FindOrCreateServiceObject()
    {
        var serviceObject = GameObject.Find("FishingService");
        return serviceObject ?? new GameObject("FishingService");
    }
    
    private FishingService GetOrAddFishingService(GameObject serviceObject)
    {
        return serviceObject.GetComponent<FishingService>() ?? 
               serviceObject.AddComponent<FishingService>();
    }
    
    
    private void SetupInitialState()
    {
        // uiManager.SetupUI();
        // fishingAnimator.InitializeVisuals();
        // visualEffects.InitializeLineRenderer();
        
        CurrentState = FishingState.Ready;
        // uiManager.UpdateButtonStates();
        // uiManager.UpdateStatusText("ready");
    }





        private void CreatePlayer()
    {
        currentPlayer = new Player 
        { 
            Id = 1, 
            Name = "Рибалка",
            Strength = 10f, // Збільшено для легшого улову
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


    
}