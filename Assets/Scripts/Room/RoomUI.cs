using TMPro;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RoomUI : MonoBehaviour
{
    [Header("UI组件")]
    public List<GameObject> playerSlots; // 手动分配的4个玩家面板
    public RoomInstance roomInstance;
    public Sprite defaultRoleImage;
    public Button readyButton;           // 准备按钮

    // 改为手动设置RoomInstance组件避免冲突
    public void SetRoomInstance(RoomInstance instance)
    {
        if (roomInstance != null)
            roomInstance.players.Callback -= OnPlayerListChanged;

        roomInstance = instance;
        if (roomInstance != null)
        {
            roomInstance.players.Callback += OnPlayerListChanged;
            RefreshUI(); // 立即触发一次刷新
        }
    }

    void OnPlayerListChanged(SyncList<RoomInstance.PlayerInfo>.Operation op, int index, RoomInstance.PlayerInfo oldItem, RoomInstance.PlayerInfo newItem)
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        Debug.Log($"正在 RefreshUI");
        if (roomInstance == null || playerSlots.Count == 0)
        {
            Debug.LogError("roomInstance或playerSlots 未初始化或为空");
            return;
        }
        Debug.Log($"Number of players in room: {roomInstance.players.Count}");
        // 初始化面板文字
        foreach (var slot in playerSlots)
        {
            slot.transform.Find("NameText/NameText").GetComponent<TextMeshProUGUI>().text = "等待中...";
            slot.transform.Find("background").GetComponent<Image>().sprite = defaultRoleImage;
            slot.transform.Find("ReadyBar").gameObject.SetActive(false);
        }
        // 写入实际玩家数据
        foreach (var player in roomInstance.players)
        {
            int slotIndex = player.slotIndex - 1; // 槽位从0开始索引
            if (slotIndex < 0 || slotIndex >= playerSlots.Count)
                continue;

            GameObject slot = playerSlots[slotIndex];
            slot.transform.Find("NameText/NameText").GetComponent<TextMeshProUGUI>().text = player.name;
            slot.transform.Find("OrderText/OrderText").GetComponent<TextMeshProUGUI>().text = $"P{player.slotIndex}";
            // 设置角色图片
            if (player.roleId != 0)
            {
                // 根据角色ID查找对应的按钮数据
                RoleButton roleButton = RoleSelectionManager.Instance.FindRoleButtonById(player.roleId);
                slot.transform.Find("background").GetComponent<Image>().sprite = roleButton.roleImage;
            }
            slot.transform.Find("ReadyBar").gameObject.SetActive(player.isReady);
        }

        Debug.Log($"Number of players in room: {roomInstance.players.Count}");
    }
    public void OnReadyButtonClicked()
    {
        Debug.Log("有人准备了");
        NetworkPlayer localPlayer = NetworkClient.connection.identity.GetComponent<NetworkPlayer>();
        RoomInstance room = RoomInstance.Instance;
        uint localNetId = NetworkClient.connection.identity.netId;
        int playerIndex = room.players.FindIndex(p => p.netId == localNetId);
        if (playerIndex == -1) return;

        // 如果未选择角色，禁止准备
        if (room.players[playerIndex].roleId == 0)
        {
            Debug.Log("请先选择角色！");
            return;
        }

        // 切换准备状态
        bool newReadyState = !room.players[playerIndex].isReady;
        localPlayer.CmdSetReady(newReadyState);
        // 更新按钮文本
        readyButton.GetComponentInChildren<TextMeshProUGUI>().text = newReadyState ? "取消" : "准备";
    }
    public void OnLeaveButtonClicked()
    {
        Debug.Log("有人离开了");
        NetworkClient.connection.identity.GetComponent<NetworkPlayer>().CmdLeaveRoom();
    }
    public void OnRoleButtonClicked(int roleId)
    {
        // 获取本地玩家的NetworkPlayer组件
        NetworkPlayer localPlayer = NetworkClient.connection.identity.GetComponent<NetworkPlayer>();
        // 发送命令到服务器尝试选择角色
        localPlayer.CmdSelectRole(roleId);
    }
}