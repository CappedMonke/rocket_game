using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;


[Serializable]
public sealed class BlockEntry
{
    public BlockKind BlockKind;
    public TileBase[] Tiles;
}
public sealed class TerrainManager : MonoSingleton<TerrainManager>
{
    public Tile MissingBlockTile;
    public BlockEntry[] BlockTiles;
    public int GlobalSeed = 1337;

    private Tilemap _tilemap;
    private TerrainGenerator _generator;
    private ConcurrentDictionary<BlockKind, TileBase[]> _blockMap = new();
    private ConcurrentDictionary<Vector2Int, Chunk> _loadedChunks = new();
    private ConcurrentQueue<GeneratedChunkInfo> _finishedChunks = new();
    private List<(Vector2 Pos, Vector2Int ChunksAround)> _loadedAreas = new();
    private List<Vector2Int> _chunksToUnload = new();
    private Vector2Int _loadedMax;
    private Vector2Int _loadedMin;

    private TileBase[] _emptyTiles;
    private ConcurrentDictionary<Vector2Int, Chunk> _generatedChunks = new();

    private void Awake()
    {
        _loadedMin = new Vector2Int(int.MaxValue, int.MaxValue);
        _loadedMax = new Vector2Int(int.MinValue, int.MinValue);
        _emptyTiles = new TileBase[Chunk.ChunkArea];
        //I hate unity so fucking much bro..
        foreach (var entry in BlockTiles)
        {
            _blockMap[entry.BlockKind] = entry.Tiles;
        }
        _generator = new TerrainGenerator(new TerrainGeneratorSettings(GlobalSeed));
        _tilemap = GetComponent<Tilemap>();
    }

    private void LateUpdate()
    {
        foreach (var entry in _loadedChunks)
        {
            var chunkCoord = entry.Key;
            bool isLoaded = false;
            for (int i = 0; i < _loadedAreas.Count; i++)
            {
                (Vector2 position, Vector2Int chunksAround) = _loadedAreas[i];
                Vector2Int centerChunk = GetChunkPos((Vector2Int)_tilemap.WorldToCell(new Vector3(position.x, position.y, 1)));
                var pos = new Vector2Int(centerChunk.x - chunksAround.x - 2, centerChunk.y - chunksAround.y - 2);
                var size = new Vector2Int((chunksAround.x + 2) * 2, (chunksAround.y + 2) * 2);
                var visibleBounds = new RectInt(pos, size);
                if (visibleBounds.Contains(chunkCoord))
                {
                    isLoaded = true;
                    break;
                }
            }
            if (!isLoaded)
            {
                _chunksToUnload.Add(chunkCoord);
            }
        }
        foreach (var chunkCoord in _chunksToUnload)
        {
            _loadedChunks.Remove(chunkCoord, out var chunk);
            BoundsInt bounds = GetBoundsFromChunkCoord(chunkCoord);
            _tilemap.SetTilesBlock(bounds, _emptyTiles);
            SaveChunk(chunkCoord, chunk);
        }
        _loadedMin = new Vector2Int(int.MaxValue, int.MaxValue);
        _loadedMax = new Vector2Int(int.MinValue, int.MinValue);
        _loadedAreas.Clear();
        _chunksToUnload.Clear();
    }

    private void SaveChunk(Vector2Int chunkPos, Chunk chunk)
    {
        _generatedChunks.AddOrUpdate(chunkPos, _ => { return chunk; }, (_, _) => { return chunk; });
    }

    /// <summary>
    /// Loads an area around the specified position
    /// </summary>
    /// <param name="position">Position to load around (in world space)</param>
    /// <param name="chunksAround">How large is the loaded area, in chunks</param>
    public void LoadArea(Vector2 position, Vector2Int chunksAround)
    {
        _loadedAreas.Add((position, chunksAround));
        Vector2Int centerChunk = GetChunkPos((Vector2Int)_tilemap.WorldToCell(new Vector3(position.x, position.y, 1)));
        _loadedMin = Vector2Int.Min(centerChunk - chunksAround - new Vector2Int(2, 2), _loadedMin);
        _loadedMax = Vector2Int.Max(centerChunk + chunksAround + new Vector2Int(2, 2), _loadedMax);
        Parallel.For(-chunksAround.y - 1, chunksAround.y + 1, (y) =>
        {
            for (int x = -chunksAround.x - 1; x < chunksAround.x + 1; x++)
            {
                var chunkCoord = new Vector2Int(centerChunk.x + x, centerChunk.y + y);

                if (_loadedChunks.ContainsKey(chunkCoord))
                {
                    continue; // Already loaded
                }
                LoadChunk(chunkCoord);
            }
        });


        while (_finishedChunks.TryDequeue(out var result))
        {
            BoundsInt bounds = GetBoundsFromChunkCoord(result.ChunkCoord);
            _tilemap.SetTilesBlock(bounds, result.Tiles);
            ArrayPool<TileBase>.Shared.Return(result.Tiles, true);
            _loadedChunks.TryAdd(result.ChunkCoord, result.Chunk);
        }
    }

    private static readonly ThreadLocal<System.Random> threadLocalRandom = new(static () =>
    {
        return new System.Random(unchecked(Environment.TickCount * 31 + Environment.CurrentManagedThreadId));
    });

