using Unity.Entities;
using Unity.Mathematics;

public struct BeeColonyStats : ISharedComponentData
{
    public float Speed;
    public float MaxFoodHeld;
    public float FoodGatherSpeed;
    public float3 HivePosition;
    public Entity BeehiveEntity;
}
