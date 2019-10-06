using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour {

    [SerializeField]
    public int goalIndex;

	// Use this for initialization
	void Start () {
        transform.position = Helpers.ReturnTilePosition(StageManager.Instance.PathTilemap, transform.position, -3);

    }
}
