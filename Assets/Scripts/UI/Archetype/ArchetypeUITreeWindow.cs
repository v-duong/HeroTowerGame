using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArchetypeUITreeWindow : MonoBehaviour
{
    private static Vector3 LineOffsetY = new Vector3(0,0,0);
    public ArchetypeUITreeNode nodePrefab;
    public HeroData hero;
    public HeroArchetypeData[] archetypeData = new HeroArchetypeData[2];
    public Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode> primaryNodes = new Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode>();
    public Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode> secondaryNodes = new Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode>();
    public UILineRenderer primaryTreeParent;
    public UILineRenderer secondaryTreeParent;
    public ScrollRect ScrollView;

    private void Awake()
    {
        RectTransform r = nodePrefab.transform as RectTransform;
        LineOffsetY.y = r.rect.height / 2;
    }

    public void InitializeTree(HeroData hero)
    {
        this.hero = hero;
        archetypeData[0] = hero.PrimaryArchetype;
        archetypeData[1] = hero.SecondaryArchetype;

        foreach (ArchetypeUITreeNode n in primaryNodes.Values)
        {
            n.gameObject.SetActive(false);
        }
        foreach (ArchetypeUITreeNode n in secondaryNodes.Values)
        {
            n.gameObject.SetActive(false);
        }

        primaryNodes.Clear();
        secondaryNodes.Clear();

        primaryTreeParent.Points.Clear();
        HashSet<ArchetypeSkillNode> primaryHash = new HashSet<ArchetypeSkillNode>();
        HashSet<ArchetypeSkillNode> secondaryHash = new HashSet<ArchetypeSkillNode>();
        foreach (ArchetypeSkillNode node in archetypeData[0].Base.nodeList)
        {
            if (node.initialLevel == 1)
                CreateTreeNode(node, primaryHash, archetypeData[0].Base, primaryTreeParent, primaryNodes);
        }
        if (archetypeData[1] != null)
        {
            foreach (ArchetypeSkillNode node in archetypeData[1].Base.nodeList)
            {
                if (node.initialLevel == 1)
                    CreateTreeNode(node, secondaryHash, archetypeData[1].Base, secondaryTreeParent, secondaryNodes);
            }
        }
    }

    public ArchetypeUITreeNode CreateTreeNode(ArchetypeSkillNode node, HashSet<ArchetypeSkillNode> traversedNodes, ArchetypeBase archetype, UILineRenderer parent, Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode> nodeDict)
    {
        if (node == null)
            return null;

        ArchetypeUITreeNode currentNode;
        if (traversedNodes.Add(node))
        {
            currentNode = Instantiate(nodePrefab, parent.transform);
            currentNode.SetNode(node);
            nodeDict.Add(node, currentNode);
        } else
        {
            return nodeDict[node];
        }

        foreach (int x in node.children)
        {
            ArchetypeSkillNode n = archetype.nodeList[x];
            ArchetypeUITreeNode child = CreateTreeNode(n, traversedNodes, archetype, parent, nodeDict);
            parent.AddPoints((currentNode.transform.localPosition + LineOffsetY, child.transform.localPosition + LineOffsetY));
        }

        return currentNode;
    }
}