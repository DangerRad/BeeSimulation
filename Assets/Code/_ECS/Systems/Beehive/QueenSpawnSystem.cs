using Code.Core;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public partial class QueenSpawnSystem : SystemBase
{
    BeeSpeciesSO[] _beeSpeciesSO;
    NativeParallelHashMap<byte, QueenRandomStats> _queenRandomStatsLookUp;

    protected override void OnCreate()
    {
        RequireForUpdate<LarvaQueen>();
    }

    protected override void OnDestroy()
    {
        _queenRandomStatsLookUp.Dispose();
    }

    protected override void OnStartRunning()
    {
        _beeSpeciesSO = GameObject.FindObjectOfType<SimulationManager>().BeeSpecies;

        int numberOfSpecies = _beeSpeciesSO.Length;
        if (numberOfSpecies < 1)
            Debug.LogError("there is no species to spawn in SimulationManager");
        _queenRandomStatsLookUp =
            new NativeParallelHashMap<byte, QueenRandomStats>(numberOfSpecies, Allocator.Persistent);

        foreach (var SO in _beeSpeciesSO)
        {
            QueenRandomStats queenStats = new QueenRandomStats();
            queenStats.Species = SO.Species;
            queenStats.Queen = SO.QueenStats;
            queenStats.Colony = SO.ColonyStats;
            byte key = (byte)queenStats.Species;
            _queenRandomStatsLookUp.Add(key, queenStats);
        }
    }

    protected override void OnUpdate()
    {
        //todo convert to Isystem
        var simulation = SystemAPI.GetSingleton<Simulation>();
        if (!simulation.MakeSimulationStep())
            return;
        Entities.WithAll<LarvaQueen>().WithNone<Beehive, Queen>().WithStructuralChanges().ForEach(
                (in LarvaQueen larvaQueen, in Entity entity) =>
                {
                    uint seed = 1 + (uint)((SystemAPI.Time.ElapsedTime + 1) * 5432155 * entity.Index);
                    var rng = new Random(seed);
                    Queen futureQueen;
                    Lifespan futureLifespan;
                    QueenRandomStats futureQueenRandomStats = _queenRandomStatsLookUp[(byte)larvaQueen.Species];
                    GetNewRandomQueen(ref futureQueenRandomStats, ref rng, out futureQueen, out futureLifespan);
                    FutureQueenData futureQueenData = new FutureQueenData(futureQueen, futureLifespan);
                    EntityManager.AddComponentData(entity, futureQueenData);
                })
            .Run();
    }

    static void GetNewRandomQueen(ref QueenRandomStats queenStats, ref Random rng,
        out Queen queen, out Lifespan lifespan)
    {
        queen = RandomizeQueen(ref queenStats.Queen, ref rng);
        queen.BeeColonyStats = RandomizeColony(ref queenStats.Colony, ref rng);
        int ticksToLive = rng.NextInt(queenStats.Queen.Min.TicksToLive, queenStats.Queen.Max.TicksToLive);
        lifespan = new Lifespan(ticksToLive);
    }

    static BeeColonyStats RandomizeColony(ref RandomRange<ColonyStats> colonyStats, ref Random rng)
    {
        return new BeeColonyStats
        {
            FoodGatherSpeed = rng.NextFloat(colonyStats.Min.GatherSpeed, colonyStats.Max.GatherSpeed),
            MaxFoodHeld = rng.NextFloat(colonyStats.Min.MaxFoodHeld, colonyStats.Max.MaxFoodHeld),
            Speed = rng.NextFloat(colonyStats.Min.Speed, colonyStats.Max.Speed),
        };
    }

    static Queen RandomizeQueen(ref RandomRange<QueenStats> queenStats, ref Random rng)
    {
        return new Queen()
        {
            BeesBirthTick = rng.NextInt(queenStats.Min.BeesBirthTick, queenStats.Max.BeesBirthTick),
            Fertility = 1,
            MitesResistance = rng.NextFloat(queenStats.Min.MitesResistance, queenStats.Max.MitesResistance),
        };
    }
}
