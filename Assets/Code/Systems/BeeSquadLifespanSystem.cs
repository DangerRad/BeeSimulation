using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public partial struct BeeSquadLifespanSystem : ISystem
{
    ComponentLookup<Beehive> beehiveLookUp;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeeSquad>();
        state.RequireForUpdate<Config>();
        beehiveLookUp = state.GetComponentLookup<Beehive>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<Config>();
        if (!config.MakeSimulationStep())
            return;

        beehiveLookUp.Update(ref state);
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        int ticksToLive = config.TicksInDay + config.TicksInNight;

        state.Dependency = new ManageForagersLifespanJob()
        {
            ECB = ECB.AsParallelWriter(),
            BeehiveLookUp = beehiveLookUp
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
    [ReadOnly] public ComponentLookup<Beehive> BeehiveLookUp;

    public void Execute([ChunkIndexInQuery] int chunkIndex, ref BeeSquad beeSquad, in Entity entity,
        in BeeColonyStats stats)
    {
        Beehive beehive = BeehiveLookUp[stats.BeehiveEntity];
        var rng = new Random((uint)(1 + (entity.Index + beeSquad.Size) * 21342252 % 52331));
        beeSquad.Size -= BeesToKill(beeSquad.TicksToLive, beeSquad.Size, beehive.WeatherSeverity,
            rng.NextFloat(-1, 1), beehive.DangerLevel, beehive.FoodScarcity);
        beeSquad.TicksToLive -= 1;
        if (beeSquad.TicksToLive <= 0 || beeSquad.Size <= 0)
        {
            ECB.DestroyEntity(chunkIndex, entity);
        }
    }

    public static int BeesToKill(
        int ticksToLive, int squadSize, float weather, float luck, float danger, float foodScarcity)
    {
        float defaultDeathRate = 1.0f / (ticksToLive + 1);
        float totalDeathRate = defaultDeathRate * (1 + weather + luck + danger + foodScarcity);
        totalDeathRate = math.clamp(totalDeathRate, 0f, 1f);
        int totalBeesToKill = (int)(totalDeathRate * squadSize);
        return totalBeesToKill;
    }
}
