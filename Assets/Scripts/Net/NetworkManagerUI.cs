using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private void Awake()
    {
        if (hostButton) hostButton.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
        if (clientButton) clientButton.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
    }
}
