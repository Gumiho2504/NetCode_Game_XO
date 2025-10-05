using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class Card : MonoBehaviour
{
    public string Name;
    public string suit;
    public string rank;



}


public class CardData : INetworkSerializable
{
    string name;
    string suite;
    string rank;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref suite);

    }
}