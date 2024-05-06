using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateAfter(typeof(ManageQueenFertilitySystem))]
public partial struct PupationSystem : ISystem
{
    ComponentLookup<HiveLarvaQueenData> _hiveLarvaLookUp;

    public void OnCreate(ref SystemState state)
    {
        _hiveLarvaLookUp = state.GetComponentLookup<HiveLarvaQueenData>();
        state.RequireForUpdate<LarvaQueen>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var simulation = SystemAPI.GetSingleton<Simulation>();
        if (!simulation.MakeSimulationStep())
            return;
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        _hiveLarvaLookUp.Update(ref state);

        state.Dependency = new QueenPromotion()
        {
            HiveLarvaLookUp = _hiveLarvaLookUp,
            ECB = ECB,
        }.Schedule(state.Dependency);
    }
}

[BurstCompile]
public partial struct QueenPromotion : IJobEntity
{
    public EntityCommandBuffer ECB;
    [ReadOnly] public ComponentLookup<HiveLarvaQueenData> HiveLarvaLookUp;

    public void Execute(ref LarvaQueen larvaQueen, in Lifespan lifespan, in FutureQueenData newQueen, Entity entity)
    {
        if (!HiveLarvaLookUp.HasComponent(larvaQueen.HiveEntity))
        {
            ECB.DestroyEntity(entity);
        }
        else if (lifespan.TicksToLive <= 0)
        {
            var hiveEntity = larvaQueen.HiveEntity;
            ECB.RemoveComponent<HiveLarvaQueenData>(hiveEntity);
            ECB.SetComponent(hiveEntity, newQueen.Queen);
            ECB.SetComponent(hiveEntity, newQueen.Lifespan);
            ECB.SetComponentEnabled<Queen>(hiveEntity, true);
        }
    }
}
