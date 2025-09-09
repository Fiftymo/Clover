// LogManager.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LogManager : MonoBehaviour
{
    public static LogManager Instance;

    [Header("UI组件")]
    public Transform logContent;  // ScrollView的Content对象
    public GameObject logPrefab;  // 日志条目预制体
    public ScrollRect scrollRect;  // ScrollView对象

    [Header("设置")]
    public int maxLogs = 20;      // 最大显示日志数

    private Queue<GameObject> logQueue = new Queue<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    // 添加一条日志
    public void AddLog(string message, Color color)
    {
        // 创建新条目
        GameObject newLog = Instantiate(logPrefab, logContent);
        TextMeshProUGUI logText = newLog.GetComponent<TextMeshProUGUI>();
        logText.text = message;
        logText.color = color;

        // 管理队列
        logQueue.Enqueue(newLog);
        if (logQueue.Count > maxLogs)
        {
            Destroy(logQueue.Dequeue());
        }
        // 自动滚动到底部
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0; // 0表示底部
        }
    }
    //---------------------------------------------------------------------------------//
    public void LogTurnStart(int turn, int coin)
    {
        if (coin > 0)
        {
            string message = $"第{turn}回合开始,奖励{coin}金币";
            AddLog(message, Color.white);
        }
        else
        {
            string message = $"第{turn}回合开始";
            AddLog(message, Color.white);
        }
    }
    public void LogHome(PlayerCore player, bool isLvUP)
    {
        string message = $"{player.playerData.playerName}到达Hmoe,回复2HP";
        AddLog(message, Color.white);
        if (isLvUP)
        {
            string message2 = $"{player.playerData.playerName}升级了";
            AddLog(message2, Color.white);
        }
    }
    public void LogBonus(PlayerCore player, int coin)
    {
        string message = $"{player.playerData.playerName}到达Bonus,获得{coin}金币";
        AddLog(message, Color.white);
    }
    public void LogMove(PlayerCore player)
    {
        string message = $"{player.playerData.playerName}再次移动";
        AddLog(message, Color.white);
    }
    public void LogDraw(PlayerCore player, int cardCount)
    {
        string message = $"{player.playerData.playerName}抽到{cardCount}张牌";
        AddLog(message, Color.white);
    }
    public void LogTeleport(PlayerCore player, PlayerCore target)
    {
        string message = $"{player.playerData.playerName}传送到{target.playerData.playerName}处";
        AddLog(message, Color.white);
    }
    public void LogCauseDamage(PlayerCore attacker, PlayerCore target, int damage)
    {
        string message = $"{attacker.playerData.playerName}对{target.playerData.playerName}造成{damage}点伤害";
        AddLog(message, Color.red);
    }
    public void LogSelfDamage(PlayerCore player, int damage)
    {
        string message = $"{player.playerData.playerName}受到{damage}点伤害";
        AddLog(message, Color.red);
    }
    public void LogPlayerDead(PlayerCore player)
    {
        string message = $"{player.playerData.playerName}倒下了";
        AddLog(message, Color.yellow);
    }
    public void cardApply(PlayerData player, int duration, string cardName)
    {
        string message = $"[效果]{player.playerName}的{cardName}效果还有{duration}回合";
        AddLog(message, Color.white);
    }
    public void cardAdd(PlayerData player, int duration, string cardName)
    {
        string message = $"[效果]{player.playerName}获得了[{cardName}]效果，持续{duration}回合";
        AddLog(message, Color.white);
    }
    public void cardExpire(PlayerData player, string cardName)
    {
        string message = $"[效果]{player.playerName}的[{cardName}]效果已结束";
        AddLog(message, Color.white);
    }
}