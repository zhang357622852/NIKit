/// <summary>
/// LPCMapping.cs
/// Copy from zhangyg 2014-10-22
/// LPC mapping数据类型
/// </summary>

using System;
using System.Collections.Generic;

namespace LPC
{
    /// <summary>
    /// 对应于LPC的mapping数据结构
    /// 不过我们只实现key是int或者string的情形
    /// </summary>
    public class LPCMapping
    {
        public static LPCMapping Empty{ get { return new LPCMapping(); } }

        // 内部使用字典来实现模拟
        private Dictionary<int, LPC.LPCValue> _mapInt = null;
        private Dictionary<string, LPC.LPCValue> _mapStr = null;

        /// <summary>
        /// 这个mapping含有多少元素
        /// </summary>
        public int Count { get { return (_mapInt == null ? 0 : _mapInt.Count) +
                (_mapStr == null ? 0 : _mapStr.Count); } }

        /// <summary>
        /// 增加一个键值对
        /// </summary>
        public void Add(string key, object value)
        {
            confirm_map_str();

            // 重置数据
            if (! (value is LPCValue))
                _mapStr[key] = LPCValue.Create(value);
            else
                _mapStr[key] = value as LPCValue;
        }

        /// <summary>
        /// 增加一个键值对
        /// </summary>
        public void Add(string key, LPCValue value)
        {
            confirm_map_str();

            // 重置数据
            _mapStr[key] = value;
        }

        /// <summary>
        /// 增加一个键值对
        /// </summary>
        public void Add(int key, object value)
        {
            confirm_map_int();

            // 重置数据
            if (! (value is LPCValue))
                _mapInt[key] = LPCValue.Create(value);
            else
                _mapInt[key] = value as LPCValue;
        }

        /// <summary>
        /// 增加一个键值对
        /// </summary>
        public void Add(int key, LPCValue value)
        {
            confirm_map_int();

            // 重置数据
            _mapInt[key] = value;
        }

        /// <summary>
        /// 拷贝mapping到指定mapping中
        /// </summary>
        public void CopyTo(List<string> keyList, LPCMapping copyToMap)
        {
            // 没有数据
            if (_mapStr == null)
                return;

            LPCValue tValue;
            string keyStr = string.Empty;

            // 逐个元素Copy
            for(int i = 0; i < keyList.Count; i++)
            {
                keyStr = keyList[i];
                if (! _mapStr.TryGetValue(keyStr, out tValue))
                    continue;

                // copy数据
                copyToMap.Add(keyStr, tValue);
            }
        }

        /// <summary>
        /// 拷贝mapping到指定mapping中
        /// </summary>
        public void CopyTo(List<int> keyList, LPCMapping copyToMap)
        {
            // 没有数据
            if (_mapInt == null)
                return;

            LPCValue tValue;
            int keyInt = 0;

            // 逐个元素Copy
            for(int i = 0; i < keyList.Count; i++)
            {
                keyInt = keyList[i];
                if (! _mapInt.TryGetValue(keyInt, out tValue))
                    continue;

                // copy数据
                copyToMap.Add(keyInt, tValue);
            }
        }

        /// <summary>
        /// 拷贝mapping
        /// </summary>
        public LPCMapping Copy()
        {
            LPCMapping dst = new LPCMapping();

            // 构建数据
            if (_mapInt != null)
            {
                foreach(int intKey in _mapInt.Keys)
                    dst.Add(intKey, _mapInt[intKey]);
            }

            if (_mapStr != null)
            {
                foreach(string strKey in _mapStr.Keys)
                    dst.Add(strKey, _mapStr[strKey]);
            }

            // 返回数据
            return dst;
        }

        /// <summary>
        /// Append the specified array.
        /// </summary>
        public void Append(LPCMapping data)
        {
            foreach(object key in data.Keys)
            {
                // key为int
                if (key is int)
                {
                    this.Add((int) key, data[(int) key]);
                    continue;
                }

                // key为stirng
                if (key is string)
                {
                    this.Add((string) key, data[(string) key]);
                    continue;
                }
            }
        }

        /// <summary>
        /// 移除指定的键
        /// </summary>
        public void Remove(string key)
        {
            if (_mapStr != null)
                _mapStr.Remove(key);
        }

        /// <summary>
        /// 移除指定的键
        /// </summary>
        public void Remove(int key)
        {
            if (_mapInt != null)
                _mapInt.Remove(key);
        }

        /// <summary>
        /// 通过下标运算符访问映射表
        /// </summary>
        public LPCValue GetItem(string key)
        {
            // 如果没有_mapStr，直接返回默认值
            if (_mapStr == null)
                return null;

            // 没有指定key信息，返回默认值
            LPCValue value;
            if (! _mapStr.TryGetValue(key, out value))
                return null;

            return value;
        }

        /// <summary>
        /// 通过下标运算符访问映射表
        /// </summary>
        public LPCValue GetItem(int key)
        {
            // 如果没有_mapInt，直接返回默认值
            if (_mapInt == null)
                return null;

            // 没有指定key信息，返回默认值
            LPCValue value;
            if (! _mapInt.TryGetValue(key, out value))
                return null;

            return value;
        }

        /// <summary>
        /// 通过下标运算符访问映射表
        /// </summary>
        public LPCValue this[string key]
        {
            get
            {
                // 如果没有_mapStr，直接返回默认值
                if (_mapStr == null)
                    return null;

                // 没有指定key信息，返回默认值
                LPCValue value;
                if (! _mapStr.TryGetValue(key, out value))
                    return null;

                return value;
            }
            set { confirm_map_str(); this.Add(key, value); }
        }

