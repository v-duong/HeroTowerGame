using UnityEngine;

public class UIHealthBar : MonoBehaviour
{
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

    public void Initialize(float maxHealth, float currentHealth, float maxShield, float currentShield, Transform actorTransform)
    {
        transform.SetParent(StageManager.Instance.WorldCanvas.transform, false);
        UpdateHealthBar(maxHealth, currentHealth, maxShield, currentShield);
        UpdatePosition(actorTransform);
    }

    public void UpdatePosition(Transform actorTransform)
    {
        Vector2 newPos = actorTransform.position;
        newPos.y += 0.75f;
        newPos = RectTransformUtility.WorldToScreenPoint(Camera.main, newPos);
        transform.position = newPos;
    }

    private void Update()
    {
        float scaleRatio = 1f / InputManager.Instance.zoomRatio;
        transform.localScale = new Vector3(scaleRatio, scaleRatio, scaleRatio);

        if (barDelay <= 0f)
        {
            if (recentlyDamagedHealth)
            {
                tempHealthBarFill.anchorMax = Vector2.Lerp(tempHealthBarFill.anchorMax, healthBarFill.anchorMax, 0.2f);
            }
            if (recentlyDamagedShield)
            {
                tempShieldBarFill.anchorMax = Vector2.Lerp(tempShieldBarFill.anchorMax, shieldBarFill.anchorMax, 0.2f);
            }
        }
        else
        {
            barDelay -= Time.deltaTime;
        }
    }

    public void UpdateHealthBar(float maxHealth, float currentHealth, float maxShield, float currentShield)
    {
        float oldHealthPercent = currentHealthPercent.x;
        currentHealthPercent.x = currentHealth / maxHealth;

        if (oldHealthPercent > currentHealthPercent.x)
        {
            recentlyDamagedHealth = true;
            if (barDelay <= 0f)
            {
                barDelay = 0.75f;
                tempHealthBarFill.anchorMax = new Vector2(oldHealthPercent, 1);
            }
        }
        else
        {
            tempHealthBarFill.anchorMax = currentHealthPercent;
        }

        healthBarFill.anchorMax = currentHealthPercent;

        if (maxShield != 0)
        {
            float oldShieldPercent = currentShieldPercent.x;
            currentShieldPercent.x = currentShield / maxShield;

            if (oldShieldPercent > currentShieldPercent.x)
            {
                recentlyDamagedShield = true;
                if (barDelay <= 0f)
                {
                    barDelay = 0.75f;
                    tempShieldBarFill.anchorMax = new Vector2(oldShieldPercent, 1);
                }
            }
            else
            {
                tempShieldBarFill.anchorMax = currentShieldPercent;
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