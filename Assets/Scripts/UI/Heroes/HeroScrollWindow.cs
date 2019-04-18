using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        slot.nameText.text = hero.Name;
        slot.archetypeText.text = hero.PrimaryArchetype.Base.idName;
        if (hero.SecondaryArchetype != null)
        {
            slot.archetypeText.text += "\n" + hero.SecondaryArchetype.Base.idName;
        }
        slot.UpdateSlot();
    }

    public void RemoveHeroSlot(HeroData hero)
    {
        HeroSlot slot = SlotsInUse.Find(x => x.hero == hero);
        slot.gameObject.SetActive(false);
    }
}
