using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArchetypeNodeInfoPanel : MonoBehaviour
{
    public HeroArchetypeData archetypeData;
    public ArchetypeSkillNode node;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI nextInfoText;
    public TextMeshProUGUI topApText;
    public ArchetypeUITreeNode uiNode;
    public GameObject buttonsParent;
    public Button levelButton;
    public Button delevelButton;
    public Button resetTreeButton;
    public HeroData hero;
    private bool isPreviewMode;

    public void OnEnable()
    {
        ClearPanel();
        if (isPreviewMode)
        {
            resetTreeButton.gameObject.SetActive(false);
            buttonsParent.SetActive(false);
            topApText.text = "";
        }
        else
        {
            resetTreeButton.gameObject.SetActive(true);
            buttonsParent.SetActive(true);
            topApText.text = "AP: " + hero.ArchetypePoints;
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
        nextInfoText.text = "";
        topApText.text = "";
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
        nextInfoText.text = "";
        topApText.text = "AP: " + hero.ArchetypePoints;
        nextInfoText.gameObject.SetActive(false);
        if (node.type == NodeType.ABILITY)
        {
            string[] strings = LocalizationManager.Instance.GetLocalizationText_Ability(node.abilityId);
            infoText.text += "<b>" + strings[0] + " Lv" + hero.GetAbilityLevel() + "</b>\n";
            infoText.text += LocalizationManager.Instance.GetLocalizationText_AbilityBaseDamage(hero.GetAbilityLevel(), node.GetAbility());
            infoText.text += node.GetAbility().GetAbilityBonusTexts(hero.GetAbilityLevel());
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
                    nextInfoText.gameObject.SetActive(true);
                    nextInfoText.text += "<b>Level " + (node.maxLevel) + ":</b>\n";
                    nextInfoText.text += node.GetBonusInfoString(node.maxLevel);
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
                nextInfoText.gameObject.SetActive(true);
                nextInfoText.text += "<b>Next: Level " + (currentLevel + 1) + "</b>\n";
                nextInfoText.text += node.GetBonusInfoString(currentLevel + 1);
            }
        }
    }

    public void UpdatePanel_Preview()
    {
        infoText.text = "";
        if (node.type == NodeType.ABILITY)
        {
            string[] strings = LocalizationManager.Instance.GetLocalizationText_Ability(node.abilityId);
            infoText.text += "<b>" + strings[0] + "</b>\n";
            infoText.text += "Lv0: " + LocalizationManager.Instance.GetLocalizationText_AbilityBaseDamage(0, node.GetAbility());
            infoText.text += node.GetAbility().GetAbilityBonusTexts(0);
            nextInfoText.gameObject.SetActive(true);
            nextInfoText.text = "Lv50: " + LocalizationManager.Instance.GetLocalizationText_AbilityBaseDamage(50, node.GetAbility());
            nextInfoText.text += node.GetAbility().GetAbilityBonusTexts(50);
            infoText.text += strings[1];
        }
        else
        {
            infoText.text += "<b>Level 1: </b>\n";

            infoText.text += node.GetBonusInfoString(1);
            if (node.maxLevel != 1)
            {
                nextInfoText.gameObject.SetActive(true);
                nextInfoText.text = "<b>Level " + node.maxLevel + ":</b>\n";

                nextInfoText.text += node.GetBonusInfoString(node.maxLevel);
            } else
            {
                nextInfoText.gameObject.SetActive(false);
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
            foreach (ArchetypeUITreeNode uiTreeNode in uiNode.connectedNodes.Keys)
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
            foreach (ArchetypeUITreeNode uiTreeNode in uiNode.connectedNodes.Keys)
            {
                if (archetypeData.GetNodeLevel(uiTreeNode.node) == 0)
                {
                    bool hasAnotherConnection = false;
                    foreach (ArchetypeUITreeNode connectedNode in uiTreeNode.connectedNodes.Keys)
                    {
                        if (connectedNode != uiNode && archetypeData.GetNodeLevel(connectedNode.node) >= connectedNode.node.maxLevel)
                        {
                            hasAnotherConnection = true;
                            break;
                        }
                    }

                    if (!hasAnotherConnection)
                        uiTreeNode.DisableNode();
                }
            }
        }

        archetypeData.DelevelNode(node);
        hero.ModifyArchetypePoints(1);
        UpdatePanel();
        uiNode.UpdateNode();
    }

    public void ResetTree()
    {
        UIManager.Instance.ArchetypeUITreeWindow.ResetCurrentTree();
    }

    private bool IsChildrenIndependent()
    {
        foreach (ArchetypeUITreeNode uiTreeNode in uiNode.connectedNodes.Keys)
        {
            if (archetypeData.GetNodeLevel(uiTreeNode.node) > 0 && !uiTreeNode.IsTherePathExcludingNode(uiNode, new System.Collections.Generic.List<ArchetypeUITreeNode>()))
            {
                return false;
            }
        }
        return true;
    }
}