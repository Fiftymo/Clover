using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class RoleButton : MonoBehaviour
{
    [Header("��ɫ����")]
    public int roleId;
    public string roleName;
    public Sprite roleImage;  // ��ɫͼƬ
    public Sprite roleAvatar; // ��ɫͷ��

    [Header("UI���")]
    public Button button;

    private void Start()
    {
        // �Զ��󶨵���¼�
        button.onClick.AddListener(OnButtonClicked);
    }

    public void OnButtonClicked()
    {
        // ֪ͨ��ɫ������ѡ��˽�ɫ
        RoleSelectionManager.Instance.SelectRole(roleId);
    }
}