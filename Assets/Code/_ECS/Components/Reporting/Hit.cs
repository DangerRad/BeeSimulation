using Unity.Entities;

public struct Hit : IComponentData
{
    public Entity HitEntity;
    public bool HitChanged;
}
