using Unity.Entities;

public struct Flower : IComponentData, IEnableableComponent
{
    public float NectarHeld;
    public float Size;
    // public ENUM FLOWER GROWTH PHASE
    public FlowerSpecies Species;

    public Flower(float nectarHeld, float size, FlowerSpecies species)
    {
        NectarHeld = nectarHeld;
        Size = size;
        Species = species;
    }
}

public enum FlowerSpecies : byte
{
    Daisy,
    Lavender,
    Goldenrod
}
