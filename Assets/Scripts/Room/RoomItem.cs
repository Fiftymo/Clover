using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] TMP_Text playerCountText;
    [SerializeField] Button joinButton;

    private LobbyUI lobbyUI;
    public RoomInstance roomInstance;

    private void Start()
    {
        roomInstance = RoomInstance.Instance;
        lobbyUI = LobbyUI.Instance;
        // 确保关键组件存在
        if (roomInstance == null)
            Debug.LogError("RoomInstance未找到！");
        if (lobbyUI == null)
            Debug.LogError("LobbyUI未找到！");
        Setup();
    }
    public void Setup()
    {
        playerCountText.text = $"玩家人数：{roomInstance.players.Count}/4";

        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(OnJoinClicked);

        if (roomInstance.players.Count >= 4)
        {
            joinButton.interactable = false;
            joinButton.GetComponentInChildren<TMP_Text>().text = "房间已满";
        }
        else
        {
            joinButton.interactable = true;
            joinButton.GetComponentInChildren<TMP_Text>().text = "加入房间";
        }
    }

    public void OnJoinClicked()
    {
        lobbyUI.OnJoinRoom();
    }
}