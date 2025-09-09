using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class RoleButton : MonoBehaviour
{
    [Header("角色属性")]
    public int roleId;
    public string roleName;
    public Sprite roleImage;  // 角色图片
    public Sprite roleAvatar; // 角色头像

    [Header("UI组件")]
    public Button button;

    private void Start()
    {
        // 自动绑定点击事件
        button.onClick.AddListener(OnButtonClicked);
    }

    public void OnButtonClicked()
    {
        // 通知角色管理器选择此角色
        RoleSelectionManager.Instance.SelectRole(roleId);
    }
}