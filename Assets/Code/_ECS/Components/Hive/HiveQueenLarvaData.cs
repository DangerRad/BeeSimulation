using Unity.Entities;

public struct HiveLarvaQueenData : IComponentData
{
    public int LarvaCount;

    public HiveLarvaQueenData(int larvaCount)
    {
        LarvaCount = larvaCount;
    }
}
