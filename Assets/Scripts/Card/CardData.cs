// 卡牌数据
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Card", menuName = "Card System/Card")]
public class CardData : ScriptableObject
{
    public string cardName;               // 卡牌名称
    public CardType type;                 // 卡牌类型
    public string spriteName;             // 卡图名称
    public string displayName;            // 展示名称
    [TextArea] public string description; // 卡牌描述
    public int cost;                      // 卡牌消耗

    // 获取卡牌图片
    public Sprite GetSprite()
    {
        return Resources.Load<Sprite>($"CardSprites/{type}/{spriteName}");
    }
}

public enum CardType { Attack, Defense, Effect, Counter, None }   // 卡牌类型

[System.Serializable]
public class CardConfig
{
    public CardData card;               // 卡牌资源
    [Range(0, 1)] public float weight;  // 生成权重（相对同类型其他卡牌）
    public int minCount;                // 本卡牌最少生成数量
    public int maxCount;                // 本卡牌最多生成数量
}

[System.Serializable]
public class CardTypeConfig
{
    public CardType type;          // 卡牌类型
    public List<CardConfig> cards; // 该类型下的卡牌配置
    public int typeMinCount;       // 该类型总生成最少数量
    public int typeMaxCount;       // 该类型总生成最多数量
}