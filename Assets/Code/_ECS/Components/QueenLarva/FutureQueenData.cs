using Unity.Entities;

public struct FutureQueenData : IComponentData
{
    public Queen Queen;
    public Lifespan Lifespan;

    public FutureQueenData(Queen queen, Lifespan lifespan)
    {
        Queen = queen;
        Lifespan = lifespan;
    }
}
