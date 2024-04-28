using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[UpdateAfter(typeof(MoveSystem))]
[UpdateBefore(typeof(BeeSquadLifeSystem))]
public partial struct HideSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeeColonyStats>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Simulation simulation = SystemAPI.GetSingleton<Simulation>();
        if (!simulation.MakeSimulationStep())
            return;
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        if (simulation.CurrentDayPhase == DayPhase.Night || simulation.CurrentSeason == Season.Winter)
        {
            state.Dependency = new StartHiding()
            {
                ECB = ECB
            }.Schedule(state.Dependency);
        }

        if (simulation.CurrentDayPhase == DayPhase.Morning && simulation.CurrentSeason != Season.Winter)
        {
            state.Dependency = new StopHidingJob()
            {
                ECB = ECB,
            }.Schedule(state.Dependency);
        }
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

    public void Execute(Entity entity, in HiveLocationInfo info)
    {
        ECB.RemoveComponent<Foraging>(entity);
        ECB.RemoveComponent<Collecting>(entity);
        ECB.RemoveComponent<Delivering>(entity);
        ECB.RemoveComponent<Roaming>(entity);
        ECB.RemoveComponent<Searching>(entity);
        ECB.SetComponentEnabled<Moving>(entity, true);
        ECB.SetComponent(entity, new Target { Position = info.HivePosition });
        ECB.AddComponent<Hiding>(entity);
    }
}
