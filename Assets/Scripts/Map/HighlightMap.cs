using UnityEngine;
using UnityEngine.Tilemaps;

public class HighlightMap : MonoBehaviour
{
    public Tilemap tilemap;

    [SerializeField]
    private TileBase highlightTile;

    private void Start()
    {
        Tilemap pathTiles = StageManager.Instance.PathTilemap;
        Tilemap displayTiles = StageManager.Instance.DisplayMap;
        Tilemap obstacleTiles = StageManager.Instance.ObstacleMap;
        displayTiles.CompressBounds();
        pathTiles.CompressBounds();
        obstacleTiles.CompressBounds();

        foreach (var pos in displayTiles.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            Vector3 position = displayTiles.CellToWorld(localPlace);
            if (displayTiles.HasTile(localPlace) && !pathTiles.HasTile(localPlace) && !obstacleTiles.HasTile(localPlace))
            {
                Debug.Log("TEST");
                tilemap.SetTile(localPlace, highlightTile);
            }
        }
       
    }
}