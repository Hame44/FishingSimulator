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
        ["hooked"] = "Риба на гачку! Натисніть 'Тягнути'",
        ["fighting"] = "Тягніть рибу!",
        ["caught"] = "Риба піймана!",
        ["escaped"] = "Риба втекла...",
        ["ready"] = "Готовий до нового закидання!"
    };
    
    public FishingUIManager(FishingController controller)
    {
        this.controller = controller;
    }
    
    public void SetupUI()
    {
        controller.castButton?.onClick.AddListener(controller.CastLine);
        controller.hookPullButton?.onClick.AddListener(HandleHookPullClick);
        controller.releaseButton?.onClick.AddListener(StartContinuousPulling);
        
        UpdateButtonStates();
        SetupProgressBar();
    }
    
    private void HandleHookPullClick()
    {
        var session = controller.FishingService?.GetCurrentSession();
        
        if (session?.State == FishingState.Fighting)
        {
            // В стані боротьби - тягнемо рибу
            controller.StartCoroutine(PullFishCoroutine());
        }
        else
        {
            // В інших станах - підсікаємо
            controller.HookOrPull();
        }
    }

    private void StartContinuousPulling()
    {
        // if (controller.gameObject.activeInHierarchy)
        // {
            Debug.Log("Continuous pulling started");
            while (Input.GetMouseButton(0)) 
            {
            controller.StartCoroutine(PullFishCoroutine());
            }

        // }
    }
    
    public System.Collections.IEnumerator PullFishCoroutine()
    {
        // Тягнемо рибу поки натиснута кнопка
        // while (Input.GetMouseButton(0) && controller.IsReeling)
        Debug.Log("PullFishCoroutine started");
        // while (Input.GetMouseButton(0))
        // {
            Debug.Log("Pulling fish...");
            controller.gameLogic.PullFish();
            yield return null;
        // }
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
        
        UpdateCastButton();
        UpdateHookPullButton(session?.State);
        UpdateReleaseButton(session?.State);
    }
    
    private void UpdateCastButton()
    {
        if (controller.castButton != null)
        {
            controller.castButton.interactable = !controller.IsFloatCast && !IsProcessingAction();
        }
    }
    
    private void UpdateHookPullButton(FishingState? state)
    {
        if (controller.hookPullButton != null)
        {
            controller.hookPullButton.interactable = controller.IsFloatCast && !IsProcessingAction();
            
            var buttonText = controller.hookPullButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = GetHookPullButtonText(state);
            }
        }
    }
    
    private void UpdateReleaseButton(FishingState? state)
    {
        if (controller.releaseButton != null)
        {
            bool canRelease = controller.IsFloatCast;
            controller.releaseButton.interactable = canRelease;
        }
    }
    
    private string GetHookPullButtonText(FishingState? state)
    {
        return state switch
        {
            FishingState.Biting => "Підсікти!",
            FishingState.Hooked => "Тягнути",
            FishingState.Fighting => "Тягнути",
            _ => "Підсікти"
        };
    }
    
    public void UpdateUI()
    {
        UpdatePlayerStats();
        UpdateInstructions();
        UpdateTimer();
        UpdateProgressBar();
    }
    
    public void UpdatePlayerStats()
    {
        if (controller.CurrentPlayer != null && controller.playerStatsText != null)
        {
            controller.playerStatsText.text = 
                $"Гравець: {controller.CurrentPlayer.Name}\n" +
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
            FishingState.Hooked => "Натисніть 'Тягнути' щоб почати боротьбу",
            FishingState.Fighting => "Утримуйте 'Тягнути' щоб підтягнути рибу",
            _ => "Очікування..."
        };
    }
    
    public void UpdateTimer()
    {
        if (controller.timerText != null && controller.IsReeling)
        {
            controller.timerText.text = $"Час боротьби: {controller.FightTimer:F1}с / {controller.maxFightTime:F0}с";
        }
        else if (controller.timerText != null)
        {
            controller.timerText.text = "";
        }
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
    
    public void ShowProgressBar()
    {
        if (controller.progressBar != null)
        {
            controller.progressBar.gameObject.SetActive(true);
        }
    }
    
    public void HideProgressBar()
    {
        if (controller.progressBar != null)
        {
            controller.progressBar.gameObject.SetActive(false);
        }
    }
    
    public void UpdateStatusText(string messageKey)
    {
        if (controller.statusText == null) return;
        
        string message = statusMessages.ContainsKey(messageKey) ? 
                        statusMessages[messageKey] : messageKey;
        
        controller.statusText.text = message;
    }
    
    private bool IsProcessingAction()
    {
        return controller.FightCoroutine != null;
    }
}
