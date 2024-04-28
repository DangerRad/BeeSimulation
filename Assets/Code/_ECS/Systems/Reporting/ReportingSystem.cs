using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(BeehivePopulationSystem))]
public partial struct ReportingSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeeSquad>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var hit = SystemAPI.GetSingleton<Hit>();
        var simulation = SystemAPI.GetSingleton<Simulation>();

        if (hit.HitChanged || simulation.MakeSimulationStep())
        {
            Entity selectedEntity = hit.HitEntity;
            if (selectedEntity == Entity.Null)
                return;
            Beehive hive = SystemAPI.GetComponent<Beehive>(selectedEntity);
            Mites mites = SystemAPI.GetComponent<Mites>(selectedEntity);
            int squadCount = 0;
            foreach (var squad in SystemAPI.Query<RefRO<BeeSquad>>()
                         .WithSharedComponentFilter(new SquadHiveID { Value = hive.Id }))
            {
                squadCount++;
            }


            float[] foodByType = { hive[0], hive[1], hive[2] };
            BeehiveInfoController.BeehiveInfoChanged?.Invoke(hive.Population, hive.TotalFood, foodByType,
                squadCount, mites.InfestationAmount);

            if (hit.HitChanged)
            {
                BeehiveInfoController.DisplayPositionChanged?.Invoke(SystemAPI
                    .GetComponent<LocalToWorld>(selectedEntity).Position);
            }
        }
    }
}
