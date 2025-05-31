using UnityEngine;
using UnityEngine.UI;

public class FishingController : MonoBehaviour
{
    [Header("UI References")]
    public Button startFishingButton;
    public Button hookButton;
    public Button pullButton;
    public Button releaseButton;
    public Text statusText;
    public Text playerStatsText;
    
    private FishingService fishingService;
    private Player currentPlayer;
    
    void Start()
    {
        InitializeServices();
        SetupUI();
        CreatePlayer();
    }
    
    private void InitializeServices()
    {
        // Створюємо GameObject для FishingService
        GameObject serviceObject = new GameObject("FishingService");
        fishingService = serviceObject.AddComponent<FishingService>();
    }
    
    private void SetupUI()
    {
        if (startFishingButton != null)
            startFishingButton.onClick.AddListener(StartFishing);
            
        if (hookButton != null)
            hookButton.onClick.AddListener(() => HandlePlayerAction(FishingAction.Hook));
            
        if (pullButton != null)
            pullButton.onClick.AddListener(() => HandlePlayerAction(FishingAction.Pull));
            
        if (releaseButton != null)
            releaseButton.onClick.AddListener(() => HandlePlayerAction(FishingAction.Release));
    }
    
    private void CreatePlayer()
    {
        currentPlayer = new Player 
        { 
            Id = 1, 
            Name = "Test Player",
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
    
    public void StartFishing()
    {
        if (fishingService != null && currentPlayer != null)
        {
            fishingService.StartFishing(currentPlayer);
            UpdateStatusText("Fishing started! Waiting for fish...");
        }
    }
    
    public void StopFishing()
    {
        if (fishingService != null)
        {
            fishingService.StopFishing();
            UpdateStatusText("Fishing stopped.");
        }
    }
    
    public void HandlePlayerAction(FishingAction action)
    {
        if (fishingService != null)
        {
            fishingService.HandlePlayerAction(action);
            
            // Оновлюємо статус в залежності від дії
            switch (action)
            {
                case FishingAction.Hook:
                    UpdateStatusText("Trying to hook the fish!");
                    break;
                case FishingAction.Pull:
                    UpdateStatusText("Fighting the fish!");
                    break;
                case FishingAction.Release:
                    UpdateStatusText("Released the fish.");
                    break;
            }
        }
    }
    
    void Update()
    {
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        if (currentPlayer != null && playerStatsText != null)
        {
            playerStatsText.text = $"Player: {currentPlayer.Name}\n" +
                                 $"Strength: {currentPlayer.Strength:F1}\n" +
                                 $"Experience: {currentPlayer.Experience}\n" +
                                 $"Rod: {currentPlayer.Equipment.RodDurability:F0}%\n" +
                                 $"Line: {currentPlayer.Equipment.LineDurability:F0}%";
        }
        
        // Оновлюємо статус сесії
        if (fishingService != null)
        {
            var session = fishingService.GetCurrentSession();
            if (session != null && statusText != null)
            {
                UpdateStatusBasedOnState(session.State);
            }
        }
    }
    
    private void UpdateStatusBasedOnState(FishingState state)
    {
        switch (state)
        {
            case FishingState.Waiting:
                UpdateStatusText("Waiting for fish...");
                break;
            case FishingState.Biting:
                UpdateStatusText("FISH IS BITING! Press Hook!");
                break;
            case FishingState.Hooked:
                UpdateStatusText("Fish hooked!");
                break;
            case FishingState.Fighting:
                UpdateStatusText("Fighting fish! Keep pulling!");
                break;
            case FishingState.Caught:
                UpdateStatusText("Fish caught!");
                break;
            case FishingState.Escaped:
                UpdateStatusText("Fish escaped...");
                break;
        }
    }
    
    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log(message);
    }
}