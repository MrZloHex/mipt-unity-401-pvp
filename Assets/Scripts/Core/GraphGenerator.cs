using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Core
{
    public sealed class GraphGenerator : NetworkBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private NodeView nodePrefab;
        [SerializeField] private LineRenderer edgePrefab;

        [Header("Graph settings")]
        [SerializeField] private int nodeCount = 12;
        [SerializeField] private float radius = 4.0f;
        [SerializeField] private int connectionsPerNode = 2;

        private readonly List<NodeView> _nodes = new();

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                GenerateNodes();
                GenerateEdges();
            }
        }

        private void GenerateNodes()
        {
            _nodes.Clear();

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

                var netObj = node.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
                    {
                        if (!netObj.IsSpawned)
                            netObj.Spawn();
                    }
                    else
                    {
                        Debug.LogWarning($"NetworkObject found on {node.name} but not spawned because this instance is not server/host.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Node prefab '{nodePrefab.name}' does not contain a NetworkObject. Add one to enable networking.");
                }

                _nodes.Add(node);
            }

            AssignStartingOwners();
        }

        private void AssignStartingOwners()
        {
            // простой старт: 3 узла игроку 1, 3 узла игроку 2, остальные нейтральные
            int p1 = 3;
            int p2 = 3;

            // сначала всем Neutral
            foreach (var n in _nodes)
                n.SetOwner(NodeOwner.Neutral);

            // берём первые p1 для Player1
            for (int i = 0; i < Mathf.Min(p1, _nodes.Count); i++)
                _nodes[i].SetOwner(NodeOwner.Player1);

            // берём последние p2 для Player2
            for (int i = 0; i < Mathf.Min(p2, _nodes.Count); i++)
                _nodes[_nodes.Count - 1 - i].SetOwner(NodeOwner.Player2);
        }


        private void GenerateEdges()
        {
            var used = new HashSet<ulong>();

            for (int i = 0; i < _nodes.Count; i++)
            {
                for (int j = 1; j <= connectionsPerNode; j++)
                {
                    int other = (i + j) % _nodes.Count;

                    int a = Mathf.Min(i, other);
                    int b = Mathf.Max(i, other);

                    ulong key = ((ulong)(uint)a << 32) | (uint)b;
                    if (!used.Add(key))
                    {
                        continue; // уже есть такое ребро
                    }

                    CreateEdge(_nodes[i], _nodes[other]);
                }
            }
        }


        private void CreateEdge(NodeView a, NodeView b)
        {
            LineRenderer lr = Instantiate(edgePrefab, transform);
            
            var edgeView = lr.GetComponent<EdgeView>();
            var netObj = lr.GetComponent<NetworkObject>();

            if (edgeView != null && netObj != null)
            {
                if (IsServer)
                {
                    netObj.Spawn();
                    edgeView.SetPoints(a.transform.position, b.transform.position);
                }
            }
            else
            {
                lr.positionCount = 2;
                lr.SetPosition(0, a.transform.position);
                lr.SetPosition(1, b.transform.position);
            }

            a.AddNeighbor(b);
            b.AddNeighbor(a);
        }
    }
}
