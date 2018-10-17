/// <summary>
/// Timer.cs
/// Copy from zhangyg 2014-10-22
/// 定时器管理
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

/// <summary>
/// 定时器管理
/// </summary>
public class Timer
{
    public delegate void TimerCallback();

    static int sTimerId = 0;
    static Dictionary<int, Timer> sTimers = new Dictionary<int, Timer>();

    /// <summary>
    /// 新建一个timer
    /// </summary>
    /// <param name="timeout">Timeout.</param>
    /// <param name="task">Task.</param>
    static public Timer New(float timeout, CallBack task, bool fixedMode = false)
    {
        if (sTimers.Count > 500)
        {
            LogMgr.Trace("Timer数量太多!");
        }

        // 确保ID > 1
        ++sTimerId;
        if (sTimerId < 1)
        {
            sTimerId = 1;
        }

        Timer t = new Timer(sTimerId);
        sTimers [sTimerId] = t;
        t.Start(timeout, task, fixedMode);
        return t;
    }

    /// <summary>
    /// 停止一个timer
    /// </summary>
    /// <param name="timerid">Timerid.</param>
    static public void Stop(int timerid)
    {
        Timer t;
        if (sTimers.TryGetValue(timerid, out t))
        {
            t.Stop();
        }
    }

    // 定时器ID
    int mTimerId = 0;
    public int TimerId { get { return mTimerId; } }

    // 定时器名字
    string mTimerName;

    /// 创建一个定时器，超时时间单位为秒
    Timer(int timerid)
    {
        mTimerId = timerid;
        mTimerName = "Timer_" + mTimerId;
    }

    // 定时器协程，会受到Time.scaleTime影响
    IEnumerator TimerCoroutine(float timeout, CallBack task)
    {
        yield return TimeMgr.WaitForCombatSeconds(timeout);
        if (task != null)
            task.Go();
        sTimers.Remove(mTimerId);
    }

    // 开始
    void Start(float timeout, CallBack task, bool fixedMode)
    {
        Coroutine.DispatchService(TimerCoroutine(timeout, task), mTimerName, fixedMode);
    }

    // 停止
    void Stop()
    {
        sTimers.Remove(mTimerId);
        Coroutine.StopCoroutine(mTimerName);
    }
}
