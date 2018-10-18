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
    /// Gets the scale.
    /// </summary>
    /// <returns>The scale.</returns>
    /// <param name="scale">Scale.</param>
    public static float GetScale(int multiple)
    {
#if UNITY_EDITOR
        if (ME.user != null)
        {
            LPCValue timeScale = ME.user.QueryTemp<LPCValue>("time_scale");
            if (timeScale != null)
                return (float) timeScale.AsInt;
        }
#endif

        // 不包含的数据
        if (!scaleMap.ContainsKey(multiple))
            return scaleMap[MinMultiple];

        // 返回配置信息
        return scaleMap[multiple];
    }

    /// <summary>
    /// Gets the scale.
    /// </summary>
    /// <returns>The scale.</returns>
    public static int GetScaleMultiple(bool isLoopFight)
    {
        float scale = 0f;
        if (isLoopFight)
            scale = TempTimeScale;
        else
            scale = TimeScale;

        // 遍历所有数据
        foreach (int key in scaleMap.Keys)
        {
            // 数据不相等
            if (!Game.FloatEqual(scaleMap[key], scale))
                continue;

            // 返回数据
            return key;
        }

        // 默认为1倍速度
        return MinMultiple;
    }

    /// <summary>
    /// 游戏暂停
    /// 影响战斗相关逻辑
    /// </summary>
    public static bool PauseCombatLogic
    {
        get { return mPauseCombatLogic; }

        private set
        {
            // 变化的时候发下一帧送异步事件
            if (mPauseCombatLogic != value)
                EventMgr.FireEvent(EventMgrEventType.EVENT_COMBAT_PAUSE_CHANGED, MixedValue.NewMixedValue<bool>(value));

            mPauseCombatLogic = value;
        }
    }

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
    /// 战斗计时的毫秒值，用于战斗逻辑计时(技能cd，状态时长)
    /// 会受PauseCombatTime影响
    /// </summary>
    /// <value>The tick.</value>
    public static int CombatTick
    {
        get
        {
            if (PauseCombatLogic)
                return mLastPauseTick - CombatLogicPauseTotalTick;
            else
                return Environment.TickCount - CombatLogicPauseTotalTick;
        }
    }

    /// <summary>
    /// 真实毫秒值，来自系统启动时间
    /// </summary>
    /// <value>The real tick.</value>
    public static int RealTick
    {
        get { return Environment.TickCount; }
    }

    /// <summary>
    /// 设置时间的缩放系数
    /// </summary>
    public static float TimeScale
    {
        get
        {
            // 如果玩家对象不存在，默认返回1倍速度
            if (ME.user == null)
                return GetScale(1);

            // 没有获取到Option设置信息
            LPCValue timeScaleOption = OptionMgr.GetOption(ME.user, "time_scale");
            if (timeScaleOption == null)
                return GetScale(1);

            // 获取配置信息
            return timeScaleOption.AsFloat;
        }

        set
        {
            // 玩家对象不存在，不允许设置速度缩放
            if (ME.user == null)
                return;

            // 保存本地设置
            OptionMgr.SetOption(ME.user, "time_scale", LPCValue.Create(value));

            // 设置时间缩放
            SetTimeScale(value);
        }
    }

    /// <summary>
    /// 设置临时的事件缩放，不保存到本地
    /// </summary>
    public static float TempTimeScale
    {
        get
        {
            // 如果玩家对象不存在，默认返回1倍速度
            if (ME.user == null)
                return GetScale(1);

            // 获取配置信息
            return mTempTimeScale;
        }

        set
        {
            // 玩家对象不存在，不允许设置速度缩放
            if (ME.user == null)
                return;

            // 设置时间缩放
            SetTimeScale(value);

            mTempTimeScale = value;
        }
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

    /// <summary>
    /// Sets the time scale.
    /// </summary>
    /// <param name="scale">Scale.</param>
    private static void SetTimeScale(float scale)
    {
#if UNITY_EDITOR
        if (ME.user != null)
        {
            LPCValue timeScale = ME.user.QueryTemp<LPCValue>("time_scale");
            if (timeScale != null)
                scale = timeScale.AsInt;
        }
#endif

        // 如果当前处于Coroutine内部驱动中，不允许立即设置timeScale
        // 需要等到Coroutine一次驱动结束后再设置timeScale
        // 设置时间缩放
        if (! Coroutine.IsFixedModeInternal)
        {
            Time.timeScale = scale;
            return;
        }

        // Time.timeScale已经修改变化，等到Coroutine内部驱动结束
        TimeScaleIsDirty = true;
        FinalTimeScaleIs = scale;
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
    /// 游戏暂停，影响战斗相关逻辑
    /// </summary>
    public static void DoPauseCombatLogic(string type)
    {
        // 已经包含了该类型的暂停不处理
        if (mPauseTypeMap.ContainsKey(type))
            return;

        // 记录暂停类型列表
        mPauseTypeMap.Add(type, 1);

        // 已经是暂停状态
        if (PauseCombatLogic)
            return;

        // 打上暂停标识
        PauseCombatLogic = true;

        // 调整逻辑驱动时间
        SetTimeScale(0.0f);
        mLastPauseTick = Environment.TickCount;
    }

    /// <summary>
    /// 游戏继续，影响战斗相关逻辑
    /// </summary>
    public static void DoContinueCombatLogic(string type)
    {
        // 没有该类型的暂停标识
        if (!mPauseTypeMap.ContainsKey(type))
            return;

        // 记录暂停类型列表
        mPauseTypeMap.Remove(type);

        // 还有其他暂停
        if (mPauseTypeMap.Count > 0)
            return;

        // 不是暂停状态不处理
        if (!PauseCombatLogic)
            return;

        // 打上暂停标识
        PauseCombatLogic = false;

        // 调整逻辑驱动时间
        SetTimeScale(InstanceMgr.IsInInstance(ME.user) ? TimeScale : 1f);
        CombatLogicPauseTotalTick += Environment.TickCount - mLastPauseTick;
    }

    /// <summary>
    /// 游戏暂停重置，影响战斗相关逻辑
    /// </summary>
    public static void DoInitCombatLogic(float scale)
    {
        // 清除暂停类型列表
        mPauseTypeMap.Clear();

        // 打上暂停标识
        PauseCombatLogic = false;

        // 调整逻辑驱动时间
        SetTimeScale(scale);
        CombatLogicPauseTotalTick = 0;
    }

    /// <summary>
    /// 获取当前的时间
    /// 功能为实现，暂时简单处理，后续如果考虑对时机制在详细处理
    /// </summary>
    public static int GetTime()
    {
        // 返回距1970.1.1的秒数
        return (int)(System.DateTime.Now - mTimeZone).TotalSeconds;
    }

    /// <summary>
    /// 获取服务器当前时间戳
    /// </summary>
    public static int GetServerTime()
    {
        // 返回服务器时间
        return Communicate.ServerTime;
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
    /// 显示距离当前服务器时间的简要时间描述
    /// 规则:小于1分钟显示：1分钟前,大于1分钟显示：X分钟前.......大于则为后
    /// </summary>
    /// <returns>The time to chinese.</returns>
    public static string ConvertTimeToSimpleChinese(int time)
    {
        string timeDesc = string.Empty;
        int second = GetServerTime() - time;

        string simpleDesc = string.Empty;

        if(second >= 0)
            simpleDesc = LocalizationMgr.Get("TimeMgr_5");
        else
            simpleDesc = LocalizationMgr.Get("TimeMgr_6");

        second = System.Math.Abs(second);

        int day = second / 86400;
        int hour = second / 3600;
        int min = second / 60;

        if (day > 0)
            timeDesc = string.Format("{0}{1}", day, LocalizationMgr.Get("TimeMgr_1"));
        else if(hour >0)
            timeDesc = string.Format("{0}{1}", hour, LocalizationMgr.Get("TimeMgr_2"));
        else if(min > 0)
            timeDesc = string.Format("{0}{1}", min, LocalizationMgr.Get("TimeMgr_7"));
        else
            timeDesc = string.Format("{0}{1}", 1, LocalizationMgr.Get("TimeMgr_7"));

        timeDesc += simpleDesc;

        return timeDesc;
    }

    /// <summary>
    /// 转换时间
    /// </summary>
    /// <returns>The time to chinese.</returns>
    public static string ConvertTimeToChinese(int second, bool needSec)
    {
        string timeDesc = string.Empty;

        int day = second / 86400;
        int hour = (second - day * 86400) / 3600;
        int min = (second - day * 86400 - hour * 3600) / 60;
        int sec = second - day * 86400 - hour * 3600 - min * 60;

        if (day > 0)
            timeDesc = string.Format("{0}{1}", day, LocalizationMgr.Get("TimeMgr_1"));

        if (hour > 0)
            timeDesc = string.Format("{0}{1}{2}", timeDesc, hour, LocalizationMgr.Get("TimeMgr_2"));

        if (min > 0)
            timeDesc = string.Format("{0}{1}{2}", timeDesc, min, LocalizationMgr.Get("TimeMgr_3"));

        if (needSec || timeDesc == "")
            return string.Format("{0}{1}{2}", timeDesc, sec, LocalizationMgr.Get("TimeMgr_4"));

        if (day <= 0 && hour <= 0 && min > 0 && sec > 0)
            timeDesc = string.Format("{0}{1}{2}", timeDesc, sec, LocalizationMgr.Get("TimeMgr_4"));

        return timeDesc;
    }

    /// <summary>
    /// 转换时间
    /// </summary>
    /// <returns>The time to chinese.</returns>
    public static string ConvertTimeToChineseIgnoreZero(int second, bool needSec = true)
    {
        string timeDesc = string.Empty;

        int day = second / 86400;
        if (day > 0)
        {
            timeDesc = string.Format("{0}{1}", day, LocalizationMgr.Get("TimeMgr_1"));

            if (second % 86400 == 0)
                return timeDesc;
        }

        int hour = (second - day * 86400) / 3600;
        if (hour > 0)
        {
            timeDesc = string.Format("{0}{1}{2}", timeDesc, hour, LocalizationMgr.Get("TimeMgr_2"));

            if (second % 3600 == 0)
                return timeDesc;
        }

        int min = (second - day * 86400 - hour * 3600) / 60;
        if (min > 0)
        {
            timeDesc = string.Format("{0}{1}{2}", timeDesc, min, LocalizationMgr.Get("TimeMgr_7"));

            if (second % 60 == 0)
                return timeDesc;
        }

        if (!needSec)
            return timeDesc;

        int sec = second - day * 86400 - hour * 3600 - min * 60;

        return string.Format("{0}{1}{2}", timeDesc, sec, LocalizationMgr.Get("TimeMgr_4"));
    }

    /// <summary>
    /// 转换为 “00时00分00秒” 时间格式
    /// </summary>
    public static string ConvertTimeToChineseTimer(int second, bool needSecond = true, bool isHour = true)
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
                timeDesc = string.Format("{0}{1} {2}", hour, timeDesc, LocalizationMgr.Get("TimeMgr_8"));
            else
                timeDesc = string.Format("0{0}{1} {2}", hour, timeDesc, LocalizationMgr.Get("TimeMgr_8"));
        }

        if (min >= 10)
            timeDesc = string.Format("{0} {1} {2}", timeDesc, min, LocalizationMgr.Get("TimeMgr_3"));
        else
            timeDesc = string.Format("{0} 0{1} {2}", timeDesc, min, LocalizationMgr.Get("TimeMgr_3"));

        if (!needSecond)
            return timeDesc;

        if (sec >= 10)
            timeDesc = string.Format("{0} {1} {2}", timeDesc, sec, LocalizationMgr.Get("TimeMgr_4"));
        else
            timeDesc = string.Format("{0} 0{1} {2}", timeDesc, sec, LocalizationMgr.Get("TimeMgr_4"));

        return timeDesc;
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
