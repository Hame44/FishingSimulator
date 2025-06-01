using UnityEngine;
using System.Collections;

public partial class FishingLogic
{
    
    private IEnumerator ReelInEmptyLine()
    {
        controller.UIManager.UpdateStatusText("Витягуємо порожню вудку...");
        controller.SetReeling(true);
        
        float reelTime = 2f;
        float elapsed = 0f;
        Vector3 startPos = controller.floatObject.transform.position;
        
        while (elapsed < reelTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / reelTime;
            
            Vector3 currentPos = Vector3.Lerp(startPos, controller.shore.position, progress);
            controller.floatObject.transform.position = currentPos;
            
            yield return null;
        }
        
        // Скидаємо стан
        ResetLineState();
        
        controller.UIManager.UpdateStatusText("ready");
        controller.UIManager.UpdateButtonStates();
        
        Debug.Log("🎣 Порожня вудка витягнута");
    }
    
    private void ResetLineState()
    {
        controller.Animator.ResetFloat();
        controller.VisualEffects.HideFishingLine();
        controller.SetReeling(false);
        controller.SetFloatCast(false);
    }
    
}