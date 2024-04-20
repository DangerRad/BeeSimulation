using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(BeeSquadLifespanSystem))]
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

    public void Execute(in Target target, in BeeColonyStats beeColony, ref LocalTransform transform, Entity entity)
    {
        float3 moveDirection = target.Position - transform.Position;
        float3 moveVector = math.normalize(moveDirection) * (beeColony.Speed * dt);
        float3 newPosition;

        if (math.lengthsq(moveDirection) <= math.lengthsq(moveVector))
        {
            newPosition = target.Position;
            ECB.SetComponentEnabled<Moving>(entity, false);
        }
        else
        {
            newPosition = moveVector + transform.Position;
        }

        float3 t = math.normalize(math.cross(new float3(0, 1, 0), moveDirection));
        quaternion newRotation = math.quaternion(math.float3x3(t, math.cross(moveDirection, t), moveDirection));
        LocalTransform updatedTransform = new LocalTransform
        {
            Position = newPosition,
            Scale = transform.Scale,
            Rotation = newRotation
        };
        transform = updatedTransform;
    }
}
