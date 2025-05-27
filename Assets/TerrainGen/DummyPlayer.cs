using UnityEngine;

public class DummyPlayer : MonoBehaviour
{
    //TODO: this is just temp, call this in the playermanager
    private void Update()
    {
        TerrainManager.Instance.LoadArea(transform.position, Vector2Int.CeilToInt(Vector2.one * Camera.main.orthographicSize / Chunk.ChunkSizes));
    }
}
