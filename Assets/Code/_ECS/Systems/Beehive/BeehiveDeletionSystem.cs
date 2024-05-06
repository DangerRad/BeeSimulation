using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct BeehiveDeletionSystem : ISystem
{
    EntityQuery _beeSquadQuery;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Beehive>();
        _beeSquadQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<BeeSquad, SquadHiveID>()
            .Build(ref state);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var hit = SystemAPI.GetSingletonRW<Hit>();
        var inputState = SystemAPI.GetSingleton<InputState>();
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        if (inputState.Delete && hit.ValueRO.HitEntity != Entity.Null)
        {
            Entity entity = hit.ValueRO.HitEntity;
            int hiveID = state.EntityManager.GetComponentData<Beehive>(entity).Id;
            _beeSquadQuery.SetSharedComponentFilter(new SquadHiveID(hiveID));
            ECB.DestroyEntity(_beeSquadQuery, EntityQueryCaptureMode.AtPlayback);
            ECB.DestroyEntity(entity);
            hit.ValueRW.HitEntity = Entity.Null;
        }
    }
}
