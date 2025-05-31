using UnityEngine;
using UnityEngine.Tilemaps;

public class MiningController : MonoBehaviour
{
    public float miningSpeed = 1.0f;
    public Tilemap tilemap;

    private Vector2Int _lookDirection;
    private Rigidbody2D _rb;
    private Vector3Int _currentMiningTarget;
    private float _miningTimer = 0.0f;


    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A)) _lookDirection = Vector2Int.left;
        if (Input.GetKey(KeyCode.D)) _lookDirection = Vector2Int.right;
        if (Input.GetKey(KeyCode.W)) _lookDirection = Vector2Int.up;
        if (Input.GetKey(KeyCode.S)) _lookDirection = Vector2Int.down;

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
        return tilemap.WorldToCell(mouseWorldPos);
    }

    private void MineTileIfExists(Vector3Int position)
    {
        TileBase tile = tilemap.GetTile(position);
        if (tile != null)
        {
            Debug.Log("Retrieved: 1x" + tile.name);
            tilemap.SetTile(position, null);
        }
    }

    private void ResetMining()
    {
        _miningTimer = 0.0f;
    }
}