    public BlockKind GetBlockKind(Vector2Int worldPos)
    {
        var chunkPos = GetChunkPos(worldPos);
        var tilePos = GetTileOffset(worldPos);
        if (!_loadedChunks.TryGetValue(chunkPos, out var chunk))
        {
            return BlockKind.Air;
        }
        return chunk.GetBlock(tilePos);
    }

    public void SetBlockKind(Vector2Int worldPos, BlockKind blockKind)
    {
        var chunkPos = GetChunkPos(worldPos);
        var tilePos = GetTileOffset(worldPos);
        if (!_loadedChunks.TryGetValue(chunkPos, out var chunk))
        {
            Debug.LogWarning($"Chunk not loaded {chunkPos}");
            return;
        }
        chunk.SetBlock(tilePos, blockKind);
        _tilemap.SetTile(new Vector3Int(worldPos.x, worldPos.y, 0), GetTileForBlock(blockKind));
    }

    public TileBase GetTileForBlock(BlockKind blockKind)
    {
        if (_blockMap.TryGetValue(blockKind, out var blocks))
        {
            if (blocks != null && blocks.Length > 0)
            {
                return blocks[threadLocalRandom.Value.Next(0, blocks.Length)];
            }
        }

        return MissingBlockTile;
    }

    private void LoadChunk(Vector2Int chunkCoord)
    {
        var tiles = ArrayPool<TileBase>.Shared.Rent(Chunk.ChunkArea);
        if (_generatedChunks.TryGetValue(chunkCoord, out var savedChunk))
        {
            for (int y = 0; y < Chunk.ChunkSizeY; y++)
            {
                for (int x = 0; x < Chunk.ChunkSizeX; x++)
                {
                    var block = savedChunk.Blocks[x + y * Chunk.ChunkSizeX];

                    tiles[x + y * Chunk.ChunkSizeX] = GetTileForBlock(block);
                }
            }
            _finishedChunks.Enqueue(new(chunkCoord, savedChunk, tiles));
            return;
        }

        Chunk chunk = _generator.GenerateChunk(chunkCoord);
        for (int y = 0; y < Chunk.ChunkSizeY; y++)
        {
            for (int x = 0; x < Chunk.ChunkSizeX; x++)
            {
                var block = chunk.Blocks[x + y * Chunk.ChunkSizeX];

                tiles[x + y * Chunk.ChunkSizeX] = GetTileForBlock(block);
            }
        }
        _finishedChunks.Enqueue(new(chunkCoord, chunk, tiles));
    }

    public Vector2Int WorldToTile(Vector2 worldPos)
    {
        return (Vector2Int)_tilemap.WorldToCell(worldPos);
    }

    public static BoundsInt GetBoundsFromChunkCoord(Vector2Int chunkCoord)
    {
        var min = new Vector3Int(chunkCoord.x * Chunk.ChunkSizeX, chunkCoord.y * Chunk.ChunkSizeY, 0);
        var size = new Vector3Int(Chunk.ChunkSizeX, Chunk.ChunkSizeY, 1);
        var bounds = new BoundsInt(min, size);
        return bounds;
    }

    public static Vector2Int GetChunkPos(Vector2Int tilePosition)
    {
        int subX = tilePosition.x < 0 ? -1 : 0;
        int subY = tilePosition.y < 0 ? -1 : 0;
        return new Vector2Int(
            Mathf.FloorToInt(tilePosition.x / Chunk.ChunkSizeX) + subX,
            Mathf.FloorToInt(tilePosition.y / Chunk.ChunkSizeY) + subY
        );
    }

    public static Vector2Int GetTileOffset(Vector2Int tilePosition)
    {
        var chunkPos = GetChunkPos(tilePosition);
        // (-2,-4) -> (-1, -1) , (-32, -32) - (-2, -4)
        var pos = chunkPos * Chunk.ChunkSizes - tilePosition;
        tilePosition = new Vector2Int(Math.Abs(pos.x), Math.Abs(pos.y));

        return tilePosition;
    }
}

internal struct GeneratedChunkInfo
{
    public Vector2Int ChunkCoord;
    public Chunk Chunk;
    public TileBase[] Tiles;

    public GeneratedChunkInfo(Vector2Int chunkCoord, Chunk chunk, TileBase[] tiles)
    {
        ChunkCoord = chunkCoord;
        Chunk = chunk;
        Tiles = tiles;
    }

    public override bool Equals(object obj)
    {
        return obj is GeneratedChunkInfo other &&
               ChunkCoord.Equals(other.ChunkCoord) &&
               EqualityComparer<Chunk>.Default.Equals(Chunk, other.Chunk) &&
               EqualityComparer<TileBase[]>.Default.Equals(Tiles, other.Tiles);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(ChunkCoord, Chunk, Tiles);
    }

    public readonly void Deconstruct(out Vector2Int item1, out Chunk item2, out TileBase[] item4)
    {
        item1 = ChunkCoord;
        item2 = Chunk;
        item4 = Tiles;
    }

    public static implicit operator (Vector2Int, Chunk, TileBase[])(GeneratedChunkInfo value)
    {
        return (value.ChunkCoord, value.Chunk, value.Tiles);
    }

    public static implicit operator GeneratedChunkInfo((Vector2Int, Chunk, TileBase[]) value)
    {
        return new GeneratedChunkInfo(value.Item1, value.Item2, value.Item3);
    }
}