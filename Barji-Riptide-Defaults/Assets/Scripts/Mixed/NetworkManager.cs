using UnityEngine;
using RiptideNetworking.Transports.SteamTransport;
using RiptideNetworking.Utils;
using System;
using System.Collections;
using RiptideNetworking;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour
{
    [Header("Client")]
    public Transform clientObjectsTransform;

    [SerializeField] private GameObject localPlayerPrefab;
    [SerializeField] private GameObject nonLocalPlayerPrefab;
    public GameObject LocalPlayerPrefab => localPlayerPrefab;
    public GameObject NonLocalPlayerPrefab => nonLocalPlayerPrefab;
    
    internal Client Client { get; private set; }

    [Header("Server")]
    [SerializeField] private GameObject serverPlayerPrefab;
    public GameObject ServerPrefab => serverPlayerPrefab;

    internal Server Server { get; private set; }

    //Mixed
    private static NetworkManager _singleton;
    internal static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    #region Mixed

    private void Awake()
    {
        Singleton = this;
    }

    void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam is not initialized.");
            return;
        }

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
        
        //SERVER
        SteamServer steamServer = new SteamServer();
        Server = new Server(steamServer);
        Server.ClientConnected += NewPlayerConnected;
        Server.ClientDisconnected += ServerPlayerLeft;

        //CLIENT
        Client = new Client(new SteamClient(steamServer));
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        Client.ClientDisconnected += ClientPlayerLeft;
        Client.Disconnected += DidConnect;
    }

    private void FixedUpdate()
    {
        if (Server == null || Client == null)
            Debug.LogError("Couldn't start Server and/or Client. Check that steam is open.");

        if (Server.IsRunning)
            Server.Tick();
        if (Client.IsConnected)
            Client.Tick();
    }

    private void OnApplicationQuit()
    {
        LobbyManager.Singleton.LeaveLobby();
        StopServer();
        Server.ClientConnected -= NewPlayerConnected;
        Server.ClientDisconnected -= ServerPlayerLeft;

        DisconnectClient();
        Client.Connected -= DidConnect;
        Client.ConnectionFailed -= FailedToConnect;
        Client.ClientDisconnected -= ClientPlayerLeft;
        Client.Disconnected -= DidDisconnect;
    }

    void OnDestroy()
    {
        LobbyManager.Singleton.LeaveLobby();
        StopServer();
        Server.ClientConnected -= NewPlayerConnected;
        Server.ClientDisconnected -= ServerPlayerLeft;

        DisconnectClient();
        Client.Connected -= DidConnect;
        Client.ConnectionFailed -= FailedToConnect;
        Client.ClientDisconnected -= ClientPlayerLeft;
        Client.Disconnected -= DidDisconnect;
    }


    #endregion

    #region Client

    private void DidConnect(object sender, EventArgs e)
    {
        ClientPlayer.List = new Dictionary<ushort, ClientPlayer>();
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServer.playerName);
        message.Add(Steamworks.SteamFriends.GetPersonaName());
        message.Add((ulong)Steamworks.SteamUser.GetSteamID());
        Client.Send(message);
    }

    private void FailedToConnect(object sender, EventArgs e)
    {
        UIManager.Singleton.LeaveClicked();
    }

    private void ClientPlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        if(ClientPlayer.List.ContainsKey(e.Id))
        {
            Destroy(ClientPlayer.List[e.Id].gameObject);
            ClientPlayer.List.Remove(e.Id);
        }
    }

    private void DidDisconnect(object sender, EventArgs e)
    {
        foreach (ClientPlayer player in ClientPlayer.List.Values)
            Destroy(player.gameObject);
        
        ClientPlayer.localPlayer = null;
        ClientPlayer.List.Clear();
        ServerPlayer.List.Clear();
        UIManager.Singleton.BackToMain();
    }

    #endregion

    #region Server

    internal void DisconnectClient()
    {
        Client.Disconnect();
        foreach (ClientPlayer player in ClientPlayer.List.Values)
            Destroy(player.gameObject);
    }

    internal void StopServer()
    {
        Server.Stop();
        foreach (ServerPlayer player in ServerPlayer.List.Values)
            Destroy(player.gameObject);
    }

    private void ServerPlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        if(ServerPlayer.List.ContainsKey(e.Id))
        {
            Destroy(ServerPlayer.List[e.Id].gameObject);
            ServerPlayer.List.Remove(e.Id);
        }   
    }

    private void NewPlayerConnected(object sender, ServerClientConnectedEventArgs e)
    {
        foreach (ServerPlayer player in ServerPlayer.List.Values)
        {
            if (player.ID != e.Client.Id)
                player.SendSpawn(e.Client.Id);
        }
    }
    #endregion
}
