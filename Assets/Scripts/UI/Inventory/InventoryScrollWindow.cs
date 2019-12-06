using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryScrollWindow : MonoBehaviour
{
    [SerializeField]
    private InventorySlot SlotPrefab;

    [SerializeField]
    private Button toggleAffixesButton;

    public Button confirmButton;
    private Action<List<Item>> confirmOnClick = null;

    private InventorySlotPool _inventorySlotPool;
    private List<InventorySlot> SlotsInUse = new List<InventorySlot>();
    private Action<Item> currentCallback = null;
    private List<Item> selectedItems = new List<Item>();
    public bool isMultiSelectMode = false;
    public bool showItemValues = false;
    public bool showItemAffixes = false;

    private void Start()
    {
        SetGridCellSize();
    }

    private void SetGridCellSize()
    {
        GridLayoutGroup grid = GetComponent<GridLayoutGroup>();
        float ySize = 350;
        if (!showItemAffixes)
            ySize = 155;
        if (GameManager.Instance.aspectRatio >= 1.85)
        {
            grid.cellSize = new Vector2(210, ySize);
        }
        else
        {
            grid.cellSize = new Vector2(230, ySize);
        }
    }

    private void OnEnable()
    {
        ((RectTransform)transform).anchoredPosition = Vector3.zero;
    }

    public InventorySlotPool InventorySlotPool
    {
        get
        {
            if (_inventorySlotPool == null)
                _inventorySlotPool = new InventorySlotPool(SlotPrefab, GameManager.Instance.PlayerStats.EquipmentInventory.Count);
            return _inventorySlotPool;
        }
    }

    public void ResetMultiSelectList()
    {
        selectedItems.Clear();
    }

    public void SetMultiSelectList(List<Item> list)
    {
        selectedItems.Clear();
        selectedItems.AddRange(list);
    }

    private void InitializeInventorySlots<T>(IList<T> itemInventory, Action<Item> callback = null) where T : Item
    {
        foreach (Item item in itemInventory)
        {
            AddInventorySlot(item, callback);
        }
        DeactivateSlotsInPool();
    }

    private void InitializeInventorySlots(IList<Equipment> equipmentInventory, Action<Item> callback = null)
    {
        foreach (Equipment item in equipmentInventory)
        {
            AddInventorySlot(item, callback);
        }
    }

    public void ShowAllEquipment(bool resetCallback = true)
    {
        if (resetCallback)
            currentCallback = null;
        ClearSlots();

        toggleAffixesButton.gameObject.SetActive(true);

        InitializeInventorySlots(GameManager.Instance.PlayerStats.EquipmentInventory, currentCallback);
        UIManager.Instance.ItemCategoryPanel.SetEquipmentSelected();
        
    }

    public void ShowAllArchetypes(bool resetCallback = true, bool addNullSlot = false)
    {
        if (resetCallback)
            currentCallback = null;
        ClearSlots();
        if (addNullSlot)
            AddInventorySlot(null);

        toggleAffixesButton.gameObject.SetActive(false);
        showItemAffixes = false;
        SetGridCellSize();

        InitializeInventorySlots(GameManager.Instance.PlayerStats.ArchetypeInventory, currentCallback);
        UIManager.Instance.ItemCategoryPanel.SetArchetypeSelected();
    }

    public void ShowAllAbility(bool resetCallback = true, bool addNullSlot = false)
    {
        if (resetCallback)
            currentCallback = null;
        ClearSlots();
        if (addNullSlot)
            AddInventorySlot(null);

        toggleAffixesButton.gameObject.SetActive(false);
        showItemAffixes = false;
        SetGridCellSize();

        InitializeInventorySlots(GameManager.Instance.PlayerStats.AbilityInventory, currentCallback);
        UIManager.Instance.ItemCategoryPanel.SetAbilitySelected();
    }

    public void ShowArchetypesFiltered(List<ArchetypeBase> filter, bool resetCallback = true, bool addNullSlot = false)
    {
        if (resetCallback)
            currentCallback = null;
        ClearSlots();
        if (addNullSlot)
            AddInventorySlot(null);

        toggleAffixesButton.gameObject.SetActive(false);
        showItemAffixes = false;
        SetGridCellSize();

        foreach (ArchetypeItem item in GameManager.Instance.PlayerStats.ArchetypeInventory)
        {
            if (filter.Contains(item.Base))
                continue;
            AddInventorySlot(item, currentCallback);
        }
        DeactivateSlotsInPool();
    }

    public void ShowEquipmentFiltered(Func<Equipment, bool> filter, bool resetCallback, bool addNullSlot)
    {
        if (resetCallback)
            currentCallback = null;
        ClearSlots();
        if (addNullSlot)
            AddInventorySlot(null);

        toggleAffixesButton.gameObject.SetActive(true);

        foreach (Equipment item in GameManager.Instance.PlayerStats.EquipmentInventory.Where(filter))
        {
            AddInventorySlot(item, currentCallback);
        }
        DeactivateSlotsInPool();
    }

    private void ClearSlots()
    {
        foreach (InventorySlot i in SlotsInUse)
        {
            InventorySlotPool.ReturnToPoolWhileActive(i);
        }
        SlotsInUse.Clear();
    }

    private void DeactivateSlotsInPool()
    {
        InventorySlotPool.DeactivateObjectsInPool();
    }

    public void AddInventorySlot(Item item, Action<Item> callback = null)
    {
        InventorySlot slot = InventorySlotPool.GetSlot();
        slot.gameObject.transform.SetParent(transform, false);
        slot.gameObject.transform.SetAsLastSibling();
        SlotsInUse.Add(slot);
        slot.item = item;
        slot.showItemValue = showItemValues;
        slot.UpdateSlot();

        slot.multiSelectMode = isMultiSelectMode;
        if (isMultiSelectMode)
            slot.onClickAction = ToggleItemMultiSelect;
        else
            slot.onClickAction = callback;


        if (selectedItems.Contains(item))
        {
            slot.selectedImage.gameObject.SetActive(true);
            slot.alreadySelected = true;
        } else
        {
            slot.selectedImage.gameObject.SetActive(false);
            slot.alreadySelected = false;
        }
    }

    public void ToggleItemMultiSelect(Item item)
    {
        if (!selectedItems.Contains(item))
            selectedItems.Add(item);
        else
            selectedItems.Remove(item);
    }

    public void RemoveEquipmentSlot(AffixedItem item)
    {
        InventorySlot slot = SlotsInUse.Find(x => x.item == item);
        slot.gameObject.SetActive(false);
        SlotsInUse.Remove(slot);
        InventorySlotPool.ReturnToPool(slot);
    }

    public void SetCallback(Action<Item> callback)
    {
        currentCallback = callback;
        foreach (InventorySlot slot in SlotsInUse)
        {
            slot.onClickAction = callback;
        }
    }

    public void SetConfirmCallback(Action<List<Item>> callback)
    {
        confirmOnClick = callback;
    }

    public void ConfirmOnClick()
    {
        confirmOnClick?.Invoke(selectedItems);
    }

    public void ToggleAffixInfoOnClick()
    {
        showItemAffixes = !showItemAffixes;
        SetGridCellSize();
        foreach(var x in SlotsInUse)
        {
            x.UpdateSlot();
        }
    }

    public void CheckHeroRequirements(HeroData hero)
    {
        foreach(InventorySlot inventorySlot in SlotsInUse)
        {
            if (inventorySlot.item is Equipment equip && !hero.CanEquipItem(equip))
                inventorySlot.lockImage.gameObject.SetActive(true);
        }
    }
}

public class InventorySlotPool : StackObjectPool<InventorySlot>
{
    public InventorySlotPool(InventorySlot prefab, int i) : base(prefab, 0)
    {
    }

    public InventorySlot GetSlot()
    {
        return Get();
    }

    public override void ReturnToPool(InventorySlot item)
    {
        Return(item);
    }

    public void ReturnToPoolWhileActive(InventorySlot item)
    {
        ReturnWithoutDeactivate(item);
    }

    public void DeactivateObjectsInPool()
    {
        DeactivatePooledObjects();
    }
}