using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingButton : MonoBehaviour
{
    [SerializeField]
    private CraftingOptionType optionType;

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

                case CraftingOptionType.LOCK_AFFIX when currentItem.GetRandomAffix() != null && currentItem.GetLockCount() < 1:
                    cost = AffixedItem.GetLockCost(currentItem);
                    break;

                default:
                    DisableButton(button);
                    return;
            }

            if (cost != 0)
            {
                GetComponent<Button>().interactable = cost <= itemFragments;
                GetComponentInChildren<TextMeshProUGUI>().text += "\n" + cost.ToString("N0");
            }
        }
    }

    private void DisableButton(UIKeyButton button)
    {
        GetComponent<Button>().interactable = false;
    }

}