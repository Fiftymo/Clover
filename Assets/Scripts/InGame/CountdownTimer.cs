using UnityEngine;
using UnityEngine.Events;
using System.Collections;

// 倒计时完成事件
[System.Serializable]
public class TimerCompleteEvent : UnityEvent { }

// 倒计时更新事件（传递剩余时间）
[System.Serializable]
public class TimerUpdateEvent : UnityEvent<float> { }

public class CountdownTimer : MonoBehaviour
{
    [Header("基本设置")]
    [Tooltip("倒计时持续时间（秒）")]
    public float duration = 30f;

    [Tooltip("是否自动开始")]
    public bool autoStart = false;

    [Header("事件")]
    public TimerCompleteEvent onTimerComplete;
    public TimerUpdateEvent onTimerUpdate;

    private Coroutine timerCoroutine;
    private float remainingTime;
    private bool isRunning = false;
    private bool isPaused = false;

    public float RemainingTime => remainingTime;
    public bool IsRunning => isRunning;
    public bool IsPaused => isPaused;

    private void Start()
    {
        if (autoStart)
        {
            StartTimer();
        }
    }

    // 开始倒计时
    public void StartTimer(float? customDuration = null)
    {
        if (customDuration.HasValue)
        {
            duration = customDuration.Value;
        }

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }

        remainingTime = duration;
        isRunning = true;
        isPaused = false;

        timerCoroutine = StartCoroutine(TimerRoutine());
    }

    // 暂停倒计时
    public void PauseTimer()
    {
        if (isRunning && !isPaused)
        {
            isPaused = true;
        }
    }

    // 继续倒计时
    public void ResumeTimer()
    {
        if (isRunning && isPaused)
        {
            isPaused = false;
        }
    }

    // 停止倒计时
    public void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        isRunning = false;
        isPaused = false;
    }

    // 重置倒计时
    public void ResetTimer()
    {
        StopTimer();
        remainingTime = duration;
    }

    // 添加时间
    public void AddTime(float seconds)
    {
        remainingTime += seconds;
    }

    // 减少时间
    public void SubtractTime(float seconds)
    {
        remainingTime = Mathf.Max(0, remainingTime - seconds);
    }

    // 倒计时协程
    private IEnumerator TimerRoutine()
    {
        while (remainingTime > 0)
        {
            if (!isPaused)
            {
                // 更新剩余时间
                remainingTime -= Time.deltaTime;

                // 触发更新事件
                onTimerUpdate?.Invoke(remainingTime);
            }

            yield return null;
        }

        // 倒计时结束
        remainingTime = 0;
        onTimerUpdate?.Invoke(0);
        onTimerComplete?.Invoke();

        isRunning = false;
        timerCoroutine = null;
    }
}