using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine.UI;
using System.Collections.Generic;
public class TestLooby : MonoBehaviour
{
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button listLobbiesButton;
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private Button quickJoinLobbyButton;
    [SerializeField] private Button updateGameModeButton;
    [SerializeField] private Button updatePlayerNameButton;
    [SerializeField] private Button listPlayerNameButton;
    [SerializeField] private Button leaveLobbyButton, kickButton, deleteLobbyButton;
    [SerializeField] private InputField lobbyCodeInput;

    private Lobby hostLobby;
    private Lobby joinedLobby;
    private string playerName = "Gumiho";
    string loobyCode;
    private float heartbeatTimer;
    private void Awake()
    {
        createLobbyButton.onClick.AddListener(CreateLooby);
        listLobbiesButton.onClick.AddListener(ListLobbies);
        joinLobbyButton.onClick.AddListener(() => { JoinLobbyByCode(loobyCode); });
        quickJoinLobbyButton.onClick.AddListener(QuickJoinLobby);
        lobbyCodeInput.onValueChanged.AddListener(OnInputChanged);
        updateGameModeButton.onClick.AddListener(() => UpdateLobbyGameMode("OnShot"));
        listPlayerNameButton.onClick.AddListener(ListPlayer);
        updatePlayerNameButton.onClick.AddListener(() => UpdatePlayerName("Richart"));
        leaveLobbyButton.onClick.AddListener(() => LeaveLobby());
        kickButton.onClick.AddListener(() => KickPlayer());
        deleteLobbyButton.onClick.AddListener(() => DeleteLobby());

    }

    private void OnInputChanged(string value)
    {
        loobyCode = value;
    }
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in : " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerName += Random.Range(0, 100).ToString();
    }

    private void Update()
    {
        HandleHeartbeatTimer();
        HandlePullLobbyForUpdate();
    }

    private async void HandleHeartbeatTimer()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;

            if (heartbeatTimer < 0)
            {
                float maxHeartbeatTimer = 15f;
                heartbeatTimer = maxHeartbeatTimer;
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    float lobbyUpdateTimer;
    private async void HandlePullLobbyForUpdate()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;

            if (lobbyUpdateTimer < 0)
            {
                float maxHeartbeatTimer = 1.1f;
                lobbyUpdateTimer = maxHeartbeatTimer;
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
            }
        }
    }


    private async void CreateLooby()
    {
        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions()
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>()
                {
                    {
                        "GameMode",
                        new DataObject(DataObject.VisibilityOptions.Public,"Survival")
                    }
                }
            };
            string loobyName = "MyLooby";
            int maxPlayers = 4;
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(loobyName, maxPlayers, createLobbyOptions);
            hostLobby = lobby;
            joinedLobby = hostLobby;
            loobyCode = lobby.LobbyCode;
            Debug.Log("Looby created : " + lobby.Name + "," + lobby.MaxPlayers + "," + lobby.Id + "," + lobby.LobbyCode);
            ListPlayer(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private Player GetPlayer()
    {
        return new Player(
                    data: new Dictionary<string, PlayerDataObject>()
                    {
                        {
                            "name",
                            new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,playerName
                                )
                        }
                    }
                );
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions()
            {
                Count = 25,
                Filters = new List<QueryFilter>()
                {
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "1"),

                },
                Order = new List<QueryOrder>()
                {
                    new QueryOrder(
                       false,
                        QueryOrder.FieldOptions.Created)
                }
            };
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            Debug.Log("Lobby count : " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + "," + lobby.MaxPlayers + ", Id :" + lobby.Id + ",Mode :" + lobby.Data["GameMode"].Value + ",Code :" + lobby.LobbyCode);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            // QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            // Lobby lobby = queryResponse.Results[0];
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions()
            {
                Player = GetPlayer(),
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;
            Debug.Log("Joined lobby : " + lobbyCode + ",Mode" + joinedLobby.Data["GameMode"].Value);
            ListPlayer(joinedLobby);

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void QuickJoinLobby()
    {
        try
        {
            await Lobbies.Instance.QuickJoinLobbyAsync();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void ListPlayer()
    {
        ListPlayer(joinedLobby);
    }

    private void ListPlayer(Lobby lobby)
    {
        Debug.Log("Player in lobby : " + lobby.Name + ",Mode :" + lobby.Data["GameMode"].Value);

        foreach (Player player in lobby.Players)
        {
            Debug.Log("PlayerId : " + player.Id + ", Name : " + player.Data["name"].Value);
        }
    }

    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            print("Data " + gameMode);
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>()
                {
                    {
                        "GameMode",
                        new DataObject(DataObject.VisibilityOptions.Public,gameMode)
                    }
                }
            });
            joinedLobby = hostLobby;

            ListPlayer(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            await Lobbies.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions()
            {
                Data = new Dictionary<string, PlayerDataObject>()
                {
                    {
                        "name",
                        new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,newPlayerName)
                    }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[0].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void MigrateLobbyHost()
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions(
                )
            {
                HostId = joinedLobby.Players[0].Id
            });
            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

}// end of class
