using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroCreationPanel : MonoBehaviour
{
    public Button archetypeSlot1;
    public Button archetypeSlot2;
    public Button previewTreeButton1;
    public Button previewTreeButton2;
    private TextMeshProUGUI archetypeSlot1Text;
    private TextMeshProUGUI archetypeSlot2Text;
    private int selectedSlot;
    public ArchetypeItem primaryArchetype;
    public ArchetypeItem secondaryArchetype;

    public TextMeshProUGUI infoText;

    private void OnEnable()
    {
        primaryArchetype = null;
        secondaryArchetype = null;
        if (archetypeSlot1Text == null)
        {
            archetypeSlot1Text = archetypeSlot1.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (archetypeSlot2Text == null)
        {
            archetypeSlot2Text = archetypeSlot2.GetComponentInChildren<TextMeshProUGUI>();
        }
        archetypeSlot2.interactable = false;
        previewTreeButton1.interactable = false;
        previewTreeButton2.interactable = false;
        UpdatePanels();
    }

    public void ConfirmCreateOnClick()
    {
        if (primaryArchetype == null)
            return;
        if (primaryArchetype.Base == secondaryArchetype?.Base)
            return;
        HeroData hero = HeroData.CreateNewHero("CREATEDHERO" + UnityEngine.Random.Range(3, 240), primaryArchetype, secondaryArchetype);
        PlayerStats player = GameManager.Instance.PlayerStats;
        player.AddHeroToList(hero);
        player.RemoveArchetypeFromInventory(primaryArchetype);
        primaryArchetype = null;
        player.RemoveArchetypeFromInventory(secondaryArchetype);
        secondaryArchetype = null;
        UpdatePanels();
    }

    public void PreviewTreeOnClick(int slotNum)
    {
        ArchetypeUITreeWindow treeWindow = UIManager.Instance.ArchetypeUITreeWindow;
        
        
        if (slotNum == 1)
        {
            if (primaryArchetype == null)
                return;
            treeWindow.OpenPreviewTree(primaryArchetype.Base);
        }

        if (slotNum == 2)
        {
            if (secondaryArchetype == null)
                return;
            treeWindow.OpenPreviewTree(secondaryArchetype.Base);
        }
    }

    public void ArchetypeSlotOnClick(int slotNum)
    {
        selectedSlot = slotNum;
        UIManager.Instance.OpenInventoryWindow(false, false, false);
        if (selectedSlot == 1)
        {
            UIManager.Instance.InvScrollContent.ShowArchetypesFiltered(new List<ArchetypeBase>() { secondaryArchetype?.Base }, true, true);
        }
        else if (selectedSlot == 2)
        {
            UIManager.Instance.InvScrollContent.ShowArchetypesFiltered(new List<ArchetypeBase>() { primaryArchetype?.Base }, true, true);
        }
        UIManager.Instance.InvScrollContent.SetCallback(ArchetypeSlotOnClick_Callback);
    }

    public void ArchetypeSlotOnClick_Callback(Item item)
    {
        if (item != null && item.GetItemType() != ItemType.ARCHETYPE)
        {
            return;
        }
        if (selectedSlot == 1)
        {
            if (item != null)
            {
                primaryArchetype = item as ArchetypeItem;
            }
            else
            {
                primaryArchetype = secondaryArchetype;
                secondaryArchetype = null;
            }
        }
        else if (selectedSlot == 2)
        {
            if (item != null)
            {
                secondaryArchetype = item as ArchetypeItem;
                archetypeSlot2Text.text = secondaryArchetype.Name;
            }
            else
            {
                secondaryArchetype = null;
            }
        }
        UpdatePanels();
        UIManager.Instance.CloseCurrentWindow();
    }

    public void UpdatePanels()
    {
        infoText.text = "";
        if (primaryArchetype == null && secondaryArchetype == null)
        {
            archetypeSlot1Text.text = "BLANK";
            archetypeSlot2Text.text = "BLANK";
            previewTreeButton1.interactable = false;
            previewTreeButton2.interactable = false;
        }
        else if (primaryArchetype != null && secondaryArchetype == null)
        {
            archetypeSlot1Text.text = primaryArchetype.Name;
            archetypeSlot2Text.text = "BLANK";
            infoText.text += "Health Growth: " + primaryArchetype.Base.healthGrowth + "\n";
            infoText.text += "SP Growth: " + primaryArchetype.Base.soulPointGrowth + "\n";
            infoText.text += "STR Growth: " + primaryArchetype.Base.strengthGrowth + "\n";
            infoText.text += "INT Growth: " + primaryArchetype.Base.intelligenceGrowth + "\n";
            infoText.text += "AGI Growth: " + primaryArchetype.Base.agilityGrowth + "\n";
            infoText.text += "WIL Growth: " + primaryArchetype.Base.willGrowth + "\n";
            archetypeSlot2.interactable = true;
            previewTreeButton1.interactable = true;
            previewTreeButton2.interactable = false;
        }
        else if (primaryArchetype != null && secondaryArchetype != null)
        {
            archetypeSlot1Text.text = primaryArchetype.Name;
            archetypeSlot2Text.text = secondaryArchetype.Name;
            infoText.text += "Health Growth: " + (primaryArchetype.Base.healthGrowth + secondaryArchetype.Base.healthGrowth / 2) + "\n";
            infoText.text += "SP Growth: " + (primaryArchetype.Base.soulPointGrowth + secondaryArchetype.Base.soulPointGrowth / 2) + "\n";
            infoText.text += "STR Growth: " + (primaryArchetype.Base.strengthGrowth + secondaryArchetype.Base.strengthGrowth / 2) + "\n";
            infoText.text += "INT Growth: " + (primaryArchetype.Base.intelligenceGrowth + secondaryArchetype.Base.intelligenceGrowth / 2) + "\n";
            infoText.text += "AGI Growth: " + (primaryArchetype.Base.agilityGrowth + secondaryArchetype.Base.agilityGrowth / 2) + "\n";
            infoText.text += "WIL Growth: " + (primaryArchetype.Base.willGrowth + secondaryArchetype.Base.willGrowth / 2) + "\n";
            previewTreeButton1.interactable = true;
            previewTreeButton2.interactable = true;
        }
    }
}