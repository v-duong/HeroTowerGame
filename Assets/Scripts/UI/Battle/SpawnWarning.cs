using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnWarning : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI timerText;
    public float timeLeft;
    public float originalTime;

    private void Update()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            image.fillAmount = timeLeft / originalTime;
            timerText.text = timeLeft.ToString("N2");
        } else
        {
            ResetSpawnWarning();
        }
    }

    public void SetTimeLeft(float time)
    {
        timeLeft = time;
        originalTime = time;
        this.gameObject.SetActive(true);
    }

    public void ResetSpawnWarning()
    {
        timeLeft = 0;
        originalTime = 0;
        this.gameObject.SetActive(false);
    }
}