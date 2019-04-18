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

    public void OnHeroSlotClick()
    {
        HeroDetailWindow detailWindow = UIManager.Instance.HeroDetailWindow;
        UIManager.Instance.OpenWindow(detailWindow.gameObject);
        detailWindow.hero = hero;
        detailWindow.heroSlot = this;
        detailWindow.UpdateWindow();
    }
}
