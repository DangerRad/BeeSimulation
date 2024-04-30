using Code.Core;
using Unity.Burst;
using Unity.Entities;
using Unity.Entities.Content;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

//used only to showcase simulation
//won't be used in final project
[BurstCompile]
public partial class BeehiveSpawnSystem : SystemBase
{
    BeeSpeciesSO[] _beeSpecies;

    protected override void OnCreate()
    {
        RequireForUpdate<BeehiveSpawner>();
    }

    protected override void OnStartRunning()
    {
        _beeSpecies = GameObject.FindObjectOfType<SimulationManager>().BeeSpecies;
    }

    protected override void OnUpdate()
    {
        Enabled = false;
        if (_beeSpecies.Length < 1)
            Debug.LogError("there is no species to spawn in SimulationManager");

        BeeSpeciesSO speciesToSpawn = _beeSpecies[0];

        var spawner = SystemAPI.GetSingleton<BeehiveSpawner>();
        var spawnerEntity = SystemAPI.GetSingletonEntity<BeehiveSpawner>();
        var rng = new Random(564646546);
        var spawnOffset = EntityManager.GetComponentData<LocalTransform>(spawnerEntity).Position;
        float3 spawnArea = new float3(1, 0, 1) * spawner.SpawnArea;

        for (int i = 0; i < spawner.Amount; i++)
        {
            Entity entity = EntityManager.Instantiate(spawner.Beehive);
            float4 beehiveColor = rng.NextFloat4(0, 1);

            Beehive beehive = EntityManager.GetComponentData<Beehive>(entity);
            beehive.Id = i;
            beehive[0] = 20f;

            Queen queen = speciesToSpawn.GetRandomQueen();
            Lifespan queenLifespan = speciesToSpawn.GetRandomQueenLifespan();
            LocalTransform transform = RandomTransform.Randomize(ref rng, spawnArea, spawnOffset, 1);
            URPMaterialPropertyBaseColor baseColor = new URPMaterialPropertyBaseColor { Value = beehiveColor };
            RandomData random = new RandomData { Value = new Random((uint)(1 + (i + 1) * entity.Index * 11111111)) };

            EntityManager.AddComponentData(entity, queen);
            EntityManager.AddComponentData(entity, queenLifespan);
            EntityManager.SetComponentData(entity, beehive);
            EntityManager.SetComponentData(entity, transform);
            EntityManager.SetComponentData(entity, baseColor);
            EntityManager.SetComponentData(entity, random);
        }
    }
}
