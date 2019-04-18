using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ArchetypeUITreeNode : MonoBehaviour
{
    private static readonly int yPositionOffset = 90;
    public ArchetypeSkillNode node;
    public TextMeshProUGUI nodeText;


    public void SetNode(ArchetypeSkillNode n)
    {
        this.node = n;
        nodeText.text = n.idName;
       
        ((RectTransform)transform).anchoredPosition = new Vector3(n.nodePosition.x * 100, n.nodePosition.y * 100 + yPositionOffset, 0);
    }

}
