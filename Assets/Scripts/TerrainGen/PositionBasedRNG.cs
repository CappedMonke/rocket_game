using UnityEngine;
using UnityEngine.InputSystem;

public enum RNGKind
{
    WhiteNoise,
    Smooth,
    Stepped
}

public static class PositionBasedRNG
{
    // Core hash function for determinism
    private static float Hash(int x, int y, int seed)
    {
        uint hash = (uint)(x * 73856093 ^ y * 19349663 ^ seed * 83492791);
        hash = (hash ^ (hash >> 13)) * 0x85ebca6b;
        hash = (hash ^ (hash >> 16)) * 0xc2b2ae35;
        return (hash & 0xFFFFFF) / (float)0xFFFFFF;
    }

    // Linear interpolation
    private static float Lerp(float a, float b, float t) => a + (b - a) * t;

    // Smooth interpolation (easing)
    private static float SmoothStep(float t) => t * t * (3f - 2f * t);

    public static float GetWhiteNoise(Vector2Int pos, int seed)
    {
        return Hash(pos.x, pos.y, seed);
    }

    public static float GetSteppedNoise(Vector2Int pos, int seed, int stepCount = 5)
    {
        float value = Hash(pos.x, pos.y, seed);
        return Mathf.Floor(value * stepCount) / (stepCount - 1f);
    }

    public static float GetSmoothNoise(Vector2 pos, int seed = 1337)
    {
        // Get fractional interpolation between 4 neighbors
        Vector2 basePos = new(Mathf.Floor(pos.x), Mathf.Floor(pos.y));
        float fx = pos.x - basePos.x;
        float fy = pos.y - basePos.y;

        float v00 = Hash((int)basePos.x, (int)basePos.y, seed);
        float v10 = Hash((int)basePos.x + 1, (int)basePos.y, seed);
        float v01 = Hash((int)basePos.x, (int)basePos.y + 1, seed);
        float v11 = Hash((int)basePos.x + 1, (int)basePos.y + 1, seed);

        fx = SmoothStep(fx);
        fy = SmoothStep(fy);

        float i1 = Lerp(v00, v10, fx);
        float i2 = Lerp(v01, v11, fx);
        return Lerp(i1, i2, fy);
    }
}
