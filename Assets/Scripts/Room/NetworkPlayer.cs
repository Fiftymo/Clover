// 客户端玩家管理器
using Mirror;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NetworkPlayer : NetworkRoomPlayer
{
    [SyncVar] public string playerName;
    [SyncVar] public string selectedRoleId;
    [SyncVar] public string joinedRoomId;

    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            CmdSetPlayerName(MainMenu.PlayerName);
        }
    }

    [Command]
    private void CmdSetPlayerName(string name)
    {
        // 服务器端设置SyncVar
        playerName = name;
    }

    [Command] // 加入房间
    public void CmdJoinRoom()
    {
        // 直接获取服务器的唯一房间
        RoomInstance room = RoomInstance.Instance;
        uint netId = connectionToClient.identity.netId; // 服务器端获取客户端的netId
        int connectionId = connectionToClient.connectionId;
        room.AddPlayer(playerName, netId, connectionId);
        // 通知客户端显示等待房间UI
        TargetShowWaitingRoom(connectionToClient);
    }
    [Command] // 退出房间
    public void CmdLeaveRoom()
    {
        RoomInstance room = RoomInstance.Instance;
        room.players.RemoveAll(p => p.netId == connectionToClient.identity.netId);
        // 如果房间为空，重置玩家列表（可选）
        if (room.players.Count == 0)
        {
            room.players.Clear();
        }
        // 通知客户端隐藏等待房间UI
        TargetHideWaitingRoom(connectionToClient);
    }
    [Command] // 准备
    public void CmdSetReady(bool ready)
    {
        if (!isServer) return; // 确保仅在服务器执行

        uint netId = connectionToClient.identity.netId;
        RoomInstance room = RoomInstance.Instance;
        int playerIndex = room.players.FindIndex(p => p.netId == netId);
        if (playerIndex != -1)
        {
            var playerInfo = room.players[playerIndex];
            playerInfo.isReady = ready;
            room.players[playerIndex] = playerInfo; // 更新准备状态
            Debug.Log($"玩家 {playerName} 准备状态：{ready}");
            // 同步UI到所有客户端
            RpcUpdateReadyUI(netId, ready);
            room.CheckAllPlayersReady();
        }
    }
    [Command]
    public void CmdSelectRole(int roleId)
    {
        RoomInstance room = RoomInstance.Instance;
        uint netId = connectionToClient.identity.netId;
        int playerIndex = room.players.FindIndex(p => p.netId == netId);

        // 检查角色是否已被选择
        bool isRoleTaken = room.players.Any(p => p.roleId == roleId);
        if (isRoleTaken)
        {
            Debug.Log($"角色 {roleId} 已被选择");
            TargetShowRoleTakenError(connectionToClient);
            return;
        }

        // 更新玩家角色信息
        if (playerIndex != -1)
        {
            var playerInfo = room.players[playerIndex];
            playerInfo.roleId = roleId;
            room.players[playerIndex] = playerInfo;
            Debug.Log($"玩家 {playerName} 选择了角色 {roleId}");

        }

        // 通知所有客户端更新UI
        RpcUpdateRoleSelection(roleId, connectionToClient.identity.netId);
    }
    //---------------------------------------------------------------------------------//
    [TargetRpc]
    private void TargetShowWaitingRoom(NetworkConnection target)
    {
        RoomInstance.Instance = NetworkClient.spawned.Values
        .FirstOrDefault(obj => obj.GetComponent<RoomInstance>() != null)
        ?.GetComponent<RoomInstance>();
        // 绑定房间数据到固定UI
        RoomUI roomUI = LobbyUI.Instance.waittingRoomPanel.GetComponent<RoomUI>();
        roomUI.SetRoomInstance(RoomInstance.Instance);
        LobbyUI.Instance.ShowWaitingRoom();
    }
    [TargetRpc]
    private void TargetHideWaitingRoom(NetworkConnection target)
    {
        LobbyUI.Instance.HideWaitingRoom();
    }
    [TargetRpc]
    private void TargetShowRoleTakenError(NetworkConnection target)
    {
        Debug.LogWarning("该角色已被其他玩家选择！");
        // 可选：在客户端显示错误提示UI
    }
    [ClientRpc]
    private void RpcUpdateRoleSelection(int roleId, uint netId)
    {
        // 更新所有客户端的UI
        RoomUI roomUI = LobbyUI.Instance.waittingRoomPanel.GetComponent<RoomUI>();
        int playerIndex = roomUI.roomInstance.players.FindIndex(p => p.netId == netId);
        if (playerIndex != -1)
        {
            // 获取角色按钮的图片
            RoleButton roleButton = RoleSelectionManager.Instance.FindRoleButtonById(roleId); // 需实现查找方法
            Image background = roomUI.playerSlots[playerIndex].transform.Find("background").GetComponent<Image>();
            background.sprite = roleButton.roleImage;
        }
    }
    [ClientRpc]
    private void RpcUpdateReadyUI(uint netId, bool isReady)
    {
        RoomUI roomUI = LobbyUI.Instance.waittingRoomPanel.GetComponent<RoomUI>();
        int playerIndex = roomUI.roomInstance.players.FindIndex(p => p.netId == netId);
        RoomInstance roomInstance = NetworkClient.spawned.Values
        .FirstOrDefault(obj => obj.GetComponent<RoomInstance>() != null)?
        .GetComponent<RoomInstance>();
        if (LobbyUI.Instance == null || LobbyUI.Instance.waittingRoomPanel == null)
        {
            Debug.LogError("5.LobbyUI或waittingRoomPanel未初始化！");
            return;
        }
        if (roomUI == null)
        {
            Debug.LogError("5.RoomUI组件未找到！");
            return;
        }
        if (roomInstance == null)
        {
            Debug.LogError("5.未找到RoomInstance！");
            return;
        }
        if (playerIndex != -1)
        {
            // 更新对应槽位的准备状态显示
            GameObject slot = roomUI.playerSlots[playerIndex];
            Transform readyIndicator = slot.transform.Find("ReadyBar");
            if (readyIndicator != null)
            {
                readyIndicator.gameObject.SetActive(isReady);
            }

            // 如果是本地玩家，禁用角色按钮
            if (netId == NetworkClient.connection.identity.netId)
            {
                LobbyUI.Instance.ToggleRoleButtons(!isReady);
            }
        }
    }
}
