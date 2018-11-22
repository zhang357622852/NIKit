/// <summary>
/// LPCArray.cs
/// Copy from zhangyg 2014-10-22
/// LPC array数据类型
/// </summary>

using System;
using System.Collections.Generic;

namespace LPC
{
    /// <summary>
    /// 对应于LPC的Array结构
    /// </summary>
    public class LPCArray
    {
        public static LPCArray Empty{ get { return new LPCArray(); } }

        // 内部实现一个list
        private List<LPCValue> _list = null;

        /// <summary>
        /// 这个array有多大
        /// </summary>
        public int Count { get { return _list == null ? 0 : _list.Count; } }

        /// <summary>
        /// 构造函数，可直接赋值
        /// </summary>
        public LPCArray(params object[] _params)
        {
            // 遍历参数直接赋值
            this.Add(_params);
        }

        /// <summary>
        /// 将一个数据插入到列表尾部
        /// </summary>
        /// <param name="val"></param>
        public void Add(params object[] valList)
        {
            // 逐个添加数据
            for (int i = 0; i < valList.Length; i++)
            {
                Add(valList [i]);
            }
        }

        /// <summary>
        /// 将一个数据插入到列表尾部
        /// </summary>
        /// <param name="val"></param>
        public void Add(object val)
        {
            confirm_list();

            if (! (val is LPCValue))
                _list.Add(LPCValue.Create(val));
            else
                _list.Add((val as LPCValue));
        }

        /// <summary>
        /// 将一个数据插入到列表尾部
        /// </summary>
        /// <param name="val"></param>
        public void Add(LPCValue val)
        {
            confirm_list();
            _list.Add(val);
        }

        /// <summary>
        /// 数组元素数据相加
        /// </summary>
        public int CountTotalInt()
        {
            // 返回数据
            return (int)CountTotalFloat();
        }

        /// <summary>
        /// 数组元素数据相加
        /// </summary>
        public float CountTotalFloat()
        {
            // 都是null返回0
            if (_list == null)
                return 0;

            float total = 0f;
            foreach (LPCValue v in this.Values)
            {
                if (v.IsInt)
                    total += v.AsInt;
                else if (v.IsFloat)
                    total += v.AsFloat;
                else if (v.IsArray)
                    total += v.AsArray.CountTotalFloat();
            }

            // 返回数据
            return total;
        }

        /// <summary>
        /// 移除索引处的值
        /// </summary>
        /// <param name="idx"></param>
        public void Remove(int value)
        {
            // 都是null
            if (_list == null)
                return;

            // 删除指定数据
            int idx = 0;
            while (true)
            {
                // 已经到达了数组边界
                if (idx >= _list.Count)
                    break;

                // 类型不同或者不相等
                if (! _list [idx].IsInt || _list [idx].AsInt != value)
                {
                    idx ++;
                    continue;
                }

                // 移除数据
                _list.RemoveAt(idx);
            }
        }

        /// <summary>
        /// 移除索引处的值
        /// </summary>
        /// <param name="idx"></param>
        public void Remove(float value)
        {
            // 都是null返回-1
            if (_list == null)
                return;

            // 删除指定数据
            int idx = 0;
            while (true)
            {
                // 已经到达了数组边界
                if (idx >= _list.Count)
                    break;

                // 类型不同或者不相等
                if (! _list [idx].IsFloat || ! GameUtility.FloatEqual(_list [idx].AsFloat, value))
                {
                    idx ++;
                    continue;
                }

                // 移除数据
                _list.RemoveAt(idx);
            }
        }

        /// <summary>
        /// 移除索引处的值
        /// </summary>
        /// <param name="idx"></param>
        public void Remove(string value)
        {
            // 都是null返回-1
            if (_list == null)
                return;

            // 删除指定数据
            int idx = 0;
            while (true)
            {
                // 已经到达了数组边界
                if (idx >= _list.Count)
                    break;

                // 类型不同或者不相等
                if (! _list [idx].IsString || _list [idx].AsString != value)
                {
                    idx ++;
                    continue;
                }

                // 移除数据
                _list.RemoveAt(idx);
            }
        }

        /// <summary>
        /// 移除索引处的值
        /// </summary>
        /// <param name="idx"></param>
        public void Remove(LPCValue value)
        {
            // 都是null返回-1
            if (_list == null || value == null)
                return;

            // int类型
            if (value.IsInt)
                IndexOf(value.AsInt);
            // float类型
            else if (value.IsFloat)
                IndexOf(value.AsFloat);
            // string类型
            else if (value.IsString)
                IndexOf(value.AsString);
            {
                // 删除指定数据
                int idx = 0;
                while (true)
                {
                    // 已经到达了数组边界
                    if (idx >= _list.Count)
                        break;

                    // 不相等
                    if (_list [idx] != value)
                    {
                        idx ++;
                        continue;
                    }

                    // 移除数据
                    _list.RemoveAt(idx);
                }
            }
        }

