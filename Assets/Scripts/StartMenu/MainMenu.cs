using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class MainMenu : MonoBehaviour
{
    public static string PlayerName { get; private set; }
    public static string IPAddress { get; private set; }

    [Header("UI组件")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField IPInputField;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] GameObject mainMenuPanel;         // 主菜单面板
    [SerializeField] private GameObject warningPanel;  // 新增警告面板
    [SerializeField] private Button closeErrorButton;  // 关闭错误按钮

    private NetworkManagerRoom networkManagerRoom;

    private void Start()
    {
        networkManagerRoom = NetworkManager.singleton as NetworkManagerRoom;
        // 自动绑定
        if (!nameInputField) nameInputField = GetComponentInChildren<TMP_InputField>();
        if (!startButton) startButton = GameObject.Find("StartBtn").GetComponent<Button>();
        if (!quitButton) quitButton = GameObject.Find("QuitBtn").GetComponent<Button>();

        // 按钮事件
        startButton.onClick.AddListener(OnStartClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        // 名称长度限制
        nameInputField.characterLimit = 10;
    }

    private void OnStartClicked()
    {
        PlayerName = string.IsNullOrWhiteSpace(nameInputField.text)
            ? "Player"
            : nameInputField.text.Trim();
        IPAddress = string.IsNullOrWhiteSpace(IPInputField.text)
            ? "1.95.195.91"
            : IPInputField.text.Trim();

        
        networkManagerRoom.networkAddress = IPAddress; 
        networkManagerRoom.StartClient();
        mainMenuPanel.SetActive(false);
        Debug.Log($"连接服务器：{IPAddress}");
    }
    private void OnConfirmError()
    {
        warningPanel.SetActive(false);
    }
    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}