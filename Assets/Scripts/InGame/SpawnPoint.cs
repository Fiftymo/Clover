using Mirror;
using UnityEngine;

public class SpawnPoint : NetworkBehaviour
{
    [Header("��������")]
    public Tile startTile;       // ��ҵ���ʼ���ӣ���Home���ӣ�
    public Tile initialPrevTile; // ��ʼpreviousTile����Teleport4��

    [Server]
    public void SetupPlayer(PlayerCore player)
    {
        player.playerMove.currentTile = startTile;
        startTile.currentPlayers.Add(player);
        player.playerMove.previousTile = initialPrevTile;
        player.transform.position = startTile.transform.position;
        Debug.Log($"���{player.playerData.playerIndex} ��ʼ���� {startTile.name}��ǰһ������Ϊ {initialPrevTile.name}");
    }

    [ClientRpc]
    private void RpcSetupAllClients(PlayerCore player, Tile cur, Tile pre)
    {
        player.playerMove.currentTile = cur;
        player.playerMove.previousTile = pre;
    }
}