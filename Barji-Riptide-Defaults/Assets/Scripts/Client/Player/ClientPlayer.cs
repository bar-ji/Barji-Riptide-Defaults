using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class ClientPlayer : MonoBehaviour
{
    public static Dictionary<ushort, ClientPlayer> List = new Dictionary<ushort, ClientPlayer>();

    public ushort ID {get; private set;}
    public static ClientPlayer localPlayer;
    public bool isServerHost => ID == 1;

    public void Move(Vector3 newPosition, Vector3 forward)
    {
        transform.position = newPosition;

        if (ID != NetworkManager.Singleton.Client.Id) // Don't overwrite local player's forward direction to avoid noticeable rotational snapping
            transform.forward = forward;
    }

    private void OnDestroy()
    {
        List.Remove(ID);
    }

    public static void Spawn(ushort ID, Vector3 position)
    {
        ClientPlayer clientPlayer;
        if (ID == NetworkManager.Singleton.Client.Id)
        {
            clientPlayer = Instantiate(NetworkManager.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<ClientPlayer>();
            localPlayer = clientPlayer;
        }
        else
            clientPlayer = Instantiate(NetworkManager.Singleton.NonLocalPlayerPrefab, position, Quaternion.identity).GetComponent<ClientPlayer>();

        clientPlayer.name = $"Player {ID}";
        clientPlayer.ID = ID;
        List.Add(clientPlayer.ID, clientPlayer);
    }

    #region Messages
    
    [MessageHandler((ushort)ServerToClient.spawnPlayer)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(),  message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClient.playerPosition)]
    private static void ReceivePlayerPosition(Message message)
    {
        ushort playerId = message.GetUShort();
        if (List.TryGetValue(playerId, out ClientPlayer player))
            player.Move(message.GetVector3(), message.GetVector3());
    }
    
    #endregion
}