using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Determine team based on ClientID (Host = 0 -> Player1, Client = 1 -> Player2)
            NodeOwner myTeam = OwnerClientId == 0 ? NodeOwner.Player1 : NodeOwner.Player2;
            
            Debug.Log($"Spawned NetworkPlayer for Client {OwnerClientId}. Assigned Team: {myTeam}");

            var clicker = FindFirstObjectByType<InputClicker>();
            if (clicker != null)
            {
                clicker.SetLocalPlayer(myTeam);
            } 
        }
    }
}
