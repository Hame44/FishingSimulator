using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FishingController : MonoBehaviour
{
    [Header("UI References")]
    public Button castButton;
    public Button hookPullButton; // Одна кнопка для підсікання та тягання
    public Button releaseButton;
    public Text statusText;
    public Text playerStatsText;
    public Text instructionText;
    
    [Header("Visual Elements")]
    public GameObject floatObject; // Поплавок
    public Transform waterSurface; // Поверхня води
    public Transform shore; // Берег
    public LineRenderer fishingLine; // Леска
    public Transform rodTip; // Кінчик вудки
    
    [Header("Float Animation")]
    public float floatBobSpeed = 2f;
    public float floatBobIntensity = 0.1f;
    public float biteBobIntensity = 0.5f;
    public float biteBobSpeed = 8f;
    
    [Header("Fishing Parameters")]
    public float castDistance = 5f;
    public float pullSpeed = 2f;
    
    private FishingService fishingService;
    private Player currentPlayer;
    private Vector3 floatStartPosition;
    private Vector3 floatTargetPosition;
    private bool isFloatCast = false;
    private bool isFishBiting = false;
    private float currentFishDistance;
    private bool isReeling = false;
    
    void Start()
    {
        InitializeServices();
        SetupUI();
        CreatePlayer();
        InitializeVisuals();
    }
    
    private void InitializeServices()
    {
        GameObject serviceObject = new GameObject("FishingService");
        fishingService = serviceObject.AddComponent<FishingService>();
        
        // Підписуємося на події сервісу
        var session = fishingService.GetCurrentSession();
    }
    
    private void SetupUI()
    {
        if (castButton != null)
            castButton.onClick.AddListener(CastLine);
            
        if (hookPullButton != null)
            hookPullButton.onClick.AddListener(HookOrPull);
            
        if (releaseButton != null)
            releaseButton.onClick.AddListener(() => HandlePlayerAction(FishingAction.Release));
            
        UpdateButtonStates();
    }
    
    private void InitializeVisuals()
    {
        if (floatObject != null)
        {
            floatStartPosition = shore != null ? shore.position : transform.position;
            floatObject.transform.position = floatStartPosition;
            floatObject.SetActive(false);
        }
        
        if (fishingLine != null)
        {
            fishingLine.positionCount = 2;
            fishingLine.enabled = false;
        }
        
        currentFishDistance = castDistance;
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
    
    public void CastLine()
    {
        if (!isFloatCast)
        {
            StartCoroutine(CastAnimation());
            fishingService.StartFishing(currentPlayer);
            UpdateStatusText("Закидання вудки...");
        }
    }
    
    private IEnumerator CastAnimation()
    {
        if (floatObject == null || waterSurface == null) yield break;
        
        isFloatCast = true;
        floatObject.SetActive(true);
        
        // Визначаємо позицію закидання
        Vector3 castPosition = waterSurface.position + Vector3.right * castDistance;
        floatTargetPosition = castPosition;
        
        // Анімація закидання
        float castTime = 1f;
        float elapsed = 0f;
        
        while (elapsed < castTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / castTime;
            
            // Параболічна траєкторія
            Vector3 currentPos = Vector3.Lerp(floatStartPosition, castPosition, progress);
            currentPos.y += Mathf.Sin(progress * Mathf.PI) * 2f; // Арка
            
            floatObject.transform.position = currentPos;
            UpdateFishingLine();
            
            yield return null;
        }
        
        floatObject.transform.position = castPosition;
        UpdateStatusText("Очікування риби...");
        UpdateButtonStates();
        
        // Запускаємо анімацію поплавка на воді
        StartCoroutine(FloatBobbing());
    }
    
    private IEnumerator FloatBobbing()
    {
        Vector3 basePosition = floatObject.transform.position;
        
        while (isFloatCast && floatObject != null)
        {
            float bobIntensity = isFishBiting ? biteBobIntensity : floatBobIntensity;
            float bobSpeed = isFishBiting ? biteBobSpeed : floatBobSpeed;
            
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobIntensity;
            floatObject.transform.position = basePosition + Vector3.up * bobOffset;
            
            UpdateFishingLine();
            yield return null;
        }
    }
    
    public void HookOrPull()
    {
        var session = fishingService.GetCurrentSession();
        if (session == null) return;
        
        switch (session.State)
        {
            case FishingState.Biting:
                // Підсікання
                HandlePlayerAction(FishingAction.Hook);
                UpdateStatusText("Підсікли! Тягніть рибу!");
                currentFishDistance = castDistance;
                isReeling = true;
                break;
                
            case FishingState.Fighting:
                // Тягання риби
                HandlePlayerAction(FishingAction.Pull);
                PullFish();
                break;
                
            case FishingState.Waiting:
                // Передчасне підсікання
                HandlePlayerAction(FishingAction.Hook);
                UpdateStatusText("Передчасно! Чекайте поклювки...");
                break;
        }
        
        UpdateButtonStates();
    }
    
    private void PullFish()
    {
        if (currentFishDistance > 0)
        {
            currentFishDistance -= pullSpeed * Time.deltaTime;
            
            // Анімація наближення риби
            Vector3 newPosition = Vector3.Lerp(
                floatStartPosition, 
                floatTargetPosition, 
                currentFishDistance / castDistance
            );
            
            if (floatObject != null)
            {
                floatObject.transform.position = newPosition;
            }
            
            UpdateStatusText($"Тягнемо рибу! Відстань: {currentFishDistance:F1}м");
            
            // Якщо риба дісталася берега
            if (currentFishDistance <= 0.5f)
            {
                CompleteCatch();
            }
        }
    }
    
    private void CompleteCatch()
    {
        var session = fishingService.GetCurrentSession();
        if (session?.CurrentFish != null)
        {
            UpdateStatusText($"Піймали {session.CurrentFish.FishType}! Вага: {session.CurrentFish.Weight:F2}кг");
        }
        
        ResetFishing();
    }
    
    public void HandlePlayerAction(FishingAction action)
    {
        if (fishingService != null)
        {
            fishingService.HandlePlayerAction(action);
        }
    }
    
    private void ResetFishing()
    {
        isFloatCast = false;
        isFishBiting = false;
        isReeling = false;
        
        if (floatObject != null)
        {
            floatObject.SetActive(false);
            floatObject.transform.position = floatStartPosition;
        }
        
        if (fishingLine != null)
        {
            fishingLine.enabled = false;
        }
        
        UpdateButtonStates();
        
        // Автоматично готуємося до нового закидання через 2 секунди
        StartCoroutine(PrepareForNextCast());
    }
    
    private IEnumerator PrepareForNextCast()
    {
        yield return new WaitForSeconds(2f);
        UpdateStatusText("Готовий до нового закидання!");
        UpdateButtonStates();
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
    
    private void UpdateButtonStates()
    {
        var session = fishingService?.GetCurrentSession();
        
        if (castButton != null)
            castButton.interactable = !isFloatCast;
            
        if (hookPullButton != null)
        {
            hookPullButton.interactable = isFloatCast;
            
            // Змінюємо текст кнопки залежно від стану
            var buttonText = hookPullButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                if (session?.State == FishingState.Biting)
                    buttonText.text = "Підсікти!";
                else if (session?.State == FishingState.Fighting)
                    buttonText.text = "Тягнути";
                else
                    buttonText.text = "Підсікти";
            }
        }
        
        if (releaseButton != null)
            releaseButton.interactable = session?.State == FishingState.Hooked || session?.State == FishingState.Fighting;
    }
    
    void Update()
    {
        UpdateUI();
        UpdateGameLogic();
    }
    
    private void UpdateGameLogic()
    {
        var session = fishingService?.GetCurrentSession();
        if (session == null) return;
        
        // Оновлюємо візуальний стан поклювки
        bool wasRitering = isFishBiting;
        isFishBiting = session.State == FishingState.Biting;
        
        if (isFishBiting && !wasRitering)
        {
            UpdateStatusText("КЛЮЄ! Натисніть 'Підсікти'!");
        }
        
        // Продовжуємо тягнути рибу якщо в процесі бою
        if (session.State == FishingState.Fighting && isReeling)
        {
            PullFish();
        }
        
        // Перевіряємо завершення риболовлі
        if (session.State == FishingState.Caught || session.State == FishingState.Escaped)
        {
            if (session.State == FishingState.Escaped)
            {
                UpdateStatusText("Риба втекла...");
            }
            ResetFishing();
        }
        
        UpdateButtonStates();
    }
    
    public void UpdateUI()
    {
        if (currentPlayer != null && playerStatsText != null)
        {
            playerStatsText.text = $"Гравець: {currentPlayer.Name}\n" +
                                 $"Сила: {currentPlayer.Strength:F1}\n" +
                                 $"Досвід: {currentPlayer.Experience}\n" +
                                 $"Вудка: {currentPlayer.Equipment.RodDurability:F0}%\n" +
                                 $"Леска: {currentPlayer.Equipment.LineDurability:F0}%";
        }
        
        // Оновлюємо інструкції
        var session = fishingService?.GetCurrentSession();
        if (instructionText != null)
        {
            string instruction = "";
            if (!isFloatCast)
                instruction = "Натисніть 'Закинути' щоб почати риболовлю";
            else if (session?.State == FishingState.Waiting)
                instruction = "Чекайте поклювки...";
            else if (session?.State == FishingState.Biting)
                instruction = "КЛЮЄ! Швидко натисніть 'Підсікти'!";
            else if (session?.State == FishingState.Fighting)
                instruction = "Тягніть рибу натискаючи 'Тягнути'";
                
            instructionText.text = instruction;
        }
    }
    
    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log(message);
    }
}