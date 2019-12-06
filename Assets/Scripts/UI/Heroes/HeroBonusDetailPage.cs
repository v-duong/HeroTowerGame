using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class HeroBonusDetailPage : MonoBehaviour, IUpdatablePanel
{
    public TextMeshProUGUI mainText;

    private void OnEnable()
    {
        UpdateWindow();
    }

    public void UpdateWindow()
    {
        HeroData hero = HeroDetailWindow.hero;
        mainText.text = "";
        Dictionary<BonusType, HeroData.HeroBonusTotal> bonusTotalsDict = hero.GetAllBonusTotals();

        List<BonusType> sortedBonuses = bonusTotalsDict.Keys.ToList();
        sortedBonuses.Sort();

        foreach (BonusType bonusType in sortedBonuses)
        {
            HeroData.HeroBonusTotal bonusTotal = bonusTotalsDict[bonusType];
            List<GroupType> sortedGroupTypes = bonusTotal.sumByRestrictions.Keys.ToList();
            sortedGroupTypes.Sort();

            if (!sortedGroupTypes.Contains(GroupType.NO_GROUP))
                mainText.text += "○ " + LocalizationManager.GetBonusTypeString(bonusType) + '\n';

            foreach (GroupType groupType in sortedGroupTypes)
            {
                StatBonus statBonus = bonusTotal.sumByRestrictions[groupType];

                if (groupType != GroupType.NO_GROUP)
                {
                    mainText.text += "<margin=2em>";
                }

                if (statBonus.HasFixedModifier)
                {
                    mainText.text += "○ " + LocalizationManager.Instance.GetLocalizationText_BonusType(bonusType, ModifyType.FLAT_ADDITION, statBonus.FixedModifier, groupType);
                }
                else
                {
                    if (statBonus.AdditiveModifier != 0)
                        mainText.text += "○ " + LocalizationManager.Instance.GetLocalizationText_BonusType(bonusType, ModifyType.ADDITIVE, statBonus.AdditiveModifier, groupType);

                    if (statBonus.FlatModifier != 0)
                        mainText.text += "○ " + LocalizationManager.Instance.GetLocalizationText_BonusType(bonusType, ModifyType.FLAT_ADDITION, statBonus.FlatModifier, groupType);

                    if (statBonus.MultiplyModifiers.Count != 0)
                    {
                        mainText.text += "○ " + LocalizationManager.Instance.GetLocalizationText_BonusType(bonusType, ModifyType.MULTIPLY, (statBonus.CurrentMultiplier-1) * 100, groupType);
                    }
                }

                mainText.text += "</margin>";
            }
            mainText.text += "\n";
        }
    }
}