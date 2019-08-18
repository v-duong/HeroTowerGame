using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public static int MenuBarSize = 60;
    public readonly Vector2 referenceResolution = new Vector2(480, 854);
    public readonly Vector2 fullWindowSize = new Vector2(480, 854 - MenuBarSize);
    public readonly Vector2 itemWindowSize = new Vector2(400, 640);

    private Canvas _invCanvas;
    private InvWindowCanvas _invWindowCanvas;
    private Canvas _heroCanvas;
    private Canvas _archetypeCanvas;
    private Canvas _battleUICanvas;
    private Canvas _teamCanvas;
    private Canvas _workshopCanvas;
    private Canvas _stageSelectCanvas;
    private ScrollRect _invWindowRect;
    private EquipmentDetailWindow _itemWindow;
    private InventoryScrollWindow _invWindow;
    private HeroDetailWindow _heroWindow;
    private HeroScrollWindow _heroScrollWindow;
    private ScrollRect _heroWindowRect;
    private ArchetypeNodeInfoPanel _archetypeNodeInfoPanel;
    private ArchetypeUITreeWindow _archetypeUITreeWindow;
    private LoadingScript _loadingScreen;
    private SummonScrollWindow _summonScrollWindow;
    private BattleCharInfoPanel _battleCharInfoPanel;
    private ItemCategoryPanel _itemCategoryPanel;
    private TeamWindow _teamWindow;
    private WorkshopParentWindow _workshopParentWindow;
    private ItemCraftingPanel _itemCraftingPanel;
    private StageSelectPanel _stageSelectPanel;

    public EquipSlotType SlotContext;
    public bool IsEquipSelectMode = false;
    public GameObject currentWindow;
    public Stack<GameObject> previousWindows = new Stack<GameObject>();

    public static HeroData selectedHero;

    public void OpenWindow(GameObject window, bool closePrevious = true)
    {
        if (currentWindow != null)
        {
            previousWindows.Push(currentWindow);
            if (closePrevious)
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

    public InvWindowCanvas InvWindowCanvas
    {
        get
        {
            if (_invWindowCanvas == null)
                _invWindowCanvas = InvCanvas.GetComponentInChildren<InvWindowCanvas>(true);
            return _invWindowCanvas;
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

    public Canvas WorkshopCanvas
    {
        get
        {
            if (_workshopCanvas == null)
                _workshopCanvas = GameObject.FindGameObjectWithTag("WorkshopCanvas").GetComponent<Canvas>();
            return _workshopCanvas;
        }
    }

    public Canvas TeamCanvas
    {
        get
        {
            if (_teamCanvas == null)
                _teamCanvas = GameObject.FindGameObjectWithTag("TeamCanvas").GetComponent<Canvas>();
            return _teamCanvas;
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

    public Canvas BattleUICanvas
    {
        get
        {
            if (_battleUICanvas == null)
                _battleUICanvas = GameObject.FindGameObjectWithTag("BattleUICanvas").GetComponent<Canvas>();
            return _battleUICanvas;
        }
    }

    public Canvas StageSelectCanvas
    {
        get
        {
            if (_stageSelectCanvas == null)
                _stageSelectCanvas = GameObject.FindGameObjectWithTag("StageSelectCanvas").GetComponent<Canvas>();
            return _stageSelectCanvas;
        }
    }

    private ScrollRect InvWindowRect
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

    public ItemCategoryPanel ItemCategoryPanel
    {
        get
        {
            if (_itemCategoryPanel == null)
                _itemCategoryPanel = InvCanvas.GetComponentInChildren<ItemCategoryPanel>(true);
            return _itemCategoryPanel;
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

    private ScrollRect HeroWindowRect
    {
        get
        {
            if (_heroWindowRect == null)
                _heroWindowRect = HeroCanvas.GetComponentInChildren<ScrollRect>(true);
            return _heroWindowRect;
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


    public ArchetypeUITreeWindow ArchetypeUITreeWindow
    {
        get
        {
            if (_archetypeUITreeWindow == null)
                _archetypeUITreeWindow = ArchetypeCanvas.GetComponentInChildren<ArchetypeUITreeWindow>(true);
            return _archetypeUITreeWindow;
        }
    }

    public LoadingScript LoadingScreen
    {
        get
        {
            if (_loadingScreen == null)
                _loadingScreen = GameObject.FindGameObjectWithTag("LoadingManager").GetComponent<LoadingScript>();
            return _loadingScreen;
        }
    }

    public SummonScrollWindow SummonScrollWindow
    {
        get
        {
            if (_summonScrollWindow == null)
                _summonScrollWindow = BattleUICanvas.GetComponentInChildren<SummonScrollWindow>(true);
            return _summonScrollWindow;
        }
    }

    public BattleCharInfoPanel BattleCharInfoPanel
    {
        get
        {
            if (_battleCharInfoPanel == null)
                _battleCharInfoPanel = BattleUICanvas.GetComponentInChildren<BattleCharInfoPanel>(true);
            return _battleCharInfoPanel;
        }
    }

    public TeamWindow TeamWindow
    {
        get
        {
            if (_teamWindow == null)
                _teamWindow = TeamCanvas.GetComponentInChildren<TeamWindow>(true);
            return _teamWindow;
        }
    }

    public WorkshopParentWindow WorkshopParentWindow
    {
        get
        {
            if (_workshopParentWindow == null)
                _workshopParentWindow = WorkshopCanvas.GetComponentInChildren<WorkshopParentWindow>(true);
            return _workshopParentWindow;
        }
    }

    public ItemCraftingPanel ItemCraftingPanel
    {
        get
        {
            if (_itemCraftingPanel == null)
                _itemCraftingPanel = WorkshopCanvas.GetComponentInChildren<ItemCraftingPanel>(true);
            return _itemCraftingPanel;
        }
    }

    public StageSelectPanel StageSelectPanel
    {
        get
        {
            if (_stageSelectPanel == null)
                _stageSelectPanel = StageSelectCanvas.GetComponentInChildren<StageSelectPanel>(true);
            return _stageSelectPanel;
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
        InvWindowCanvas.gameObject.SetActive(false);
        EquipDetailWindow.gameObject.SetActive(false);
    }

    public void CloseHeroWindows()
    {
        HeroWindowRect.gameObject.SetActive(false);
        HeroDetailWindow.gameObject.SetActive(false);
    }

    public void OpenInventoryWindow(bool closeWindows = true, bool showCategories = true, bool showDefault = true)
    {
        if (closeWindows)
            CloseAllWindows();

        RectTransform rect = InvWindowRect.GetComponent<RectTransform>();

        if (showCategories)
        {
            ItemCategoryPanel.gameObject.SetActive(true);
            rect.offsetMin = new Vector2(rect.offsetMin.x, 60);
        }
        else
        {
            ItemCategoryPanel.gameObject.SetActive(false);
            rect.offsetMin = new Vector2(rect.offsetMin.x, 0);
        }

        OpenWindow(InvWindowCanvas.gameObject, closeWindows);
        if (showDefault)
            InvScrollContent.ShowAllEquipment();
    }

    public void OpenTeamWindow(bool closeWindows = true)
    {
        if (closeWindows)
            CloseAllWindows();
        OpenWindow(TeamWindow.gameObject);
    }

    public void OpenHeroWindow(bool closeWindows = true, Func<HeroData, bool> predicate = null)
    {
        if (closeWindows)
            CloseAllWindows();
        HeroScrollContent.filterPredicate = predicate;
        OpenWindow(HeroWindowRect.gameObject, closeWindows);
    }


    public void OpenWorkshopWindow(bool closeWindows = true)
    {
        if (closeWindows)
            CloseAllWindows();
        OpenWindow(WorkshopParentWindow.gameObject, closeWindows);
        WorkshopParentWindow.Instance.SetItemCraftingPanelActive();
    }

    public void OpenStageSelectWindow(bool closeWindows = true)
    {
        if (closeWindows)
            CloseAllWindows();
        OpenWindow(StageSelectPanel.gameObject);
    }

    public void SetInventoryScrollRectTransform(int type)
    {
        if (type == 0)
        {
            InvWindowRect.GetComponent<RectTransform>().sizeDelta = fullWindowSize;
        }
    }
}