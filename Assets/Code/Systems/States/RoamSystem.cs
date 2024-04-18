using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[UpdateBefore(typeof(MoveSystem))]
public partial struct RoamSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Roaming>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        state.Dependency = new RoamJob
        {
            Time = SystemAPI.Time.ElapsedTime,
            ECB = ECB,
        }.Schedule(state.Dependency);
    }
}

[BurstCompile]
[WithDisabled(typeof(Moving))]
[WithAll(typeof(Roaming), typeof(Forager))]
public partial struct RoamJob : IJobEntity
{
    public Random Rng;
    public double Time;
    public EntityCommandBuffer ECB;

    public void Execute(ref Target target, in BeeColonyStats beeColony, Entity entity)
    {
        uint randomSeed = 1 + (uint)(Time * (beeColony.BeehiveEntity.Index + 1) * 1231231) % 213992;
        Rng = new Random(randomSeed);
        if (Rng.NextInt(15) == 1)
        {
            ECB.AddComponent<Searching>(entity);
            ECB.RemoveComponent<Roaming>(entity);
        }
        else
        {
            target.Position = Rng.NextFloat3(-1, 1) + beeColony.HivePosition + new float3(0, 1.9f, 0);
            ECB.SetComponent(entity, target);
            ECB.SetComponentEnabled<Moving>(entity, true);
        }
    }
}
