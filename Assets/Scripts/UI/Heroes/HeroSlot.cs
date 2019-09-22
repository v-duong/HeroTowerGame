using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroSlot : MonoBehaviour
{
    public HeroData hero;
    public Image slotImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI archetypeText;
    public Action<HeroData> callback;

    public void UpdateSlot()
    {
    }

    public void SetSlot(HeroData hero)
    {
        this.hero = hero;
        nameText.text = "Lv" + hero.Level + " " + hero.Name + "\n";
        nameText.text += "<size=80%>" + LocalizationManager.Instance.GetLocalizationText_ArchetypeName(hero.PrimaryArchetype.Base.idName) + "</size>";
        if (hero.SecondaryArchetype != null)
        {
            nameText.text += "\n" + "<size=80%>" + LocalizationManager.Instance.GetLocalizationText_ArchetypeName(hero.SecondaryArchetype.Base.idName) + "</size>";
        }
        archetypeText.text = "";
        if (hero.GetAbilityFromSlot(0) != null)
        {
            archetypeText.text += hero.GetAbilityFromSlot(0).abilityBase.idName + "\n";
        }
        if (hero.GetAbilityFromSlot(1) != null)
        {
            archetypeText.text += hero.GetAbilityFromSlot(1).abilityBase.idName;
        }
        UpdateSlot();
    }

    public void OnHeroSlotClick()
    {
        if (callback != null)
        {
            callback.Invoke(hero);
            return;
        }
        HeroDetailWindow detailWindow = UIManager.Instance.HeroDetailWindow;
        UIManager.Instance.OpenWindow(detailWindow.gameObject);
        HeroDetailWindow.hero = hero;
        detailWindow.UpdateWindow();
    }
}