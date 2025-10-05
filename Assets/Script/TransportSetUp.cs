using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class TransportSetUp : MonoBehaviour
{
    public Text ipText;
    public Text networkIpText;
    public static string GetLocalIPAddress()
    {
        string localIP = "127.0.0.1";  // fallback default

        foreach (var host in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            // Use only IPv4 and non-loopback addresses
            if (host.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = host.ToString();
                break;
            }
        }

        return localIP;
    }
    public InputField ipInput;



    public void Connect(string value)
    {

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = value;

    }

    void Start()
    {

        ipInput.onValueChanged.AddListener(Connect);
        Debug.Log("Local IP Address: " + GetLocalIPAddress());
        ipText.text = "Local IP Address: " + GetLocalIPAddress();

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = GetLocalIPAddress();
        transport.ConnectionData.Port = 7777;
        networkIpText.text = "Network IP Address: " + transport.ConnectionData.Address;
    }
}
