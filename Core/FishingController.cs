using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public partial class FishingController : MonoBehaviour
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
    public float floatBobIntensity = 0.02f;
    public float biteBobIntensity = 0.15f;
    public float floatSubmergeDepth = 0.1f;
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
    public FishingGameLogic gameLogic;
    private FishingVisualEffects visualEffects;
    
    // State
    private FishingState currentState;
    private bool isFloatCast = false;
    private bool isFishBiting = false;
    private bool isReeling = false;
    private bool isHooked = false;
    private bool isFighting = false;
    
    private float currentFishDistance;
    private float fightTimer = 0f;
    private float tensionLevel = 0f;
    private Vector3 originalFloatPosition;
    private Vector3 floatCastPosition;
    
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
    public bool IsFighting => isFighting;
    public FishingState CurrentState => currentState;
    
    public float CurrentFishDistance => currentFishDistance;
    public float FightTimer => fightTimer;
    public float TensionLevel => tensionLevel;
    public Vector3 OriginalFloatPosition => originalFloatPosition;
    public Vector3 FloatCastPosition => floatCastPosition;
    
    public WaitForSeconds ShortDelay => shortDelay;
    public WaitForSeconds MediumDelay => mediumDelay;
}