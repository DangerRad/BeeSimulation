using Unity.Entities;

public struct Flower : IComponentData, IEnableableComponent
{
    public float NectarHeld;
    public float Size;
    // public ENUM FLOWER GROWTH PHASE
    public FlowerSpecies Species;
}

public enum FlowerSpecies : byte
{
    Daisy,
    Lavender,
    Goldenrod
}
