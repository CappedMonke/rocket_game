using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class MiningController : MonoBehaviour
{
    public float miningSpeed = 1.0f;
    public float maxMiningDistance = 2.0f;
    public AudioClip miningSound;
    [SerializeField] private AudioSource audioSource;

    private Vector2Int _currentMiningTarget;
    private Tilemap _tilemap;
    private float _miningTimer = 0.0f;
    private Inventory _inventory;

    void Start()
    {
        _inventory = FindAnyObjectByType<Inventory>();
        _tilemap = FindAnyObjectByType<Tilemap>();
    }

    void Update()
    {
        if (IsMiningInputHeld())
        {
            HandleMining();
        }
        else
        {
            ResetMining();
        }
    }

    private bool IsMiningInputHeld()
    {
        return Input.GetMouseButton(0);
    }

    private void HandleMining()
    {
        Vector2Int targetTilePos = GetTargetedTilePos();
        Vector2Int playerTilePos = TerrainManager.Instance.WorldToTile(transform.position);
        float distance = Vector2Int.Distance(targetTilePos, playerTilePos);

        if (distance > maxMiningDistance)
        {
            ResetMining();
            return;
        }

        if (targetTilePos == _currentMiningTarget)
        {
            _miningTimer += Time.deltaTime * miningSpeed;

            float progress = Mathf.Clamp01(_miningTimer / 1.0f);  // Make this dependant on actual Tile being mined 
            HighlightMiningTile(targetTilePos, progress);

            if (_miningTimer >= 1.0f) // Make this dependant on actual Tile being mined
            {
                MineTileIfExists(targetTilePos);
                ResetMining();
            }
        }
        else
        {
            ResetMining();
            _currentMiningTarget = targetTilePos;
            _miningTimer = 0.0f;
        }
    }

    private Vector2Int GetTargetedTilePos()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return TerrainManager.Instance.WorldToTile(mouseWorldPos);
    }

    private void MineTileIfExists(Vector2Int tilePos)
    {
        var block = TerrainManager.Instance.GetBlockKind(tilePos);
        if (block != BlockKind.Air)
        {
            _inventory.AddItemByBlockKind(block);
            audioSource.PlayOneShot(miningSound);
            TerrainManager.Instance.SetBlockKind(tilePos, BlockKind.Air);
        }
    }

    // Currently just color highlighting, eventually replace with something pretty (animation?)
    private void HighlightMiningTile(Vector2Int tilePos, float progress)
    {
        Vector3Int adjustedPos = new(tilePos.x, tilePos.y, 0);
        TileBase tile = _tilemap.GetTile(adjustedPos);
        if (tile != null)
        {
            _tilemap.SetColor(adjustedPos, Color.Lerp(Color.blue, Color.gray, progress));
            if (progress >= 0.95f)
            {
                _tilemap.SetColor(adjustedPos, Color.white);
            }
        }
    }

    private void ResetMining()
    {
        _miningTimer = 0.0f;
        if (_tilemap != null && TerrainManager.Instance.GetBlockKind(_currentMiningTarget) != BlockKind.Air)
        {
            _tilemap.SetColor(new Vector3Int(_currentMiningTarget.x, _currentMiningTarget.y, 0), Color.white);
        }
    }
}