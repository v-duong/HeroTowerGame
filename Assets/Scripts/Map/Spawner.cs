using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    public int spawnerIndex;

    private List<List<Vector3>> nodesToGoal;
    public bool nodesAreOutdated;

    // Use this for initialization
    private void Start()
    {
        nodesToGoal = new List<List<Vector3>>();

        if (StageManager.Instance.WaveManager.GoalList.Count == 0)
        {
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
            UpdateNodes();
        }
    }

    public List<Vector3> GetNodesToGoal(int index)
    {
        if (nodesToGoal.Count != 0)
            return nodesToGoal[index] ?? null;
        else return null;
    }

    private void UpdateNodes()
    {
        foreach (var goal in StageManager.Instance.WaveManager.GoalList)
        {
            nodesToGoal.Add(Pathfinding.FindPath(transform.position, goal.transform.position, StageManager.Instance.PathTilemap, false));
        }
        nodesAreOutdated = false;
    }
}