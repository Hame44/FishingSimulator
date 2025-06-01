public interface IFishSpawner
{
    void StartSpawning();
    void StopSpawning();
    void ScheduleNextFish(float delay);
}