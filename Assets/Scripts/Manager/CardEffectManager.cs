using Mirror;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardEffectManager : NetworkBehaviour
{
    public static CardEffectManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    public void HandleEffect(PlayerCore player, CardData card)
    {
        switch (card.cardName)
        {
            case "boom":
                boom(player);
                break;
            case "dice":
                dice(player);
                break;
            case "doubleMove":
                doubleMove(player);
                break;
            case "EXsword":
                player.playerUI.TargetShowPlayerChoice("effect", "EXsword", 8, false);
                break;
            case "fight":
                fight(player);
                break;
            case "gogogo":
                gogogo(player);
                break;
            case "heal1":
                player.playerUI.TargetShowCardChoice(card);
                break;
            case "heal2":
                heal2(player);
                break;
            case "kuangnu":
                kuangnu(player);
                break;
            case "laser":
                player.playerUI.TargetShowPlayerChoice("effect", "laser", 5, false);
                break;
            case "link":
                player.playerUI.TargetShowPlayerChoice("effect", "link", 0, true);
                break;
            case "moneng":
                player.playerUI.TargetShowPlayerChoice("effect", "moneng", 0, true);
                break;
            case "nvwu":
                player.playerUI.TargetShowCardChoice(card);
                break;
            case "shop":
                player.playerUI.TargetShowCardChoice(card);
                break;
            default:
                Debug.Log("δ֪Ч����");
                break;
        }
    }
    //-----------------------------------------------
    public void HandleCounter(PlayerCore player, CardData card)
    {
    }
    //-----------------------------------------------
    public void HandleBattle(PlayerCore player, CardData card)
    {
        atk1(player);
        /*
        switch (card.cardName)
        {
            case "atk1":
                atk1(player);
                break;
            case "atk2":
                atk2(player);
                break;
            case "atk3":
                atk3(player);
                break;
            case "atk99":
                atk99(player);
                break;
            case "atkCoin":
                if (player.playerData.coin < 5) return;
                player.playerUI.selectStepsUI.AtkChoiceIndex(player);
                break;
            case "def1":
                def1(player);
                break;
            case "def2":
                def2(player);
                break;
            case "def3":
                def3(player);
                break;
            case "defMiss":
                defMiss(player);
                break;
            default:
                Debug.Log("δ֪ս����");
                break;
        }*/
    }
    //-----------------Ч����------------------------
    //-----------�����Ч��-----------
    [Server]
    public void boom(PlayerCore player)
    {
        player.playerHand.ConfirmUse();
        //-----------------------------
    }
    [Server]
    public void dice(PlayerCore player)
    {
        player.playerHand.ConfirmUse();
        player.playerMove.canFreeRoll = true;
    }
    [Server]
    public void doubleMove(PlayerCore player)
    {
        player.playerHand.ConfirmUse();
        player.playerMove.canDoubleRoll = true;
    }
    [Server]
    public void gogogo(PlayerCore player)
    {
        player.playerHand.ConfirmUse();
        //-----------------------------
    }
    [Server]
    public void heal2(PlayerCore player)
    {
        player.playerHand.ConfirmUse();
        player.playerData.ChangeHp(5);
    }
    [Server]
    public void kuangnu(PlayerCore player)
    {
        player.playerHand.ConfirmUse();
        //------------------------------
    }
    //----------��֧ѡ����---------------
    [Server]
    public void heal1(PlayerCore player, int choice, int count)
    {
        player.playerHand.ConfirmUse();
        // ѡ��1���ָ�2������
        if (choice == 1)
        {
            player.playerData.ChangeHp(2);
        }
        // ѡ��2���ָ�2+��������
        else if (choice == 2)
        {
            player.playerData.ChangeHp(2+count);
            player.playerData.cardPoint -= count;
        }
    }
    [Server]
    public void nvwu(PlayerCore player, int choice, int target=1)
    {
        player.playerHand.ConfirmUse();
        // ѡ��1�������˺�
        if (choice == 1)
        {
            var nvwu1 = new damage(1, 3, "Ů��ҩ��");
            player.playerData.activeEffects.Add(nvwu1);
        }
        // ѡ��2�������ظ�
        else if (choice == 2)
        {
            var nvwu2 = new heal(2, 2, "Ů��ҩ��");
            player.playerData.activeEffects.Add(nvwu2);
        }
    }
    [Server]
    public void shop(PlayerCore player, int choice, int count=1)
    {
        player.playerHand.ConfirmUse();
        // ѡ��1��֧�������鿨
        if (choice == 1)
        {
            player.playerData.ChangeHp(-count*2);
            player.playerHand.ServerDrawCards(count);
        }
        // ѡ��2���������ƻ�Ѫ
        else if (choice == 2)
        {
            player.playerHand.ServerDiscardCards(count);
            player.playerData.ChangeHp(count);
        }
    }
    //----------����ѡ����-----------
    [Server]
    public void EXsword(PlayerCore player, PlayerCore target)
    {
        player.playerHand.ConfirmUse();
        target.playerData.HandleDamage(4);
    }
    [Server]
    public void laser(PlayerCore player, PlayerCore target)
    {
        player.playerHand.ConfirmUse();
        target.playerData.HandleDamage(3);
    }
    //----------ָ�������------------
    [Server]
    public void fight(PlayerCore player)
    {
        player.playerHand.ConfirmUse();
        //-----------------------------
    }
    [Server]
    public void link(PlayerCore player)
    {
        player.playerHand.ConfirmUse();
        //-----------------------------
    }
    [Server]
    public void moneng(PlayerCore player)
    {
        player.playerHand.ConfirmUse();
        //------------------------------
    }
    //-----------------������------------------------
    [Server]
    public void tuji()
    {
    }
    [Server]
    public void wuxie()
    {
    }
    [Server]
    public void yuntou()
    {
    }
    [Server]
    public void zuihou()
    {
    }
    //-----------------������------------------------
    [Server]
    public void atk1(PlayerCore player)
    {
        player.playerData.minAtk += 1;
        player.playerData.maxAtk += 2;
        player.playerHand.ConfirmUse();
        BattleManager.Instance.RpcUpdateBattleUI(true, player.playerData.minAtk, player.playerData.maxAtk, player.playerData.hp);
    }
    [Server]
    public void atk2(PlayerCore player)
    {
        player.playerData.minAtk += 1;
        player.playerData.maxAtk += 4;
        player.playerHand.ConfirmUse();
    }
    [Server]
    public void atk3(PlayerCore player)
    {
        player.playerData.minAtk += 1;
        player.playerData.maxAtk += 6;
        player.playerHand.ConfirmUse();
    }
    [Server]
    public void atk99(PlayerCore player)
    {
        player.playerData.minAtk += 99;
        player.playerData.maxAtk += 99;
        player.playerHand.ConfirmUse();
    }
    [Server]
    public void atkCoin(PlayerCore player,int atkUp)
    {
        player.playerData.minAtk += atkUp;
        player.playerData.maxAtk += atkUp;
        player.playerHand.ConfirmUse();
    }
    //-----------------������------------------------
    [Server]
    public void def1(PlayerCore player)
    {
        player.playerData.minDef += 1;
        player.playerData.maxDef += 2;
        player.playerHand.ConfirmUse();
    }
    [Server]
    public void def2(PlayerCore player)
    {
        player.playerData.minDef += 1;
        player.playerData.maxDef += 4;
        player.playerHand.ConfirmUse();
    }
    [Server]
    public void def3(PlayerCore player)
    {
        player.playerData.minDef += 1;
        player.playerData.maxDef += 6;
        player.playerHand.ConfirmUse();
    }
    [Server]
    public void defMiss(PlayerCore player)
    {
    }
    [Server]
    public void defReverser(PlayerCore player)
    {
    }
}