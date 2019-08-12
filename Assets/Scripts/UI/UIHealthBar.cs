using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour {

    /*
    [SerializeField]
    private RectTransform m_healthBarFillArea;
    */
    [SerializeField]
    private RectTransform healthBarFill;
    [SerializeField]
    private RectTransform shieldBarFill;
    private Vector2 currenthealthPercent = Vector2.one;
    private Vector2 currentshieldPercent = Vector2.one;

    public void Initialize(float maxHealth, float currentHealth, float maxShield, float currentShield, Transform actorTransform)
    {
        this.transform.SetParent(StageManager.Instance.WorldCanvas.transform, false);
        UpdateHealthBar(maxHealth, currentHealth, maxShield, currentShield);
        UpdatePosition(actorTransform);
    }

    public void UpdatePosition(Transform actorTransform)
    {
        Vector2 newPos = actorTransform.position;
        newPos.y += 0.75f;
        newPos = RectTransformUtility.WorldToScreenPoint(Camera.main, newPos);
        this.transform.position = newPos;
    }

    public void UpdateHealthBar(float maxHealth, float currentHealth, float maxShield, float currentShield)
    {
        currenthealthPercent.x = (float)(currentHealth / maxHealth);
        healthBarFill.anchorMax = currenthealthPercent;
        if (maxShield != 0)
        {
            currentshieldPercent.x = (currentShield / maxShield);
            shieldBarFill.anchorMax = currentshieldPercent;
        } else
        {
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
