using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
///  Developer : HEM CHANMETREY
/// DATE : 29/05/2025
/// </summary> <summary>
/// 
/// </summary>
public class GameManager : NetworkBehaviour
{
    public enum PlayerType
    {
        None, Cross, Circle
    }
    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB
    }
    public static GameManager Instance { get; private set; }
    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    public event EventHandler OnGameStart;
    public event EventHandler OnCurrentPlayablePlayerChanged;
    public event EventHandler<OnGameWinArgs> OnGameWin;
    public event EventHandler OnRematch;
    public event EventHandler OnGameTie;
    public event EventHandler OnScoreChanged;
    public event EventHandler OnPlaceObject;
    public class OnGameWinArgs : EventArgs
    {
        public Line line;
        public PlayerType playerType;
    }
    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public Orientation orientation;
    }

    private PlayerType localPlayerType = PlayerType.Cross;
    private NetworkVariable<PlayerType> currentPlayablePlayerType = new NetworkVariable<PlayerType>();

    private PlayerType[,] playerTypeArray;
    private List<Line> lineList;

    private NetworkVariable<int> crossPlayerScore = new NetworkVariable<int>();
    private NetworkVariable<int> circlePlayerScore = new NetworkVariable<int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        playerTypeArray = new PlayerType[3, 3];
        lineList = new List<Line>
        {

            // Horizontal
            new Line {
                gridVector2IntList = new List<Vector2Int>{
                    new Vector2Int(0,0),new Vector2Int(1,0),new Vector2Int(2,0)
                },
                centerGridPosition = new Vector2Int(1,0),
                orientation = Orientation.Horizontal
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{
                    new Vector2Int(0,1),new Vector2Int(1,1),new Vector2Int(2,1)
                },
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.Horizontal
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{
                    new Vector2Int(0,2),new Vector2Int(1,2),new Vector2Int(2,2)
                },
                centerGridPosition = new Vector2Int(1,2),
                orientation = Orientation.Horizontal
            },
            // Vertical
             new Line {
                gridVector2IntList = new List<Vector2Int>{
                    new Vector2Int(0,0),new Vector2Int(0,1),new Vector2Int(0,2)
                },
                centerGridPosition = new Vector2Int(0,1),
                orientation = Orientation.Vertical
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{
                    new Vector2Int(1,0),new Vector2Int(1,1),new Vector2Int(1,2)
                },
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.Vertical
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{
                    new Vector2Int(2,0),new Vector2Int(2,1),new Vector2Int(2,2)
                },
                centerGridPosition = new Vector2Int(2,1),
                orientation = Orientation.Vertical
            },

            // Diagonals
             new Line {
                gridVector2IntList = new List<Vector2Int>{
                    new Vector2Int(0,0),new Vector2Int(1,1),new Vector2Int(2,2)
                },
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.DiagonalA
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{
                    new Vector2Int(0,2),new Vector2Int(1,1),new Vector2Int(2,0)
                },
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.DiagonalB
            },

        };
    }



    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn = " + NetworkManager.Singleton.LocalClientId.ToString());
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            localPlayerType = PlayerType.Cross;
        }
        else
        {
            localPlayerType = PlayerType.Circle;
        }
        if (IsServer)
        {

            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }
        currentPlayablePlayerType.OnValueChanged += (previousValue, newValue) =>
        {
            OnCurrentPlayablePlayerChanged?.Invoke(this, EventArgs.Empty);
        };
        crossPlayerScore.OnValueChanged += (previousValue, newValue) =>
        {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
        };
        circlePlayerScore.OnValueChanged += (previousValue, newValue) =>
        {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
        };
    }



    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            currentPlayablePlayerType.Value = PlayerType.Cross;
            TriggerOnGameStartRpc();
        }
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartRpc()
    {
        OnGameStart?.Invoke(this, EventArgs.Empty);
    }


    [Rpc(SendTo.Server)]
    public void ClickOnGridPositionRpc(int x, int y, PlayerType playerType)
    {

        if (currentPlayablePlayerType.Value != playerType)
        {
            return;
        }
        if (playerTypeArray[x, y] != PlayerType.None)
        {
            //  print($"type[{x},{y}] : {playerTypeArray[x, y]}");
            return;
        }
        playerTypeArray[x, y] = playerType;
        // Debug.Log($"ClickOnGridPosition {x}, {y}");
        TriggerOnPlacedObjectRpc();
        OnClickedOnGridPosition.Invoke(this, new OnClickedOnGridPositionEventArgs { x = x, y = y, playerType = playerType });
        currentPlayablePlayerType.Value = playerType switch
        {
            PlayerType.Circle => PlayerType.Cross,
            _ => PlayerType.Circle,
        };

        // TriggerOnCurrentPlayablePlayerChangedRpc();

        TestPlayerWinner();

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnPlacedObjectRpc()
    {
        OnPlaceObject?.Invoke(this, EventArgs.Empty);
    }


    // [Rpc(SendTo.ClientsAndHost)]
    // private void TriggerOnCurrentPlayablePlayerChangedRpc()
    // {
    //     OnCurrentPlayablePlayerChanged?.Invoke(this, EventArgs.Empty);
    // }
    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType.Value;
    }

    private bool TestPlayerWinnerLine(Line line)
    {
        return TestPlayerWinnerLine(
            playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
            playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
            playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
          );
    }

    private bool TestPlayerWinnerLine(PlayerType a, PlayerType b, PlayerType c)
    {
        return a != PlayerType.None && a == b && b == c;
    }



    private void TestPlayerWinner()
    {
        for (int i = 0; i < lineList.Count; i++)
        {
            Line line = lineList[i];
            if (TestPlayerWinnerLine(line))
            {
                Debug.Log("Win");
                currentPlayablePlayerType.Value = PlayerType.None;
                PlayerType winPlayerType = playerTypeArray[line.centerGridPosition.x, line.centerGridPosition.y];
                switch (winPlayerType)
                {
                    default:
                    case PlayerType.Cross:
                        crossPlayerScore.Value += 1;
                        break;
                    case PlayerType.Circle:
                        circlePlayerScore.Value += 1;
                        break;
                }
                TriggerOnGameWinRpc(i, winPlayerType);
                return;
            }

        }

        bool hasTie = true;
        for (int x = 0; x < playerTypeArray.GetLength(0); x++)
        {
            for (int y = 0; y < playerTypeArray.GetLength(1); y++)
            {
                if (playerTypeArray[x, y] == PlayerType.None)
                {
                    hasTie = false;
                    break;
                }
            }
        }
        if (hasTie)
        {
            TriggerOnGameTieRpc();
        }
        // if (TestPlayerWinnerLine(playerTypeArray[0, 0], playerTypeArray[1, 0], playerTypeArray[2, 0]))
        // {
        //     Debug.Log("Win");
        //     currentPlayablePlayerType.Value = PlayerType.None;

        //     OnGameWin?.Invoke(this, new OnGameWinArgs
        //     {
        //         centerGridPosition = new Vector2Int(1, 0)
        //     });
        // }

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameTieRpc()
    {
        OnGameTie?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRpc(int lineIndex, PlayerType winPlayerType)
    {
        Line line = lineList[lineIndex];
        OnGameWin?.Invoke(this, new OnGameWinArgs
        {
            line = line,
            playerType = winPlayerType
        });
    }
    [Rpc(SendTo.Server)]
    public void RematchRpc()
    {
        for (int i = 0; i < playerTypeArray.GetLength(0); i++)
        {
            for (int j = 0; j < playerTypeArray.GetLength(1); j++)
            {
                playerTypeArray[i, j] = PlayerType.None;
            }

        }
        currentPlayablePlayerType.Value = PlayerType.Cross;
        TriggerOnRematchRpc();
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnRematchRpc()
    {
        OnRematch?.Invoke(this, EventArgs.Empty);
    }

    public void GetScore(out int crossScore, out int circleScore)
    {
        crossScore = this.crossPlayerScore.Value;
        circleScore = this.circlePlayerScore.Value;
    }
}
