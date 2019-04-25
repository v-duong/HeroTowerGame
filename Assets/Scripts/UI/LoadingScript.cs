using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingScript : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI loadingText;
    [SerializeField]
    private Image loadingImage;
    private float time = 0;
    public bool endLoadingScreen = false;
    private static float FADE_TIME = 0.5f;

    // Update is called once per frame
    void Update()
    {
        loadingText.color = new Color(loadingText.color.r, loadingText.color.g, loadingText.color.b, Mathf.PingPong(Time.time, 1));
        if (endLoadingScreen)
        {
            loadingText.gameObject.SetActive(false);
            loadingImage.color = new Color(0, 0, 0, 1 - time * 2);
            time += Time.deltaTime;
            if (time >= 0.5)
            {
                SceneManager.UnloadSceneAsync("loadingScene");
            }
        }

    }
}
