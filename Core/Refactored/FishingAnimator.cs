using UnityEngine;
using System.Collections;

public class FishingAnimator
{
    private FishingController controller;
    private Vector3 floatStartPosition;
    private Vector3 floatTargetPosition;
    private Vector3 floatBasePosition;
    
    public FishingAnimator(FishingController controller)
    {
        this.controller = controller;
    }
    
    public void InitializeVisuals()
    {
        SetupFloatStartPosition();
        HideFloat();
    }
    
    private void SetupFloatStartPosition()
    {
        if (controller.floatObject != null)
        {
            floatStartPosition = controller.shore != null ? 
                controller.shore.position : controller.transform.position;
            controller.floatObject.transform.position = floatStartPosition;
        }
    }
    
    private void HideFloat()
    {
        if (controller.floatObject != null)
        {
            controller.floatObject.SetActive(false);
        }
    }
    
    public IEnumerator CastAnimation()
    {
        if (controller.floatObject == null || controller.waterSurface == null) yield break;
        
        ShowFloat();
        CalculateCastTarget();
        
        yield return controller.StartCoroutine(AnimateCastArc());
        
        SetFloatAtTarget();
        PlaySplashEffect();
    }
    
    private void ShowFloat()
    {
        controller.floatObject.SetActive(true);
        controller.SetFloatCast(true);
    }
    
    private void CalculateCastTarget()
    {
        Vector3 castPosition = controller.waterSurface.position + 
                              Vector3.right * controller.castDistance;
        floatTargetPosition = castPosition;
        floatBasePosition = castPosition;
    }
    
    private IEnumerator AnimateCastArc()
    {
        float castTime = 1.5f;
        float elapsed = 0f;
        
        while (elapsed < castTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / castTime;
            float curveValue = controller.castCurve.Evaluate(progress);
            
            Vector3 currentPos = Vector3.Lerp(floatStartPosition, floatTargetPosition, curveValue);
            currentPos.y += Mathf.Sin(curveValue * Mathf.PI) * 2f;
            
            controller.floatObject.transform.position = currentPos;
            yield return null;
        }
    }
    
    private void SetFloatAtTarget()
    {
        controller.floatObject.transform.position = floatTargetPosition;
        floatBasePosition = floatTargetPosition;
    }
    
    private void PlaySplashEffect()
    {
        if (controller.splashEffect != null)
        {
            controller.splashEffect.transform.position = floatTargetPosition;
            controller.splashEffect.Play();
        }
    }
    
    public IEnumerator FloatBobbing()
    {
        while (controller.IsFloatCast && controller.floatObject != null && !controller.IsReeling)
        {
            if (controller.IsFishBiting)
            {
                yield return controller.StartCoroutine(BiteAnimation());
            }
            else
            {
                AnimateNormalBobbing();
            }
            
            yield return controller.ShortDelay;
        }
    }
    
    private void AnimateNormalBobbing()
    {
        float time = Time.time * controller.floatBobSpeed;
        float bobOffset = -Mathf.Abs(Mathf.Sin(time)) * controller.floatBobIntensity * 0.3f;
        
        Vector3 newPos = floatBasePosition;
        newPos.y += bobOffset;
        controller.floatObject.transform.position = newPos;
    }
    
    private IEnumerator BiteAnimation()
    {
        float biteTime = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < biteTime && controller.IsFishBiting && !controller.IsHooked)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / biteTime;
            
            float sideMovement = Mathf.Sin(progress * Mathf.PI * 10) * 0.1f;
            float downMovement = -Mathf.Abs(Mathf.Sin(progress * Mathf.PI * 8)) * controller.biteBobIntensity;
            
            Vector3 newPos = floatBasePosition;
            newPos.x += sideMovement;
            newPos.y += downMovement;
            
            controller.floatObject.transform.position = newPos;
            
            yield return null;
        }
        
        if (!controller.IsHooked && controller.floatObject != null)
        {
            controller.floatObject.transform.position = floatBasePosition;
        }
    }
    
    public void AnimateFighting(float distanceRatio, float tensionLevel)
    {
                if (controller.floatObject == null || controller.shore == null) return;
        
        // Розраховуємо цільову позицію поплавка
        Vector3 shorePos = controller.shore.position;
        Vector3 targetPos = Vector3.Lerp(shorePos, floatTargetPosition, distanceRatio);
        
        // Додаємо ефект боротьби
        Vector3 fightOffset = new Vector3(
            UnityEngine.Random.Range(-0.1f, 0.1f) * tensionLevel,
            UnityEngine.Random.Range(-0.05f, 0.05f) * tensionLevel,
            0
        );
        
        // Плавно переміщуємо поплавок
        Vector3 currentPos = controller.floatObject.transform.position;
        Vector3 newPos = Vector3.Lerp(currentPos, targetPos + fightOffset, Time.deltaTime * 2f);
        
        controller.floatObject.transform.position = newPos;
        floatBasePosition = targetPos;
        
        // Логування для дебагу
        if (Time.time % 1f < 0.1f)
        {
            Debug.Log($"🎯 Поплавок: позиція={newPos:F2}, дистанція={distanceRatio:F2}, напруга={tensionLevel:F2}");
        }
    }
    
    public void ResetFloat()
    {
        if (controller.floatObject != null)
        {
            controller.floatObject.transform.position = floatStartPosition;
            controller.floatObject.SetActive(false);
        }
        
        controller.SetFloatCast(false);
    }
    
    public Vector3 FloatStartPosition => floatStartPosition;
    public Vector3 FloatTargetPosition => floatTargetPosition;
    public Vector3 FloatBasePosition => floatBasePosition;
}