using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FanLayout : MonoBehaviour
{
    public float radius = 500f;        // 扇形半径
    public float angleStep = 40f;      // 相邻卡牌角度间隔
    public float verticalOffset = 20f; // 纵向层级偏移
    public GameObject blockPanel;

    public List<Transform> cards = new List<Transform>();


    void Update()
    {
        // 根据屏幕宽度调整半径
        //radius = Screen.width * 0.2f;
    }

    public void UpdateLayout()
    {
        // 计算起始角度（居中对称）
        float startAngle = -angleStep * (cards.Count - 1) / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            Transform card = cards[i];
            // 计算角度
            float angle = startAngle + i * angleStep;
            // 极坐标转笛卡尔坐标
            Vector3 pos = new Vector3(
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                -i * verticalOffset // Z轴层级偏移
            );

            // 设置位置和旋转
            card.localPosition = pos;
            card.localRotation = Quaternion.Euler(0, 0, -angle);
            CardUI cardUI = card.GetComponent<CardUI>();
            if (cardUI != null)
            {
                cardUI.UpdateOriginalPosition();
            }
            StartCoroutine(UnlockAnimation(card));
        }
    }
    private IEnumerator UnlockAnimation(Transform card)
    {
        var cardUI = card.GetComponent<CardUI>();
        yield return new WaitForSeconds(0.5f);
        cardUI.isAnimating = false;
        cardUI.UpdateBlock();
    }
    // 添加卡牌到布局
    public void AddCard(Transform card)
    {
        var cardUI = card.GetComponent<CardUI>();
        cardUI.blockPanel.SetActive(true);
        cards.Add(card);
        UpdateLayout();
    }

    // 移除卡牌
    public void RemoveCard(Transform card)
    {
        cards.Remove(card);
        UpdateLayout();
    }
}