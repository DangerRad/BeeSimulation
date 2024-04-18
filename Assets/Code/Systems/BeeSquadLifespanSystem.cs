using Unity.Burst;
using Unity.Collections;
using Unity.Entities;


public partial struct BeeSquadLifespanSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeeSquad>();
        state.RequireForUpdate<Config>();
        // beeQuery = state.GetEntityQuery(typeof(BeeColonyStats));
    }

    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<Config>();
        if (!config.MakeSimulationStep())
            return;

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        int ticksToLive = config.TicksInDay + config.TicksInNight;

        state.Dependency = new ManageForagersLifespanJob()
        {
            ECB = ECB.AsParallelWriter(),
        }.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();

        state.Dependency = new BeeSquadPromotionJob()
        {
            TicksToPromotion = ticksToLive,
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
        beeSquad.TicksToLive -= 1;
        if (beeSquad.TicksToLive <= TicksToPromotion)
        {
            ECB.AddComponent<Forager>(chunkIndex, entity);
        }
    }
}

[BurstCompile]
[WithAll(typeof(Forager))]
public partial struct ManageForagersLifespanJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute([ChunkIndexInQuery] int chunkIndex, ref BeeSquad beeSquad, in Entity entity)
    {
        beeSquad.TicksToLive -= 1;
        if (beeSquad.TicksToLive <= 0)
        {
            ECB.DestroyEntity(chunkIndex, entity);
        }
    }
}
