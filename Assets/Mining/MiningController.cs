using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MiningController : MonoBehaviour
{
    public float miningSpeed = 1.0f;

    private Vector3Int _currentMiningTarget;
    private Tilemap _tilemap;
    private float _miningTimer = 0.0f;


    void Start()
    {
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

        if (targetTilePos == _currentMiningTarget)
        {
            _miningTimer += Time.deltaTime * miningSpeed;

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
            Debug.Log("Retrieved: 1x" + tile.name);
            _tilemap.SetTile(adjustedPos, null);
        }
    }

    private void ResetMining()
    {
        _miningTimer = 0.0f;
    }
}