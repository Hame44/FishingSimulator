using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public partial class FishingController 
{
    private FishingService fishingService;
    private Player currentPlayer;
    // private FishingAnima fishingAnimator;
    // private FishingUIManager uiManager;
    public FishingLogic fishingLogic;
    // private FishingVisualEffects visualEffects;
    public FloatAnimation floatAnimation;
    private PlayerActionHandler playerActionHandler;
    public SessionManager sessionManager;
    
    public FishingState CurrentState { get; private set; }
    public bool IsFloatCast { get; private set; }
    public bool IsFishBiting { get; private set; }
    public bool IsReeling { get; private set; }
    public bool IsHooked { get; private set; }
    public bool IsFighting { get; private set; }
    public float CurrentFishDistance { get; private set; }
    public Vector3 FloatCastPosition { get; private set; }
    
    private Coroutine fightCoroutine;
    private Coroutine floatBobCoroutine;
    
    public WaitForSeconds ShortDelay { get; private set; }
    public WaitForSeconds MediumDelay { get; private set; }
    
    public FishingService FishingService => fishingService;
    public Player CurrentPlayer => currentPlayer;
    public FishingLogic FishingLogic => fishingLogic;
    // public FishingAnimator Animator => fishingAnimator;
    public FloatAnimation FloatAnimation => floatAnimation;
    public PlayerActionHandler PlayerActionHandler => playerActionHandler;
    public SessionManager SessionManager => sessionManager;
    // public FishingUIManager UIManager => uiManager;
    // public FishingVisualEffects VisualEffects => visualEffects;
    public Coroutine FightCoroutine => fightCoroutine;
    public Coroutine FloatBobCoroutine => floatBobCoroutine;

    internal void SetFloatCast(bool value) => IsFloatCast = value;
    internal void SetFishBiting(bool value) => IsFishBiting = value;
    internal void SetReeling(bool value) => IsReeling = value;
    internal void SetHooked(bool value) => IsHooked = value;
    internal void SetFighting(bool value) => IsFighting = value;
    internal void SetCurrentState(FishingState value) => CurrentState = value;
    internal void SetCurrentFishDistance(float value) => CurrentFishDistance = value;
    internal void SetFightCoroutine(Coroutine value) => fightCoroutine = value;
    internal void SetFloatBobCoroutine(Coroutine value) => floatBobCoroutine = value;
    internal void SetFloatCastPosition(Vector3 value) => FloatCastPosition = value;
}