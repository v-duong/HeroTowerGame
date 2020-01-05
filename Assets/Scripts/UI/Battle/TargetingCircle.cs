using TMPro;
using UnityEngine;

public class TargetingCircle : MonoBehaviour
{
    public SpriteRenderer image;
    public TextMeshPro text;

    public void SetScale(float scale)
    {
        this.transform.localScale = new Vector2(scale, scale);
        text.fontSize = 1f / (scale / 2.5f);
    }

    public void SetText(string circleText)
    {
        text.text = circleText;
    }

    public void Update()
    {
        image.transform.Rotate(new Vector3(0, 0, 0.05f));
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }
}