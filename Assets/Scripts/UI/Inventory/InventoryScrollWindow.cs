using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class InventoryScrollWindow : MonoBehaviour
{
    [SerializeField]
    private InventorySlot SlotPrefab;
    private InventorySlotPool _inventorySlotPool;
    public InventorySlotPool InventorySlotPool {
        get
        {
            if (_inventorySlotPool == null)
                _inventorySlotPool = new InventorySlotPool(SlotPrefab, GameManager.Instance.PlayerStats.equipmentInventory.Count);
            return _inventorySlotPool;
        }
    }
    private List<InventorySlot> SlotsInUse = new List<InventorySlot>();



    private void InitializeInventorySlots(List<Equipment> equipmentInventory)
    {
        foreach (AffixedItem item in equipmentInventory)
        {
            AddEquipmentSlot(item);
        }
    }

    public void InitializeInventorySlots(List<AffixedItem> list)
    {
        foreach (AffixedItem item in list)
        {
            AddEquipmentSlot(item);
        }
    }

    public void ShowAllEquipment()
    {

        ClearSlots();
        InitializeInventorySlots(GameManager.Instance.PlayerStats.equipmentInventory);
    }

    public void ShowEquipmentBySlotType(EquipSlotType type)
    {
        ClearSlots();
        List<Equipment> e = GameManager.Instance.PlayerStats.equipmentInventory.FindAll(x => x.Base.equipSlot == type && !x.IsEquipped);
        foreach(Equipment equip in e)
        {
            AddEquipmentSlot(equip);
        }
    }

    private void ClearSlots()
    {
        foreach(InventorySlot i in SlotsInUse)
        {
            InventorySlotPool.ReturnToPool(i);
        }
        SlotsInUse.Clear();
    }

    public void AddEquipmentSlot(AffixedItem item)
    {
        InventorySlot slot;
        slot = InventorySlotPool.GetSlot();
        slot.gameObject.transform.SetParent(this.transform, false);
        SlotsInUse.Add(slot);
        slot.item = item;
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