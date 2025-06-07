using UnityEngine;
using System.Collections;

public class FloatAnimation
{
    private FishingController controller;
    private Vector3 floatStartPosition;
    private Vector3 floatTargetPosition;
    private Vector3 floatBasePosition;
    
    private PolygonCollider2D waterCollider;
    
    public FloatAnimation(FishingController controller)
    {
        this.controller = controller;
        waterCollider = GameObject.FindObjectOfType<PolygonCollider2D>();
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

    public IEnumerator ReturnToShore()
{
    if (controller.floatObject == null || controller.shore == null) yield break;
    
    Vector3 startPos = controller.floatObject.transform.position;
    Vector3 endPos = controller.shore.position;
    
    float returnTime = 2f;
    float elapsed = 0f;
    
    Debug.Log("🔄 Повертаємо поплавок до берега...");
    
    while (elapsed < returnTime)
    {
        elapsed += Time.deltaTime;
        float progress = elapsed / returnTime;
        
        Vector3 currentPos = Vector3.Lerp(startPos, endPos, progress);
        controller.floatObject.transform.position = currentPos;
        
        yield return null;
    }
    
    // Ховаємо поплавок після повернення
    HideFloat();
    Debug.Log("✅ Поплавок повернуто до берега");
}


    public void ShowFloatAtPosition(Vector3 position)
    {
        if (controller.floatObject != null)
        {
            controller.floatObject.transform.position = position;
            floatBasePosition = position; // Встановлюємо базову позицію
            controller.floatObject.SetActive(true);
            controller.SetFloatCast(true);
            
            Debug.Log($"🎯 Поплавок показано в позиції: {position}");
        }
    }

    public IEnumerator BiteBobbing(float biteSpeed, float biteDuration)
    {
        Debug.Log($"🎣 BiteBobbing почався: швидкість {biteSpeed}, тривалість {biteDuration:F1}с");
        
        if (controller.floatObject == null)
        {
            Debug.LogError("❌ FloatObject відсутній!");
            yield break;
        }
        
        yield return controller.StartCoroutine(BiteAnimation(biteSpeed, biteDuration));
        
        Debug.Log("✅ BiteBobbing завершено");
    }

    public void StartBobbing(float speed, float duration)
    {
        if (controller != null)
        {
            controller.StartCoroutine(BiteBobbing(speed, duration));
        }
    }
    
    public IEnumerator BaseBobbing()
    {
        Debug.Log("🌊 BaseBobbing почався");
        
        while (controller.IsFloatCast && 
               controller.floatObject != null && 
               !controller.IsReeling &&
               !controller.IsFishBiting)
        {
            float time = Time.time * controller.floatBobSpeed;
            float bobOffset = -Mathf.Abs(Mathf.Sin(time)) * controller.floatBobIntensity * 0.3f;
        
            Vector3 newPos = floatBasePosition;
            newPos.y += bobOffset;
            controller.floatObject.transform.position = newPos;

            yield return controller.ShortDelay;
        }
        
        Debug.Log("🛑 BaseBobbing зупинено");
    }

    public IEnumerator BiteAnimation(float biteSpeed, float biteDuration)
    {
        if (controller.floatObject == null) yield break;
        
        float elapsed = 0f;
        Vector3 startBitePosition = controller.floatObject.transform.position;
        
        Vector2 moveDirection = new Vector2(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized;
        
        float moveSpeed = biteSpeed * 0.3f;
        
        Debug.Log($"🎣 BiteAnimation: напрямок {moveDirection}");

        Vector3 currentPosition = startBitePosition;

        while (elapsed < biteDuration && 
               controller.IsFishBiting && 
               !controller.IsHooked &&
               controller.floatObject != null)
        {
            elapsed += Time.deltaTime;
            
            Vector2 moveOffset = moveDirection * moveSpeed * Time.deltaTime;
            Vector3 newPos = currentPosition;
            newPos.x += moveOffset.x;
            newPos.y += moveOffset.y;
            
            if (!IsPositionInWater(newPos))
            {
                Debug.Log($"🔄 Поплавок досяг межі води! Позиція: {newPos}");
                
                moveDirection = -moveDirection;
                
                newPos = currentPosition;
            }

            controller.floatObject.transform.position = newPos;
            currentPosition = newPos;

            yield return null;
        }
        
        floatBasePosition = currentPosition;
        Debug.Log($"✅ BiteAnimation завершено");
    }

    private bool IsPositionInWater(Vector3 position)
    {
        if (waterCollider == null) return true; // Якщо немає колайдера, дозволяємо рух
        
        return waterCollider.OverlapPoint(position);
    }


    public Vector3 FloatStartPosition => floatStartPosition;
    public Vector3 FloatTargetPosition => floatTargetPosition;
    public Vector3 FloatBasePosition => floatBasePosition;
}