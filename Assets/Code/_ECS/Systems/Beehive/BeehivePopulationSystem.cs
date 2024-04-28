using System;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

//todo refactor, split
public partial struct BeehivePopulationSystem : ISystem
{
    EntityQuery beeSquadQuery;
    NativeParallelHashMap<int, int> populationByHIveID;
    ComponentTypeHandle<HiveChunkStats> hiveChunkHandle;
    SharedComponentTypeHandle<SquadHiveID> squadHiveIdHandle;
    ComponentTypeHandle<BeeSquad> beeSquadHandle;
    NativeArray<int> totalBeeSquadCount;

    int numberFlowerSpecies;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Simulation>();
        state.RequireForUpdate<Beehive>();

        populationByHIveID = new NativeParallelHashMap<int, int>(100, Allocator.Persistent);
        totalBeeSquadCount = new NativeArray<int>(1, Allocator.Persistent);
        squadHiveIdHandle = state.GetSharedComponentTypeHandle<SquadHiveID>();
        hiveChunkHandle = state.GetComponentTypeHandle<HiveChunkStats>();
        beeSquadHandle = state.GetComponentTypeHandle<BeeSquad>();
        beeSquadQuery = state.GetEntityQuery(typeof(BeeSquad), ComponentType.ChunkComponent<HiveChunkStats>(),
            typeof(SquadHiveID));

        numberFlowerSpecies = Enum.GetNames(typeof(FlowerSpecies)).Length;
    }

    public void OnDestroy(ref SystemState state)
    {
        populationByHIveID.Dispose();
        totalBeeSquadCount.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        var simulation = SystemAPI.GetSingletonRW<Simulation>();
        if (!simulation.ValueRO.MakeSimulationStep())
            return;
        simulation.ValueRW.BeeSquadCount = totalBeeSquadCount[0];
        if (!populationByHIveID.ContainsKey(0))
        {
            foreach (var beehive in SystemAPI.Query<RefRO<Beehive>>())
            {
                populationByHIveID.TryAdd(beehive.ValueRO.Id, 0);
            }
        }

        foreach (var hivePopulation in populationByHIveID)
        {
            hivePopulation.Value = 0;
        }

        totalBeeSquadCount[0] = 0;

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        hiveChunkHandle.Update(ref state);
        beeSquadHandle.Update(ref state);
        squadHiveIdHandle.Update(ref state);

        state.Dependency = new UpdateHiveChunkStatsJob()
        {
            HiveChunkStatsTypeHandle = hiveChunkHandle,
            BeeSquadTypeHandle = beeSquadHandle,
        }.ScheduleParallel(beeSquadQuery, state.Dependency);

        state.Dependency = new CombineHiveChunkStatsJob()
        {
            HiveChunkStatsTypeHandle = hiveChunkHandle,
            SquadHiveIDTypeHandle = squadHiveIdHandle,
            PopulationByID = populationByHIveID,
            TotalBeeSquadCount = totalBeeSquadCount
        }.Schedule(beeSquadQuery, state.Dependency);
        state.Dependency.Complete();

        foreach (var (beehive, entity) in SystemAPI.Query<RefRO<Beehive>>().WithEntityAccess())
        {
            Beehive currentBeehive = beehive.ValueRO;
            currentBeehive.Population = populationByHIveID[currentBeehive.Id];
            float totalFood = 0;
            for (int i = 0; i < numberFlowerSpecies; i++)
            {
                totalFood += currentBeehive[i];
            }

            float foodEatenPerTick = SimulationData.FOOD_EATEN_TICK * currentBeehive.Population;
            for (int i = 0; i < numberFlowerSpecies; i++)
            {
                float currentSpeciesEaten = foodEatenPerTick * (currentBeehive[i] / totalFood);
                currentSpeciesEaten = math.clamp(currentSpeciesEaten, 0, currentBeehive[i]);
                currentBeehive[i] -= currentSpeciesEaten;
                totalFood -= currentSpeciesEaten;
            }

            currentBeehive.TotalFood = totalFood;
            ECB.SetComponent(entity, currentBeehive);
        }
    }
}

[BurstCompile]
internal struct UpdateHiveChunkStatsJob : IJobChunk
{
    public ComponentTypeHandle<BeeSquad> BeeSquadTypeHandle;
    public ComponentTypeHandle<HiveChunkStats> HiveChunkStatsTypeHandle;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
        in v128 chunkEnabledMask)
    {
        NativeArray<BeeSquad> beeSquads = chunk.GetNativeArray(ref BeeSquadTypeHandle);
        var en = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
        int beeCount = 0;
        int squadCount = 0;
        while (en.NextEntityIndex(out int i))
        {
            beeCount += beeSquads[i].Size;
            squadCount++;
        }

        chunk.SetChunkComponentData(ref HiveChunkStatsTypeHandle, new HiveChunkStats
        {
            BeeCount = beeCount,
            SquadCount = squadCount
        });
    }
}

[BurstCompile]
internal struct CombineHiveChunkStatsJob : IJobChunk
{
    [ReadOnly] public ComponentTypeHandle<HiveChunkStats> HiveChunkStatsTypeHandle;
    [ReadOnly] public SharedComponentTypeHandle<SquadHiveID> SquadHiveIDTypeHandle;
    public NativeParallelHashMap<int, int> PopulationByID;
    public NativeArray<int> TotalBeeSquadCount;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
        in v128 chunkEnabledMask)
    {
        HiveChunkStats hiveChunkStats = chunk.GetChunkComponentData(ref HiveChunkStatsTypeHandle);
        int hiveID = chunk.GetSharedComponent(SquadHiveIDTypeHandle).Value;
        PopulationByID[hiveID] += hiveChunkStats.BeeCount;
        TotalBeeSquadCount[0] += hiveChunkStats.SquadCount;
    }
}
