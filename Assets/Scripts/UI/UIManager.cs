using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public static int MenuBarSize = 60;
    private Canvas _invCanvas;
    private Canvas _heroCanvas;
    private Canvas _archetypeCanvas;
    private ScrollRect _invWindowRect;
    private EquipmentDetailWindow _itemWindow;
    private InventoryScrollWindow _invWindow;
    private HeroDetailWindow _heroWindow;
    private HeroScrollWindow _heroScrollWindow;
    private ScrollRect _heroWindowRect;
    private ArchetypeUITreeWindow _archetypeTreeWindow;
    private ArchetypeNodeInfoPanel _archetypeNodeInfoPanel;
    public readonly Vector2 referenceResolution = new Vector2(480, 854);
    public readonly Vector2 fullWindowSize = new Vector2(480, 854 - MenuBarSize);
    public readonly Vector2 itemWindowSize = new Vector2(400, 640);
    public EquipSlotType SlotContext;
    public bool IsEquipSelectMode = false;
    public GameObject currentWindow;
    public Stack<GameObject> previousWindows = new Stack<GameObject>();

    public void OpenWindow(GameObject window)
    {
        if (currentWindow != null)
        {
            previousWindows.Push(currentWindow);
            currentWindow.SetActive(false);
        }
        currentWindow = window;
        window.SetActive(true);
    }

    public void CloseCurrentWindow()
    {
        currentWindow.SetActive(false);
        if (previousWindows.Count > 0)
        {
            currentWindow = previousWindows.Pop();
            currentWindow.SetActive(true);
        }
    }

    public Canvas InvCanvas
    {
        get
        {
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

    public Canvas ArchetypeCanvas
    {
        get
        {
            if (_archetypeCanvas == null)
                _archetypeCanvas = GameObject.FindGameObjectWithTag("ArchetypeCanvas").GetComponent<Canvas>();
            return _archetypeCanvas;
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
        get
        {
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

    public ArchetypeUITreeWindow ArchetypeTreeWindow
    {
        get
        {
            if (_archetypeTreeWindow == null)
                _archetypeTreeWindow = ArchetypeCanvas.GetComponentInChildren<ArchetypeUITreeWindow>(true);
            return _archetypeTreeWindow;
        }
    }

    public ArchetypeNodeInfoPanel ArchetypeNodeInfoPanel
    {
        get
        {
            if (_archetypeNodeInfoPanel == null)
                _archetypeNodeInfoPanel = ArchetypeCanvas.GetComponentInChildren<ArchetypeNodeInfoPanel>(true);
            return _archetypeNodeInfoPanel;
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
        previousWindows.Clear();
        currentWindow = null;
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

    public void OpenInventoryWindow(bool closeWindows = true)
    {
        GameObject target = InvWindowRect.gameObject;
        if (closeWindows)
            CloseAllWindows();
        //SetInventoryScrollRectTransform(0);
        this.EquipDetailWindow.HideButtons();
        OpenWindow(target);
        InvScrollContent.ShowAllEquipment();
    }

    public void SetInventoryScrollRectTransform(int type)
    {
        if (type == 0)
        {
            InvWindowRect.GetComponent<RectTransform>().sizeDelta = fullWindowSize;
        }
    }
}