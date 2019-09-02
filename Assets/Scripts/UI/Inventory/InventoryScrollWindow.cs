using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryScrollWindow : MonoBehaviour
{
    private enum LastViewType
    {
        NONE,
        ALL_EQUIPMENT,
        ALL_ARCHETYPE,
        ALL_ABILITY_CORE
    }

    [SerializeField]
    private InventorySlot SlotPrefab;

    private InventorySlotPool _inventorySlotPool;
    private List<InventorySlot> SlotsInUse = new List<InventorySlot>();
    private Action<Item> currentCallback = null;

    private LastViewType lastView = LastViewType.NONE;

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
        foreach (Item item in itemInventory)
        {
            AddInventorySlot(item, callback);
        }
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
        InitializeInventorySlots(GameManager.Instance.PlayerStats.EquipmentInventory, currentCallback);
        lastView = LastViewType.ALL_EQUIPMENT;
    }

    public void ShowAllArchetypes(bool resetCallback = true, bool addNullSlot = false)
    {
        if (resetCallback)
            currentCallback = null;
        ClearSlots();
        if (addNullSlot)
            AddInventorySlot(null);
        InitializeInventorySlots(GameManager.Instance.PlayerStats.ArchetypeInventory, currentCallback);
    }

    public void ShowAllAbility(bool resetCallback = true, bool addNullSlot = false)
    {
        if (resetCallback)
            currentCallback = null;
        ClearSlots();
        if (addNullSlot)
            AddInventorySlot(null);
        InitializeInventorySlots(GameManager.Instance.PlayerStats.AbilityStorageInventory, currentCallback);
    }

    public void ShowArchetypesFiltered(List<ArchetypeBase> filter, bool resetCallback = true, bool addNullSlot = false)
    {
        if (resetCallback)
            currentCallback = null;
        ClearSlots();
        if (addNullSlot)
            AddInventorySlot(null);

        foreach (ArchetypeItem item in GameManager.Instance.PlayerStats.ArchetypeInventory)
        {
            if (filter.Contains(item.Base))
                continue;
            AddInventorySlot(item, currentCallback);
        }
    }

    public void ShowEquipmentFiltered(Func<Equipment, bool> filter, bool resetCallback, bool addNullSlot)
    {
        if (resetCallback)
            currentCallback = null;
        ClearSlots();
        if (addNullSlot)
            AddInventorySlot(null);

        foreach (Equipment item in GameManager.Instance.PlayerStats.EquipmentInventory.Where(filter))
        {
            AddInventorySlot(item, currentCallback);
        }
    }

    public void ShowEquipmentBySlotType(EquipSlotType type)
    {
        ClearSlots();
        List<Equipment> e = GameManager.Instance.PlayerStats.EquipmentInventory.Where(x => x.Base.equipSlot == type && !x.IsEquipped).ToList();
        foreach (Equipment equip in e)
        {
            AddInventorySlot(equip, currentCallback);
        }
    }

    public void ShowEquipmentBySlotType(EquipSlotType[] types)
    {
        ClearSlots();
        List<Equipment> e = GameManager.Instance.PlayerStats.EquipmentInventory.Where(x => types.Contains(x.Base.equipSlot) && !x.IsEquipped).ToList();
        foreach (Equipment equip in e)
        {
            AddInventorySlot(equip, currentCallback);
        }
    }

    private void ClearSlots()
    {
        foreach (InventorySlot i in SlotsInUse)
        {
            InventorySlotPool.ReturnToPool(i);
        }
        SlotsInUse.Clear();
    }

    public void AddInventorySlot(Item item, Action<Item> callback = null)
    {
        InventorySlot slot;
        slot = InventorySlotPool.GetSlot();
        slot.gameObject.transform.SetParent(transform, false);
        slot.gameObject.transform.SetAsLastSibling();
        SlotsInUse.Add(slot);
        slot.item = item;
        slot.onClickAction = callback;
        slot.UpdateSlot();
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
}

public class InventorySlotPool : ObjectPool<InventorySlot>
{
    public InventorySlotPool(InventorySlot prefab, int i) : base(prefab, i)
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
}