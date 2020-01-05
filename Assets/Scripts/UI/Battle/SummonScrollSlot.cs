using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummonScrollSlot : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    public Image image;

    [SerializeField]
    private Slider respawnTimer;

    [SerializeField]
    private Image recallBar;

    [SerializeField]
    private Image recallFill;

    [SerializeField]
    private Image heroSprite;



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
        heroSprite.sprite = this.actor.GetComponent<SpriteRenderer>().sprite;
    }

    private void Update()
    {
        if (currentRespawnTime > 0)
        {
            currentRespawnTime -= Time.deltaTime;

            respawnTimer.value = currentRespawnTime / maxRespawnTime;

            if (currentRespawnTime <= 0f)
            {
                respawnTimer.gameObject.SetActive(false);
                image.color = Color.white;
                if (heroDead)
                {
                    heroDead = false;
                    actor.Data.ResetHealthShieldValues();
                }
            }
        }

        if (!heroSummoned)
        {
            actor.ModifyCurrentHealth(actor.Data.MaximumHealth * -0.05f * Time.deltaTime);
            actor.ModifyCurrentShield(actor.Data.MaximumManaShield * -0.05f * Time.deltaTime, false);
        }

        if (actor.isBeingRecalled)
        {
            recallBar.gameObject.SetActive(true);
            recallFill.fillAmount = actor.RecallTimer / HeroActor.BASE_RECALL_TIME;
        } else
        {
            recallBar.gameObject.SetActive(false);
        }

        healthBar.UpdateHealthBar(actor.Data.MaximumHealth, actor.Data.CurrentHealth, actor.Data.MaximumManaShield, actor.Data.CurrentManaShield, false);
    }

    public void OnClickSlot()
    {
        if (!heroSummoned && !heroDead && currentRespawnTime <= 0)
        {
            image.color = Helpers.SELECTION_COLOR;
            InputManager.Instance.SetSummoning(actor, this, SummonCallback);
        } else if (heroSummoned)
        {
            InputManager.Instance.OnTargetSelect(actor);
        }
    }

    public void SummonCallback()
    {
        InputManager.Instance.SetTileHighlight(false);
        heroSummoned = true;
        image.color = new Color(0.85f, 0.85f, 0.85f);
    }

    public void OnHeroDeath(float respawnTime, bool isDead)
    {
        heroSummoned = false;
        heroDead = isDead;
        maxRespawnTime = respawnTime;
        currentRespawnTime = respawnTime;
        respawnTimer.gameObject.SetActive(true);
        respawnTimer.value = 1f;
    }

}