using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArchetypeUITreeNode : MonoBehaviour
{
    public static readonly Color UNAVAILABLE_COLOR = Color.black;
    public static readonly Color AVAILABLE_COLOR = new Color(0.6f, 0.6f, 0.6f);
    public static readonly Color CONNECTED_COLOR = new Color(0f, 0.6f, 0f);
    public static readonly Color LEVEL_COLOR = new Color(0.3f, 0.95f, 0.1f);
    public static readonly Color MAX_LEVEL_COLOR = new Color(1f, 0.85f, 0.1f);

    private static readonly int yPositionOffset = 180;
    public ArchetypeSkillNode node;
    public TextMeshProUGUI nodeText;
    public TextMeshProUGUI levelText;
    public Button nodeButton;
    public GameObject levelIconParent;
    public List<Image> levelIcons;

    public Dictionary<ArchetypeUITreeNode, UILineRenderer.LinePoint> connectedNodes = new Dictionary<ArchetypeUITreeNode, UILineRenderer.LinePoint>();
    public HeroArchetypeData archetypeData;
    public bool isLevelable;
    private bool isPreviewMode = false;

    public void SetNode(ArchetypeSkillNode n, HeroArchetypeData data, bool isPreviewMode)
    {
        node = n;
        archetypeData = data;

        for (int i = 0; i < node.maxLevel - 1; i++)
        {
            Image newLevelIcon = Instantiate(levelIcons[0], levelIconParent.transform);
            levelIcons.Add(newLevelIcon);
        }

        if (node.type == NodeType.GREATER)
        {
            GetComponent<Image>().sprite = UIManager.Instance.ArchetypeUITreeWindow.LargeNodeImage;
            ((RectTransform)transform).sizeDelta = new Vector2(95,95);
        }

        this.isPreviewMode = isPreviewMode;

        if (isPreviewMode)
            UpdateNodePreview();
        else
            UpdateNode();
        ((RectTransform)transform).anchoredPosition = new Vector3(n.nodePosition.x * 110, n.nodePosition.y * 110 + yPositionOffset, 0);
    }

    public void UpdateNode()
    {
        int level = archetypeData.GetNodeLevel(node);
        nodeText.text = "";

        if (node.type == NodeType.ABILITY)
        {
            nodeText.text = LocalizationManager.Instance.GetLocalizationText_Ability(node.abilityId)[0];
        }
        else
        {
            foreach (NodeScalingBonusProperty nodeBonus in node.bonuses)
            {
                nodeText.text += LocalizationManager.Instance.GetBonusTypeString(nodeBonus.bonusType) + "\n";
            }
            foreach(TriggeredEffectBonusProperty nodeTrigger in node.triggeredEffects)
            {
                nodeText.text += LocalizationManager.Instance.GetLocalizationText_TriggeredEffect(nodeTrigger, nodeTrigger.effectMaxValue);
            }
        }

        levelText.text = level + "/" + node.maxLevel;

        for (int i = 0; i < node.maxLevel; i++)
        {
            if (i < level)
                levelIcons[i].color = level == node.maxLevel ? MAX_LEVEL_COLOR : LEVEL_COLOR;
            else
                levelIcons[i].color = new Color(0.25f, 0.25f, 0.25f);
        }

        if (level == node.maxLevel)
        {
            nodeButton.image.color = new Color(1f, 1f, 1f, 1);
            foreach (var x in connectedNodes)
            {
                if (archetypeData.GetNodeLevel(x.Key.node) == 0)
                    x.Value.color = AVAILABLE_COLOR;
                else
                    x.Value.color = CONNECTED_COLOR;
            }
        }
        else if (level > 0)
        {
            nodeButton.image.color = new Color(1f, 1f, 1f, 1);
            foreach (var x in connectedNodes)
            {
                if (archetypeData.GetNodeLevel(x.Key.node) > 0)
                    x.Value.color = CONNECTED_COLOR;
                else
                    x.Value.color = UNAVAILABLE_COLOR;
            }
        }
        else
        {
            nodeButton.image.color = new Color(0.8f, 0.8f, 0.8f, 1);

            foreach (var x in connectedNodes)
            {
                if (archetypeData.GetNodeLevel(x.Key.node) == x.Key.node.maxLevel)
                    x.Value.color = AVAILABLE_COLOR;
                else
                    x.Value.color = UNAVAILABLE_COLOR;
            }
        }

        UIManager.Instance.ArchetypeUITreeWindow.primaryTreeParent.SetAllDirty();
        UIManager.Instance.ArchetypeUITreeWindow.secondaryTreeParent.SetAllDirty();
    }

    public void UpdateNodePreview()
    {
        int level = 0;
        nodeText.text = node.idName;
        levelText.text = level + "/" + node.maxLevel;
        if (level > 0)
        {
            nodeButton.image.color = new Color(1f, 1f, 1f, 1);
        }
    }

    public void EnableNode()
    {
        int level = archetypeData.GetNodeLevel(node);
        isLevelable = true;
        nodeButton.image.color = new Color(0.8f, 0.8f, 0.8f, 1);
        if (level > 0)
        {
            nodeButton.image.color = new Color(1f, 1f, 1f, 1);
        }
    }

    private void EnablePreviewNode()
    {
        isLevelable = false;
        nodeButton.image.color = new Color(0.8f, 0.8f, 0.8f, 1);
    }

    public void DisableNode()
    {
        isLevelable = false;
        nodeButton.image.color = new Color(0.4f, 0.4f, 0.4f, 1);
    }

    public void CheckSurroundingNodes()
    {
        if (isPreviewMode)
        {
            EnablePreviewNode();
            return;
        }

        if (archetypeData.GetNodeLevel(node) > 0)
        {
            EnableNode();
            return;
        }
        foreach (ArchetypeUITreeNode treeNode in connectedNodes.Keys)
        {
            if (archetypeData.IsNodeMaxLevel(treeNode.node))
            {
                EnableNode();
                return;
            }
            else
                DisableNode();
        }
    }

    public void SelectNode()
    {
        ArchetypeNodeInfoPanel panel = UIManager.Instance.ArchetypeNodeInfoPanel;
        panel.SetAndUpdatePanel(node, archetypeData, this);
    }

    public bool IsTherePathExcludingNode(ArchetypeUITreeNode uiNode, List<ArchetypeUITreeNode> traversedNodes)
    {
        if (node == null || this == uiNode || traversedNodes.Contains(this))
            return false;

        traversedNodes.Add(this);

        if (node.initialLevel > 0)
            return true;

        foreach (ArchetypeUITreeNode connectedNode in connectedNodes.Keys)
        {
            if (archetypeData.GetNodeLevel(node) > 0 && connectedNode.IsTherePathExcludingNode(uiNode, traversedNodes))
                return true;
        }

        return false;
    }
}