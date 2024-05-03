using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

[UpdateBefore(typeof(MoveSystem))]
public partial struct RoamSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {

    }

    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new RoamJob
        {
            Time = SystemAPI.Time.ElapsedTime,
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithDisabled(typeof(Moving), typeof(Searching))]
[WithAll(typeof(Roaming), typeof(Forager))]
public partial struct RoamJob : IJobEntity
{
    public Random Rng;
    public double Time;

    public void Execute(ref Target target, in HiveLocationInfo info, EnabledRefRW<Roaming> roaming,
        EnabledRefRW<Moving> moving, EnabledRefRW<Searching> searching)
    {
        uint randomSeed = 1 + (uint)(Time * (info.BeehiveEntity.Index + 1) * 4986455);
        Rng = new Random(randomSeed);
        if (Rng.NextInt(5) == 1)
        {
            roaming.ValueRW = false;
            searching.ValueRW = true;
        }
        else
        {
            target.Position = Rng.NextFloat3(-1, 1) + info.HivePosition + new float3(0, 1.9f, 0);
            moving.ValueRW = true;
        }
    }
}
