using System;
using System.Buffers;
using System.Threading;
using UnityEngine;

public struct TerrainGeneratorSettings
{
    public int Seed;

    public TerrainGeneratorSettings(int seed)
    {
        Seed = seed;
    }
}

[Serializable]
public enum BlockKind : int
{
    Air,
    Stone,
    Oxygenium,
    Kerosene,
    Gold,
}

[Serializable]
public sealed class Chunk
{
    public const int ChunkSizeX = 32;
    public const int ChunkSizeY = 32;
    public const int ChunkArea = ChunkSizeX * ChunkSizeY;
    public static Vector2Int ChunkSizes => new(ChunkSizeX, ChunkSizeY);
    public BlockKind[] Blocks;

    public Chunk(BlockKind[] blocks)
    {
        Blocks = blocks;
    }

    public BlockKind GetBlock(Vector2Int pos)
    {
        return Blocks[pos.y * ChunkSizeX + pos.x];
    }

    public void SetBlock(Vector2Int pos, BlockKind blockKind)
    {
        Blocks[pos.y * ChunkSizeX + pos.x] = blockKind;
    }
}

struct BiomeSettings
{
    public SamplingParams1D SeaLevel;
    public SamplingParams1D SurfaceLevel;
    public SamplingParams2D Hardness;
    public SamplingParams2D AdditionalHardness;
    public SamplingParams2D CaveNoise;
    public SamplingParams2D Erosion;

    public BiomeSettings(SamplingParams1D seaLevel,
                         SamplingParams1D surfaceLevel,
                         SamplingParams2D hardness,
                         SamplingParams2D additionalHardness,
                         SamplingParams2D caveNoise,
                         SamplingParams2D erosion)
    {
        SeaLevel = seaLevel;
        SurfaceLevel = surfaceLevel;
        Hardness = hardness;
        AdditionalHardness = additionalHardness;
        CaveNoise = caveNoise;
        Erosion = erosion;
    }

    public static BiomeSettings Lerp(BiomeSettings a, BiomeSettings b, float t)
    {
        return new BiomeSettings(
            SamplingParams1D.Lerp(a.SeaLevel, b.SeaLevel, t),
            SamplingParams1D.Lerp(a.SurfaceLevel, b.SurfaceLevel, t),
            SamplingParams2D.Lerp(a.Hardness, b.Hardness, t),
            SamplingParams2D.Lerp(a.AdditionalHardness, b.AdditionalHardness, t),
            SamplingParams2D.Lerp(a.CaveNoise, b.CaveNoise, t),
            SamplingParams2D.Lerp(a.Erosion, b.Erosion, t));
    }
}

struct SamplingParams1D
{
    public float SamplingScale;
    public float SamplingBias;
    public float AmplitudeScale;
    public float AmplitudeBias;

    public SamplingParams1D(float samplingScale, float samplingBias, float amplitudeScale, float amplitudeBias)
    {
        SamplingScale = samplingScale;
        SamplingBias = samplingBias;
        AmplitudeScale = amplitudeScale;
        AmplitudeBias = amplitudeBias;
    }

    public static SamplingParams1D Lerp(SamplingParams1D a, SamplingParams1D b, float t)
    {
        return new SamplingParams1D(
            Mathf.Lerp(a.SamplingScale, b.SamplingScale, t),
            Mathf.Lerp(a.SamplingBias, b.SamplingBias, t),
            Mathf.Lerp(a.AmplitudeScale, b.AmplitudeScale, t),
            Mathf.Lerp(a.AmplitudeBias, b.AmplitudeBias, t));
    }
}

struct SamplingParams2D
{
    public Vector2 SamplingScale;
    public Vector2 SamplingBias;
    public float AmplitudeScale;
    public float AmplitudeBias;

    public SamplingParams2D(Vector2 samplingScale, Vector2 samplingBias, float amplitudeScale, float amplitudeBias)
    {
        SamplingScale = samplingScale;
        SamplingBias = samplingBias;
        AmplitudeScale = amplitudeScale;
        AmplitudeBias = amplitudeBias;
    }

    public static SamplingParams2D Lerp(SamplingParams2D a, SamplingParams2D b, float t)
    {
        return new SamplingParams2D(
            Vector2.Lerp(a.SamplingScale, b.SamplingScale, t),
            Vector2.Lerp(a.SamplingBias, b.SamplingBias, t),
            Mathf.Lerp(a.AmplitudeScale, b.AmplitudeScale, t),
            Mathf.Lerp(a.AmplitudeBias, b.AmplitudeBias, t));
    }
}

public sealed class TerrainGenerator
{
    readonly BiomeSettings TestBiome = new BiomeSettings(
        new SamplingParams1D(10, 0, 10, 0),
        new SamplingParams1D(10, 0, 32, 0),
        new SamplingParams2D(new(3, 3), new(0, 0), 0.25f, 0.75f),
        new SamplingParams2D(new(1, 1), new(0, 0), 0.5f, 0.5f),
        new SamplingParams2D(new(4, 4), new(0, 0), 1, 0),
        new SamplingParams2D(new(3, 3), new(0, 0), 0.5f, 0.5f));

    readonly BiomeSettings TestBiomeFar = new BiomeSettings(
        new SamplingParams1D(5, 0, 10, 0),
        new SamplingParams1D(3, 0, 20, 0),
        new SamplingParams2D(new(3, 3), new(0, 0), 0.25f, 0.75f),
        new SamplingParams2D(new(1, 1), new(0, 0), 0.5f, 0.4f),
        new SamplingParams2D(new(4, 4), new(0, 0), 0.4f, 0),
        new SamplingParams2D(new(3, 3), new(0, 0), 0.5f, 0.5f));


