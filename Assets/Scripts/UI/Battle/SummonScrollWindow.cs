using System.Collections.Generic;
using UnityEngine;

public class SummonScrollWindow : MonoBehaviour
{
    [SerializeField]
    private SummonScrollSlot prefab;

    private readonly List<SummonScrollSlot> summonSlots = new List<SummonScrollSlot>();

    public void AddHeroActor(HeroActor actor)
    {
        SummonScrollSlot slot = Instantiate(prefab, transform);
        slot.SetActor(actor);
        summonSlots.Add(slot);
    }

    public void SetHeroDead(HeroActor actor)
    {
        var slot = summonSlots.Find(x => x.actor == actor);
        if (slot != null)
        {
            slot.OnHeroDeath();
        }
    }
}
