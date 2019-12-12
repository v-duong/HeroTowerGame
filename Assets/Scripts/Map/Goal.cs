using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField]
    public int goalIndex;

    // Use this for initialization
    private void Start()
    {
        transform.position = Helpers.ReturnTilePosition(StageManager.Instance.PathTilemap[0], transform.position, -3);
    }
}