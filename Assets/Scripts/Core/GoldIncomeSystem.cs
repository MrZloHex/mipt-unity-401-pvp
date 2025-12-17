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

        public NetworkVariable<int> BonusP1 = new(0);
        public NetworkVariable<int> BonusP2 = new(0);

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

            if (incP1 != 0 || BonusP1.Value != 0) GoldP1.Value += incP1 + BonusP1.Value;
            if (incP2 != 0 || BonusP2.Value != 0) GoldP2.Value += incP2 + BonusP2.Value;
        }

        public bool TrySpendGold(NodeOwner player, int amount)
        {
            if (!IsServer) return false;

            int currentGold = (player == NodeOwner.Player1) ? GoldP1.Value : GoldP2.Value;
            Debug.Log($"[GoldSystem] TrySpendGold: Player {player} wants to spend {amount}. Current: {currentGold}");

            if (player == NodeOwner.Player1)
            {
                if (GoldP1.Value >= amount)
                {
                    GoldP1.Value -= amount;
                    return true;
                }
            }
            else if (player == NodeOwner.Player2)
            {
                if (GoldP2.Value >= amount)
                {
                    GoldP2.Value -= amount;
                    return true;
                }
            }
            return false;
        }

        public void AddIncomeBonus(NodeOwner player, int amount)
        {
            if (!IsServer) return;

            if (player == NodeOwner.Player1) BonusP1.Value += amount;
            else if (player == NodeOwner.Player2) BonusP2.Value += amount;
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

