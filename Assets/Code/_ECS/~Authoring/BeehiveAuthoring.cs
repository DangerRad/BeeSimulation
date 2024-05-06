using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class BeehiveAuthoring : MonoBehaviour
{
    class Baker : Baker<BeehiveAuthoring>
    {
        public override void Bake(BeehiveAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new Mites());
            AddComponent(entity, new RandomData());
            AddComponent(entity, new Beehive());
            AddComponent(entity, new FoodScarcity());
            // AddComponent(entity, new Lifespan());
        }
    }
}
