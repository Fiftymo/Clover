using Mirror;
using UnityEngine;

public class RoleSelectionManager : MonoBehaviour
{
    public static RoleSelectionManager Instance;

    private void Awake()
    {
        Instance = this; // 单例初始化
    }

    // 客户端选择角色后调用
    public void SelectRole(int roleId)
    {
        // 获取本地玩家的NetworkPlayer组件
        NetworkPlayer localPlayer = NetworkClient.connection.identity.GetComponent<NetworkPlayer>();
        // 发送命令到服务器
        localPlayer.CmdSelectRole(roleId);
    }
    // 根据角色ID查找按钮（假设所有角色按钮挂载在同一个父物体下）
    public RoleButton FindRoleButtonById(int roleId)
    {
        foreach (Transform child in LobbyUI.Instance.roleSelectionPanel.transform)
        {
            RoleButton button = child.GetComponent<RoleButton>();
            if (button != null && button.roleId == roleId)
                return button;
        }
        return null;
    }
}