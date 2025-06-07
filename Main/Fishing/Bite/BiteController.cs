using UnityEngine;
using System.Collections;

public class BiteController : MonoBehaviour
{
    [SerializeField] private FishingController fishingController;

    [Header("Bite Settings")]
    [SerializeField] private float defaultBiteDuration = 3f;
    [SerializeField] private float defaultBiteSpeed = 1f;
    [SerializeField] private float pullTimeout = 4f; // 3-5 секунд без витягування

    private Coroutine biteSequenceCoroutine;
    // ВИДАЛЕНО: pullMonitorCoroutine - не потрібен
    private Coroutine respawnCoroutine;
    private Coroutine pullMonitorCoroutine; // ДОДАНО: новий монітор витягування

    private Fish currentFish;
    private IBiteBehavior currentBiteBehavior;
    
    private FloatAnimation FloatAnimation => fishingController?.floatAnimation;
    
    void Start()
    {
        StartCoroutine(DelayedSubscription());
    }
    
    private IEnumerator DelayedSubscription()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (fishingController == null)
        {
            Debug.LogError("❌ BiteController: FishingController не встановлений в інспекторі!");
            yield break;
        }
        
        if (FloatAnimation == null)
        {
            Debug.LogError("❌ BiteController: FloatAnimation не ініціалізований в FishingController!");
            yield break;
        }
        
