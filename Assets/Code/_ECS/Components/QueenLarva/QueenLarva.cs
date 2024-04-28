using Unity.Entities;

public struct LarvaQueen : IComponentData
{
    public BeeSpecies Species;
    public int HiveId;
    public Entity HiveEntity;

    public LarvaQueen(BeeSpecies species, int hiveId, Entity hiveEntity)
    {
        Species = species;
        HiveId = hiveId;
        HiveEntity = hiveEntity;
    }
}
