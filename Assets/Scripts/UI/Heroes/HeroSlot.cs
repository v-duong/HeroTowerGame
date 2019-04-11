using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroSlot : MonoBehaviour
{
    public HeroData hero;
    public Image slotImage;
    public Text nameText;

    public void UpdateSlot()
    {
    }

    public void OnHeroSlotClick()
    {
        HeroDetailWindow detailWindow = UIManager.Instance.HeroDetailWindow;
        detailWindow.gameObject.SetActive(true);
        detailWindow.hero = hero;
        detailWindow.heroSlot = this;
        detailWindow.UpdateWindow();
    }
}
