using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    public float floatBobSpeed = 1.5f;
    public float floatBobIntensity = 0.03f;
    public float biteBobIntensity = 0.2f;
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
    private FishingVisualEffects visualEffects;
    
    // State
    private FishingState currentState;
    private bool isFloatCast = false;
    private bool isFishBiting = false;
    private bool isReeling = false;
    private bool isHooked = false;
    
    private float currentFishDistance;
    private float fightTimer = 0f;
    private float tensionLevel = 0f;
    
    // Coroutines
    private Coroutine fightCoroutine;
    private Coroutine floatBobCoroutine;
    
    // Cached delays
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
        CreatePlayer();
        SetupInitialState();
    }
    
    void Update()
    {
        gameLogic.UpdateGameLogic();
        visualEffects.UpdateVisualEffects();
        uiManager.UpdateUI();
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
        visualEffects = new FishingVisualEffects(this);
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
    
    // Public API methods
    public void CastLine()
    {
        if (!isFloatCast && !gameLogic.IsProcessingAction())
        {
            StartCoroutine(gameLogic.CastLineCoroutine());
        }
    }
    
    public void HookOrPull()
    {
        gameLogic.HookOrPull();
    }
    
    public void ReleaseLine()
    {
        gameLogic.ReleaseLine();
    }
    
    // Getters for components and state
    public FishingService FishingService => fishingService;
    public Player CurrentPlayer => currentPlayer;
    public FishingAnimator Animator => fishingAnimator;
    public FishingUIManager UIManager => uiManager;
    public FishingVisualEffects VisualEffects => visualEffects;
    
    public bool IsFloatCast => isFloatCast;
    public bool IsFishBiting => isFishBiting;
    public bool IsReeling => isReeling;
    public bool IsHooked => isHooked;
    public FishingState CurrentState => currentState;
    
    public float CurrentFishDistance => currentFishDistance;
    public float FightTimer => fightTimer;
    public float TensionLevel => tensionLevel;
    
    public WaitForSeconds ShortDelay => shortDelay;
    public WaitForSeconds MediumDelay => mediumDelay;
    
    // State setters (внутрішні методи)
    internal void SetFloatCast(bool value) => isFloatCast = value;
    internal void SetFishBiting(bool value) => isFishBiting = value;
    internal void SetReeling(bool value) => isReeling = value;
    internal void SetHooked(bool value) => isHooked = value;
    internal void SetCurrentState(FishingState value) => currentState = value;
    internal void SetCurrentFishDistance(float value) => currentFishDistance = value;
    internal void SetFightTimer(float value) => fightTimer = value;
    internal void SetTensionLevel(float value) => tensionLevel = value;
    internal void SetFightCoroutine(Coroutine value) => fightCoroutine = value;
    internal void SetFloatBobCoroutine(Coroutine value) => floatBobCoroutine = value;
    
    internal Coroutine FightCoroutine => fightCoroutine;
    internal Coroutine FloatBobCoroutine => floatBobCoroutine;
}