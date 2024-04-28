using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[UpdateBefore(typeof(BeeSquadLifeSystem))]
public partial struct FoodScarcitySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FoodScarcity>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var simulation = SystemAPI.GetSingleton<Simulation>();
        if (!simulation.MakeSimulationStep())
            return;
        state.Dependency = new CalculateFoodScarcity().Schedule(state.Dependency);
    }
}

[BurstCompile]
public partial struct CalculateFoodScarcity : IJobEntity
{
    public void Execute(ref FoodScarcity foodScarcity, in Beehive beehive)
    {
        int population = 1 + beehive.Population;
        float foodStored = beehive.TotalFood;
        foodStored = math.max(foodStored, 0.1f);
        float uncappedFoodScarcity = SimulationData.AVERAGE_FOOD_PREDICTION_VALUE * population / foodStored;
        float foodScarcityCapped = math.clamp(uncappedFoodScarcity, 0, SimulationData.MAX_FOOD_SCARCITY_VALUE);
        foodScarcity.Value = foodScarcityCapped;
    }
}
