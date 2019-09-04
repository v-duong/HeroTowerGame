using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoUI : MonoBehaviour
{
    public TextMeshProUGUI statsText;

    private void Update()
    {
        string s = "";

        if (GameManager.Instance.PlayerStats.consumables != null)
            foreach (KeyValuePair<ConsumableType, int> pair in GameManager.Instance.PlayerStats.consumables)
            {
                s += pair.Key + ": " + pair.Value + "\n";
            }
        statsText.text = s;
    }
}