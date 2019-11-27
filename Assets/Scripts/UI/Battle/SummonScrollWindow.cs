using System.Collections.Generic;
using UnityEngine;

public class SummonScrollWindow : MonoBehaviour
{
    [SerializeField]
    private SummonScrollSlot prefab;

    [SerializeField]
    private GameObject panelParent;

    private readonly List<SummonScrollSlot> summonSlots = new List<SummonScrollSlot>();
    private bool isHidden = false;

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

    public void HideToggle()
    {
        isHidden = !isHidden;
        RectTransform parentRect = panelParent.transform as RectTransform;
        RectTransform infoRect = UIManager.Instance.BattleCharInfoPanel.transform as RectTransform;

        if (isHidden)
        {
            parentRect.anchoredPosition = new Vector2(0, -90);
            infoRect.anchoredPosition = new Vector2(0, 25);
        }
        else
        {
            parentRect.anchoredPosition = new Vector2(0, 0);
            infoRect.anchoredPosition = new Vector2(0, 115);
        }
    }
}