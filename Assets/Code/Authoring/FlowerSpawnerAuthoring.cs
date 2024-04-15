using Unity.Entities;
using UnityEngine;

public class FlowerSpawnerAuthoring : MonoBehaviour
{
    public GameObject FlowerPrefab;
    public float SpawnArea;
    public int spawnAmount;

    class Baker : Baker<FlowerSpawnerAuthoring>
    {
        public override void Bake(FlowerSpawnerAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new FlowerSpawner
            {
                FlowerPrefab = GetEntity(authoring.FlowerPrefab, TransformUsageFlags.Dynamic),
                SpawnArea = authoring.SpawnArea,
                SpawnAmount = authoring.spawnAmount
            });
        }
    }
}

public struct FlowerSpawner : IComponentData
{
    public Entity FlowerPrefab;
    public float SpawnArea;
    public int SpawnAmount;
}
