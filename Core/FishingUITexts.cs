using UnityEngine;
using UnityEngine.UI;

public class FishingUITexts
{
    private FishingController controller;
    
    public FishingUITexts(FishingController controller)
    {
        this.controller = controller;
    }
    
    public void UpdateAllTexts()
    {
        UpdateStatusText();
        UpdatePlayerStatsText();
        UpdateInstructionText();
        UpdateTimerText();
        UpdateProgressBar();
    }
    
    public void UpdateStatusText()
    {
        if (controller.statusText == null) return;
        
        string statusMessage = GetStatusMessage();
        controller.statusText.text = statusMessage;
    }
    
    private string GetStatusMessage()
    {
        var session = controller.FishingService?.GetCurrentSession();
        if (session == null) return "Сервіс недоступний";
        
        return controller.CurrentState switch
        {
            FishingState.Ready => "Готовий до риболовлі",
            FishingState.Waiting => "Чекаємо рибу...",
            FishingState.Biting => "РИБА КЛЮЄ! Натисніть підсікти!",
            FishingState.Hooked => "Риба зачіплена! Натисніть тягнути!",
            FishingState.Fighting => controller.IsFighting ? "Боротьба з рибою..." : "Витягуємо рибу...",
            FishingState.Completed => GetCompletionMessage(session),
            _ => "Невідомий стан"
        };
    }
    
    private string GetCompletionMessage(FishingSession session)
    {
        if (session?.LastResult == null) return "Риболовля завершена";
        
        return session.LastResult switch
        {
            FishingResult.Success => $"Піймано {session.CurrentFish?.FishType}!",
            FishingResult.FishEscaped => "Риба втекла",
            FishingResult.LineBroken => "Порвалась леска",
            FishingResult.RodBroken => "Зламалась вудка",
            FishingResult.RodPulledAway => "Риба вирвала вудку",
            _ => "Риболовля завершена"
        };
    }
    
    public void UpdatePlayerStatsText()
    {
        if (controller.playerStatsText == null || controller.CurrentPlayer == null) return;
        
        var player = controller.CurrentPlayer;
        var equipment = player.Equipment;
        
        controller.playerStatsText.text = 
            $"Гравець: {player.Name}\n" +
            $"Сила: {player.Strength:F1}\n" +
            $"Досвід: {player.Experience}\n" +
            $"Вудка: {equipment.RodDurability:F0}%\n" +
            $"Леска: {equipment.LineDurability:F0}%\n" +
            $"Удача: {equipment.FishingLuck:F1}x";
    }
    
    public void UpdateInstructionText()
    {
        if (controller.instructionText == null) return;
        
        string instruction = GetCurrentInstruction();
        controller.instructionText.text = instruction;
    }
    
    private string GetCurrentInstruction()
    {
        return controller.CurrentState switch
        {
            FishingState.Ready => "Натисніть 'Закинути' для початку риболовлі",
            FishingState.Waiting => "Чекайте поклювки або натисніть 'Витягнути' для повернення",
            FishingState.Biting => "ШВИДКО! Натисніть 'Підсікти' поки риба клює!",
            FishingState.Hooked => "Риба зачіплена! Натисніть 'Тягнути' щоб витягти",
            FishingState.Fighting => "Утримуйте кнопку 'Тягнути' або відпустіть щоб зменшити напругу",
            FishingState.Completed => "Натисніть 'Закинути' для нової спроби",
            _ => ""
        };
    }
    
    public void UpdateTimerText()
    {
        if (controller.timerText == null) return;
        
        string timerInfo = GetTimerInfo();
        controller.timerText.text = timerInfo;
    }
    
    private string GetTimerInfo()
    {
        var session = controller.FishingService?.GetCurrentSession();
        if (session == null) return "";
        
        if (controller.CurrentState == FishingState.Fighting)
        {
            float remainingTime = controller.maxFightTime - controller.FightTimer;
            return $"Час бою: {remainingTime:F1}с";
        }
        else if (controller.CurrentState == FishingState.Waiting)
        {
            return $"Час очікування: {session.SessionTime:F0}с";
        }
        
        return "";
    }
    
    public void UpdateProgressBar()
    {
        if (controller.progressBar == null) return;
        
        if (controller.CurrentState == FishingState.Fighting)
        {
            // Прогрес бою з рибою
            float progress = controller.CurrentFishDistance / 10f; // Максимальна дистанція
            controller.progressBar.value = Mathf.Clamp01(1f - progress);
            controller.progressBar.gameObject.SetActive(true);
        }
        else if (controller.IsReeling && !controller.IsFighting)
        {
            // Прогрес витягування порожньої лінії
            Vector3 floatPos = controller.floatObject.transform.position;
            Vector3 shorePos = controller.shore.position;
            float totalDistance = Vector3.Distance(controller.FloatCastPosition, shorePos);
            float currentDistance = Vector3.Distance(floatPos, shorePos);
            float progress = 1f - (currentDistance / totalDistance);
            
            controller.progressBar.value = Mathf.Clamp01(progress);
            controller.progressBar.gameObject.SetActive(true);
        }
        else
        {
            controller.progressBar.gameObject.SetActive(false);
        }
    }
    
    public void ShowMessage(string message, float duration = 2f)
    {
        if (controller.statusText != null)
        {
            controller.statusText.text = message;
            // Можна додати корутину для автоматичного очищення повідомлення
        }
    }
}