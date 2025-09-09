using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Tile : NetworkBehaviour
{
    // 类型枚举
    public enum TileType { Home, Bonus, Attack, Shop, Move, Draw, Trap, Teleport, Thome, Event, Fire }

    [Header("基础设置")]
    public List<Tile> neighbors = new List<Tile>();   // 邻居列表
    public readonly SyncList<PlayerCore> currentPlayers = new SyncList<PlayerCore>();

    [Header("特殊设置")]
    public TileType tileType;
    public Tile teleportTarget;
    public int homeIndex;

    [Header("调试视图")]
    [SerializeField] private Color linkColor = Color.cyan;

    // 获取可用移动路径（排除来时方向）
    public List<Tile> GetAvailablePaths(Tile fromTile)
    {
        List<Tile> available = new List<Tile>(neighbors);
        available.Remove(fromTile); // 移除来路方向
        return available;
    }
    // 玩家进入
    [Server]
    public void OnPlayerEnter(PlayerCore player)
    {
        currentPlayers.Add(player);
    }
    // 玩家离开
    [Server]
    public void OnPlayerExit(PlayerCore player)
    {
        currentPlayers.Remove(player);
    }
    // 处理效果
    [Server]
    public void HandleSpecialTile(PlayerCore player)
    {
        if (player.playerMove.isTurnDone) return;
        switch (tileType)
        {
            case TileType.Home:
                TileManager.Instance.OnHome(player);
                player.playerTurn.OnTurnDone();
                break;
            case TileType.Bonus:
                TileManager.Instance.OnBonus(player);
                player.playerTurn.OnTurnDone();
                break;
            case TileType.Attack:
                TileManager.Instance.OnAttack(player);
                break;
            case TileType.Shop:
                player.playerTurn.OnTurnDone();
                break;
            case TileType.Move:
                TileManager.Instance.OnMove(player);
                break;
            case TileType.Draw:
                TileManager.Instance.OnDraw(player);
                player.playerTurn.OnTurnDone();
                break;
            case TileType.Trap:
                TileManager.Instance.OnTrap(player);
                player.playerTurn.OnTurnDone();
                break;
            case TileType.Teleport:
                TileManager.Instance.OnTeleport(player);
                player.playerTurn.OnTurnDone();
                break;
            case TileType.Event:
                TileManager.Instance.OnEvent(player);
                player.playerTurn.OnTurnDone();
                break;
            case TileType.Fire:
                TileManager.Instance.OnFire(player);
                break;
        }
    }
    //-------------------------------------日志显示-----------------------------------------//
    // 调试显示连接线
    void OnDrawGizmosSelected()
    {
        Gizmos.color = linkColor;
        foreach (Tile neighbor in neighbors)
        {
            if (neighbor != null)
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
        }
    }
}