using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

[UpdateBefore(typeof(MoveSystem))]
public partial struct SearchSystem : ISystem
{
    [ReadOnly] ComponentLookup<LocalTransform> transformsLookUp;
    public EntityQuery flowerQuery;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Searching>();
        flowerQuery = state.EntityManager.CreateEntityQuery(typeof(Flower));
        transformsLookUp = state.GetComponentLookup<LocalTransform>();
        state.RequireForUpdate<Collecting>();

    }

    public void OnUpdate(ref SystemState state)
    {
        transformsLookUp.Update(ref state);
        var flowerEntities = flowerQuery.ToEntityListAsync(Allocator.TempJob, state.Dependency, out var dep);

        state.Dependency = new SearchJob
        {
            TransformLookUp = transformsLookUp,
            FlowerEntities = flowerEntities.AsDeferredJobArray(),
            Time = SystemAPI.Time.ElapsedTime,
        }.Schedule(JobHandle.CombineDependencies(dep, state.Dependency));
        flowerEntities.Dispose(state.Dependency);
    }
}

[BurstCompile]
[WithAll(typeof(Searching))]
[WithDisabled(typeof(Moving), typeof(Roaming), typeof(Foraging))]
public partial struct SearchJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<LocalTransform> TransformLookUp;
    public NativeArray<Entity> FlowerEntities;
    public double Time;
    public Random Rng;

    public void Execute(ref Target target, in HiveLocationInfo info, in Entity entity,
        EnabledRefRW<Searching> searching, EnabledRefRW<Roaming> roaming, EnabledRefRW<Foraging> foraging,
        EnabledRefRW<Moving> moving)
    {
        if (FlowerEntities.Length == 0)
        {
            searching.ValueRW = false;
            roaming.ValueRW = true;
        }
        else
        {
            uint randomSeed = 1 + (uint)(Time * (info.BeehiveEntity.Index + 1 + entity.Index) * 9787652231);
            Rng = new Random(randomSeed);
            Entity randomizedEntity = FlowerEntities[Rng.NextInt(0, FlowerEntities.Length)];
            target.Position = TransformLookUp[randomizedEntity].Position + new float3(0, 0.15f, 0);
            target.Entity = randomizedEntity;
            moving.ValueRW = true;
            foraging.ValueRW = true;
            searching.ValueRW = false;
        }
    }
}
