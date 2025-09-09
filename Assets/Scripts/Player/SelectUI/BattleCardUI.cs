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

    public Image[] pointIndicators;    // �������������Image���
    public Sprite atkIcon;             // ������������ͼ
    public Sprite defIcon;             // ������������ͼ
    public float blinkSpeed = 2f;      // ��˸�ٶ�
    public int battlePoints = 3;       // ��ǰʣ�����
    public Button readyButton;         // ׼����ť
    public TextMeshProUGUI buttonText; // ��ť����
    private bool isAttacker;           // �Ƿ��ǹ�����

    private void Awake()
    {
        playerCore = GetComponentInParent<PlayerCore>();
        // [ս��]����׼����ϰ�ť
        readyButton.onClick.RemoveAllListeners();
        readyButton.onClick.AddListener(() =>
        {
            SetReady();
        });
    }
    // [ս��]����ս�����ɻ���������������
    public void ShowCardSelect(bool isAttacker)
    {
        this.isAttacker = isAttacker;
        gameObject.SetActive(true);
        var type = isAttacker ? CardType.Attack : CardType.Defense;
        var cards = fanLayout.cards;
        Sprite iconSprite = isAttacker ? atkIcon : defIcon;
        // �޸ĵ���ͼ��
        foreach (var indicator in pointIndicators)
        {
            indicator.sprite = iconSprite;
        }
        // ���ÿ��ƿɷ���
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
    // �������ĵ���������Ӧ�ķ���
    public void HighlightCostPoints(int cost)
    {
        int startIndex = Mathf.Max(battlePoints - cost, 0);
        int endIndex = battlePoints - 1;
        for (int i = startIndex; i <= endIndex; i++)
        {
            StartCoroutine(BlinkIndicator(pointIndicators[i]));
        }
    }
    // ���������������˸Э��
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
    // �������з���ĸ���״̬
    public void ResetPointsHighlight()
    {
        StopAllCoroutines(); // ֹͣ������˸
        for (int i = 0; i < pointIndicators.Length; i++)
        {
            pointIndicators[i].color = new Color(1, 1, 1, 1); // �ָ���͸��
            pointIndicators[i].gameObject.SetActive(i < battlePoints); // ͬ���ɼ���
        }
    }
    // ���µ�����ʾ
    public void UpdatePoints(int newPoints)
    {
        battlePoints = newPoints;
        for (int i = 0; i < pointIndicators.Length; i++)
        {
            pointIndicators[i].gameObject.SetActive(i < newPoints);
        }
    }
    // ׼��
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
    // ����
    private void SetDiceRoll()
    {
        // ����1-6���������Ϊ������
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
    // ����ť����Ϊ����ģʽ
    public void SwitchDiceButton()
    {
        Debug.Log("switching...");
        readyButton.interactable = true;
        Debug.Log("set button true");
        buttonText.text = "Ͷ��";
        readyButton.onClick.RemoveAllListeners();
        readyButton.onClick.AddListener(() =>
        {
            SetDiceRoll();
        });
    }
    // ����ť����Ϊ׼��ģʽ
    public void SwitchReadyButton()
    {
        readyButton.interactable = true;
        buttonText.text = "׼��";
        readyButton.onClick.RemoveAllListeners();
        readyButton.onClick.AddListener(() =>
        {
            SetReady();
        });
    }
    public void ShowDefChoice()
    {
        // ��ʾ����ѡ��
        // ����ѡ��ص�
    }
}
