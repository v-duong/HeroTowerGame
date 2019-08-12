using UnityEngine;
using UnityEngine.Tilemaps;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }
    public int accumulatedConsumables;

    private BattleManager _battleManager;
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
    }
}