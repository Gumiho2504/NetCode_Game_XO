using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    private void Start()
    {
        hostButton.onClick.AddListener(OnHostButtonClicked);
        clientButton.onClick.AddListener(OnClientButtonClicked);
    }
    private void OnHostButtonClicked()
    {
        NetworkManager.Singleton.StartHost();
        Hide();
    }
    private void OnClientButtonClicked()
    {
        NetworkManager.Singleton.StartClient();
        Hide();
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
