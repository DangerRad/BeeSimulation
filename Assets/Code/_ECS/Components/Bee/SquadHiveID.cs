using Unity.Entities;

public struct SquadHiveID : ISharedComponentData
{
    public int Value;

    public SquadHiveID(int id) => Value = id;
}
