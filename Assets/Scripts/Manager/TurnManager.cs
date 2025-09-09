using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.Events;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;

    [SyncVar(hook = nameof(OnCurrentPlayerChanged))]
    public int currentPlayerIndex = 0; // 当前回合玩家索引（0-3）
    [SyncVar] public PlayerCore currentPlayer;
    public TextMeshProUGUI turnText;   // 当前回合数显示文本
    [SyncVar(hook = nameof(OnTurnCountChanged))]
    public int turnCount;          // 当前回合数
    private int playersCompleted = 0;      // 本回合已完成的玩家数
    public List<PlayerCore> players = new List<PlayerCore>();

    [Header("回合奖励")]
    public List<int> coinReward = new List<int>() { 0, 5, 10};
    public List<int> cardReward = new List<int>() { 0, 1, 2};
    [Header("升级要求")]
    public List<int> lvUP = new List<int>() { 15, 30, 60 };
    private void Awake()
    {
        Instance = this;
        currentPlayerIndex = 0;
        turnCount = 1;
    }
    public override void OnStartServer()
    {
        GlobalDeckManager.Instance.InitializeDeck();
    }
    // 注册玩家
    public void RegisterPlayer(PlayerCore player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
            players = players.OrderBy(p => p.playerData.playerIndex).ToList();
        }
        currentPlayer = players[0];
    }
    // 相机获取玩家
    public PlayerCore GetCurrentPlayer()
    {
        if (players.Count == 0 || currentPlayerIndex >= players.Count)
            return null;
        return players[currentPlayerIndex];
    }
    public PlayerCore GetPlayerByIndex(int index)  //0-3
    {
        if (index >= 0 && index < players.Count)
            return players[index];
        return null;
    }
    // 切换到下一个玩家回合
    [Server]
    public void NextPlayer()
    {
        playersCompleted++;
        if (playersCompleted >= players.Count)
        {
            ProcessEndOfTurn();
        }
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        currentPlayer = players[currentPlayerIndex];
        currentPlayer.playerData.ApplyTurnEffects();
        if (currentPlayer.playerData.isDead)
        {
            currentPlayer.playerData.isDead = false;
            currentPlayer.playerData.InitPlayer();
            NextPlayer();
        }
        CameraController.Instance.ResetToAutoFocus(); // 通知摄像头重置
    }
    // 结束总回合
    [Server]
    private void ProcessEndOfTurn()
    {
        // 重置回合状态
        turnCount++;
        // 发放回合奖励
        DistributeTurnRewards();
        playersCompleted = 0;
        ResetPlayerActions();

        Debug.Log($"Turn {turnCount} start");
    }
    // 发放回合奖励
    [Server]
    private void DistributeTurnRewards()
    {
        int coin = coinReward[0];
        int card = cardReward[0];
        if (turnCount >= 6 && turnCount <= 10)
        {
            coin = coinReward[1];
            card = cardReward[1];
        }
        else if (turnCount > 10)
        {
            coin = coinReward[2];
            card = cardReward[2];
        }
        if (coin > 0)
        {
            foreach (PlayerCore player in players)
            {
                player.playerData.ChangeCoin(coin);
                player.playerHand.ServerDrawCards(card);
            }
        }
        RpcLogTurnStart(turnCount, coin);
    }
    // 重置玩家状态
    [Server]
    private void ResetPlayerActions()
    {
        foreach (PlayerCore player in players)
        {
            player.playerMove.ResetActionState();
            player.playerData.cardPoint = player.playerData.cardPointMax;
        }
    }
    // 回合数变化时的同步回调
    private void OnTurnCountChanged(int oldValue, int newValue)
    {
        turnText.text = turnCount.ToString();
    }
    // 当前玩家变化时的回调
    private void OnCurrentPlayerChanged(int oldIndex, int newIndex)
    {
        Debug.Log($"当前回合玩家：玩家{newIndex + 1}");
    }
    //-------------------------------------日志显示-----------------------------------------//
    [ClientRpc]
    private void RpcLogTurnStart(int turnCount, int goldAmount)
    {
        LogManager.Instance.LogTurnStart(turnCount, goldAmount);
    }
}