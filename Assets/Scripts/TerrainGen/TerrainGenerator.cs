using System;
using UnityEngine;

public struct TerrainGeneratorSettings
{
    public int Seed;
    public int TerrainChunksWidth;
    public int TerrainChunksHeight;

    public TerrainGeneratorSettings(int seed, int terrainChunksWidth, int terrainChunksHeight)
    {
        Seed = seed;
        TerrainChunksWidth = terrainChunksWidth;
        TerrainChunksHeight = terrainChunksHeight;
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
    TerrainGeneratorSettings s_settings;
    public void Initalize(TerrainGeneratorSettings settings)
    {
        s_settings = settings;
    }

    public Chunk GenerateChunk(Vector2Int ChunkPos)
    {
        BlockKind[] chunkData = new BlockKind[Chunk.ChunkArea];
        float seaLevel = s_settings.TerrainChunksHeight * 0.5f * Chunk.ChunkSizeY;

        for (int y = 0; y < Chunk.ChunkSizeY; y++)
        {
            var worldY = y + ChunkPos.y * Chunk.ChunkSizeY;
            for (int x = 0; x < Chunk.ChunkSizeX; x++)
            {
                var worldX = x + ChunkPos.x * Chunk.ChunkSizeX;
                //TODO: this shit is so ass
                float surfaceLevel = PositionBasedRNG.GetSmoothNoise(new(worldX, 0), s_settings.Seed)
                * (s_settings.TerrainChunksHeight * 0.5f * Chunk.ChunkSizeY)
                + seaLevel;
                if (worldY < surfaceLevel)
                {
                    chunkData[x + Chunk.ChunkSizeX * y] = BlockKind.Stone;
                }
                else
                {
                    chunkData[x + Chunk.ChunkSizeX * y] = BlockKind.Air;
                }
            }
        }
        return new Chunk(chunkData);
    }
}
