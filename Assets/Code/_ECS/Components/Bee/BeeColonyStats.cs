using Unity.Entities;

public struct BeeColonyStats : ISharedComponentData
{
    public float Speed;
    public float MaxFoodHeld;
    public float FoodGatherSpeed;
}
