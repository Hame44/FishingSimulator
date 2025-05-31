using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FishingUIManager
{
    private FishingController controller;
    
    private readonly Dictionary<string, string> statusMessages = new Dictionary<string, string>
    {
        ["cast"] = "Закидання вудки...",
        ["waiting"] = "Очікування риби...",
        ["biting"] = "КЛЮЄ! Натисніть 'Підсікти'!",
        ["fighting"] = "Тягніть рибу!",
        ["caught"] = "Риба піймана!",
        ["escaped"] = "Риба втекла...",
        ["ready"] = "Готовий до нового закидання!"
    };
    
    public FishingUIManager(FishingController controller)
    {
        this.controller = controller;
    }
    
    public void SetupUI(FishingController fishingController)
    {
        controller.castButton?.onClick.AddListener(controller.CastLine);
        controller.hookPullButton?.onClick.AddListener(controller.HookOrPull);
        controller.releaseButton?.onClick.AddListener(() => HandlePlayerAction(FishingAction.Release));
        
        UpdateButtonStates();
        SetupProgressBar();
    }
    
    private void SetupProgressBar()
    {
        if (controller.progressBar != null)
        {
            controller.progressBar.gameObject.SetActive(false);
            controller.progressBar.minValue = 0f;
            controller.progressBar.maxValue = 1f;
        }
    }
    
    public void UpdateButtonStates()
    {
        var session = controller.FishingService?.GetCurrentSession();
        
        if (controller.castButton != null)
            controller.castButton.interactable = !controller.IsFloatCast && !IsProcessingAction();
            
        if (controller.hookPullButton != null)
        {
            controller.hookPullButton.interactable = controller.IsFloatCast && !IsProcessingAction();
            
            var buttonText = controller.hookPullButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = GetHookPullButtonText(session?.State);
            }
        }
        
        if (controller.releaseButton != null)
        {
            bool canRelease = session?.State == FishingState.Hooked || 
                             session?.State == FishingState.Fighting;
            controller.releaseButton.interactable = canRelease && !IsProcessingAction();
        }
    }
    
    private string GetHookPullButtonText(FishingState? state)
    {
        return state switch
        {
            FishingState.Biting => "Підсікти!",
            FishingState.Fighting => "Тягнути",
            _ => "Підсікти"
        };
    }
    
    public void UpdateUI()
    {
        UpdatePlayerStats();
        UpdateInstructions();
    }
    
    public void UpdatePlayerStats()
    {
        if (controller.CurrentPlayer != null && controller.playerStatsText != null)
        {
            controller.playerStatsText.text = $"Гравець: {controller.CurrentPlayer.Name}\n" +
                                 $"Сила: {controller.CurrentPlayer.Strength:F1}\n" +
                                 $"Досвід: {controller.CurrentPlayer.Experience}\n" +
                                 $"Вудка: {controller.CurrentPlayer.Equipment.RodDurability:F0}%\n" +
                                 $"Леска: {controller.CurrentPlayer.Equipment.LineDurability:F0}%";
        }
    }
    
    public void UpdateInstructions()
    {
        if (controller.instructionText == null) return;
        
        var session = controller.FishingService?.GetCurrentSession();
        string instruction = GetInstructionText(session?.State);
        controller.instructionText.text = instruction;
    }
    
    private string GetInstructionText(FishingState? state)
    {
        if (!controller.IsFloatCast)
            return "Натисніть 'Закинути' щоб почати риболовлю";
            
        return state switch
        {
            FishingState.Waiting => "Чекайте поклювки...",
            FishingState.Biting => "КЛЮЄ! Швидко натисніть 'Підсікти'!",
            FishingState.Fighting => "Тягніть рибу натискаючи 'Тягнути'",
            _ => "Очікування..."
        };
    }
    
    public void UpdateProgressBar()
    {
        if (controller.progressBar != null && controller.IsReeling)
        {
            float progress = 1f - (controller.CurrentFishDistance / controller.castDistance);
            controller.progressBar.value = progress;
            
            var fillImage = controller.progressBar.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = controller.lineColorGradient.Evaluate(controller.TensionLevel);
            }
        }
    }
    
    public void UpdateStatusText(string messageKey)
    {
        if (controller.statusText == null) return;
        
        string message = statusMessages.ContainsKey(messageKey) ? 
                        statusMessages[messageKey] : messageKey;
        
        controller.statusText.text = message;
        Debug.Log(message);
    }
    
    private void HandlePlayerAction(FishingAction action)
    {
        // Handle player actions here
        Debug.Log($"Player action: {action}");
    }
    
    private bool IsProcessingAction()
    {
        return controller.FightCoroutine != null;
    }
}