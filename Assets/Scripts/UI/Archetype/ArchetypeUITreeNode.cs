using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArchetypeUITreeNode : MonoBehaviour
{
    private static readonly int yPositionOffset = 130;
    public ArchetypeSkillNode node;
    public TextMeshProUGUI nodeText;
    public TextMeshProUGUI levelText;
    public Button nodeButton;
    public List<ArchetypeUITreeNode> connectedNodes = new List<ArchetypeUITreeNode>();
    public HeroArchetypeData archetypeData;
    public bool isLevelable;

    public void SetNode(ArchetypeSkillNode n, HeroArchetypeData data)
    {
        this.node = n;
        this.archetypeData = data;

        UpdateNode();
        ((RectTransform)transform).anchoredPosition = new Vector3(n.nodePosition.x * 100, n.nodePosition.y * 100 + yPositionOffset, 0);
    }

    public void UpdateNode()
    {
        int level = archetypeData.GetNodeLevel(node);
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

    public void DisableNode()
    {
        isLevelable = false;
        nodeButton.image.color = new Color(0.4f, 0.4f, 0.4f, 1);
    }

    public void CheckSurroundingNodes()
    {
        if (archetypeData.GetNodeLevel(node) > 0)
        {
            EnableNode();
            return;
        }
        foreach (ArchetypeUITreeNode treeNode in connectedNodes)
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
}