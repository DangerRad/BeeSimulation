using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


[UpdateAfter(typeof(SearchSystem))]
public partial struct FlowerReplenishSystem : ISystem
{
    public bool shouldReplenish;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Flower>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var simulation = SystemAPI.GetSingleton<Simulation>();
        if (!simulation.MakeSimulationStep())
            return;
        int dayNightTick = simulation.TicksInDay + simulation.TicksInNight;
        int currentDayTick = simulation.CurrentTick % dayNightTick;

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        if (currentDayTick == dayNightTick - 1)
        {
            state.Dependency = new FlowerReplenish()
            {
                ECB = ECB,
                BaseNectarHeld = SimulationData.BASE_NECTAR_HELD_FLOWER
            }.Schedule(state.Dependency);
        }
    }
}

[BurstCompile]
[WithAll(typeof(Flower))]
public partial struct FlowerReplenish : IJobEntity
{
    public EntityCommandBuffer ECB;
    public float BaseNectarHeld;

    public void Execute(in Flower flower, Entity entity)
    {
        Flower newFlower = new Flower { NectarHeld = BaseNectarHeld * (1 + (int)flower.Species) };
        ECB.SetComponent(entity, newFlower);
        ECB.SetComponentEnabled<Flower>(entity, true);
    }
}
