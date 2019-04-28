using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SummonScrollSlot : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private Image image;
    private HeroActor actor;
    public bool heroSummoned = false;

    public void SetActor(HeroActor actor)
    {
        nameText.text = actor.Data.Name;
        actor.gameObject.SetActive(false);
        this.actor = actor;
    }

    public void OnClickSlot()
    {
        if (!heroSummoned)
        {
            InputManager.Instance.SetSummoning(actor, SummonCallback);
            StageManager.Instance.HighlightMap.gameObject.SetActive(true);
        }
    }

    public void SummonCallback()
    {
        StageManager.Instance.HighlightMap.gameObject.SetActive(false);
        heroSummoned = true;
        image.color = Color.grey;
    }
}
