using Unity.Entities;

public struct Lifespan : IComponentData
{
    public int TicksToLive;

    public Lifespan(int ticksToLive) => TicksToLive = ticksToLive;
}
