using UnityEngine;
using System.Collections;

public partial class FishingLogic
{
    
        private IEnumerator HandleCompletion(FishingState completionState)
    {
        StopFightSequence();
        
        string message = completionState == FishingState.Caught ? "🏆 Риба піймана!" : "💨 Риба втекла!";
        Debug.Log(message);
        
        yield return controller.MediumDelay;
        
        // ЗМІНЕНО: Використовуємо нову анімацію повернення
        yield return controller.StartCoroutine(controller.FloatAnimation.ReturnToShore());
        yield return controller.StartCoroutine(ResetAfterCompletion());
    }
    
    private IEnumerator ResetAfterCompletion()
    {
        // Поплавок вже схований в ReturnToShore()
        
        // Скидаємо всі стани
        ResetAllStates();
        
        yield return new WaitForSeconds(1f);
        
        Debug.Log("🎣 Готовий до нової риболовлі!");
    }
    
    
    private void ResetAllStates()
    {
        controller.SetReeling(false);
        controller.SetHooked(false);
        controller.SetFishBiting(false);
        controller.SetFloatCast(false);
        controller.SetCurrentFishDistance(0f);
        // controller.SetFightTimer(0f);
        // controller.SetTensionLevel(0f);
        
        // Зупиняємо всі корутіни
        if (controller.FloatBobCoroutine != null)
        {
            controller.StopCoroutine(controller.FloatBobCoroutine);
            controller.SetFloatBobCoroutine(null);
        }
    }
}