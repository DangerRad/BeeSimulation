using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;


public partial struct BeehiveSpawnSqudSystem : ISystem
{
    Entity beeSquadEntityTemplate;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Beehive>();
        state.RequireForUpdate<Config>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<Config>();
        if (!config.MakeSimulationStep())
            return;

        float ticksInDayNight = config.TicksInDay + config.TicksInNight;
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        int currentTick = config.CurrentTick;

        if (beeSquadEntityTemplate == Entity.Null)
        {
            beeSquadEntityTemplate = state.EntityManager.Instantiate(config.BeePrefab);
            state.EntityManager.AddChunkComponentData<HiveChunkStats>(beeSquadEntityTemplate);
            state.EntityManager.SetComponentEnabled<Moving>(beeSquadEntityTemplate, false);
        }

        foreach (var (beehive, transform, color, entity)
                 in SystemAPI.Query<RefRO<Beehive>,
                         RefRO<LocalToWorld>,
                         RefRO<URPMaterialPropertyBaseColor>>()
                     .WithEntityAccess())
        {
            var rng = new Random((uint)(entity.Index * 342524123 % 1231) + 1);
            float beesPerTick = beehive.ValueRO.BeesBirthPerDay;
            float dailyScalar = beehive.ValueRO.BirthRateAtPoint(currentTick);
            int totalBeesToSpawn = (int)(1.0f * beesPerTick * rng.NextFloat(0.9f, 1.1f) * dailyScalar);
            var squadColor = new URPMaterialPropertyBaseColor { Value = color.ValueRO.Value };

            while (totalBeesToSpawn > 0)
            {
                int beeSquadSize = ((totalBeesToSpawn - 1) % SimulationData.MAX_SQUAD_SIZE) + 1;
                totalBeesToSpawn -= beeSquadSize;

                var beeSquadEntity = ecb.Instantiate(beeSquadEntityTemplate);
                BeeSquad beeSquad = new BeeSquad
                {
                    Size = beeSquadSize,
                };
                ecb.SetComponent(beeSquadEntity, beeSquad);
                ecb.AddSharedComponent(beeSquadEntity,
                    new BeeColonyStats
                    {
                        HivePosition = transform.ValueRO.Position,
                        Speed = SimulationData.BEE_SPEED,
                        FoodGatherSpeed = SimulationData.FOOD_GATHER_SPEED,
                        MaxFoodHeld = SimulationData.MAX_FOOD_HELD,
                        BeehiveEntity = entity,
                    });
                ecb.AddSharedComponent(beeSquadEntity, new SquadHiveID
                {
                    Value = beehive.ValueRO.Id
                });

                ecb.SetComponent(beeSquadEntity, squadColor);
                ecb.SetComponent(beeSquadEntity, new LocalTransform
                {
                    Position = transform.ValueRO.Position,
                    Scale = SimulationData.BEE_SCALE,
                    Rotation = quaternion.identity,
                });
            }
        }
    }
}
