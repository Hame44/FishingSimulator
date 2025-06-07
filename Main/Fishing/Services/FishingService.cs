using UnityEngine;
using System;

public class FishingService : MonoBehaviour, IFishingService
{
    private ISessionManager sessionManager;
    private IFishSpawner fishSpawner;
    private IPlayerActionHandler actionHandler;
    private IFishingResultHandler resultHandler;

    public event Action<FishingState> OnStateChanged;
    public event Action<FishingResult, Fish> OnFishingComplete;

    public FishingSession CurrentSession => sessionManager?.CurrentSession;

    void Awake()
    {
        InitializeDependencies();
        SubscribeToEvents();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    public void StartFishing(Player player)
    {
        sessionManager.StartSession(player);
        fishSpawner.StartSpawning();
    }

    public void StopFishing()
    {
        fishSpawner.StopSpawning();
        sessionManager.EndSession();
    }

    public void HandlePlayerAction(FishingAction action)
    {
        actionHandler.HandleAction(action, CurrentSession);
    }

    private void InitializeDependencies()
    {
        var playerRepository = new PlayerRepository("");
        var fishRepository = new FishRepository("");


        var spawnerObject = new GameObject("FishSpawner");
        spawnerObject.transform.SetParent(transform);
        var spawnerComponent = spawnerObject.AddComponent<FishSpawner>();
        spawnerComponent.Initialize(new FishFactoryProvider());
        fishSpawner = spawnerComponent;


        actionHandler = new PlayerActionHandler();
        resultHandler = new FishingResultHandler(playerRepository, fishRepository);
    }

    public void SetSessionManager(ISessionManager sessionManager)
    {
        this.sessionManager = sessionManager;

        SubscribeToEvents();

        Debug.Log("✅ SessionManager встановлений в FishingService");
    }

    private void SubscribeToEvents()
    {
        if (sessionManager != null)
        {
            sessionManager.OnStateChanged += (state) => OnStateChanged?.Invoke(state);
            sessionManager.OnFishingComplete += (result, fish) =>
            {
                resultHandler.HandleResult(result, fish, sessionManager.CurrentPlayer);
                OnFishingComplete?.Invoke(result, fish);
            };
        }
    }

    private void UnsubscribeFromEvents()
    {
        // Clean unsubscription logic
    }
}