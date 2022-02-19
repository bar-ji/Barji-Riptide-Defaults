internal enum ClientToServer : ushort
{
    spawnPlayer = 1,
    playerPosition,
    visualMessage,
    playerName,
    word
}

internal enum ServerToClient : ushort
{
    spawnPlayer = 1,
    playerPosition,
    visualMessage
}