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
    public Button delevelButton;
    public HeroData hero;
    private bool isPreviewMode;

    public void OnEnable()
    {
        ClearPanel();
        if (isPreviewMode)
        {
            levelButton.gameObject.SetActive(false);
            delevelButton.gameObject.SetActive(false);
        }
        else
        {
            levelButton.gameObject.SetActive(true);
            delevelButton.gameObject.SetActive(true);
        }
    }

    public void SetPreviewMode(bool set)
    {
        isPreviewMode = set;
    }

    public void ClearPanel()
    {
        levelButton.interactable = false;
        delevelButton.interactable = false;
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
        if (uiNode.isLevelable && !archetypeData.IsNodeMaxLevel(node) && hero.ArchetypePoints > 0)
            levelButton.interactable = true;
        else
            levelButton.interactable = false;

        if (archetypeData.GetNodeLevel(node) > 0 && node.initialLevel == 0 && IsChildrenIndependent())
            delevelButton.interactable = true;
        else
            delevelButton.interactable = false;

        infoText.text = "";
        if (node.type == NodeType.ABILITY)
        {
            string[] strings = LocalizationManager.Instance.GetLocalizationText_Ability(node.abilityId);
            infoText.text += "<b>" + strings[0] + " Lv" + hero.GetAbilityLevel() + "</b>\n";
            infoText.text += LocalizationManager.Instance.GetLocalizationText_AbilityBaseDamage(hero.GetAbilityLevel(), node.GetAbility());
            infoText.text += strings[1];
        }
        else
        {
            if (currentLevel == 0)
            {
                infoText.text += "<b>Level 1: </b>\n";
                infoText.text += node.GetBonusInfoString(1);
                if (node.maxLevel > 1)
                {
                    infoText.text += "<b>Level " + (node.maxLevel) + ":</b>\n";
                    infoText.text += node.GetBonusInfoString(node.maxLevel);
                }
                return;
            }
            if (currentLevel != 0)
            {
                infoText.text += "<b>Current: Level " + currentLevel + "</b>\n";
                infoText.text += node.GetBonusInfoString(currentLevel);
            }
            if (currentLevel != node.maxLevel)
            {
                infoText.text += "<b>Next: Level " + (currentLevel + 1) + "</b>\n";
                infoText.text += node.GetBonusInfoString(currentLevel + 1);
            }
        }
    }

    public void UpdatePanel_Preview()
    {
        infoText.text = "";
        float bonusValue;
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

                infoText.text += LocalizationManager.Instance.GetLocalizationText_BonusType(bonusProperty.bonusType, bonusProperty.modifyType, bonusValue, bonusProperty.restriction);
            }
            if (node.maxLevel != 1)
            {
                infoText.text += "<b>Level " + node.maxLevel + ":</b>\n";

                foreach (NodeScalingBonusProperty bonusProperty in node.bonuses)
                {
                    bonusValue = bonusProperty.growthValue * (node.maxLevel - 1) + bonusProperty.finalLevelValue;
                    if (bonusValue == 0 && (bonusProperty.modifyType != ModifyType.MULTIPLY || bonusProperty.modifyType != ModifyType.FIXED_TO))
                        continue;

                    infoText.text += LocalizationManager.Instance.GetLocalizationText_BonusType(bonusProperty.bonusType, bonusProperty.modifyType, bonusValue, bonusProperty.restriction);
                }
            }
        }
    }

    public void LevelUpNode()
    {
        if (archetypeData.IsNodeMaxLevel(node) || hero.ArchetypePoints <= 0)
            return;
        archetypeData.LevelUpNode(node);
        hero.ModifyArchetypePoints(-1);
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

    public void DelevelNode()
    {
        if (archetypeData.GetNodeLevel(node) == 0 || archetypeData.GetNodeLevel(node) == node.initialLevel)
            return;
        if (archetypeData.IsNodeMaxLevel(node))
        {
            foreach (ArchetypeUITreeNode uiTreeNode in uiNode.connectedNodes)
            {
                if (archetypeData.GetNodeLevel(uiTreeNode.node) == 0)
                    uiTreeNode.DisableNode();
            }
        }

        archetypeData.DelevelNode(node);
        hero.ModifyArchetypePoints(1);
        UpdatePanel();
        uiNode.UpdateNode();
    }

    private bool IsChildrenIndependent()
    {
        foreach (ArchetypeUITreeNode uiTreeNode in uiNode.connectedNodes)
        {
            if (archetypeData.GetNodeLevel(uiTreeNode.node) > 0 && !uiTreeNode.IsTherePathExcludingNode(uiNode, new System.Collections.Generic.List<ArchetypeUITreeNode>()))
            {
                Debug.Log("DEPEND " + uiTreeNode.node.idName);
                return false;
            }
        } 
        return true;
    }
}