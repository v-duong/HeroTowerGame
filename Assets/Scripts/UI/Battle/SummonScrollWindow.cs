using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonScrollWindow : MonoBehaviour
{
    [SerializeField]
    private SummonScrollSlot prefab;
    private List<SummonScrollSlot> summonSlots = new List<SummonScrollSlot>();

    public void AddHeroActor(HeroActor actor)
    {
        SummonScrollSlot slot = Instantiate(prefab, this.transform);
        slot.SetActor(actor);
        summonSlots.Add(slot);
    }
}
