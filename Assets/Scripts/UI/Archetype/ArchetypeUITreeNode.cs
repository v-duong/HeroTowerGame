using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArchetypeUITreeNode : MonoBehaviour
{
    public static readonly Color UNAVAILABLE_COLOR = Color.black;
    public static readonly Color AVAILABLE_COLOR = new Color(0.6f, 0.6f, 0.6f);
    public static readonly Color CONNECTED_COLOR = new Color(0f, 0.6f, 0f);

    private static readonly int yPositionOffset = 130;
    public ArchetypeSkillNode node;
    public TextMeshProUGUI nodeText;
    public TextMeshProUGUI levelText;
    public Button nodeButton;
    public Dictionary<ArchetypeUITreeNode, UILineRenderer.LinePoint> connectedNodes = new Dictionary<ArchetypeUITreeNode, UILineRenderer.LinePoint>();
    public HeroArchetypeData archetypeData;
    public bool isLevelable;
    private bool isPreviewMode = false;

    public void SetNode(ArchetypeSkillNode n, HeroArchetypeData data, bool isPreviewMode)
    {
        node = n;
        archetypeData = data;

        this.isPreviewMode = isPreviewMode;

        if (isPreviewMode)
            UpdateNodePreview();
        else
            UpdateNode();
        ((RectTransform)transform).anchoredPosition = new Vector3(n.nodePosition.x * 100, n.nodePosition.y * 100 + yPositionOffset, 0);
    }

    public void UpdateNode()
    {
        int level = archetypeData.GetNodeLevel(node);
        nodeText.text = node.idName;
        levelText.text = level + "/" + node.maxLevel;
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