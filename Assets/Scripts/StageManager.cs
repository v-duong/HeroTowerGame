using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    public Tilemap PathTilemap
    {
        get
        {
            if (m_pathTilemap == null)
            {
                m_pathTilemap = GameObject.FindGameObjectWithTag("PathMap").GetComponent<Tilemap>();
            }
            return m_pathTilemap;
        }
    }

    public WaveManager WaveManager
    {
        get
        {
            if (m_waveManager == null)
                m_waveManager = GameObject.FindGameObjectWithTag("WaveManager").GetComponent<WaveManager>();

            return m_waveManager;
        }
    }
    public GameObject WorldCanvas
    {
        get
        {
            if (m_worldCanvas == null)
                m_worldCanvas = GameObject.Find("WorldCanvas");
            return m_worldCanvas;
        }
    }
    private WaveManager m_waveManager;
    private Tilemap m_pathTilemap;
    private GameObject m_worldCanvas;

    void Awake()
    {
        Instance = this;
    }
}
