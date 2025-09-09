using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FanLayout : MonoBehaviour
{
    public float radius = 500f;        // ���ΰ뾶
    public float angleStep = 40f;      // ���ڿ��ƽǶȼ��
    public float verticalOffset = 20f; // ����㼶ƫ��
    public GameObject blockPanel;

    public List<Transform> cards = new List<Transform>();


    void Update()
    {
        // ������Ļ��ȵ����뾶
        //radius = Screen.width * 0.2f;
    }

    public void UpdateLayout()
    {
        // ������ʼ�Ƕȣ����жԳƣ�
        float startAngle = -angleStep * (cards.Count - 1) / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            Transform card = cards[i];
            // ����Ƕ�
            float angle = startAngle + i * angleStep;
            // ������ת�ѿ�������
            Vector3 pos = new Vector3(
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                -i * verticalOffset // Z��㼶ƫ��
            );

            // ����λ�ú���ת
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
    // ��ӿ��Ƶ�����
    public void AddCard(Transform card)
    {
        var cardUI = card.GetComponent<CardUI>();
        cardUI.blockPanel.SetActive(true);
        cards.Add(card);
        UpdateLayout();
    }

    // �Ƴ�����
    public void RemoveCard(Transform card)
    {
        cards.Remove(card);
        UpdateLayout();
    }
}