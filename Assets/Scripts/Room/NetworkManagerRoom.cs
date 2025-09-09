using Mirror;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class NetworkManagerRoom : NetworkRoomManager
{
    [Header("房间属性")]
    public string roomName = "默认房间";
    public GameObject roomInstancePrefab; // 房间实例

    // 服务器启动时
    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("服务器启动，生成唯一房间实例");
        // 生成唯一房间
        GameObject roomObj = Instantiate(roomInstancePrefab); // 生成房间实例
        NetworkServer.Spawn(roomObj);
        RoomInstance.Instance = roomObj.GetComponent<RoomInstance>();
        DontDestroyOnLoad(roomObj); // 防止场景切换时销毁
    }
    // 连接服务器
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        Debug.Log($"[服务器] 客户端玩家连接");
    }
    // 断开服务器
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log($"[服务器] 客户端玩家断连");
    }
    public override void OnGUI()
    {
        // 禁用默认的OnGUI()
    }
    //----------------------------------游戏中部分------------------------------------------//
}