
// This file is provided under The MIT License as part of RiptideNetworking.
// Copyright (c) 2021 Tom Weiland
// For additional information please see the included LICENSE.md file or view it on GitHub: https://github.com/tom-weiland/RiptideNetworking/blob/main/LICENSE.md

using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class ServerPlayer : MonoBehaviour
{
    public static Dictionary<ushort, ServerPlayer> List { get; private set; } = new Dictionary<ushort, ServerPlayer>();

    public ushort ID { get; private set; }
    public string Username { get; private set; }
    public CSteamID steamID {get;set;}


    private void OnDestroy()
    {
        List.Remove(ID);
    }

    [MessageHandler((ushort)ClientToServer.playerName)]
    private static void RecievePlayerName(ushort fromClientId, Message message)
    {
        Spawn(fromClientId, message.GetString(), message.GetULong());
    }

    public static void Spawn(ushort id, string username, ulong steamID)
    {
        ServerPlayer serverPlayer = Instantiate(NetworkManager.Singleton.ServerPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<ServerPlayer>();
        serverPlayer.name = $"[SERVER] Player {id} ({(username == "" ? "Guest" : username)})";
        serverPlayer.ID = id;
        serverPlayer.steamID = new CSteamID(steamID);
        serverPlayer.Username = username;

        serverPlayer.SendSpawn();
        List.Add(serverPlayer.ID, serverPlayer);
    }

    #region Messages

    public void SendSpawn(ushort toClient)
    {
        NetworkManager.Singleton.Server.Send(GetSpawnData(Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.spawnPlayer)), toClient);
    }
    private void SendSpawn()
    {
        NetworkManager.Singleton.Server.SendToAll(GetSpawnData(Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.spawnPlayer)));
    }

    public void SendPosition(Vector3 position, Vector3 forward)
    {
        Message message = Message.Create(MessageSendMode.unreliable, (ushort)ServerToClient.playerPosition);
        message.Add(ID);
        message.Add(position);
        message.Add(forward);
        NetworkManager.Singleton.Server.SendToAll(message, ID);
    }

    private Message GetSpawnData(Message message)
    {
        message.Add(ID);
        message.Add(transform.position);
        return message;
    }

    [MessageHandler((ushort)ClientToServer.playerPosition)]
    private static void ReceivePlayerMovement(ushort fromClientId, Message message)
    {
        ServerPlayer.List[fromClientId].SendPosition(message.GetVector3(), message.GetVector3());
    }
    #endregion
}