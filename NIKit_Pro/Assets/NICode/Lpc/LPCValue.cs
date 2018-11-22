/// <summary>
/// LPCValue.cs
/// Copy from zhangyg 2014-10-22
/// LPC数据类型
/// </summary>

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace LPC
{
    /// <summary>
    /// 对应于LPC的数据类型
    /// </summary>
    public class LPCValue
    {
        interface ILPCValueImpl
        {
            ValueType Type();
            bool IsInt();
            bool IsFloat();
            bool IsArray();
            bool IsMapping();
            bool IsString();
            bool IsBuffer();
        }

        class LPCValueImpl<T> : ILPCValueImpl
        {
            public T value = default(T);
            public ValueType Type()
            {
                if (IsInt())
                    return ValueType.INT;
                else if (IsFloat())
                    return ValueType.FLOAT;
                else if (IsString())
                    return ValueType.STRING;
                else if (IsBuffer())
                    return ValueType.BUFFER;
                else if (IsArray())
                    return ValueType.ARRAY;
                else if (IsMapping())
                    return ValueType.MAPPING;
                else
                    return ValueType.UNDEFINED;
            }
            public bool IsInt()
            {
                return typeof(T) == typeof(int);
            }
            public bool IsFloat()
            {
                return typeof(T) == typeof(float);
            }
            public bool IsArray()
            {
                return typeof(T) == typeof(LPCArray);
            }
            public bool IsMapping()
            {
                return typeof(T) == typeof(LPCMapping);
            }
            public bool IsString()
            {
                return typeof(T) == typeof(string);
            }
            public bool IsBuffer()
            {
                return typeof(T) == typeof(byte[]);
            }
        }

        // 数据的类型，默认为整型
        private ILPCValueImpl mValue = null;

        #region 判断是不是某一种类型

        /// <summary>
        /// 定义LPC的7种数据类型
        /// </summary>
        public enum ValueType
        {
            UNDEFINED = 0,
            INT = 2,
            FLOAT = 3,
            STRING = 4,
            BUFFER = 5,
            ARRAY = 6,
            MAPPING = 7,
        }

        /// <summary>
        /// 取得数据类型
        /// </summary>
        public ValueType type { get { return IsUndefined ? ValueType.UNDEFINED : mValue.Type(); } }

        public bool IsUndefined
        {
            get { return mValue == null; }
        }
        public bool IsInt
        {
            get { return mValue != null && mValue.IsInt(); }
        }
        public bool IsFloat
        {
            get { return mValue != null && mValue.IsFloat(); }
        }
        public bool IsArray
        {
            get { return mValue != null && mValue.IsArray(); }
        }
        public bool IsMapping
        {
            get { return mValue != null && mValue.IsMapping(); }
        }
        public bool IsString
        {
            get { return mValue != null && mValue.IsString(); }
        }
        public bool IsBuffer
        {
            get { return mValue != null && mValue.IsBuffer(); }
        }
        #endregion

        #region 作为某一种类型使用
        public int AsInt
        {
            get
            {
                if (IsInt)
                    return (mValue as LPCValueImpl<int>).value;
                else if (IsFloat)
                    return (int)(mValue as LPCValueImpl<float>).value;
                else if (mValue == null || IsString && string.IsNullOrEmpty(AsString))
                    return 0;
                else
                {
                    NIDebug.LogError("LPCValue不是int");
                    return 0;
                }
            }
            set
            {
                if (mValue == null)
                    mValue = new LPCValueImpl<int>();
                (mValue as LPCValueImpl<int>).value = value;
            }
        }

        public float AsFloat
        {
            get
            {
                if (IsFloat)
                    return (mValue as LPCValueImpl<float>).value;
                else if (IsInt)
                    return (float)(mValue as LPCValueImpl<int>).value;
                else if (mValue == null || IsString && string.IsNullOrEmpty(AsString))
                    return 0f;
                else
                {
                    NIDebug.LogError("LPCValue不是float");
                    return 0f;
                }
            }
            set
            {
                if (mValue == null)
                    mValue = new LPCValueImpl<float>();
                (mValue as LPCValueImpl<float>).value = value;
            }
        }

        public string AsString
        {
            get
            {
                if (IsString)
                    return (mValue as LPCValueImpl<string>).value;
                else
                    return GetDescription();
            }
            set
            {
                if (mValue == null)
                    mValue = new LPCValueImpl<string>();
                (mValue as LPCValueImpl<string>).value = value;
            }
        }

        public byte[] AsBuffer
        {
            get
            {
                if (IsBuffer)
                    return (mValue as LPCValueImpl<byte[]>).value;
                else if (mValue == null || IsString && string.IsNullOrEmpty(AsString))
                    return null;
                else
                {
                    NIDebug.LogError("LPCValue不是buffer");
                    return null;
                }
            }
            set
            {
                if (mValue == null)
                    mValue = new LPCValueImpl<byte[]>();
                (mValue as LPCValueImpl<byte[]>).value = value;
            }
        }

        public LPCArray AsArray
        {
            get
            {
                if (IsArray)
                    return (mValue as LPCValueImpl<LPCArray>).value;
                else if (mValue == null || IsString && string.IsNullOrEmpty(AsString))
                    return null;
                else
                {
                    NIDebug.LogError("LPCValue不是array");
                    return null;
                }
            }
            set
            {
                if (mValue == null)
                    mValue = new LPCValueImpl<LPCArray>();
                (mValue as LPCValueImpl<LPCArray>).value = value;
            }
        }

        public LPCMapping AsMapping
        {
            get
            {
                if (IsMapping)
                    return (mValue as LPCValueImpl<LPCMapping>).value;
                else if (mValue == null || IsString && string.IsNullOrEmpty(AsString))
                    return null;
                else
                {
                    NIDebug.LogError("LPCValue不是mapping");
                    return null;
                }
            }
            set
            {
                if (mValue == null)
                    mValue = new LPCValueImpl<LPCMapping>();
                (mValue as LPCValueImpl<LPCMapping>).value = value;
            }
        }

        /// <summary>
        /// 转换为指定类型数据
        /// </summary>
        public T As<T>()
        {
            LPCValueImpl<T> val = mValue as LPCValueImpl<T>;

            // 类型相同
            if (val != null)
                return val.value;

            // LPCValue不能转换为指定类型，返回默认值
            throw new Exception(string.Format("LPCValue不是{0}", typeof(T).Name));
        }

        #endregion

        /// <summary>
        /// 判断两个LPCValue是否相等
        /// </summary>
        /// <param name="obj">
        /// 待比较的值
        /// </param>
        /// <returns>
        /// 如果相等则返回true，否则返回false
        /// </returns>
        public static bool operator ==(LPCValue a, LPCValue b)
        {
            bool an = object.Equals(a, null);
            bool bn = object.Equals(b, null);
            if (an && bn)
                return true;
            if (an || bn)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(LPCValue a, LPCValue b)
        {
            bool an = object.Equals(a, null);
            bool bn = object.Equals(b, null);
            if (an && bn)
                return false;
            if (an || bn)
                return true;
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var lpc = obj as LPCValue;

            if (this.type != lpc.type)
                return false;
            switch (this.type)
            {
                case ValueType.UNDEFINED:
                    return lpc.IsUndefined;
                case ValueType.INT:
                    return this.AsInt == lpc.AsInt;
                case ValueType.FLOAT:
                    return this.AsFloat == lpc.AsFloat;
                case ValueType.STRING:
                    return this.AsString == lpc.AsString;
                case ValueType.BUFFER:
                    {
                        var a = this.AsBuffer;
                        var b = lpc.AsBuffer;
                        if (a.Length != b.Length)
                            return false;
                        if (a.Length > 128)
                            return false;
                        for (int i = 0; i < a.Length; i++)
                            if (a [i] != b [i])
                                return false;
                        return true;
                    }
                case ValueType.ARRAY:
                    {
                        var a = this.AsArray;
                        var b = lpc.AsArray;
                        if (a.Count != b.Count)
                            return false;
                        for (int i = 0; i < a.Count; i++)
                            if (a [i] != b [i])
                                return false;
                        return true;
                    }
                case ValueType.MAPPING:
                    {
                        var a = this.AsMapping;
                        var b = lpc.AsMapping;
                        if (a.Count != b.Count)
                            return false;
                        foreach (object k in a.Keys)
                            if (k is int)
                            {
                                var ik = (int)k;
                                if (a [ik] != b [ik])
                                    return false;
                            } else
                            {
                                var sk = (string)k;
                                if (a [sk] != b [sk])
                                    return false;
                            }
                        return true;
                    }
                default :
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string _GetDescription(int depth)
        {
            if (depth > 10)
                throw new Exception("LPCValue层次太深(depth=" + depth + ")! 可能是出现自引用!");

            string blank = LPCUtil.GetSpace(4 * depth);
            if (IsUndefined)
            {
                return string.Format("{0}(undefined)", blank);
            } else if (IsInt)
            {
                return string.Format("{0}{1}", blank, AsInt);
            } else if (IsFloat)
            {
                return string.Format("{0}{1}", blank, AsFloat);
            } else if (IsString)
            {
                return string.Format("{0}\"{1}\"", blank, AsString);
            } else if (IsBuffer)
            {
                return string.Format("{0}<buffer:{1}>", blank, AsBuffer.Length);
            } else if (IsArray)
            {
                return AsArray._GetDescription(depth);
            } else if (IsMapping)
            {
                return AsMapping._GetDescription(depth);
            }

            Debug.Assert(false, "按理说不应该走到这里");
            return "";
        }

        /// <summary>
        /// 取得LPCValue的字符串描述
        /// </summary>
        public string GetDescription()
        {
            return this._GetDescription(0);
        }

        #region 创建函数

        public static LPCValue Create()
        {
            return new LPCValue();
        }

        /// 创建
        public static LPCValue Create(object val)
        {
            LPCValue v = new LPCValue();
            Type valType = val.GetType();

            if (valType == typeof(int))
            {
                v.AsInt = (int)val;
            }
            else if (valType == typeof(float))
            {
                v.AsFloat = (float)val;
            }
            else if (valType == typeof(string))
            {
                v.AsString = (string)val;
            }
            else if (valType == typeof(byte[]))
            {
                v.AsBuffer = (byte[])val;
            }
            else if (valType == typeof(LPCArray))
            {
                v.AsArray = (LPCArray)val;
            }
            else if (valType == typeof(LPCMapping))
            {
                v.AsMapping = (LPCMapping)val;
            } else
                throw new Exception("不识别的类型");

            return v;
        }

        /// <summary>
        /// 创建array类型
        /// </summary>
        public static LPCValue CreateArray()
        {
            var v = Create(new LPCArray());
            return v;
        }

        /// <summary>
        /// 创建mapping类型
        /// </summary>
        public static LPCValue CreateMapping()
        {
            var v = Create(new LPCMapping());
            return v;
        }

        /// <summary>
        /// 创建BUFFER类型
        /// </summary>
        /// <param name="val">
        /// buffer的初始值
        /// </param>
        /// <returns>
        /// A <see cref="LPCValue"/>
        /// </returns>
        public static LPCValue CreateBuffer(string val)
        {
            var buf = System.Text.ASCIIEncoding.UTF8.GetBytes(val);

            return Create(buf);
        }

        // Duplicate an lpcvalue object
        public static LPCValue Duplicate(LPCMapping val)
        {
            LPCMapping dupVal = new LPCMapping();
            foreach (object key in val.Keys)
            {
                if (key is int)
                {
                    int keyInt = (int)key;
                    dupVal.Add(keyInt, Duplicate(val[keyInt]));
                    continue;
                } else if (key is string)
                {
                    string keyStr = (string)key;
                    dupVal.Add(keyStr, Duplicate(val[keyStr]));
                    continue;
                }
                throw new Exception("Unexpected key type.");
            }

            // 返回数据
            return LPCValue.Create(dupVal);
        }

        // Duplicate an lpcvalue object
        public static LPCValue Duplicate(LPCValue val)
        {
            if (val.IsInt)
                return Create(val.AsInt);
            else if (val.IsFloat)
                return Create(val.AsFloat);
            else if (val.IsString)
                return Create(val.AsString);
            else if (val.IsBuffer)
                return Create((byte[])val.AsBuffer.Clone());
            else if (val.IsArray)
            {
                LPCArray dupVal = LPCArray.Empty;

                foreach (LPCValue element in val.AsArray.Values)
                    dupVal.Add(Duplicate(element));

                return LPCValue.Create(dupVal);
            } else if (val.IsMapping)
            {
                LPCMapping dupVal = LPCMapping.Empty;
                LPCMapping valMap = val.AsMapping;
                foreach (object key in valMap.Keys)
                {
                    if (key is int)
                    {
                        int keyInt = (int)key;
                        dupVal.Add(keyInt, Duplicate(valMap[keyInt]));
                        continue;
                    } else if (key is string)
                    {
                        string keyStr = (string)key;
                        dupVal.Add(keyStr, Duplicate(valMap[keyStr]));
                        continue;
                    }
                    throw new Exception("Unexpected key type.");
                }
                return LPCValue.Create(dupVal);
            } else
                throw new Exception("Unexpected duplicated.");
        }
        #endregion

        #region 序列化

        /// <summary>
        /// 将指定的对象序列化到缓冲区中缓冲区。如果缓冲不够，会抛出异常。
        /// </summary>
        /// <returns>成功保存了多少字节到缓冲区中。</returns>
        public static int SaveToBuffer(byte[] buf, int offset, LPCValue value)
        {
            if (value.IsString)
            {
                byte[] utf8Data = System.Text.Encoding.UTF8.GetBytes(value.AsString);
                int skip = _SaveTypeDataTo((int)value.type, utf8Data.Length, buf, offset);
                offset += skip;
                System.Buffer.BlockCopy(utf8Data, 0, buf, offset, utf8Data.Length);
                return skip + utf8Data.Length;
            } else if (value.IsBuffer)
            {
                int skip = _SaveTypeDataTo((int)value.type, value.AsBuffer.Length, buf, offset);
                offset += skip;
                System.Buffer.BlockCopy(value.AsBuffer, 0, buf, offset, value.AsBuffer.Length);
                return skip + value.AsBuffer.Length;
            } else if (value.IsInt)
            {
                int skip = _SaveTypeDataTo((int)value.type, value.AsInt, buf, offset);
                offset += skip;
                return skip;
            } else if (value.IsFloat)
            {
                int skip = _SaveTypeDataTo((int)value.type, sizeof(float), buf, offset);
                offset += skip;
                byte[] arr = BitConverter.GetBytes(value.AsFloat);
                Debug.Assert(arr.Length == sizeof(float));
                System.Buffer.BlockCopy(arr, 0, buf, offset, sizeof(float));
                return skip + sizeof(float);
            } else if (value.IsUndefined)
            {
                int skip = _SaveTypeDataTo((int)value.type, 0, buf, offset);
                offset += skip;
                return skip;
            } else if (value.IsArray)
            {
                int skip = _SaveTypeDataTo((int)value.type, value.AsArray.Count, buf, offset);
                offset += skip;
                for (int i = 0; i < value.AsArray.Count; i++)
                {
                    int _skip = SaveToBuffer(buf, offset, value.AsArray [i]);
                    skip += _skip;
                    offset += _skip;
                }
                return skip;
            } else if (value.IsMapping)
            {
                int skip = _SaveTypeDataTo((int)value.type, value.AsMapping.Count, buf, offset);
                offset += skip;
                foreach (object key in value.AsMapping.Keys)
                {
                    LPCValue keyValue = null, valValue = null;

                    Debug.Assert((key is int) || (key is string), "不支持非string和int的key值类型。");
                    if (key is int)
                    {
                        keyValue = LPCValue.Create((int)key);
                        valValue = value.AsMapping [(int)key];
                    } else if (key is string)
                    {
                        keyValue = LPCValue.Create((string)key);
                        valValue = value.AsMapping [(string)key];
                    }

                    // 打包key值
                    int _skip = SaveToBuffer(buf, offset, keyValue);
                    skip += _skip;
                    offset += _skip;

                    // 打包value值
                    _skip = SaveToBuffer(buf, offset, valValue);
                    skip += _skip;
                    offset += _skip;
                }
                return skip;
            } else
                throw new Exception("不识别的类型");
        }

        /// <summary>
        /// 通过缓冲区还原回值信息。失败了会抛出异常。
        /// </summary>
        /// <returns>返回有多少字节数被成功解析了。</returns>
        public static int RestoreFromBuffer(byte[] buf, int offset, out LPCValue value)
        {
            int matchCnt = _RestoreFromBuffer(buf, offset, out value);
            return matchCnt;
        }

        /// <summary>
        /// 将类型数据存储到指定的缓冲区中
        /// </summary>
        private static int _SaveTypeDataTo(int type, int len, byte[] buf, int offset)
        {
            Debug.Assert(type >= 0 && type < 15);

            int dataSize = LPCUtil.GetSaveBinaryIntSize(len);
            Debug.Assert(dataSize <= 15);

            // 打包第一个字节
            byte byte1 = ((byte)(type << 4));
            byte byte2 = ((byte)dataSize);
            buf [offset + 0] = (byte)(byte1 | byte2);

            // 打包长度，little endian
            for (int i = dataSize; i >= 1; i--)
            {
                buf [offset + i] = (byte)(len & 0xFF);
                len >>= 8;
            }

            // 保存了多少长度
            return dataSize + 1;
        }

        /// <summary>
        /// 仅供内部调用的递归版本
        /// </summary>
        private static int _RestoreFromBuffer(byte[] buf, int offset, out LPCValue value)
        {
            // 匹配到了多少个
            int matchCnt = 0;

            // 获取 类型 和 数据大小
            int _type = buf [offset++];
            int dataSize = _type & 0x0F;
            ValueType type = (ValueType)(_type >> 4);
            matchCnt++;

            // 看是否需要获得额外的数据
            int data = 0;
            if (dataSize > 0)
            {
                /* Set 0 for positive, 0xFFFF.... for negative */
                data = (buf [offset] & 0x80) != 0 ? -1 : 0;

                /* Get low n bytes of integer */
                for (int i = 0; i < dataSize; i++)
                {
                    data <<= 8;
                    data |= buf [offset + i];
                }

                offset += dataSize;
                matchCnt += dataSize;
            }

            // 根据是否为各种类型，来恢复数据
            switch (type)
            {
                case ValueType.UNDEFINED:
                    {
                        value = LPCValue.Create();
                        return matchCnt;
                    }
                case ValueType.STRING:
                    {
                        string val = System.Text.Encoding.UTF8.GetString(buf, offset, data);
                        value = LPCValue.Create(val);
                        matchCnt += data;
                        return matchCnt;
                    }
                case ValueType.BUFFER:
                    {
                        byte[] _buf = new byte[data];
                        System.Buffer.BlockCopy(buf, offset, _buf, 0, data);
                        value = LPCValue.Create(_buf);
                        matchCnt += data;
                        return matchCnt;
                    }
                case ValueType.INT:
                    {
                        value = LPCValue.Create(data);
                        return matchCnt;
                    }
                case ValueType.FLOAT:
                    {
                        Debug.Assert(data == sizeof(float) || data == sizeof(double));

                        float val = 0;
                        if (data == sizeof(float))
                            val = BitConverter.ToSingle(buf, offset);
                        else if (data == sizeof(double))
                            val = (float)BitConverter.ToDouble(buf, offset);

                        value = LPCValue.Create(val);
                        matchCnt += data;
                        return matchCnt;
                    }
                case ValueType.ARRAY:
                    {
                        LPCValue val = LPCValue.CreateArray();
                        for (int i = 0; i < data; i++)
                        {
                            LPCValue subVal;
                            int cnt = _RestoreFromBuffer(buf, offset, out subVal);
                            offset += cnt;
                            matchCnt += cnt;
                            val.AsArray.Add(subVal);
                        }
                        value = val;
                        return matchCnt;
                    }
                case ValueType.MAPPING:
                    {
                        LPCValue val = LPCValue.CreateMapping();
                        for (int i = 0; i < data; i++)
                        {
                            LPCValue keyVal, valVal;

                            int cnt1 = _RestoreFromBuffer(buf, offset, out keyVal);
                            offset += cnt1;
                            matchCnt += cnt1;

                            int cnt2 = _RestoreFromBuffer(buf, offset, out valVal);
                            offset += cnt2;
                            matchCnt += cnt2;

                            // 我们只支持key类型为int和string的
                            if (keyVal.IsInt)
                                val.AsMapping.Add(keyVal.AsInt, valVal);
                            else if (keyVal.IsString)
                                val.AsMapping.Add(keyVal.AsString, valVal);
                            else
                                throw new Exception("不支持的key类型。");
                        }
                        value = val;
                        return matchCnt;
                    }
                default:
                    {
                        throw new Exception("不支持的Value类型。");
                    }
            } // switch case 结束
        }

        #endregion
    }
}
