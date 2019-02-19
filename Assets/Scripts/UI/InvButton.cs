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
        Equipment equipment = new Equipment( ResourceManager.Instance.GetEquipmentBase(Random.Range(0, ResourceManager.Instance.EquipmentCount-1)), 10 );
        GameManager.Instance.PlayerStats.AddEquipmentToInventory(equipment);
    }
}
