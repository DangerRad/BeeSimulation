using System;
using Unity.Entities;

[Serializable]
public struct Queen : IComponentData
{
    public BeeSpecies Species;
    public int BeesBirthTick;
    public float Fertility;
    public float MitesResistance;
    public BeeColonyStats BeeColonyStats;
}
