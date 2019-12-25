using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingButton : MonoBehaviour
{
    [SerializeField]
    private CraftingOptionType optionType;

    private string costText;

    public enum CraftingOptionType
    {
        REROLL_AFFIX,
        REROLL_VALUES,
        ADD_AFFIX,
        REMOVE_AFFIX,
        UPGRADE_RARITY,
        TO_NORMAL,
        LOCK_AFFIX
    }

    public void UpdateButton(AffixedItem currentItem)
    {
        var button = GetComponent<UIKeyButton>();
        if (!button.initialized)
            button.Initialize();

        GetComponentInChildren<TextMeshProUGUI>().text = button.localizedString;

        if (currentItem == null)
        {
            DisableButton(button);
        }
        else
        {
            int itemFragments = GameManager.Instance.PlayerStats.ItemFragments;
            float cost;
            switch (optionType)
            {
                case CraftingOptionType.REROLL_AFFIX when currentItem.Rarity != RarityType.NORMAL && currentItem.Rarity != RarityType.UNIQUE:
                    cost = AffixedItem.GetRerollAffixCost(currentItem) * UIManager.Instance.ItemCraftingPanel.costModifier;
                    break;

                case CraftingOptionType.REROLL_VALUES when currentItem.Rarity != RarityType.NORMAL:
                    cost = AffixedItem.GetRerollValuesCost(currentItem);
                    break;

                case CraftingOptionType.ADD_AFFIX when currentItem.GetRandomOpenAffixType() != null:
                    cost = AffixedItem.GetAddAffixCost(currentItem) * UIManager.Instance.ItemCraftingPanel.costModifier;
                    break;

                case CraftingOptionType.REMOVE_AFFIX when currentItem.GetRandomAffix() != null:
                    cost = AffixedItem.GetRemoveAffixCost(currentItem);
                    break;

                case CraftingOptionType.UPGRADE_RARITY when currentItem.Rarity != RarityType.UNIQUE && currentItem.Rarity != RarityType.EPIC:
                    cost = AffixedItem.GetUpgradeCost(currentItem) * UIManager.Instance.ItemCraftingPanel.costModifier;
                    break;

                case CraftingOptionType.TO_NORMAL when currentItem.Rarity != RarityType.NORMAL && currentItem.Rarity != RarityType.UNIQUE:
                    cost = AffixedItem.GetToNormalCost(currentItem);
                    break;

                case CraftingOptionType.LOCK_AFFIX when currentItem.GetRandomAffix() != null:
                    cost = AffixedItem.GetLockCost(currentItem);
                    break;

                default:
                    DisableButton(button);
                    return;
            }

            cost = (int)(cost);

            costText = cost.ToString("N0") + " <sprite=10>";

            if (cost != 0)
            {
                GetComponent<Button>().interactable = cost <= itemFragments;
                if (!GetComponent<Button>().interactable)
                    costText = "<color=#aa0000>" + costText + "</color>";
                GetComponentInChildren<TextMeshProUGUI>().text += "\n" + costText;
            }
        }
    }

    private void DisableButton(UIKeyButton button)
    {
        GetComponent<Button>().interactable = false;
    }

    public void CraftOnClick()
    {
        PopUpWindow popUpWindow = UIManager.Instance.PopUpWindow;
        UnityEngine.Events.UnityAction confirmAction;
        string text = "";

        switch (optionType)
        {
            case CraftingOptionType.REROLL_AFFIX:
                text = "Reroll All Affixes?";
                break;

            case CraftingOptionType.REROLL_VALUES:
                text = "Reroll Affix Values?";
                break;

            case CraftingOptionType.ADD_AFFIX:
                text = "Add a Random Affix?";
                break;

            case CraftingOptionType.REMOVE_AFFIX:
                text = "Remove a Random Affix?";
                break;

            case CraftingOptionType.UPGRADE_RARITY:
                text = "Upgrade Rarity and Add a Random Affix?";
                break;

            case CraftingOptionType.TO_NORMAL:
                text = "Clear All Affixes and Turn Item To Normal?";
                break;

            default:
                return;
        }

        confirmAction = delegate { UIManager.Instance.ItemCraftingPanel.ModifyItem(optionType); UIManager.Instance.CloseCurrentWindow(); };
        popUpWindow.OpenTextWindow(text, 380, 150);
        popUpWindow.textField.fontSize = 27;
        popUpWindow.textField.paragraphSpacing = 0;
        popUpWindow.textField.alignment = TextAlignmentOptions.Center;

        popUpWindow.SetButtonValues("-" + costText, confirmAction, "Cancel", delegate { UIManager.Instance.CloseCurrentWindow(); });
    }
}