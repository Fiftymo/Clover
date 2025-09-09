using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountdownUIController : MonoBehaviour
{
    [Header("UI 引用")]
    public TextMeshProUGUI countdownText;
    public Image countdownBar;

    [Header("视觉设置")]
    [Tooltip("警告时间（秒）")]
    public float warningThreshold = 5f;

    [Tooltip("正常时间颜色")]
    public Color normalColor = Color.white;

    [Tooltip("警告时间颜色")]
    public Color warningColor = Color.yellow;

    // 绑定到倒计时器
    public void BindToTimer(CountdownTimer timer)
    {
        // 清除之前的监听
        timer.onTimerUpdate.RemoveAllListeners();
        timer.onTimerComplete.RemoveAllListeners();

        // 添加新的监听
        timer.onTimerUpdate.AddListener(UpdateTimerUI);
        timer.onTimerComplete.AddListener(OnTimerComplete);

        // 初始更新
        UpdateTimerUI(timer.RemainingTime);
    }

    // 更新UI
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

        // 警告状态处理
        bool showWarning = remainingTime <= warningThreshold;

        if (countdownText != null)
        {
            countdownText.color = showWarning ? warningColor : normalColor;
        }
    }

    // 倒计时结束处理
    private void OnTimerComplete()
    {
        // 可以添加结束动画
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