using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class ConfigAuthoring : MonoBehaviour
{
    [Header("Simulation")] public float TickLength = 0.5f;
    public int TicksInYear = 1;
    public int TicksInDay = 1;
    public int TicksInNight = 1;
    [Header("Other")] public int SizeOfInfoSquare = 15;
    public GameObject BeePrefab;


    class Baker : Baker<ConfigAuthoring>
    {
        public override void Bake(ConfigAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new Config
            {
                TickLength = authoring.TickLength,
                TicksInDay = authoring.TicksInDay,
                TicksInYear = authoring.TicksInYear,
                TicksInNight = authoring.TicksInNight,
                BeePrefab = GetEntity(authoring.BeePrefab, TransformUsageFlags.Dynamic)
            });
            AddComponent<Hit>(entity);
        }
    }
}

public struct Config : IComponentData
{
    public float TickLength;
    public float TimeToNextStep;
    public int TicksInYear;
    public int TicksInNight;
    public int CurrentTick;
    public int TicksInDay;
    public Season currentSeason;
    public DayPhase currentDayPhase;
    public Entity BeePrefab;
    public bool MakeSimulationStep() => TimeToNextStep <= 0 ? true : false;
}

public struct Hit : IComponentData
{
    public Entity HitEntity;
    public bool HitChanged;
}

public enum Season : byte
{
    Winter,
    Spring,
    Summer,
    Fall
}

public enum DayPhase : byte
{
    Morning,
    Midday,
    Evening,
    Night
}
