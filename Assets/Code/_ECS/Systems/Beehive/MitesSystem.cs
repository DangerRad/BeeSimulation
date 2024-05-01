using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[UpdateBefore(typeof(BeeSquadLifeSystem))]
public partial struct MitesSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Mites>();
        state.RequireForUpdate<Queen>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var simulation = SystemAPI.GetSingleton<Simulation>();
        if (!simulation.MakeSimulationStep())
            return;
        state.Dependency = new HandleMitesPopulation().Schedule(state.Dependency);
    }
}

[BurstCompile]
public partial struct HandleMitesPopulation : IJobEntity
{
    public void Execute(ref Mites mites, in Queen queen, ref RandomData random)
    {
        float randomMites = random.Value.NextFloat(-1.1f, 1f) * SimulationData.MITES_MULTIPLICATION_RATE *
                            SimulationData.MITES_RANDOM_NEW * (1 - queen.MitesResistance);
        float treatedMites = math.lerp(0, mites.TreatmentMultiplier, math.sqrt(mites.InfestationAmount));
        treatedMites = treatedMites * 4 + mites.TreatmentMultiplier;
        float mitesGrowthRate = 0f;
        math.lerp(0, SimulationData.MITES_MULTIPLICATION_RATE, math.sqrt(mites.InfestationAmount));
        float newMites = 4 * mitesGrowthRate + randomMites;
        mites.InfestationAmount = math.clamp(mites.InfestationAmount + newMites - treatedMites, 0, 1);
    }
}
