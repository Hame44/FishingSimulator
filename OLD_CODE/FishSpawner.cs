using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("Fish Spawn Settings")]
    [SerializeField] private float luckModifier = 1f;
    [SerializeField] private List<FishTypeWeight> fishTypes = new List<FishTypeWeight>();

    private List<FishFactory> fishFactories = new List<FishFactory>();

    [System.Serializable]
    public class FishTypeWeight
    {
        public string fishType;
        public float spawnWeight = 1f;
    }

    private void Start()
    {
        InitializeFishFactories();
    }

    private void InitializeFishFactories()
    {
        fishFactories.Add(new CarpFactory(luckModifier));
        fishFactories.Add(new PerchFactory(luckModifier));
    }

    public Fish CreateRandomFish()
    {
        if (fishFactories.Count == 0)
        {
            InitializeFishFactories();
        }

        // Вибираємо випадкову фабрику
        int randomIndex = UnityEngine.Random.Range(0, fishFactories.Count);
        FishFactory selectedFactory = fishFactories[randomIndex];

        Fish createdFish = selectedFactory.CreateFish();
        Debug.Log($"🎣 Створено рибу: {createdFish.FishType}, Вага: {createdFish.Weight:F2}кг");

        return createdFish;
    }

    public void SetLuckModifier(float luck)
    {
        luckModifier = luck;
        foreach (var factory in fishFactories)
        {
            factory.SetLuckModifier(luck);
        }
    }
}