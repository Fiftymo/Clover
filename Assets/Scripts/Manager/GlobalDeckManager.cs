// ���ع�����
using System.Collections.Generic;
using Mirror;
using System.Linq;
using UnityEngine;

public class GlobalDeckManager : NetworkBehaviour
{
    public static GlobalDeckManager Instance;

    [Header("��������")]
    public List<CardTypeConfig> typeConfigs = new List<CardTypeConfig>();

    [SyncVar]
    private List<CardData> deck = new List<CardData>();        // �����ƿ�
    private List<CardData> discardPile = new List<CardData>(); // ���ƶ�

    void Awake() => Instance = this;

    // ��ʼ���ƿ�
    [Server]
    public void InitializeDeck()
    {
        Debug.Log("[DeckManager] InitializeDeck...");
        deck.Clear();
        foreach (var typeConfig in typeConfigs)
        {
            // 1. �������������������
            int typeCount = Random.Range(
                typeConfig.typeMinCount,
                typeConfig.typeMaxCount + 1
            );
            // 2. ��Ȩ�ط����������������
            var weightedCards = typeConfig.cards
                .Where(c => c.weight > 0)
                .ToList();
            // ������Ȩ��
            float totalWeight = weightedCards.Sum(c => c.weight);
            for (int i = 0; i < typeCount; i++)
            {
                // ��Ȩ�����ѡ����
                float randomPoint = Random.value * totalWeight;
                foreach (var cardConfig in weightedCards)
                {
                    if (randomPoint < cardConfig.weight)
                    {
                        AddCardWithLimits(cardConfig);
                        break;
                    }
                    randomPoint -= cardConfig.weight;
                }
            }
        }
        ShuffleDeck();
        Debug.Log($"[DeckManager] Total card in deck: {deck.Count}");
    }
    // ��������
    [Server]
    private void AddCardWithLimits(CardConfig config)
    {
        // ��鵱ǰ�����������Ƿ񳬹�����
        int currentCount = deck.Count(c => c == config.card);
        if (currentCount >= config.maxCount) return;
        if (currentCount < config.minCount)
        {
            // ����������С����
            deck.Add(config.card);
            return;
        }

        // ��������ӣ�Ȩ����Ч����
        deck.Add(config.card);
    }
    // ϴ��
    [Server]
    private void ShuffleDeck() => deck = deck.OrderBy(x => Random.value).ToList();
    // ���ƿ��һ��
    [Server]
    public CardData DrawCard()
    {
        Debug.Log("[DeckManager] DrawCard...");
        if (deck.Count == 0) return null;
        CardData card = deck[0];
        deck.RemoveAt(0);
        return card;
    }
}