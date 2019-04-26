using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapPath : MonoBehaviour {
    public static TilemapPath Instance;
    public List<Vector3> pathLocations = new List<Vector3>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Tilemap tilemap = this.gameObject.GetComponent<Tilemap>();
        tilemap.CompressBounds();
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            Vector3 position = tilemap.CellToWorld(localPlace);
            if (tilemap.HasTile(localPlace))
            {
                HighlightMap.Instance.tilemap.SetTile(localPlace, null);
            }
        }
    }

}
