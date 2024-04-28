using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

public partial struct QueenSystem : ISystem
{
    EntityArchetype larvaArchetype;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Queen>();
        state.RequireForUpdate<Beehive>();
        larvaArchetype = state.EntityManager.CreateArchetype(typeof(LarvaQueen), typeof(Lifespan));
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

        state.Dependency = new SpawnQueenLarva()
        {
            ECB = ECB,
            LarvaArchetype = larvaArchetype
        }.Schedule(state.Dependency);

        state.Dependency = new MarkQueenToLayLarvae()
        {
            ECB = ECB,
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

[BurstCompile]
public partial struct SpawnQueenLarva : IJobEntity
{
    public EntityCommandBuffer ECB;
    public EntityArchetype LarvaArchetype;

    public void Execute(in Queen queen, in Beehive hive, ref HiveLarvaQueenData hiveLarvaData, Entity entity)
    {
        if (hiveLarvaData.LarvaCount <= SimulationData.MAX_QUEEN_LARVAE_IN_HIVE)
        {
            hiveLarvaData.LarvaCount += 1;
            var larvaEntity = ECB.CreateEntity(LarvaArchetype);
            ECB.SetComponent(larvaEntity, new Lifespan(SimulationData.LARVA_LIFESPAN));
            LarvaQueen larvaQueen = new LarvaQueen(queen.Species, hive.Id, entity);
            ECB.SetComponent(larvaEntity, larvaQueen);
        }
    }
}

[BurstCompile]
[WithAll(typeof(Beehive), typeof(Queen), typeof(Lifespan))]
[WithNone(typeof(HiveLarvaQueenData))]
public partial struct MarkQueenToLayLarvae : IJobEntity
{
    public EntityCommandBuffer ECB;

    public void Execute(in Queen queen, in Lifespan lifespan, Entity entity)
    {
        if (MarkHiveToSpawnLarva(queen.Fertility, lifespan.TicksToLive))
        {
            ECB.AddComponent<HiveLarvaQueenData>(entity);
        }
    }

    bool MarkHiveToSpawnLarva(float fertility, int ticksToLive)
    {
        return fertility < SimulationData.FERTILITY_THRESHOLD_FOR_NEW_QUEEN ||
               ticksToLive < SimulationData.TICKS_TO_LIVE_LEFT_TO_SPAWN_NEW_QUEEN;
    }
}
