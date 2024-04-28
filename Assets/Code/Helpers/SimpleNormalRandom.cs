using UnityEngine;

public static class SimpleRandom
{
    public static float Normal(float min, float max)
    {
        float x = Random.Range(0, 1f);
        float y = Random.Range(0, 1f);
        return ((x + y) / 2) * (max - min) + min;
    }

    public static int Normal(int min, int max)
    {
        return Mathf.RoundToInt(Normal((float)min, (float)max));
    }
}
