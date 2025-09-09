// �ͻ���������ʾUI
using TMPro;
using Mirror;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerUI : NetworkBehaviour
{
    public static PlayerUI LocalInstance { get; private set; }
    public GameObject cardPrefab;
    public Transform handParent;
    public FanLayout fanLayout;
    public PlayerCore playerCore;
    [Header("�ű����")]
    public CardChoicePanelUI cardChoicePanelUI;     // Ч��ѡ��UI
    public PlayerChoicePanelUI playerChoicePanelUI; // ���ѡ��UI
    public PlayerCardDescribe playerCardDescribe;   // ��������UI
    public SelectStepsUI selectStepsUI;             // ����ѡ��UI
    public RequestBattleUI requestBattleUI;         // ����ս��UI
    public BattleCardUI battleCardUI;               // ս������UI
    [Header("������ʾ")]
    public Image[] pointIndicators;    // �������������Image���
    public float blinkSpeed = 2f;      // ��˸�ٶ�
    private int currentPoints = 3;     // ��ǰʣ�����
    [Header("����UI")]
    public GameObject selectPlayerUI;  // ѡ��Ŀ�����
    public GameObject shopUI;          // �̵�UI
    public GameObject discardPanel;    // �������
    public GameObject choicePanelUI;   // Ч��ѡ��
    public GameObject cardDescribeUI;  // ��������UI
    [Header("UI���")]
    public TextMeshProUGUI discardPrompt;           // ������ʾ�ı�
    [Header("��ʱ����")]
    [SyncVar] public int choice = 0;                // ѡ���Ч��
    [SyncVar] public string handlingCardName;
    [SyncVar] public int count = 0;

    private List<GameObject> currentCards = new List<GameObject>();// ��ǰ�����б�

    private void Awake()
    {
        playerCore = GetComponent<PlayerCore>();
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (isLocalPlayer)
            LocalInstance = this;
    }
    //----------------------------�������------------------------------------------------------
    // ��UI���������
    public void AddCardToUI(CardData data)
    {
        GameObject cardObj = Instantiate(cardPrefab, handParent);
        cardObj.GetComponent<CardUI>().Initialize(data);
        currentCards.Add(cardObj);
        fanLayout.AddCard(cardObj.transform);
    }
    // ��UI���Ƴ�����
    public void RemoveCardFromUI(int index)
    {
        Transform cardObj = currentCards[index].transform;
        currentCards.RemoveAt(index);
        fanLayout.RemoveCard(cardObj);
        Destroy(cardObj.gameObject);
        CloseCardInfo();
    }
    public void ShowCardInfo(CardData card)
    {
        cardDescribeUI.SetActive(true);
        playerCardDescribe.SetDescribe(card);
    }
    public void CloseCardInfo()
    {
        cardDescribeUI.SetActive(false);
    }
    // �����ڵ�����ʾ��������
    public void UpdateCardCount(int count)
    {
        Debug.Log($"��ǰ������: {count}");
    }
    // �ͻ��˵�����ƺ���÷���˷���
    public void OnCardClicked(int index, CardType type)
    {
        playerCore.playerHand.CmdUseCard(index,type);
    }
    // �������ĵ���������Ӧ�ķ���
    public void HighlightCostPoints(int cost)
    {
        if (playerCore.playerHand.isDiscarding) return;
        int startIndex = Mathf.Max(currentPoints - cost, 0);
        int endIndex = currentPoints - 1;
        for (int i = startIndex; i <= endIndex; i++)
        {
            StartCoroutine(BlinkIndicator(pointIndicators[i]));
        }
    }
    // �����������˸Э��
    private IEnumerator BlinkIndicator(Image indicator)
    {
        float alpha = 1f;
        bool isFadingOut = true;
        while (true)
        {
            alpha += (isFadingOut ? -1 : 1) * blinkSpeed * Time.deltaTime;
            alpha = Mathf.Clamp01(alpha);
            indicator.color = new Color(1, 1, 1, alpha);
            if (alpha <= 0 || alpha >= 1) isFadingOut = !isFadingOut;
            yield return null;
        }
    }
    // �������з���ĸ���״̬
    public void ResetPointsHighlight()
    {
        StopAllCoroutines(); // ֹͣ������˸
        for (int i = 0; i < pointIndicators.Length; i++)
        {
            pointIndicators[i].color = new Color(1, 1, 1, 1); // �ָ���͸��
            pointIndicators[i].gameObject.SetActive(i < currentPoints); // ͬ���ɼ���
        }
    }
    // ���µ�����ʾ
    public void UpdatePoints(int newPoints)
    {
        currentPoints = newPoints;
        for (int i = 0; i < pointIndicators.Length; i++)
        {
            pointIndicators[i].gameObject.SetActive(i < newPoints);
        }
    }
    //-----------------------------��Ҷ���ѡ��-------------------------------------------------------
    [TargetRpc]
    public void TargetShowPlayerChoice(string mode, string cardName, int searchSteps, bool canSelectSelf)
    {
        handlingCardName = cardName;
        switch (mode)
        {
            // ���Σ�PlayerCore player, bool canSelectSelf, Action[] callbacks��searchSteps
            case "teleport":
                playerChoicePanelUI.SetOptions(
                "��ѡ��Ҫ���͵����",
                playerCore,
                canSelectSelf,
                new System.Action[]
                    {
                    () => TileManager.Instance.CmdHandleTeleport(playerCore,0),
                    () => TileManager.Instance.CmdHandleTeleport(playerCore,1),
                    () => TileManager.Instance.CmdHandleTeleport(playerCore,2),
                    () => TileManager.Instance.CmdHandleTeleport(playerCore,3),
                    () => TileManager.Instance.CmdHandleTeleport(playerCore,-1)
                    }
                );
                break;
            case "damage":
                playerChoicePanelUI.SetOptions(
                "��ѡ��Ҫ�ڻ������",
                playerCore,
                canSelectSelf,
                new System.Action[]
                    {
                    () => TileManager.Instance.CmdHandleDamage(playerCore,0,2),
                    () => TileManager.Instance.CmdHandleDamage(playerCore,1,2),
                    () => TileManager.Instance.CmdHandleDamage(playerCore,2,2),
                    () => TileManager.Instance.CmdHandleDamage(playerCore,3,2),
                    () => TileManager.Instance.CmdHandleDamage(playerCore,-1,0)
                    }
                );
                break;
            case "effect":
                playerChoicePanelUI.SetOptions(
                    "��ѡ��Ҫ���õ����",
                    playerCore,
                    canSelectSelf,
                    new System.Action[]
                    {
                    () => HandleCardTarget(0),
                    () => HandleCardTarget(1),
                    () => HandleCardTarget(2),
                    () => HandleCardTarget(3),
                    () => ClosePlayerSelectUI()
                    },
                    searchSteps
                );
                break;
            default:
                Debug.LogError($"unkonwn choice mode: {mode}");
                ClosePlayerSelectUI();
                break;
        }
    }
    public void ClosePlayerSelectUI()
    {
        selectPlayerUI.SetActive(false);
    }
    //-----------------------ѡ������Ŀ��ƴ���--------------------------
    [Command]
    private void HandleCardTarget(int index)
    {
        PlayerCore targetPlayer = TurnManager.Instance.GetPlayerByIndex(index);
        switch (handlingCardName)
        {
            case "EXsword":
                CardEffectManager.Instance.EXsword(playerCore, targetPlayer); 
                break;
            case "laser":
                CardEffectManager.Instance.laser(playerCore, targetPlayer);
                break;
            default:
                Debug.LogError($"unkonwn card to handle��{handlingCardName}");
                break;
        }
    }
    //=======================�����¼�==============================
    public void ShowDiscardPanel(int discardCount)
    {
        discardPanel.SetActive(true);
        discardPrompt.text = $"������{discardCount}����";
    }
    public void HideDiscardPanel()
    {
        discardPanel.SetActive(false);
    }
    // ���Ƶ���¼��󶨵���������
    public void OnCardClickedForDiscard(int index)
    {
        playerCore.playerHand.CmdDiscardCard(index);
    }
    //-------------------------------����Ч��ѡ��--------------------------------------------
    [TargetRpc]
    public void TargetShowCardChoice(CardData card)
    {
        handlingCardName = /*"heal1";*/card.cardName;
        switch (handlingCardName)
        {
            case "shop":
                cardChoicePanelUI.SetOptions(
                    playerCore,
                    "ѡ��Ҫ���õ�Ч��",
                    new[] { "ÿ֧��2������1�ſ�(����4)", "ÿ����1�ſ��ظ�1����(����4)" },
                    new System.Action[]
                    {
                    () => SwitchChoice(1,cardChoicePanelUI.inputValue),
                    () => SwitchChoice(2,cardChoicePanelUI.inputValue)
                    },
                    true,
                    "��������",
                    Mathf.Min(playerCore.playerData.hp / 2, 4),
                    0
                );
                break;
            case "heal1":
                cardChoicePanelUI.SetOptions(
                    playerCore,
                    "ѡ��Ҫ���õ�Ч��",
                    new[] { "�ظ�2HP", "�ظ�2+�������ĵ���HP" },
                    new System.Action[]
                    {
                    () => SwitchChoice(1),
                    () => SwitchChoice(2,cardChoicePanelUI.inputValue)
                    },
                    true,
                    "��������",
                    currentPoints - 1,
                    0
                );
                break;
            case "nvwu":
                cardChoicePanelUI.SetOptions(
                    playerCore,
                    "ѡ��Ҫ���õ�Ч��",
                    new[] { "ÿ�غ��ܵ�1�˺�������3�غ�", "ÿ�غϻظ�2HP������2�غ�" },
                    new System.Action[]
                    {
                    () => SwitchChoice(1),
                    () => SwitchChoice(2)
                    },
                    false
                );
                break;
        }
    }
    public void SwitchChoice(int newChoice, int newCount = 0)
    {
        choice = newChoice;
        count = newCount;
        if (handlingCardName == "shop")
            cardChoicePanelUI.ResetInput(choice);
    }
    public void ResolveCardChoice()
    {
        Debug.Log($"{playerCore.playerData.playerName}ѡ����Ч��{choice}");
        CmdResolveCardChoice(handlingCardName, choice, count);
    }
    [Command]
    public void CmdResolveCardChoice(string handingName, int choice, int count)
    {
        handlingCardName = handingName;
        Debug.Log($"handling card:{handlingCardName},choice{choice},count{count}");
        if (choice == 0)
        {
            TargetCloseChoiceUI(connectionToClient);
            return;
        }
        switch (handlingCardName)
        {
            case "shop":
                CardEffectManager.Instance.shop(playerCore, choice, count);
                break;
            case "heal1":
                CardEffectManager.Instance.heal1(playerCore, choice, count);
                break;
            case "nvwu":
                CardEffectManager.Instance.nvwu(playerCore, choice);
                break;
        }
        TargetCloseChoiceUI(connectionToClient);
    }
    [TargetRpc]
    public void TargetCloseChoiceUI(NetworkConnection target)
    {
        cardChoicePanelUI.ClosePanel();
    }
}