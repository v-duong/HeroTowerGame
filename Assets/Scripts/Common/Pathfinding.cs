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
    public static List<Vector3> FindPath(Vector3 start, Vector3 end, Tilemap t)
    {
        List<Vector3> ret = new List<Vector3>();
        Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3>();
        Dictionary<Vector3, int> cost = new Dictionary<Vector3, int>();
        Vector3 current;
        int newcost = 0;

        FastPriorityQueue<QueueNode> queue = new FastPriorityQueue<QueueNode>(16);
        queue.Enqueue(new QueueNode(start), 0);
        cost.Add(start, 0);

        while (queue.Count != 0)
        {
            current = queue.Dequeue().data;

            if (current == end) break;

            foreach (Vector3 neighbor in GetTileNeighbor(t, current))
            {
                newcost = cost[current] + 1;
                if (!cost.ContainsKey(neighbor) || newcost < cost[neighbor])
                {
                    cost.Add(neighbor, newcost);
                    queue.Enqueue(new QueueNode(neighbor), newcost + Vector3.Distance(neighbor, end));
                    cameFrom.Add(neighbor, current);
                }
            }
        }
        TraverseCameFrom(end, cameFrom, ret);

        return ret;
    }

    //recursively traverse the generated path
    public static void TraverseCameFrom(Vector3 v, Dictionary<Vector3, Vector3> d, List<Vector3> r)
    {
        if (!d.ContainsKey(v))
        {
            r.Add(v);
            return;
        }
        TraverseCameFrom(d[v], d, r);
        r.Add(v);
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
}