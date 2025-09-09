// LogManager.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LogManager : MonoBehaviour
{
    public static LogManager Instance;

    [Header("UI���")]
    public Transform logContent;  // ScrollView��Content����
    public GameObject logPrefab;  // ��־��ĿԤ����
    public ScrollRect scrollRect;  // ScrollView����

    [Header("����")]
    public int maxLogs = 20;      // �����ʾ��־��

    private Queue<GameObject> logQueue = new Queue<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    // ���һ����־
    public void AddLog(string message, Color color)
    {
        // ��������Ŀ
        GameObject newLog = Instantiate(logPrefab, logContent);
        TextMeshProUGUI logText = newLog.GetComponent<TextMeshProUGUI>();
        logText.text = message;
        logText.color = color;

        // �������
        logQueue.Enqueue(newLog);
        if (logQueue.Count > maxLogs)
        {
            Destroy(logQueue.Dequeue());
        }
        // �Զ��������ײ�
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0; // 0��ʾ�ײ�
        }
    }
    //---------------------------------------------------------------------------------//
    public void LogTurnStart(int turn, int coin)
    {
        if (coin > 0)
        {
            string message = $"��{turn}�غϿ�ʼ,����{coin}���";
            AddLog(message, Color.white);
        }
        else
        {
            string message = $"��{turn}�غϿ�ʼ";
            AddLog(message, Color.white);
        }
    }
    public void LogHome(PlayerCore player, bool isLvUP)
    {
        string message = $"{player.playerData.playerName}����Hmoe,�ظ�2HP";
        AddLog(message, Color.white);
        if (isLvUP)
        {
            string message2 = $"{player.playerData.playerName}������";
            AddLog(message2, Color.white);
        }
    }
    public void LogBonus(PlayerCore player, int coin)
    {
        string message = $"{player.playerData.playerName}����Bonus,���{coin}���";
        AddLog(message, Color.white);
    }
    public void LogMove(PlayerCore player)
    {
        string message = $"{player.playerData.playerName}�ٴ��ƶ�";
        AddLog(message, Color.white);
    }
    public void LogDraw(PlayerCore player, int cardCount)
    {
        string message = $"{player.playerData.playerName}�鵽{cardCount}����";
        AddLog(message, Color.white);
    }
    public void LogTeleport(PlayerCore player, PlayerCore target)
    {
        string message = $"{player.playerData.playerName}���͵�{target.playerData.playerName}��";
        AddLog(message, Color.white);
    }
    public void LogCauseDamage(PlayerCore attacker, PlayerCore target, int damage)
    {
        string message = $"{attacker.playerData.playerName}��{target.playerData.playerName}���{damage}���˺�";
        AddLog(message, Color.red);
    }
    public void LogSelfDamage(PlayerCore player, int damage)
    {
        string message = $"{player.playerData.playerName}�ܵ�{damage}���˺�";
        AddLog(message, Color.red);
    }
    public void LogPlayerDead(PlayerCore player)
    {
        string message = $"{player.playerData.playerName}������";
        AddLog(message, Color.yellow);
    }
    public void cardApply(PlayerData player, int duration, string cardName)
    {
        string message = $"[Ч��]{player.playerName}��{cardName}Ч������{duration}�غ�";
        AddLog(message, Color.white);
    }
    public void cardAdd(PlayerData player, int duration, string cardName)
    {
        string message = $"[Ч��]{player.playerName}�����[{cardName}]Ч��������{duration}�غ�";
        AddLog(message, Color.white);
    }
    public void cardExpire(PlayerData player, string cardName)
    {
        string message = $"[Ч��]{player.playerName}��[{cardName}]Ч���ѽ���";
        AddLog(message, Color.white);
    }
}