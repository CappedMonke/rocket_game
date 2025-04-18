using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Unity.Collections.AllocatorManager;


[Serializable]
public sealed class BlockEntry
{
    public BlockKind blockKind;
    public TileBase[] tiles;
}
public sealed class TerrainManager : MonoSingleton<TerrainManager>
{
    public Tile MissingBlockTile;
    public BlockEntry[] BlockTiles;

    private Tilemap _tilemap;
    private TerrainGenerator _generator;
    private Dictionary<BlockKind, TileBase[]> _blockMap = new();
    private Dictionary<Vector2Int, Chunk> _loadedChunks = new();


    private void Awake()
    {
        //I hate unity so fucking much bro..
        foreach (var entry in BlockTiles)
        {
            _blockMap[entry.blockKind] = entry.tiles;
        }
        _generator = new TerrainGenerator();
        _tilemap = GetComponent<Tilemap>();
        _generator.Initalize(new TerrainGeneratorSettings(1337, 10, 3));
    }

    //TODO: this is just temp, call this in the playermanager
    private void Update()
    {
        LoadArea(Vector2.zero, Vector2Int.CeilToInt(Vector2.one * Camera.main.orthographicSize));
    }

    /// <summary>
    /// Loads an area around the specified position
    /// </summary>
    /// <param name="position">Position to load around (in world space)</param>
    /// <param name="chunksAround">How large is the loaded area, in chunks</param>
    public void LoadArea(Vector2 position, Vector2Int chunksAround)
    {
        Vector2Int centerChunk = WorldToChunk(position);
        for (int y = -chunksAround.y; y <= chunksAround.y; y++)
        {
            for (int x = -chunksAround.x; x <= chunksAround.x; x++)
            {
                Vector2Int chunkCoord = new Vector2Int(centerChunk.x + x, centerChunk.y + y);

                if (_loadedChunks.ContainsKey(chunkCoord))
                    continue; // Already loaded
                Debug.Log($"Generationg chunk {chunkCoord}");
                _loadedChunks.Add(chunkCoord, LoadChunk(chunkCoord));
            }
        }
    }

    private TileBase GetTileForBlock(BlockKind blockKind)
    {
        if (_blockMap.TryGetValue(blockKind, out var blocks))
        {
            if (blocks != null && blocks.Length > 0)
            {
                return blocks[UnityEngine.Random.Range(0, blocks.Length)];
            }
        }

        return MissingBlockTile;
    }

    private Chunk LoadChunk(Vector2Int chunkCoord)
    {
        Chunk chunk = _generator.GenerateChunk(chunkCoord);

        for (int y = 0; y < Chunk.ChunkSizeY; y++)
        {
            for (int x = 0; x < Chunk.ChunkSizeX; x++)
            {
                var block = chunk.Blocks[x + y * Chunk.ChunkSizeX];

                var tilePos = new Vector2Int(x, y) + chunkCoord * Chunk.ChunkSizes;
                _tilemap.SetTile((Vector3Int)tilePos, GetTileForBlock(block));
            }
        }
        return chunk;
    }

    private static Vector2Int WorldToChunk(Vector2 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / Chunk.ChunkSizeX),
            Mathf.FloorToInt(position.y / Chunk.ChunkSizeY)
        );
    }
}

