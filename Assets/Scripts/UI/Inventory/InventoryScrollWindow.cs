using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class InventoryScrollWindow : MonoBehaviour
{
    [SerializeField]
    private InventorySlot SlotPrefab;
    private InventorySlotPool inventorySlotPool;
    private List<InventorySlot> SlotsInUse = new List<InventorySlot>();

    public InventoryScrollWindow ()
    {

    }

    private void Awake()
    {

        inventorySlotPool = new InventorySlotPool(SlotPrefab, GameManager.Instance.PlayerStats.equipmentInventory.Count);
    }

    private void InitializeInventorySlots(List<Equipment> equipmentInventory)
    {
        foreach (Item item in equipmentInventory)
        {
            AddEquipmentSlot(item);
        }
    }

    public void InitializeInventorySlots(List<Item> list)
    {
        foreach (Item item in list)
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
        Debug.Log(type);
        ClearSlots();
        List<Equipment> e = GameManager.Instance.PlayerStats.equipmentInventory.FindAll(x => x.Base.equipSlot == type);
        foreach(Equipment equip in e)
        {
            AddEquipmentSlot(equip);
        }
    }

    private void ClearSlots()
    {
        foreach(InventorySlot i in SlotsInUse)
        {
            inventorySlotPool.ReturnToPool(i);
        }
        SlotsInUse.Clear();
    }

    public void AddEquipmentSlot(Item item)
    {
        InventorySlot slot;
        slot = inventorySlotPool.GetSlot();
        slot.gameObject.transform.SetParent(this.transform, false);
        SlotsInUse.Add(slot);
        slot.item = item;
        slot.UpdateSlot();
    }

    public void RemoveEquipmentSlot(Item item)
    {
        InventorySlot slot = SlotsInUse.Find(x => x.item == item);
        slot.gameObject.SetActive(false);
        SlotsInUse.Remove(slot);
        inventorySlotPool.ReturnToPool(slot);
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