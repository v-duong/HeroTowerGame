using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }
    public int accumulatedConsumables;
    public Bounds stageBounds;

    private BattleManager _battleManager;
    private List<Tilemap> _pathTilemaps = new List<Tilemap>();
    private Tilemap _displayMap;
    private GameObject _worldCanvas;
    private HighlightMap _highlightMap;
    private Tilemap _obstacleMap;

    public List<Tilemap> PathTilemap
    {
        get
        {
            if (_pathTilemaps.Count ==0 || (_pathTilemaps.Count > 0 && _pathTilemaps[0] == null))
            {
                _pathTilemaps.Clear();
                GameObject.FindGameObjectsWithTag("PathMap").ToList().ForEach(x => _pathTilemaps.Add(x.GetComponent<Tilemap>()));
                if (_pathTilemaps.Count == 0)
                    _pathTilemaps.Add(GameObject.FindGameObjectWithTag("DisplayMap").GetComponent<Tilemap>());
            }
            return _pathTilemaps;
        }
    }

    public Tilemap ObstacleMap
    {
        get
        {
            if (_obstacleMap == null)
            {
                _obstacleMap = GameObject.FindGameObjectWithTag("ObstacleMap").GetComponent<Tilemap>();
            }
            return _obstacleMap;
        }
    }

    public Tilemap DisplayMap
    {
        get
        {
            if (_displayMap == null)
            {
                _displayMap = GameObject.FindGameObjectWithTag("DisplayMap").GetComponent<Tilemap>();
            }
            return _displayMap;
        }
    }

    public BattleManager BattleManager
    {
        get
        {
            if (_battleManager == null)
                _battleManager = GameObject.FindGameObjectWithTag("WaveManager").GetComponent<BattleManager>();

            return _battleManager;
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

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void InitalizeStage()
    {
        DisplayMap.CompressBounds();
        Bounds bounds = Instance.DisplayMap.localBounds;
        stageBounds = bounds;
        InputManager.Instance.SetCameraBounds();
        WorldCanvas.GetComponent<Canvas>().worldCamera = Camera.main;
        HighlightMap.gameObject.SetActive(false);
        BattleManager.Initialize();
    }
}