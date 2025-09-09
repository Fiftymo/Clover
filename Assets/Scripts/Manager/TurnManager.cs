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
    public int currentPlayerIndex = 0; // ��ǰ�غ����������0-3��
    [SyncVar] public PlayerCore currentPlayer;
    public TextMeshProUGUI turnText;   // ��ǰ�غ�����ʾ�ı�
    [SyncVar(hook = nameof(OnTurnCountChanged))]
    public int turnCount;          // ��ǰ�غ���
    private int playersCompleted = 0;      // ���غ�����ɵ������
    public List<PlayerCore> players = new List<PlayerCore>();

    [Header("�غϽ���")]
    public List<int> coinReward = new List<int>() { 0, 5, 10};
    public List<int> cardReward = new List<int>() { 0, 1, 2};
    [Header("����Ҫ��")]
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
    // ע�����
    public void RegisterPlayer(PlayerCore player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
            players = players.OrderBy(p => p.playerData.playerIndex).ToList();
        }
        currentPlayer = players[0];
    }
    // �����ȡ���
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
    // �л�����һ����һغ�
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
        CameraController.Instance.ResetToAutoFocus(); // ֪ͨ����ͷ����
    }
    // �����ܻغ�
    [Server]
    private void ProcessEndOfTurn()
    {
        // ���ûغ�״̬
        turnCount++;
        // ���ŻغϽ���
        DistributeTurnRewards();
        playersCompleted = 0;
        ResetPlayerActions();

        Debug.Log($"Turn {turnCount} start");
    }
    // ���ŻغϽ���
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
    // �������״̬
    [Server]
    private void ResetPlayerActions()
    {
        foreach (PlayerCore player in players)
        {
            player.playerMove.ResetActionState();
            player.playerData.cardPoint = player.playerData.cardPointMax;
        }
    }
    // �غ����仯ʱ��ͬ���ص�
    private void OnTurnCountChanged(int oldValue, int newValue)
    {
        turnText.text = turnCount.ToString();
    }
    // ��ǰ��ұ仯ʱ�Ļص�
    private void OnCurrentPlayerChanged(int oldIndex, int newIndex)
    {
        Debug.Log($"��ǰ�غ���ң����{newIndex + 1}");
    }
    //-------------------------------------��־��ʾ-----------------------------------------//
    [ClientRpc]
    private void RpcLogTurnStart(int turnCount, int goldAmount)
    {
        LogManager.Instance.LogTurnStart(turnCount, goldAmount);
    }
}