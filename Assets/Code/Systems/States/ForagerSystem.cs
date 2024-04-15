using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Random = Unity.Mathematics.Random;


public partial struct ForagerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Forager>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        state.Dependency = new ForageJob
        {
            ECB = ECB,
        }.Schedule(state.Dependency);
    }
}

[WithDisabled(typeof(Moving))]
[WithAll(typeof(Foraging))]
public partial struct ForageJob : IJobEntity
{
    public EntityCommandBuffer ECB;

    public void Execute(ref BeeSquad beeSquad, ref Timer timer, Entity entity)
    {
        timer.TimeLeft = 2f;
        ECB.RemoveComponent<Foraging>(entity);
        ECB.AddComponent<Collecting>(entity);
    }
}
