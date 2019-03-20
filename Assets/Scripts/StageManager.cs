using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }
    public int accumulatedConsumables;

    public Tilemap PathTilemap
    {
        get
        {
            if (_pathTilemap == null)
            {
                _pathTilemap = GameObject.FindGameObjectWithTag("PathMap").GetComponent<Tilemap>();
            }
            return _pathTilemap;
        }
    }

    public WaveManager WaveManager
    {
        get
        {
            if (_waveManager == null)
                _waveManager = GameObject.FindGameObjectWithTag("WaveManager").GetComponent<WaveManager>();

            return _waveManager;
        }
    }
    public GameObject WorldCanvas
    {
        get
        {
            if (_worldCanvas == null)
                _worldCanvas = GameObject.Find("WorldCanvas");
            return _worldCanvas;
        }
    }
    private WaveManager _waveManager;
    private Tilemap _pathTilemap;
    private GameObject _worldCanvas;

    void Awake()
    {
        Instance = this;
    }
}
