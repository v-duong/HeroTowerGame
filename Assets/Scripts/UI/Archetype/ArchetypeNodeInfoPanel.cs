﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArchetypeNodeInfoPanel : MonoBehaviour
{
    public HeroArchetypeData archetypeData;
    public ArchetypeSkillNode node;
    public TextMeshProUGUI infoText;
    public ArchetypeUITreeNode uiNode;
    public Button levelButton;

    public void OnEnable()
    {
        ClearPanel();
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
        UpdatePanel();
    }

    public void UpdatePanel()
    {
        int currentLevel = archetypeData.GetNodeLevel(node);
        if (uiNode.isLevelable && !archetypeData.IsNodeMaxLevel(node))
            levelButton.interactable = true;
        else
            levelButton.interactable = false;

        infoText.text = "";
        if (node.type == NodeType.ABILITY)
        {
            string[] strings = LocalizationManager.Instance.GetLocalizationText_Ability(node.abilityId);
            infoText.text += "<b>" + strings[0] + "</b>\n";
            infoText.text += LocalizationManager.Instance.GetLocalizationText_AbilityBaseDamage(archetypeData.GetAbilityLevel(), node.GetAbility());
            infoText.text += strings[1];
        }
        else
        {
            if (currentLevel != 0)
            {
                infoText.text += "<b>Current Level: " + currentLevel + "</b>\n";
                foreach (ScalingBonusProperty bonusProperty in node.bonuses)
                {
                    infoText.text += LocalizationManager.Instance.GetLocalizationText_BonusType(bonusProperty.bonusType, bonusProperty.modifyType, bonusProperty.initialValue + bonusProperty.growthValue * (currentLevel - 1));
                }
            }
            if (currentLevel != node.maxLevel)
            {
                infoText.text += "<b>Next Level: " + (currentLevel + 1) + "</b>\n";
                foreach (ScalingBonusProperty bonusProperty in node.bonuses)
                {
                    infoText.text += LocalizationManager.Instance.GetLocalizationText_BonusType(bonusProperty.bonusType, bonusProperty.modifyType, bonusProperty.initialValue + bonusProperty.growthValue * currentLevel);
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