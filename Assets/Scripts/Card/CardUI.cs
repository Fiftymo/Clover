// 卡牌UI,点击
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour
{
    public Image cardImage;
    private int handIndex;
    public Vector3 originalPos;     // 原始位置
    public Quaternion originalRot;  // 原始旋转
    public GameObject blockPanel;
    [Header("卡牌信息")]
    private CardData cardData;
    public TextMeshPro costText;    // UI文本显示消耗点数
   // public GameObject glowEffect; // 高光特效
    private int requiredCost;       // 当前卡牌需要的点数
    private CardType cardType;
    public bool isAnimating = true; // 默认在初始化时为动画中
    public CardType typeCanUse;       // 当前卡牌可操作的技能类型

    private void Awake()
    {
        typeCanUse = CardType.Effect;
    }

    public void UpdateBlock()
    {
        blockPanel.SetActive(cardType != typeCanUse);
    }

    public void SetUsableType(CardType usableType)
    {
        typeCanUse = usableType;
        UpdateBlock();
    }

    public void UpdateOriginalPosition()
    {
        originalPos = transform.localPosition;
        originalRot = transform.localRotation;
    }

    public void OnMouseEnter()
    {
        if (isAnimating) return;
        // 悬停时放大并抬升
        transform.localPosition = originalPos + Vector3.up * 30;
        transform.localScale = Vector3.one * 1.2f;
        if (PlayerUI.LocalInstance != null)
        {
            PlayerUI.LocalInstance.ShowCardInfo(cardData);   // 显示卡牌信息，可加选项禁用
            if (cardType == CardType.Effect)
                PlayerUI.LocalInstance.HighlightCostPoints(requiredCost);
            else if (cardType == CardType.Attack)
                PlayerUI.LocalInstance.battleCardUI.HighlightCostPoints(requiredCost);
            else if (cardType == CardType.Defense)
                PlayerUI.LocalInstance.battleCardUI.HighlightCostPoints(requiredCost);
        }
    }

    public void OnMouseExit()
    {
        if (isAnimating) return;
        // 恢复原始状态
        transform.localPosition = originalPos;
        transform.localScale = Vector3.one;
        if (PlayerUI.LocalInstance != null)
        {
            PlayerUI.LocalInstance.ResetPointsHighlight();
            PlayerUI.LocalInstance.CloseCardInfo();
        }
    }

    public void Initialize(CardData data)
    {
        cardData = data;
        Sprite sprite = data.GetSprite();
        requiredCost = data.cost;
        cardType = data.type;
        if (sprite != null)
            cardImage.sprite = sprite;
        else
            Debug.LogError($"未找到卡牌贴图: {data.spriteName}");
    }

    public void OnClick()
    {
        if (cardType != typeCanUse) return;
        if (PlayerUI.LocalInstance.playerCore.playerHand.isDiscarding) return;
        if (PlayerUI.LocalInstance != null && PlayerUI.LocalInstance.playerCore.isLocalPlayer)
            PlayerUI.LocalInstance.OnCardClicked(transform.GetSiblingIndex(),cardType);
    }
    public void OnDiscardClick()
    {
        if (!PlayerUI.LocalInstance.playerCore.playerHand.isDiscarding) return;
        if (PlayerUI.LocalInstance != null && PlayerUI.LocalInstance.playerCore.isLocalPlayer)
            PlayerUI.LocalInstance.OnCardClickedForDiscard(transform.GetSiblingIndex());
    }
}