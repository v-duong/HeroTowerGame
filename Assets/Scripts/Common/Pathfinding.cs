using Priority_Queue;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class Pathfinding
{
    private class QueueNode : FastPriorityQueueNode
    {
        public Vector3 data;

        public QueueNode(Vector3 v)
        {
            data = v;
        }
    }

    // A* pathfinding on tilemap
    public static List<Vector3> FindPath(Vector3 start, Vector3 end, Tilemap t, bool useDiagonals)
    {
        List<Vector3> ret = new List<Vector3>();
        Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3>();
        Dictionary<Vector3, float> cost = new Dictionary<Vector3, float>();
        Vector3 current;
        float newcost = 0;
        float dist = 0;
        FastPriorityQueue<QueueNode> queue = new FastPriorityQueue<QueueNode>(64);
        queue.Enqueue(new QueueNode(start), 0);
        cost.Add(start, 0);

        while (queue.Count != 0)
        {
            current = queue.Dequeue().data;

            if (Vector3.SqrMagnitude(end - current) < 0.5f)
            {
                if (!cameFrom.ContainsKey(end) && cameFrom.ContainsKey(current))
                {
                    cameFrom.Add(end, cameFrom[current]);
                }
                break;
            }

            List<Vector3> neighbors;
            if (useDiagonals)
            {
                neighbors = GetTileNeighborDiagonal(t, current);
            }
            else
                neighbors = GetTileNeighbor(t, current);

            foreach (Vector3 neighbor in neighbors)
            {
                newcost = cost[current] + 0.5f;
                if (!cost.ContainsKey(neighbor) || newcost < cost[neighbor])
                {
                    cost[neighbor] = newcost;
                    if (useDiagonals)
                       dist = Mathf.Max(Mathf.Abs(neighbor.x - end.x), Mathf.Abs(neighbor.y - end.y));
                    else
                        dist = Vector3.Distance(neighbor, end);
                    queue.Enqueue(new QueueNode(neighbor), newcost + dist);
                    cameFrom[neighbor] = current;
                }
            }
        }

        TraverseCameFrom(end, cameFrom, ret);

        return ret;
    }

    //recursively traverse the generated path
    public static void TraverseCameFrom(Vector3 currentVector, Dictionary<Vector3, Vector3> previousPath, List<Vector3> returnList)
    {
        if (!previousPath.ContainsKey(currentVector))
        {
            returnList.Add(currentVector);
            return;
        }
        TraverseCameFrom(previousPath[currentVector], previousPath, returnList);
        returnList.Add(currentVector);
    }

    public static List<Vector3> GetTileNeighbor(Tilemap t, Vector3 vector)
    {
        List<Vector3> ret = new List<Vector3>();
        Vector3Int baseVector = t.WorldToCell(vector);

        if (t.GetTile(baseVector + Vector3Int.up) != null)
            ret.Add(vector + Vector3.up);
        if (t.GetTile(baseVector + Vector3Int.down) != null)
            ret.Add(vector + Vector3.down);
        if (t.GetTile(baseVector + Vector3Int.left) != null)
            ret.Add(vector + Vector3.left);
        if (t.GetTile(baseVector + Vector3Int.right) != null)
            ret.Add(vector + Vector3.right);

        return ret;
    }

    public static List<Vector3> GetTileNeighborDiagonal(Tilemap t, Vector3 vector)
    {
        List<Vector3> ret = new List<Vector3>();
        Vector3Int baseVector = t.WorldToCell(vector);

        if (t.GetTile(baseVector + Vector3Int.up) != null)
            ret.Add(vector + Vector3.up);
        if (t.GetTile(baseVector + Vector3Int.down) != null)
            ret.Add(vector + Vector3.down);
        if (t.GetTile(baseVector + Vector3Int.left) != null)
            ret.Add(vector + Vector3.left);
        if (t.GetTile(baseVector + Vector3Int.right) != null)
            ret.Add(vector + Vector3.right);

        if (t.GetTile(baseVector + Vector3Int.up + Vector3Int.left) != null)
            ret.Add(vector + Vector3.up + Vector3.left);
        if (t.GetTile(baseVector + Vector3Int.up + Vector3Int.right) != null)
            ret.Add(vector + Vector3.up + Vector3.right);
        if (t.GetTile(baseVector + Vector3Int.down + Vector3Int.left) != null)
            ret.Add(vector + Vector3.down + Vector3.left);
        if (t.GetTile(baseVector + Vector3Int.down + Vector3Int.right) != null)
            ret.Add(vector + Vector3.down + Vector3.right);

        return ret;
    }
}