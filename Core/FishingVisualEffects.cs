using UnityEngine;

public class FishingVisualEffects
{
    private FishingController controller;
    
    public FishingVisualEffects(FishingController controller)
    {
        this.controller = controller;
    }
    
    public void InitializeLineRenderer()
    {
        if (controller.fishingLine == null) return;
        
        SetupLineProperties();
        SetupLineMaterial();
        SetupLineGradient();
        HideLine();
    }
    
    private void SetupLineProperties()
    {
        controller.fishingLine.positionCount = 2;
        controller.fishingLine.startWidth = controller.lineWidth;
        controller.fishingLine.endWidth = controller.lineWidth;
        controller.fishingLine.sortingLayerName = "Default";
        controller.fishingLine.sortingOrder = 10;
        controller.fishingLine.useWorldSpace = true;
    }
    
    private void SetupLineMaterial()
    {
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
    }
    
    private void SetupLineGradient()
    {
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
    
    private void HideLine()
    {
        controller.fishingLine.enabled = false;
    }
    
    public void UpdateVisualEffects()
    {
        UpdateFishingLine();
        UpdateLineColor();
    }
    
    public void UpdateFishingLine()
    {
        if (!ShouldShowLine()) return;
        
        ShowLine();
        UpdateLinePositions();
    }
    
    private bool ShouldShowLine()
    {
        return controller.fishingLine != null && 
               controller.floatObject != null && 
               controller.rodTip != null && 
               controller.IsFloatCast;
    }
    
    private void ShowLine()
    {
        controller.fishingLine.enabled = true;
    }
    
    private void UpdateLinePositions()
    {
        controller.fishingLine.SetPosition(0, controller.rodTip.position);
        controller.fishingLine.SetPosition(1, controller.floatObject.transform.position);
    }
    
    public void UpdateLineColor()
    {
        if (controller.fishingLine?.material == null) return;
        
        if (controller.IsReeling && controller.TensionLevel > 0)
        {
            Color lineColor = controller.lineColorGradient.Evaluate(controller.TensionLevel);
            controller.fishingLine.material.color = lineColor;
        }
        else
        {
            controller.fishingLine.material.color = controller.normalLineColor;
        }
    }
    
    public void HideFishingLine()
    {
        if (controller.fishingLine != null)
        {
            controller.fishingLine.enabled = false;
            controller.fishingLine.material.color = controller.normalLineColor;
        }
    }
    
    public void PlayBiteEffect()
    {
        if (controller.biteEffect != null && controller.floatObject != null)
        {
            controller.biteEffect.transform.position = controller.floatObject.transform.position;
            controller.biteEffect.Play();
        }
    }
    
    public void PlaySplashEffect()
    {
        if (controller.splashEffect != null && controller.floatObject != null)
        {
            controller.splashEffect.transform.position = controller.floatObject.transform.position;
            controller.splashEffect.Play();
        }
    }
}