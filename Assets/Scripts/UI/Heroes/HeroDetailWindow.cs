﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroDetailWindow : MonoBehaviour
{
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Text infoText;
    public HeroData hero;
    public HeroSlot heroSlot;

    public List<HeroEquipmentSlot> equipSlots;

    public void UpdateWindow()
    {
        nameText.text = hero.Name;

        infoText.text = "";
        infoText.text += "Level: " + hero.Level + "\n";
        infoText.text += "Experience: " + hero.Experience + "\n\n";
        infoText.text += "Health: " + hero.MaximumHealth + "\n";
        infoText.text += "Shield: " + hero.MaximumManaShield + "\n";
        infoText.text += "Soul Points: " + hero.MaximumSoulPoints + "\n\n";
        infoText.text += "Strength: " + hero.Strength + "\n";
        infoText.text += "Intelligence: " + hero.Intelligence + "\n";
        infoText.text += "Agility: " + hero.Agility + "\n";
        infoText.text += "Will: " + hero.Will + "\n";
        infoText.text += "Armor: " + hero.Armor + "\n";
        infoText.text += "Shield: " + hero.MaximumManaShield + "\n";
        infoText.text += "Dodge Rating: " + hero.DodgeRating + "\n";
        infoText.text += "Resolve: " + hero.ResolveRating + "\n";

        foreach(HeroEquipmentSlot slot in equipSlots)
        {
            Equipment e = hero.GetEquipmentInSlot(slot.EquipSlot);
            if (e == null)
            {
                slot.slotText.text = slot.EquipSlot.ToString();
            } else
            {
                slot.slotText.text = e.Name;
            }
        }
    }

    public void SetActiveToggle()
    {
        if (!this.gameObject.activeSelf)
            this.gameObject.SetActive(true);
        else
            this.gameObject.SetActive(false);
    }
}
