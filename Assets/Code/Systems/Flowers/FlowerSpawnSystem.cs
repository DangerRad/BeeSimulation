using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;


public partial struct FlowerSpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FlowerSpawner>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        var spawner = SystemAPI.GetSingleton<FlowerSpawner>();
        var rng = new Random(999);
        var flowerSpawnerEntity = SystemAPI.GetSingletonEntity<FlowerSpawner>();
        var offset = SystemAPI.GetComponent<LocalTransform>(flowerSpawnerEntity);
        float3 spawnArea = new float3(1, 0, 1) * spawner.SpawnArea;

        for (int i = 0; i < spawner.SpawnAmount; i++)
        {
            var flowerEntity = state.EntityManager.Instantiate(spawner.FlowerPrefab);
            state.EntityManager.SetComponentData(flowerEntity, RandomTransform.Randomize(ref rng, spawnArea,
                offset.Position, 0.4f));
            Flower flower = state.EntityManager.GetComponentData<Flower>(flowerEntity);
            flower.Size = rng.NextFloat(0, 1f);
            flower.NectarHeld = 50f * (1 + (i % 3));
            flower.Species = (FlowerSpecies)(i % 3);
            state.EntityManager.SetComponentData(flowerEntity, flower);
            float4 color = new float4(0.30f, 0.65f, 0.11f, 1) * (i % 3);
            color %= 1.2f;
            state.EntityManager.SetComponentData(flowerEntity,
                new URPMaterialPropertyBaseColor { Value = color });
        }
    }
}
