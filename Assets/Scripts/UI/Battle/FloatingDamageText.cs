using TMPro;
using UnityEngine;

public class FloatingDamageText : MonoBehaviour
{
    private const float TEXT_DURATION = 0.5f;

    [SerializeField]
    private TextMeshProUGUI text;

    private float durationLeft = TEXT_DURATION;

    private void Update()
    {
        durationLeft -= Time.deltaTime;

        if (durationLeft <= 0)
            Destroy(this.gameObject);

        text.color = new Color(1, 1, 1, durationLeft / (TEXT_DURATION / 2));
        text.outlineColor = new Color(0, 0, 0, durationLeft / (TEXT_DURATION / 2));

        Vector3 pos = transform.position;
        pos.y += Time.deltaTime;

        transform.position = pos;
    }

    public void SetDamageText(float damage)
    {
        text.text = damage.ToString("N0");
    }
}