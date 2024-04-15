using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


[UpdateAfter(typeof(SearchSystem))]
public partial struct FlowerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Flower>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<Config>();
        int currentDayTick = config.CurrentTick % 72;

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        state.Dependency.Complete();
        if (currentDayTick == 71)
        {
            state.Dependency = new FlowerReplenish()
            {
                ECB = ECB,
            }.Schedule(state.Dependency);
        }
    }
}

[BurstCompile]
[WithDisabled(typeof(Flower))]
// [WithAll(typeof(Flower))]
public partial struct FlowerReplenish : IJobEntity
{
    public EntityCommandBuffer ECB;

    public void Execute(in Flower flower, Entity entity)
    {
        Flower newFlower = new Flower { NectarHeld = 17f };
        ECB.SetComponent<Flower>(entity, newFlower);
        ECB.SetComponentEnabled<Flower>(entity, true);
    }
}
