// ��������
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Card", menuName = "Card System/Card")]
public class CardData : ScriptableObject
{
    public string cardName;               // ��������
    public CardType type;                 // ��������
    public string spriteName;             // ��ͼ����
    public string displayName;            // չʾ����
    [TextArea] public string description; // ��������
    public int cost;                      // ��������

    // ��ȡ����ͼƬ
    public Sprite GetSprite()
    {
        return Resources.Load<Sprite>($"CardSprites/{type}/{spriteName}");
    }
}

public enum CardType { Attack, Defense, Effect, Counter, None }   // ��������

[System.Serializable]
public class CardConfig
{
    public CardData card;               // ������Դ
    [Range(0, 1)] public float weight;  // ����Ȩ�أ����ͬ�����������ƣ�
    public int minCount;                // ������������������
    public int maxCount;                // �����������������
}

[System.Serializable]
public class CardTypeConfig
{
    public CardType type;          // ��������
    public List<CardConfig> cards; // �������µĿ�������
    public int typeMinCount;       // ��������������������
    public int typeMaxCount;       // �������������������
}