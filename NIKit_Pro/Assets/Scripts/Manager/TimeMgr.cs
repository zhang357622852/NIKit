/// <summary>
/// TimeMgr.cs
/// Create by zhaozy 2014-11-6
/// 时间管理模块
/// </summary>

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

/// 道具管理器
public static class TimeMgr
{
    #region 成员变量

    // 是否暂停中
    private static bool mPauseCombatLogic = false;

    // 上次暂停的时间
    private static int mLastPauseTick = Environment.TickCount;

    // 发起暂停列表
    private static Dictionary<string, int> mPauseTypeMap = new Dictionary<string, int>();

    // 时间起点距（1970.1.1的秒数）
    private static System.DateTime mTimeZone = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));

    /// <summary>
    /// 时间缩放比例倍速和缩放比例映射表
    /// </summary>
    private static Dictionary<int, float> scaleMap = new Dictionary<int, float>()
    {
        { 1, 1f },
        { 2, 1.25f },
        { 3, 1.5f },
    };

    private static float mTempTimeScale;

    // 缩放倍速max和min
    public static int MaxMultiple = 3;
    public static int MinMultiple = 1;

    #endregion

    #region 属性

    /// <summary>
    /// Gets a value indicating whether this <see cref="TimeMgr"/> time scale is dirty.
    /// </summary>
    /// <value><c>true</c> if time scale is dirty; otherwise, <c>false</c>.</value>
    public static bool TimeScaleIsDirty { get; private set; }

    /// <summary>
    /// Gets the final time scale is.
    /// </summary>
    /// <value>The final time scale is.</value>
    public static float FinalTimeScaleIs { get; private set; }

    /// <summary>
    /// 将Unix(1970.1.1 00:00:00)时间戳转换为DateTime类型时间
    /// </summary>
    public static System.DateTime ConvertIntDateTime(double d)
    {
        System.DateTime time = System.DateTime.MinValue;
        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
        time = startTime.AddSeconds(d);
        return time;
    }

    /// <summary>
    /// 将c# DateTime时间格式转换为Unix时间戳格式
    /// </summary>
    public static double ConvertDateTimeInt(System.DateTime time)
    {
        double intResult = 0;
        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
        intResult = (time - startTime).TotalSeconds;
        return intResult;
    }

    /// <summary>
    /// 战斗逻辑的暂停总时长
    /// </summary>
    /// <value>The pause total tick.</value>
    public static int CombatLogicPauseTotalTick { get; private set; }


    /// <summary>
    /// 真实毫秒值，来自系统启动时间
    /// </summary>
    /// <value>The real tick.</value>
    public static int RealTick
    {
        get { return Environment.TickCount; }
    }

    #endregion

    #region 内部函数

    /// <summary>
    /// 形如Unity的WaitForSeconds，但是不受Timer.scaleTime影响
    /// </summary>
    /// <returns>The for real seconds.</returns>
    /// <param name="timeout">Timeout.</param>
    static private IEnumerator YieldWaitForRealSeconds(float timeout)
    {
        float targetTime = Time.unscaledTime + timeout;
        while (targetTime > Time.unscaledTime)
            yield return null;
    }

    #endregion

    #region 功能接口

    /// <summary>
    /// Refreshs the time scale.
    /// </summary>
    public static void RefreshTimeScale()
    {
        // 如果当前处于Coroutine内部驱动中，不允许立即设置timeScale
        if (Coroutine.IsFixedModeInternal)
            return;

        // 如果时间缩放没有变化
        if (! TimeScaleIsDirty)
            return;

        // 直接修改
        Time.timeScale = FinalTimeScaleIs;
    }

    /// <summary>
    /// 形如Unity的WaitForSeconds，但是不受暂停和Timer.scaleTime影响
    /// 本质是以下语句的简化：
    /// Coroutine.DispatchService(TimeMgr.WaitForRealSeconds(t));
    /// 外部用法：
    /// yield return WaitForRealSeconds(t);
    /// </summary>
    /// <param name="timeout">Timeout.</param>
    public static Coroutine.Proxy WaitForRealSeconds(float timeout)
    {
        return Coroutine.DispatchService(YieldWaitForRealSeconds(timeout));
    }

    /// <summary>
    /// 等待t秒，会受到暂停和Timer.scaleTime影响
    /// </summary>
    /// <returns>The for combat secondss.</returns>
    /// <param name="timeout">Timeout.</param>
    public static WaitForSeconds WaitForCombatSeconds(float timeout)
    {
        return new WaitForSeconds(timeout);
    }

    /// <summary>
    /// 判断两个时间是否同一天
    /// </summary>
    public static bool IsSameDay(int ti1, int ti2)
    {
        // 时间相差大于1天
        if (Math.Abs(ti1 - ti2) > 86400)
            return false;

        DateTime time1 = ConvertIntDateTime(ti1);
        DateTime time2 = ConvertIntDateTime(ti2);

        // 返回Day是否一致
        return time1.ToLongDateString() == time2.ToLongDateString();
    }

    /// <summary>
    ///转换时间
    /// </summary>
    public static string ConvertTime(int second, bool isHour = false, bool isSecond = true)
    {
        string timeDesc = string.Empty;

        int hour = 0;

        int min = 0;

        if (isHour)
        {
            hour = second / 3600;

            min = (second - hour * 3600) / 60;
        }
        else
        {
            min = second / 60;
        }

        int sec = second - hour * 3600 - min * 60;

        if (isHour)
        {
            if (hour >= 10)
                timeDesc = string.Format("{0}:{1}", hour, timeDesc);
            else
                timeDesc = string.Format("0{0}:{1}", hour, timeDesc);
        }

        if (min >= 10)
            timeDesc = string.Format("{0}{1}:", timeDesc, min);
        else
            timeDesc = string.Format("{0}0{1}:", timeDesc, min);

        // 去掉末尾的冒号
        if (!isSecond)
            return timeDesc.Substring(0, timeDesc.Length - 1);

        if (sec >= 10)
            timeDesc = string.Format("{0}{1}", timeDesc, sec);
        else
            timeDesc = string.Format("{0}0{1}", timeDesc, sec);

        return timeDesc;
    }

    /// <summary>
    /// 根据格式话时间格式(2018-05-10 10:00:00)
    /// </summary>
    public static DateTime ConvertDateTime(string timeFormat)
    {
        // 转换时间
        DateTime dt = DateTime.Parse(timeFormat);

        // 返回时间
        return dt;
    }

    /// <summary>
    /// 获取时间格式
    /// yyyy 包括纪元的四位数的年份
    /// MM 月份数字,一位数的月份有一个前导零
    /// dd 月中的某一天,一位数的日期有一个前导零
    /// HH : 24小时制
    /// hh : 12小时制
    /// mm : 分钟
    /// ss : 秒
    /// </summary>
    /// <param name="format">"yyyy-MM-dd HH:mm:ss"</param>
    public static string GetTimeFormat(int sec, string format)
    {
        DateTime dt = TimeMgr.ConvertIntDateTime(sec);

        return dt.ToString(format);
    }

    /// <summary>
    /// 获取时间详细数据
    /// year 包括纪元的四位数的年份
    /// month 月份数字,一位数的月份有一个前导零
    /// day 月中的某一天,一位数的日期有一个前导零
    /// hour : 24小时制
    /// minute : 分钟
    /// Second : 秒
    /// </summary>
    public static LPCMapping GetTimeDetail(int sec, bool show24)
    {
        // 转换为DateTime
        DateTime dt = TimeMgr.ConvertIntDateTime(sec);
        LPCMapping data = LPCMapping.Empty;

        // 2018-10-12 24:00:00
        // 不需要修正向下修正格式化后2018-10-13 00:00:00
        // 向下修正后显示2018-10-12 24:00:00
        if (! show24)
        {
            data.Add("year", dt.Year);
            data.Add("month", dt.Month);
            data.Add("day", dt.Day);
            data.Add("hour", dt.Hour);
            data.Add("minute", dt.Minute);
            data.Add("Second", dt.Second);
            return data;
        }

        // 如果时间不是00:00:00, 直接返回当前时间
        if (dt.Hour != 0 || dt.Minute != 0 || dt.Second != 0)
        {
            data.Add("year", dt.Year);
            data.Add("month", dt.Month);
            data.Add("day", dt.Day);
            data.Add("hour", dt.Hour);
            data.Add("minute", dt.Minute);
            data.Add("Second", dt.Second);
            return data;
        }

        // 转换为DateTime，在原来时间减1秒让时间推移到上一天
        DateTime dt1 = TimeMgr.ConvertIntDateTime(sec - 1);

        // 构建数据
        data.Add("year", dt1.Year);
        data.Add("month", dt1.Month);
        data.Add("day", dt1.Day);
        data.Add("hour", 24);
        data.Add("minute", 0);
        data.Add("Second", 0);

        // 返回格式化后数据
        return data;
    }

    #endregion
}
