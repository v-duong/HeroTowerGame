using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour {

    [SerializeField]
    public int goalIndex;

	// Use this for initialization
	void Start () {
        this.transform.position = Helpers.ReturnTilePosition(StageManager.Instance.PathTilemap, this.transform.position);

    }

    // Update is called once per frame
    void Update () {
		
	}
}
