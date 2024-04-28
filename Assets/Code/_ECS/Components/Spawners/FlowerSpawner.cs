using Unity.Entities;

public struct FlowerSpawner : IComponentData
{
    public Entity FlowerPrefab;
    public float SpawnArea;
    public int SpawnAmount;
    public float FlowerScale;
}
