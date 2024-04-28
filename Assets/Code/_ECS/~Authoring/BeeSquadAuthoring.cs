using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class beeSquadAuthoring : MonoBehaviour
{
    class Baker : Baker<beeSquadAuthoring>
    {
        public override void Bake(beeSquadAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new BeeSquad());
            AddComponent(entity, new Target());
            AddComponent(entity, new Roaming());
            AddComponent(entity, new Timer { TimeLeft = SimulationData.TIME_SPENT_COLLECTING });
            AddComponent(entity, new Moving());
            AddComponent(entity, new Lifespan());
        }
    }
}
