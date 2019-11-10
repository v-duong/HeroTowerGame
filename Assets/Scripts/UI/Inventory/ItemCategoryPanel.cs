using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCategoryPanel : MonoBehaviour
{

    public Button equipButton;
    public Button archetypeButton;
    public Button abilityButton;

    public void EquipButtonClick()
    {
        UIManager.Instance.InvScrollContent.ShowAllEquipment();
    }

    public void ArchetypeButtonClick()
    {
        UIManager.Instance.InvScrollContent.ShowAllArchetypes();
    }

    public void AbilityButtonClick()
    {
        UIManager.Instance.InvScrollContent.ShowAllAbility();
    }

    public void SetEquipmentSelected()
    {
        equipButton.image.color = Helpers.SELECTION_COLOR;
        archetypeButton.image.color = Color.white;
        abilityButton.image.color = Color.white;
    }

    public void SetArchetypeSelected()
    {
        equipButton.image.color = Color.white;
        archetypeButton.image.color = Helpers.SELECTION_COLOR;
        abilityButton.image.color = Color.white;
    }

    public void SetAbilitySelected()
    {
        equipButton.image.color = Color.white;
        archetypeButton.image.color = Color.white;
        abilityButton.image.color = Helpers.SELECTION_COLOR;
    }
}
