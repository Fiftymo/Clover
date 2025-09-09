using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CardChoicePanelUI : MonoBehaviour
{
    [Header("UI 元素")]
    public TextMeshProUGUI promptText;         // 提示文本
    public Button[] optionButtons;             // 选项按钮数组
    public Button cancelButton;                // 取消按钮
    public TMP_InputField inputField;          // 值输入框
    [Header("输入框属性")]
    public int inputValue = 0;
    public int minValue = 0;
    public int maxValue = 4;

    private PlayerCore player;

    public void SetOptions(PlayerCore player, string prompt, string[] options,
        Action[] callbacks, bool isNeedInput, string inputPrompt = "",
        int max = 4, int min = 0)
    {
        gameObject.SetActive(true);
        this.player = player;
        promptText.text = prompt;
        if (isNeedInput)
        {
            inputField.gameObject.SetActive(true);
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputField.text = minValue.ToString(); // 默认值
            inputField.placeholder.GetComponent<TextMeshProUGUI>().text = inputPrompt;

            inputField.onValueChanged.RemoveAllListeners();
            inputField.onValueChanged.AddListener(ValidateInput);

            maxValue = max;
            ValidateInput(inputField.text);
        }
        else
        {
            inputField.gameObject.SetActive(false);
            inputValue = 1; // 重置
        }
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < options.Length)
            {
                optionButtons[i].gameObject.SetActive(true);
                optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = options[i];

                int index = i; // 防闭包问题
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() =>
                {
                    Debug.Log($"点击了选项:{index+1}, input ={inputValue}");
                    if (isNeedInput && inputValue < 0) return; // 阻止继续
                    callbacks[index]?.Invoke();
                });
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }
    // 输入校验
    private void ValidateInput(string input)
    {
        if (int.TryParse(input, out int val))
        {
            val = Mathf.Clamp(val, minValue, maxValue);
            inputValue = val;
            inputField.text = val.ToString(); // 强制刷新显示
        }
        else
        {
            inputValue = 1;
            inputField.text = ""; // 清空非法输入
        }
        player.playerUI.count = inputValue;
    }
    public void ResetInput(int choice) // 低价购物的重置输入限制
    {
        switch (choice)
        {
            case 1: // 扣血抽卡：每2血抽1张，HP 至少留1点
                int maxByHp = Mathf.Min((player.playerData.hp - 1) / 2, 4);
                maxValue = Mathf.Max(1, maxByHp);
                break;

            case 2: // 丢牌回血：每张牌回1血，不能超过当前手牌数，最多4
                int maxByHand = Mathf.Min(player.playerHand.handCount - 1, 4);
                maxValue = Mathf.Max(1, maxByHand);
                break;
        }

        Debug.Log($"设置 maxValue = {maxValue}");

        // 重设 inputField 的值和限制
        inputValue = Mathf.Clamp(inputValue, minValue, maxValue);
        inputField.text = inputValue.ToString();
    }
    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
