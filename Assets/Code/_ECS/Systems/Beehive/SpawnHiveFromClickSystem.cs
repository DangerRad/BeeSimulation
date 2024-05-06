using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct SpawnHiveFromClickSystem : ISystem
{
    int _hiveToSpawnId;
    float3 _spawnOffset;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeehvieSpawnInfo>();
        _hiveToSpawnId = 100;
        _spawnOffset = new float3(0, 0.35f, 0);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var beehiveSpawnInfo = SystemAPI.GetSingleton<BeehvieSpawnInfo>();
        var spawner = SystemAPI.GetSingleton<BeehiveSpawner>();

        if (!beehiveSpawnInfo.HasClicked)
            return;
        float3 spawnPosition = beehiveSpawnInfo.Position + _spawnOffset;
        var entity = state.EntityManager.Instantiate(spawner.Beehive);
        Beehive beehive = state.EntityManager.GetComponentData<Beehive>(entity);
        beehive.Id = _hiveToSpawnId++;
        beehive[0] = 20f;


        LocalTransform transform = new LocalTransform
        {
            Position = spawnPosition,
            Rotation = quaternion.identity,
            Scale = 1,
        };
        RandomData random = new RandomData
            { Value = new Random((uint)(1 + (_hiveToSpawnId + 1) * entity.Index * 11111111)) };

        state.EntityManager.AddComponent<HiveLarvaQueenData>(entity);
        state.EntityManager.AddComponentData(entity, new Lifespan());
        state.EntityManager.AddComponentData(entity, new Queen());
        state.EntityManager.SetComponentEnabled<Queen>(entity, false);
        state.EntityManager.SetComponentData(entity, beehive);
        state.EntityManager.SetComponentData(entity, transform);
        state.EntityManager.SetComponentData(entity, random);
    }
}
