using System.Collections.Generic;
using UnityEngine;

public sealed class GraphGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private NodeView nodePrefab;
    [SerializeField] private LineRenderer edgePrefab;

    [Header("Graph settings")]
    [SerializeField] private int nodeCount = 12;
    [SerializeField] private float radius = 4.0f;
    [SerializeField] private int connectionsPerNode = 2;

    private readonly List<NodeView> nodes = new();

    private void Start()
    {
        GenerateNodes();
        GenerateEdges();
    }

    private void GenerateNodes()
    {
        nodes.Clear();

        for (int i = 0; i < nodeCount; i++)
        {
            float angle = i * Mathf.PI * 2f / nodeCount;
            Vector2 pos = new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle)
            ) * radius;

            NodeView node = Instantiate(nodePrefab, pos, Quaternion.identity, transform);
            node.Init(i);
            node.name = $"Node_{i}";

            nodes.Add(node);
        }

        AssignStartingOwners();
    }

    private void AssignStartingOwners()
    {
        // простой старт: 3 узла игроку 1, 3 узла игроку 2, остальные нейтральные
        int p1 = 3;
        int p2 = 3;

        // сначала всем Neutral
        foreach (var n in nodes)
            n.SetOwner(NodeOwner.Neutral);

        // берём первые p1 для Player1
        for (int i = 0; i < Mathf.Min(p1, nodes.Count); i++)
            nodes[i].SetOwner(NodeOwner.Player1);

        // берём последние p2 для Player2
        for (int i = 0; i < Mathf.Min(p2, nodes.Count); i++)
            nodes[nodes.Count - 1 - i].SetOwner(NodeOwner.Player2);
    }


    private void GenerateEdges()
    {
        var used = new HashSet<ulong>();

        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 1; j <= connectionsPerNode; j++)
            {
                int other = (i + j) % nodes.Count;

                int a = Mathf.Min(i, other);
                int b = Mathf.Max(i, other);

                ulong key = ((ulong)(uint)a << 32) | (uint)b;
                if (!used.Add(key))
                {
                    continue; // уже есть такое ребро
                }

                CreateEdge(nodes[i], nodes[other]);
            }
        }
    }


    private void CreateEdge(NodeView a, NodeView b)
    {
        LineRenderer lr = Instantiate(edgePrefab, transform);
        lr.positionCount = 2;
        lr.SetPosition(0, a.transform.position);
        lr.SetPosition(1, b.transform.position);

        a.AddNeighbor(b);
        b.AddNeighbor(a);
    }
}

