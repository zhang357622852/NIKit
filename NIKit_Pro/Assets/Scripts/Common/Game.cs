/// <summary>
/// Game.cs
/// Copy from zhangyg 2014-10-22
/// 提供一些公共接口
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;
using LPC;

public static partial class Game
{
    /// <summary>
    /// 大数相加
    /// </summary>
    /// <returns>The number add.</returns>
    /// <param name="numMax">Number max.</param>
    /// <param name="numMin">Number minimum.</param>
    public static string BigNumAdd(string numMax, string numMin)
    {
        // 把max固定为位数较大的那个数，方便后面处理
        if(numMax.Length < numMin.Length)
        {
            string temp = numMax;
            numMax = numMin;
            numMin = temp;
        }

        // 转换数据为utf8数组
        char[] maxUtf8 = numMax.ToCharArray();
        char[] minUtf8 = numMin.ToCharArray();
        int length_max = numMax.Length;
        int length_min = numMin.Length;
        int flag = 0; // flag是进位标记
        int a;
        int b;
        int sum;

        // 检查数据是否合法
        foreach (char utf8word in maxUtf8)
        {
            // 数字或英文算一个字符
            if (utf8word < '0' || utf8word > '9')
                UnityEngine.Debug.Assert(false);
        }

        // 检查数据是否合法
        foreach (char utf8word in minUtf8)
        {
            // 数字或英文算一个字符
            if (utf8word < '0' || utf8word > '9')
                UnityEngine.Debug.Assert(false);
        }

        // 获取0的ascii码中对应的值
        int zeroInt = Convert.ToInt32('0');

        //从低位开始把对应的位相加
        while(length_max > 0)
        {
            // 获取max当前位的数字
            a = Convert.ToInt32(maxUtf8[length_max - 1]) - zeroInt;

            // 如果min还没加完（注意，min是位数较少的）
            // 获取min当前位的数字
            // 如果min加完了，min对应位上就没有数来加了
            if(length_min > 0)
                b = Convert.ToInt32(minUtf8[length_min - 1]) - zeroInt;
            else
                b = 0;

            // 这时我没有break，因为虽然min没有数字来加了，但可能还有进位需要加
            // num1与num2对应位上的数字相加，再加上进位位
            sum = a + b + flag;

            // 如果加起来大于于10，那就需要进位了
            if(sum >= 10)
            {
                // 计算加完之后，当前位应该是多少
                maxUtf8[length_max - 1] = (char) (zeroInt + sum % 10);

                // 把进位标记置1
                flag = 1;
            }
            else
            {
                //计算加完之后，当前位应该是多少
                maxUtf8[length_max - 1] = (char) (zeroInt + sum);

                // 把进位标记置0
                flag = 0;
            }

            // 向高位移动1位
            length_max--;
            length_min--;
        }

        // 如果两个数对应位都加完了，进位位是1，说明位数要增加1了
        // 比如99+1，加完之后，变成了三位数100，其实就是再在前面加一位1
        return (flag != 0) ? string.Format("{0}{1}", flag, new string(maxUtf8)) : new string(maxUtf8);
    }

    /// <summary>
    /// Bigs the number format.
    /// </summary>
    /// <returns>The number format.</returns>
    /// <param name="num">Number.</param>
    public static string BigNumFormat(string num)
    {
        string finalStr = string.Empty;
        int length = num.Length;
        string tempStr;

        // 拼接数据
        while(length > 0)
        {
            // 每3个字符串
            length -= 3;

            // 获取数据
            if (length >= 0)
                tempStr = num.Substring(length, 3);
            else
                tempStr = num.Substring(0, length + 3);

            // 拼接数据
            if (! string.IsNullOrEmpty(finalStr))
                finalStr = string.Format("{0},{1}", tempStr, finalStr);
            else
                finalStr = tempStr;
        }

        // 返回格式化后的字符串
        return finalStr;
    }

    /// <summary>
    /// Converts the distance from point to inch.
    /// </summary>
    /// <returns>The distance from point to inch.</returns>
    /// <param name="pointDis">Point dis.</param>
    public static float convertDistanceFromPointToInch(float pointDis)
    {
        // 获取当前dpi
        float dpi = Screen.dpi;
        if (dpi > 0f)
            return pointDis / dpi;
        else
            return pointDis;
    }

