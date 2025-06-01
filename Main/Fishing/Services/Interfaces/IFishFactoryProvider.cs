public interface IFishFactoryProvider
{
    FishFactory GetFactory(string fishType);
}