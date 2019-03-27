﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

public class MenuButton : MonoBehaviour
{
    public void OnClickInvToggle()
    {
        GameObject target = UIManager.Instance.InvWindowRect.gameObject;
        UIManager.Instance.CloseHeroWindows();
        UIManager.Instance.EquipDetailWindow.gameObject.SetActive(false);
        if (!target.activeSelf)
        {
            target.SetActive(true);
            UIManager.Instance.InvScrollContent.ShowAllEquipment();
        }
        else
            target.SetActive(false);
        
    }


    public void OnClickHeroToggle()
    {
        GameObject target = UIManager.Instance.HeroWindowRect.gameObject;
        UIManager.Instance.CloseInventoryWindows();
        UIManager.Instance.HeroDetailWindow.gameObject.SetActive(false);
        if (!target.activeSelf)
            target.SetActive(true);
        else
            target.SetActive(false);
    }

    public void AddItem()
    {
        Equipment equipment = ResourceManager.Instance.CreateRandomEquipment(100);
        GameManager.Instance.PlayerStats.AddEquipmentToInventory(equipment);
    }

    public void AddHero()
    {
        HeroData hero = new HeroData
        {
            Name = "TEST " + UnityEngine.Random.Range(2, 321)
        };
        GameManager.Instance.PlayerStats.AddHeroToList(hero);
    }
}
