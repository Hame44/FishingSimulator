using UnityEngine;

public class FishingBobberSpawner : MonoBehaviour
{
    public GameObject bobberPrefab;
    public LayerMask waterLayer;
    public Transform player;
    public float edgeMargin = 0.2f; // мінімальна відстань до краю води

    private GameObject currentBobber;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && currentBobber == null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, waterLayer);

            if (hit.collider != null)
            {
                float bobberRadius = bobberPrefab.GetComponent<FishingBobber>().bobberRadius;
                Vector2 spawnPosition = hit.point;
                Vector2 safePosition = FindSafeSpawnPosition(spawnPosition, hit.collider, bobberRadius + edgeMargin);

                if (safePosition != Vector2.positiveInfinity)
                {
                    currentBobber = Instantiate(bobberPrefab, safePosition, Quaternion.identity);
                    FishingBobber bobberComponent = currentBobber.GetComponent<FishingBobber>();
                    bobberComponent.Initialize(player, this);
                }
            }
        }
    }

    // Пошук найближчої безпечної точки всередині води
    private Vector2 FindSafeSpawnPosition(Vector2 start, Collider2D waterCollider, float radius)
{
    if (IsFullyInsideWater(start, radius))
        return start;

    Vector2 center = waterCollider.bounds.center;
    Vector2 dir = (center - start).normalized;
    float maxStep = Vector2.Distance(start, center);
    int steps = Mathf.CeilToInt(maxStep / 0.05f);
    Vector2 pos = start;

    for (int i = 0; i < steps; i++)
    {
        pos += dir * 0.05f;
        if (IsFullyInsideWater(pos, radius))
        {
            Debug.Log($"Safe spawn found at {pos}");
            return pos;
        }
    }
    Debug.Log("No safe spawn found");
    return Vector2.positiveInfinity;
}

    private bool IsPositionSafeFromWaterEdge(Vector2 position, float radius)
    {
        Collider2D[] waterColliders = Physics2D.OverlapCircleAll(position, radius, waterLayer);
        if (waterColliders.Length == 0)
            return false;

        Collider2D[] allColliders = Physics2D.OverlapCircleAll(position, radius);
        foreach (var col in allColliders)
        {
            if (((1 << col.gameObject.layer) & waterLayer) == 0)
                return false;
        }
        return true;
    }

    public void ClearBobberReference()
    {
        currentBobber = null;
    }

    public bool IsFullyInsideWater(Vector2 position, float radius)
    {
    // Знаходимо всі water-колайдери, які перекривають центр 
        Collider2D water = Physics2D.OverlapPoint(position, waterLayer);
        if (water == null)
            return false;

        // Перевіряємо 8 напрямків по колу
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI / 4f;
            Vector2 checkPoint = position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            if (!water.OverlapPoint(checkPoint))
                return false;
        }
        return true;
    }
}