using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Tile : NetworkBehaviour
{
    // ����ö��
    public enum TileType { Home, Bonus, Attack, Shop, Move, Draw, Trap, Teleport, Thome, Event, Fire }

    [Header("��������")]
    public List<Tile> neighbors = new List<Tile>();   // �ھ��б�
    public readonly SyncList<PlayerCore> currentPlayers = new SyncList<PlayerCore>();

    [Header("��������")]
    public TileType tileType;
    public Tile teleportTarget;
    public int homeIndex;

    [Header("������ͼ")]
    [SerializeField] private Color linkColor = Color.cyan;

    // ��ȡ�����ƶ�·�����ų���ʱ����
    public List<Tile> GetAvailablePaths(Tile fromTile)
    {
        List<Tile> available = new List<Tile>(neighbors);
        available.Remove(fromTile); // �Ƴ���·����
        return available;
    }
    // ��ҽ���
    [Server]
    public void OnPlayerEnter(PlayerCore player)
    {
        currentPlayers.Add(player);
    }
    // ����뿪
    [Server]
    public void OnPlayerExit(PlayerCore player)
    {
        currentPlayers.Remove(player);
    }
    // ����Ч��
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
    //-------------------------------------��־��ʾ-----------------------------------------//
    // ������ʾ������
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