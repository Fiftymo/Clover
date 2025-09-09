using Mirror;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerData : NetworkBehaviour
{
    public RoomInstance roomInstance;

    [Header("�������")]
    [SyncVar] public int playerIndex;     // ���������0-3��--soltIndex
    [SyncVar] public string playerName;
    [SyncVar] public int playerRoleId;
    [SyncVar] public int coin;
    [SyncVar] public int level;
    [SyncVar] public int maxHp;
    [SyncVar] public int hp;
    [SyncVar] public int atk;
    [SyncVar] public int minAtk;
    [SyncVar] public int maxAtk;
    [SyncVar] public int def;
    [SyncVar] public int minDef;    
    [SyncVar] public int maxDef;
    [SyncVar] public int mis = 0;
    [SyncVar] public int effectChioce = 0;
    [SyncVar] public int cardPointMax = 3;
    [SyncVar(hook = nameof(OnCPointsUpdated))]
    public int cardPoint;
    [SyncVar] public int battlePointMax = 3;
    [SyncVar(hook = nameof(OnBPointsUpdated))]
    public int battlePoint;
    [Header("��������")]
    [SyncVar] public int damageReduce = 0;                   // �˺�����
    private PlayerCore playerCore;
    [Header("���״̬")]
    public List<TimedEffect> activeEffects = new List<TimedEffect>(); // ����Ч��
    [SyncVar] public bool isDead = false;          // �Ƿ�����
    [SyncVar] public bool isLinked = false;        // �Ƿ�����
    [SyncVar] public bool canBeSelected = true;    // �Ƿ��ѡ

    //-------------------------------------��ʼ��---------------------------------------------------
    private void Awake()
    {
        playerCore = GetComponent<PlayerCore>(); // ��ȡ���ӵ�ͬһ����� PlayerCore ���
    }
    [Server]
    public override void OnStartServer()
    {
        base.OnStartServer();
        
        roomInstance = RoomInstance.Instance;
        int connectionId = connectionToClient.connectionId;
        // ��ȡ��ɫID
        RoomInstance.PlayerInfo playerInfo = RoomInstance.Instance.players
            .FirstOrDefault(p => p.connectionId == connectionId);
        playerIndex = playerInfo.slotIndex - 1;  // (0-3)
        playerName = playerInfo.name;
        coin = 10;     // ��ʼ���
        level = 0;    // ��ʼ�ȼ�
        InitPlayer();
        // ����������ȡ��Ӧ������
        SpawnPoint spawn = SpawnManager.Instance.GetSpawnPoint(playerIndex);
        if (spawn != null)
        {
            spawn.SetupPlayer(playerCore);
            Debug.Log($"player netId({netId}) spawn in {spawn.startTile.name}");
        }
    }
    [Server]
    public void InitPlayer()
    {
        int connectionId = connectionToClient.connectionId;
        // ��ȡ��ɫID
        RoomInstance.PlayerInfo playerInfo = RoomInstance.Instance.players
            .FirstOrDefault(p => p.connectionId == connectionId);
        if (playerInfo.connectionId != 0) // playerInfo�ṹ�岻��ֱ�ӿ�ֵ�Ƚ�
        {
            // ��RoleManager��ȡ��ɫ����
            RoleData role = RoleManager.Instance.roleList
                .FirstOrDefault(r => r.roleId == playerInfo.roleId);
            if (role != null)
            {
                // ��ʼ��SyncVar����
                playerRoleId = role.roleId;
                maxHp = role.hp;
                hp = role.hp;
                atk = role.atk;
                def = role.def;
                cardPoint = cardPointMax;
                battlePoint = battlePointMax;
                minAtk = maxAtk = atk;
                minDef = maxDef = def;
            }
        }
    }
    //-------------------------------------��ֵ�䶯------------------------------------------------
    [Server]
    public void ChangeCoin(int amount)
    {
        coin = Mathf.Max(coin + amount, 0);
        UpdatePlayerUI(playerIndex);
    }
    [Server]
    public void ChangeLevel(int amount)
    {
        level = Mathf.Min(level + amount, 3);
        UpdatePlayerUI(playerIndex);
    }
    [Server]
    public void ChangeHp(int amount)
    {
        hp = Mathf.Max(hp + amount, 0);
        hp = Mathf.Min(hp, maxHp);
        UpdatePlayerUI(playerIndex);
    }
    [Server]
    public void ChangeAtk(int amount)
    {
        atk += amount;
        UpdatePlayerUI(playerIndex);
    }
    [Server]
    public void ChangeDef(int amount)
    {
        def += amount;
        UpdatePlayerUI(playerIndex);
    }
    [Server]
    public void ChangeCPoints(int cost)
    {
        cardPoint = Mathf.Max(cardPoint - cost, 0);
    }
    [Server]
    public void ChangeBPoints(int cost)
    {
        battlePoint = Mathf.Max(battlePoint - cost, 0);
    }
    [Server] // �����˺��������˺����⣬���������˺�
    public void HandleDamage(int damage)
    {
        int finalDamage = damage - damageReduce;
        ChangeHp(-finalDamage);
    }
    public void UpdatePlayerUI(int index)
    {
        index++;
        GameUI.Instance.RpcUpdatePlayerPanel(index, coin, level, hp, atk, def);
        if (hp == 0)
        {
            isDead = true;
            UpdateDead(isDead);
        }
    }
    [Server]
    private void UpdateDead(bool newValue)
    {
        isDead = newValue;
    }
    // ���Ƶ���ͬ���ص�
    private void OnCPointsUpdated(int oldValue, int newValue)
    {
        PlayerUI.LocalInstance.UpdatePoints(newValue);
    }
    private void OnBPointsUpdated(int oldValue, int newValue)
    {
        PlayerUI.LocalInstance.battleCardUI.UpdatePoints(newValue);
    }
    //-------------------------------------����Ч������------------------------------------------------
    public void ApplyTurnEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];
            effect.Apply(this);
            effect.duration--;

            if (effect.duration <= 0)
            {
                effect.OnExpire(this);
                activeEffects.RemoveAt(i);
            }
        }
    }
}
