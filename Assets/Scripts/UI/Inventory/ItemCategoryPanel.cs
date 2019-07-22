using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCategoryPanel : MonoBehaviour
{
    public Button equipButton;
    public Button archetypeButton;

    public void EquipButtonClick()
    {
        UIManager.Instance.InvScrollContent.ShowAllEquipment();
    }

    public void ArchetypeButtonClick()
    {
        UIManager.Instance.InvScrollContent.ShowAllArchetype();
    }
}
