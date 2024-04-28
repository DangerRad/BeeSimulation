using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Core
{
    [CreateAssetMenu(fileName = "BeeSpecies", menuName = "BeeSimulation/BeeSpeciesStats", order = 0)]
    public class BeeSpeciesSO : ScriptableObject
    {
        [SerializeField] BeeSpecies _species;
        [SerializeField] RandomRange<QueenStats> _queenStats;
        [FormerlySerializedAs("_workerStats")] [SerializeField] RandomRange<ColonyStats> colonyStats;

        public BeeSpecies Species => _species;
        public RandomRange<QueenStats> QueenStats => _queenStats;
        public RandomRange<ColonyStats> ColonyStats => colonyStats;

        public Queen GetRandomQueen()
        {
            return new Queen
            {
                Species = _species,
                BeesBirthTick = SimpleRandom.Normal(_queenStats.Min.BeesBirthTick, _queenStats.Max.BeesBirthTick),
                Fertility = 1.0f,
                MitesResistance = SimpleRandom.Normal(_queenStats.Min.MitesResistance, _queenStats.Max.MitesResistance),
                BeeColonyStats = GetRandomColonyStats()
            };
        }

        public BeeColonyStats GetRandomColonyStats()
        {
            return new BeeColonyStats
            {
                Speed = SimpleRandom.Normal(colonyStats.Min.Speed, colonyStats.Max.Speed),
                MaxFoodHeld = SimpleRandom.Normal(colonyStats.Min.MaxFoodHeld, colonyStats.Max.MaxFoodHeld),
                FoodGatherSpeed = SimpleRandom.Normal(colonyStats.Min.GatherSpeed, colonyStats.Max.GatherSpeed),
            };
        }

        public Lifespan GetRandomQueenLifespan()
        {
            return new Lifespan
            {
                TicksToLive = SimpleRandom.Normal(_queenStats.Min.TicksToLive, _queenStats.Max.TicksToLive)
            };
        }
    }
}

[Serializable]
public struct QueenStats
{
    public int TicksToLive;
    public int BeesBirthTick;
    public float MitesResistance;
}

[Serializable]
public struct ColonyStats
{
    public float Speed;
    public float MaxFoodHeld;
    public float GatherSpeed;
}

[Serializable]
public struct RandomRange<T>
{
    public T Min;
    public T Max;
}
