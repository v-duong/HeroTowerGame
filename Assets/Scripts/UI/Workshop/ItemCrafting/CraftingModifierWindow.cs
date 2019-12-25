using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CraftingModifierWindow : MonoBehaviour
{
    public List<CraftingModifierButtons> modifierButtons;
    [SerializeField]
    private TextMeshProUGUI costText;

    public void OnClickCloseWindow()
    {
        gameObject.SetActive(false);
        ItemCraftingPanel craftingPanel = UIManager.Instance.ItemCraftingPanel;
        craftingPanel.modifiers.Clear();
        float costMulti = 1f;
      
        foreach(CraftingModifierButtons craftingModifier in modifierButtons)
        {
            costMulti *= craftingModifier.currentCostMultiplier;
            craftingPanel.modifiers.Add(craftingModifier.groupType, craftingModifier.currentModifier);
        }

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

        costText.text = "Crafting Cost x" + costMulti.ToString("N2");
    }
}
