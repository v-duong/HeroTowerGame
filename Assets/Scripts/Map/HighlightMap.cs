using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HighlightMap : MonoBehaviour
{
    public static HighlightMap Instance;
    public Tilemap tilemap;

    private void Awake()
    {
        Instance = this;
    }
}
