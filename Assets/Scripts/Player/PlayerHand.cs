// ������ƹ����鿨��ʹ�õȹ���
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
    public int handCount = 0; // ��ͬ����������
    public int maxHandCount = 6;
    [SyncVar] public bool isDiscarding = false;
    [SyncVar] public bool isForce = false;
    [SyncVar] private int discardCount = 0;
    private int handleCardIndex = -1;

    public List<CardData> hand = new List<CardData>(); // ����˴洢ʵ������
    private PlayerCore playerCore;

    private void Awake()
    {
        playerCore = GetComponent<PlayerCore>(); // ��ȡ���ӵ�ͬһ����� PlayerCore ���
    }
    // ��ʼ���������
    [Server]
    public void ServerInitializeHand()
    {
        ServerDrawCards(4);
    }
    // �鿨
    [Server]
    private void ServerDrawCard()
    {
        CardData card = GlobalDeckManager.Instance.DrawCard();
        if (card != null)
        {
            hand.Add(card);
            handCount++;
            // ͨ�� TargetRpc ֪ͨ��Ӧ�ͻ���
            TargetAddCard(connectionToClient, card.type, card.cardName);
        }
    }
    // �����鿨
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
            yield return new WaitForSeconds(0.1f); // ��ֹ˲��鿨����ͬ������
        }
    }

    [TargetRpc]
    private void TargetAddCard(NetworkConnection target, CardType type, string cardName)
    {
        Debug.Log("�ͻ����յ����ƣ�" + cardName);
        CardData cardData = Resources.Load<CardData>($"Cards/{type}/{cardName}");
        if (cardData != null)
            playerCore.playerUI.AddCardToUI(cardData);
        else
            Debug.LogError("�Ҳ���������Դ��" + cardName);
    }
    // // --------------����-------------
    [Command]
    public void CmdUseCard(int handIndex, CardType type)
    {
        if (handIndex < 0 || handIndex >= hand.Count) return;
        if (playerCore.playerHand.isDiscarding) return;
        if (playerCore.playerMove.isMoving) return;
        // ִ�п���Ч��������չ��
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
                Debug.Log("�������㣬�޷�ʹ�ÿ��ƣ�");
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
                Debug.Log("�������㣬�޷�ʹ�ÿ��ƣ�");
            }
        }
    }
    [Server]
    public void ConfirmUse()
    {
        CardData card = hand[handleCardIndex];
        // �۳�����
        if (card.type == CardType.Effect)
            playerCore.playerData.ChangeCPoints(card.cost);
        else if (card.type == CardType.Attack || card.type == CardType.Defense)
            playerCore.playerData.ChangeBPoints(card.cost);
        // �Ƴ�����
        hand.RemoveAt(handleCardIndex);
        handCount--;
        // ͬ�����ͻ���
        TargetRemoveCardFromUI(connectionToClient, handleCardIndex);
    }
    // �Ƴ�����
    [TargetRpc]
    private void TargetRemoveCardFromUI(NetworkConnection target, int index)
    {
        PlayerUI.LocalInstance.RemoveCardFromUI(index);
    }
    // ��������
    private void OnHandCountUpdated(int oldCount, int newCount)
    {
        if (isLocalPlayer)
            PlayerUI.LocalInstance.UpdateCardCount(newCount);
    }
    //------------------------------------����-------------------------------------------
    [Server]
    public bool CheckHandOverflow()
    {
        return hand.Count > maxHandCount;
    }
    // ǿ������ֱ����������
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
    // --------------����-------------
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
            // ��������Ƿ���Ҫ��������
            if (discardCount > 0)
            {
                ForceDiscard();
            }
            else
            {
                // ������ɺ���������غ�
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
    // ��ʾ����UI
    [TargetRpc]
    private void TargetShowDiscardUI(NetworkConnection target, int discardCount)
    {
        PlayerUI.LocalInstance.ShowDiscardPanel(discardCount);
    }
    // ��������UI
    [TargetRpc]
    private void TargetHideDiscardPanel(NetworkConnection target)
    {
        PlayerUI.LocalInstance.HideDiscardPanel();
    }
}