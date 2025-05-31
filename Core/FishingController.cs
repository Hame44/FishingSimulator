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
    public Slider progressBar;
    public Text timerText;
    
    [Header("Visual Elements")]
    public GameObject floatObject;
    public Transform waterSurface;
    public Transform shore;
    public LineRenderer fishingLine;
    public Transform rodTip;
    public ParticleSystem splashEffect;
    public ParticleSystem biteEffect;
    
    [Header("Float Animation")]
    public float floatBobSpeed = 2f;
    public float floatBobIntensity = 0.1f;
    public float biteBobIntensity = 0.5f;
    public float biteBobSpeed = 8f;
    public AnimationCurve castCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Fishing Parameters")]
    public float castDistance = 5f;
    public float pullSpeed = 2f;
    public float maxFightTime = 30f;
    
    [Header("Visual Feedback")]
    public Color normalLineColor = Color.white;
    public Color tensionLineColor = Color.red;
    public float lineWidth = 0.05f;
    public Gradient lineColorGradient;
    
    // Components
    private FishingService fishingService;
    private Player currentPlayer;
    private FishingAnimator fishingAnimator;
    private FishingUIManager uiManager;
    private FishingGameLogic gameLogic;
    
    // State
    private Vector3 floatStartPosition;
    private Vector3 floatTargetPosition;
    private bool isFloatCast = false;
    private bool isFishBiting = false;
    private bool isReeling = false;
    private float currentFishDistance;
    private float fightTimer = 0f;
    private float tensionLevel = 0f;
    private Coroutine fightCoroutine;
    
    private WaitForSeconds shortDelay;
    private WaitForSeconds mediumDelay;
    
    void Awake()
    {
        shortDelay = new WaitForSeconds(0.1f);
        mediumDelay = new WaitForSeconds(2f);
        InitializeComponents();
    }
    
    void Start()
    {
        InitializeServices();
        uiManager.SetupUI(this);
        CreatePlayer();
        fishingAnimator.InitializeVisuals();
    }
    
    void Update()
    {
        gameLogic.UpdateGameLogic();
        fishingAnimator.UpdateVisualEffects();
    }
    
    void OnDestroy()
    {
        CleanupSubscriptions();
    }
    
    private void InitializeComponents()
    {
        fishingAnimator = new FishingAnimator(this);
        uiManager = new FishingUIManager(this);
        gameLogic = new FishingGameLogic(this);
    }
    
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
            session.OnStateChanged += gameLogic.OnFishingStateChanged;
            session.OnFishBite += gameLogic.OnFishBite;
            session.OnFishingComplete += gameLogic.OnFishingComplete;
        }
    }
    
    private void CleanupSubscriptions()
    {
        var session = fishingService?.GetCurrentSession();
        if (session != null)
        {
            session.OnStateChanged -= gameLogic.OnFishingStateChanged;
            session.OnFishBite -= gameLogic.OnFishBite;
            session.OnFishingComplete -= gameLogic.OnFishingComplete;
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
    
    // Public methods for external access
    public void CastLine()
    {
        if (!isFloatCast && !IsProcessingAction())
        {
            StartCoroutine(gameLogic.CastLineCoroutine());
        }
    }
    
    public void HookOrPull()
    {
        gameLogic.HookOrPull();
    }
    
    // Getters for components
    public FishingService FishingService => fishingService;
    public Player CurrentPlayer => currentPlayer;
    public bool IsFloatCast => isFloatCast;
    public bool IsFishBiting => isFishBiting;
    public bool IsReeling => isReeling;
    public float CurrentFishDistance => currentFishDistance;
    public float FightTimer => fightTimer;
    public float TensionLevel => tensionLevel;
    public Coroutine FightCoroutine => fightCoroutine;
    public Vector3 FloatStartPosition => floatStartPosition;
    public Vector3 FloatTargetPosition => floatTargetPosition;
    public WaitForSeconds ShortDelay => shortDelay;
    public WaitForSeconds MediumDelay => mediumDelay;
    
    // Setters for state
    public void SetFloatCast(bool value) => isFloatCast = value;
    public void SetFishBiting(bool value) => isFishBiting = value;
    public void SetReeling(bool value) => isReeling = value;
    public void SetCurrentFishDistance(float value) => currentFishDistance = value;
    public void SetFightTimer(float value) => fightTimer = value;
    public void SetTensionLevel(float value) => tensionLevel = value;
    public void SetFightCoroutine(Coroutine value) => fightCoroutine = value;
    public void SetFloatStartPosition(Vector3 value) => floatStartPosition = value;
    public void SetFloatTargetPosition(Vector3 value) => floatTargetPosition = value;
    
    private bool IsProcessingAction()
    {
        return fightCoroutine != null;
    }
}