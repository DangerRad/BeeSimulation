using Unity.Entities;
using Unity.Mathematics;

public struct BeehvieSpawnInfo : IComponentData
{
    public bool HasClicked;
    public float3 Position;
}
