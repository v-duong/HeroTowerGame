using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingModifierButtons : MonoBehaviour
{
    public GroupType groupType;
    public float currentModifier = 1f;
    public float currentCostMultiplier = 1f;
    public List<Button> buttons;
    public Button selectedButton;

    public void OnEnable()
    {
        if (buttons.Count > 0)
        {
            if (selectedButton == null)
                selectedButton = buttons[0];

            foreach (Button listButton in buttons)
            {
                listButton.image.color = Color.white;
            }

            selectedButton.image.color = Helpers.SELECTION_COLOR;
        }
    }

    public void OnClickSetChanceMod(float mod)
    {
        currentModifier = mod;
    }

    public void OnClickSetCostMod(float mod)
    {
        currentCostMultiplier = mod;
    }

    public void OnClickSelectButton(Button button)
    {
        foreach(Button listButton in buttons)
        {
            listButton.image.color = Color.white;
        }
        selectedButton = button;

        button.image.color = Helpers.SELECTION_COLOR;
        UIManager.Instance.ItemCraftingPanel.craftingModifierWindow.UpdateCostText();
    }
}