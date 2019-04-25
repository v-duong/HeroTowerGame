﻿using System.Collections.Generic;
using UnityEngine;

public class HeroScrollWindow : MonoBehaviour
{
    [SerializeField]
    private HeroSlot SlotPrefab;

    private List<HeroSlot> SlotsInUse = new List<HeroSlot>();
    private Stack<HeroSlot> AvailableSlots = new Stack<HeroSlot>();
    private bool initialized = false;

    private void OnEnable()
    {
        InitializeHeroSlots(GameManager.Instance.PlayerStats.heroList);
    }

    public void InitializeHeroSlots(List<HeroData> list)
    {
        foreach (HeroSlot slot in SlotsInUse)
        {
            slot.gameObject.SetActive(false);
            AvailableSlots.Push(slot);
        }
        SlotsInUse.Clear();
        foreach (HeroData hero in list)
        {
            AddHeroSlot(hero);
        }
    }

    public void AddHeroSlot(HeroData hero)
    {
        HeroSlot slot;
        if (AvailableSlots.Count > 0)
        {
            slot = AvailableSlots.Pop();
        }
        else
        {
            slot = Instantiate(SlotPrefab, this.transform);
        }
        slot.gameObject.SetActive(true);
        SlotsInUse.Add(slot);

        slot.SetSlot(hero);
    }

    public void RemoveHeroSlot(HeroData hero)
    {
        HeroSlot slot = SlotsInUse.Find(x => x.hero == hero);
        slot.gameObject.SetActive(false);
    }
}