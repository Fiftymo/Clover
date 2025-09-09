using TMPro;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class RequestBattleUI : NetworkBehaviour
{
    [Header("战斗框组件")]
    public Image targetAvatar;
    public TextMeshProUGUI hp;
    public TextMeshProUGUI atk;
    public TextMeshProUGUI def;
    public Button yes;
    public Button no;

    [SyncVar] private PlayerCore attacker;
    [SyncVar] private PlayerCore defender;

    private void Awake()
    {
        yes.onClick.AddListener(() =>
        {
            // 实际的战斗逻辑
            gameObject.SetActive(false);
            CmdStartBattle(attacker,defender);

        });
        no.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            CmdCanleBattle(attacker);
        });
    }
    // 显示请求战斗的UI
    public void SetData(PlayerCore curPlayer, PlayerCore targetPlayer)
    {
        attacker = curPlayer;
        defender = targetPlayer;
        RoleData role = RoleManager.Instance.roleList.Find(r => r.roleId == targetPlayer.playerData.playerRoleId);
        this.targetAvatar.sprite = role.roleImage;
        this.hp.text = targetPlayer.playerData.hp.ToString();
        this.atk.text = targetPlayer.playerData.atk.ToString();
        this.def.text = targetPlayer.playerData.def.ToString();
        gameObject.SetActive(true);
    }

    [Command]
    private void CmdCanleBattle(PlayerCore player)
    {
        player.playerMove.isStepWaiting = false;
    }
    [Command]
    private void CmdStartBattle(PlayerCore attacker, PlayerCore defender)
    {
        BattleManager.Instance.StartBattle(attacker, defender);
    }
}
