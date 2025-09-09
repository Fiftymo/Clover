using Mirror;
using UnityEngine;
using System.Collections;


[RequireComponent(typeof(PlayerData))]
[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(PlayerHand))]
[RequireComponent(typeof(PlayerTurn))]
public class PlayerCore : NetworkBehaviour
{
    public static PlayerCore LocalInstance { get; private set; }

    public PlayerData playerData { get; private set; }
    public PlayerHand playerHand { get; private set; }
    public PlayerMove playerMove { get; private set; }
    public PlayerTurn playerTurn { get; private set; }
    public PlayerUI playerUI { get; private set; }

    public Canvas playerHandUI;

    private void Awake()
    {
        playerData = GetComponent<PlayerData>();
        playerHand = GetComponent<PlayerHand>();
        playerMove = GetComponent<PlayerMove>();
        playerTurn = GetComponent<PlayerTurn>();
        playerUI = GetComponent<PlayerUI>();
    }
    [Server]
    public override void OnStartServer()
    {
        base.OnStartServer();
        TurnManager.Instance.RegisterPlayer(this);
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        // 标记本地玩家实例
        if (isLocalPlayer)
        {
            LocalInstance = this;
        }
        TurnManager.Instance.RegisterPlayer(this);
    }
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        playerHandUI.gameObject.SetActive(true);
        playerHandUI.enabled = true;
        StartCoroutine(NotifyServerReady());
    }
    IEnumerator NotifyServerReady()
    {
        yield return new WaitForSeconds(0.1f); // 或等待某些UI/资源加载完毕
        CmdRequestInitHand();
    }

    [Command]
    void CmdRequestInitHand()
    {
        Debug.Log($"{playerData.playerName} request card");
        playerHand.ServerInitializeHand();
    }
}
