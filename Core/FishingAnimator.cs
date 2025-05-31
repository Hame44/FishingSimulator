using UnityEngine;
using System.Collections;

public class FishingAnimator
{
    private FishingController controller;
    
    public FishingAnimator(FishingController controller)
    {
        this.controller = controller;
    }
    
    public void InitializeVisuals()
    {
        // Налаштування поплавка
        if (controller.floatObject != null)
        {
            controller.SetFloatStartPosition(controller.shore != null ? controller.shore.position : controller.transform.position);
            controller.floatObject.transform.position = controller.FloatStartPosition;
            controller.floatObject.SetActive(false);
        }
        
        SetupFishingLine();
        controller.SetCurrentFishDistance(controller.castDistance);
    }
    
    private void SetupFishingLine()
    {
        if (controller.fishingLine != null)
        {
            controller.fishingLine.positionCount = 2;
            controller.fishingLine.enabled = false;
            controller.fishingLine.startWidth = controller.lineWidth;
            controller.fishingLine.endWidth = controller.lineWidth;
            
            if (controller.fishingLine.material == null)
            {
                Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
                lineMaterial.color = controller.normalLineColor;
                controller.fishingLine.material = lineMaterial;
            }
            else
            {
                controller.fishingLine.material.color = controller.normalLineColor;
            }

            controller.fishingLine.sortingLayerName = "Default";
            controller.fishingLine.sortingOrder = 10;
            controller.fishingLine.useWorldSpace = true;
            
            if (controller.lineColorGradient.colorKeys.Length == 0)
            {
                GradientColorKey[] colorKeys = new GradientColorKey[2];
                colorKeys[0] = new GradientColorKey(controller.normalLineColor, 0.0f);
                colorKeys[1] = new GradientColorKey(controller.tensionLineColor, 1.0f);
                
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
                alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);
                
                controller.lineColorGradient.SetKeys(colorKeys, alphaKeys);
            }
        }
    }
    
    public IEnumerator CastAnimation()
    {
        if (controller.floatObject == null || controller.waterSurface == null) yield break;
        
        controller.SetFloatCast(true);
        controller.floatObject.SetActive(true);
        
        Vector3 castPosition = controller.waterSurface.position + Vector3.right * controller.castDistance;
        controller.SetFloatTargetPosition(castPosition);
        
        float castTime = 1.5f;
        float elapsed = 0f;
        
        while (elapsed < castTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / castTime;
            float curveValue = controller.castCurve.Evaluate(progress);
            
            Vector3 currentPos = Vector3.Lerp(controller.FloatStartPosition, castPosition, curveValue);
            currentPos.y += Mathf.Sin(curveValue * Mathf.PI) * 2f;
            
            controller.floatObject.transform.position = currentPos;
            UpdateFishingLine();
            
            yield return null;
        }
        
        controller.floatObject.transform.position = castPosition;
    }
    
    public IEnumerator FloatBobbing()
    {
        Vector3 basePosition = controller.floatObject.transform.position;
        float originalY = basePosition.y;
        
        while (controller.IsFloatCast && controller.floatObject != null)
        {
            if (controller.IsFishBiting)
            {
                yield return controller.StartCoroutine(BiteAnimation(basePosition));
            }
            else
            {
                float bobOffset = Mathf.Sin(Time.time * controller.floatBobSpeed) * controller.floatBobIntensity;
                Vector3 newPos = basePosition;
                newPos.y = originalY + bobOffset;
                controller.floatObject.transform.position = newPos;
            }
            
            UpdateFishingLine();
            yield return controller.ShortDelay;
        }
    }

    public IEnumerator BiteAnimation(Vector3 basePosition)
    {
        float biteTime = 0.5f;
        float elapsed = 0f;
    
        while (elapsed < biteTime && controller.IsFishBiting)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / biteTime;
        
            float sideMovement = Mathf.Sin(progress * Mathf.PI * 4) * 0.3f;
            float downMovement = -Mathf.Sin(progress * Mathf.PI * 2) * controller.biteBobIntensity;
        
            Vector3 newPos = basePosition;
            newPos.x += sideMovement;
            newPos.y += downMovement;
        
            controller.floatObject.transform.position = newPos;
            UpdateFishingLine();
        
            yield return null;
        }
    }
    
    public void UpdateVisualEffects()
    {
        UpdateFishingLine();
        UpdateLineColor();
    }
    
    public void UpdateFishingLine()
    {
        if (controller.fishingLine != null && controller.floatObject != null && controller.rodTip != null)
        {
            controller.fishingLine.enabled = controller.IsFloatCast;
            if (controller.IsFloatCast)
            {
                controller.fishingLine.SetPosition(0, controller.rodTip.position);
                controller.fishingLine.SetPosition(1, controller.floatObject.transform.position);
            }
        }
    }
    
    private void UpdateLineColor()
    {
        if (controller.fishingLine != null && controller.IsReeling)
        {
            Color lineColor = controller.lineColorGradient.Evaluate(controller.TensionLevel);
            controller.fishingLine.material.color = lineColor;
        }
    }
}