        /// <summary>
        /// 通过下标运算符访问映射表
        /// </summary>
        public LPCValue this[int key]
        {
            get
            {
                // 如果没有_mapInt，直接返回默认值
                if (_mapInt == null)
                    return null;

                // 没有指定key信息，返回默认值
                LPCValue value;
                if (! _mapInt.TryGetValue(key, out value))
                    return null;

                return value;
            }
            set { confirm_map_int(); this.Add(key, value); }
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        public T GetValue<T>(int key, T defaultValue = default(T))
        {
            // 如果没有_mapInt，直接返回默认值
            if (_mapInt == null)
                return defaultValue;

            // 没有指定key信息，返回默认值
            LPCValue value;
            if (! _mapInt.TryGetValue(key, out value))
                return defaultValue;

            // 如果是LPCValue类型
            if (typeof(T) == typeof(LPCValue))
                return (T)(object)value;

            // 返回目标类型数据
            return value.As<T>();
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            // 如果没有_mapStr，直接返回默认值
            if (_mapStr == null)
                return defaultValue;

            // 没有指定key信息，返回默认值
            LPCValue value;
            if (! _mapStr.TryGetValue(key, out value))
                return defaultValue;

            // 如果是LPCValue类型
            if (typeof(T) == typeof(LPCValue))
                return (T)(object)value;

            // 返回目标类型数据
            return value.As<T>();
        }

        /// <summary>
        /// 测试是否含有指定的key值
        /// </summary>
        public bool ContainsKey(string key)
        {
            return (_mapStr != null) && _mapStr.ContainsKey(key);
        }

        /// <summary>
        /// 测试是否含有指定的key值
        /// </summary>
        public bool ContainsKey(int key)
        {
            return (_mapInt != null) && _mapInt.ContainsKey(key);
        }

        /// <summary>
        /// 测试是否含有指定的value值
        /// </summary>
        public bool ContainsValue(LPCValue val)
        {
            if ((_mapStr != null) && _mapStr.ContainsValue(val))
                return true;
            if ((_mapInt != null) && _mapInt.ContainsValue(val))
                return true;
            return false;
        }

        /// <summary>
        /// 通过这个可以使用foreach语句，遍历所有的key值
        /// </summary>
        public List<object> GetKeys()
        {
            List<object> keys = new List<object>();

            if (_mapInt != null)
            {
                foreach (int key in _mapInt.Keys)
                    keys.Add(key);
            }

            if (_mapStr != null)
            {
                foreach (string key in _mapStr.Keys)
                    keys.Add(key);
            }

            return keys;
        }

        /// <summary>
        /// 通过这个可以使用foreach语句，遍历所有的key值
        /// </summary>
        public List<LPCValue> GetValues()
        {
            List<LPCValue> values = new List<LPCValue>();

            if (_mapInt != null)
            {
                foreach (LPCValue value in _mapInt.Values)
                    values.Add(value);
            }

            if (_mapStr != null)
            {
                foreach (LPCValue value in _mapStr.Values)
                    values.Add(value);
            }

            return values;
        }

        /// <summary>
        /// 通过这个可以使用foreach语句，遍历所有的key值
        /// </summary>
        public IEnumerable<object> Keys
        {
            get
            {
                if (_mapInt != null)
                {
                    foreach (int key in _mapInt.Keys)
                        yield return key;
                }

                if (_mapStr != null)
                {
                    foreach (string key in _mapStr.Keys)
                        yield return key;
                }
            }
        }

        /// <summary>
        /// 通过这个可以使用foreach语句，遍历所有的value值
        /// </summary>
        public IEnumerable<LPCValue> Values
        {
            get
            {
                if (_mapInt != null)
                {
                    foreach (LPCValue val in _mapInt.Values)
                        yield return val;
                }

                if (_mapStr != null)
                {
                    foreach (LPCValue val in _mapStr.Values)
                        yield return val;
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

            // 如果为空mapping，那么我们打印一种简单的形式
            if (this.Count == 0)
                return string.Format("{0}([ ])", blank);

            string str = string.Format("{0}([ /* sizeof=={1} */\n", blank, this.Count);

            // 遍历每一个成员 int
            if (_mapInt != null)
            {
                foreach (int key in _mapInt.Keys)
                {
                    string childValueStr = _mapInt[key]._GetDescription(depth + 1).TrimStart();

                    str += string.Format("{0}{1} : {2},\n", blankNext, key, childValueStr);

                    // 检查一下，不要太长
                    if (str.Length > 40000)
                    {
                        str += string.Format("{0}...\n", blankNext);
                        return str;
                    }
                }
            }

            // 遍历每一个成员 string
            if (_mapStr != null)
            {
                foreach (string key in _mapStr.Keys)
                {
                    string childValueStr = _mapStr[key]._GetDescription(depth + 1).Trim();

                    str += string.Format("{0}\"{1}\" : {2},\n", blankNext, key, childValueStr);

                    // 检查一下，不要太长
                    if (str.Length > 40000)
                    {
                        str += string.Format("{0}...\n", blankNext);
                        return str;
                    }
                }
            }

            // 结束符号
            str += string.Format("{0}])\n", blank);
            return str;
        }

#if UNITY_EDITOR
        // 调试用的内容显示
        public string 数据信息 { get { return _GetDescription(3); } }
#endif

        private void confirm_map_int()
        {
            if (_mapInt == null)
                _mapInt = new Dictionary<int, LPCValue>();
        }

        private void confirm_map_str()
        {
            if (_mapStr == null)
                _mapStr = new Dictionary<string, LPCValue>();
        }
    }
}