        /// <summary>
        /// 移除索引处的值
        /// </summary>
        /// <param name="idx"></param>
        public void RemoveAt(int idx)
        {
            if (_list != null)
                _list.RemoveAt(idx);
        }

        /// <summary>
        /// 获取索引处的值
        /// </summary>
        public LPCValue Get(int idx)
        {
            if (_list == null)
                throw new Exception("get: out of range");

            return _list [idx];
        }

        /// <summary>
        /// 获取一段列表
        /// </summary>
        /// <returns>获取的新列表</returns>
        /// <param name="index">起点Index.</param>
        /// <param name="needCount">总个数.</param>
        public LPCArray GetRange(int index, int needCount)
        {
            if (_list == null || index >= Count)
                return LPCArray.Empty;

            LPCArray retList = new LPCArray();
            int need = needCount;
            for (int i = index; i < Count && need > 0; i++)
            {
                need--;
                retList.Add(this [i]);
            }

            return retList;
        }

        /// <summary>
        /// 设置索引处的值
        /// </summary>
        public void Set(int idx, LPCValue val)
        {
            confirm_list();

            _list [idx] = val;
        }

        /// <summary>
        /// 设置索引处的值
        /// </summary>
        public void Set(int idx, object val)
        {
            confirm_list();

            _list [idx] = LPCValue.Create(val);
        }

        /// <summary>
        /// 排序整个数组
        /// </summary>
        /// <param name="f">排序函数</param>
        public void Sort(Comparer<LPCValue> f)
        {
            if (_list != null)
                _list.Sort(f);
        }

        public void Sort(Comparison<LPCValue> f)
        {
            if (_list != null)
                _list.Sort(f);
        }

        public LPCArray Copy()
        {
            LPCValue dst = LPCValue.CreateArray();
            foreach (LPCValue v in this.Values)
                dst.AsArray.Add(v);
            return dst.AsArray;
        }

        public LPCArray Append(LPCArray array)
        {
            foreach (LPCValue v in array.Values)
                this.Add(v);
            return this;
        }

        /// <summary>
        /// 在array中查询指定元素的所在处索引
        /// </summary>
        public int IndexOf(int val)
        {
            // 都是null返回-1
            if (_list == null)
                return -1;

            // 遍历各个元素比较
            for (int i = 0; i < _list.Count; i++)
            {
                // 类型不同
                if (! _list [i].IsInt)
                    continue;

                // 相等返回true
                if (_list [i].AsInt == val)
                    return i;
            }

            // 返回数据
            return -1;
        }

        /// <summary>
        /// 在array中查询指定元素的所在处索引
        /// </summary>
        public int IndexOf(float val)
        {
            // 都是null返回-1
            if (_list == null)
                return -1;

            // 遍历各个元素比较
            for (int i = 0; i < _list.Count; i++)
            {
                // 类型不同
                if (! _list [i].IsFloat)
                    continue;

                // 相等返回true
                if (GameUtility.FloatEqual(_list [i].AsFloat, val))
                    return i;
            }

            // 返回数据
            return -1;
        }

        /// <summary>
        /// 在array中查询指定元素的所在处索引
        /// </summary>
        public int IndexOf(string val)
        {
            // 都是null返回-1
            if (_list == null)
                return -1;

            // 遍历各个元素比较
            for (int i = 0; i < _list.Count; i++)
            {
                // 类型不同
                if (! _list [i].IsString)
                    continue;

                // 相等返回true
                if (_list [i].AsString == val)
                    return i;
            }

            // 返回数据
            return -1;
        }

        /// <summary>
        /// 在array中查询指定元素的所在处索引
        /// </summary>
        public int IndexOf(LPCValue val)
        {
            // 都是null返回-1
            if (_list == null || val == null)
                return -1;

            // int类型
            if (val.IsInt)
                return IndexOf(val.AsInt);
            // float类型
            else if (val.IsFloat)
                return IndexOf(val.AsFloat);
            // string类型
            else if (val.IsString)
                return IndexOf(val.AsString);

            // 返回数据
            return _list.IndexOf(val);
        }

        /// <summary>
        /// 通过下标索引来访问数据
        /// </summary>
        public LPCValue this [int idx]
        {
            get { return _list [idx]; }
            set { _list [idx] = value; }
        }

        /// <summary>
        /// 通过这个可以使用foreach语句
        /// </summary>
        public IEnumerable<LPCValue> Values
        {
            get
            {
                if (_list != null)
                {
                    for (int i = 0; i < _list.Count; i++)
                        yield return _list [i];
                }
            }
        }

