using TMPro;
using UnityEngine;

public class FloatingDamageText : MonoBehaviour
{
    private const float TEXT_DURATION = 0.5f;

    [SerializeField]
    private TextMeshProUGUI text;

    public float durationLeft = TEXT_DURATION;

    private void Update()
    {
        durationLeft -= Time.deltaTime;

        if (durationLeft <= 0)
            StageManager.Instance.BattleManager.DamageTextPool.ReturnToPool(this);

        text.color = new Color(text.color.r, text.color.g, text.color.b, durationLeft / (TEXT_DURATION / 2));
        text.outlineColor = new Color(0, 0, 0, durationLeft / (TEXT_DURATION / 2));

        Vector3 pos = transform.position;
        pos.y += Time.deltaTime;

        transform.position = pos;
    }

    public void SetDamageText(float damage, Color color)
    {
        text.text = damage.ToString("N0");
        text.color = color;
        this.gameObject.SetActive(true);
    }
    public void ResetDuration()
    {
        durationLeft = TEXT_DURATION;
    }
}