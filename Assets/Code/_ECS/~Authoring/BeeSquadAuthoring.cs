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
            AddComponent(entity, new Timer { TimeLeft = SimulationData.TIME_SPENT_COLLECTING });
            AddComponent(entity, new Lifespan());
            AddComponent(entity, new Moving());
            AddComponent(entity, new Roaming());
            AddComponent(entity, new Searching());
            AddComponent(entity, new Collecting());
            AddComponent(entity, new Hiding());
            AddComponent(entity, new Foraging());
            AddComponent(entity, new Delivering());
            AddComponent(entity, new Forager());
        }
    }
}
