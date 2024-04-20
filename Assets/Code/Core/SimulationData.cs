using Unity.Collections;

//temporary class to hold constant values in one place
public static class SimulationData
{
    //Flowers
    public const float BASE_NECTAR_HELD_FLOWER = 50;

    //Beehives
    public const float FOOD_EATEN_TICK = 0.0000001f;

    //BeeSquads
    public const int MAX_SQUAD_SIZE = 200;
    public const float TIME_SPENT_COLLECTING = 0.4f;
    public const float BEE_SPEED = 20;
    public const float FOOD_GATHER_SPEED = 0.006F;
    public const float MAX_FOOD_HELD = 0.02F;
    public const float BEE_SCALE = 0.2f;
    public const int TICKS_IN_DAY_NIGHT = 72;
    public const int TICKS_BEFORE_WINTER_TO_SPAWN_WINTER_BEES = 60;
}
