using Unity.Entities;

public struct LarvaQueen : IComponentData
{
    public BeeSpecies Species;
    public Entity HiveEntity;

    public LarvaQueen(BeeSpecies species, Entity hiveEntity)
    {
        Species = species;
        HiveEntity = hiveEntity;
    }
}
