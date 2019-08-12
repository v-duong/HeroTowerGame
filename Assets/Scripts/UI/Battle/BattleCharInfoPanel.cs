using TMPro;
using UnityEngine;

public class BattleCharInfoPanel : MonoBehaviour
{
    public TextMeshProUGUI text;
    private ActorData data;
    private string targetName;

    public void Update()
    {
        if (data == null || data.IsDead)
        {
            targetName = null;
            data = null;
            text.text = "";
            return;
        }
        text.text = targetName + "\n";
        if (data.CurrentHealth < 1 && data.CurrentHealth > 0)
            text.text += "Health: " + data.CurrentHealth.ToString("F2") + "/" + data.MaximumHealth;
        else
            text.text += "Health: " + data.CurrentHealth.ToString("F0") + "/" + data.MaximumHealth;
    }

    public void SetTarget(ActorData data)
    {
        this.data = data;
        targetName = LocalizationManager.Instance.GetLocalizationText_Enemy(data.Name, ".name");
    }
}