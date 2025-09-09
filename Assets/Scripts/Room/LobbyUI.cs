// 客户端UI脚本
using Mirror;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class LobbyUI : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] public GameObject roomItemPrefab; // 房间列表预制体
    [SerializeField] public Transform roomListContent; // 房间列表展示框
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TextMeshProUGUI statusText;

    [SerializeField] public GameObject roomListPanel;         // 大厅房间列表
    [SerializeField] private GameObject createRoomPanel;      // 创建房间对话框
    [SerializeField] public GameObject waittingRoomPanel;     // 等待房间
    [SerializeField] public GameObject roleSelectionPanel;    // 角色选择面板

    private NetworkManagerRoom networkManagerRoom;
    public static LobbyUI Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        } // 初始化单例
    }

    void Start()
    {
        networkManagerRoom = NetworkManager.singleton as NetworkManagerRoom;
        if (networkManagerRoom == null)
        {
            Debug.LogError("NetworkManager 不是 NetworkManagerRoom 类型");
            return;
        }
    }
    // 点击"创建房间"按钮时调用
    public void OnCreateRoomButtonClick()
    {
        Debug.Log($"{MainMenu.PlayerName}正在尝试创建房间");
        createRoomPanel.SetActive(true);
    }
    // 点击确认按钮时调用
    public void OnConfirmCreateRoom()
    {
        OnCancelCreateRoom();
    }
    // 点击取消按钮时调用
    public void OnCancelCreateRoom()
    {
        createRoomPanel.SetActive(false);
        Debug.Log("取消创建房间");
    }
    // 点击"加入房间"按钮时调用
    public void OnJoinRoom()
    {
        Debug.Log($"{MainMenu.PlayerName}正在尝试加入房间");
        var player = NetworkClient.connection.identity.GetComponent<NetworkPlayer>();
        if (player != null)
        {
            player.CmdJoinRoom(); // 使用房间ID加入
        }
        ShowWaitingRoom();
    }
    // 点击"返回大厅"按钮时调用
    public void OnBackToLobby()
    {
        HideWaitingRoom();
    }
    // 客户端连接服务器成功
    void OnConnected(NetworkConnection conn)
    {
        Debug.Log("成功连接到服务器");
    }
    // 客户端断开连接
    void OnDisconnected(NetworkConnection conn)
    {
        Debug.LogError("与服务器断开连接");
    }
    // 显示等待房间
    public void ShowWaitingRoom()
    {
        roomListPanel.SetActive(false);
        waittingRoomPanel.SetActive(true);
    }
    // 隐藏等待房间
    public void HideWaitingRoom()
    {
        waittingRoomPanel.SetActive(false);
        roomListPanel.SetActive(true);
    }
    // 切换角色按钮的可用性
    public void ToggleRoleButtons(bool enable)
    {
        foreach (Transform child in roleSelectionPanel.transform)
        {
            Button selectButton = child.GetComponent<Button>();
            if (selectButton != null)
            {
                selectButton.interactable = enable;
            }
        }
        Transform quitButtonTransform = waittingRoomPanel.transform.Find("QuitButton");
        Button quitButton = quitButtonTransform.GetComponent<Button>();
        if (quitButton != null)
        {
            quitButton.interactable = enable;
        }
    }
}