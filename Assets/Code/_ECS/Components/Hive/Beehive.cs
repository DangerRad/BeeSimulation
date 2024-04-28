using System;
using System.Diagnostics;
using Unity.Entities;

unsafe public struct Beehive : IComponentData
{
    public int Id;
    // public float Ventilation;
    public fixed float FoodStoredByType[3];
    public float TotalFood;
    public int Population;

    [Conditional("DEBUG")]
    private static void BoundsCheck(int index)
    {
        if (index < 0 || index > (3 - 1))
        {
            throw new IndexOutOfRangeException();
        }
    }

    public float this[int index]
    {
        get
        {
            BoundsCheck(index);
            return FoodStoredByType[index];
        }
        set
        {
            BoundsCheck(index);
            FoodStoredByType[index] = value;
        }
    }
}
