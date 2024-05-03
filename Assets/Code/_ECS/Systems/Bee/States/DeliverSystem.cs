using Unity.Burst;
using Unity.Entities;

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
        state.Dependency = new DeliverToHiveJob().ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithAll(typeof(Delivering))]
[WithDisabled(typeof(Moving), typeof(Roaming))]
public partial struct DeliverToHiveJob : IJobEntity
{
    public void Execute(ref BeeSquad beeSquad, EnabledRefRW<Delivering> delivering, EnabledRefRW<Roaming> roaming)
    {
        beeSquad.FoodHeld = 0;
        delivering.ValueRW = false;
        roaming.ValueRW = true;
    }
}
