using Code.Core;
using Unity.Entities;

public struct QueenRandomStats : IComponentData
{
    public BeeSpecies Species;
    public RandomRange<QueenStats> Queen;
    public RandomRange<ColonyStats> Colony;
}
