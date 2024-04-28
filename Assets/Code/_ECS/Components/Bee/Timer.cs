using Unity.Entities;

public struct Timer : IComponentData
{
    public float TimeLeft;

    public Timer(float timeLeft) => TimeLeft = timeLeft;
}
