using UnityEngine;

public abstract class TimedEffect
{
    public int duration; // ʣ��غ���
    public string sourceCardName;

    public TimedEffect(int duration, string sourceCardName)
    {
        this.duration = duration;
        this.sourceCardName = sourceCardName;
    }
    public abstract void Apply(PlayerData player); // ÿ�غϴ�����Ч��
    public virtual void OnAdded(PlayerData player) { }  // ���ʱ����
    public virtual void OnExpire(PlayerData player) { } // ����ʱ����
}
// �����˺�
public class damage : TimedEffect
{
    public int damagePerTurn;

    public damage(int duration, int damagePerTurn, string sourceCardName) :  base(duration, sourceCardName)
    {
        this.damagePerTurn = damagePerTurn;
    }
    public override void Apply(PlayerData player)
    {
        player.HandleDamage(damagePerTurn);
    }
}
// ��������
public class heal : TimedEffect
{
    public int healPerTurn;

    public heal(int duration, int healPerTurn, string sourceCardName) :  base(duration, sourceCardName)
    {
        this.healPerTurn = healPerTurn;
    }
    public override void Apply(PlayerData player)
    {
        player.ChangeHp(healPerTurn);
    }
}
// ħ�ܽ��
public class moneng : TimedEffect
{
    public int atk = 3;
    public int hurt = 1;

    public moneng(int duration, int atk, int hurt, string sourceCardName) :  base(duration, sourceCardName)
    {
        this.atk = atk;
    }
    public override void Apply(PlayerData player)
    {
        Debug.Log($"ħ�ܽ�Ż��� {duration} �غ�");
    }
    public override void OnAdded(PlayerData player)
    {
        player.ChangeAtk(atk);
        //player.ChangeHurt(hurt);
    }
    public override void OnExpire(PlayerData player)
    {
        player.ChangeAtk(-atk);
        //player.ChangeHurt(-hurt);
    }
}
/*
public class atkUp :  TimedEffect
{ 
    public int atk;

    public atkUp(int duration, int atk, string sourceCardName) :  base(duration, sourceCardName)
    {
        this.atk = atk;
    }
    public override void OnAdded(PlayerData player)
    {
        player.ChangeAtk(atk);
    }
}
*/
