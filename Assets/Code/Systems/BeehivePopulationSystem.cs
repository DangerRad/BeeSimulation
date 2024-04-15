using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public partial struct BeehiveSystem : ISystem
{
    EntityQuery beeSquadQuery;
    NativeParallelHashMap<int, int> populationByHIveID;
    ComponentTypeHandle<HiveChunkStats> hiveChunkHandle;
    SharedComponentTypeHandle<SquadHiveID> squadHiveIdHandle;
    ComponentTypeHandle<BeeSquad> beeSquadHandle;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Beehive>();

        populationByHIveID = new NativeParallelHashMap<int, int>(100, Allocator.Persistent);
        squadHiveIdHandle = state.GetSharedComponentTypeHandle<SquadHiveID>();
        hiveChunkHandle = state.GetComponentTypeHandle<HiveChunkStats>();
        beeSquadHandle = state.GetComponentTypeHandle<BeeSquad>();
        beeSquadQuery = state.GetEntityQuery(typeof(BeeSquad), ComponentType.ChunkComponent<HiveChunkStats>(),
            typeof(SquadHiveID));
    }

    public void OnDestroy(ref SystemState state)
    {
        populationByHIveID.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<Config>();
        if (!config.MakeSimulationStep())
            return;

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
            PopulationByID = populationByHIveID
        }.Schedule(beeSquadQuery, state.Dependency);
        state.Dependency.Complete();

        foreach (var (beehive, entity) in SystemAPI.Query<RefRO<Beehive>>().WithEntityAccess())
        {
            Beehive currentBeehive = beehive.ValueRO;
            currentBeehive.Population = populationByHIveID[currentBeehive.Id];
            float totalFoodStored = 0;
            for (int i = 0; i < 3; i++)
            {
                totalFoodStored += currentBeehive[i];
            }
            currentBeehive.TotalFood = totalFoodStored;
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
        int count = 0;
        while (en.NextEntityIndex(out int i))
        {
            count += beeSquads[i].Size;
        }

        chunk.SetChunkComponentData(ref HiveChunkStatsTypeHandle, new HiveChunkStats { Count = count });
    }
}

[BurstCompile]
internal struct CombineHiveChunkStatsJob : IJobChunk
{
    [ReadOnly] public ComponentTypeHandle<HiveChunkStats> HiveChunkStatsTypeHandle;
    [ReadOnly] public SharedComponentTypeHandle<SquadHiveID> SquadHiveIDTypeHandle;
    public NativeParallelHashMap<int, int> PopulationByID;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
        in v128 chunkEnabledMask)
    {
        HiveChunkStats hiveChunkStats = chunk.GetChunkComponentData(ref HiveChunkStatsTypeHandle);
        int hiveID = chunk.GetSharedComponent(SquadHiveIDTypeHandle).Value;
        PopulationByID[hiveID] += hiveChunkStats.Count;
    }
}
