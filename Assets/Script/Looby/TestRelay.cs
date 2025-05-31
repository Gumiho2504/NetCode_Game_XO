using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class TestRelay : MonoBehaviour
{
    [SerializeField] private Button createRelayButton, joinRelayButton;
    [SerializeField] private InputField relayCodeInputField;
    private void Awake()
    {
        createRelayButton.onClick.AddListener(CreateRelay);
        joinRelayButton.onClick.AddListener(JoinRelay);
        relayCodeInputField.onValueChanged.AddListener((value) =>
        {
            this.joinCode = value;
        });
    }
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in : " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("JoinCode : " + joinCode);
            this.joinCode = joinCode;
            //RelayServerData relayServerData;
            // relayServerData = new RelayServerData(Allocation : allocation,co "ref");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key, allocation.ConnectionData);
            //  NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }

    }

    private string joinCode;
    private async void JoinRelay()
    {
        try
        {

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData
                , joinAllocation.HostConnectionData
            );

            // NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(relayConnectionData);
            NetworkManager.Singleton.StartClient();

        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
