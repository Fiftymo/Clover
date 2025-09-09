using UnityEngine;

public abstract class TimedEffect
{
    public int duration; // 剩余回合数
    public string sourceCardName;

    public TimedEffect(int duration, string sourceCardName)
    {
        this.duration = duration;
        this.sourceCardName = sourceCardName;
    }
    public abstract void Apply(PlayerData player); // 每回合触发的效果
    public virtual void OnAdded(PlayerData player) { }  // 添加时触发
    public virtual void OnExpire(PlayerData player) { } // 结束时触发
}
// 持续伤害
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
// 持续治疗
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
// 魔能解放
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
        Debug.Log($"魔能解放还有 {duration} 回合");
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
