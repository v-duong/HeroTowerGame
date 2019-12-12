using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class HighlightMap : MonoBehaviour
{
    public Tilemap tilemap;

    [SerializeField]
    private TileBase highlightTile;

    private void Start()
    {
        Tilemap displayTiles = StageManager.Instance.DisplayMap;
        Tilemap obstacleTiles = StageManager.Instance.ObstacleMap;
        displayTiles.CompressBounds();
        StageManager.Instance.PathTilemap.ForEach(x => x.CompressBounds());
        obstacleTiles.CompressBounds();

        foreach (var pos in displayTiles.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            Vector3 position = displayTiles.CellToWorld(localPlace);
            if (displayTiles.HasTile(localPlace) && !obstacleTiles.HasTile(localPlace) && !StageManager.Instance.PathTilemap.Where(x=>x.HasTile(localPlace)).Any())
            {
                tilemap.SetTile(localPlace, highlightTile);
            }
        }
       
    }
}