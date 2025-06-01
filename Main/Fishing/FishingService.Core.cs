using UnityEngine;
using System;
using System.Collections;

public partial class FishingService : MonoBehaviour, IFishingService
{
    private IPlayerRepository playerRepository;
    private IFishRepository fishRepository;
    private FishingSession currentSession;
    private Player currentPlayer;
    
    private Coroutine fishSpawnCoroutine;
    
    void Awake()
    {
        playerRepository = new PlayerRepository("");
        fishRepository = new FishRepository("");
    }
    
    void Update()
    {
        currentSession?.Update(Time.deltaTime);
    }
    
    public void StartFishing(Player player)
    {
        currentPlayer = player;
        
        if (currentSession == null)
        {
            InitializeSession();
        }
        
        currentSession.StartSession();
        Debug.Log("🎣 Риболовля почалася!");
        
        // Запускаємо логіку появи риби (довгі інтервали)
        StartFishSpawnLogic();
    }
    
    public void StopFishing()
    {
        StopAllCoroutines();
        fishSpawnCoroutine = null;
        
        currentSession?.EndSession();
        Debug.Log("🛑 Риболовля зупинена");
    }
    
    public FishingSession GetCurrentSession() => currentSession;
    
    private void InitializeSession()
    {
        currentSession = new FishingSession();
        currentSession.OnFishingComplete += OnFishingComplete;
        currentSession.OnStateChanged += OnStateChanged;
    }
    
    private void OnStateChanged(FishingState newState)
    {
        Debug.Log($"📋 Стан сесії: {newState}");
    }
    
}