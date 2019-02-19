using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public static UIManager Instance { get; private set; }
    public InventorySlot slotPrefab;
    private Canvas _invCanvas;
    private GridLayoutGroup _invWindow;
    private ItemDetailWindow _itemWindow;

    public Canvas currentActive;


    public Canvas InvCanvas {
        get {
            if (_invCanvas == null)
                _invCanvas = GameObject.FindGameObjectWithTag("InventoryCanvas").GetComponent<Canvas>();
            return _invCanvas;
        }
    }

    public GridLayoutGroup InvWindow
    {
        get
        {
            if (_invWindow == null)
                _invWindow = InvCanvas.GetComponentInChildren<GridLayoutGroup>(true);
            return _invWindow;
        }
    }

    public ItemDetailWindow ItemWindow
    {
        get {
            if (_itemWindow == null)
                _itemWindow = InvCanvas.GetComponentInChildren<ItemDetailWindow>(true);
            return _itemWindow;
        }
    }

    public void AddEquipmentSlot (Equipment equip)
    {
        InventorySlot slot = Instantiate(slotPrefab, InvWindow.transform);
        slot.item = equip;
        slot.UpdateSlot();
        slot.transform.position = new Vector3(0, 0, -2);
    }

    private void Start()
    {
        Instance = this;
    }

}
