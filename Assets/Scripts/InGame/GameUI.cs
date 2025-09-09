using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class GameUI : NetworkBehaviour
{
    [Header("玩家面板")]
    public GameObject playerPanel1;
    public GameObject playerPanel2;
    public GameObject playerPanel3;
    public GameObject playerPanel4;

    public static GameUI Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        // 绑定RoomInstance的玩家数据
        RoomInstance room = NetworkClient.spawned.Values
            .FirstOrDefault(obj => obj.GetComponent<RoomInstance>() != null)?
            .GetComponent<RoomInstance>();

        if (room != null)
        {
            UpdateAllPlayerPanels(room);
        }
    }

    public void UpdateAllPlayerPanels(RoomInstance room)
    {
        foreach (var player in room.players)
        {
            GameObject panel = GetPanelBySlotIndex(player.slotIndex);
            if (panel == null) continue;

            // 绑定基础数据
            panel.transform.Find("Text/Name").GetComponent<TMP_Text>().text = player.name;
            panel.transform.Find("Text/Coin").GetComponent<TMP_Text>().text = player.coin.ToString();
            panel.transform.Find("Text/LV").GetComponent<TMP_Text>().text = player.level.ToString();

            // 获取角色属性
            RoleData role = RoleManager.Instance.roleList.Find(r => r.roleId == player.roleId);
            if (role != null)
            {
                panel.transform.Find("Text/HP/MaxHP").GetComponent<TMP_Text>().text = role.hp.ToString();
                panel.transform.Find("Text/HP").GetComponent<TMP_Text>().text = role.hp.ToString();
                panel.transform.Find("Text/ATK").GetComponent<TMP_Text>().text = role.atk.ToString();
                panel.transform.Find("Text/DEF").GetComponent<TMP_Text>().text = role.def.ToString();
                panel.transform.Find("AvatarButton").GetComponent<Image>().sprite = role.roleImage;
            }
        }
    }

    private GameObject GetPanelBySlotIndex(int slotIndex)
    {
        return slotIndex switch
        {
            1 => playerPanel1,
            2 => playerPanel2,
            3 => playerPanel3,
            4 => playerPanel4,
            _ => null
        };
    }
    [ClientRpc]
    public void RpcUpdatePlayerPanel(int index, int coin, int level, int hp, int atk, int def)
    {
        GameObject panel = GetPanelBySlotIndex(index);
        if (panel == null) Debug.Log($"panel{index} is null!");
        Debug.Log($"update player{index}");
        panel.transform.Find("Text/Coin").GetComponent<TMP_Text>().text = $"{coin}";
        panel.transform.Find("Text/LV").GetComponent<TMP_Text>().text = level.ToString();
        panel.transform.Find("Text/HP").GetComponent<TMP_Text>().text = hp.ToString();
        panel.transform.Find("Text/ATK").GetComponent<TMP_Text>().text = atk.ToString();
        panel.transform.Find("Text/DEF").GetComponent<TMP_Text>().text = def.ToString();
    }
}