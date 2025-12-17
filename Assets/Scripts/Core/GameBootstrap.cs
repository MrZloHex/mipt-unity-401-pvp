using Unity.Netcode;
using UnityEngine;

namespace Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GoldIncomeSystem goldIncomePrefab;

        private void Start()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsServer)
                return;

            // чтобы не спавнить второй раз
            if (FindFirstObjectByType<GoldIncomeSystem>() != null)
                return;

            var go = Instantiate(goldIncomePrefab);
            go.GetComponent<NetworkObject>().Spawn();
        }
    }
}

