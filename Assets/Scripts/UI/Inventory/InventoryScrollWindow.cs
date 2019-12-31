using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryScrollWindow : MonoBehaviour
{
    [SerializeField]
    private InventorySlot SlotPrefab;

    [SerializeField]
    private Button toggleAffixesButton;

    [SerializeField]
    private Button selectFilterButton;

    [SerializeField]
    private Button toggleEquippedButton;

    [SerializeField]
    private Button typeFilterButton;

    [SerializeField]
    public InventoryFilterWindow filterWindow;

    [SerializeField]
    public ItemSelectionWindow selectionWindow;

    public Button confirmButton;
    private Action<List<Item>> confirmOnClick = null;

    private InventorySlotPool _inventorySlotPool;
    private List<InventorySlot> SlotsInUse = new List<InventorySlot>();
    private Action<Item> currentCallback = null;
    private List<Item> selectedItems = new List<Item>();
    public bool isMultiSelectMode = false;
    public bool showItemValues = false;
    public bool showItemAffixes = false;
    public bool hideEquipped = false;
    private float currentY = 120;

    private void Start()
    {
        //SetGridCellSize(ViewType.EQUIPMENT);
    }

    private void SetGridCellSize(ViewType viewType)
    {
        GridLayoutGroup grid = GetComponent<GridLayoutGroup>();
        float ySize = 350;

        switch (viewType)
        {
            case ViewType.EQUIPMENT:
                if (!showItemAffixes)
                    ySize = 155;
                break;

            case ViewType.ARCHETYPE:
            case ViewType.ABILITY_CORE:
                ySize = 85;
                break;
        }

        if (GameManager.Instance.aspectRatio >= 1.92)
        {
            grid.cellSize = new Vector2(185, ySize);
        }
        else if (GameManager.Instance.aspectRatio >= 1.85)
        {
            grid.cellSize = new Vector2(210, ySize);
        }
        else
        {
            grid.cellSize = new Vector2(230, ySize);
        }

        currentY = ySize;
    }

    private void OnEnable()
    {
        ((RectTransform)transform).anchoredPosition = Vector3.zero;

        selectFilterButton.gameObject.SetActive(isMultiSelectMode);
        toggleEquippedButton.gameObject.SetActive(!isMultiSelectMode);
        SetHideEquippedToggle(false);
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

    private void InitializeInventorySlots<T>(IList<T> itemInventory, Action<Item> callback = null) where T : Item
    {
        for (int i = 0; i < itemInventory.Count; i++)
        {
            Item item = itemInventory[i];
            AddInventorySlot(item, callback, i);
        }
        DeactivateSlotsInPool();
    }

    private void InitializeInventorySlots(IList<Equipment> equipmentInventory, Action<Item> callback = null)
    {
        for (int i = 0; i < equipmentInventory.Count; i++)
        {
            Equipment item = equipmentInventory[i];
            AddInventorySlot(item, callback, i);
        }
    }

    public void ShowAllEquipment(bool resetCallback = true)
    {
        if (resetCallback)
            currentCallback = null;
        ClearSlots();

        toggleAffixesButton.gameObject.SetActive(true);
        SetGridCellSize(ViewType.EQUIPMENT);
        InitializeInventorySlots(GameManager.Instance.PlayerStats.EquipmentInventory, currentCallback);
        UIManager.Instance.ItemCategoryPanel.SetEquipmentSelected();
    }

    public void ShowAllArchetypes(bool resetCallback = true, bool addNullSlot = false)
    {
        if (resetCallback)
            currentCallback = null;
        ClearSlots();
        if (addNullSlot)
            AddInventorySlot(null, null, 0);

        toggleAffixesButton.gameObject.SetActive(false);
        showItemAffixes = false;
        SetGridCellSize(ViewType.ARCHETYPE);

        InitializeInventorySlots(GameManager.Instance.PlayerStats.ArchetypeInventory, currentCallback);
        UIManager.Instance.ItemCategoryPanel.SetArchetypeSelected();
    }

    public void ShowAllAbility( bool resetCallback = true, bool addNullSlot = false)
    {
        if (resetCallback)
            currentCallback = null;
        ClearSlots();
        if (addNullSlot)
            AddInventorySlot(null, null, 0);

        toggleAffixesButton.gameObject.SetActive(false);
        showItemAffixes = false;
        SetGridCellSize(ViewType.ABILITY_CORE);

        InitializeInventorySlots(GameManager.Instance.PlayerStats.AbilityInventory, currentCallback);
        UIManager.Instance.ItemCategoryPanel.SetAbilitySelected();
    }

    public void ShowAbilityFiltered(Func<AbilityCoreItem, bool> filter, bool resetCallback = true, bool addNullSlot = false)
    {
        if (resetCallback)
            currentCallback = null;
        ClearSlots();
        if (addNullSlot)
            AddInventorySlot(null, null, 0);

        toggleAffixesButton.gameObject.SetActive(false);
        showItemAffixes = false;
        SetGridCellSize(ViewType.ABILITY_CORE);

        int i = 1;
        foreach (AbilityCoreItem item in GameManager.Instance.PlayerStats.AbilityInventory.Where(filter))
        {
            AddInventorySlot(item, currentCallback, i);
            i++;
        }
        DeactivateSlotsInPool();
    }

    public void ShowArchetypesFiltered(List<ArchetypeBase> filter, bool resetCallback = true, bool addNullSlot = false)
    {
        if (resetCallback)
            currentCallback = null;
        ClearSlots();
        if (addNullSlot)
            AddInventorySlot(null, null, 0);

        toggleAffixesButton.gameObject.SetActive(false);
        showItemAffixes = false;
        SetGridCellSize(ViewType.ARCHETYPE);

        for (int i = 0; i < GameManager.Instance.PlayerStats.ArchetypeInventory.Count; i++)
        {
            ArchetypeItem item = GameManager.Instance.PlayerStats.ArchetypeInventory[i];
            if (filter.Contains(item.Base))
                continue;
            AddInventorySlot(item, currentCallback, i + 1);
        }
        DeactivateSlotsInPool();
    }

    public void ShowEquipmentFiltered(Func<Equipment, bool> filter, bool resetCallback, bool addNullSlot)
    {
        if (resetCallback)
            currentCallback = null;
        ClearSlots();
        if (addNullSlot)
            AddInventorySlot(null, null, 0);

        toggleAffixesButton.gameObject.SetActive(true);

        SetGridCellSize(ViewType.EQUIPMENT);

        int i = 1;
        foreach (Equipment item in GameManager.Instance.PlayerStats.EquipmentInventory.Where(filter))
        {
            AddInventorySlot(item, currentCallback, i);
            i++;
        }
        DeactivateSlotsInPool();
    }

    private void ClearSlots()
    {
        foreach (InventorySlot i in SlotsInUse)
        {
            i.multiSelectMode = false;
            i.showItemValue = false;
            i.alreadySelected = false;
            InventorySlotPool.ReturnToPool(i);
        }
        SlotsInUse.Clear();
    }

    private void DeactivateSlotsInPool()
    {
        InventorySlotPool.DeactivateObjectsInPool();
    }

    public void AddInventorySlot(Item item, Action<Item> callback, int index)
    {
        InventorySlot slot = InventorySlotPool.GetSlot(false);
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

        bool slotIsSelected = selectedItems.Contains(item);
        slot.selectedImage.gameObject.SetActive(slotIsSelected);
        slot.alreadySelected = slotIsSelected;
        slot.SetTextVisiblity(index < 2100 / currentY);
        slot.gameObject.SetActive(true);
    }

    public void OnScrollChange(Vector2 vector2)
    {
        float invY = (transform as RectTransform).anchoredPosition.y;
        foreach (InventorySlot i in SlotsInUse)
        {
            float slotY = -(i.transform as RectTransform).anchoredPosition.y;
            i.SetTextVisiblity(invY - 170 < slotY && slotY < invY + 890);
        }
    }

    public void ToggleItemMultiSelect(Item item)
    {
        if (!selectedItems.Contains(item))
            selectedItems.Add(item);
        else
            selectedItems.Remove(item);
    }

    public void SetHideEquippedToggle(bool value)
    {
        hideEquipped = value;

        if (hideEquipped)
        {
            toggleEquippedButton.GetComponentInChildren<TextMeshProUGUI>().text = "Show Equipped";
        }
        else
        {
            toggleEquippedButton.GetComponentInChildren<TextMeshProUGUI>().text = "Hide Equipped";
        }
    }

    public void ToggleEquippedOnClick()
    {
        SetHideEquippedToggle(!hideEquipped);

        foreach (InventorySlot slot in SlotsInUse)
        {
            if (slot.item is Equipment e)
            {
                if (hideEquipped && e.IsEquipped)
                {
                    slot.gameObject.SetActive(false);
                }
                else
                {
                    slot.gameObject.SetActive(true);
                }
            }
        }
    }

    public void FilterTypeButtonOnClick()
    {
        UIManager.Instance.OpenWindow(filterWindow.gameObject, false);
    }

    public void FilterShownSlotsByType(HashSet<GroupType> groupTypes, HashSet<GroupType> optionalTypes)
    {
        
        foreach (InventorySlot slot in SlotsInUse)
        {
            if (groupTypes.Count == 0 || (slot.item is Equipment e
                                          && e.GetGroupTypes().IsSupersetOf(groupTypes)
                                          && (optionalTypes.Count == 0 || e.GetGroupTypes().Intersect(optionalTypes).Any())))
            {
                slot.gameObject.SetActive(true);
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }

        ((RectTransform)transform).anchoredPosition = Vector3.zero;
    }

    public void SelectByILvlButtonOnClick()
    {
        UIManager.Instance.OpenWindow(selectionWindow.gameObject, false);
    }

    public void SelectByCriteria(Func<Item, bool> criteriaFunction)
    {
        foreach (InventorySlot inventorySlot in SlotsInUse)
        {
            if (inventorySlot.alreadySelected || !criteriaFunction(inventorySlot.item))
                continue;
            else
            {
                selectedItems.Add(inventorySlot.item);
                inventorySlot.alreadySelected = true;
                inventorySlot.selectedImage.gameObject.SetActive(true);
            }
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
        SetGridCellSize(ViewType.EQUIPMENT);
        foreach (var x in SlotsInUse)
        {
            x.UpdateSlot();
        }
    }

    public void CheckHeroRequirements(HeroData hero)
    {
        foreach (InventorySlot inventorySlot in SlotsInUse)
        {
            if (inventorySlot.item is Equipment equip && !hero.CanEquipItem(equip))
                inventorySlot.lockImage.gameObject.SetActive(true);
        }
    }

    private enum ViewType
    {
        EQUIPMENT,
        ARCHETYPE,
        ABILITY_CORE
    }
}

public class InventorySlotPool : StackObjectPool<InventorySlot>
{
    public InventorySlotPool(InventorySlot prefab, int i) : base(prefab, 75)
    {
    }

    public InventorySlot GetSlot(bool activeState)
    {
        return Get(activeState);
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