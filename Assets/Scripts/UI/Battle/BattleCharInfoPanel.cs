using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleCharInfoPanel : MonoBehaviour
{
    public TextMeshProUGUI text;
    public ActorData data;

    public void Update()
    {
        if (data == null || data.IsDead)
        {
            data = null;
            text.text = "";
            return;
        }
        text.text = "Health: " + data.CurrentHealth.ToString("F1") + "/" + data.MaximumHealth;
    }
}
