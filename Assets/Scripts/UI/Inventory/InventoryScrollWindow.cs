using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScrollWindow : MonoBehaviour
{
    [SerializeField]
    private InventorySlot SlotPrefab;

    private InventorySlotPool _inventorySlotPool;

    public InventorySlotPool InventorySlotPool
    {
        get
        {
            if (_inventorySlotPool == null)
                _inventorySlotPool = new InventorySlotPool(SlotPrefab, GameManager.Instance.PlayerStats.equipmentInventory.Count);
            return _inventorySlotPool;
        }
    }

    private List<InventorySlot> SlotsInUse = new List<InventorySlot>();
    private bool showingAllEquip = false;

    private void InitializeInventorySlots(List<ArchetypeItem> archetypeInventory, Action callback = null)
    {
        foreach (ArchetypeItem item in archetypeInventory)
        {
            AddEquipmentSlot(item, callback);
        }
    }

    private void InitializeInventorySlots(List<Equipment> equipmentInventory, Action callback = null)
    {
        foreach (AffixedItem item in equipmentInventory)
        {
            AddEquipmentSlot(item, callback);
        }
    }

    public void InitializeInventorySlots(List<AffixedItem> list, Action callback = null)
    {
        foreach (AffixedItem item in list)
        {
            AddEquipmentSlot(item, callback);
        }
    }

    public void ShowAllEquipment(Action callback = null)
    {
        if (!showingAllEquip)
        {
            ClearSlots();
            InitializeInventorySlots(GameManager.Instance.PlayerStats.equipmentInventory);
            showingAllEquip = true;
        }
    }

    public void ShowAllArchetype(Action callback = null)
    {
        ClearSlots();
        InitializeInventorySlots(GameManager.Instance.PlayerStats.archetypeInventory);
        showingAllEquip = false;
    }

    public void ShowEquipmentBySlotType(EquipSlotType type, Action callback = null)
    {
        showingAllEquip = false;
        ClearSlots();
        List<Equipment> e = GameManager.Instance.PlayerStats.equipmentInventory.FindAll(x => x.Base.equipSlot == type && !x.IsEquipped);
        foreach (Equipment equip in e)
        {
            AddEquipmentSlot(equip, callback);
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

    public void AddEquipmentSlot(Item item, Action callback = null)
    {
        InventorySlot slot;
        slot = InventorySlotPool.GetSlot();
        slot.gameObject.transform.SetParent(this.transform, false);
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