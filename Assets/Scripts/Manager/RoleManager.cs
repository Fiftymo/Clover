using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RoleData
{
    public int roleId;          // ��ɫΨһID
    public string roleName;     // ��ɫ����
    public Sprite roleImage;    // ��ɫͷ������UI��
    public int hp;              // ����ֵ
    public int atk;             // ������
    public int def;             // ������
}

public class RoleManager : MonoBehaviour
{
    public static RoleManager Instance; // ����ʵ��

    [Header("��ɫ�����б�")]
    public List<RoleData> roleList = new List<RoleData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // ��ʼ������
            DontDestroyOnLoad(gameObject); // �糡������
        }
        else
        {
            Destroy(gameObject); // ��ֹ�ظ�ʵ��
        }
    }
}