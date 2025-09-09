using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectStepsUI : MonoBehaviour
{
    public Button[] stepButtons = new Button[6];
    public TextMeshProUGUI title;
    private PlayerCore player;
    // 健步如飞的选择面板
    public void FreeChoiceStep(PlayerCore player)
    {
        title.text = "请选择步数";
        gameObject.SetActive(true);
        this.player = player;
        // 步数选择
        for (int i = 0; i < 6; i++)
        {
            int step = i + 1; // 防闭包问题
            stepButtons[i].onClick.RemoveAllListeners();
            stepButtons[i].onClick.AddListener(() =>
            {
                Debug.Log($"选择了:{i + 1}");
                SelectStep(step);
            });
        }
    }
    private void SelectStep(int steps)
    {
        // 隐藏选择UI
        gameObject.SetActive(false);
        // 执行移动逻辑
        player.playerMove.CmdMovePlayer(steps);
    }
    // 聚财一击的选择面板
    public void AtkChoiceIndex(PlayerCore player)
    {
        title.text = "请选择增加的攻击";
        gameObject.SetActive(true);
        this.player = player;
        // 攻击选择
        for (int i = 0; i < 6; i++)
        {
            int index = i + 1; // 防闭包问题
            stepButtons[i].interactable = (player.playerData.coin >= i * 5);
            stepButtons[i].onClick.RemoveAllListeners();
            stepButtons[i].onClick.AddListener(() =>
            {
                Debug.Log($"选择了:{i + 1}");
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
