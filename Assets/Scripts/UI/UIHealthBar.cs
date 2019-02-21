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
    private RectTransform m_healthBarFill;
    private Vector2 m_cachedHealthPercent = Vector2.one;

    public void Initialize(float maxHealth, float currentHealth, Transform actorTransform)
    {
        this.transform.SetParent(StageManager.Instance.WorldCanvas.transform);
        UpdateHealthBar(maxHealth, currentHealth);
        UpdatePosition(actorTransform);
    }

    public void UpdatePosition(Transform actorTransform)
    {
        Vector2 newPos = actorTransform.position;
        newPos.y += 0.75f;
        newPos = RectTransformUtility.WorldToScreenPoint(Camera.main, newPos);
        this.transform.position = newPos;
    }

    public void UpdateHealthBar(float maxHealth, float currentHealth)
    {
        m_cachedHealthPercent.x = 1 - (float)(currentHealth / maxHealth);
        m_healthBarFill.anchorMax = m_cachedHealthPercent;

        if (m_cachedHealthPercent.x == 0.0f)
            m_healthBarFill.parent.parent.transform.localScale = Vector3.zero;
        else
            m_healthBarFill.parent.parent.transform.localScale = Vector3.one;
    }
}
