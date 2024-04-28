using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public partial struct BeeSquadLifeSystem : ISystem
{
    ComponentLookup<Beehive> beehiveLookUp;
    ComponentLookup<Mites> mitesLookUp;
    ComponentLookup<FoodScarcity> foodScarcityLookup;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeeSquad>();
        state.RequireForUpdate<Simulation>();
        state.RequireForUpdate<Beehive>();
        state.RequireForUpdate<FoodScarcity>();
        beehiveLookUp = state.GetComponentLookup<Beehive>();
        mitesLookUp = state.GetComponentLookup<Mites>();
        foodScarcityLookup = state.GetComponentLookup<FoodScarcity>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var simulation = SystemAPI.GetSingleton<Simulation>();
        if (!simulation.MakeSimulationStep())
            return;

        beehiveLookUp.Update(ref state);
        mitesLookUp.Update(ref state);
        foodScarcityLookup.Update(ref state);

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        int ticksToLive = simulation.TicksInDay + simulation.TicksInNight;

        state.Dependency = new ManageForagersLifespanJob()
        {
            ECB = ECB.AsParallelWriter(),
            BeehiveLookUp = beehiveLookUp,
            MitesLookUp = mitesLookUp,
            FoodScarcityLookup = foodScarcityLookup
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


    public void Execute([ChunkIndexInQuery] int chunkIndex, in Lifespan lifespan, in Entity entity)
    {
        if (lifespan.TicksToLive <= TicksToPromotion)
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
    [ReadOnly] public ComponentLookup<FoodScarcity> FoodScarcityLookup;

    public void Execute([ChunkIndexInQuery] int chunkIndex, in Lifespan lifespan, in Entity entity,
        ref BeeSquad beeSquad, in HiveLocationInfo info)
    {
        int ticksToLive = lifespan.TicksToLive;
        float infestation = MitesLookUp[info.BeehiveEntity].InfestationAmount;
        float foodScarcity = FoodScarcityLookup[info.BeehiveEntity].Value;
        float weather = 0; //todo when implementing weather system calculate fatality penalty
        float danger = 0; //todo when implementing Dangers (like wasps) calculate fatality penalty
        var rng = new Random((uint)(1 + (entity.Index + beeSquad.Size) * 213422552));
        float luck = rng.NextFloat(-1, 1);

        beeSquad.Size -= BeesToKill(ticksToLive, beeSquad.Size, weather, luck, danger, foodScarcity, infestation);

        if (lifespan.TicksToLive <= 0 || beeSquad.Size <= 0)
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
