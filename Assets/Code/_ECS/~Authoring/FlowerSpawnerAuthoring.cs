using Unity.Entities;
using UnityEngine;

public class FlowerSpawnerAuthoring : MonoBehaviour
{
    public GameObject FlowerPrefab;
    public float SpawnArea;
    public int spawnAmount;
    public float FlowerScale = 0.4f;

    class Baker : Baker<FlowerSpawnerAuthoring>
    {
        public override void Bake(FlowerSpawnerAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new FlowerSpawner
            {
                FlowerPrefab = GetEntity(authoring.FlowerPrefab, TransformUsageFlags.Dynamic),
                SpawnArea = authoring.SpawnArea,
                SpawnAmount = authoring.spawnAmount,
                FlowerScale = authoring.FlowerScale
            });
        }
    }
}
