using UnityEngine;
using System.Collections;

public class FloatAnimation
{
    private FishingController controller;
    private Vector3 floatStartPosition;
    private Vector3 floatTargetPosition;
    private Vector3 floatBasePosition;
    
    public FloatAnimation(FishingController controller)
    {
        this.controller = controller;
    }
    
    public void InitializeVisuals()
    {
        SetupFloatStartPosition();
        HideFloat();
    }
    
    public void SetupFloatStartPosition()
    {
        if (controller.floatObject != null)
        {
            floatStartPosition = controller.shore != null ? 
            controller.shore.position : controller.transform.position;
            controller.floatObject.transform.position = floatStartPosition;
        }
    }
    
    public void HideFloat()
    {
        if (controller.floatObject != null)
        {
            controller.floatObject.SetActive(false);
        }
    }
    
    public void ShowFloat()
    {
        controller.floatObject.SetActive(true);
        controller.SetFloatCast(true);
    }
    
    
    public IEnumerator BiteBobbing(float biteSpeed, float biteDuration)
    {
        while (controller.IsFloatCast && controller.floatObject != null && !controller.IsReeling)
        {
            yield return controller.StartCoroutine(BiteAnimation(biteSpeed, biteDuration)); 
            // yield return controller.ShortDelay;
        }
    }
    
    public IEnumerator BaseBobbing()
    {
        while (controller.IsFloatCast && controller.floatObject != null && !controller.IsReeling)
        {
            float time = Time.time * controller.floatBobSpeed;
            float bobOffset = -Mathf.Abs(Mathf.Sin(time)) * controller.floatBobIntensity * 0.3f;
        
            Vector3 newPos = floatBasePosition;
            newPos.y += bobOffset;
            controller.floatObject.transform.position = newPos;

            yield return controller.ShortDelay;
        }
    }


    public IEnumerator BiteAnimation(float biteSpeed, float biteDuration)
    {
        float elapsed = 0f;
        Vector3 floatBasePosition = controller.floatObject.transform.position;

        float directionX = (UnityEngine.Random.value > 0.5f) ? 1f : -1f;
        float directionY = (UnityEngine.Random.value > 0.5f) ? 1f : -1f;

        Vector2 moveDirection = new Vector2(directionX, directionY).normalized;

        while (elapsed < biteDuration && controller.IsFishBiting && !controller.IsHooked)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / biteDuration;

            Vector2 moveOffset = moveDirection * biteSpeed * elapsed;

            float bobbing = Mathf.Sin(progress * Mathf.PI * 10) * controller.biteBobIntensity;

            Vector3 newPos = floatBasePosition;
            newPos.x += moveOffset.x;
            newPos.y += moveOffset.y + bobbing;

            controller.floatObject.transform.position = newPos;

            yield return null;
        }
    }


    public IEnumerator CastAnimation()
    {
        if (controller.floatObject == null || controller.waterSurface == null) yield break;
        
        ShowFloat();
        CalculateCastTarget();
        
        yield return controller.StartCoroutine(AnimateCastArc());
        
        SetFloatAtTarget();
    }

    public void CalculateCastTarget()
    {
        Vector3 castPosition = controller.waterSurface.position + 
                              Vector3.right * controller.castDistance;
        floatTargetPosition = castPosition;
        floatBasePosition = castPosition;
    }
    
    public IEnumerator AnimateCastArc()
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

    public void SetFloatAtTarget()
    {
        controller.floatObject.transform.position = floatTargetPosition;
        floatBasePosition = floatTargetPosition;
    }

    public Vector3 FloatStartPosition => floatStartPosition;
    public Vector3 FloatTargetPosition => floatTargetPosition;
    public Vector3 FloatBasePosition => floatBasePosition;
}