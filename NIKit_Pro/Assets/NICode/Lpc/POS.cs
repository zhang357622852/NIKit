/// <summary>
/// POS.cs
/// Update by wangxw 2014-12-11
/// Copy from zhangyg 2014-10-22
/// 位置信息
/// </summary>

using UnityEngine;

namespace LPC
{
    public class POS
    {
        /// <summary>
        /// 构建一个POS
        /// </summary>
        public static string Make(int x, int y, int z)
        {
            return string.Format("{0}-{1}-{2}", x, y, z);
        }

        /// <summary>
        /// 构建一个POS
        /// </summary>
        public static string Make(float x, float y, float z)
        {
            return string.Format("{0}-{1}-{2}", x, y, z);
        }

        /// <summary>
        /// 解析一个POS
        /// </summary>
        public static bool Read(string pos, out int x, out int y, out int z)
        {
            x = 0;
            y = 0;
            z = 0;
            string[] arr = pos.Split(new char[] { '-' });
            if (arr.Length != 3)
                return false;

            if (GetIntByRangeString(arr [0], out x) &&
                GetIntByRangeString(arr [1], out y) &&
                GetIntByRangeString(arr [2], out z))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 解析一个POS
        /// </summary>
        public static bool Read(string pos, out float x, out float y, out float z)
        {
            x = 0.0f;
            y = 0.0f;
            z = 0.0f;
            string[] arr = pos.Split(new char[] { '-' });
            if (arr.Length != 3)
                return false;

            if (GetFloatByRangeString(arr [0], out x) &&
                GetFloatByRangeString(arr [1], out y) &&
                GetFloatByRangeString(arr [2], out z))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 获取int范围随机
        /// 解析“[a,b]”这种字符串，从[a,b]范围中随机出一个int数据
        /// 如果a,b不是闭区间，默认返回a
        /// </summary>
        /// <returns>如果无法解析成int，则返回false；成功则返回true</returns>
        /// <param name="intStr">形如[4,8]的字符串格式</param>
        public static bool GetIntByRangeString(string intStr, out int value)
        {
            value = 0;

            // 普通int类型
            if (intStr [0] != '[')
                return int.TryParse(intStr, out value);

            // 未知类型
            string[] arr = intStr.Trim('[', ']').Split(',');
            if (arr.Length < 2)
                return false;

            int a, b;
            if (! int.TryParse(arr [0], out a) ||
                ! int.TryParse(arr [1], out b))
                return false;

            // 返回范围随机值
            value = (a < b) ? Random.Range(a, b) : Mathf.Max(b, a);
            return true;
        }

        /// <summary>
        /// 获取float范围随机
        /// 解析“[a,b]”这种字符串，从[a,b]范围中随机出一个float数据
        /// 如果a,b不是闭区间，默认返回a
        /// </summary>
        /// <returns>如果无法解析成float，则返回false；成功则返回true</returns>
        /// <param name="floatStr">形如[4,8]的字符串格式</param>
        public static bool GetFloatByRangeString(string floatStr, out float value)
        {
            value = 0f;

            // 普通float类型
            if (floatStr [0] != '[')
                return float.TryParse(floatStr, out value);

            // 未知类型
            string[] arr = floatStr.Trim('[', ']').Split(',');
            if (arr.Length < 2)
                return false;

            float a, b;
            if (! float.TryParse(arr [0], out a) ||
                ! float.TryParse(arr [1], out b))
                return false;

            // 返回范围随机值
            value = (a < b) ? Random.Range(a, b) : Mathf.Max(b, a);
            return true;
        }
    }
}