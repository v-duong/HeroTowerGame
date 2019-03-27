using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroScrollWindow : MonoBehaviour
{
    [SerializeField]
    private HeroSlot SlotPrefab;
    private List<HeroSlot> SlotsInUse = new List<HeroSlot>();

    public void InitializeHeroSlots(List<HeroData> list)
    {
        foreach (HeroData hero in list)
        {
            AddHeroSlot(hero);
        }
    }


    public void AddHeroSlot(HeroData hero)
    {
        HeroSlot slot;
        slot = Instantiate(SlotPrefab, this.transform);
        SlotsInUse.Add(slot);
        slot.hero = hero;
        slot.UpdateSlot();
    }

    public void RemoveHeroSlot(HeroData hero)
    {
        HeroSlot slot = SlotsInUse.Find(x => x.hero == hero);
        slot.gameObject.SetActive(false);
    }
}
