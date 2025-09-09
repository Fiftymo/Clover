using Mirror;
using UnityEngine;
using System.Collections;

public class BattleManager : NetworkBehaviour
{
    public static BattleManager Instance;
    public GameObject battleUI;              // ս������UI

    [SyncVar] public PlayerCore attacker;
    [SyncVar] public PlayerCore defender;
    private bool isAttackerReady;            // �������Ƿ���׼��
    private bool isDefenderReady;            // �������Ƿ���׼��
    private int defChoice;                   // ������ѡ��Ĳ���
    [Header("������ս����ֵ�洢")]
    private int baseAtk;
    private int baseDef;

    private void Awake()
    {
        Instance = this;
    }
    // ����������ս��
    [Server]
    public void RequestBattle(PlayerCore atk, PlayerCore def)
    {
        this.attacker = atk;
        this.defender = def;    // ��ʱ��Ϊ��--------��������

        // ���������ߵ� UI���Ƿ���ս��
        attacker.playerUI.requestBattleUI.SetData(attacker,defender);
    }
    // ������ִ����ʽս��
    [Server]
    public void StartBattle(PlayerCore atk, PlayerCore def)
    {
        this.attacker = atk;
        this.defender = def;
        // ��ʼ��ս��̨��Ĭ�Ϸ��ط�ѡ�����
        RpcSetUpStage(atk.netId, def.netId);
        // ��������
        if (atk != null)
            Debug.Log("[StartBattle] attacker is not null");
        if (atk.connectionToClient != null)
            Debug.Log("[StartBattle] attacker.connectionToClient is not null");

        // ����׶�1��˫��ѡ����
        TargetStartCardSelection(atk.connectionToClient, true);
        TargetStartCardSelection(def.connectionToClient, false);
    }
    //---------------------------ʵ�ʵ�ս������---------------------------------
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
            // ����׶�2-1��������������
            if (attacker == null)
                Debug.LogError("[CheckReady] attacker is null");
            else
                Debug.Log("attacker not null");
            RpcAttackerDice(attacker);
        }
    }
    // �����������Ӿ�����������
    [ClientRpc]
    private void RpcAttackerDice(PlayerCore player)
    {
        if (player == null)
            Debug.LogError("[RpcAttackerDice] player is null");
        player.playerUI.battleCardUI.SwitchDiceButton();
    }
    // �����������ӻص�
    [Server]
    public void AttackerDice(int atk)
    {
        this.baseAtk = atk;
        // �������ӽ������UI��
        RpcUpdateFinalText(true, atk);
        // ����׶�2-2�����ط���������
        RpcDefenderChoice(defender);
    }
    // ���ط�ѡ��Ӧ�Է���
    [ClientRpc]
    private void RpcDefenderChoice(PlayerCore player)
    {
        if (player == null)
            Debug.LogError("[RpcDefenderChoice] player is null");
        player.playerUI.battleCardUI.ShowDefChoice();
    }
    // ���ط�ѡ�񷽰��ص�
    [Server]
    public void DefenderChoice(int choice)
    {
        if (choice == 0)
        {
            // ����
            defChoice = 0;
        }
        else
        {
            // ����
            defChoice = 1;
        }
        // ����׶�2-3�����ط�Ͷ����
        RpcDefenderDice(defender.connectionToClient);
    }
    // ���ط�������
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
        // �������ӽ������UI��
        // .....
        // ����׶�3������ս���˺�
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
    // ��ȫ�����ͬ��ս��"��"̨
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
        // ��ȡ����(��Ȼ�Ҿ��ú���û��)
        var attacker = atkObj.GetComponent<PlayerCore>();
        var defender = defObj.GetComponent<PlayerCore>();
        // �������������ó���
        BattleStageManager.Instance.SetUp(attacker, defender);
    }
    // ��ս��˫����ʾ����ѡ��UI
    [TargetRpc]
    private void TargetStartCardSelection(NetworkConnection target, bool isAttacker)
    {
        var player = PlayerCore.LocalInstance;
        player.playerUI.battleCardUI.ShowCardSelect(isAttacker);
    }
    //-------------------------������--------------------------
    public void testForBattle()
    {
        PlayerCore player = TurnManager.Instance.currentPlayer;
        player.playerUI.requestBattleUI.SetData(player,player);
    }
    //----------------------------------------------------------
    // ս��UI����
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
