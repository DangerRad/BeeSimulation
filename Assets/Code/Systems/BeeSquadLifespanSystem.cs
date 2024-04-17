using Unity.Burst;
using Unity.Collections;
using Unity.Entities;


public partial struct BeeSquadLifespanSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeeSquad>();
        state.RequireForUpdate<Config>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<Config>();
        if (!config.MakeSimulationStep())
            return;

        float ticksInDay = config.TicksInDay / 364.0f;
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        int ticksToLive = config.TicksInDay + config.TicksInNight;
        state.Dependency = new ManageForagersLifespanJob()
        {
            TicksToLive = ticksToLive,
            ECB = ECB.AsParallelWriter(),
        }.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();

        state.Dependency = new BeeSquadPromotionJob()
        {
            TicksToPromotion = ticksToLive / 2,
            ECB = ECB.AsParallelWriter()
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithNone(typeof(Forager))]
[WithAll(typeof(BeeColonyStats))]
public partial struct BeeSquadPromotionJob : IJobEntity
{
    public int TicksToPromotion;
    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute([ChunkIndexInQuery] int chunkIndex, ref BeeSquad beeSquad, in Entity entity)
    {
        beeSquad.AgeInTicks += 1;
        if (beeSquad.AgeInTicks > TicksToPromotion)
        {
            ECB.AddComponent<Forager>(chunkIndex, entity);
        }
    }
}

[BurstCompile]
[WithAll(typeof(Forager))]
public partial struct ManageForagersLifespanJob : IJobEntity
{
    public int TicksToLive;
    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute([ChunkIndexInQuery] int chunkIndex, ref BeeSquad beeSquad, in Entity entity)
    {
        beeSquad.AgeInTicks += 1;
        if (beeSquad.AgeInTicks > TicksToLive)
        {
            ECB.DestroyEntity(chunkIndex, entity);
        }
    }
}
