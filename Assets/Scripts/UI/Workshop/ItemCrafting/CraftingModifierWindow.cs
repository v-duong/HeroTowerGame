using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CraftingModifierWindow : MonoBehaviour
{
    public List<CraftingModifierButtons> modifierButtons;
    public CraftingModifierButtons highLevelMod;
    [SerializeField]
    private TextMeshProUGUI costText;

    public void ResetWindow()
    {
        foreach (CraftingModifierButtons craftingModifier in modifierButtons)
        {
            craftingModifier.ResetButtons();
        }
        highLevelMod.ResetButtons();
        ItemCraftingPanel craftingPanel = UIManager.Instance.ItemCraftingPanel;
        craftingPanel.costModifier = 1f;
        craftingPanel.modifiers.Clear();
    }

    public void OnClickCloseWindow()
    {
        UIManager.Instance.CloseCurrentWindow();
        ItemCraftingPanel craftingPanel = UIManager.Instance.ItemCraftingPanel;
        craftingPanel.modifiers.Clear();
        float costMulti = 1f;
      
        foreach(CraftingModifierButtons craftingModifier in modifierButtons)
        {
            costMulti *= craftingModifier.currentCostMultiplier;
            craftingPanel.modifiers.Add(craftingModifier.groupType, craftingModifier.currentModifier);
        }

        costMulti *= highLevelMod.currentCostMultiplier;

        craftingPanel.costModifier = costMulti;
        craftingPanel.UpdateButtons();
    }

    public void UpdateCostText()
    {
        float costMulti = 1f;
        foreach (CraftingModifierButtons craftingModifier in modifierButtons)
        {
            costMulti *= craftingModifier.currentCostMultiplier;
        }
        costMulti *= highLevelMod.currentCostMultiplier;

        costText.text = "Crafting Cost x" + costMulti.ToString("N2");
    }
}
