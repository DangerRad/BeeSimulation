using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public partial struct BeeSquadLifespanSystem : ISystem
{
    ComponentLookup<Beehive> beehiveLookUp;
    ComponentLookup<Mites> mitesLookUp;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeeSquad>();
        state.RequireForUpdate<Config>();
        beehiveLookUp = state.GetComponentLookup<Beehive>();
        mitesLookUp = state.GetComponentLookup<Mites>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<Config>();
        if (!config.MakeSimulationStep())
            return;

        beehiveLookUp.Update(ref state);
        mitesLookUp.Update(ref state);
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        int ticksToLive = config.TicksInDay + config.TicksInNight;

        state.Dependency = new ManageForagersLifespanJob()
        {
            ECB = ECB.AsParallelWriter(),
            BeehiveLookUp = beehiveLookUp,
            MitesLookUp = mitesLookUp
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
    [ReadOnly] public ComponentLookup<Mites> MitesLookUp;

    public void Execute([ChunkIndexInQuery] int chunkIndex, ref BeeSquad beeSquad, in Entity entity,
        in BeeColonyStats stats)
    {
        Beehive beehive = BeehiveLookUp[stats.BeehiveEntity];
        Mites mites = MitesLookUp[stats.BeehiveEntity];

        var rng = new Random((uint)(1 + (entity.Index + beeSquad.Size) * 213422552));

        beeSquad.Size -= BeesToKill(beeSquad.TicksToLive, beeSquad.Size, beehive.WeatherSeverity,
            rng.NextFloat(-1, 1), beehive.DangerLevel, beehive.FoodScarcity, mites.InfestationAmount);

        beeSquad.TicksToLive -= 1;
        if (beeSquad.TicksToLive <= 0 || beeSquad.Size <= 0)
        {
            ECB.DestroyEntity(chunkIndex, entity);
        }
    }

    public static int BeesToKill(int ticksToLive, int squadSize, float weather,
        float luck, float danger, float foodScarcity, float mitesInfestationAmount)
    {
        float defaultDeathRate = 1.0f / (ticksToLive + 1);
        float totalDeathRate = defaultDeathRate * (1 + weather + luck + danger + foodScarcity + mitesInfestationAmount);
        totalDeathRate = math.clamp(totalDeathRate, 0f, 1f);
        int totalBeesToKill = (int)(totalDeathRate * squadSize);
        return totalBeesToKill;
    }
}
