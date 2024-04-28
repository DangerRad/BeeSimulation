using System.Drawing;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;


public partial struct BeeSquadSpawnSystem : ISystem
{
    Entity beeSquadEntityTemplate;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Beehive>();
        state.RequireForUpdate<Simulation>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var simulation = SystemAPI.GetSingleton<Simulation>();
        if (!simulation.MakeSimulationStep() || simulation.CurrentSeason == Season.Winter)
            return;

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        bool spawnWinterBees = simulation.SpawnWinterBees;
        int ticksToLive = simulation.TicksInDay + simulation.TicksInNight;
        float beeSpawnDayScalar = simulation.BirthRateThisTick();

        if (beeSquadEntityTemplate == Entity.Null)
        {
            beeSquadEntityTemplate = state.EntityManager.Instantiate(simulation.BeePrefab);
            state.EntityManager.AddChunkComponentData<HiveChunkStats>(beeSquadEntityTemplate);
            state.EntityManager.SetComponentEnabled<Moving>(beeSquadEntityTemplate, false);
        }

        foreach (var QueenInHive in SystemAPI.Query<QueenInHiveAspect>())
        {
            HiveLocationInfo info = new HiveLocationInfo(QueenInHive.HiveTransform.Position, QueenInHive.Self);

            float beesToBirth = QueenInHive.BeesToBirth() * beeSpawnDayScalar;
            int totalBeesToSpawn = (int)beesToBirth;

            while (totalBeesToSpawn > 0)
            {
                int beeSquadSize = ((totalBeesToSpawn - 1) % SimulationData.MAX_SQUAD_SIZE) + 1;
                totalBeesToSpawn -= beeSquadSize;
                var squadEntity = ecb.Instantiate(beeSquadEntityTemplate);

                if (spawnWinterBees)
                {
                    ecb.AddComponent(squadEntity, new WinterBee());
                    ticksToLive = simulation.TicksInSeason + SimulationData.TICKS_BEFORE_WINTER_TO_SPAWN_WINTER_BEES;
                }

                ecb.SetComponent(squadEntity, new BeeSquad(beeSquadSize));
                ecb.SetComponent(squadEntity, new Lifespan(ticksToLive));
                ecb.AddSharedComponent(squadEntity, QueenInHive.BeeColonyStats);
                ecb.AddSharedComponent(squadEntity, info);
                ecb.AddSharedComponent(squadEntity, new SquadHiveID(QueenInHive.HiveID));
                ecb.SetComponent(squadEntity, QueenInHive.HiveMaterial);
                ecb.SetComponent(squadEntity, QueenInHive.SpawnedSquadTransform(SimulationData.BEE_SCALE));
            }
        }
    }
}
