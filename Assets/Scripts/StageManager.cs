using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }
    public int accumulatedConsumables;

    private BattleManager _waveManager;
    private Tilemap _pathTilemap;
    private GameObject _worldCanvas;
    private HighlightMap _highlightMap;

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

    public BattleManager WaveManager
    {
        get
        {
            if (_waveManager == null)
                _waveManager = GameObject.FindGameObjectWithTag("WaveManager").GetComponent<BattleManager>();

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

    public HighlightMap HighlightMap
    {
        get
        {
            if (_highlightMap == null)
                _highlightMap = GameObject.FindGameObjectWithTag("HighlightMap").GetComponent<HighlightMap>();
            return _highlightMap;
        }
    }



    void Awake()
    {
        Instance = this;
    }
}
