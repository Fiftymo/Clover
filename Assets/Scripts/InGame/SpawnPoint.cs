using Mirror;
using UnityEngine;

public class SpawnPoint : NetworkBehaviour
{
    [Header("关联设置")]
    public Tile startTile;       // 玩家的起始格子（如Home格子）
    public Tile initialPrevTile; // 初始previousTile（如Teleport4）

    [Server]
    public void SetupPlayer(PlayerCore player)
    {
        player.playerMove.currentTile = startTile;
        startTile.currentPlayers.Add(player);
        player.playerMove.previousTile = initialPrevTile;
        player.transform.position = startTile.transform.position;
        Debug.Log($"玩家{player.playerData.playerIndex} 初始化在 {startTile.name}，前一个格子为 {initialPrevTile.name}");
    }

    [ClientRpc]
    private void RpcSetupAllClients(PlayerCore player, Tile cur, Tile pre)
    {
        player.playerMove.currentTile = cur;
        player.playerMove.previousTile = pre;
    }
}