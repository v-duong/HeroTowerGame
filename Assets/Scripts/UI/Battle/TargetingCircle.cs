using UnityEngine;

public class TargetingCircle : MonoBehaviour
{
    public void Update()
    {
        this.transform.Rotate(new Vector3(0,0,0.05f));
    }

    public void SetColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }
}
