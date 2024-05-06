using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

public partial struct ManageQueenFertilitySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Queen>();
        state.RequireForUpdate<Beehive>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var simulation = SystemAPI.GetSingleton<Simulation>();
        if (!simulation.MakeSimulationStep())
            return;

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        state.Dependency = new ManageQueenFertility()
        {
            ECB = ECB
        }.Schedule(state.Dependency);
    }
}

[BurstCompile]
[WithAll(typeof(Lifespan), typeof(Beehive))]
public partial struct ManageQueenFertility : IJobEntity
{
    public EntityCommandBuffer ECB;

    public void Execute(in Queen queen, in Lifespan lifespan, Entity entity)
    {
        //todo find better parametrised function to calculate fertility
        float newFertility = queen.Fertility - math.pow(1.0f / ((lifespan.TicksToLive - 30.0f) / 20), 2);
        var newQueenData = queen;
        newQueenData.Fertility = math.max(0.1f, newFertility);
        ECB.SetComponent(entity, newQueenData);
    }
}
