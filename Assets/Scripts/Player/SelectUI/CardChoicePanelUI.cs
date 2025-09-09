using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CardChoicePanelUI : MonoBehaviour
{
    [Header("UI Ԫ��")]
    public TextMeshProUGUI promptText;         // ��ʾ�ı�
    public Button[] optionButtons;             // ѡ�ť����
    public Button cancelButton;                // ȡ����ť
    public TMP_InputField inputField;          // ֵ�����
    [Header("���������")]
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
            inputField.text = minValue.ToString(); // Ĭ��ֵ
            inputField.placeholder.GetComponent<TextMeshProUGUI>().text = inputPrompt;

            inputField.onValueChanged.RemoveAllListeners();
            inputField.onValueChanged.AddListener(ValidateInput);

            maxValue = max;
            ValidateInput(inputField.text);
        }
        else
        {
            inputField.gameObject.SetActive(false);
            inputValue = 1; // ����
        }
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < options.Length)
            {
                optionButtons[i].gameObject.SetActive(true);
                optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = options[i];

                int index = i; // ���հ�����
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() =>
                {
                    Debug.Log($"�����ѡ��:{index+1}, input ={inputValue}");
                    if (isNeedInput && inputValue < 0) return; // ��ֹ����
                    callbacks[index]?.Invoke();
                });
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }
    // ����У��
    private void ValidateInput(string input)
    {
        if (int.TryParse(input, out int val))
        {
            val = Mathf.Clamp(val, minValue, maxValue);
            inputValue = val;
            inputField.text = val.ToString(); // ǿ��ˢ����ʾ
        }
        else
        {
            inputValue = 1;
            inputField.text = ""; // ��շǷ�����
        }
        player.playerUI.count = inputValue;
    }
    public void ResetInput(int choice) // �ͼ۹����������������
    {
        switch (choice)
        {
            case 1: // ��Ѫ�鿨��ÿ2Ѫ��1�ţ�HP ������1��
                int maxByHp = Mathf.Min((player.playerData.hp - 1) / 2, 4);
                maxValue = Mathf.Max(1, maxByHp);
                break;

            case 2: // ���ƻ�Ѫ��ÿ���ƻ�1Ѫ�����ܳ�����ǰ�����������4
                int maxByHand = Mathf.Min(player.playerHand.handCount - 1, 4);
                maxValue = Mathf.Max(1, maxByHand);
                break;
        }

        Debug.Log($"���� maxValue = {maxValue}");

        // ���� inputField ��ֵ������
        inputValue = Mathf.Clamp(inputValue, minValue, maxValue);
        inputField.text = inputValue.ToString();
    }
    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
