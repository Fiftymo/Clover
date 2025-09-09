using UnityEngine;
using UnityEngine.Events;
using System.Collections;

// ����ʱ����¼�
[System.Serializable]
public class TimerCompleteEvent : UnityEvent { }

// ����ʱ�����¼�������ʣ��ʱ�䣩
[System.Serializable]
public class TimerUpdateEvent : UnityEvent<float> { }

public class CountdownTimer : MonoBehaviour
{
    [Header("��������")]
    [Tooltip("����ʱ����ʱ�䣨�룩")]
    public float duration = 30f;

    [Tooltip("�Ƿ��Զ���ʼ")]
    public bool autoStart = false;

    [Header("�¼�")]
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

    // ��ʼ����ʱ
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

    // ��ͣ����ʱ
    public void PauseTimer()
    {
        if (isRunning && !isPaused)
        {
            isPaused = true;
        }
    }

    // ��������ʱ
    public void ResumeTimer()
    {
        if (isRunning && isPaused)
        {
            isPaused = false;
        }
    }

    // ֹͣ����ʱ
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

    // ���õ���ʱ
    public void ResetTimer()
    {
        StopTimer();
        remainingTime = duration;
    }

    // ���ʱ��
    public void AddTime(float seconds)
    {
        remainingTime += seconds;
    }

    // ����ʱ��
    public void SubtractTime(float seconds)
    {
        remainingTime = Mathf.Max(0, remainingTime - seconds);
    }

    // ����ʱЭ��
    private IEnumerator TimerRoutine()
    {
        while (remainingTime > 0)
        {
            if (!isPaused)
            {
                // ����ʣ��ʱ��
                remainingTime -= Time.deltaTime;

                // ���������¼�
                onTimerUpdate?.Invoke(remainingTime);
            }

            yield return null;
        }

        // ����ʱ����
        remainingTime = 0;
        onTimerUpdate?.Invoke(0);
        onTimerComplete?.Invoke();

        isRunning = false;
        timerCoroutine = null;
    }
}