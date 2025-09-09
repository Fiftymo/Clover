using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RoleData
{
    public int roleId;          // 角色唯一ID
    public string roleName;     // 角色名称
    public Sprite roleImage;    // 角色头像（用于UI）
    public int hp;              // 生命值
    public int atk;             // 攻击力
    public int def;             // 防御力
}

public class RoleManager : MonoBehaviour
{
    public static RoleManager Instance; // 单例实例

    [Header("角色数据列表")]
    public List<RoleData> roleList = new List<RoleData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // 初始化单例
            DontDestroyOnLoad(gameObject); // 跨场景保留
        }
        else
        {
            Destroy(gameObject); // 防止重复实例
        }
    }
}