using UnityEngine;

public class WaterClickHandler : MonoBehaviour
{
    [Header("Water Settings")]
    public PolygonCollider2D waterCollider;
    public float floatRadius = 3f; // Радіус поплавка для перевірки меж
    
    private FishingController fishingController;
    private Camera mainCamera;

    void Start()
    {
        fishingController = FindObjectOfType<FishingController>();
        mainCamera = Camera.main;
        
        if (waterCollider == null)
        {
            waterCollider = GetComponent<PolygonCollider2D>();
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // ЛКМ
        {
            HandleWaterClick();
        }
    }

    private void HandleWaterClick()
    {
        if (fishingController.IsFloatCast) return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; // 2D координата

        if (IsPointInWater(mouseWorldPos))
        {
            Vector3 validPosition = GetValidFloatPosition(mouseWorldPos);
            fishingController.CastToPosition(validPosition);
        }
    }

    private bool IsPointInWater(Vector3 point)
    {
        return waterCollider.OverlapPoint(point);
    }

    private Vector3 GetValidFloatPosition(Vector3 clickPosition)
    {
        Bounds waterBounds = waterCollider.bounds;
        
        float minX = waterBounds.min.x + floatRadius;
        float maxX = waterBounds.max.x - floatRadius;
        float minY = waterBounds.min.y + floatRadius;
        float maxY = waterBounds.max.y - floatRadius;

        Vector3 validPosition = new Vector3(
            Mathf.Clamp(clickPosition.x, minX, maxX),
            Mathf.Clamp(clickPosition.y, minY, maxY),
            0f
        );

        Debug.Log($"🎯 Закидання в позицію: {validPosition}");
        return validPosition;
    }
}