        /// <summary>
        /// 获取打印用的，具有可读性的字符串
        /// </summary>
        public string _GetDescription(int depth)
        {
            string blank = LPCUtil.GetSpace(4 * depth);
            string blankNext = LPCUtil.GetSpace(4 * (depth + 1));

            // 如果没有成员，那么我们打印简单的形式
            if (Count == 0)
                return string.Format("{0}{1} {2}", blank, "({", "})");

            // 如果有成员，那么打印稍微复杂点的形式
            string str = string.Format("{0}{1} /* sizeof=={2} */ \n", blank, "({", this.Count);

            // 遍历每一个成员
            for (int i = 0; i < _list.Count; i++)
            {
                string childStr = _list [i]._GetDescription(depth + 1).Trim();
                str += string.Format("{0}{1},\n", blankNext, childStr);

                // 检查一下，不要太长
                if (str.Length > 40000)
                {
                    str += string.Format("{0}...\n", blankNext);
                    return str;
                }
            }

            // 结束符号
            str += string.Format("{0}{1}\n", blank, "})");
            return str;
        }

#if UNITY_EDITOR
        // 调试用的内容显示
        public string 数据信息 { get { return _GetDescription(3); } }
#endif

        // 确保list被创建了
        private void confirm_list()
        {
            if (_list == null)
                _list = new List<LPCValue>();
        }


        /// <summary>
        /// 返回指定数组中int或float的最小值所在的数组下标.
        /// </summary>
        /// <returns>最小值所在的下标;没有有效值的时候，返回-1</returns>
        public int ArrayMinNumberIndex()
        {
            int retIndex = -1;
            float temp = (float)float.MaxValue;
            float curValue = 0f;
            for (int index = 0; index < this.Count; index++)
            {
                if (this[index].type == LPCValue.ValueType.INT)
                {
                    curValue = (float)this[index].AsInt;

                    if (curValue < temp)
                    {
                        retIndex = index;
                        temp = curValue;
                    }
                }
                else if (this[index].type == LPCValue.ValueType.FLOAT)
                {
                    curValue = (float)this[index].AsFloat;

                    if (curValue < temp)
                    {
                        retIndex = index;
                        temp = curValue;
                    }
                }
            }

            return retIndex;
        }

        /// <summary>
        /// Min this instance.
        /// </summary>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public float MinFloat()
        {
            // 必须满足条件
            if (this.Count == 0)
                return 0f;

            // 初始化curValue
            float curFloatValue = this[0].AsFloat;
            float tempFloatValue = 0f;

            // 遍历列表
            for (int index = 1; index < this.Count; index++)
            {
                // 获取当前值
                tempFloatValue = this[index].AsFloat;

                // 记录数据
                if (curFloatValue > tempFloatValue)
                    curFloatValue = tempFloatValue;
            }

            // 返回当前最小数值
            return curFloatValue;
        }

        /// <summary>
        /// Min this instance.
        /// </summary>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public int MinInt()
        {
            // 必须满足条件
            if (this.Count == 0)
                return 0;

            // 初始化curValue
            int curIntValue = this[0].AsInt;
            int tempIntValue = 0;

            // 遍历列表
            for (int index = 1; index < this.Count; index++)
            {
                // 获取当前值
                tempIntValue = this[index].AsInt;

                // 记录数据
                if (curIntValue > tempIntValue)
                    curIntValue = tempIntValue;
            }

            // 返回当前最小数值
            return curIntValue;
        }

        /// <summary>
        /// Max this instance.
        /// </summary>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public float MaxFloat()
        {
            // 必须满足条件
            if (this.Count == 0)
                return 0f;

            // 初始化curValue
            float curFloatValue = this[0].AsFloat;
            float tempFloatValue = 0f;

            // 遍历列表
            for (int index = 1; index < this.Count; index++)
            {
                // 获取当前值
                tempFloatValue = this[index].AsFloat;

                // 记录数据
                if (curFloatValue < tempFloatValue)
                    curFloatValue = tempFloatValue;
            }

            // 返回当前最大数值
            return curFloatValue;
        }

        /// <summary>
        /// Max this instance.
        /// </summary>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public int MaxInt()
        {
            // 必须满足条件
            if (this.Count == 0)
                return 0;

            // 初始化curValue
            int curIntValue = this[0].AsInt;
            int tempIntValue = 0;

            // 遍历列表
            for (int index = 1; index < this.Count; index++)
            {
                // 获取当前值
                tempIntValue = this[index].AsInt;

                // 记录数据
                if (curIntValue < tempIntValue)
                    curIntValue = tempIntValue;
            }

            // 返回当前最大数值
            return curIntValue;
        }

        /// <summary>
        /// 返回指定数组中int或float的最小值所在的数组下标.
        /// </summary>
        /// <returns>最小值所在的下标;没有有效值的时候，返回-1</returns>
        public int ArrayMaxNumberIndex()
        {
            int retIndex = -1;
            float temp = (float)float.MinValue;
            float curValue = 0f;
            for (int index = 0; index < this.Count; index++)
            {
                if (this[index].type == LPCValue.ValueType.INT)
                {
                    curValue = (float)this[index].AsInt;

                    if (curValue > temp)
                    {
                        retIndex = index;
                        temp = curValue;
                    }
                }
                else if (this[index].type == LPCValue.ValueType.FLOAT)
                {
                    curValue = (float)this[index].AsFloat;

                    if (curValue > temp)
                    {
                        retIndex = index;
                        temp = curValue;
                    }
                }
            }

            return retIndex;
        }
    }
}
