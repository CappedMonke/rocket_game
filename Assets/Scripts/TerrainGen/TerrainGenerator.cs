using System;
using System.Buffers;
using TreeEditor;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LightTransport;

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
    Stone
}

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
}

public sealed class TerrainGenerator
{
    readonly FastNoiseLite noise;
    readonly FastNoiseLite noise2;
    readonly TerrainGeneratorSettings s_settings;

    public TerrainGenerator(TerrainGeneratorSettings settings)
    {
        s_settings = settings;
        noise = new FastNoiseLite(s_settings.Seed);
        noise2 = new FastNoiseLite(s_settings.Seed + 1);
    }

    public Chunk GenerateChunk(Vector2Int ChunkPos)
    {
        BlockKind[] chunkData = new BlockKind[Chunk.ChunkArea];

        noise.SetNoiseType(FastNoiseLite.NoiseType.Value);
        noise2.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        var chunkBuffer = ArrayPool<float>.Shared.Rent(Chunk.ChunkArea);
        Span<float> chunkHardness = new Span<float>(chunkBuffer, 0, Chunk.ChunkArea);
        //Surface
        for (int x = 0; x < Chunk.ChunkSizeX; x++)
        {
            var worldX = x + ChunkPos.x * Chunk.ChunkSizeX;
            float seaLevel = noise2.GetNoise(worldX * 10, 0) * 10;
            float surfaceLevel = seaLevel + noise.GetNoise(worldX * 10, 0) * 32;
            for (int y = 0; y < Chunk.ChunkSizeY; y++)
            {
                var worldY = y + ChunkPos.y * Chunk.ChunkSizeY;
                if (worldY < surfaceLevel)
                {
                    chunkHardness[x + Chunk.ChunkSizeX * y] = noise.GetNoise(worldX * 3, worldY * 3) * 0.25f + 0.75f;
                }
                else
                {
                    chunkHardness[x + Chunk.ChunkSizeX * y] = 0;
                }
                float additional = noise2.GetNoise(worldX * 1, worldY * 1) * 0.5f + 0.5f;
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
                float falloff = Mathf.Clamp01(1000f - (worldY / 1000f));
                float erosion = noise2.GetNoise(worldX * 3, worldY * 3) * 0.5f + 0.5f;
                float caveNoise = noise.GetNoise(worldX * 4, worldY * 4);
                if (Mathf.Abs(caveNoise) < 0.2f || caveNoise > 0.7f)
                {
                    chunkHardness[x + Chunk.ChunkSizeX * y] -= (erosion * falloff + 0.2f);
                }
            }
        }

        //Translate noise map to blocks
        for (int i = 0; i < chunkHardness.Length; i++)
        {
            chunkData[i] = chunkHardness[i] > 0 ? BlockKind.Stone : BlockKind.Air;
        }

        //TODO: This is where is would put ore gen

        //TODO: This is where i would put surface detailing

        return new Chunk(chunkData);
    }
}