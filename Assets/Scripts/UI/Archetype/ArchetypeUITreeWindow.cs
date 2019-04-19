using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArchetypeUITreeWindow : MonoBehaviour
{
    private static Vector3 LineOffsetY = new Vector3(0, 0, 0);
    public ArchetypeUITreeNode nodePrefab;
    public HeroData hero;
    public HeroArchetypeData[] archetypeData = new HeroArchetypeData[2];
    //public Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode> primaryNodes = new Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode>();
    //public Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode> secondaryNodes = new Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode>();
    public Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode> primaryNodes = new Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode>();
    public Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode> secondaryNodes = new Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode>();
    public UILineRenderer primaryTreeParent;
    public UILineRenderer secondaryTreeParent;
    public ScrollRect ScrollView;
    private float largestX = 0, largestY = 0;

    private void Awake()
    {
        RectTransform r = nodePrefab.transform as RectTransform;
        LineOffsetY.y = r.rect.height / 2;
    }

    public void ResetTreeView()
    {
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
        secondaryTreeParent.Points.Clear();
    }

    public void InitializeTree(HeroData hero)
    {
        HashSet<ArchetypeSkillNode> primaryHash = new HashSet<ArchetypeSkillNode>();
        HashSet<ArchetypeSkillNode> secondaryHash = new HashSet<ArchetypeSkillNode>();

        this.hero = hero;
        archetypeData[0] = hero.PrimaryArchetype;
        archetypeData[1] = hero.SecondaryArchetype;

        ResetTreeView();

        largestX = 0;
        largestY = 0;
        foreach (ArchetypeSkillNode node in archetypeData[0].Base.nodeList)
        {
            if (node.initialLevel == 1)
            {
                CreateTreeNode(node, primaryHash, archetypeData[0], primaryTreeParent, primaryNodes);
            }
        }
        foreach (ArchetypeUITreeNode uiNode in primaryNodes.Values)
        {
            uiNode.CheckSurroundingNodes();
        }
        primaryTreeParent.rectTransform.sizeDelta = new Vector2(largestX * 230, largestY * 150);

        largestX = 0;
        largestY = 0;
        if (archetypeData[1] != null)
        {
            foreach (ArchetypeSkillNode node in archetypeData[1].Base.nodeList)
            {
                if (node.initialLevel == 1)
                {
                    CreateTreeNode(node, secondaryHash, archetypeData[1], secondaryTreeParent, secondaryNodes);
                }
            }
            foreach (ArchetypeUITreeNode uiNode in secondaryNodes.Values)
            {
                uiNode.CheckSurroundingNodes();
            }
            secondaryTreeParent.rectTransform.sizeDelta = new Vector2(largestX * 230, largestY * 150);
        }
    }


    private ArchetypeUITreeNode CreateTreeNode(ArchetypeSkillNode node, HashSet<ArchetypeSkillNode> traversedNodes,
        HeroArchetypeData archetype, UILineRenderer parent, Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode> nodeDict)
    {
        if (node == null)
            return null;

        ArchetypeUITreeNode currentNode;

        // Check if node has been traversed yet
        // if already created, just return the node
        if (traversedNodes.Add(node))
        {
            currentNode = Instantiate(nodePrefab, parent.transform);
            currentNode.SetNode(node, archetype);
            nodeDict.Add(node, currentNode);
            if (Math.Abs(node.nodePosition.x) > largestX)
                largestX = Math.Abs(node.nodePosition.x);
            if (Math.Abs(node.nodePosition.y) > largestY)
                largestY = Math.Abs(node.nodePosition.y);

        }
        else
        {
            return nodeDict[node];
        }

        foreach (int x in node.children)
        {
            ArchetypeSkillNode n = archetype.Base.GetNode(x);
            ArchetypeUITreeNode child = CreateTreeNode(n, traversedNodes, archetype, parent, nodeDict);
            currentNode.connectedNodes.Add(child);
            child.connectedNodes.Add(currentNode);
            parent.AddPoints((currentNode.transform.localPosition + LineOffsetY, child.transform.localPosition + LineOffsetY));
        }

        return currentNode;
    }

    public void OpenPrimaryTree()
    {
        secondaryTreeParent.gameObject.SetActive(false);
        primaryTreeParent.gameObject.SetActive(true);
        (primaryTreeParent.transform as RectTransform).anchoredPosition = new Vector2(0, 0);
        ScrollView.content = primaryTreeParent.rectTransform;

    }

    public void OpenSecondaryTree()
    {
        primaryTreeParent.gameObject.SetActive(false);
        secondaryTreeParent.gameObject.SetActive(true);
        (secondaryTreeParent.transform as RectTransform).anchoredPosition = new Vector2(0, 0);
        ScrollView.content = secondaryTreeParent.rectTransform;

    }
}