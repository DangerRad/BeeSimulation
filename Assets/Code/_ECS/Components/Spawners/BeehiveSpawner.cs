using Unity.Entities;

public struct BeehiveSpawner : IComponentData
{
    public Entity Beehive;
    public float SpawnArea;
    public int Amount;
}
