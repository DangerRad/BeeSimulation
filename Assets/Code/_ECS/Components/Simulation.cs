using Unity.Entities;
using Unity.Mathematics;

public struct Simulation : IComponentData
{
    public int TicksInSeason;
    public int BeeSquadCount;
    public float TickLength;
    public float TimeToNextStep;
    public int TicksInYear;
    public int TicksInNight;
    public int CurrentTick;
    public int TicksInDay;
    public int CurrentYearTick;
    public Season CurrentSeason;
    public DayPhase CurrentDayPhase;
    public Entity BeePrefab;
    public bool SpawnWinterBees;
    public bool MakeSimulationStep() => TimeToNextStep <= 0 ? true : false;

    public BlobAssetReference<BirthRateOverYearData> BirthRateData;

    public float BirthRateThisTick() => BirthRateData.Value.Ticks[CurrentYearTick];
    // public readonly float BirthRateAtPoint(int currentTick) =>
    //     BirthRateData.Value.Ticks[currentTick % BirthRateData.Value.Ticks.Length];
}