    /// <summary>
    /// unity单位和屏幕像素比例（即1 unity单位等于100的像素）
    /// </summary>
    public static int UnitToPixelScale
    {
        get
        {
            // 1 unity单位等于100的像素
            return 100;
        }
    }

    // 资源存放的根目录
    private static string mRootPath = null;

    public static string RootPath
    {
        get
        {
            if (mRootPath == null)
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                        mRootPath = Application.dataPath + "/../../bundle";
                        break;
                    case RuntimePlatform.Android:
                        mRootPath = Application.persistentDataPath;
                        break;
                    case RuntimePlatform.IPhonePlayer:
                        mRootPath = Application.persistentDataPath;
                        break;
                    default :
                        mRootPath = Application.streamingAssetsPath;
                        break;
                }
            }

            return mRootPath;
        }
    }

    // StreamingAsset目录
    public static string StreamAssetPath
    {
        get
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return Application.streamingAssetsPath + "/";
                default :
                    return string.Format("file://{0}/", Application.streamingAssetsPath);
            }
        }
    }

    static int[] _value_arr = new int[]
    {
        20,
        40,
        100,
        200,
        400,
        1000,
        2000,
        4000,
        10000,
        20000,
        40000,
        100000,
        200000,
        400000,
    };
    static int[] _multi_arr = new int[]
    {
        1,
        2,
        5,
        10,
        20,
        50,
        100,
        200,
        500,
        1000,
        2000,
        5000,
        10000,
        20000,
    };

    /// <summary>
    /// 数字美化函数1
    /// </summary>
    public static int FormulaFloor(float f, int flag = 0)
    {
        int val = (int)f;
        int sign = (val <= 0) ? -1 : 1;

        val = Math.Abs(val);

        if (val > (flag == 1 ? _multi_arr[13] * 100 : _value_arr[13]))
            val = val / 50000 * 50000;
        else
        {
            int i = 0;
            for (i = 0; i < 14; i++)
            {
                if (val <= (flag == 1 ? _multi_arr[i] * 100 : _value_arr[i]))
                    break;
            }
            val = val / _multi_arr[i] * _multi_arr[i];
        }

        return sign * val;
    }

    /// <summary>
    /// 数字美化函数2，四位数末尾2位去掉，五位数以上末尾三位数去掉
    /// </summary>
    public static int FormulaFloor_1(float f)
    {
        int val = (int)f;

        if (f <= 999)
            val = val / 2 * 2;
        else
            val = val / 5 * 5;

        return val;
    }

    /// <summary>
    /// 数字美化函数3，数字取5或者10的倍数
    /// </summary>
    public static int FormulaFloor_2(float f)
    {
        float val_f = f % 10;
        int val = (int)f % 10;

        if (val_f < 2.5)
            val = (int)val / 10 * 10;
        else if (val_f < 7.5)
            val = (int)val / 10 * 10 + 5;
        else
            val = (int)val / 10 * 10 + 10;

        return val;
    }

    /// <summary>
    /// Converts to CS format.
    /// </summary>
    /// <returns>The to CS format.</returns>
    /// <param name="c_format_text">C_format_text.</param>
    public static string ConvertToCSFormat(string c_format_text)
    {
        List<char> cs_format_list = new List<char>();
        int format_count = 0;

        string temp_text = c_format_text.Replace("<br/>", "\\n");

        // Convert %s %O %d to {0} {1} {2} format
        int len = temp_text.Length;
        for (int i = 0; i < len; i++)
        {
            char ch = temp_text[i];
            if (ch != '%')
            {
                if ((ch == '{') || (ch == '}'))
                    cs_format_list.Add(ch);

                cs_format_list.Add(ch);
                continue;
            }

            if (i + 1 >= len)
            {
                cs_format_list.Add(ch);
                continue;
            }

            char next_ch = temp_text[i + 1];
            if ((next_ch == 'd') ||
                (next_ch == 's') ||
                (next_ch == 'O'))
            {
                cs_format_list.AddRange(string.Format("{{{0}}}", format_count).ToCharArray());
                format_count++;
                i++;
                continue;
            }

            if (next_ch == '%')
            {
                cs_format_list.Add(next_ch);
                i++;
                continue;
            }
        }

        return new string(cs_format_list.ToArray());
    }

    public static string ConvertToNGUIFormat(string c_format_text)
    {
        string temp_text = c_format_text.Replace("<br/>", "\n");
        temp_text = temp_text.Replace(@"<\>", "[");
        temp_text = temp_text.Replace(@"</>", "]");

        foreach (KeyValuePair<string, string> v in ColorConfig.mColorTag)
            temp_text = temp_text.Replace(v.Key, v.Value);

        return temp_text;
    }

    public static int atoi(string text)
    {
        int val = 0;
        try
        {
            val = System.Convert.ToInt32(text);
        }
        catch
        {
            val = 0;
        }
        return val;
    }

    public static string[] Explode(string path, string seperator)
    {
        return path.Split(seperator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
    }

    public static string Implode(string[] parts, string seperator)
    {
        if (parts.Length < 1)
            return string.Empty;

        if (parts.Length < 2)
            return parts[0];

        StringBuilder builder = new StringBuilder(string.Empty);
        builder.Append(parts[0]);
        for (int i = 1; i < parts.Length; i++)
            builder.AppendFormat("{0}{1}", seperator, parts[i]);

        return builder.ToString();
    }

    public static string Implode(string[] parts, string seperator, int start_index, int end_index)
    {
        if (start_index >= parts.Length)
            return string.Empty;

        if (start_index < 0)
            start_index = 0;

        if (end_index >= parts.Length)
            end_index = parts.Length - 1;

        List<string> part_list = new List<string>();
        for (int i = start_index; i <= end_index; i++)
            part_list.Add(parts[i]);

        return Implode(part_list.ToArray(), seperator);
    }

    /// <summary>
    /// 数值除以10再加上百分号
    /// </summary>
    public static string ChangeToPersent(int val)
    {
        float f = val * 0.1f;
        f = float.Parse(f.ToString("F1"));
        return f.ToString() + "%";
    }

    /// <summary>
    /// 构造字典序结构
    /// </summary>
    public static Dictionary<string, object> NewMapping(params object[] args)
    {
        // 参数一定是偶数个
        System.Diagnostics.Debug.Assert(args.Length % 2 == 0);

        // 打包为mapping
        Dictionary<string, object> m = new Dictionary<string, object>();
        for (int i = 0; i < args.Length; i += 2)
        {
            string k = args[i] as string;
            object v = args[i + 1];

            m[k] = v;
        }

        // 返回结果
        return m;
    }

    /// <summary>
    /// 转换字符串为标志位
    /// </summary>
    public static int ConvertFieldToFlags(string value_text, Dictionary<char, int> field_table)
    {
        int result = 0;
        foreach (char c in value_text)
        {
            if (field_table.ContainsKey(c))
                result |= field_table[c];
        }
        return result;
    }

    /// <summary>
    /// 根据包裹页及位置索引构造真实的包裹位置
    /// </summary>
    /// <returns>
    /// 真实的包裹位置
    /// </returns>
    /// <param name='page'>
    /// 包裹页
    /// </param>
    /// <param name='index_in_page'>
    /// 位置索引
    /// </param>
    ///
    public static string MakePos(int page, int index_in_page)
    {
        return string.Format("{0}-{1}-{2}", page, 0, index_in_page);
    }

    /// <summary>
    /// 从真实的包裹位置中读取包裹页和页中的索引
    /// </summary>
    /// <returns>
    /// 是否读取成功
    /// </returns>
    /// <param name='pos'>
    /// 真实的包裹位置
    /// </param>
    /// <param name='page'>
    /// 包裹页
    /// </param>
    /// <param name='index_in_page'>
    /// 页中索引
    /// </param>
    public static bool ReadPos(string pos, ref int page, ref int index_in_page)
    {
        string[] str_list = pos.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        if (str_list.Length < 3)
            return false;

        page = System.Convert.ToInt32(str_list[0]);
        index_in_page = System.Convert.ToInt32(str_list[2]);
        return true;
    }

    public static float Fxn(params object[] args)
    {
        float result = 0;
        float x = (float)args[0];
        for (int i = 1; i < args.Length; i++)
        {
            float pi = (float)args[i];
            result += pi * (float)Math.Pow(x, i);
        }
        return result;
    }

    /// <summary>
    /// 判断两个时间是不是同一天
    /// </summary>
    public static bool IsSameDay(int sec1, int sec2)
    {
        DateTime dt1 = TimeMgr.ConvertIntDateTime(sec1);

        DateTime dt2 = TimeMgr.ConvertIntDateTime(sec2);

        return dt1.ToLongDateString() == dt2.ToLongDateString();
    }

    /// <summary>
    /// 判断两个时间是否是同一周
    /// </summary>
    public static bool IsSameWeek(int sec1, int sec2)
    {
        DateTime dt1 = TimeMgr.ConvertIntDateTime(sec1);

        DateTime dt2 = TimeMgr.ConvertIntDateTime(sec2);

        // 时间相差大于一周
        if (Math.Abs(sec1 - sec2) >= (60 * 60 * 24 * 7))
            return false;

        // 同一天
        if (dt1.Year.Equals(dt2.Year) && dt1.DayOfYear.Equals(dt2.DayOfYear))
            return true;

        int weekDay1 = GetWeekDay(sec1) == 0 ? 7 : GetWeekDay(sec1);

        int weekDay2 = GetWeekDay(sec2) == 0 ? 7 : GetWeekDay(sec2);

        if (sec1 > sec2 && weekDay1 <= weekDay2)
            return false;

        if (sec1 < sec2 && weekDay1 >= weekDay2)
            return false;

        return true;
    }

    /// <summary>
    /// 判断两个时间是否是同一个月
    /// </summary>
    public static bool IsSameMonth(int sec1, int sec2)
    {
        DateTime dt1 = TimeMgr.ConvertIntDateTime(sec1);

        DateTime dt2 = TimeMgr.ConvertIntDateTime(sec2);

        if(dt1.Year.Equals(dt2.Year) && dt1.Month.Equals(dt2.Month))
            return true;
        else
            return false;
    }

    /// <summary>
    /// 判断两个时间是否是同一年
    /// </summary>
    public static bool IsSameYear(int sec1, int sec2)
    {
        DateTime dt1 = TimeMgr.ConvertIntDateTime(sec1);

        DateTime dt2 = TimeMgr.ConvertIntDateTime(sec2);

        if(dt1.Year.Equals(dt2.Year))
            return true;
        else
            return false;
    }

    /// <summary>
    /// 获取当前时间距离某天零点的时间间隔
    /// </summary>
    /// <returns>返回多少个小时</returns>
    /// <param name="days">距离的天数</param>
    public static double GetZeroClock(int days)
    {
        // 将当前服务器时间转化为DateTime
        int serverTime = TimeMgr.GetServerTime();

        // 偏移指定天数
        DateTime dt = TimeMgr.ConvertIntDateTime(serverTime);
        DateTime dTime = dt.AddDays(days).Date;

        // 获取00:00:00
        TimeSpan ts = TimeMgr.ConvertDateTime(string.Format("{0} 00:00:00", dTime.ToString("yyyy-MM-dd"))) - dt;
        return ts.TotalSeconds;
    }

    /// <summary>
    /// 星期几
    /// </summary>
    public static string GetWeekDayToChinese(int sec)
    {
        switch (GetWeekDay(sec))
        {
            case 0:
                return LocalizationMgr.Get("ActivityWnd_10");

            case 1:
                return LocalizationMgr.Get("ActivityWnd_4");

            case 2:
                return LocalizationMgr.Get("ActivityWnd_5");

            case 3:
                return LocalizationMgr.Get("ActivityWnd_6");

            case 4:
                return LocalizationMgr.Get("ActivityWnd_7");

            case 5:
                return LocalizationMgr.Get("ActivityWnd_8");

            case 6:
                return LocalizationMgr.Get("ActivityWnd_9");

            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// 获取某个时间是那一周的星级几
    /// </summary>
    public static int GetWeekDay(int sec)
    {
        DateTime dt = TimeMgr.ConvertIntDateTime(sec);

        return Convert.ToInt32(dt.DayOfWeek);
    }

    /// <summary>
    /// 获取某个时间是某月的第几天,
    /// </summary>
    public static int GetDaysMonth(int sec)
    {
        DateTime dt = TimeMgr.ConvertIntDateTime(sec);

        return dt.Day;
    }

    /// <summary>
    /// 获取某月有多少天
    /// </summary>
    public static int GetDaysInMonth(int sec)
    {
        DateTime dt = TimeMgr.ConvertIntDateTime(sec);

        return DateTime.DaysInMonth(dt.Year, dt.Month);
    }

    /// <summary>
    /// 获取年份
    /// </summary>
    public static int GetYear(int sec)
    {
        DateTime dt = TimeMgr.ConvertIntDateTime(sec);

        return dt.Year;
    }

    /// <summary>
    /// 获取月份
    /// </summary>
    public static int GetMonth(int sec)
    {
        DateTime dt = TimeMgr.ConvertIntDateTime(sec);

        return dt.Month;
    }

    /// <summary>
    /// 是否是周末
    /// </summary>
    public static bool IsWeekend(int sec)
    {
        DateTime dt = TimeMgr.ConvertIntDateTime(sec);

        DayOfWeek dw = dt.DayOfWeek;

        if (dw == DayOfWeek.Saturday || dw == DayOfWeek.Sunday)
            return true;

        return false;
    }

    /// <summary>
    /// 获取本周日零点的时间
    /// </summary>
    public static int GetThisSundayTime()
    {
        // 将当前服务器时间转化为DateTime
        int serverTime = TimeMgr.GetServerTime();

        // 偏移指定天数
        DateTime dt = TimeMgr.ConvertIntDateTime(serverTime);
        DateTime dTime = dt.AddDays(DayOfWeek.Sunday - dt.DayOfWeek + 7).Date;

        // 获取00:00:00
        TimeSpan ts = TimeMgr.ConvertDateTime(string.Format("{0} 00:00:00", dTime.ToString("yyyy-MM-dd"))) - dt;
        return (int) ts.TotalSeconds;
    }

    public static List<LPCValue> SortLPCArray(LPCArray arr, Comparison<LPCValue> comparison)
    {
        List<LPCValue> result = new List<LPCValue>();
        result.AddRange(arr.Values);

        result.Sort(comparison);
        return result;
    }

    // 比较字符串
    // 因为之前有发现在ios上的字符串比较有点问题，所以自己实现了一套，目前
    // 这个接口只会用在csv的行排序上
    public static int StringCompare(string one, string two)
    {
        if (one == two)
            return 0;
        if (one == null)
            return -1;
        if (two == null)
            return 1;
        for (int idx = 0; idx < Math.Min(one.Length, two.Length); ++idx)
        {
            if (one[idx] != two[idx])
                return one[idx] > two[idx] ? 1 : -1;
        }
        if (one.Length == two.Length)
            return 0;
        return one.Length > two.Length ? 1 : -1;
    }

    // 产生一个唯一不重复的名字
    static long sSeqNum = 100000;

    public static string GetUniqueName(string prefix)
    {
        return string.Format("{0}_{1}", prefix, (sSeqNum++));
    }

    // 产生一个唯一不重复的Cookie
    public static string NewCookie(string prefix)
    {
        return GetUniqueName(prefix);
    }

    // 一个字节的位设定
    static public void SetBit(byte[] byTargetByte, int nTargetPos, bool nValue)
    {
        int byteIdx = nTargetPos / 8;
        int bitidx = nTargetPos % 8;

        if (byteIdx < 0 || byteIdx >= byTargetByte.Length)
            return;
        if (nValue)
            byTargetByte[byteIdx] = Convert.ToByte(byTargetByte[byteIdx] | (0x01 << bitidx));
        else
            byTargetByte[byteIdx] = Convert.ToByte(byTargetByte[byteIdx] & (~(0x01 << bitidx)));

        return;
    }

    // 模拟 NET4.0 的Enum.TryPars<T>()方法，u3d没有支持
    public static bool EnumTryParse<T>(string valueToParse, out T returnValue)
    {
        returnValue = default(T);
        if (Enum.IsDefined(typeof(T), valueToParse))
        {
            // 指定的Enum已经定义，直接转换

            // u3d的bug 换个写法
            //TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            //returnValue = (T)converter.ConvertFromString(valueToParse);
            try
            {
                returnValue = (T)Enum.Parse(typeof(T), valueToParse);
            }catch(Exception)
            {
                return false;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 判断屏幕点是否超出了屏幕
    /// </summary>
    /// <returns><c>true</c> if is out of screen the specified point; otherwise, <c>false</c>.</returns>
    /// <param name="point">Point.</param>
    public static bool IsOutOfScreen(Vector3 point)
    {
        // 检查边界
        if (point.x < 0f || point.x > Screen.width ||
            point.y < 0f || point.y > Screen.height)
            return true;

        // 没有超出屏幕
        return false;
    }

    /// <summary>
    /// 把3D点换算成NGUI屏幕上的2D点
    /// <returns>The to U.</returns>
    /// <param name="point">Point.</param>
    public static Vector3 WorldToUI(Vector3 point)
    {
        // 获取场景相机
        System.Diagnostics.Debug.Assert(SceneMgr.SceneCamera != null);
        System.Diagnostics.Debug.Assert(SceneMgr.UiCamera != null);

        Vector3 pt = SceneMgr.SceneCamera.WorldToScreenPoint(point);
        Vector3 ff = SceneMgr.UiCamera.ScreenToWorldPoint(pt);
        ff.z = 0;

        return ff;
    }

    /// <summary>
    /// 把NGUI相机空间下的世界坐标，转换为场景相机空间下的世界坐标
    /// </summary>
    public static Vector3 UIToWorld(Vector3 uiPoint)
    {
        System.Diagnostics.Debug.Assert(SceneMgr.SceneCamera != null);
        System.Diagnostics.Debug.Assert(SceneMgr.UiCamera != null);

        Vector3 sp = SceneMgr.UiCamera.WorldToScreenPoint(uiPoint);
        Vector3 wp = SceneMgr.SceneCamera.ScreenToWorldPoint(sp);
        wp.z = 0;

        return wp;
    }

    /// <summary>
    /// 一维向量，移动到目标（最大距离限制）
    /// </summary>
    /// <returns>The move towards.</returns>
    /// <param name="current">Current.</param>
    /// <param name="target">Target.</param>
    /// <param name="maxDistanceDelta">Max distance delta.</param>
    public static float Vector1MoveTowards(float current, float target, float maxDistanceDelta)
    {
        float a = target - current;
        float magnitude = Mathf.Abs(a);
        if (magnitude <= maxDistanceDelta || magnitude == 0f)
        {
            return target;
        }
        return (a > 0) ? (current + maxDistanceDelta) : (current - maxDistanceDelta);
    }

    /// <summary>
    /// 比较两个浮点数是否相等
    /// </summary>
    /// <returns><c>true</c>, if equal was floated, <c>false</c> otherwise.</returns>
    /// <param name="tolerance">误差范围</param>
    public static bool FloatEqual(float a, float b, float tolerance = float.Epsilon)
    {
        return (Math.Abs(b - a) <= tolerance);
    }

    /// <summary>
    /// 比较浮点数大小
    /// </summary>
    /// <param name="tolerance">误差范围</param>
    public static bool FloatGreat(float a, float b, float tolerance = float.Epsilon)
    {
        return (a - b) > tolerance;
    }

    /// <summary>
    /// 实现C语言sscanf功能
    /// </summary>
    /// <param name="inputStr">待匹配字符串</param>
    /// <param name="pattern">匹配格式字符串</param>
    public static List<string> Scanf(string inputStr, string pattern)
    {
        List<string> ret = new List<string>();

        // 正则表达式匹配
        Match mat = Regex.Match(inputStr, pattern);

        // 匹配失败
        if (!mat.Success)
            return ret;

        // 匹配成功逐个填充数据
        for (int index = 0; index < mat.Groups.Count - 1; index++)
            ret[index] = mat.Groups[index + 1].Value;

        // 返回结果
        return ret;
    }

    /// <summary>
    /// 实现C语言Sscanf功能
    /// </summary>
    /// <param name="inputStr">待匹配字符串</param>
    /// <param name="pattern">匹配格式字符串</param>
    /// <param name="_params">返回匹配结果</param>
    public static bool Scanf(string inputStr, string pattern, params MixedValue[] _params)
    {
        // 正则表达式匹配
        Match mat = Regex.Match(inputStr, pattern);

        // 匹配失败
        if (!mat.Success)
            return false;

        // 匹配成功逐个填充数据
        for (int index = 0; index < _params.Length && index < mat.Groups.Count - 1; index++)
        {
            Type t = _params[index].GetValueType();
            if (t == typeof(int))
                _params[index].SetValue<int>(int.Parse(mat.Groups[index + 1].Value));
            else if (t == typeof(float))
                _params[index].SetValue<float>(float.Parse(mat.Groups[index + 1].Value));
            else
                _params[index].SetValue<string>(mat.Groups[index + 1].Value);
        }

        // 返回解析成功
        return true;
    }

    /// <summary>
    /// 获取字符串长度
    /// </summary>
    public static int GetStrLength(string str)
    {
        // 字符串为空
        if (str.Length == 0)
            return 0;

        int tempLen = 0;

        // 转换为byte
        byte[] bytes = Encoding.ASCII.GetBytes(str);

        // 遍历各个元素进行判断
        for (int i = 0; i < bytes.Length; i++)
        {
            // 是中文字符
            if ((int)bytes[i] == 63)
            {
                tempLen += 2;
                continue;
            }

            // 非中文字符
            tempLen += 1;
        }

        // 返回字符串长度
        return tempLen;
    }

    /// <summary>
    /// Pow the specified x and y.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// 支持int(x,y必须全部大于等于0)
    public static int Pow(int x, int y)
    {
        // 必须满足条件
        UnityEngine.Debug.Assert(x >= 0 && y >= 0);

        // 如果
        if (y == 0)
            return 1;

        // 如果是为1
        if (y == 1)
            return x;

        // 计算结果
        int result = 0;
        int tmp = Pow(x, y / 2);

        // 奇数
        if((y & 1) != 0)
            result = x * tmp * tmp;
        else
            result = tmp * tmp;

        // 返回最终数值
        return result;
    }

    /// <summary>
    /// Determines if is same sign the specified x y.
    /// </summary>
    /// <returns><c>true</c> if is same sign the specified x y; otherwise, <c>false</c>.</returns>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public static bool IsSameSign(int x, int y)
    {
        return (((x ^ y) >> 31) == 0);
    }

    /// <summary>
    /// Divided the specified x, y and quantile.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="quantile">精确位置(eg:1000(千分位))</param>
    public static int Divided(int x, int y, int quantile = 1000)
    {
        // 返回0的情况
        if (x == 0 || y == 0 || quantile == 0)
            return 0;

        // 转换数据
        Int64 ux = Convert.ToInt64(x);
        Int64 uy = Convert.ToInt64(y);
        Int64 uquantile = Convert.ToInt64(quantile);

        // 再将结果转换为int32
        // 如果两者数据大于0x7FFFFFFF, 这个地方处理不了
        return Convert.ToInt32(ux * uquantile / uy);
    }

    /// <summary>
    /// 两个数相乘（保留精度比）
    /// </summary>
    /// <param name="basicValue">基础数值</param>
    /// <param name="multiple">倍数</param>
    /// <param name="quantile">精确位置(eg:1000(千分位))</param>
    public static int Multiple(int x, int y, int quantile = 1000)
    {
        // 返回0的情况
        if (x == 0 || y == 0 || quantile == 0)
            return 0;

        // 转换数据
        Int64 ux = Convert.ToInt64(x);
        Int64 uy = Convert.ToInt64(y);
        Int64 uquantile = Convert.ToInt64(quantile);

        // 再将结果转换为int32
        // 如果两者数据大于0x7FFFFFFF, 这个地方处理不了
        return Convert.ToInt32(ux * uy / uquantile);
    }

    /// <summary>
    /// 生成域名
    /// </summary>
    public static string GenerateDomainAddress(string d, string catalog, int thread)
    {
        if (thread > 0)
            return string.Format("{0}#{1}.{2}", d, thread, catalog);

        return string.Format("{0}.{1}", d, catalog);
    }

    /// <summary>
    /// 根据域名(形式为：rid@XXX#1)得到rid
    /// </summary>
    public static string GetRidByDomain(string domain)
    {
        int end = domain.IndexOf("@");
        string str = domain.Substring(0, end);

        // 干掉分线号
        return Game.Explode(str, "#")[0];
    }

    /// <summary>
    /// 根据完整域地址取得rid
    /// XX@rid.XX
    /// </summary>
    public static string GetRidByDomainAddress(string domainAddress)
    {
        int start = domainAddress.IndexOf("@");
        int end = domainAddress.IndexOf(".");

        string str = domainAddress.Substring(start + 1, end - start - 1);
        return Game.Explode(str, "#")[0];
    }

    /// <summary>
    ///设置货币的显示格式(123,123,123),N后面的数值表示保留的小数位
    /// </summary>
    public static string SetMoneyShowFormat(int money)
    {
        return string.Format("{0:N0}", money);
    }

    /// <summary>
    /// 十以内的阿拉伯数字转换成中文数字
    /// </summary>
    public static string ConvertChineseNumber(int number)
    {
        string chinese = string.Empty;
        switch(number)
        {
            case 1 :
                chinese = "一";
                break;
            case 2 :
                chinese = "二";
                break;
            case 3 :
                chinese = "三";
                break;
            case 4 :
                chinese = "四";
                break;
            case 5 :
                chinese = "五";
                break;
            case 6 :
                chinese = "六";
                break;
            case 7 :
                chinese = "七";
                break;
            case 8 :
                chinese = "八";
                break;
            case 9 :
                chinese = "九";
                break;
            case 10 :
                chinese = "十";
                break;
        }

        return chinese;
    }

    /// <summary>
    /// 计算屏幕缩放
    /// </summary>
    /// <returns>The screen scale.</returns>
    public static float CalcWndScale()
    {
        float scale = 1.0f;

        float basicScale =  (float)16 /9;
        float screenScale = (float)Screen.width /Screen.height ;

        if(basicScale < screenScale)
            scale = basicScale / screenScale;

        return scale;
    }

    /// <summary>
    /// Captures the screenshot.
    /// </summary>
    /// <returns>The screenshot.</returns>
    /// <param name="filePath">File path.</param>
    /// <param name="rect">Rect.</param>
    public static void CaptureScreenshot(string filePath)
    {
        // 确保文件路径存在
        FileMgr.CreateDirectory(Path.GetDirectoryName(filePath));

        // 截屏操作
        Application.CaptureScreenshot(filePath);
    }

    /// <summary>
    /// Captures the screenshot.
    /// 该接口只能等到WaitForEndOfFrame后在调用，否则报错或者截屏不完整
    /// </summary>
    /// <returns>The screenshot.</returns>
    /// <param name="filePath">FilePath.</param>
    /// <param name="rect">Rect.</param>
    public static Texture2D CustomCaptureScreenshot(string filePath, Rect rect)
    {
        // 确保文件路径存在
        FileMgr.CreateDirectory(Path.GetDirectoryName(filePath));

        // 先创建一个的空纹理，大小可根据实现需要来设置
        Texture2D screenShot = new Texture2D((int) rect.width,
            (int) rect.height,
            TextureFormat.RGB24,
            false);

        // 读取屏幕像素信息并存储为纹理数据
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();

        // 然后将这些纹理数据，成一个png图片文件
        byte[] bytes = screenShot.EncodeToPNG();
        FileMgr.WriteFile(filePath, bytes);

        // 最后，我返回这个Texture2d对象，这样我们直接，所这个截图图示在游戏中，当然这个根据自己的需求的。
        return screenShot;
    }

    /// <summary>
    /// 对相机截图
    /// </summary>
    /// <param name="cameras"></param>
    /// <param name="rect"></param>
    /// <param name="filePath">空字符串时表示不保存到本地，只获取截图信息</param>
    /// <returns></returns>
    public static Texture2D CaptureCamera(Camera[] cameras, Rect rect, string filePath = "")
    {
        // 创建一个RenderTexture对象
        RenderTexture rt = new RenderTexture((int)rect.width, (int)rect.height, 0);

        // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].targetTexture = rt;
            cameras[i].Render();
        }

        // 激活这个rt, 并从中读取像素。
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(rect, 0, 0);// 注：这个时候，它是从RenderTexture.active中读取像素
        screenShot.Apply();

        // 重置相关参数，以使用camera继续在屏幕上显示
        for (int i = 0; i < cameras.Length; i++)
            cameras[i].targetTexture = null;
        RenderTexture.active = null;
        GameObject.Destroy(rt);

        // 最后将这些纹理数据，成一个png图片文件
        if (!string.IsNullOrEmpty(filePath))
        {
            byte[] bytes = screenShot.EncodeToPNG();
            FileMgr.WriteFile(filePath, bytes);
        }

        return screenShot;
    }

}
