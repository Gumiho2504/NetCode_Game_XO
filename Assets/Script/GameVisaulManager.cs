using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameVisaulManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3f;
    public Transform crossPrefab;
    public Transform circlePrefab;
    public Transform line;
    private List<GameObject> visualGameObjectList = new List<GameObject>();
    private void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition += OnClickedOnGridPosition;
        GameManager.Instance.OnGameWin += OnGameWin;
        GameManager.Instance.OnRematch += OnRematch;
    }

    private void OnRematch(object sender, EventArgs e)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }
        foreach (GameObject gameObject in visualGameObjectList)
        {
            Destroy(gameObject);
        }
        visualGameObjectList.Clear();
    }

    private void OnGameWin(object sender, GameManager.OnGameWinArgs e)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }
        float eulerZ = 0F;
        switch (e.line.orientation)
        {
            default:
            case GameManager.Orientation.Horizontal: eulerZ = 0f; break;
            case GameManager.Orientation.Vertical: eulerZ = 90f; break;
            case GameManager.Orientation.DiagonalA: eulerZ = 45f; break;
            case GameManager.Orientation.DiagonalB: eulerZ = -45f; break;
        }
        Transform linePre = Instantiate(line, GetGridWorldPosition(e.line.centerGridPosition.x, e.line.centerGridPosition.y), Quaternion.Euler(new Vector3(0, 0, eulerZ)));
        linePre.GetComponent<NetworkObject>().Spawn();
        visualGameObjectList.Add(linePre.gameObject);
    }


    private void OnClickedOnGridPosition(object sender, GameManager.OnClickedOnGridPositionEventArgs e)
    {
        print("OnClickedOnGridPosition GameManagerVisual " + e.x + ", " + e.y);

        SpawnObjectRpc(e.x, e.y, e.playerType);
    }

    // its work both server and client
    [Rpc(SendTo.Server)]  // if client run this code its send to server
    // if the call on client that is not execute on the client its send to server to execute
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType)
    {
        Debug.Log("SpawnObject " + x + ", " + y);
        Transform prefab;
        switch (playerType)
        {
            default:
            case GameManager.PlayerType.Cross:
                prefab = crossPrefab;
                break;
            case GameManager.PlayerType.Circle:
                prefab = circlePrefab;
                break;

        }
        ;
        Transform crossTransform = Instantiate(prefab, GetGridWorldPosition(x, y), Quaternion.identity);
        crossTransform.GetComponent<NetworkObject>().Spawn();
        visualGameObjectList.Add(crossTransform.gameObject);
        // crossTransform.position = GetGridWorldPosition(x, y);
    }
    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
