using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

public readonly partial struct QueenInHiveAspect : IAspect
{
    public readonly RefRO<Beehive> Hive;
    public readonly RefRO<Queen> Queen;
    public readonly RefRO<LocalToWorld> Transform;
    public readonly RefRO<URPMaterialPropertyBaseColor> material;
    public readonly RefRW<RandomData> RandomData;
    public readonly Entity Self;

    public BeeColonyStats BeeColonyStats => Queen.ValueRO.BeeColonyStats;
    public int HiveID => Hive.ValueRO.Id;
    public URPMaterialPropertyBaseColor HiveMaterial => material.ValueRO;
    public LocalToWorld HiveTransform => Transform.ValueRO;

    public LocalTransform SpawnedSquadTransform(float scale)
    {
        return new LocalTransform
        {
            Position = Transform.ValueRO.Position,
            Rotation = quaternion.identity,
            Scale = scale,
        };
    }

    public float BeesToBirth()
    {
        return RandomData.ValueRW.Value.NextFloat(0.9f + 1.1f) * Queen.ValueRO.Fertility * Queen.ValueRO.BeesBirthTick;
    }
}
