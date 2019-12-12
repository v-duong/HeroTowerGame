using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    public int spawnerIndex;

    private List<List<Vector3>> nodesToGoal;

    public bool nodesAreOutdated;
    public Transform warningLocation;

    // Use this for initialization
    private void Start()
    {
        List<UnityEngine.Tilemaps.Tilemap> pathTilemap = StageManager.Instance.PathTilemap;
        transform.position = Helpers.ReturnTilePosition(pathTilemap[0], transform.position, -3);

        Debug.Log(spawnerIndex + " " + transform.position);

        nodesToGoal = new List<List<Vector3>>();

        if (StageManager.Instance.BattleManager.GoalList.Count == 0)
        {
            Debug.Log("TEST");
            nodesAreOutdated = true;
            return;
        }
        UpdateNodes();
    }

    // Update is called once per frame
    private void Update()
    {
        if (nodesAreOutdated)
        {
            Debug.Log("outdated");
            UpdateNodes();
        }
    }

    public List<Vector3> GetNodesToGoal(int index)
    {
        if (nodesToGoal.Count != 0)
            return nodesToGoal[index];
        else return null;
    }

    private void UpdateNodes()
    {
        nodesToGoal.Clear();
        List<List<Vector3>> temp = new List<List<Vector3>>();

        foreach (Goal goal in StageManager.Instance.BattleManager.GoalList)
        {
            int lowestNodes = int.MaxValue; int lowestIndex = 0;
            temp.Clear();

            for (int i = 0; i < StageManager.Instance.PathTilemap.Count; i++)
            {
                UnityEngine.Tilemaps.Tilemap pathMap = StageManager.Instance.PathTilemap[i];
                temp.Add(Pathfinding.FindPath(transform.position, goal.transform.position, pathMap, false));
                if (temp[i].Count>1 && temp[i].Count < lowestNodes)
                {
                    lowestNodes = temp[i].Count;
                    lowestIndex = i;
                }
            }

            Debug.Log(spawnerIndex + "s " + goal.goalIndex + "g:" + temp[lowestIndex].Count + " @ "+ lowestIndex);

            nodesToGoal.Insert(goal.goalIndex, temp[lowestIndex]);
        }
        nodesAreOutdated = false;
    }
}