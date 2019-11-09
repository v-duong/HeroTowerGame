using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveInfoPanel : MonoBehaviour
{
    public Button startwaveButton;
    public TextMeshProUGUI waveNumText;
    public TextMeshProUGUI enemyNamesText;
    public TextMeshProUGUI timeText;
    public float timeUntilNext = 0f;

    private void Update()
    {
        if (timeUntilNext > 0f)
        {
            timeUntilNext -= Time.deltaTime;
            if (timeUntilNext < 0f)
                timeUntilNext = 0f;
            timeText.text = timeUntilNext.ToString("N2");
        }
    }
}