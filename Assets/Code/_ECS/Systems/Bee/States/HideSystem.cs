using Unity.Burst;
using Unity.Entities;

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
                ECB = ECB,
            }.Schedule(state.Dependency);
        }
        if (simulation.CurrentDayPhase == DayPhase.Morning && simulation.CurrentSeason != Season.Winter)
        {
            state.Dependency = new StopHidingJob().Schedule(state.Dependency);
        }
    }
}

[BurstCompile]
[WithAll(typeof(BeeColonyStats), typeof(Hiding))]
[WithDisabled(typeof(Moving))]
public partial struct StopHidingJob : IJobEntity
{
    public void Execute(EnabledRefRW<Roaming> roaming, EnabledRefRW<Moving> moving)
    {
        moving.ValueRW = false;
        roaming.ValueRW = true;
    }
}

[BurstCompile]
[WithAll(typeof(BeeColonyStats))]
[WithDisabled(typeof(Hiding))]
public partial struct StartHiding : IJobEntity
{
    public EntityCommandBuffer ECB;

    public void Execute(ref Target target, in HiveLocationInfo info, Entity entity, EnabledRefRW<Hiding> hiding)
    {
        ECB.SetComponentEnabled<Foraging>(entity, false);
        ECB.SetComponentEnabled<Delivering>(entity, false);
        ECB.SetComponentEnabled<Searching>(entity, false);
        ECB.SetComponentEnabled<Collecting>(entity, false);
        ECB.SetComponentEnabled<Roaming>(entity, false);
        ECB.SetComponentEnabled<Moving>(entity, true);
        hiding.ValueRW = true;
        target.Position = info.HivePosition;
    }
}
