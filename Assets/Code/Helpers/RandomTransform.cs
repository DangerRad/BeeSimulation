using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

public static class RandomTransform
{
    public static LocalTransform Randomize(ref Random rng, float3 randomXYZ, float3 offset, float scale)
    {
        return new LocalTransform
        {
            Position = new float3
            {
                x = rng.NextFloat(-randomXYZ.x, randomXYZ.x) + offset.x,
                y = rng.NextFloat(-randomXYZ.y, randomXYZ.y) + offset.y,
                z = rng.NextFloat(-randomXYZ.z, randomXYZ.z) + offset.z
            },
            Scale = scale,
            Rotation = quaternion.identity
        };
    }
}