    readonly ThreadLocal<FastNoiseLite> noise = new ThreadLocal<FastNoiseLite>(() =>
    {
        return new FastNoiseLite();
    });
    readonly ThreadLocal<FastNoiseLite> noise2 = new ThreadLocal<FastNoiseLite>(() =>
    {
        return new FastNoiseLite();
    });
    readonly TerrainGeneratorSettings _settings;

    public TerrainGenerator(TerrainGeneratorSettings settings)
    {
        _settings = settings;
    }

    BiomeSettings GetBiomeSettings(Vector2 samplePos)
    {
        var dist = samplePos.magnitude / 1500;
        //TODO: Interpolate with nearby biomes
        //TODO: Add more biome types
        return BiomeSettings.Lerp(TestBiome, TestBiomeFar, dist);
    }

    float GetNoise(FastNoiseLite noise, Vector2 samplePos, SamplingParams2D samplingParams)
    {

        return noise.GetNoise(
            samplePos.x * samplingParams.SamplingScale.x + samplingParams.SamplingBias.x,
            samplePos.y * samplingParams.SamplingScale.y + samplingParams.SamplingBias.y)
            * samplingParams.AmplitudeScale + samplingParams.AmplitudeBias;
    }

    float GetNoise(FastNoiseLite noise, Vector2 samplePos, SamplingParams1D samplingParams)
    {
        return noise.GetNoise(
            samplePos.x * samplingParams.SamplingScale + samplingParams.SamplingBias,
            samplePos.y * samplingParams.SamplingScale + samplingParams.SamplingBias)
            * samplingParams.AmplitudeScale + samplingParams.AmplitudeBias;
    }

    public Chunk GenerateChunk(Vector2Int ChunkPos)
    {
        BlockKind[] chunkData = new BlockKind[Chunk.ChunkArea];
        var noise = this.noise.Value;
        var noise2 = this.noise2.Value;

        noise.SetNoiseType(FastNoiseLite.NoiseType.Value);
        noise2.SetSeed(_settings.Seed);

        noise2.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        noise2.SetSeed(_settings.Seed + 1);
        var chunkBuffer = ArrayPool<float>.Shared.Rent(Chunk.ChunkArea);
        Span<float> chunkHardness = new(chunkBuffer, 0, Chunk.ChunkArea);
        //Surface
        for (int x = 0; x < Chunk.ChunkSizeX; x++)
        {
            var worldX = x + ChunkPos.x * Chunk.ChunkSizeX;
            var surfaceSamplePos = new Vector2(worldX, 0);
            var biomeSettings = GetBiomeSettings(surfaceSamplePos);

            float seaLevel = GetNoise(noise2, surfaceSamplePos, biomeSettings.SeaLevel);
            float surfaceLevel = seaLevel + GetNoise(noise, surfaceSamplePos, biomeSettings.SurfaceLevel);
            for (int y = 0; y < Chunk.ChunkSizeY; y++)
            {
                var worldY = y + ChunkPos.y * Chunk.ChunkSizeY;
                var samplePos = new Vector2(worldX, worldY);
                biomeSettings = GetBiomeSettings(samplePos);
                if (worldY < surfaceLevel)
                {
                    chunkHardness[x + Chunk.ChunkSizeX * y] = GetNoise(noise, samplePos, biomeSettings.Hardness);
                }
                else
                {
                    chunkHardness[x + Chunk.ChunkSizeX * y] = 0;
                }
                float additional = GetNoise(noise2, samplePos, biomeSettings.AdditionalHardness);
                float falloff = Mathf.Clamp01(1 - Mathf.Abs(worldY - surfaceLevel) / 30);
                chunkHardness[x + Chunk.ChunkSizeX * y] += additional * falloff;
            }
        }

        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        //Caves
        for (int y = 0; y < Chunk.ChunkSizeY; y++)
        {
            var worldY = y + ChunkPos.y * Chunk.ChunkSizeY;
            for (int x = 0; x < Chunk.ChunkSizeX; x++)
            {
                var worldX = x + ChunkPos.x * Chunk.ChunkSizeX;
                var samplePos = new Vector2(worldX, worldY);
                float falloff = Mathf.Clamp01(1000f - (worldY / 1000f));
                var biomeSettings = GetBiomeSettings(samplePos);

                float caveNoise = GetNoise(noise, samplePos, biomeSettings.CaveNoise);
                float erosion = GetNoise(noise2, samplePos, biomeSettings.Erosion);

                if (Mathf.Abs(caveNoise) < 0.2f || caveNoise > 0.7f)
                {
                    chunkHardness[x + Chunk.ChunkSizeX * y] -= (erosion * falloff + 0.4f);
                }
            }
        }

        //Translate noise map to blocks
        for (int i = 0; i < chunkHardness.Length; i++)
        {
            chunkData[i] = chunkHardness[i] > 0 ? BlockKind.Stone : BlockKind.Air;
        }

        noise2.SetSeed(_settings.Seed + 2);
        //TODO: This is where is would put ore gen
        for (int y = 0; y < Chunk.ChunkSizeY; y++)
        {
            var worldY = y + ChunkPos.y * Chunk.ChunkSizeY;
            for (int x = 0; x < Chunk.ChunkSizeX; x++)
            {
                var worldX = x + ChunkPos.x * Chunk.ChunkSizeX;
                float oreNoise = noise2.GetNoise(worldX * 10, worldY * 10);
                if (chunkData[x + Chunk.ChunkSizeX * y] == BlockKind.Stone && oreNoise > 0.9f)
                {
                    chunkData[x + Chunk.ChunkSizeX * y] = BlockKind.Oxygenium;
                }
            }
        }

        //TODO: This is where i would put surface detailing

        return new Chunk(chunkData);
    }
}