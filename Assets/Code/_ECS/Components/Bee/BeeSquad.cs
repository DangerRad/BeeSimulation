using Unity.Entities;

public struct BeeSquad : IComponentData
{
    public int Size;
    public float FoodHeld;

    public BeeSquad(int size)
    {
        Size = size;
        FoodHeld = 0;
    }
}
