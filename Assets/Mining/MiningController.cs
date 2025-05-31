using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MiningController : MonoBehaviour
{
    public float miningSpeed = 1.0f;
    public float maxMiningDistance = 2.0f;

    private Vector3Int _currentMiningTarget;
    private Tilemap _tilemap;
    private float _miningTimer = 0.0f;
    private Inventory _inventory;


    void Start()
    {
        _inventory = GetComponent<Inventory>();
        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart()
    {
        yield return null;
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
        Vector3Int targetTilePos = GetTargetTilePositionByMouse();
        Vector3Int playerTilePos = _tilemap.WorldToCell(transform.position);
        float distance = Vector3Int.Distance(targetTilePos, playerTilePos);

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
            _currentMiningTarget = targetTilePos;
            _miningTimer = 0.0f;
        }
    }

    private Vector3Int GetTargetTilePositionByMouse()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return _tilemap.WorldToCell(mouseWorldPos);
    }

    private void MineTileIfExists(Vector3Int position)
    {
        Vector3Int adjustedPos = new Vector3Int(position.x, position.y, 0);
        TileBase tile = _tilemap.GetTile(adjustedPos);
        if (tile != null)
        {
            _tilemap.SetTile(adjustedPos, null);
            _inventory.AddItemByTileName(tile.name);
        }
    }

    // Currently just color highlighting, eventually replace with something pretty (animation?)
    private void HighlightMiningTile(Vector3Int tilePos, float progress)
    {
        Vector3Int adjustedPos = new Vector3Int(tilePos.x, tilePos.y, 0);
        TileBase tile = _tilemap.GetTile(adjustedPos);
        if (tile != null)
        {
            _tilemap.SetColor(tilePos, Color.Lerp(Color.blue, Color.gray, progress));
            if (progress >= 0.95f)
            {
                _tilemap.SetColor(adjustedPos, Color.white);
            }
        }
    }

    private void ResetMining()
    {
        _miningTimer = 0.0f;
        if (_tilemap != null && _tilemap.GetTile(_currentMiningTarget) != null)
        {
            _tilemap.SetColor(_currentMiningTarget, Color.white);
        }
    }
}