using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArchetypeNodeInfoPanel : MonoBehaviour
{
    public HeroArchetypeData archetypeData;
    public ArchetypeSkillNode node;
    public TextMeshProUGUI infoText;
    public ArchetypeUITreeNode uiNode;
    public Button levelButton;
    private bool isPreviewMode;

    public void OnEnable()
    {
        ClearPanel();
        if (isPreviewMode)
            levelButton.gameObject.SetActive(false);
        else
            levelButton.gameObject.SetActive(true);
    }

    public void SetPreviewMode(bool set)
    {
        isPreviewMode = set;
    }

    public void ClearPanel()
    {
        levelButton.interactable = false;
        infoText.text = "";
    }

    public void SetAndUpdatePanel(ArchetypeSkillNode node, HeroArchetypeData archetypeData, ArchetypeUITreeNode uiNode)
    {
        this.node = node;
        this.archetypeData = archetypeData;
        this.uiNode = uiNode;
        if (isPreviewMode)
        {
            UpdatePanel_Preview();
        }
        else
        {
            UpdatePanel();
        }
    }

    public void UpdatePanel()
    {
        int currentLevel = archetypeData.GetNodeLevel(node);
        int bonusValue;
        if (uiNode.isLevelable && !archetypeData.IsNodeMaxLevel(node))
            levelButton.interactable = true;
        else
            levelButton.interactable = false;

        infoText.text = "";
        if (node.type == NodeType.ABILITY)
        {
            string[] strings = LocalizationManager.Instance.GetLocalizationText_Ability(node.abilityId);
            infoText.text += "<b>" + strings[0] + " Lv" + archetypeData.GetAbilityLevel() + "</b>\n";
            infoText.text += LocalizationManager.Instance.GetLocalizationText_AbilityBaseDamage(archetypeData.GetAbilityLevel(), node.GetAbility());
            infoText.text += strings[1];
        }
        else
        {
            if (currentLevel != 0)
            {
                infoText.text += "<b>Current Level: " + currentLevel + "</b>\n";

                foreach (NodeScalingBonusProperty bonusProperty in node.bonuses)
                {
                    if (currentLevel == node.maxLevel && node.maxLevel > 1)

                        bonusValue = bonusProperty.growthValue * (currentLevel - 1) + bonusProperty.finalLevelValue;
                    else
                        bonusValue = bonusProperty.growthValue * (currentLevel);

                    if (bonusValue == 0 && (bonusProperty.modifyType != ModifyType.MULTIPLY || bonusProperty.modifyType != ModifyType.FIXED_TO))
                        continue;

                    infoText.text += LocalizationManager.Instance.GetLocalizationText_BonusType(bonusProperty.bonusType, bonusProperty.modifyType, bonusValue);
                }
            }
            if (currentLevel != node.maxLevel)
            {
                infoText.text += "<b>Next Level: " + (currentLevel + 1) + "</b>\n";

                foreach (NodeScalingBonusProperty bonusProperty in node.bonuses)
                {
                    if (currentLevel == node.maxLevel - 1 && node.maxLevel > 1)
                        bonusValue = bonusProperty.growthValue * (currentLevel) + bonusProperty.finalLevelValue;
                    else
                        bonusValue = bonusProperty.growthValue * (currentLevel + 1);

                    if (bonusValue == 0 && (bonusProperty.modifyType != ModifyType.MULTIPLY || bonusProperty.modifyType != ModifyType.FIXED_TO))
                        continue;

                    infoText.text += LocalizationManager.Instance.GetLocalizationText_BonusType(bonusProperty.bonusType, bonusProperty.modifyType, bonusValue);
                }
            }
        }
    }

    public void UpdatePanel_Preview()
    {
        infoText.text = "";
        int bonusValue;
        if (node.type == NodeType.ABILITY)
        {
            string[] strings = LocalizationManager.Instance.GetLocalizationText_Ability(node.abilityId);
            infoText.text += "<b>" + strings[0] + "</b>\n";
            infoText.text += "Lv0: " + LocalizationManager.Instance.GetLocalizationText_AbilityBaseDamage(0, node.GetAbility());
            infoText.text += "Lv50: " + LocalizationManager.Instance.GetLocalizationText_AbilityBaseDamage(50, node.GetAbility());
            infoText.text += strings[1];
        }
        else
        {
            infoText.text += "<b>Level 1: </b>\n";

            foreach (NodeScalingBonusProperty bonusProperty in node.bonuses)
            {
                bonusValue = bonusProperty.growthValue;
                if (bonusValue == 0 && (bonusProperty.modifyType != ModifyType.MULTIPLY || bonusProperty.modifyType != ModifyType.FIXED_TO))
                    continue;

                infoText.text += LocalizationManager.Instance.GetLocalizationText_BonusType(bonusProperty.bonusType, bonusProperty.modifyType, bonusValue);
            }
            if (node.maxLevel != 1)
            {
                infoText.text += "<b>Level " + (node.maxLevel) + ":</b>\n";

                foreach (NodeScalingBonusProperty bonusProperty in node.bonuses)
                {
                    bonusValue = bonusProperty.growthValue * (node.maxLevel - 1) + bonusProperty.finalLevelValue;
                    if (bonusValue == 0 && (bonusProperty.modifyType != ModifyType.MULTIPLY || bonusProperty.modifyType != ModifyType.FIXED_TO))
                        continue;

                    infoText.text += LocalizationManager.Instance.GetLocalizationText_BonusType(bonusProperty.bonusType, bonusProperty.modifyType, bonusValue);
                }
            }
        }
    }

    public void LevelUpNode()
    {
        if (archetypeData.IsNodeMaxLevel(node))
            return;
        archetypeData.LevelUpNode(node);
        UpdatePanel();
        uiNode.UpdateNode();
        if (archetypeData.IsNodeMaxLevel(node))
        {
            foreach (ArchetypeUITreeNode uiTreeNode in uiNode.connectedNodes)
            {
                if (archetypeData.GetNodeLevel(uiTreeNode.node) == 0)
                    uiTreeNode.EnableNode();
            }
        }
    }
}