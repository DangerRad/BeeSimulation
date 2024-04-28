using Unity.Entities;
using UnityEngine;


public class FlowerAuthoring : MonoBehaviour
{
    class Baker : Baker<FlowerAuthoring>
    {
        public override void Bake(FlowerAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new Flower());
        }
    }
}
