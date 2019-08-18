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
    }

}
