using Code.Core;
using Unity.Entities;

public struct HiveLarvaQueenData : IComponentData
{
    public BeeSpecies Species;
    public int LarvaCount;

    public HiveLarvaQueenData(int larvaCount)
    {
        LarvaCount = larvaCount;
        Species = BeeSpecies.Italian;
    }
}
