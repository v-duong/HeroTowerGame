using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSelectionWindow : MonoBehaviour
{
    public TMP_InputField iLvlInput;
    public TMP_Dropdown iLvlDropdown;
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
        int iLvl = int.Parse(iLvlInput.text);
        switch(iLvlDropdown.value)
        {
            case 0:
                UIManager.Instance.InvScrollContent.SelectByCriteria(x=>x.ItemLevel < iLvl && (rarityTypes.Count == 0 || rarityTypes.Contains(x.Rarity)));
                break;
            case 1:
                UIManager.Instance.InvScrollContent.SelectByCriteria(x => x.ItemLevel == iLvl && (rarityTypes.Count == 0 || rarityTypes.Contains(x.Rarity)));
                break;
            case 2:
                UIManager.Instance.InvScrollContent.SelectByCriteria(x => x.ItemLevel > iLvl && (rarityTypes.Count == 0 || rarityTypes.Contains(x.Rarity)));
                break;
            default:
                break;
        }
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