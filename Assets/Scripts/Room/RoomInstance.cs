using System.Linq;
using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomInstance : NetworkBehaviour
{
    public static RoomInstance Instance; // 单例

    [SyncVar]
    public string roomId = "DefaultRoom";
    [SyncVar]
    public string selectedMap = "Map_1_Basic"; // 默认地图

    // 同步玩家信息列表
    public readonly SyncList<PlayerInfo> players = new SyncList<PlayerInfo>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    [System.Serializable]
    public struct PlayerInfo
    {
        public string name;
        public int slotIndex; // 玩家顺序
        public bool isReady;
        public uint netId; // 改用netId标识玩家唯一性
        public int connectionId; // 玩家连接ID,用于游戏内和此处绑定
        public int roleId; // 玩家角色
        public int coin;
        public int level;
    }
    // 客户端初始化回调
    public override void OnStartClient()
    {
        players.Callback += OnPlayerDataUpdated;
    }
    // 玩家数据更新回调
    public void OnPlayerDataUpdated(SyncList<PlayerInfo>.Operation op, int index, PlayerInfo oldItem, PlayerInfo newItem)
    {
        if (SceneManager.GetActiveScene().name == "Map_1_basic")
        {
            // 如果当前是游戏场景，刷新UI
            GameUI.Instance?.UpdateAllPlayerPanels(this);
        }
    }
    [Server]
    public void AddPlayer(string playerName, uint netId, int connectionId)
    {
        Debug.Log($"服务器正在添加玩家 {playerName} (ID: {netId})");
        int targetSlot = -1;
        for (int slot = 1; slot <= 4; slot++)
        {
            if (!players.Any(p => p.slotIndex == slot))
            {
                targetSlot = slot;
                break;
            }
        }

        if (targetSlot == -1)
        {
            Debug.LogError("房间已满，无法加入！");
            return;
        }

        PlayerInfo info = new PlayerInfo
        {
            name = playerName,
            slotIndex = targetSlot,
            isReady = false,
            netId = netId,
            connectionId = connectionId,
            roleId = 0, // 默认角色为空
            coin = 10,  // 默认金币
            level = 0      // 默认等级
        };

        players.Add(info);
        Debug.Log($"玩家 {playerName} 已作为P{targetSlot}添加到房间，当前人数：{players.Count}");
    }

    [Server]
    public void SetPlayerReady(uint netId, bool ready)
    {
        int index = players.FindIndex(p => p.netId == netId);
        if (index >= 0)
        {
            var player = players[index];
            player.isReady = ready;
            players[index] = player;
        }
    }
    [Server]
    public void CheckAllPlayersReady()
    {
        if (AllPlayersReady())
        {
            // 调用NetworkManager加载场景
            NetworkManager.singleton.ServerChangeScene(selectedMap);
        }
    }

    [Server]
    public bool AllPlayersReady()
    {
        //if (players.Count < 4) return false;
        foreach (var p in players)
        {
            if (!p.isReady)
                return false;
        }
        return true;
    }
}
