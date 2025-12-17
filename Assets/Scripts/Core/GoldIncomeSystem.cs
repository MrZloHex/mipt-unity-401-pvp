using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Core
{
    public sealed class GoldIncomeSystem : NetworkBehaviour
    {
        [SerializeField] private GraphGenerator graph;
        [SerializeField] private TMP_Text goldText;

        [Header("Tick")]
        [SerializeField] private float tickSeconds = 1.0f;

        [Header("Income")]
        [SerializeField] private int goldPerValuePerTick = 1;

        // Для UI: кто “я” на этом клиенте
        [SerializeField] private NodeOwner localPlayer = NodeOwner.Player1;

        public NetworkVariable<int> GoldP1 = new(0);
        public NetworkVariable<int> GoldP2 = new(0);

        private float timer;

        public override void OnNetworkSpawn()
        {
            // Простейшее сопоставление ролей:
            // Host/Server -> Player1, Client -> Player2
            if (NetworkManager != null)
            {
                localPlayer = (NetworkManager.IsHost || NetworkManager.IsServer)
                    ? NodeOwner.Player1
                    : NodeOwner.Player2;
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

            int g = (localPlayer == NodeOwner.Player1) ? GoldP1.Value : GoldP2.Value;
            goldText.text = $"Gold: {g}";
        }
    }
}

