using Unity.Burst;
using Unity.Entities;

public partial struct LifespanSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Lifespan>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var simulation = SystemAPI.GetSingleton<Simulation>();
        if (!simulation.MakeSimulationStep())
            return;
        state.Dependency = new ReduceLifespan().Schedule(state.Dependency);
    }
}

[BurstCompile]
public partial struct ReduceLifespan : IJobEntity
{
    public void Execute(ref Lifespan lifespan)
    {
        lifespan.TicksToLive -= 1;
    }
}
