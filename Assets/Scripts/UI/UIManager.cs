﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public static UIManager Instance { get; private set; }
    private Canvas _invCanvas;
    private Canvas _heroCanvas;
    private ScrollRect _invWindowRect;
    private EquipmentDetailWindow _itemWindow;
    private InventoryScrollWindow _invWindow;
    private HeroDetailWindow _heroWindow;
    private HeroScrollWindow _heroScrollWindow;
    private ScrollRect _heroWindowRect;

    public Canvas currentActive;


    public Canvas InvCanvas {
        get {
            if (_invCanvas == null)
                _invCanvas = GameObject.FindGameObjectWithTag("InventoryCanvas").GetComponent<Canvas>();
            return _invCanvas;
        }
    }

    public Canvas HeroCanvas
    {
        get
        {
            if (_heroCanvas == null)
                _heroCanvas = GameObject.FindGameObjectWithTag("HeroCanvas").GetComponent<Canvas>();
            return _heroCanvas;
        }
    }

    public ScrollRect InvWindowRect
    {
        get
        {
            if (_invWindowRect == null)
                _invWindowRect = InvCanvas.GetComponentInChildren<ScrollRect>(true);
            return _invWindowRect;
        }
    }

    public InventoryScrollWindow InvScrollContent
    {
        get
        {
            if (_invWindow == null)
                _invWindow = InvCanvas.GetComponentInChildren<InventoryScrollWindow>(true);
            return _invWindow;
        }
    }

    public EquipmentDetailWindow EquipDetailWindow
    {
        get {
            if (_itemWindow == null)
                _itemWindow = InvCanvas.GetComponentInChildren<EquipmentDetailWindow>(true);
            return _itemWindow;
        }
    }

    public HeroDetailWindow HeroDetailWindow
    {
        get
        {
            if (_heroWindow == null)
                _heroWindow = HeroCanvas.GetComponentInChildren<HeroDetailWindow>(true);
            return _heroWindow;
        }
    }

    public HeroScrollWindow HeroScrollContent
    {
        get
        {
            if (_heroScrollWindow == null)
                _heroScrollWindow = HeroCanvas.GetComponentInChildren<HeroScrollWindow>(true);
            return _heroScrollWindow;
        }
    }

    public ScrollRect HeroWindowRect
    {
        get
        {
            if (_heroWindowRect == null)
                _heroWindowRect = HeroCanvas.GetComponentInChildren<ScrollRect>(true);
            return _heroWindowRect;
        }
    }

    private void Start()
    {
        Instance = this;
    }

    public void CloseAllWindows()
    {
        CloseInventoryWindows();
        CloseHeroWindows();
    }

    public void CloseInventoryWindows()
    {
        InvWindowRect.gameObject.SetActive(false);
        EquipDetailWindow.gameObject.SetActive(false);
    }

    public void CloseHeroWindows()
    {
        HeroWindowRect.gameObject.SetActive(false);
        HeroDetailWindow.gameObject.SetActive(false);
    }

}
