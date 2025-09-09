using Mirror;
using UnityEngine;

public class RoleSelectionManager : MonoBehaviour
{
    public static RoleSelectionManager Instance;

    private void Awake()
    {
        Instance = this; // ������ʼ��
    }

    // �ͻ���ѡ���ɫ�����
    public void SelectRole(int roleId)
    {
        // ��ȡ������ҵ�NetworkPlayer���
        NetworkPlayer localPlayer = NetworkClient.connection.identity.GetComponent<NetworkPlayer>();
        // �������������
        localPlayer.CmdSelectRole(roleId);
    }
    // ���ݽ�ɫID���Ұ�ť���������н�ɫ��ť������ͬһ���������£�
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