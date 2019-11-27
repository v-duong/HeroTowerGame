using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveInfoPanel : MonoBehaviour
{
    public Button startwaveButton;
    public TextMeshProUGUI waveNumText;
    public TextMeshProUGUI enemyNamesText;
    public TextMeshProUGUI timeText;
    public Image timerImage;
    public float timeUntilNext = 0f;
    public float originalTime = 0f;

    private void Start()
    {
        if (timerImage != null)
            timerImage.fillAmount = 0;
    }

    private void Update()
    {
        if (timeUntilNext > 0f)
        {
            timeUntilNext -= Time.deltaTime;
            if (timeUntilNext < 0f)
                timeUntilNext = 0f;
            timeText.text = timeUntilNext.ToString("N2");
            if (timerImage != null)
            {
                if (originalTime > 0)
                    timerImage.fillAmount = timeUntilNext / originalTime;
                else
                    timerImage.fillAmount = 0;
            }

        }
    }
}