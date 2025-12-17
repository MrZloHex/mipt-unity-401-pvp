using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private Button clickUpgradeButton;
    [SerializeField] private Button goldUpgradeButton;
    [SerializeField] private TMP_Text clickPowerText;

    private NetworkPlayer localNetworkPlayer;

    private void Start()
    {
        if (clickUpgradeButton != null)
        {
            clickUpgradeButton.onClick.AddListener(OnClickUpgrade);
        }

        if (goldUpgradeButton != null)
        {
            goldUpgradeButton.onClick.AddListener(OnGoldUpgrade);
        }
    }

    private void Update()
    {
        if (localNetworkPlayer == null)
        {
            FindLocalPlayer();
        }
    }

    private void FindLocalPlayer()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient) return;

        var playerObject = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (playerObject != null)
        {
            localNetworkPlayer = playerObject.GetComponent<NetworkPlayer>();
            if (localNetworkPlayer != null)
            {
                UpdateClickPowerText(localNetworkPlayer.ClickPower.Value);
                localNetworkPlayer.ClickPower.OnValueChanged += (oldVal, newVal) => UpdateClickPowerText(newVal);
            }
        }
    }

    private void UpdateClickPowerText(int power)
    {
        if (clickPowerText != null)
        {
            clickPowerText.text = $"Click Power: {power}";
        }
    }

    private void OnClickUpgrade()
    {
        if (localNetworkPlayer != null)
        {
            Debug.Log("UpgradeUI: Requesting Click Upgrade");
            localNetworkPlayer.BuyClickUpgradeServerRpc();
        }
        else
        {
            Debug.LogWarning("UpgradeUI: Local NetworkPlayer not found!");
        }
    }

    private void OnGoldUpgrade()
    {
        if (localNetworkPlayer != null)
        {
            Debug.Log("UpgradeUI: Requesting Gold Upgrade");
            localNetworkPlayer.BuyGoldUpgradeServerRpc();
        }
        else
        {
            Debug.LogWarning("UpgradeUI: Local NetworkPlayer not found!");
        }
    }
}
