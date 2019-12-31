using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    private static StringBuilder stringBuilder = new StringBuilder(128);
    private static StringBuilder stringBuilder2 = new StringBuilder(128);
    public Item item;
    public Image slotImage;
    public Image lockImage;
    public Image selectedImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI infoText1;
    public TextMeshProUGUI infoText2;
    public TextMeshProUGUI groupText;
    public TextMeshProUGUI slotText;
    public TextMeshProUGUI baseItemText;
    public TextMeshProUGUI prefixText;
    public TextMeshProUGUI suffixText;
    public TextMeshProUGUI equippedToText;
    public GameObject affixParent;
    public Action<Item> onClickAction;
    public GameObject allTextParent;
    public bool multiSelectMode = false;
    public bool alreadySelected = false;
    public bool showItemValue = false;
    public bool isHeroSlot = false;
    public DateTime lastModifyTime;


    public void SetTextVisiblity(bool visible)
    {
        allTextParent.SetActive(visible);
    }

    public void ClearSlot()
    {
        nameText.text = "Empty";
        infoText1.text = "";
        infoText2.text = "";
        groupText.text = "";
        slotText.text = "";
        prefixText.text = "";
        suffixText.text = "";
        baseItemText.text = "";
        equippedToText.text = "";
        lockImage.gameObject.SetActive(false);
        slotImage.color = Helpers.NORMAL_COLOR;
    }

    public void UpdateSlot()
    {
        groupText.text = "";
        slotText.text = "";
        prefixText.text = "";
        suffixText.text = "";
        baseItemText.text = "";
        equippedToText.text = "";

        stringBuilder.Clear();
        stringBuilder2.Clear();
        lockImage.gameObject.SetActive(false);
        if (item == null)
        {
            nameText.text = "Remove item";
            infoText1.text = "";
            infoText2.text = "";
            slotImage.color = Color.cyan;
            return;
        }

        nameText.text = item.Name + "\n";

        switch (item.GetItemType())
        {
            case ItemType.ARMOR:
                Armor armor = item as Armor;
                groupText.text = LocalizationManager.Instance.GetLocalizationText(armor.Base.group);
                slotText.text = LocalizationManager.Instance.GetLocalizationText(armor.Base.equipSlot);
                stringBuilder.AppendFormat("AR: {0} \n", armor.armor);
                stringBuilder.AppendFormat("MS: {0}", armor.shield);
                stringBuilder2.AppendFormat("DR: {0}\n", armor.dodgeRating);
                stringBuilder2.AppendFormat("RR: {0}", armor.resolveRating);
                if (armor.GetGroupTypes().Contains(GroupType.SHIELD))
                {
                    stringBuilder.AppendFormat("\nBlock%: {0}%", armor.blockChance);
                    stringBuilder2.AppendFormat("\nBlockDmg: {0}%", armor.blockProtection);
                }
                break;

            case ItemType.WEAPON:
                Weapon weapon = item as Weapon;
                groupText.text = LocalizationManager.Instance.GetLocalizationText(weapon.Base.group);

                stringBuilder.AppendFormat("<sprite=0> DPS: {0:n1} \n", weapon.GetPhysicalDPS());
                stringBuilder.AppendFormat("<sprite=7> DPS: {0:n1} \n", weapon.GetElementalDPS());
                stringBuilder.AppendFormat("<sprite=8> DPS: {0:n1}", weapon.GetPrimordialDPS());

                stringBuilder2.AppendFormat("Crit: {0:n2}%\n", weapon.CriticalChance);
                stringBuilder2.AppendFormat("APS: {0:n2}/s\n", weapon.AttackSpeed);
                stringBuilder2.AppendFormat("Range: {0:n2}", weapon.WeaponRange);
                break;

            case ItemType.ACCESSORY:
                Accessory accessory = item as Accessory;
                groupText.text = LocalizationManager.Instance.GetLocalizationText(accessory.Base.group);

                if (!UIManager.Instance.InvScrollContent.showItemAffixes || isHeroSlot)
                {
                    if (accessory.prefixes.Count > 0)
                    {
                        foreach (Affix prefix in accessory.prefixes)
                        {
                            stringBuilder.Append( Affix.BuildAffixString(prefix.Base, 0, prefix, prefix.GetAffixValues(), prefix.GetEffectValues()));
                        }
                    }

                    if (accessory.suffixes.Count > 0)
                    {
                        foreach (Affix suffix in accessory.suffixes)
                        {
                            stringBuilder2.Append(Affix.BuildAffixString(suffix.Base, 0, suffix, suffix.GetAffixValues(), suffix.GetEffectValues()));
                        }
                    }
                }

                break;

            default:
                break;
        }

        infoText1.text = stringBuilder.ToString();
        infoText2.text = stringBuilder2.ToString();

        if (item is Equipment equip)
        {
            if (equip.IsEquipped)
                equippedToText.text = "Equipped to\n" + equip.equippedToHero.Name;

            baseItemText.text = "iLvl " + equip.ItemLevel;

            if (item.Rarity > RarityType.UNCOMMON && item.Rarity != RarityType.UNIQUE)
            {
                baseItemText.text += " " + equip.Base.LocalizedName;
            }
            /*
            if (equip.innate.Count > 0)
            {
                affixText.text += "<b>Innate</b>\n";
                foreach(var x in equip.innate)
                {
                    affixText.text += Affix.BuildAffixString(x.Base, 0, x, x.GetAffixValues(), x.GetEffectValues());
                }
            }
            */
            if (UIManager.Instance.InvScrollContent.showItemAffixes && !isHeroSlot)
            {
                ExpandItemSlot(equip);
            }
            else
            {
                DeflateItemSlot();
            }
            slotImage.color = Helpers.ReturnRarityColor(item.Rarity);
        } else if (item is ArchetypeItem archetypeItem)
        {
            slotImage.color = Helpers.GetArchetypeStatColor(archetypeItem.Base);
        } else if (item is AbilityCoreItem abilityCoreItem)
        {
            if (abilityCoreItem.EquippedHero != null)
                baseItemText.text = "Equipped to " + abilityCoreItem.EquippedHero.Name;
            slotImage.color = Helpers.NORMAL_COLOR;
        }

        if (showItemValue)
        {
            if (baseItemText.text != "")
                baseItemText.text += " | ";
            baseItemText.text += item.GetItemValue();

            if (item is ArchetypeItem)
            {
                baseItemText.text += " <sprite=9>";
            }
            else
            {
                baseItemText.text += " <sprite=10>";
            }
        }

        
        //nameImage.color = Helpers.ReturnRarityColor(item.Rarity);
    }

    public void ExpandItemSlot(Equipment equip)
    {
        affixParent.SetActive(true);

        if (equip.prefixes.Count > 0)
        {
            if (equip.Rarity == RarityType.UNIQUE)
            {
                prefixText.text += "<b>Affixes</b>\n";
                suffixText.gameObject.SetActive(false);
            }
            else
            {
                prefixText.text += "<b>Prefixes</b>\n";
            }
            foreach (var x in equip.prefixes)
            {
                prefixText.text +=  Affix.BuildAffixString(x.Base, 0, x, x.GetAffixValues(), x.GetEffectValues());
            }
        }

        if (equip.suffixes.Count > 0)
        {
            suffixText.gameObject.SetActive(true);
            suffixText.text += "<b>Suffixes</b>\n";
            foreach (var x in equip.suffixes)
            {
                suffixText.text +=  Affix.BuildAffixString(x.Base, 0, x, x.GetAffixValues(), x.GetEffectValues());
            }
        }
    }

    public void DeflateItemSlot()
    {
        affixParent.SetActive(false);
    }

    public void OnItemSlotClick()
    {
        if (onClickAction != null)
        {
            if (multiSelectMode)
            {
                alreadySelected = !alreadySelected;
                selectedImage.gameObject.SetActive(alreadySelected);
            }
            onClickAction?.Invoke(item);
            return;
        }
        else
        {
            if (item == null)
            {
                return;
            }
            switch (item.GetItemType())
            {
                case ItemType.ARMOR:
                case ItemType.WEAPON:
                case ItemType.ACCESSORY:
                    break;

                default:
                    return;
            }

            UIManager.Instance.OpenEquipmentDetailWindow(false, (Equipment)item, null);
        }
    }
}