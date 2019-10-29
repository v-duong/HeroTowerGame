using UnityEngine;

public class UIHealthBar : MonoBehaviour
{
    private const float DELAY_TIME = 0.75f;

    /*
[SerializeField]
private RectTransform m_healthBarFillArea;
*/

    [SerializeField]
    private RectTransform healthBarFill;

    [SerializeField]
    private RectTransform shieldBarFill;

    [SerializeField]
    private RectTransform tempHealthBarFill;

    [SerializeField]
    private RectTransform tempShieldBarFill;

    private Vector2 currentHealthPercent = Vector2.one;
    private Vector2 currentShieldPercent = Vector2.one;

    private bool recentlyDamagedHealth;
    private bool recentlyDamagedShield;
    private float barDelay = 0f;
    private float healthDifference;
    private float shieldDifference;

    public void InitializeForActor(float maxHealth, float currentHealth, float maxShield, float currentShield, Transform actorTransform)
    {
        transform.SetParent(StageManager.Instance.WorldCanvas.transform, false);
        UpdateHealthBar(maxHealth, currentHealth, maxShield, currentShield, false);
        UpdatePosition(actorTransform);
    }

    public void UpdatePosition(Transform actorTransform)
    {
        
        Vector2 newPos = actorTransform.position;
        newPos.y += DELAY_TIME;
        /*
        newPos = RectTransformUtility.WorldToScreenPoint(Camera.main, newPos);
        transform.position = newPos;
        */
        transform.position = newPos;
    }

    private void Update()
    {
        //float scaleRatio = 1f / InputManager.Instance.zoomRatio;
        //transform.localScale = new Vector3(scaleRatio, scaleRatio, scaleRatio);

        if (barDelay > 0f && (recentlyDamagedHealth || recentlyDamagedShield))
        {
            if (recentlyDamagedHealth)
            {
                tempHealthBarFill.anchorMax = Vector2.MoveTowards(tempHealthBarFill.anchorMax, healthBarFill.anchorMax, healthDifference / DELAY_TIME * Time.deltaTime);
            }
            if (recentlyDamagedShield)
            {
                tempShieldBarFill.anchorMax = Vector2.MoveTowards(tempShieldBarFill.anchorMax, shieldBarFill.anchorMax, shieldDifference / DELAY_TIME * Time.deltaTime);
            }
            barDelay -= Time.deltaTime;
        }
        else
        {
            tempHealthBarFill.anchorMax = healthBarFill.anchorMax;
            tempShieldBarFill.anchorMax = shieldBarFill.anchorMax;
            recentlyDamagedHealth = false;
            recentlyDamagedShield = false;
        }
    }

    public void UpdateHealthBar(float maxHealth, float currentHealth, float maxShield, float currentShield, bool showRecentDamage)
    {
        float oldHealthPercent = currentHealthPercent.x;
        currentHealthPercent.x = currentHealth / maxHealth;

        if (!showRecentDamage)
        {
            barDelay = 0;
            recentlyDamagedHealth = false;
            recentlyDamagedShield = false;
        }

        if (oldHealthPercent > currentHealthPercent.x && showRecentDamage)
        {
            recentlyDamagedHealth = true;

            if (barDelay <= 0f)
            {
                healthDifference = oldHealthPercent - currentHealthPercent.x;
                barDelay = DELAY_TIME;
                tempHealthBarFill.anchorMax = new Vector2(oldHealthPercent, 1);
            } else
                healthDifference += oldHealthPercent - currentHealthPercent.x;
        }

        healthBarFill.anchorMax = currentHealthPercent;

        if (maxShield != 0)
        {
            float oldShieldPercent = currentShieldPercent.x;
            currentShieldPercent.x = currentShield / maxShield;

            if (oldShieldPercent > currentShieldPercent.x && showRecentDamage)
            {
                recentlyDamagedShield = true;
                if (barDelay <= 0f)
                {
                    shieldDifference = oldShieldPercent - currentShieldPercent.x;
                    barDelay = DELAY_TIME;
                    tempShieldBarFill.anchorMax = new Vector2(oldShieldPercent, 1);
                } else
                    shieldDifference += oldShieldPercent - currentShieldPercent.x;
            }

            shieldBarFill.anchorMax = currentShieldPercent;
        }
        else
        {
            tempShieldBarFill.anchorMax = Vector2.zero;
            shieldBarFill.anchorMax = Vector2.zero;
        }

        /*
        if (m_cachedHealthPercent.x == 1.0f)
            this.gameObject.SetActive(false);
        else
            this.gameObject.SetActive(true);
            */
    }
}