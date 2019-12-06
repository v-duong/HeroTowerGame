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
    public Button swapButton;
    public Button confirmButton;
    public TextMeshProUGUI archetypeSlot1Text;
    public TextMeshProUGUI archetypeSlot2Text;
    public TextMeshProUGUI archetypeSlot1GrowthText;
    public TextMeshProUGUI archetypeSlot2GrowthText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI placeholderText;
    private int selectedSlot;
    public ArchetypeItem primaryArchetype;
    public ArchetypeItem secondaryArchetype;
    public HeroCreationStatLabel healthLine;
    public HeroCreationStatLabel strLine;
    public HeroCreationStatLabel intLine;
    public HeroCreationStatLabel agiLine;
    public HeroCreationStatLabel willLine;
    public string HeroName;

    private void OnEnable()
    {
        HeroName = "";
        primaryArchetype = null;
        secondaryArchetype = null;
        UpdatePanels();
    }

    public void ConfirmCreateOnClick()
    {
        if (primaryArchetype == null)
            return;
        if (primaryArchetype.Base == secondaryArchetype?.Base)
            return;
        HeroData hero = HeroData.CreateNewHero(HeroName, primaryArchetype.Base, secondaryArchetype?.Base);
        PlayerStats player = GameManager.Instance.PlayerStats;
        player.AddHeroToList(hero);
        player.RemoveArchetypeFromInventory(primaryArchetype);
        primaryArchetype = null;

        if (secondaryArchetype != null)
        {
            player.RemoveArchetypeFromInventory(secondaryArchetype);
            secondaryArchetype = null;
        }

        HeroName = "";
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

    public void SwapButtonClick()
    {
        if (primaryArchetype != null && secondaryArchetype != null)
        {
            ArchetypeItem temp = primaryArchetype;
            primaryArchetype = secondaryArchetype;
            secondaryArchetype = temp;
            UpdatePanels();
        }
    }

    public void HeroNameOnClick()
    {
        UIManager.Instance.PopUpWindow.OpenTextInput(HeroName);
        UIManager.Instance.PopUpWindow.textInput.characterLimit = 20;
        UIManager.Instance.PopUpWindow.textInput.contentType = TMP_InputField.ContentType.Alphanumeric;
        UIManager.Instance.PopUpWindow.textInput.lineType = TMP_InputField.LineType.SingleLine;
        UIManager.Instance.PopUpWindow.SetButtonValues("Confirm", delegate
        {
            UIManager.Instance.CloseCurrentWindow();
            HeroName = UIManager.Instance.PopUpWindow.textInput.text;
            UpdatePanels();
        }, null, null);
    }

    public void UpdatePanels()
    {
        nameText.text = HeroName;
        if (HeroName == "")
            placeholderText.gameObject.SetActive(true);
        else
            placeholderText.gameObject.SetActive(false);
        archetypeSlot2.interactable = false;
        previewTreeButton1.interactable = false;
        previewTreeButton2.interactable = false;
        confirmButton.interactable = false;
        float healthGrow = 0, strGrow = 0, intGrow = 0, agiGrow = 0, willGrow = 0;

        if (primaryArchetype == null && secondaryArchetype == null)
        {
            archetypeSlot1Text.text = "Select Primary Archetype";
            archetypeSlot2Text.text = "Select Secondary Archetype";
            archetypeSlot1GrowthText.text = "";
            archetypeSlot2GrowthText.text = "";
            swapButton.interactable = false;
            healthLine.ClearValues();
            strLine.ClearValues();
            intLine.ClearValues();
            agiLine.ClearValues();
            willLine.ClearValues();
            return;
        }

        if (primaryArchetype != null)
        {
            archetypeSlot1Text.text = primaryArchetype.Name;
            archetypeSlot1GrowthText.text = "<b>Applied Growths</b>\n";
            archetypeSlot1GrowthText.text += "Health: " + primaryArchetype.Base.healthGrowth.ToString("N2") + "\n";
            //archetypeSlot1GrowthText.text += "SP: " + primaryArchetype.Base.soulPointGrowth.ToString("N2") + "\n";
            archetypeSlot1GrowthText.text += "STR: " + primaryArchetype.Base.strengthGrowth.ToString("N2") + "\n";
            archetypeSlot1GrowthText.text += "INT: " + primaryArchetype.Base.intelligenceGrowth.ToString("N2") + "\n";
            archetypeSlot1GrowthText.text += "AGI: " + primaryArchetype.Base.agilityGrowth.ToString("N2") + "\n";
            archetypeSlot1GrowthText.text += "WIL: " + primaryArchetype.Base.willGrowth.ToString("N2") + "\n";

            healthGrow = primaryArchetype.Base.healthGrowth;
            strGrow = primaryArchetype.Base.strengthGrowth;
            intGrow = primaryArchetype.Base.intelligenceGrowth;
            agiGrow = primaryArchetype.Base.agilityGrowth;
            willGrow = primaryArchetype.Base.willGrowth;

            archetypeSlot2.interactable = true;
            previewTreeButton1.interactable = true;

            if (secondaryArchetype != null)
            {
                archetypeSlot2Text.text = secondaryArchetype.Name;
                archetypeSlot2GrowthText.text = "<b>Applied Growths (Base)</b>\n";
                archetypeSlot2GrowthText.text += "Health: " + (secondaryArchetype.Base.healthGrowth / 4).ToString("N2") + " (" + secondaryArchetype.Base.healthGrowth.ToString("N2") + ")\n";
                //archetypeSlot2GrowthText.text += "SP: " + (secondaryArchetype.Base.soulPointGrowth / 2).ToString("N2") + " (" + secondaryArchetype.Base.soulPointGrowth.ToString("N2") + ")\n";
                archetypeSlot2GrowthText.text += "STR: " + (secondaryArchetype.Base.strengthGrowth / 2).ToString("N2") + " (" + secondaryArchetype.Base.strengthGrowth.ToString("N2") + ")\n";
                archetypeSlot2GrowthText.text += "INT: " + (secondaryArchetype.Base.intelligenceGrowth / 2).ToString("N2") + " (" + secondaryArchetype.Base.intelligenceGrowth.ToString("N2") + ")\n";
                archetypeSlot2GrowthText.text += "AGI: " + (secondaryArchetype.Base.agilityGrowth / 2).ToString("N2") + " (" + secondaryArchetype.Base.agilityGrowth.ToString("N2") + ")\n";
                archetypeSlot2GrowthText.text += "WIL: " + (secondaryArchetype.Base.willGrowth / 2).ToString("N2") + " (" + secondaryArchetype.Base.willGrowth.ToString("N2") + ")\n";

                healthGrow += secondaryArchetype.Base.healthGrowth / 4;
                strGrow += secondaryArchetype.Base.strengthGrowth / 2;
                intGrow += secondaryArchetype.Base.intelligenceGrowth / 2;
                agiGrow += secondaryArchetype.Base.agilityGrowth / 2;
                willGrow += secondaryArchetype.Base.willGrowth / 2;

                previewTreeButton2.interactable = true;
                swapButton.interactable = true;
            }

            healthLine.SetValues(healthGrow, healthGrow * 100 + 100);
            strLine.SetValues(strGrow, strGrow * 100 + 10);
            intLine.SetValues(intGrow, intGrow * 100 + 10);
            agiLine.SetValues(agiGrow, agiGrow * 100 + 10);
            willLine.SetValues(willGrow, willGrow * 100 + 10);

            if (!string.IsNullOrEmpty(HeroName))
                confirmButton.interactable = true;
        }
    }
}