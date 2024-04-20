using System;
using System.Diagnostics;
using Unity.Entities;

unsafe public struct Beehive : IComponentData
{
    public int Id;
    public float Ventilation;
    public float TotalFood;
    public int BeesBirthPerDay;
    public BlobAssetReference<BirthRateOverYearData> BirthRateData;
    public int Population;
    public float FoodExpenditureTick;
    public fixed float FoodStoredByType[3];

    public float WeatherSeverity;
    public float FoodScarcity;
    public float DangerLevel;

    public readonly float BirthRateAtPoint(int currentTick) =>
        BirthRateData.Value.Ticks[currentTick % BirthRateData.Value.Ticks.Length];


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
