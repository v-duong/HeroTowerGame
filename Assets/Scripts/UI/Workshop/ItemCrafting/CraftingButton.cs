using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingButton : MonoBehaviour
{
    [SerializeField]
    private CraftingOptionType optionType;

    private string costText;

    private enum CraftingOptionType
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
            int cost = 0;
            switch (optionType)
            {
                case CraftingOptionType.REROLL_AFFIX when currentItem.Rarity != RarityType.NORMAL && currentItem.Rarity != RarityType.UNIQUE:
                    cost = AffixedItem.GetRerollAffixCost(currentItem);
                    break;

                case CraftingOptionType.REROLL_VALUES when currentItem.Rarity != RarityType.NORMAL:
                    cost = AffixedItem.GetRerollValuesCost(currentItem);
                    break;

                case CraftingOptionType.ADD_AFFIX when currentItem.GetRandomOpenAffixType() != null:
                    cost = AffixedItem.GetAddAffixCost(currentItem);
                    break;

                case CraftingOptionType.REMOVE_AFFIX when currentItem.GetRandomAffix() != null:
                    cost = AffixedItem.GetRemoveAffixCost(currentItem);
                    break;

                case CraftingOptionType.UPGRADE_RARITY when currentItem.Rarity != RarityType.UNIQUE && currentItem.Rarity != RarityType.EPIC:
                    cost = AffixedItem.GetUpgradeCost(currentItem);
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

            costText = cost.ToString("N0") + " <sprite=10>";

            if (cost != 0)
            {
                GetComponent<Button>().interactable = cost <= itemFragments;
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
                confirmAction = delegate { UIManager.Instance.ItemCraftingPanel.RerollAffixOnClick(); UIManager.Instance.CloseCurrentWindow(); };
                break;

            case CraftingOptionType.REROLL_VALUES:
                text = "Reroll Affix Values?";
                confirmAction = delegate { UIManager.Instance.ItemCraftingPanel.RerollValuesOnClick(); UIManager.Instance.CloseCurrentWindow(); };
                break;

            case CraftingOptionType.ADD_AFFIX:
                text = "Add a Random Affix?";
                confirmAction = delegate { UIManager.Instance.ItemCraftingPanel.AddAffixOnClick(); UIManager.Instance.CloseCurrentWindow(); };
                break;

            case CraftingOptionType.REMOVE_AFFIX:
                text = "Remove a Random Affix?";
                confirmAction = delegate { UIManager.Instance.ItemCraftingPanel.RemoveAffixOnClick(); UIManager.Instance.CloseCurrentWindow(); };
                break;

            case CraftingOptionType.UPGRADE_RARITY:
                text = "Upgrade Rarity and Add a Random Affix?";
                confirmAction = delegate { UIManager.Instance.ItemCraftingPanel.UpgradeRarityOnClick(); UIManager.Instance.CloseCurrentWindow(); };
                break;

            case CraftingOptionType.TO_NORMAL:
                text = "Clear All Affixes and Turn Item To Normal?";
                confirmAction = delegate { UIManager.Instance.ItemCraftingPanel.ToNormalOnClick(); UIManager.Instance.CloseCurrentWindow(); };
                break;

            default:
                return;
        }

        popUpWindow.OpenTextWindow(text, 400, 150);
        popUpWindow.textField.fontSize = 27;
        popUpWindow.textField.paragraphSpacing = 0;
        popUpWindow.textField.alignment = TextAlignmentOptions.Center;

        popUpWindow.SetButtonValues("-" + costText, confirmAction, "Cancel", delegate { UIManager.Instance.CloseCurrentWindow(); });
    }
}