/// <summary>
/// Author: WinMi
/// Description: 时间管理模块
/// 2018-08-14
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 时间工具类
/// </summary>
public class TimeUtility : Singleton<TimeUtility>
{
    #region 公有数据成员

    /// <summary>
    /// 一天总的秒数
    /// </summary>
    public readonly int ONE_DAY_TOTAL_SECONDS = 86400;

    /// <summary>
    /// 时间戳的起始DateTime,以格林威治时间区时
    /// </summary>
    public readonly DateTime START_DATETIME = new DateTime(1970, 1, 1, 0, 0, 0);

    #endregion

    #region 外部接口

    /// <summary>
    /// 通过时间戳转换成本地时区DateTime
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public DateTime ConvertToLocalDateTimeByTimestamp(long timestamp)
    {
        DateTime dt = DateTime.MinValue;
        DateTime startDt = TimeZone.CurrentTimeZone.ToLocalTime(START_DATETIME);
        dt = startDt.AddSeconds(timestamp);

        return dt;
    }

    /// <summary>
    /// 通过DateTime转换成时间戳
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public long ConvertToTimestampByDateTime(DateTime dt)
    {
        long timestamp;

        DateTime startDt = TimeZone.CurrentTimeZone.ToLocalTime(START_DATETIME);
        timestamp = (long)((dt - startDt).TotalSeconds);

        return timestamp;
    }

    /// <summary>
    /// 通过时间戳比较是否是同一天
    /// </summary>
    /// <param name="timestamp1"></param>
    /// <param name="timestamp2"></param>
    /// <returns></returns>
    public bool IsSameDay(long timestamp1, long timestamp2)
    {
        //两个时间戳是否相差一整天
        if (Math.Abs(timestamp1 - timestamp2) > ONE_DAY_TOTAL_SECONDS)
            return false;

        DateTime dt1 = ConvertToLocalDateTimeByTimestamp(timestamp1);
        DateTime dt2 = ConvertToLocalDateTimeByTimestamp(timestamp2);

        //比较年月日是否相同
        //ToLongDateString: 2018-08-14
        return dt1.ToLongDateString() == dt2.ToLongDateString();
    }

    #endregion
}

