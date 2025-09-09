// 玩家手牌管理，抽卡，使用等功能
using Mirror;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerHand : NetworkBehaviour
{
    public static PlayerHand LocalInstance { get; private set; }
    [SyncVar(hook = nameof(OnHandCountUpdated))]
    public int handCount = 0; // 仅同步手牌数量
    public int maxHandCount = 6;
    [SyncVar] public bool isDiscarding = false;
    [SyncVar] public bool isForce = false;
    [SyncVar] private int discardCount = 0;
    private int handleCardIndex = -1;

    public List<CardData> hand = new List<CardData>(); // 服务端存储实际手牌
    private PlayerCore playerCore;

    private void Awake()
    {
        playerCore = GetComponent<PlayerCore>(); // 获取附加到同一对象的 PlayerCore 组件
    }
    // 初始化玩家手牌
    [Server]
    public void ServerInitializeHand()
    {
        ServerDrawCards(4);
    }
    // 抽卡
    [Server]
    private void ServerDrawCard()
    {
        CardData card = GlobalDeckManager.Instance.DrawCard();
        if (card != null)
        {
            hand.Add(card);
            handCount++;
            // 通过 TargetRpc 通知对应客户端
            TargetAddCard(connectionToClient, card.type, card.cardName);
        }
    }
    // 批量抽卡
    [Server]
    public void ServerDrawCards(int count)
    {
        StartCoroutine(DrawMultipleCards(count));
    }

    [Server]
    private IEnumerator DrawMultipleCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            ServerDrawCard();
            yield return new WaitForSeconds(0.1f); // 防止瞬间抽卡导致同步问题
        }
    }

    [TargetRpc]
    private void TargetAddCard(NetworkConnection target, CardType type, string cardName)
    {
        Debug.Log("客户端收到卡牌：" + cardName);
        CardData cardData = Resources.Load<CardData>($"Cards/{type}/{cardName}");
        if (cardData != null)
            playerCore.playerUI.AddCardToUI(cardData);
        else
            Debug.LogError("找不到卡牌资源：" + cardName);
    }
    // // --------------用牌-------------
    [Command]
    public void CmdUseCard(int handIndex, CardType type)
    {
        if (handIndex < 0 || handIndex >= hand.Count) return;
        if (playerCore.playerHand.isDiscarding) return;
        if (playerCore.playerMove.isMoving) return;
        // 执行卡牌效果（需扩展）
        CardData card = hand[handIndex];
        handleCardIndex = handIndex;
        if (type == CardType.Effect)
        {
            if (playerCore.playerData.cardPoint >= card.cost)
            {
                CardEffectManager.Instance.HandleEffect(playerCore, card);
            }
            else
            {
                Debug.Log("点数不足，无法使用卡牌！");
            }
        }
        else if (type == CardType.Attack || type == CardType.Defense)
        {
            if (playerCore.playerUI.battleCardUI.battlePoints >= card.cost)
            {
                CardEffectManager.Instance.HandleBattle(playerCore, card);
            }
            else
            {
                Debug.Log("点数不足，无法使用卡牌！");
            }
        }
    }
    [Server]
    public void ConfirmUse()
    {
        CardData card = hand[handleCardIndex];
        // 扣除点数
        if (card.type == CardType.Effect)
            playerCore.playerData.ChangeCPoints(card.cost);
        else if (card.type == CardType.Attack || card.type == CardType.Defense)
            playerCore.playerData.ChangeBPoints(card.cost);
        // 移除卡牌
        hand.RemoveAt(handleCardIndex);
        handCount--;
        // 同步到客户端
        TargetRemoveCardFromUI(connectionToClient, handleCardIndex);
    }
    // 移除卡牌
    [TargetRpc]
    private void TargetRemoveCardFromUI(NetworkConnection target, int index)
    {
        PlayerUI.LocalInstance.RemoveCardFromUI(index);
    }
    // 牌数更新
    private void OnHandCountUpdated(int oldCount, int newCount)
    {
        if (isLocalPlayer)
            PlayerUI.LocalInstance.UpdateCardCount(newCount);
    }
    //------------------------------------弃牌-------------------------------------------
    [Server]
    public bool CheckHandOverflow()
    {
        return hand.Count > maxHandCount;
    }
    // 强制弃牌直到符合上限
    [Server]
    public void ForceDiscard()
    {
        discardCount = hand.Count - maxHandCount;
        isDiscarding = true;
        isForce = true;
        TargetShowDiscardUI(connectionToClient, discardCount);
    }
    [Server]
    public void ServerDiscardCards(int count)
    {
        discardCount = count;
        isDiscarding = true;
        isForce = false;
        TargetShowDiscardUI(connectionToClient, discardCount);
    }
    // --------------弃牌-------------
    [Command]
    public void CmdDiscardCard(int handIndex)
    {
        if (handIndex < 0 || handIndex >= hand.Count) return;
        if (!isDiscarding) return;
        hand.RemoveAt(handIndex);
        handCount--;
        discardCount--;
        Debug.Log($"player discard 1,current hand {handCount},current discardCount {discardCount}");
        TargetRemoveCardFromUI(connectionToClient, handIndex);
        if (isForce)
        {
            // 继续检测是否需要继续弃牌
            if (discardCount > 0)
            {
                ForceDiscard();
            }
            else
            {
                // 弃牌完成后允许结束回合
                isDiscarding = false;
                isForce = false;
                TargetHideDiscardPanel(connectionToClient);
                playerCore.playerTurn.OnTurnDone();
            }
        }
        else
        {
            if (discardCount > 0)
                ServerDiscardCards(discardCount);
            else
            {
                isDiscarding = false;
                TargetHideDiscardPanel(connectionToClient);
            }
        }
    }
    // 显示弃牌UI
    [TargetRpc]
    private void TargetShowDiscardUI(NetworkConnection target, int discardCount)
    {
        PlayerUI.LocalInstance.ShowDiscardPanel(discardCount);
    }
    // 隐藏弃牌UI
    [TargetRpc]
    private void TargetHideDiscardPanel(NetworkConnection target)
    {
        PlayerUI.LocalInstance.HideDiscardPanel();
    }
}