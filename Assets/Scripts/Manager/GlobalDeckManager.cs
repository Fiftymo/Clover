// 卡池管理器
using System.Collections.Generic;
using Mirror;
using System.Linq;
using UnityEngine;

public class GlobalDeckManager : NetworkBehaviour
{
    public static GlobalDeckManager Instance;

    [Header("卡牌配置")]
    public List<CardTypeConfig> typeConfigs = new List<CardTypeConfig>();

    [SyncVar]
    private List<CardData> deck = new List<CardData>();        // 公共牌库
    private List<CardData> discardPile = new List<CardData>(); // 弃牌堆

    void Awake() => Instance = this;

    // 初始化牌库
    [Server]
    public void InitializeDeck()
    {
        Debug.Log("[DeckManager] InitializeDeck...");
        deck.Clear();
        foreach (var typeConfig in typeConfigs)
        {
            // 1. 计算该类型总生成数量
            int typeCount = Random.Range(
                typeConfig.typeMinCount,
                typeConfig.typeMaxCount + 1
            );
            // 2. 按权重分配各卡牌生成数量
            var weightedCards = typeConfig.cards
                .Where(c => c.weight > 0)
                .ToList();
            // 计算总权重
            float totalWeight = weightedCards.Sum(c => c.weight);
            for (int i = 0; i < typeCount; i++)
            {
                // 按权重随机选择卡牌
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
    // 生成限制
    [Server]
    private void AddCardWithLimits(CardConfig config)
    {
        // 检查当前已生成数量是否超过限制
        int currentCount = deck.Count(c => c == config.card);
        if (currentCount >= config.maxCount) return;
        if (currentCount < config.minCount)
        {
            // 必须满足最小数量
            deck.Add(config.card);
            return;
        }

        // 概率性添加（权重生效区）
        deck.Add(config.card);
    }
    // 洗牌
    [Server]
    private void ShuffleDeck() => deck = deck.OrderBy(x => Random.value).ToList();
    // 从牌库抽一张
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