        SubscribeToFishingEvents();
        Debug.Log("🔧 BiteController підписаний на події");
    }
    
    void OnDestroy()
    {
        UnsubscribeFromFishingEvents();
        
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
        }
    }
    
    private void SubscribeToFishingEvents()
    {
        if (fishingController.sessionManager != null)
        {
            fishingController.sessionManager.OnStateChanged += HandleStateChanged;
        }
        
        FishingEventBus.Instance.OnFishSpawned += HandleFishSpawned;
    }
    
    private void UnsubscribeFromFishingEvents()
    {
        if (fishingController?.sessionManager != null)
        {
            fishingController.sessionManager.OnStateChanged -= HandleStateChanged;
        }
        
        if (FishingEventBus.Instance != null)
        {
            FishingEventBus.Instance.OnFishSpawned -= HandleFishSpawned;
        }
    }
    
    private void HandleStateChanged(FishingState state)
    {
        if (state == FishingState.Biting && currentFish != null)
        {
            StartBite(currentFish);
        }
    }
    
    private void HandleFishSpawned(Fish fish)
    {
        currentFish = fish;
        Debug.Log($"🐟 BiteController отримав рибу: {fish.FishType}");
        StartCoroutine(DelayedBiteStart(fish));
    }
    
    private IEnumerator DelayedBiteStart(Fish fish)
    {
        float delay = Random.Range(1f, 3f);
        yield return new WaitForSeconds(delay);
        
        if (fishingController.IsFloatCast && 
            !fishingController.IsFishBiting && 
            !fishingController.IsHooked)
        {
            StartBite(fish);
        }
    }
    
    public void StartBite(Fish fish)
    {
        if (fish == null) return;
        
        if (FloatAnimation == null)
        {
            Debug.LogError("❌ FloatAnimation відсутній!");
            return;
        }
        
        currentFish = fish;
        currentBiteBehavior = fish.GetBiteBehavior();
        
        Debug.Log($"🎣 BiteController: Риба {fish.FishType} почала клювати!");
        
        StopAllCoroutines();
        
        // ЗМІНЕНО: Видалено BiteInputHandler з конструктора
        var sequence = new BiteSequence(
            fishingController,
            FloatAnimation,
            currentFish,
            OnFishHooked,
            OnBiteMissed
        );

        biteSequenceCoroutine = StartCoroutine(sequence.Run(defaultBiteDuration, defaultBiteSpeed));
    }

    private void OnFishHooked()
    {
        Debug.Log("✅ BiteController: Риба підсічена!");
        fishingController.SetHooked(true);
        fishingController.SetFishBiting(false);

            var session = fishingController.sessionManager?.CurrentSession;
        if (session != null)
    {
        // ДОДАНО: Перевіряємо і встановлюємо рибу якщо потрібно
        if (session.CurrentFish == null && currentFish != null)
        {
            Debug.Log($"🔧 Встановлюємо рибу в сесії: {currentFish.FishType}");
            session.SetFish(currentFish);
        }
        
        // ВИПРАВЛЕНО: Використовуємо setState замість SetFish для зміни стану
        if (session.State != FishingState.Fighting)
        {
            session.setState(FishingState.Fighting);
            Debug.Log("🔧 Стан сесії встановлено в Fighting");
        }
        
        Debug.Log($"🔧 Сесія після підсікання: стан={session.State}, риба={session.CurrentFish?.FishType}");
    }
    else
    {
        Debug.LogError("❌ Сесія відсутня при підсіканні!");
    }

         fishingController.SetCurrentFishDistance(100f); // Початкова дистанція

        // ДОДАНО: Запускаємо новий монітор витягування
        pullMonitorCoroutine = StartCoroutine(MonitorPulling());
    }

    private void OnBiteMissed()
    {
        Debug.Log("❌ BiteController: Гравець не встиг підсікти");
        fishingController.SetFishBiting(false);
        TryRebite();
    }

    // ДОДАНО: Новий метод моніторингу витягування через ПКМ
    private IEnumerator MonitorPulling()
    {
        float idleTimer = 0f;
        
        while (fishingController.IsHooked)
        {
            bool isPulling = Input.GetMouseButton(1); // ПКМ затиснуто
            
            if (isPulling)
            {
                // Витягуємо рибу
                idleTimer = 0f;
                fishingController.SetReeling(true);

                fishingController.fishingLogic.PullFish();
                
                // Тут можна додати логіку витягування (зменшення дистанції до риби)
                // Debug.Log("🎣 Витягуємо рибу...");
            }
            else
            {
                // ПКМ відпущено - риба не витягується
                fishingController.SetReeling(false);
                idleTimer += Time.deltaTime;
                
                // Якщо не витягуємо довше ніж pullTimeout - риба сходить
                if (idleTimer >= pullTimeout)
                {
                    Debug.Log($"🐟 Риба сходить через бездіяльність ({pullTimeout}с)!");
                    OnFishLost();
                    yield break;
                }
            }

            yield return null;
        }
    }

    private void OnFishLost()
    {
        Debug.Log("🐟 BiteController: Риба втрачена");
        fishingController.SetHooked(false);
        fishingController.SetReeling(false);
        TryRebite();
    }

    private void TryRebite()
    {
        StopAllCoroutines();

        var rebiteHandler = new BiteRebiteHandler(
            currentBiteBehavior,
            () => StartBite(currentFish),
            OnFishEscapedFinal
        );

        StartCoroutine(rebiteHandler.TryRebite());
    }

    private void OnFishEscapedFinal()
    {
        Debug.Log("💨 BiteController: Риба остаточно втекла");
        
        NotifyFishEscaped();
        StartFishRespawn();
    }

    private void StartFishRespawn()
    {
        if (!fishingController.IsFloatCast || fishingController.IsReeling)
        {
            Debug.Log("🚫 Поплавок не закинутий - респавн скасовано");
            return;
        }
        
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
        }
        
        respawnCoroutine = StartCoroutine(RespawnFishAfterDelay());
    }
    
    private IEnumerator RespawnFishAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        
        if (fishingController.IsFloatCast && 
            !fishingController.IsFishBiting && 
            !fishingController.IsHooked && 
            !fishingController.IsReeling)
        {
            Debug.Log("🐟 Запускаємо респавн нової риби...");
            RequestNewFishSpawn();
        }
        
        respawnCoroutine = null;
    }
    
    private void RequestNewFishSpawn()
    {
        var fishSpawner = FindObjectOfType<FishSpawner>();
        
        if (fishSpawner != null)
        {
            fishSpawner.ScheduleNextFish(0f);
            Debug.Log("✅ Запит на новий спавн відправлено");
        }
        else
        {
            Debug.LogError("❌ FishSpawner не знайдено!");
        }
    }
    
    private void NotifyFishEscaped()
    {
        var session = fishingController.sessionManager.CurrentSession;
        session?.CompleteFishing(FishingResult.MissedBite);
    }

    public void StopFishRespawn()
    {
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
            Debug.Log("🛑 Автоматичний респавн зупинено");
        }
    }
}