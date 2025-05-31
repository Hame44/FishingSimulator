using UnityEngine;
using System.Collections;

public class FishingFloatAnimation
{
    private FishingController controller;
    private Vector3 waterLevelPosition;
    
    public FishingFloatAnimation(FishingController controller)
    {
        this.controller = controller;
    }
    
    public void InitializeFloat()
    {
        if (controller.floatObject != null && controller.waterSurface != null)
        {
            waterLevelPosition = new Vector3(
                controller.FloatCastPosition.x,
                controller.waterSurface.position.y,
                controller.FloatCastPosition.z
            );
        }
    }
    
    public IEnumerator StartNormalBobbing()
    {
        while (controller.IsFloatCast && !controller.IsFishBiting)
        {
            yield return BobFloat(controller.floatBobIntensity, controller.floatBobSpeed);
        }
    }
    
    public IEnumerator StartBiteBobbing()
    {
        float biteStartTime = Time.time;
        float biteDuration = 3f; // Тривалість клювання
        
        while (controller.IsFishBiting && (Time.time - biteStartTime) < biteDuration)
        {
            yield return BobFloat(controller.biteBobIntensity, controller.floatBobSpeed * 2f);
        }
    }
    
    private IEnumerator BobFloat(float intensity, float speed)
    {
        if (controller.floatObject == null) yield break;
        
        float time = Time.time * speed;
        
        // Використовуємо синус для плавного руху вниз-вгору
        // Додаємо offset щоб поплавок більше часу проводив під водою
        float bobOffset = (Mathf.Sin(time) - 0.3f) * intensity;
        
        // Обмежуємо рух тільки вниз від рівня води
        bobOffset = Mathf.Min(bobOffset, 0f);
        
        Vector3 newPosition = waterLevelPosition;
        newPosition.y += bobOffset - controller.floatSubmergeDepth;
        
        controller.floatObject.transform.position = newPosition;
        
        yield return null;
    }
    
    public IEnumerator AnimateFloatCast(Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            float curveValue = controller.castCurve.Evaluate(progress);
            
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, curveValue);
            
            // Додаємо арку для реалістичного польоту
            float arc = Mathf.Sin(progress * Mathf.PI) * 2f;
            currentPos.y += arc;
            
            controller.floatObject.transform.position = currentPos;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        controller.floatObject.transform.position = endPos;
        controller.SetFloatCastPosition(endPos);
        
        // Ініціалізуємо позицію рівня води після закидання
        InitializeFloat();
        
        // Показуємо ефект бризок
        controller.VisualEffects.PlaySplashEffect();
    }
    
    public IEnumerator AnimateFloatReturn(Vector3 targetPos, float duration)
    {
        Vector3 startPos = controller.floatObject.transform.position;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
            
            controller.floatObject.transform.position = currentPos;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        controller.floatObject.transform.position = targetPos;
    }
    
    public void StopFloatAnimation()
    {
        if (controller.FloatBobCoroutine != null)
        {
            controller.StopCoroutine(controller.FloatBobCoroutine);
            controller.SetFloatBobCoroutine(null);
        }
    }
    
    public void ResetFloatPosition()
    {
        if (controller.floatObject != null)
        {
            controller.floatObject.transform.position = controller.OriginalFloatPosition;
        }
    }
}