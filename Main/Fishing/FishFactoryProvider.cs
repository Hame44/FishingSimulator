using UnityEngine;

public class FishFactoryProvider : IFishFactoryProvider
{
    public FishFactory GetFactory(string fishType)
    {
        return fishType switch
        {
            "Carp" => new CarpFactory(),
            "Perch" => new PerchFactory(),
            _ => null
        };
    }
}