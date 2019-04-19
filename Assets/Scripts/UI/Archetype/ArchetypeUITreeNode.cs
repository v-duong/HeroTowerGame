using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArchetypeUITreeNode : MonoBehaviour
{
    private static readonly int yPositionOffset = 90;
    public ArchetypeSkillNode node;
    public TextMeshProUGUI nodeText;
    public TextMeshProUGUI levelText;
    public Button nodeButton;
    public List<ArchetypeUITreeNode> childNodes = new List<ArchetypeUITreeNode>();
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
        nodeText.text = node.idName;
        levelText.text = archetypeData.GetNodeLevel(node) + "/" + node.maxLevel;
    }

    public void EnableNode()
    {
        isLevelable = true;

        nodeButton.image.color = new Color(1, 1, 1, 1);

    }

    public void DisableNode()
    {
        isLevelable = false;
        nodeButton.image.color = new Color(0.7f, 0.7f, 0.7f, 1);
    }

    public void SelectNode()
    {
        ArchetypeNodeInfoPanel panel = UIManager.Instance.ArchetypeNodeInfoPanel;
        panel.SetAndUpdatePanel(node, archetypeData, this);
    }
}