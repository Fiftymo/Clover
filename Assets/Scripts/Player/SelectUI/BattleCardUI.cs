using TMPro;
using Mirror;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections;

public class BattleCardUI : NetworkBehaviour
{
    public FanLayout fanLayout;
    public PlayerCore playerCore;

    public Image[] pointIndicators;    // 三个点数方块的Image组件
    public Sprite atkIcon;             // 攻击方点数贴图
    public Sprite defIcon;             // 防御方点数贴图
    public float blinkSpeed = 2f;      // 闪烁速度
    public int battlePoints = 3;       // 当前剩余点数
    public Button readyButton;         // 准备按钮
    public TextMeshProUGUI buttonText; // 按钮文字
    private bool isAttacker;           // 是否是攻击方

    private void Awake()
    {
        playerCore = GetComponentInParent<PlayerCore>();
        // [战斗]设置准备完毕按钮
        readyButton.onClick.RemoveAllListeners();
        readyButton.onClick.AddListener(() =>
        {
            SetReady();
        });
    }
    // [战斗]设置战斗卡可互动，禁用其他卡
    public void ShowCardSelect(bool isAttacker)
    {
        this.isAttacker = isAttacker;
        gameObject.SetActive(true);
        var type = isAttacker ? CardType.Attack : CardType.Defense;
        var cards = fanLayout.cards;
        Sprite iconSprite = isAttacker ? atkIcon : defIcon;
        // 修改点数图标
        foreach (var indicator in pointIndicators)
        {
            indicator.sprite = iconSprite;
        }
        // 设置卡牌可否点击
        foreach (Transform card in cards)
        {
            var cardUI = card.GetComponent<CardUI>();
            cardUI.SetUsableType(type);
        }
    }
    public void SetCardInteractable(CardType cardType)
    {
        var cards = fanLayout.cards;
        foreach (Transform card in cards)
        {
            var cardUI = card.GetComponent<CardUI>();
            cardUI.SetUsableType(cardType);
        }
    }
    // 根据消耗点数高亮对应的方块
    public void HighlightCostPoints(int cost)
    {
        int startIndex = Mathf.Max(battlePoints - cost, 0);
        int endIndex = battlePoints - 1;
        for (int i = startIndex; i <= endIndex; i++)
        {
            StartCoroutine(BlinkIndicator(pointIndicators[i]));
        }
    }
    // 单个点数方块的闪烁协程
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
            pointIndicators[i].gameObject.SetActive(i < battlePoints); // 同步可见性
        }
    }
    // 更新点数显示
    public void UpdatePoints(int newPoints)
    {
        battlePoints = newPoints;
        for (int i = 0; i < pointIndicators.Length; i++)
        {
            pointIndicators[i].gameObject.SetActive(i < newPoints);
        }
    }
    // 准备
    private void SetReady()
    {
        CmdCheckReady(isAttacker);
        readyButton.interactable = false;
        SetCardInteractable(CardType.None);
    }
    [Command]
    public void CmdCheckReady(bool isAttacker)
    {
        if (playerCore != null)
            Debug.LogError("PlayerCore not null");
        BattleManager.Instance.CheckReady(playerCore,isAttacker);
    }
    // 骰子
    private void SetDiceRoll()
    {
        // 产生1-6的随机数作为攻击力
        int dice = Random.Range(1, 7);
        readyButton.interactable = false;
        CmdSendDice(isAttacker,dice);
    }
    [Command]
    private void CmdSendDice(bool isA, int dice)
    {
        if (isA)
            BattleManager.Instance.AttackerDice(dice);
        else
            BattleManager.Instance.DefenderDice(dice);
    }
    // 将按钮设置为骰子模式
    public void SwitchDiceButton()
    {
        Debug.Log("switching...");
        readyButton.interactable = true;
        Debug.Log("set button true");
        buttonText.text = "投掷";
        readyButton.onClick.RemoveAllListeners();
        readyButton.onClick.AddListener(() =>
        {
            SetDiceRoll();
        });
    }
    // 将按钮设置为准备模式
    public void SwitchReadyButton()
    {
        readyButton.interactable = true;
        buttonText.text = "准备";
        readyButton.onClick.RemoveAllListeners();
        readyButton.onClick.AddListener(() =>
        {
            SetReady();
        });
    }
    public void ShowDefChoice()
    {
        // 显示防御选择
        // 根据选择回调
    }
}
