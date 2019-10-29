using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummonScrollSlot : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private Image image;

    [SerializeField]
    private Slider respawnTimer;

    public HeroActor actor;
    public UIHealthBar healthBar;
    public bool heroSummoned = false;
    public bool heroDead = false;
    private float currentRespawnTime = 0f;
    private float maxRespawnTime = 0f;

    public void SetActor(HeroActor actor)
    {
        nameText.text = actor.Data.Name;
        actor.gameObject.SetActive(false);
        this.actor = actor;
    }

    private void Update()
    {
        if (heroDead)
        {
            currentRespawnTime -= Time.deltaTime;

            respawnTimer.value = currentRespawnTime / maxRespawnTime;

            if (currentRespawnTime <= 0f)
            {
                respawnTimer.gameObject.SetActive(false);
                heroDead = false;
                image.color = Color.white;
                actor.Data.ResetHealthShieldValues();
            }
        }

        healthBar.UpdateHealthBar(actor.Data.MaximumHealth, actor.Data.CurrentHealth, actor.Data.MaximumManaShield, actor.Data.CurrentManaShield, false);
    }

    public void OnClickSlot()
    {
        if (!heroSummoned && !heroDead)
        {
            InputManager.Instance.SetSummoning(actor, SummonCallback);
            StageManager.Instance.HighlightMap.gameObject.SetActive(true);
        }
    }

    public void SummonCallback()
    {
        StageManager.Instance.HighlightMap.gameObject.SetActive(false);
        heroSummoned = true;
        image.color = new Color(0.85f, 0.85f, 0.85f);
    }

    public void OnHeroDeath()
    {
        heroSummoned = false;
        heroDead = true;
        maxRespawnTime = 5f;
        currentRespawnTime = 5f;
        respawnTimer.gameObject.SetActive(true);
        respawnTimer.value = 1f;
    }
}