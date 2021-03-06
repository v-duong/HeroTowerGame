﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArchetypeUITreeWindow : MonoBehaviour
{
    private static Vector3 LineOffsetY = Vector3.zero;
    private Vector3 primaryTreeStartingView = Vector3.zero;
    private Vector3 secondaryTreeStartingView = Vector3.zero;
    public Sprite LargeNodeImage;

    public ArchetypeUITreeNode nodePrefab;
    public HeroData hero;
    public HeroArchetypeData[] archetypeData = new HeroArchetypeData[2];

    public Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode> primaryNodes = new Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode>();
    public Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode> secondaryNodes = new Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode>();
    public UILineRenderer primaryTreeParent;
    public UILineRenderer secondaryTreeParent;
    public ScrollRect ScrollView;
    public int selectedTree;
    private float largestX = 0, largestY = 0;
    private bool isPreviewMode = false;

    private void Awake()
    {
        RectTransform r = nodePrefab.transform as RectTransform;
        LineOffsetY.y = r.rect.height / 2;
    }

    private void OnDisable()
    {
        if (!isPreviewMode)
        {
            SaveManager.CurrentSave.SaveHeroData(hero);
            SaveManager.Save();
        }
    }

    public void ResetTreeView()
    {
        foreach (ArchetypeUITreeNode node in primaryNodes.Values)
        {
            node.gameObject.SetActive(false);
            Destroy(node.gameObject);
        }
        foreach (ArchetypeUITreeNode node in secondaryNodes.Values)
        {
            node.gameObject.SetActive(false);
            Destroy(node.gameObject);
        }

        primaryNodes.Clear();
        secondaryNodes.Clear();

        primaryTreeParent.Points.Clear();
        secondaryTreeParent.Points.Clear();
    }

    public void InitializeTree(HeroData hero)
    {
        this.hero = hero;
        archetypeData[0] = hero.PrimaryArchetype;
        archetypeData[1] = hero.SecondaryArchetype;

        ResetTreeView();

        if (archetypeData[0] != null)
        {
            primaryTreeStartingView = BuildArchetypeTree(archetypeData[0], primaryTreeParent, primaryNodes);
        }

        if (archetypeData[1] != null)
        {
            secondaryTreeStartingView = BuildArchetypeTree(archetypeData[1], secondaryTreeParent, secondaryNodes);
        }
    }

    public void BuildArchetypeTree(ArchetypeBase archetypeBase)
    {
        ResetTreeView();
        hero = null;
        primaryTreeStartingView = BuildArchetypeTree(archetypeBase, primaryTreeParent, primaryNodes);
    }

    private Vector3 BuildArchetypeTree(HeroArchetypeData archetype, UILineRenderer treeParent, Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode> nodeDict)
    {
        HashSet<ArchetypeSkillNode> traversedNodes = new HashSet<ArchetypeSkillNode>();
        largestX = 0;
        largestY = 0;
        Vector3 homeNodePosition = Vector3.zero;
        Vector2 halfScreen = new Vector2(0, UIManager.Instance.referenceResolution.y / 2);
        foreach (ArchetypeSkillNode node in archetype.Base.nodeList)
        {
            if (node.initialLevel == 1)
            {
                CreateTreeNode(node, traversedNodes, archetype, treeParent, nodeDict);
            }
        }
        foreach (ArchetypeUITreeNode uiNode in nodeDict.Values)
        {
            uiNode.CheckSurroundingNodes();
            if (uiNode.node.id == 0)
                homeNodePosition = (uiNode.transform as RectTransform).anchoredPosition;
        }
        treeParent.rectTransform.sizeDelta = new Vector2(largestX * 250, largestY * 160);
        return -homeNodePosition;
    }

    private Vector3 BuildArchetypeTree(ArchetypeBase archetypeBase, UILineRenderer treeParent, Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode> nodeDict)
    {
        HashSet<ArchetypeSkillNode> traversedNodes = new HashSet<ArchetypeSkillNode>();
        largestX = 0;
        largestY = 0;
        Vector3 homeNodePosition = Vector3.zero;
        Vector2 halfScreen = new Vector2(0, UIManager.Instance.referenceResolution.y / 2);
        foreach (ArchetypeSkillNode node in archetypeBase.nodeList)
        {
            if (node.initialLevel == 1)
            {
                CreateTreeNode(node, traversedNodes, archetypeBase, treeParent, nodeDict);
            }
        }
        foreach (ArchetypeUITreeNode uiNode in nodeDict.Values)
        {
            uiNode.CheckSurroundingNodes();
            if (uiNode.node.id == 0)
                homeNodePosition = (uiNode.transform as RectTransform).anchoredPosition;
        }
        treeParent.rectTransform.sizeDelta = new Vector2(largestX * 230, largestY * 150);
        return -homeNodePosition;
    }

    // Version for Archetypes attached to heroes
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
            currentNode.SetNode(node, archetype, false);
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

        int currentNodeLevel = archetype.GetNodeLevel(node);

        foreach (int x in node.children)
        {
            ArchetypeSkillNode n = archetype.Base.GetNode(x);
            ArchetypeUITreeNode child = CreateTreeNode(n, traversedNodes, archetype, parent, nodeDict);

            Color color = Color.black;
            int childNodeLevel = archetype.GetNodeLevel(n);
            if (currentNodeLevel > 0 && childNodeLevel > 0)
            {
                color = ArchetypeUITreeNode.CONNECTED_COLOR;
            }
            else if (currentNodeLevel > 0 || childNodeLevel > 0)
            {
                color = ArchetypeUITreeNode.AVAILABLE_COLOR;
            }

            UILineRenderer.LinePoint point = new UILineRenderer.LinePoint(currentNode.transform.localPosition + LineOffsetY, child.transform.localPosition + LineOffsetY, color);
            currentNode.connectedNodes.Add(child, point);
            child.connectedNodes.Add(currentNode, point);
            parent.AddPoints(point);
        }

        return currentNode;
    }

    // Version for archetype items
    private ArchetypeUITreeNode CreateTreeNode(ArchetypeSkillNode node, HashSet<ArchetypeSkillNode> traversedNodes,
    ArchetypeBase archetypeBase, UILineRenderer parent, Dictionary<ArchetypeSkillNode, ArchetypeUITreeNode> nodeDict)
    {
        if (node == null)
            return null;

        ArchetypeUITreeNode currentNode;

        // Check if node has been traversed yet
        // if already created, just return the node
        if (traversedNodes.Add(node))
        {
            currentNode = Instantiate(nodePrefab, parent.transform);
            currentNode.SetNode(node, null, true);
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
            ArchetypeSkillNode n = archetypeBase.GetNode(x);
            ArchetypeUITreeNode child = CreateTreeNode(n, traversedNodes, archetypeBase, parent, nodeDict);
            currentNode.connectedNodes.Add(child, null);
            child.connectedNodes.Add(currentNode, null);
            UILineRenderer.LinePoint point = new UILineRenderer.LinePoint(currentNode.transform.localPosition + LineOffsetY, child.transform.localPosition + LineOffsetY, Color.black);
            parent.AddPoints(point);
        }

        return currentNode;
    }

    public void OpenArchetypeTree(HeroData hero, bool rebuildTree, int treeIndex)
    {
        isPreviewMode = false;
        UIManager.Instance.ArchetypeNodeInfoPanel.SetPreviewMode(false);
        UIManager.Instance.ArchetypeNodeInfoPanel.hero = hero;
        selectedTree = treeIndex;

        if (treeIndex == 0)
        {
            primaryTreeParent.gameObject.SetActive(true);
            secondaryTreeParent.gameObject.SetActive(false);
            ScrollView.content = primaryTreeParent.rectTransform;
        }
        else
        {
            primaryTreeParent.gameObject.SetActive(false);
            secondaryTreeParent.gameObject.SetActive(true);
            ScrollView.content = secondaryTreeParent.rectTransform;
        }

        UIManager.Instance.OpenWindow(gameObject, true);

        if (rebuildTree)
        {
            InitializeTree(hero);
        }

        SetTreeStartingView(treeIndex);
    }

    public void ResetCurrentTree()
    {
        archetypeData[selectedTree].ResetArchetypeTree();
        InitializeTree(hero);
    }

    public void OpenPreviewTree(ArchetypeBase archetype)
    {
        isPreviewMode = true;
        UIManager.Instance.ArchetypeNodeInfoPanel.SetPreviewMode(true);
        secondaryTreeParent.gameObject.SetActive(false);
        primaryTreeParent.gameObject.SetActive(true);
        ScrollView.content = primaryTreeParent.rectTransform;
        UIManager.Instance.OpenWindow(gameObject, false);
        BuildArchetypeTree(archetype);
        SetTreeStartingView(0);
    }

    public void SetTreeStartingView(int treeNum)
    {
        primaryTreeParent.transform.localScale = Vector3.one;
        secondaryTreeParent.transform.localScale = Vector3.one;
        (primaryTreeParent.transform as RectTransform).anchoredPosition = primaryTreeStartingView;
        (secondaryTreeParent.transform as RectTransform).anchoredPosition = secondaryTreeStartingView;
    }
}