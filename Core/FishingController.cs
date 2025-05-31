using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FishingController : MonoBehaviour
{
    [Header("UI References")]
    public Button castButton;
    public Button hookPullButton;
    public Button releaseButton;
    public Text statusText;
    public Text playerStatsText;
    public Text instructionText;
    public Slider progressBar; // Додано прогрес-бар для боротьби з рибою
    public Text timerText; // Таймер для клювання
    
    [Header("Visual Elements")]
    public GameObject floatObject;
    public Transform waterSurface;
    public Transform shore;
    public LineRenderer fishingLine;
    public Transform rodTip;
    public ParticleSystem splashEffect; // Ефект бризок
    public ParticleSystem biteEffect; // Ефект поклювки
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip castSound;
    public AudioClip splashSound;
    public AudioClip biteSound;
    public AudioClip catchSound;
    public AudioClip escapeSound;
    
    [Header("Float Animation")]
    public float floatBobSpeed = 2f;
    public float floatBobIntensity = 0.1f;
    public float biteBobIntensity = 0.5f;
    public float biteBobSpeed = 8f;
    public AnimationCurve castCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Fishing Parameters")]
    public float castDistance = 5f;
    public float pullSpeed = 2f;
    public float maxFightTime = 30f; // Максимальний час боротьби
    
    [Header("Visual Feedback")]
    public Color normalLineColor = Color.white;
    public Color tensionLineColor = Color.red;
    public float lineWidth = 0.05f;
    public Gradient lineColorGradient;
    
    // Core components
    private FishingService fishingService;
    private Player currentPlayer;
    
    // Animation and visual state
    private Vector3 floatStartPosition;
    private Vector3 floatTargetPosition;
    private bool isFloatCast = false;
    private bool isFishBiting = false;
    private bool isReeling = false;
    
    // Fight mechanics
    private float currentFishDistance;
    private float fightTimer = 0f;
    private float tensionLevel = 0f;
    private Coroutine fightCoroutine;
    
    // Performance optimization
    private readonly Dictionary<string, string> statusMessages = new Dictionary<string, string>
    {
        ["cast"] = "Закидання вудки...",
        ["waiting"] = "Очікування риби...",
        ["biting"] = "КЛЮЄ! Натисніть 'Підсікти'!",
        ["fighting"] = "Тягніть рибу!",
        ["caught"] = "Риба піймана!",
        ["escaped"] = "Риба втекла...",
        ["ready"] = "Готовий до нового закидання!"
    };
    
    private WaitForSeconds shortDelay;
    private WaitForSeconds mediumDelay;
    
    #region Unity Lifecycle
    
    void Awake()
    {
        // Кешуємо WaitForSeconds для оптимізації
        shortDelay = new WaitForSeconds(0.1f);
        mediumDelay = new WaitForSeconds(2f);
    }
    
    void Start()
    {
        InitializeServices();
        SetupUI();
        CreatePlayer();
        InitializeVisuals();
        InitializeAudio();
    }
    
    void Update()
    {
        UpdateGameLogic();
        UpdateVisualEffects();
    }
    
    void OnDestroy()
    {
        // Очищення підписок
        CleanupSubscriptions();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeServices()
    {
        GameObject serviceObject = GameObject.Find("FishingService");
        if (serviceObject == null)
        {
            serviceObject = new GameObject("FishingService");
        }
        
        fishingService = serviceObject.GetComponent<FishingService>();
        if (fishingService == null)
        {
            fishingService = serviceObject.AddComponent<FishingService>();
        }
        
        SubscribeToServiceEvents();
    }
    
    private void SubscribeToServiceEvents()
    {
        var session = fishingService.GetCurrentSession();
        if (session != null)
        {
            session.OnStateChanged += OnFishingStateChanged;
            session.OnFishBite += OnFishBite;
            session.OnFishingComplete += OnFishingComplete;
        }
    }
    
    private void CleanupSubscriptions()
    {
        var session = fishingService?.GetCurrentSession();
        if (session != null)
        {
            session.OnStateChanged -= OnFishingStateChanged;
            session.OnFishBite -= OnFishBite;
            session.OnFishingComplete -= OnFishingComplete;
        }
    }
    
    private void SetupUI()
    {
        // Налаштування кнопок
        castButton?.onClick.AddListener(CastLine);
        hookPullButton?.onClick.AddListener(HookOrPull);
        releaseButton?.onClick.AddListener(() => HandlePlayerAction(FishingAction.Release));
        
        // Початковий стан UI
        UpdateButtonStates();
        SetupProgressBar();
    }
    
    private void SetupProgressBar()
    {
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(false);
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
        }
    }
    
    private void InitializeVisuals()
    {
        // Налаштування поплавка
        if (floatObject != null)
        {
            floatStartPosition = shore != null ? shore.position : transform.position;
            floatObject.transform.position = floatStartPosition;
            floatObject.SetActive(false);
        }
        
        // Налаштування лески
        SetupFishingLine();
        
        currentFishDistance = castDistance;
    }
    
    private void SetupFishingLine()
    {
        if (fishingLine != null)
        {
            fishingLine.positionCount = 2;
            fishingLine.enabled = false;
            fishingLine.startWidth = lineWidth;
            fishingLine.endWidth = lineWidth;
            
            if (fishingLine.material == null)
            {
                Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
                lineMaterial.color = normalLineColor;
                fishingLine.material = lineMaterial;
            }
            else
            {
                fishingLine.material.color = normalLineColor;
            }

            fishingLine.sortingLayerName = "Default";
            fishingLine.sortingOrder = 10;
            fishingLine.useWorldSpace = true;
            
            if (lineColorGradient.colorKeys.Length == 0)
            {
                // Налаштування градієнта за замовчуванням
                GradientColorKey[] colorKeys = new GradientColorKey[2];
                colorKeys[0] = new GradientColorKey(normalLineColor, 0.0f);
                colorKeys[1] = new GradientColorKey(tensionLineColor, 1.0f);
                
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
                alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);
                
                lineColorGradient.SetKeys(colorKeys, alphaKeys);
            }
        }
    }
    
    private void InitializeAudio()
    {
        if (audioSource == null && GetComponent<AudioSource>() != null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    
    private void CreatePlayer()
    {
        currentPlayer = new Player 
        { 
            Id = 1, 
            Name = "Рибалка",
            Strength = 10f,
            Experience = 0,
            Equipment = new Equipment
            {
                RodDurability = 100f,
                LineDurability = 100f,
                LineLength = 10f,
                FishingLuck = 1.2f
            }
        };
    }
    
    #endregion
    
    #region Fishing Actions
    
    public void CastLine()
    {
        if (!isFloatCast && !IsProcessingAction())
        {
            StartCoroutine(CastLineCoroutine());
        }
    }
    
    private IEnumerator CastLineCoroutine()
    {
        PlaySound(castSound);
        UpdateStatusText("cast");
        
        yield return StartCoroutine(CastAnimation());
        
        fishingService.StartFishing(currentPlayer);
        UpdateStatusText("waiting");
        UpdateButtonStates();
        
        StartCoroutine(FloatBobbing());
    }
    
    public void HookOrPull()
    {
        if (IsProcessingAction()) return;
        
        var session = fishingService.GetCurrentSession();
        if (session == null) return;
        
        switch (session.State)
        {
            case FishingState.Biting:
                PerformHook();
                break;
                
            case FishingState.Fighting:
                PerformPull();
                break;
                
            case FishingState.Waiting:
                PerformPrematureHook();
                break;
        }
    }
    
    private void PerformHook()
    {
        var session = fishingService.GetCurrentSession();
        if (session != null && session.TryHook())
        {
            PlaySound(catchSound);
            PlayEffect(biteEffect);
        
            UpdateStatusText("fighting");
        
            // Не запускаємо бій одразу, даємо час на реакцію
            StartCoroutine(DelayedFightStart());
        }
    }

    private IEnumerator DelayedFightStart()
    {
    // Коротка затримка перед початком боротьби
        yield return new WaitForSeconds(0.5f);
    
        var session = fishingService.GetCurrentSession();
        if (session != null && session.State == FishingState.Hooked)
        {
            session.StartFight();
            StartFightSequence();
        }
    }
    
    private void PerformPull()
    {
        HandlePlayerAction(FishingAction.Pull);
        PullFish();
    }
    
    private void PerformPrematureHook()
    {
        HandlePlayerAction(FishingAction.Hook);
        UpdateStatusText("waiting");
        StartCoroutine(ShowTemporaryMessage("Передчасно! Чекайте поклювки...", 2f));
    }
    
    #endregion
    
    #region Animation and Visual Effects
    
    private IEnumerator CastAnimation()
    {
        if (floatObject == null || waterSurface == null) yield break;
        
        isFloatCast = true;
        floatObject.SetActive(true);
        
        Vector3 castPosition = waterSurface.position + Vector3.right * castDistance;
        floatTargetPosition = castPosition;
        
        float castTime = 1.5f;
        float elapsed = 0f;
        
        while (elapsed < castTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / castTime;
            float curveValue = castCurve.Evaluate(progress);
            
            // Плавна параболічна траєкторія
            Vector3 currentPos = Vector3.Lerp(floatStartPosition, castPosition, curveValue);
            currentPos.y += Mathf.Sin(curveValue * Mathf.PI) * 2f;
            
            floatObject.transform.position = currentPos;
            UpdateFishingLine();
            
            yield return null;
        }
        
        floatObject.transform.position = castPosition;
        PlaySound(splashSound);
        PlayEffect(splashEffect);
    }
    
    private IEnumerator FloatBobbing()
    {
        Vector3 basePosition = floatObject.transform.position;
        float originalY = basePosition.y;
        
        while (isFloatCast && floatObject != null)
        {
            if (isFishBiting)
            {
                // Під час клювання поплавок рухається більш драматично
                yield return StartCoroutine(BiteAnimation(basePosition));
            }
            else
            {
                // Звичайне тихе покачування
                float bobOffset = Mathf.Sin(Time.time * floatBobSpeed) * floatBobIntensity;
                Vector3 newPos = basePosition;
                newPos.y = originalY + bobOffset;
                floatObject.transform.position = newPos;
            }
            
            UpdateFishingLine();
            yield return shortDelay;
        }
    }

    private IEnumerator BiteAnimation(Vector3 basePosition)
    {
        float biteTime = 0.5f; // Тривалість одного циклу клювання
        float elapsed = 0f;
    
        while (elapsed < biteTime && isFishBiting)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / biteTime;
        
            // Рух поплавка: спочатку в сторону, потім вниз
            float sideMovement = Mathf.Sin(progress * Mathf.PI * 4) * 0.3f; // Рух в сторону
            float downMovement = -Mathf.Sin(progress * Mathf.PI * 2) * biteBobIntensity; // Рух вниз
        
            Vector3 newPos = basePosition;
            newPos.x += sideMovement;
            newPos.y += downMovement;
        
            floatObject.transform.position = newPos;
            UpdateFishingLine();
        
            yield return null;
        }
    }
    
    private void StartFightSequence()
    {
        currentFishDistance = castDistance;
        isReeling = true;
        fightTimer = 0f;
        tensionLevel = 0f;
        
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(true);
            progressBar.value = 0f;
        }
        
        if (fightCoroutine != null)
        {
            StopCoroutine(fightCoroutine);
        }
        fightCoroutine = StartCoroutine(FightSequenceCoroutine());
    }
    
    private IEnumerator FightSequenceCoroutine()
    {
        while (isReeling && currentFishDistance > 0.5f && fightTimer < maxFightTime)
        {
            fightTimer += Time.deltaTime;
            
            // Оновлення натягу лески
            UpdateTension();
            
            // Оновлення прогрес-бару
            UpdateProgressBar();
            
            // Перевірка на розрив лески при високому натягу
            if (tensionLevel > 0.9f)
            {
                if (Random.value < 0.1f) // 10% шанс розриву при критичному натягу
                {
                    HandleLineBroken();
                    yield break;
                }
            }
            
            yield return null;
        }
        
        // Таймаут боротьби
        if (fightTimer >= maxFightTime)
        {
            HandleFishEscape("Час боротьби вийшов!");
        }
        
        fightCoroutine = null;
    }
    
    private void UpdateTension()
    {
        var session = fishingService.GetCurrentSession();
        if (session?.CurrentFish != null)
        {
            // Розрахунок натягу на основі сили риби та відстані
            float fishStrength = session.CurrentFish.Strength;
            float distanceFactor = currentFishDistance / castDistance;
            tensionLevel = Mathf.Clamp01((fishStrength * distanceFactor) / currentPlayer.Strength);
        }
    }
    
    private void PullFish()
    {
        if (currentFishDistance > 0 && isReeling)
        {
            var session = fishingService.GetCurrentSession();
            if (session?.CurrentFish == null) return;
        
            // Розраховуємо швидкість тягання залежно від сили риби та гравця
            float fishResistance = session.CurrentFish.Strength / currentPlayer.Strength;
            float basePullSpeed = pullSpeed * Time.deltaTime;
            float adjustedPullSpeed = basePullSpeed / (1f + fishResistance * 0.5f);
        
            // Додаємо випадковість - риба може опиратися
            if (UnityEngine.Random.value < fishResistance * 0.3f)
            {
                // Риба опирається - тягнемо повільніше або навіть назад
                adjustedPullSpeed *= 0.2f;
                if (UnityEngine.Random.value < 0.1f)
                {
                    adjustedPullSpeed = -adjustedPullSpeed * 0.5f; // Риба тягне назад
                }
            }
        
            currentFishDistance -= adjustedPullSpeed;
            currentFishDistance = Mathf.Max(0, currentFishDistance); // Не менше 0
        
            // Плавна анімація наближення поплавка
            float distanceRatio = currentFishDistance / castDistance;
            Vector3 targetPos = Vector3.Lerp(shore.position, floatTargetPosition, distanceRatio);
        
            // Додаємо ефект боротьби - поплавок трясеться
            Vector3 fightOffset = new Vector3(
                UnityEngine.Random.Range(-0.1f, 0.1f),
                UnityEngine.Random.Range(-0.1f, 0.1f),
                0
            ) * tensionLevel;
        
            if (floatObject != null)
            {
                floatObject.transform.position = targetPos + fightOffset;
            }
        
            UpdateStatusText($"Тягнемо рибу! Відстань: {currentFishDistance:F1}м (Опір: {fishResistance:F1})");
        
            // Перевірка завершення
            if (currentFishDistance <= 0.1f)
            {
                CompleteCatch();
            }
        }
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnFishingStateChanged(FishingState newState)
    {
        Debug.Log($"Fishing state changed to: {newState}");
        UpdateButtonStates();
        
        switch (newState)
        {
            case FishingState.Biting:
                isFishBiting = true;
                StartCoroutine(BiteTimerCoroutine());
                break;
            case FishingState.Fighting:
                isFishBiting = false;
                break;
            default:
                isFishBiting = false;
                break;
        }
    }
    
    private void OnFishBite(Fish fish)
    {
        PlaySound(biteSound);
        PlayEffect(biteEffect);
        UpdateStatusText("biting");
    }
    
    private void OnFishingComplete(FishingResult result, Fish fish)
    {
        switch (result)
        {
            case FishingResult.Success:
                PlaySound(catchSound);
                UpdateStatusText($"Піймали {fish.FishType}! Вага: {fish.Weight:F2}кг");
                break;
            case FishingResult.FishEscaped:
                PlaySound(escapeSound);
                UpdateStatusText("escaped");
                break;
            default:
                PlaySound(escapeSound);
                UpdateStatusText("escaped");
                break;
        }
        
        StartCoroutine(ResetAfterCompletion());
    }
    
    private IEnumerator BiteTimerCoroutine()
    {
        float biteTime = 3f; // Час на реакцію
        float elapsed = 0f;
        
        while (elapsed < biteTime && isFishBiting)
        {
            elapsed += Time.deltaTime;
            
            if (timerText != null)
            {
                timerText.text = $"Час: {(biteTime - elapsed):F1}с";
            }
            
            yield return null;
        }
        
        if (timerText != null)
        {
            timerText.text = "";
        }
    }
    
    #endregion
    
    #region UI Updates
    
    private void UpdateGameLogic()
    {
        var session = fishingService?.GetCurrentSession();
        if (session == null) return;
        
        // Продовжуємо бій якщо потрібно
        if (session.State == FishingState.Fighting && isReeling)
        {
            PullFish();
        }
        
        // Автоматичне завершення при зміні стану
        if (session.State == FishingState.Caught || session.State == FishingState.Escaped)
        {
            if (session.State == FishingState.Escaped)
            {
                UpdateStatusText("escaped");
            }
            ResetFishing();
        }
    }
    
    private void UpdateVisualEffects()
    {
        UpdateFishingLine();
        UpdateLineColor();
    }
    
    private void UpdateFishingLine()
    {
        if (fishingLine != null && floatObject != null && rodTip != null)
        {
            fishingLine.enabled = isFloatCast;
            if (isFloatCast)
            {
                fishingLine.SetPosition(0, rodTip.position);
                fishingLine.SetPosition(1, floatObject.transform.position);
            }
        }
    }
    
    private void UpdateLineColor()
    {
        if (fishingLine != null && isReeling)
        {
            Color lineColor = lineColorGradient.Evaluate(tensionLevel);
            fishingLine.material.color = lineColor;
        }
    }
    
    private void UpdateProgressBar()
    {
        if (progressBar != null && isReeling)
        {
            float progress = 1f - (currentFishDistance / castDistance);
            progressBar.value = progress;
            
            // Зміна кольору залежно від натягу
            var fillImage = progressBar.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = lineColorGradient.Evaluate(tensionLevel);
            }
        }
    }
    
    private void UpdateButtonStates()
    {
        var session = fishingService?.GetCurrentSession();
        
        if (castButton != null)
            castButton.interactable = !isFloatCast && !IsProcessingAction();
            
        if (hookPullButton != null)
        {
            hookPullButton.interactable = isFloatCast && !IsProcessingAction();
            
            var buttonText = hookPullButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = GetHookPullButtonText(session?.State);
            }
        }
        
        if (releaseButton != null)
        {
            bool canRelease = session?.State == FishingState.Hooked || 
                             session?.State == FishingState.Fighting;
            releaseButton.interactable = canRelease && !IsProcessingAction();
        }
    }
    
    private string GetHookPullButtonText(FishingState? state)
    {
        return state switch
        {
            FishingState.Biting => "Підсікти!",
            FishingState.Fighting => "Тягнути",
            _ => "Підсікти"
        };
    }
    
    public void UpdateUI()
    {
        UpdatePlayerStats();
        UpdateInstructions();
    }
    
    private void UpdatePlayerStats()
    {
        if (currentPlayer != null && playerStatsText != null)
        {
            playerStatsText.text = $"Гравець: {currentPlayer.Name}\n" +
                                 $"Сила: {currentPlayer.Strength:F1}\n" +
                                 $"Досвід: {currentPlayer.Experience}\n" +
                                 $"Вудка: {currentPlayer.Equipment.RodDurability:F0}%\n" +
                                 $"Леска: {currentPlayer.Equipment.LineDurability:F0}%";
        }
    }
    
    private void UpdateInstructions()
    {
        if (instructionText == null) return;
        
        var session = fishingService?.GetCurrentSession();
        string instruction = GetInstructionText(session?.State);
        instructionText.text = instruction;
    }
    
    private string GetInstructionText(FishingState? state)
    {
        if (!isFloatCast)
            return "Натисніть 'Закинути' щоб почати риболовлю";
            
        return state switch
        {
            FishingState.Waiting => "Чекайте поклювки...",
            FishingState.Biting => "КЛЮЄ! Швидко натисніть 'Підсікти'!",
            FishingState.Fighting => "Тягніть рибу натискаючи 'Тягнути'",
            _ => "Очікування..."
        };
    }
    
    #endregion
    
    #region Helper Methods
    
    private void HandlePlayerAction(FishingAction action)
    {
        fishingService?.HandlePlayerAction(action);
    }
    
    private void CompleteCatch()
    {
        var session = fishingService.GetCurrentSession();
        if (session?.CurrentFish != null)
        {
            UpdateStatusText("caught");
            PlaySound(catchSound);
        }
        
        StopFightSequence();
        ResetFishing();
    }
    
    private void HandleLineBroken()
    {
        UpdateStatusText("Леска порвалась!");
        PlaySound(escapeSound);
        StopFightSequence();
        ResetFishing();
    }
    
    private void HandleFishEscape(string reason)
    {
        UpdateStatusText($"Риба втекла! {reason}");
        PlaySound(escapeSound);
        StopFightSequence();
        ResetFishing();
    }
    
    private void StopFightSequence()
    {
        if (fightCoroutine != null)
        {
            StopCoroutine(fightCoroutine);
            fightCoroutine = null;
        }
        
        isReeling = false;
        tensionLevel = 0f;
        
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(false);
        }
    }
    
    private void ResetFishing()
    {
        isFloatCast = false;
        isFishBiting = false;
        
        StopFightSequence();
        
        if (floatObject != null)
        {
            floatObject.SetActive(false);
            floatObject.transform.position = floatStartPosition;
        }
        
        if (fishingLine != null)
        {
            fishingLine.enabled = false;
            fishingLine.material.color = normalLineColor;
        }
        
        if (timerText != null)
        {
            timerText.text = "";
        }
        
        UpdateButtonStates();
        StartCoroutine(PrepareForNextCast());
    }
    
    private IEnumerator ResetAfterCompletion()
    {
        yield return mediumDelay;
        ResetFishing();
    }
    
    private IEnumerator PrepareForNextCast()
    {
        yield return mediumDelay;
        UpdateStatusText("ready");
        UpdateButtonStates();
    }
    
    private IEnumerator ShowTemporaryMessage(string message, float duration)
    {
        string originalMessage = statusText?.text;
        UpdateStatusText(message);
        
        yield return new WaitForSeconds(duration);
        
        if (!string.IsNullOrEmpty(originalMessage))
        {
            UpdateStatusText(originalMessage);
        }
    }
    
    private void UpdateStatusText(string messageKey)
    {
        if (statusText == null) return;
        
        string message = statusMessages.ContainsKey(messageKey) ? 
                        statusMessages[messageKey] : messageKey;
        
        statusText.text = message;
        Debug.Log(message);
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    private void PlayEffect(ParticleSystem effect)
    {
        if (effect != null)
        {
            effect.Play();
        }
    }
    
    private bool IsProcessingAction()
    {
        return fightCoroutine != null;
    }
    
    #endregion
}