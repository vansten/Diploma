using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class NavigationManager : Singleton<NavigationManager>
{
    private class Node
    {
        public int Index;
        public Vector3 Position;
        public List<int> Neighboors;
        
        public Node(Vector3 p, int i)
        {
            Index = i;
            Position = p;
            Neighboors = new List<int>();
        }
    }

    private class AStarNode
    {
        public Node Node;
        public AStarNode Parent;
        public int Cost;

        public AStarNode(Node n, AStarNode p, int c)
        {
            Node = n;
            Parent = p;
            Cost = c;
        }
    }

    [SerializeField]
    private LayerMask _navPointsLayers;
    [SerializeField]
    private List<Transform> _navPoints;

    private List<Node> _graph;

    [ContextMenu("Fill nav points")]
    void FillNavPoints()
    {
        _navPoints = new List<Transform>(FindObjectsOfType<Transform>()).Where(t => (t.gameObject.layer & (1 << _navPointsLayers)) != 0 && t.GetComponent<SpriteRenderer>() != null).ToList();
        _navPoints.Sort((t1, t2) => { return t1.name.CompareTo(t2.name); });
    }

    void Start()
    {
        int i = 0;
        _graph = new List<Node>();
        foreach (Transform t in _navPoints)
        {
            _graph.Add(new Node(t.position, i));
            i += 1;
        }

        foreach (Node n in _graph)
        {
            foreach (Node n2 in _graph)
            {
                if (n.Index != n2.Index)
                {
                    float dx = Mathf.Abs(n.Position.x - n2.Position.x);
                    float dy = Mathf.Abs(n.Position.y - n2.Position.y);
                    if (dx <= 0.5f && dx > 0.0f && dy == 0.0f)
                    {
                        n.Neighboors.Add(n2.Index);
                    }
                    else if (dy <= 0.5f && dy > 0.0f && dx == 0.0f)
                    {
                        n.Neighboors.Add(n2.Index);
                    }
                }
            }
        }
    }

    public Transform GetRandomNavPoint()
    {
        if(_navPoints == null || _navPoints.Count == 0)
        {
            Debug.LogError("Nav points not filled. Please fill nav points by right clicking on NavigationManager component and selecting \"Fill nav points\" option");

            return null;
        }

        return _navPoints[UnityEngine.Random.Range(0, _navPoints.Count)];
    }

    public List<Vector3> FindWay(Vector3 start, Vector3 target)
    {
        List<Vector3> way = new List<Vector3>();

        way.Add(start);

        Node startN = null;
        float closestDistance = float.MaxValue;
        foreach(Node n in _graph)
        {
            float dist = Vector3.Distance(n.Position, start);
            if(dist < closestDistance)
            {
                startN = n;
                closestDistance = dist;
            }
        }

        if(startN == null)
        {
            return null;
        }

        way.Add(startN.Position);
        List<AStarNode> tmp = new List<AStarNode>();
        tmp.Add(new AStarNode(startN, null, 0));
        List<int> processed = new List<int>();
        processed.Add(startN.Index);

        AStarNode final = null;

        while(tmp.Count > 0)
        {
            AStarNode n = tmp[0];
            tmp.RemoveAt(tmp.IndexOf(n));

            if(Vector3.Distance(n.Node.Position, target) < 0.5f)
            {
                final = n;
                tmp.Clear();
                processed.Clear();
                break;
            }

            foreach(int i in n.Node.Neighboors)
            {
                Node node = _graph[i];
                if(!processed.Contains(node.Index))
                {
                    int cost = n.Cost;
                    cost += Mathf.CeilToInt(Vector3.Distance(node.Position, target));
                    AStarNode asn = new AStarNode(node, n, cost);
                    tmp.Add(asn);
                    processed.Add(node.Index);
                }
            }

            tmp.Sort((n1, n2) => { return n1.Cost.CompareTo(n2.Cost); });
        }

        List<Vector3> temp = new List<Vector3>();
        while(final != null)
        {
            temp.Add(final.Node.Position);
            final = final.Parent;
        }

        for(int i = temp.Count - 1; i >= 0; --i)
        {
            if(!way.Contains(temp[i]))
            {
                way.Add(temp[i]);
            }
        }

        temp.Clear();

        return way;
    }
}
