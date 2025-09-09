using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountdownUIController : MonoBehaviour
{
    [Header("UI ����")]
    public TextMeshProUGUI countdownText;
    public Image countdownBar;

    [Header("�Ӿ�����")]
    [Tooltip("����ʱ�䣨�룩")]
    public float warningThreshold = 5f;

    [Tooltip("����ʱ����ɫ")]
    public Color normalColor = Color.white;

    [Tooltip("����ʱ����ɫ")]
    public Color warningColor = Color.yellow;

    // �󶨵�����ʱ��
    public void BindToTimer(CountdownTimer timer)
    {
        // ���֮ǰ�ļ���
        timer.onTimerUpdate.RemoveAllListeners();
        timer.onTimerComplete.RemoveAllListeners();

        // ����µļ���
        timer.onTimerUpdate.AddListener(UpdateTimerUI);
        timer.onTimerComplete.AddListener(OnTimerComplete);

        // ��ʼ����
        UpdateTimerUI(timer.RemainingTime);
    }

    // ����UI
    private void UpdateTimerUI(float remainingTime)
    {
        if (countdownText != null)
        {
            countdownText.text = Mathf.CeilToInt(remainingTime).ToString();
        }

        if (countdownBar != null)
        {
            countdownBar.fillAmount = remainingTime / GetComponentInParent<CountdownTimer>().duration;
        }

        // ����״̬����
        bool showWarning = remainingTime <= warningThreshold;

        if (countdownText != null)
        {
            countdownText.color = showWarning ? warningColor : normalColor;
        }
    }

    // ����ʱ��������
    private void OnTimerComplete()
    {
        // ������ӽ�������
        if (countdownText != null)
        {
            countdownText.text = "0";
        }

        if (countdownBar != null)
        {
            countdownBar.fillAmount = 0;
        }
    }
}