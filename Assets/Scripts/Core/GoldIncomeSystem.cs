using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Core
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class GoldIncomeSystem : NetworkBehaviour
    {
        [SerializeField] private GraphGenerator graph;
        [SerializeField] private TMP_Text goldText;

        [Header("Tick")]
        [SerializeField] private float tickSeconds = 1.0f;

        [Header("Income")]
        [SerializeField] private int goldPerValuePerTick = 1;

        // Для UI: кто “я” на этом клиенте
        private NodeOwner localPlayer = NodeOwner.Neutral;

        public NetworkVariable<int> GoldP1 = new(0);
        public NetworkVariable<int> GoldP2 = new(0);

        private float timer;

        private void Awake()
        {
            if (graph == null)
            {
                graph = FindObjectOfType<GraphGenerator>();
            }
        }

        public override void OnNetworkSpawn()
        {
            // Determine team based on ClientID (Host = 0 -> Player1, Client = 1 -> Player2)
            // This matches the logic in NetworkPlayer.cs
            if (NetworkManager.Singleton != null)
            {
                ulong id = NetworkManager.Singleton.LocalClientId;
                localPlayer = (id == 0) ? NodeOwner.Player1 : NodeOwner.Player2;
                Debug.Log($"[GoldSystem] Spawned. LocalID: {id}, Team: {localPlayer}");
            }

            GoldP1.OnValueChanged += (_, __) => UpdateGoldUI();
            GoldP2.OnValueChanged += (_, __) => UpdateGoldUI();

            UpdateGoldUI();
        }

        private void Update()
        {
            if (!IsServer)
            {
                return;
            }

            if (graph == null)
            {
                return;
            }

            timer += Time.deltaTime;
            if (timer < tickSeconds)
            {
                return;
            }
            timer -= tickSeconds;

            int incP1 = 0;
            int incP2 = 0;

            var nodes = graph.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                NodeView n = nodes[i];

                int v = n.Value;
                if (v <= 0)
                {
                    continue;
                }

                if (n.Owner == NodeOwner.Player1)
                {
                    incP1 += v * goldPerValuePerTick;
                }
                else if (n.Owner == NodeOwner.Player2)
                {
                    incP2 += v * goldPerValuePerTick;
                }
            }

            if (incP1 != 0) GoldP1.Value += incP1;
            if (incP2 != 0) GoldP2.Value += incP2;
        }

        private void UpdateGoldUI()
        {
            if (goldText == null)
            {
                return;
            }

            int g = 0;
            if (localPlayer == NodeOwner.Player1) g = GoldP1.Value;
            else if (localPlayer == NodeOwner.Player2) g = GoldP2.Value;

            goldText.text = $"Gold: {g}";
        }

        public int GetGold(NodeOwner player)
        {
            if (player == NodeOwner.Player1) return GoldP1.Value;
            if (player == NodeOwner.Player2) return GoldP2.Value;
            return 0;
        }
    }
}

