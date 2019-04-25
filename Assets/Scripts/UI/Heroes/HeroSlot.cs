using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroSlot : MonoBehaviour
{
    public HeroData hero;
    public Image slotImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI archetypeText;
    

    public void UpdateSlot()
    {
    }

    public void SetSlot(HeroData hero)
    {
        this.hero = hero;
        nameText.text = hero.Name;
        archetypeText.text = hero.PrimaryArchetype.Base.idName;
        if (hero.SecondaryArchetype != null)
        {
            archetypeText.text += "\n" + hero.SecondaryArchetype.Base.idName;
        }
        UpdateSlot();
    }

    public void OnHeroSlotClick()
    {
        HeroDetailWindow detailWindow = UIManager.Instance.HeroDetailWindow;
        UIManager.Instance.OpenWindow(detailWindow.gameObject);
        HeroDetailWindow.hero = hero;
        detailWindow.UpdateWindow();
    }
}
