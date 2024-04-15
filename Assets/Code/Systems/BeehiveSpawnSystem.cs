using UnityEngine;
using Unity.Burst;
using Unity.Entities;
using Unity.Entities.Content;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public partial struct BeehiveSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeehiveSpawner>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        var spawner = SystemAPI.GetSingleton<BeehiveSpawner>();
        var spawnerEntity = SystemAPI.GetSingletonEntity<BeehiveSpawner>();
        var rng = new Random(999);
        var spawnerTransform = state.EntityManager.GetComponentData<LocalTransform>(spawnerEntity);
        float3 spawnArea = new float3(1, 0, 1) * spawner.SpawnArea;

        for (int i = 0; i < spawner.Amount; i++)
        {
            int birthRateBase = i % 2 == 0 ? 2000 : 1300;
            var beehiveEntity = state.EntityManager.Instantiate(spawner.Beehive);
            float4 beeHiveColor = rng.NextFloat4(0, 2);
            beeHiveColor.w = 0;

            Beehive beehive = state.EntityManager.GetComponentData<Beehive>(beehiveEntity);
            beehive.Id = i;
            beehive.BeesBirthPerDay = rng.NextInt(birthRateBase - birthRateBase / 2, birthRateBase);
            beehive.FoodExpenditureBeeDay = 0;

            state.EntityManager.SetComponentData(beehiveEntity, beehive);
            state.EntityManager.SetComponentData(beehiveEntity, RandomTransform.Randomize(ref rng, spawnArea,
                spawnerTransform.Position, 1));
            state.EntityManager.SetComponentData(beehiveEntity,
                new URPMaterialPropertyBaseColor { Value = beeHiveColor });
        }
    }
}
