using UnityEngine;

public class BobberBiteBehaviour : MonoBehaviour
{
    public float biteDuration = 0.75f;
    public float biteSpeed = 1.5f;
    public float lostFishTimeout = 2f;

    private FishingBobber bobber;
    private bool isBiting = false;
    private bool canBite = true;
    private bool fishHooked = false;
    private bool fishLost = false;
    private Vector2 biteDirection;
    private float biteTimer = 0f;
    private float autoBiteTimer = 0f;
    private bool biteScheduled = false;
    private float notPullingTimer = 0f;

    private float waitForNextBiteTimer = 0f;
    private bool waitingForNextBite = false;

    // Додаємо таймер для великого інтервалу
    private float bigBiteCooldown = 0f;
    private bool waitingBigCooldown = false;

    void Awake()
    {
        bobber = GetComponent<FishingBobber>();
    }

    void OnEnable()
    {
        ScheduleAutoBite();
    }

    void ScheduleAutoBite()
    {
        if (!canBite) return;
        autoBiteTimer = Random.Range(1f, 3f); // для першої покльовки швидко
        biteScheduled = true;
    }

    void ScheduleBigCooldown()
    {
        if (waitingBigCooldown) return;
        waitingBigCooldown = true;
        bigBiteCooldown = Random.Range(10f, 20f);
        Debug.Log("Великий інтервал покльовок: " + bigBiteCooldown + " секунд.");
    }

    void Update()
    {

        if (bobber == null || !bobber.IsFullyInsideWater(transform.position, bobber.bobberRadius + bobber.edgeMargin))
            return;

        // Якщо риба зійшла або більше не клює — нічого не робимо

        // Великий інтервал між циклами покльовок
        if (waitingBigCooldown)
        {
            bigBiteCooldown -= Time.deltaTime;
            // Debug.Log("Час до великого інтервалу: " + bigBiteCooldown + " секунд.");
            if (bigBiteCooldown <= 0f)
            {
                waitingBigCooldown = false;
                canBite = true;
                fishLost = false;
                fishHooked = false;
                waitingForNextBite = false;
                biteScheduled = false;
                ScheduleAutoBite();
            }
            return;
        }

        // Таймер автоклювання
        if (biteScheduled && !isBiting && !fishHooked)
        {
            autoBiteTimer -= Time.deltaTime;
            if (autoBiteTimer <= 0f)
            {
                StartBite(Random.value > 0.5f);
                biteScheduled = false;
            }
        }

        // Клювання
        if (isBiting && !fishHooked)
        {
            biteTimer -= Time.deltaTime;
            Vector2 current = transform.position;
            Vector2 nextPosition = current + biteDirection * biteSpeed * Time.deltaTime;

            if (bobber != null && bobber.IsFullyInsideWater(nextPosition, bobber.bobberRadius + bobber.edgeMargin))
                transform.position = new Vector3(nextPosition.x, nextPosition.y, transform.position.z);

            // Якщо гравець тягне під час клювання — риба підсічена
            if (Input.GetMouseButton(1))
            {
                fishHooked = true;
                isBiting = false;
                Debug.Log("Риба підсічена!");
            }

            // Клювання закінчилось, якщо не підсік — плануємо нову або великий інтервал
            if (biteTimer <= 0f && !fishHooked)
            {
                isBiting = false;
                float chance = Random.value;
                if (chance < 0.8f)
                {
                    waitingForNextBite = true;
                    Debug.Log("Риба клюнула, але не підсікли — чекаємо на нову покльовку.");
                    waitForNextBiteTimer = Random.Range(1f, 3f);
                }
                else
                {
                    canBite = false;
                    ScheduleBigCooldown();
                    Debug.Log("Риба клюнула, але не підсікли — великий інтервал покльовок.");
                }
            }
        }

        // Короткий інтервал між покльовками
        if (waitingForNextBite && !fishHooked && !isBiting)
        {
            waitForNextBiteTimer -= Time.deltaTime;
            if (waitForNextBiteTimer <= 0f)
            {
                waitingForNextBite = false;
                StartBite(Random.value > 0.5f);
            }
        }

        // Якщо риба підсічена, але гравець не тягне — запускаємо таймер втрати риби
        if (fishHooked)
        {
            if (Input.GetMouseButton(1))
            {
                notPullingTimer = 0f;
            }
            else
            {
                notPullingTimer += Time.deltaTime;
                if (notPullingTimer > lostFishTimeout)
                {
                    fishLost = true;
                    canBite = false;
                    ScheduleBigCooldown();
                    Debug.Log("Риба зійшла!");
                }
            }
        }

        // Якщо гравець тягне не під час клювання (невчасно) — риба більше не клює до великого інтервалу
        if (!isBiting && !fishHooked && Input.GetMouseButtonDown(1))
        {
            canBite = false;
            ScheduleBigCooldown();
            Debug.Log("Ви тягнули невчасно — риба більше не клює.");
        }
    }

    public void StartBite(bool sideways)
    {
        isBiting = true;
        biteTimer = biteDuration;
        if (sideways)
            biteDirection = Random.value > 0.5f ? Vector2.left : Vector2.right;
        else
            biteDirection = Vector2.down;
    }

    public void OnFishCaught()
    {
        if (fishHooked && !fishLost)
        {
            Debug.Log("Риба зловлена!");
            canBite = false;
            fishHooked = false;

            if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(1);
        }
    }

    public bool IsBiting() => isBiting;
    public bool IsFishHooked() => fishHooked;
    public bool IsFishLost() => fishLost;
    public bool CanBite() => canBite;
}