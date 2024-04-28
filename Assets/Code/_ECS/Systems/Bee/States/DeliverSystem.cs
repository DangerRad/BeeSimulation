using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[UpdateBefore(typeof(MoveSystem))]
public partial struct DeliverSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Delivering>();
        state.RequireForUpdate<Beehive>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        state.Dependency = new DeliverToHiveJob
        {
            ECB = ECB,
        }.Schedule(state.Dependency);
    }
}

[BurstCompile]
[WithAll(typeof(Delivering))]
[WithDisabled(typeof(Moving))]
public partial struct DeliverToHiveJob : IJobEntity
{
    public EntityCommandBuffer ECB;

    public void Execute(ref BeeSquad beeSquad, Entity entity)
    {
        beeSquad.FoodHeld = 0;
        ECB.RemoveComponent<Delivering>(entity);
        ECB.AddComponent<Roaming>(entity);
    }
}
