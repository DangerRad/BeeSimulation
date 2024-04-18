using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[UpdateAfterAttribute(typeof(BeeSquadLifespanSystem))]
public partial struct HideSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeeColonyStats>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Config config = SystemAPI.GetSingleton<Config>();
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        if (config.currentDayPhase == DayPhase.Night || config.currentSeason == Season.Winter)
        {
            state.Dependency = new StartHiding()
            {
                ECB = ECB,
            }.Schedule(state.Dependency);
        }

        state.Dependency.Complete();
        if (config.currentDayPhase == DayPhase.Morning && config.currentSeason != Season.Winter)
        {
            state.Dependency = new StopHidingJob()
            {
                ECB = ECB,
            }.Schedule(state.Dependency);
        }
    }

    [BurstCompile]
    [WithAll(typeof(BeeColonyStats), typeof(Hiding))]
    [WithDisabled(typeof(Moving))]
    public partial struct StopHidingJob : IJobEntity
    {
        public EntityCommandBuffer ECB;

        public void Execute(Entity entity)
        {
            ECB.AddComponent<Roaming>(entity);
            ECB.RemoveComponent<Moving>(entity);
        }
    }

    [BurstCompile]
    [WithAll(typeof(BeeColonyStats))]
    [WithNone(typeof(Hiding))]
    public partial struct StartHiding : IJobEntity
    {
        public EntityCommandBuffer ECB;

        public void Execute(Entity entity, in BeeColonyStats stats)
        {
            ECB.RemoveComponent<Foraging>(entity);
            ECB.RemoveComponent<Collecting>(entity);
            ECB.RemoveComponent<Delivering>(entity);
            ECB.RemoveComponent<Roaming>(entity);
            ECB.RemoveComponent<Searching>(entity);
            ECB.SetComponentEnabled<Moving>(entity, true);
            ECB.SetComponent(entity, new Target { Position = stats.HivePosition });
            ECB.AddComponent<Hiding>(entity);
        }
    }
}
