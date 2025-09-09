// ����UI,���
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour
{
    public Image cardImage;
    private int handIndex;
    public Vector3 originalPos;     // ԭʼλ��
    public Quaternion originalRot;  // ԭʼ��ת
    public GameObject blockPanel;
    [Header("������Ϣ")]
    private CardData cardData;
    public TextMeshPro costText;    // UI�ı���ʾ���ĵ���
   // public GameObject glowEffect; // �߹���Ч
    private int requiredCost;       // ��ǰ������Ҫ�ĵ���
    private CardType cardType;
    public bool isAnimating = true; // Ĭ���ڳ�ʼ��ʱΪ������
    public CardType typeCanUse;       // ��ǰ���ƿɲ����ļ�������

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
        // ��ͣʱ�Ŵ�̧��
        transform.localPosition = originalPos + Vector3.up * 30;
        transform.localScale = Vector3.one * 1.2f;
        if (PlayerUI.LocalInstance != null)
        {
            PlayerUI.LocalInstance.ShowCardInfo(cardData);   // ��ʾ������Ϣ���ɼ�ѡ�����
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
        // �ָ�ԭʼ״̬
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
            Debug.LogError($"δ�ҵ�������ͼ: {data.spriteName}");
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