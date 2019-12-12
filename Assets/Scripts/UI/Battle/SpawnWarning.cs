using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnWarning : MonoBehaviour
{
    private static Color yellowColor = new Color(1f, 0.95f, 0f, 0.75f);
    private static Color redColor = new Color(1f, 0f, 0f, 0.75f);

    public Image image;
    public Image warningImage;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI waveText;
    public float timeLeft;
    public TimeInfo currentTimeInfo;
    public List<TimeInfo> timeQueue = new List<TimeInfo>();

    private void Update()
    {
        if (currentTimeInfo == null || timeLeft < 0)
        {
            if (!GetNextTimeInfo())
                return;
        }

        if (currentTimeInfo != null && timeLeft > 0)
        {
            if (currentTimeInfo.waveNum == StageManager.Instance.BattleManager.currentWave)
            {
                warningImage.color = redColor;
                waveText.text = "Incoming";
            }
            else
            {
                warningImage.color = yellowColor;
                waveText.text = "Wave " + (currentTimeInfo.waveNum+1);
            }

            timeLeft -= Time.deltaTime;

            image.fillAmount = timeLeft / currentTimeInfo.adjustedTime;
            timerText.text = timeLeft.ToString("N2");
        }
    }

    public bool GetNextTimeInfo()
    {
        if (timeQueue.Count == 0)
        {
            ResetSpawnWarning();
            return false;
        }

        if (currentTimeInfo != null)
        {
            TimeInfo oldTimeInfo = currentTimeInfo;
            timeQueue[0].adjustedTime -= oldTimeInfo.overallTime;
        } 

        currentTimeInfo = timeQueue[0];
        timeQueue.RemoveAt(0);
        timeLeft = currentTimeInfo.adjustedTime;

        //Debug.Log("GET TIME " + timeLeft);

        return true;
    }

    public void StartWarning()
    {
        timeQueue = timeQueue.OrderBy(x => x.overallTime).ToList();
        if (timeQueue.Count > 0)
        this.gameObject.SetActive(true);
    }

    public void AddTimeInfo(TimeInfo timeInfo)
    {
        //Debug.Log(timeInfo.time);

        timeQueue.Add(timeInfo);
    }

    public void ResetSpawnWarning()
    {
        timeLeft = 0;
        currentTimeInfo = null;
        timeQueue.Clear();
        this.gameObject.SetActive(false);
    }

    public class TimeInfo
    {
        public float overallTime;
        public float adjustedTime;
        public int waveNum;

        public TimeInfo(float time, int waveNum)
        {
            adjustedTime = time;
            this.overallTime = time;
            this.waveNum = waveNum;
        }
    }
}