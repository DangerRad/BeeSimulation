using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateBefore(typeof(BeehiveSystem))]
public partial struct ReportingSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeeSquad>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var hit = SystemAPI.GetSingleton<Hit>();
        var config = SystemAPI.GetSingleton<Config>();
        if (hit.HitChanged || config.MakeSimulationStep())
        {
            Entity selectedEntity = hit.HitEntity;
            if (selectedEntity == Entity.Null)
                return;
            Beehive hive = SystemAPI.GetComponent<Beehive>(selectedEntity);
            int squadCount = 0;
            foreach (var squad in SystemAPI.Query<RefRO<BeeSquad>>().
                         WithSharedComponentFilter(new SquadHiveID{Value = hive.Id}))
            {
                squadCount++;
            }

            float[] foodByType = { hive[0], hive[1], hive[2] };
            BeehiveInfoController.BeehiveInfoChanged?.Invoke(hive.Population, hive.TotalFood, foodByType, squadCount);
            if (hit.HitChanged)
            {
                BeehiveInfoController.DisplayPositionChanged?.Invoke(SystemAPI
                    .GetComponent<LocalToWorld>(selectedEntity).Position);
            }
        }
    }
}
