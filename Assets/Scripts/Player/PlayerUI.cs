// 客户端手牌显示UI
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
    [Header("脚本组件")]
    public CardChoicePanelUI cardChoicePanelUI;     // 效果选择UI
    public PlayerChoicePanelUI playerChoicePanelUI; // 玩家选择UI
    public PlayerCardDescribe playerCardDescribe;   // 卡牌描述UI
    public SelectStepsUI selectStepsUI;             // 步数选择UI
    public RequestBattleUI requestBattleUI;         // 请求战斗UI
    public BattleCardUI battleCardUI;               // 战斗出牌UI
    [Header("点数显示")]
    public Image[] pointIndicators;    // 三个点数方块的Image组件
    public float blinkSpeed = 2f;      // 闪烁速度
    private int currentPoints = 3;     // 当前剩余点数
    [Header("个人UI")]
    public GameObject selectPlayerUI;  // 选择目标玩家
    public GameObject shopUI;          // 商店UI
    public GameObject discardPanel;    // 弃牌面板
    public GameObject choicePanelUI;   // 效果选择
    public GameObject cardDescribeUI;  // 卡牌描述UI
    [Header("UI组件")]
    public TextMeshProUGUI discardPrompt;           // 弃牌提示文本
    [Header("临时属性")]
    [SyncVar] public int choice = 0;                // 选择的效果
    [SyncVar] public string handlingCardName;
    [SyncVar] public int count = 0;

    private List<GameObject> currentCards = new List<GameObject>();// 当前手牌列表

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
    //----------------------------卡牌相关------------------------------------------------------
    // 在UI中添加手牌
    public void AddCardToUI(CardData data)
    {
        GameObject cardObj = Instantiate(cardPrefab, handParent);
        cardObj.GetComponent<CardUI>().Initialize(data);
        currentCards.Add(cardObj);
        fanLayout.AddCard(cardObj.transform);
    }
    // 从UI中移除手牌
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
    // 仅用于调试显示手牌数量
    public void UpdateCardCount(int count)
    {
        Debug.Log($"当前手牌数: {count}");
    }
    // 客户端点击卡牌后调用服务端方法
    public void OnCardClicked(int index, CardType type)
    {
        playerCore.playerHand.CmdUseCard(index,type);
    }
    // 根据消耗点数高亮对应的方块
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
    // 单个方块的闪烁协程
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
    // 重置所有方块的高亮状态
    public void ResetPointsHighlight()
    {
        StopAllCoroutines(); // 停止所有闪烁
        for (int i = 0; i < pointIndicators.Length; i++)
        {
            pointIndicators[i].color = new Color(1, 1, 1, 1); // 恢复不透明
            pointIndicators[i].gameObject.SetActive(i < currentPoints); // 同步可见性
        }
    }
    // 更新点数显示
    public void UpdatePoints(int newPoints)
    {
        currentPoints = newPoints;
        for (int i = 0; i < pointIndicators.Length; i++)
        {
            pointIndicators[i].gameObject.SetActive(i < newPoints);
        }
    }
    //-----------------------------玩家对象选择-------------------------------------------------------
    [TargetRpc]
    public void TargetShowPlayerChoice(string mode, string cardName, int searchSteps, bool canSelectSelf)
    {
        handlingCardName = cardName;
        switch (mode)
        {
            // 传参：PlayerCore player, bool canSelectSelf, Action[] callbacks，searchSteps
            case "teleport":
                playerChoicePanelUI.SetOptions(
                "请选择要传送的玩家",
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
                "请选择要炮击的玩家",
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
                    "请选择要适用的玩家",
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
    //-----------------------选择对象后的卡牌处理--------------------------
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
                Debug.LogError($"unkonwn card to handle：{handlingCardName}");
                break;
        }
    }
    //=======================弃牌事件==============================
    public void ShowDiscardPanel(int discardCount)
    {
        discardPanel.SetActive(true);
        discardPrompt.text = $"请弃掉{discardCount}张牌";
    }
    public void HideDiscardPanel()
    {
        discardPanel.SetActive(false);
    }
    // 卡牌点击事件绑定到弃牌命令
    public void OnCardClickedForDiscard(int index)
    {
        playerCore.playerHand.CmdDiscardCard(index);
    }
    //-------------------------------卡牌效果选择--------------------------------------------
    [TargetRpc]
    public void TargetShowCardChoice(CardData card)
    {
        handlingCardName = /*"heal1";*/card.cardName;
        switch (handlingCardName)
        {
            case "shop":
                cardChoicePanelUI.SetOptions(
                    playerCore,
                    "选择要适用的效果",
                    new[] { "每支付2生命抽1张卡(上限4)", "每丢弃1张卡回复1生命(上限4)" },
                    new System.Action[]
                    {
                    () => SwitchChoice(1,cardChoicePanelUI.inputValue),
                    () => SwitchChoice(2,cardChoicePanelUI.inputValue)
                    },
                    true,
                    "卡的张数",
                    Mathf.Min(playerCore.playerData.hp / 2, 4),
                    0
                );
                break;
            case "heal1":
                cardChoicePanelUI.SetOptions(
                    playerCore,
                    "选择要适用的效果",
                    new[] { "回复2HP", "回复2+额外消耗点数HP" },
                    new System.Action[]
                    {
                    () => SwitchChoice(1),
                    () => SwitchChoice(2,cardChoicePanelUI.inputValue)
                    },
                    true,
                    "额外消耗",
                    currentPoints - 1,
                    0
                );
                break;
            case "nvwu":
                cardChoicePanelUI.SetOptions(
                    playerCore,
                    "选择要适用的效果",
                    new[] { "每回合受到1伤害，持续3回合", "每回合回复2HP，持续2回合" },
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
        Debug.Log($"{playerCore.playerData.playerName}选择了效果{choice}");
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