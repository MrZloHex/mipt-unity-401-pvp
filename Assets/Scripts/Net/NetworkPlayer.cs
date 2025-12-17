using Unity.Netcode;
using UnityEngine;
using Core;

public class NetworkPlayer : NetworkBehaviour
{
    public NetworkVariable<int> ClickPower = new NetworkVariable<int>(1);

    public NodeOwner Team { get; private set; }

    public override void OnNetworkSpawn()
    {
        // Determine team based on ClientID (Host = 0 -> Player1, Client = 1 -> Player2)
        Team = OwnerClientId == 0 ? NodeOwner.Player1 : NodeOwner.Player2;

        if (IsOwner)
        {
            Debug.Log($"Spawned NetworkPlayer for Client {OwnerClientId}. Assigned Team: {Team}");

            var clicker = FindFirstObjectByType<InputClicker>();
            if (clicker != null)
            {
                clicker.SetLocalPlayer(Team);
                clicker.SetNetworkPlayer(this);
            } 
        }
    }

    [ServerRpc]
    public void BuyClickUpgradeServerRpc()
    {
        Debug.Log($"[Server] BuyClickUpgrade received from {OwnerClientId} (Team: {Team})");
        var goldSystem = FindFirstObjectByType<GoldIncomeSystem>();
        if (goldSystem != null)
        {
            if (goldSystem.TrySpendGold(Team, 10))
            {
                ClickPower.Value += 1;
                Debug.Log($"[Server] Upgrade Success! New ClickPower: {ClickPower.Value}");
            }
            else
            {
                Debug.Log($"[Server] Not enough gold for {Team}");
            }
        }
        else
        {
            Debug.LogError("[Server] GoldIncomeSystem not found!");
        }
    }

    [ServerRpc]
    public void BuyGoldUpgradeServerRpc()
    {
        Debug.Log($"[Server] BuyGoldUpgrade received from {OwnerClientId} (Team: {Team})");
        var goldSystem = FindFirstObjectByType<GoldIncomeSystem>();
        if (goldSystem != null)
        {
            if (goldSystem.TrySpendGold(Team, 10))
            {
                goldSystem.AddIncomeBonus(Team, 1);
                Debug.Log($"[Server] Upgrade Success! Added Income Bonus.");
            }
            else
            {
                Debug.Log($"[Server] Not enough gold for {Team}");
            }
        }
        else
        {
            Debug.LogError("[Server] GoldIncomeSystem not found!");
        }
    }
}
