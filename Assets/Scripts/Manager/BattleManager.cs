using Mirror;
using UnityEngine;
using System.Collections;

public class BattleManager : NetworkBehaviour
{
    public static BattleManager Instance;
    public GameObject battleUI;              // 战斗界面UI

    [SyncVar] public PlayerCore attacker;
    [SyncVar] public PlayerCore defender;
    private bool isAttackerReady;            // 攻击者是否已准备
    private bool isDefenderReady;            // 防御者是否已准备
    private int defChoice;                   // 防御者选择的策略
    [Header("服务器战斗数值存储")]
    private int baseAtk;
    private int baseDef;

    private void Awake()
    {
        Instance = this;
    }
    // 服务器请求战斗
    [Server]
    public void RequestBattle(PlayerCore atk, PlayerCore def)
    {
        this.attacker = atk;
        this.defender = def;    // 此时不为空--------！！！！

        // 弹出攻击者的 UI：是否发起战斗
        attacker.playerUI.requestBattleUI.SetData(attacker,defender);
    }
    // 服务器执行正式战斗
    [Server]
    public void StartBattle(PlayerCore atk, PlayerCore def)
    {
        this.attacker = atk;
        this.defender = def;
        // 初始化战斗台，默认防守方选择防御
        RpcSetUpStage(atk.netId, def.netId);
        // 测试引用
        if (atk != null)
            Debug.Log("[StartBattle] attacker is not null");
        if (atk.connectionToClient != null)
            Debug.Log("[StartBattle] attacker.connectionToClient is not null");

        // 进入阶段1：双方选择卡牌
        TargetStartCardSelection(atk.connectionToClient, true);
        TargetStartCardSelection(def.connectionToClient, false);
    }
    //---------------------------实际的战斗流程---------------------------------
    [Server]
    public void CheckReady(PlayerCore player, bool isAttacker)
    {
        defChoice = 0;
        if (isAttacker)
        {
            isAttackerReady = true;
            this.attacker = player;
        }
        else
        {
            isDefenderReady = true;
            this.defender = player;
        }
        isDefenderReady = true;
        if (isAttackerReady && isDefenderReady)
        {
            // 进入阶段2-1：攻击方丢骰子
            if (attacker == null)
                Debug.LogError("[CheckReady] attacker is null");
            else
                Debug.Log("attacker not null");
            RpcAttackerDice(attacker);
        }
    }
    // 攻击方掷骰子决定基础攻击
    [ClientRpc]
    private void RpcAttackerDice(PlayerCore player)
    {
        if (player == null)
            Debug.LogError("[RpcAttackerDice] player is null");
        player.playerUI.battleCardUI.SwitchDiceButton();
    }
    // 攻击方掷骰子回调
    [Server]
    public void AttackerDice(int atk)
    {
        this.baseAtk = atk;
        // 根据骰子结果更新UI等
        RpcUpdateFinalText(true, atk);
        // 进入阶段2-2：防守方决定策略
        RpcDefenderChoice(defender);
    }
    // 防守方选择应对方案
    [ClientRpc]
    private void RpcDefenderChoice(PlayerCore player)
    {
        if (player == null)
            Debug.LogError("[RpcDefenderChoice] player is null");
        player.playerUI.battleCardUI.ShowDefChoice();
    }
    // 防守方选择方案回调
    [Server]
    public void DefenderChoice(int choice)
    {
        if (choice == 0)
        {
            // 防御
            defChoice = 0;
        }
        else
        {
            // 闪避
            defChoice = 1;
        }
        // 进入阶段2-3：防守方投骰子
        RpcDefenderDice(defender.connectionToClient);
    }
    // 防守方掷骰子
    [TargetRpc]
    private void RpcDefenderDice(NetworkConnection target)
    {
        var player = PlayerCore.LocalInstance;
        player.playerUI.battleCardUI.SwitchDiceButton();
    }
    [Server]
    public void DefenderDice(int def)
    {
        this.baseDef = def;
        // 根据骰子结果更新UI等
        // .....
        // 进入阶段3：计算战斗伤害
        ComputeDamage();
    }
    [Server]
    private void ComputeDamage()
    { 
        if (defChoice == 0)
        { }
        else if (defChoice == 1)
        {
            if (baseDef > baseAtk)
            { }
            else
            {
                if (baseDef == baseAtk && baseAtk == 6)
                { }
            }
        }
    }
    //-------------------------------------------------------------------------
    // 向全体玩家同步战斗"武"台
    [ClientRpc]
    private void RpcSetUpStage(uint attackerId, uint defenderId)
    {
        battleUI.SetActive(true);
        if (!NetworkClient.spawned.TryGetValue(attackerId, out var atkObj))
        {
            Debug.LogError($"Cannot find attacker with id {attackerId}");
            return;
        }
        if (!NetworkClient.spawned.TryGetValue(defenderId, out var defObj))
        {
            Debug.LogError($"Cannot find defender with id {defenderId}");
            return;
        }
        // 获取引用(虽然我觉得好像没用)
        var attacker = atkObj.GetComponent<PlayerCore>();
        var defender = defObj.GetComponent<PlayerCore>();
        // 场景管理器设置场景
        BattleStageManager.Instance.SetUp(attacker, defender);
    }
    // 向战斗双方显示卡牌选择UI
    [TargetRpc]
    private void TargetStartCardSelection(NetworkConnection target, bool isAttacker)
    {
        var player = PlayerCore.LocalInstance;
        player.playerUI.battleCardUI.ShowCardSelect(isAttacker);
    }
    //-------------------------测试用--------------------------
    public void testForBattle()
    {
        PlayerCore player = TurnManager.Instance.currentPlayer;
        player.playerUI.requestBattleUI.SetData(player,player);
    }
    //----------------------------------------------------------
    // 战斗UI更新
    [ClientRpc]
    public void RpcUpdateBattleUI(bool isAttacker,int min,int max,int hp)
    {
        if (BattleStageManager.Instance != null)
        {
            BattleStageManager.Instance.UpdateText(isAttacker,min,max,hp);
        }
        else
        {
            Debug.LogWarning("BattleStageManager.Instance is null on client.");
        }
    }
    [ClientRpc]
    public void RpcUpdateFinalText(bool isAttacker, int values)
    {
        if (BattleStageManager.Instance != null)
        {
            BattleStageManager.Instance.UpdateFinalText(isAttacker, values);
        }
        else
        {
            Debug.LogWarning("BattleStageManager.Instance is null on client.");
        }
    }
    [ClientRpc]
    private void SwitchToMiss()
    { }
}
