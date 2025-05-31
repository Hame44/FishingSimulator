using UnityEngine;

public class FishingBobber : MonoBehaviour
{
    private Transform player;
    public float returnSpeed = 3f;
    public LayerMask waterLayer;
    private FishingBobberSpawner spawner;

    public float bobberRadius = 0.2f;
    public float pickupDistance = 0.5f;
    public float edgeMargin = 0.2f;

    public void Initialize(Transform p, FishingBobberSpawner sp)
    {
        player = p;
        spawner = sp;
    }

    void Update()
    {
    if (Input.GetMouseButton(1) && player != null)
    {
        Vector2 current = transform.position;
        Vector2 target = player.position;
        Vector2 direction = (target - current).normalized;
        Vector2 nextPosition = current + direction * returnSpeed * Time.deltaTime;

        // Якщо можемо рухатись до гравця — рухаємось
        if (IsFullyInsideWater(nextPosition, bobberRadius + edgeMargin))
        {
            transform.position = new Vector3(nextPosition.x, nextPosition.y, transform.position.z);
        }
        else
        {
            // Пробуємо рухатись вздовж краю (перпендикулярно до напрямку на гравця)
            Vector2 perp = new Vector2(-direction.y, direction.x); // 90 градусів
            Vector2 try1 = current + perp * returnSpeed * Time.deltaTime;
            Vector2 try2 = current - perp * returnSpeed * Time.deltaTime;

            if (IsFullyInsideWater(try1, bobberRadius + edgeMargin))
                transform.position = new Vector3(try1.x, try1.y, transform.position.z);
            else if (IsFullyInsideWater(try2, bobberRadius + edgeMargin))
                transform.position = new Vector3(try2.x, try2.y, transform.position.z);
            // Якщо і так не можна — стоїмо на місці
        }

        // Зникнення біля гравця
        if (Vector2.Distance(current, target) < pickupDistance)
        {
            var bite = GetComponent<BobberBiteBehaviour>();
            if (bite != null)
            {
                // Debug.Log("Ви витягнули поплавок під час клювання!");
                bite.OnFishCaught();
            }
            spawner.ClearBobberReference();
            Destroy(gameObject);
        }
    }
    }

    // Перевіряє, чи круг з центром position і радіусом radius повністю у воді
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