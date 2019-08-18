﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class HeroScrollWindow : MonoBehaviour
{
    [SerializeField]
    private HeroSlot SlotPrefab;

    private List<HeroSlot> SlotsInUse = new List<HeroSlot>();
    private Queue<HeroSlot> AvailableSlots = new Queue<HeroSlot>();
    private bool initialized = false;
    public Func<HeroData, bool> filterPredicate = null;

    private void OnEnable()
    {
        if (filterPredicate == null)
            InitializeHeroSlots(GameManager.Instance.PlayerStats.HeroList);
        else
            InitializeHeroSlots(GameManager.Instance.PlayerStats.HeroList, filterPredicate);
    }

    public void InitializeHeroSlots(IList<HeroData> list)
    {
        foreach (HeroSlot slot in SlotsInUse)
        {
            slot.gameObject.SetActive(false);
            AvailableSlots.Enqueue(slot);
        }
        SlotsInUse.Clear();
        foreach (HeroData hero in list)
        {
            AddHeroSlot(hero);
        }
    }

    public void InitializeHeroSlots(IList<HeroData> list, Func<HeroData, bool> filter)
    {
        foreach (HeroSlot slot in SlotsInUse)
        {
            slot.gameObject.SetActive(false);
            AvailableSlots.Enqueue(slot);
        }
        SlotsInUse.Clear();
        foreach (HeroData hero in list.Where(filter))
        {
            AddHeroSlot(hero);
        }
    }

    public void AddHeroSlot(HeroData hero)
    {
        HeroSlot slot;
        if (AvailableSlots.Count > 0)
        {
            slot = AvailableSlots.Dequeue();
        }
        else
        {
            slot = Instantiate(SlotPrefab, this.transform);
        }
        slot.gameObject.SetActive(true);
        SlotsInUse.Add(slot);
        slot.callback = null;
        slot.SetSlot(hero);
    }

    public void RemoveHeroSlot(HeroData hero)
    {
        HeroSlot slot = SlotsInUse.Find(x => x.hero == hero);
        slot.gameObject.SetActive(false);
    }

    public void SetCallback(Action<HeroData> callback)
    {
        foreach (HeroSlot slot in SlotsInUse)
        {
            slot.callback = callback;
        }
    }
}