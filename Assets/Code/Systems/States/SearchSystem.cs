using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


public partial struct SearchSystem : ISystem
{
    [ReadOnly] ComponentLookup<LocalTransform> transformsLookUp;
    public EntityQuery flowerQuery;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Searching>();
        flowerQuery = state.EntityManager.CreateEntityQuery(typeof(Flower));
        transformsLookUp = state.GetComponentLookup<LocalTransform>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var flowerEntities = flowerQuery.ToEntityListAsync(Allocator.TempJob, state.Dependency, out var dep);
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        transformsLookUp.Update(ref state);


        state.Dependency = new SearchJob
        {
            TransformLookUp = transformsLookUp,
            FlowerEntities = flowerEntities.AsDeferredJobArray(),
            Time = SystemAPI.Time.ElapsedTime,
            ECB = ECB,
        }.Schedule(JobHandle.CombineDependencies(dep, state.Dependency));
    }
}

[BurstCompile]
[WithAll(typeof(Searching))]
[WithDisabled(typeof(Moving))]
public partial struct SearchJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<LocalTransform> TransformLookUp;
    public NativeArray<Entity> FlowerEntities;
    public double Time;
    public EntityCommandBuffer ECB;
    public Random Rng;

    public void Execute(ref Target target, in BeeColonyStats beeColonyStats, in Entity entity)
    {
        if (FlowerEntities.Length == 0)
        {
            ECB.AddComponent<Roaming>(entity);
            ECB.RemoveComponent<Searching>(entity);
        }
        else
        {
            uint randomSeed = 1 + (uint)(Time * (beeColonyStats.BeehiveEntity.Index + 1 + entity.Index) * 9787652231) %
                2021399;
            Rng = new Random(randomSeed);
            Entity randomizedEntity = FlowerEntities[Rng.NextInt(0, FlowerEntities.Length)];
            target.Position = TransformLookUp[randomizedEntity].Position + new float3(0, 0.15f, 0);
            target.Entity = randomizedEntity;
            ECB.SetComponentEnabled<Moving>(entity, true);
            ECB.AddComponent<Foraging>(entity);
            ECB.RemoveComponent<Searching>(entity);
        }
    }
}
