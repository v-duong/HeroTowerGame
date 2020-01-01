using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSelectionWindow : MonoBehaviour
{
    public TMP_InputField iLvlInput;
    public TMP_Dropdown iLvlDropdown;
    public TMP_InputField dLvlInput;
    public TMP_Dropdown dLvlDropdown;
    public List<Button> rarityButtons;
    public HashSet<RarityType> rarityTypes = new HashSet<RarityType>();

    private void Start()
    {
        rarityButtons[0].onClick.AddListener(delegate { RarityButtonOnClick(RarityType.NORMAL, 0); });
        rarityButtons[1].onClick.AddListener(delegate { RarityButtonOnClick(RarityType.UNCOMMON, 1); });
        rarityButtons[2].onClick.AddListener(delegate { RarityButtonOnClick(RarityType.RARE, 2); });
        rarityButtons[3].onClick.AddListener(delegate { RarityButtonOnClick(RarityType.EPIC, 3); });
        rarityButtons[4].onClick.AddListener(delegate { RarityButtonOnClick(RarityType.UNIQUE, 4); });
    }

    public void ConfirmOnClick()
    {
        Func<Item, bool> iLvlFunc = x => true;
        Func<Item, bool> dLvlFunc = x => true;
        int iLvl = int.Parse(iLvlInput.text);
        switch (iLvlDropdown.value)
        {
            case 1:
                iLvlFunc = x => x.ItemLevel < iLvl;
                break;

            case 2:
                iLvlFunc = x => x.ItemLevel == iLvl;
                break;

            case 3:
                iLvlFunc = x => x.ItemLevel > iLvl;
                break;

            default:
                break;
        }
        int dLvl = int.Parse(dLvlInput.text);
        switch (dLvlDropdown.value)
        {
            case 1:
                dLvlFunc = x => x is Equipment e && e.Base.dropLevel < dLvl && e.Rarity != RarityType.UNIQUE;
                break;

            case 2:
                dLvlFunc = x => x is Equipment e && e.Base.dropLevel == dLvl && e.Rarity != RarityType.UNIQUE;
                break;

            case 3:
                dLvlFunc = x => x is Equipment e && e.Base.dropLevel > dLvl && e.Rarity != RarityType.UNIQUE;
                break;

            default:
                break;
        }
        UIManager.Instance.InvScrollContent.SelectByCriteria(x => iLvlFunc(x) && dLvlFunc(x) && (rarityTypes.Count == 0 || rarityTypes.Contains(x.Rarity)));
        UIManager.Instance.CloseCurrentWindow();
    }

    public void RarityButtonOnClick(RarityType rarityType, int index)
    {
        Image image = rarityButtons[index].GetComponent<Button>().image;
        if (rarityTypes.Contains(rarityType))
        {
            image.color = Color.white;
            rarityTypes.Remove(rarityType);
        }
        else
        {
            image.color = Helpers.SELECTION_COLOR;
            rarityTypes.Add(rarityType);
        }
    }
}