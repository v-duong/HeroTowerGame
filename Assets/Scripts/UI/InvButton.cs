using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

public class InvButton : MonoBehaviour
{
    public void OnClickInvToggle()
    {
        GameObject target = UIManager.Instance.InvCanvas.gameObject;
        if (!target.activeSelf)
            target.SetActive(true);
        else
            target.SetActive(false);
    }

    public void AddItem()
    {
        Armor equipment = new Armor( ResourceManager.Instance.GetRandomEquipmentBase(100) , 100 );
        GameManager.Instance.PlayerStats.AddEquipmentToInventory(equipment);
    }
}
