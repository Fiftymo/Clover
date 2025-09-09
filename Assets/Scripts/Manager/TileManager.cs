using Mirror;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TileManager : NetworkBehaviour
{
    [Header("相关面板")]
    public GameObject homePanel;
    public GameObject eventPanel;
    [Header("面板组件")]
    public TextMeshProUGUI bonusText;
    [Header("格子列表")]
    public List<Tile> allTiles = new List<Tile>();
    public static TileManager Instance;

    private PlayerCore currentPlayer;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        allTiles = FindObjectsByType<Tile>(
            FindObjectsSortMode.None
            ).ToList();
    }
    // 按位置查找
    public Tile GetTileAtPosition(Vector3 pos, float tolerance = 0.1f)
    {
        foreach (var tile in allTiles)
        {
            if (Vector3.Distance(tile.transform.position, pos) <= tolerance)
            {
                return tile;
            }
        }
        Debug.LogWarning($"No tile found near position {pos}");
        return null;
    }
    // 按步数查找
    public List<Tile> GetTileBySteps(Tile current, int steps)
    {
        if (current ==  null || steps <= 0)
        {
            Debug.LogWarning($"tile not find : Invalid parameters");
            return null;
        }

        HashSet<Tile> visited = new HashSet<Tile>();
        List<Tile> tilesCanReach = new List<Tile>();
        Queue<(Tile tile, int remainingSteps)> queue = new Queue<(Tile, int)>();

        queue.Enqueue((current, steps));
        visited.Add(current);
        tilesCanReach.Add(current);

        while (queue.Count > 0)
        {
            var output = queue.Dequeue();
            Tile currentTile = output.tile;
            int remainingSteps = output.remainingSteps;

            if (remainingSteps <= 0) continue;

            foreach (Tile neighbor in currentTile.neighbors)
            {
                if (visited.Contains(neighbor)) continue;

                visited.Add(neighbor);
                tilesCanReach.Add(neighbor);
                queue.Enqueue((neighbor, remainingSteps - 1));
            }
        }
        Debug.Log($"find {tilesCanReach.Count} reachable Tile");
        return tilesCanReach;
    }
    //-------停留效果处理-------------------------------------------------------------------//
    [Server]
    public void OnHome(PlayerCore player)
    {
        player.playerData.ChangeHp(2);
        bool isLvUP = false;
        if (player.playerData.level < TurnManager.Instance.lvUP.Count &&
            player.playerData.coin >= TurnManager.Instance.lvUP[player.playerData.level])
        {
            //player.playerData.coin -= TurnManager.Instance.lvUP[player.playerData.level]; // 扣除金币
            player.playerData.ChangeLevel(1);
            isLvUP = true;
        }
        RpcLogOnHome(player, isLvUP);
    }
    [Server]
    public void OnBonus(PlayerCore player)
    {
        int coin = Random.Range(1, 7) * 2;
        player.playerData.ChangeCoin(coin);
        RpcLogOnBonus(player, coin);
    }
    [Server]
    public void OnAttack(PlayerCore player)
    {
        currentPlayer = player;
        player.playerUI.TargetShowPlayerChoice("teleport","none", 0, false);
    }
    [Server]
    public void OnShop(PlayerCore player)
    {
        // 翻开3张商店卡牌
        //ShopManager.Instance.ShowShopItems(player);
        player.playerTurn.OnTurnDone();
        RpcLogOnShop(player);
    }
    [Server]
    public void OnMove(PlayerCore player)
    {
        player.playerMove.rollCounts++;
        RpcLogOnMove(player);
    }
    [Server]
    public void OnDraw(PlayerCore player)
    {
        player.playerHand.ServerDrawCards(2);
        RpcLogOnDraw(player,2);
    }
    [Server]
    public void OnTrap(PlayerCore player)
    {
        player.playerData.hp = Mathf.Max(player.playerData.hp - 2, 0);
        player.playerData.UpdatePlayerUI(player.playerData.playerIndex);
        RpcLogOnTrap(player);
    }
    [Server]
    public void OnTeleport(PlayerCore player)
    {
        Tile targetTile = player.playerMove.currentTile.teleportTarget;
        player.playerMove.Teleport(targetTile);
    }
    [Server]
    public void OnEvent(PlayerCore player)
    {
        // 随机事件（需实现事件系统）
        //EventManager.Instance.TriggerRandomEvent(player);
        RpcLogOnEvent(player);
    }
    [Server]
    public void OnFire(PlayerCore player)
    {
        currentPlayer = player;
        //player.playerUI.RpcShowPlayerSelectUI("选择炮击目标", "fire", player.playerData.playerIndex, false);
        player.playerUI.TargetShowPlayerChoice("damage","none",0,false);
    }
    IEnumerator Delay()
    {
        yield return new WaitForSeconds(1);
    }
    //---------------------------------------------------------------------------//
    // 选择目标后处理---------------------------------------------------------//
    [Command]
    public void CmdHandleTeleport(PlayerCore curPlayer, int targetIndex)
    {
        HandleTeleport(curPlayer, targetIndex);
    }
    [Server]
    private void HandleTeleport(PlayerCore curPlayer, int targetIndex)
    {
        currentPlayer = curPlayer;
        if (targetIndex == currentPlayer.playerData.playerIndex) return;
      
        if (targetIndex >= 0 && targetIndex <= TurnManager.Instance.players.Count)
        {
            var targetPlayer = TurnManager.Instance.GetPlayerByIndex(targetIndex);
            currentPlayer.playerMove.Teleport(targetPlayer.playerMove.currentTile);
            LogManager.Instance.LogTeleport(currentPlayer, targetPlayer);

            currentPlayer.playerTurn.OnTurnDone();
        }
        else
            currentPlayer.playerTurn.OnTurnDone();
    }
    [Command]
    public void CmdHandleDamage(PlayerCore curPlayer, int targetIndex, int damage)
    {
        HandleDamage(curPlayer, targetIndex, damage);
    }
    [Server]
    private void HandleDamage(PlayerCore curPlayer, int targetIndex, int damage)
    {
        currentPlayer = curPlayer;
        if (targetIndex == currentPlayer.playerData.playerIndex) return;

        if (targetIndex >= 0 && targetIndex <= TurnManager.Instance.players.Count)
        {
            var targetPlayer = TurnManager.Instance.GetPlayerByIndex(targetIndex);
            targetPlayer.playerData.HandleDamage(damage);
            LogManager.Instance.LogCauseDamage(currentPlayer, targetPlayer, damage);

            currentPlayer.playerTurn.OnTurnDone();
        }
        else
            currentPlayer.playerTurn.OnTurnDone();
    }
    //--------------------------------------------------------------------------//
    [ClientRpc]
    public void RpcLogOnHome(PlayerCore player, bool isLvUP)
    {
        LogManager.Instance.LogHome(player, isLvUP);
    }
    [ClientRpc]
    public void RpcLogOnBonus(PlayerCore player, int coin)
    {
        LogManager.Instance.LogBonus(player, coin);
    }
    [ClientRpc]
    public void RpcLogOnShop(PlayerCore player)
    {
    }
    [ClientRpc]
    public void RpcLogOnMove(PlayerCore player)
    {
        LogManager.Instance.LogMove(player);
    }
    [ClientRpc]
    public void RpcLogOnDraw(PlayerCore player,int count)
    {
        LogManager.Instance.LogDraw(player,count);
    }
    [ClientRpc]
    public void RpcLogOnTrap(PlayerCore player)
    {
        LogManager.Instance.LogSelfDamage(player, 2);
    }
    [ClientRpc]
    public void RpcLogOnEvent(PlayerCore player)
    {
    }
}
