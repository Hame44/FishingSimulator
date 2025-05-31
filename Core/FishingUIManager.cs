using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class FishingUIManager
{
    private FishingController controller;
    private bool isPulling = false;
    private Coroutine pullingCoroutine;
    
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
        
        // Налаштовуємо кнопку для безперервного тягнення
        SetupReleaseButton();
        
        UpdateButtonStates();
        SetupProgressBar();
    }
    
    private void SetupReleaseButton()
    {
        if (controller.releaseButton != null)
        {
            // Додаємо EventTrigger для обробки натискання та відпускання
            EventTrigger trigger = controller.releaseButton.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = controller.releaseButton.gameObject.AddComponent<EventTrigger>();
            
            // Коли натискаємо кнопку
            EventTrigger.Entry pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((data) => StartContinuousPulling());
            trigger.triggers.Add(pointerDown);
            
            // Коли відпускаємо кнопку
            EventTrigger.Entry pointerUp = new EventTrigger.Entry();
            pointerUp.eventID = EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((data) => StopContinuousPulling());
            trigger.triggers.Add(pointerUp);
            
            // Коли курсор виходить за межі кнопки
            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => StopContinuousPulling());
            trigger.triggers.Add(pointerExit);
        }
    }
    
    private void HandleHookPullClick()
    {
        var session = controller.FishingService?.GetCurrentSession();
        
        if (session?.State == FishingState.Fighting)
        {
            // В стані боротьби - один раз тягнемо рибу
            controller.gameLogic.PullFish();
        }
        else
        {
            // В інших станах - підсікаємо
            controller.HookOrPull();
        }
    }

    private void StartContinuousPulling()
    {
        var session = controller.FishingService?.GetCurrentSession();
        
        // Перевіряємо чи можемо тягнути рибу
        if (session?.State != FishingState.Fighting || isPulling)
            return;
            
        Debug.Log("Continuous pulling started");
        isPulling = true;
        
        if (pullingCoroutine != null)
        {
            controller.StopCoroutine(pullingCoroutine);
        }
        
        pullingCoroutine = controller.StartCoroutine(ContinuousPullCoroutine());
    }
    
    private void StopContinuousPulling()
    {
        if (!isPulling) return;
        
        Debug.Log("Continuous pulling stopped");
        isPulling = false;
        
        if (pullingCoroutine != null)
        {
            controller.StopCoroutine(pullingCoroutine);
            pullingCoroutine = null;
        }
    }
    
    private System.Collections.IEnumerator ContinuousPullCoroutine()
    {
        while (isPulling)
        {
            var session = controller.FishingService?.GetCurrentSession();
            
            // Перевіряємо чи досі можемо тягнути
            if (session?.State != FishingState.Fighting)
            {
                break;
            }
            
            Debug.Log("Pulling fish...");
            
            // Тягнемо рибу з більшою силою (можна викликати кілька разів за ітерацію)
            controller.gameLogic.PullFish();
            controller.gameLogic.PullFish(); // Додатковий виклик для швидшого тягнення
            
            // Зменшена затримка для швидшого тягнення
            yield return new WaitForSeconds(0.05f);
        }
        
        isPulling = false;
        pullingCoroutine = null;
    }
    
    public System.Collections.IEnumerator PullFishCoroutine()
    {
        Debug.Log("Single pull fish action");
        controller.gameLogic.PullFish();
        yield return null;
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
            bool canRelease = state == FishingState.Fighting;
            controller.releaseButton.interactable = canRelease;
            
            var buttonText = controller.releaseButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = canRelease ? "Утримувати для тягнення" : "Відпустити";
            }
        }
    }
    
    private string GetHookPullButtonText(FishingState? state)
    {
        return state switch
        {
            FishingState.Biting => "Підсікти!",
            FishingState.Hooked => "Тягнути",
            FishingState.Fighting => "Тягнути (один раз)",
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
            FishingState.Fighting => "Утримуйте кнопку 'Утримувати' щоб підтягнути рибу",
            _ => "Очікування..."
        };
    }
    
    public void UpdateTimer()
    {
        if (controller.timerText != null && controller.IsReeling)
        {
            string pullingStatus = isPulling ? " (Тягну!)" : "";
            controller.timerText.text = $"Час боротьби: {controller.FightTimer:F1}с / {controller.maxFightTime:F0}с{pullingStatus}";
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
    
    public void OnDestroy()
    {
        // Зупиняємо тягнення при знищенні об'єкта
        StopContinuousPulling();
    }
}