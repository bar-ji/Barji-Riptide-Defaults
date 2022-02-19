using Steamworks;
using UnityEngine;
using RiptideNetworking;
using System.Collections;

public class LobbyManager : MonoBehaviour
{
    private static LobbyManager _singleton;
    internal static LobbyManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(LobbyManager)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEnter;

    private const string HostAddressKey = "HostAddress";
    public CSteamID lobbyId {get; private set;}

    public ELobbyType lobbyType = ELobbyType.k_ELobbyTypePublic;
    public int maxPlayers = 10;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam is not initialized!");
            return;
        }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
    }

    internal void CreateLobby()
    {
        SteamMatchmaking.CreateLobby(lobbyType, maxPlayers);
    }

    public void UpdateLobbyVisibility(bool visible)
    {
        SteamMatchmaking.SetLobbyJoinable(lobbyId, visible);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            UIManager.Singleton.LobbyCreationFailed();
            return;
        }

        lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(lobbyId, HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyMemberLimit(lobbyId, 10);
        UIManager.Singleton.LobbyCreationSucceeded(callback.m_ulSteamIDLobby);

        NetworkManager.Singleton.Server.Start(0, 10);
        NetworkManager.Singleton.Client.Connect("127.0.0.1");
    }

    internal void JoinLobby(ulong lobbyId)
    {
        if(NetworkManager.Singleton.Client.IsConnected)
        {
            Debug.LogWarning("Can't join a room when already connected to one.");
            return;
        }
        SteamMatchmaking.JoinLobby(new CSteamID(lobbyId));
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEnter(LobbyEnter_t callback)
    {
        if (NetworkManager.Singleton.Server.IsRunning)
            return;

        if(NetworkManager.Singleton.Client.IsConnected)
        {
            Debug.LogWarning("Can't join a room when already connected to one.");
            return;
        }
        
        lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        print(lobbyId);
        string hostAddress = SteamMatchmaking.GetLobbyData(lobbyId, HostAddressKey);

        NetworkManager.Singleton.Client.Connect(hostAddress);
        UIManager.Singleton.LobbyEntered();
    }

    internal void LeaveLobby()
    {
        if(ClientPlayer.localPlayer == null) return;
        DoDisconnect(lobbyId);
    }

    IEnumerator DelayedServerShutdown(float delay)
    {
        yield return new WaitForSeconds(delay);
        DoDisconnect(lobbyId);
    }

    public static void DoDisconnect(CSteamID lobbyId)
    {
        NetworkManager.Singleton.StopServer();
        NetworkManager.Singleton.DisconnectClient();
        SteamMatchmaking.LeaveLobby(lobbyId);
    }
}