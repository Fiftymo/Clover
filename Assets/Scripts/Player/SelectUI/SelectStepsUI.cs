using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectStepsUI : MonoBehaviour
{
    public Button[] stepButtons = new Button[6];
    public TextMeshProUGUI title;
    private PlayerCore player;
    // ������ɵ�ѡ�����
    public void FreeChoiceStep(PlayerCore player)
    {
        title.text = "��ѡ����";
        gameObject.SetActive(true);
        this.player = player;
        // ����ѡ��
        for (int i = 0; i < 6; i++)
        {
            int step = i + 1; // ���հ�����
            stepButtons[i].onClick.RemoveAllListeners();
            stepButtons[i].onClick.AddListener(() =>
            {
                Debug.Log($"ѡ����:{i + 1}");
                SelectStep(step);
            });
        }
    }
    private void SelectStep(int steps)
    {
        // ����ѡ��UI
        gameObject.SetActive(false);
        // ִ���ƶ��߼�
        player.playerMove.CmdMovePlayer(steps);
    }
    // �۲�һ����ѡ�����
    public void AtkChoiceIndex(PlayerCore player)
    {
        title.text = "��ѡ�����ӵĹ���";
        gameObject.SetActive(true);
        this.player = player;
        // ����ѡ��
        for (int i = 0; i < 6; i++)
        {
            int index = i + 1; // ���հ�����
            stepButtons[i].interactable = (player.playerData.coin >= i * 5);
            stepButtons[i].onClick.RemoveAllListeners();
            stepButtons[i].onClick.AddListener(() =>
            {
                Debug.Log($"ѡ����:{i + 1}");
                SelectAtk(index);
            });
        }
    }
    private void SelectAtk(int atkValue)
    {
        gameObject.SetActive(false);
        CardEffectManager.Instance.atkCoin(player, atkValue);
    }
}
