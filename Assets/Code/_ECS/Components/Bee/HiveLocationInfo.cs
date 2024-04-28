using Unity.Entities;
using Unity.Mathematics;

public struct HiveLocationInfo : ISharedComponentData
{
    public float3 HivePosition;
    public Entity BeehiveEntity;

    public HiveLocationInfo(float3 hivePosition, Entity beehiveEntity)
    {
        HivePosition = hivePosition;
        BeehiveEntity = beehiveEntity;
    }
}
