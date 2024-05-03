using Unity.Entities;

[UpdateBefore(typeof(MoveSystem))]
public partial struct ForagerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Forager>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new ForageJob
        {
        }.ScheduleParallel(state.Dependency);
    }
}

[WithDisabled(typeof(Moving), typeof(Collecting))]
[WithAll(typeof(Foraging))]
public partial struct ForageJob : IJobEntity
{
    public void Execute(ref Timer timer, EnabledRefRW<Foraging> foraging,
        EnabledRefRW<Collecting> collecting)
    {
        timer.TimeLeft = SimulationData.TIME_SPENT_COLLECTING;
        foraging.ValueRW = false;
        collecting.ValueRW = true;
    }
}
