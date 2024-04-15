using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


public partial struct MoveSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Moving>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        state.Dependency = new MoveJob
        {
            dt = SystemAPI.Time.DeltaTime,
            ECB = ECB,
        }.Schedule(state.Dependency);
    }
}

[BurstCompile]
[WithAll(typeof(Moving))]
public partial struct MoveJob : IJobEntity
{
    public float dt;
    public EntityCommandBuffer ECB;

    public void Execute(ref Target target, in BeeColonyStats beeColony, ref LocalTransform transform, Entity entity)
    {
        float DISTANCE_TOLERANCE = 0.15f;
        float3 direction = math.normalize(target.Position - transform.Position);
        float3 newPosition = direction * beeColony.Speed * dt + transform.Position;
        LocalTransform updatedTransform = new LocalTransform
        {
            Position = newPosition,
            Scale = transform.Scale,
            Rotation = transform.Rotation
        };
        ECB.SetComponent(entity, updatedTransform);
        if (math.distancesq(newPosition, target.Position) < DISTANCE_TOLERANCE)
        {
            ECB.SetComponentEnabled<Moving>(entity, false);
        }
    }
}
