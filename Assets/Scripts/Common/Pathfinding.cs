using Priority_Queue;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class Pathfinding
{
    private class QueueNode : FastPriorityQueueNode
    {
        public Vector2 data;

        public QueueNode(Vector2 v)
        {
            data = v;
        }
    }

    // A* pathfinding on tilemap
    public static List<Vector3> FindPath(Vector2 start, Vector2 end, Tilemap t, bool useDiagonals)
    {
        List<Vector3> ret = new List<Vector3>();
        Dictionary<Vector2, Vector2> cameFrom = new Dictionary<Vector2, Vector2>();
        Dictionary<Vector2, float> cost = new Dictionary<Vector2, float>();
        Vector2 current;
        float newcost = 0;
        float dist = 0;
        FastPriorityQueue<QueueNode> queue = new FastPriorityQueue<QueueNode>(64);
        queue.Enqueue(new QueueNode(start), 0);
        cost.Add(start, 0);

        while (queue.Count != 0)
        {
            current = queue.Dequeue().data;

            if (Vector2.SqrMagnitude(end - current) < 0.5f)
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
                        dist = Vector2.Distance(neighbor, end);
                    queue.Enqueue(new QueueNode(neighbor), newcost + dist);
                    cameFrom[neighbor] = current;
                }
            }
        }

        TraverseCameFrom(end, cameFrom, ret);

        return ret;
    }

    //recursively traverse the generated path
    public static void TraverseCameFrom(Vector2 currentVector, Dictionary<Vector2, Vector2> previousPath, List<Vector3> returnList)
    {
        if (!previousPath.ContainsKey(currentVector))
        {
            returnList.Add(new Vector3 (currentVector.x, currentVector.y, -3));
            return;
        }
        TraverseCameFrom(previousPath[currentVector], previousPath, returnList);
        returnList.Add(new Vector3(currentVector.x, currentVector.y, -3));
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