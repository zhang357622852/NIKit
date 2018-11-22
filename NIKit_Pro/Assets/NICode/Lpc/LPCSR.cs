/// <summary>
/// LPCSR.cs
/// Copy from zhangyg 2014-10-22
/// LPCValue转换为字符串
/// </summary>

using System;
using System.Collections.Generic;

namespace LPC
{
    /// <summary>
    /// LPCValue转换为字符串
    /// </summary>
    public class LPCSaveString
    {
        public static string SaveToString(int val)
        {
            return Convert.ToString(val);
        }
        public static string SaveToString(float val)
        {
            return Convert.ToString(val);
        }
        public static string SaveToString(string val)
        {
            val = val.Replace("\\", "\\\\");
            val = val.Replace("\"", "\\\"");
            val = val.Replace("\n", "\r");
            return "\"" + val + "\"";
        }

        public static string SaveToString(LPCValue val)
        {
            string result = "";

            if (val.IsString)
            {
                return SaveToString(val.AsString);
            } else if (val.IsBuffer)
            {
                result = ":";
                byte[] bytes = val.AsBuffer;
                for (int i = 0; i < bytes.Length; i++)
                {
                    result += string.Format("{0:X}", bytes [i]);
                }
                return result + ":";
            } else if (val.IsInt)
            {
                return Convert.ToString(val.AsInt);
            } else if (val.IsArray)
            {
                result = "({";
                foreach (LPCValue v in val.AsArray.Values)
                {
                    result += SaveToString(v) + ",";
                }
                return result + "})";
            } else if (val.IsMapping)
            {
                result = "([";
                foreach (object k in val.AsMapping.Keys)
                {
                    if (k is int)
                    {
                        result += SaveToString((int)k) + ":";
                        result += SaveToString(val.AsMapping [(int)k]) + ",";
                        continue;
                    }
                    if (k is string)
                    {
                        result += SaveToString((string)k) + ":";
                        result += SaveToString(val.AsMapping [(string)k]) + ",";
                        continue;
                    }
                }

                return result + "])";
            } else if (val.IsFloat)
            {
                return SaveToString(val.AsFloat);
            } else if (val.IsUndefined)
            {
                return "(undefined)";
            } else
            {
                throw new Exception("未知LPC类型");
            }
        }
    }

    /// <summary>
    /// 字符串转换为LPCValue
    /// </summary>
    public class LPCRestoreString
    {
        public static LPCValue RestoreFromString(string content)
        {
            if (string.IsNullOrEmpty(content))
                return LPCValue.Create("");

            int offset = 0;
            switch (content [offset])
            {
                case '"':
                    {
                        offset++;
                        return RestoreString(ref content, ref offset);
                    }
                case '(':
                    {
                        offset++;
                        if (content [offset] == '{')
                        {
                            offset++;
                            return RestoreArray(ref content, ref offset);
                        }
                        if (content [offset] == '[')
                        {
                            offset++;
                            return RestoreMapping(ref content, ref offset);
                        }
                    }
                    break;
                case '-':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    {
                        return RestoreNumber(ref content, ref offset);
                    }
                case ':':
                    {
                        offset++;
                        return RestoreBuffer(ref content, ref offset);
                    }
                default:
                    {
                        throw new Exception("Bad format for convert to LPCValue");
                    }
            }

            return null;
        }

        /// <summary>
        /// 是否需要SafeRestoreFromString
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool IsSafeRestoreFromString(string content)
        {
            if (string.IsNullOrEmpty(content))
                return false;

            int offset = 0;
            switch (content[offset])
            {
                case '"':
                    return true;

                case '(':
                    {
                        offset++;
                        if (content[offset] == '{')
                        {
                            offset++;
                            if (RestoreArray(ref content, ref offset).IsArray)
                                return true;
                            else
                                return false;
                        }
                        if (content[offset] == '[')
                        {
                            offset++;
                            if (RestoreMapping(ref content, ref offset).IsMapping)
                                return true;
                            else
                                return false;
                        }

                        return false;
                    }

                case '-':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    {
                        offset++;
                        int numCount = 0;
                        int len = content.Length;

                        while (offset < len)
                        {
                            if (content[offset] == '0' || content[offset] == '1' || content[offset] == '2' || content[offset] == '3' ||
                                content[offset] == '4' || content[offset] == '5' || content[offset] == '6' || content[offset] == '7' || content[offset] == '8' ||
                                content[offset] == '9')
                                numCount++;

                            offset++;
                        }

                        // 纯数字 -878789  656567
                        if ((numCount + 1) == len)
                            return true;
                        else
                            return false;
                    }
                case ':':
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        public static LPCValue SafeRestoreFromString(string content)
        {
            if (string.IsNullOrEmpty(content))
                return LPCValue.Create("");

            int offset = 0;
            switch (content [offset])
            {
                case '"':
                    {
                        offset++;
                        return RestoreString(ref content, ref offset);
                    }
                case '(':
                    {
                        offset++;
                        if (content [offset] == '{')
                        {
                            offset++;
                            return RestoreArray(ref content, ref offset);
                        }
                        if (content [offset] == '[')
                        {
                            offset++;
                            return RestoreMapping(ref content, ref offset);
                        }
                    }
                    break;
                case '-':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    {
                        return RestoreNumber(ref content, ref offset);
                    }
                case ':':
                    {
                        offset++;
                        return RestoreBuffer(ref content, ref offset);
                    }
                default:
                    {
                        // 其他情况直接返回content
                        return LPCValue.Create(content);
                    }
            }

            return null;
        }

        private static LPCValue RestoreString(ref string content, ref int offset)
        {
            int len = content.Length;
            int start_offset = offset;
            int count = 0;
            while (offset < len)
            {
                char c = content [offset];
                if (c == '"')
                {
                    offset++;
                    break;
                }
                if (c == '\\')
                {
                    offset += 2;
                    count += 2;
                    continue;
                }

                offset++;
                count++;
            }

            string str = content.Substring(start_offset, count);
            str.Replace("\\\"", "\"");
            str.Replace("\\\\", "\\");
            str.Replace("\r", "\n");
            LPCValue v = LPCValue.Create(str);
            return v;
        }

        // "1000", "100.38","0.98","-1000", "-100.38", "-0.98"
        private static LPCValue RestoreNumber(ref string content, ref int offset)
        {
            int len = content.Length;
            int start_offset = offset;
            bool is_float = false;
            int count = 0;
            while (offset < len)
            {
                char c = content [offset];
                if (c == '.')
                {
                    is_float = true;
                }

                if ((c == '-') || (c == '.') || ((c >= '0') && (c <= '9')))
                {
                    offset++;
                    count++;
                    continue;
                }

                break;
            }

            string numberstr = content.Substring(start_offset, count);
            if (is_float)
            {
                float fval = (float)System.Convert.ToDouble(numberstr);
                return LPCValue.Create(fval);
            }

            int ival = System.Convert.ToInt32(numberstr);
            return LPCValue.Create(ival);
        }

        private static LPCValue RestoreAlias(ref string content, ref int offset)
        {
            int start_offset = offset;
            int len = content.Length;
            int count = 0;
            while (offset < len)
            {
                int c = content [offset];

                // alias maybe 'A'-'Z', '0'-'9','(',')'
                if ((c >= 'A' && c <= 'Z') ||
                    (c >= '0' && c <= '9') ||
                    (c == '(') ||
                    (c == ')') ||
                    (c == '_'))
                {
                    offset++;
                    count++;
                    continue;
                }

                break;
            }
            string alias_name = content.Substring(start_offset, count);
            if (! AliasMgr.ContainsAlias(alias_name))
            {
                return LPC.LPCValue.Create("@" + alias_name);
            }

            object v = AliasMgr.Get(alias_name);
            if (v is string)
                return LPC.LPCValue.Create((string)v);
            if (v is int)
                return LPC.LPCValue.Create((int)v);

            throw new Exception(string.Format("Unexpected alias name: {0}", alias_name));
        }

        private static LPCValue RestoreBuffer(ref string content, ref int offset)
        {
            List<byte> bytes = new List<byte>();
            int len = content.Length;
            bool ending_flag = false;
            while (offset < len)
            {
                switch (content [offset])
                {
                    case ':':
                        offset++;
                        ending_flag = true;
                        break;
                    default:
                        {
                            int v1 = content [offset];
                            offset++;
                            int v2 = content [offset];
                            offset++;
                            if (v1 >= '0' && v1 <= '9')
                                v1 = v1 - '0';
                            if (v1 >= 'A' && v1 <= 'F')
                                v1 = v1 - 'A' + 10;
                            if (v2 >= '0' && v2 <= '9')
                                v2 = v2 - '0';
                            if (v2 >= 'A' && v2 <= 'F')
                                v2 = v2 - 'A' + 10;
                            int val = v1 * 16 + v2;
                            bytes.Add((byte)val);
                        }
                        break;
                }

                if (ending_flag)
                    break;
            }

            return LPCValue.Create(bytes.ToArray());
        }

        private static LPCValue RestoreArray(ref string content, ref int offset)
        {
            int len = content.Length;
            LPCValue arr = LPCValue.CreateArray();
            while (offset < len)
            {
                switch (content [offset])
                {
                    case '"':
                        {
                            offset++;
                            LPCValue v = RestoreString(ref content, ref offset);
                            arr.AsArray.Add(v);
                        }
                        break;
                    case ':':
                        {
                            offset++;
                            LPCValue v = RestoreBuffer(ref content, ref offset);
                            arr.AsArray.Add(v);
                        }
                        break;
                    case '(':
                        offset++;
                        if (content [offset] == '{')
                        {
                            offset++;
                            LPCValue v = RestoreArray(ref content, ref offset);
                            arr.AsArray.Add(v);
                        } else if (content [offset] == '[')
                        {
                            offset++;
                            LPCValue v = RestoreMapping(ref content, ref offset);
                            arr.AsArray.Add(v);
                        }
                        break;
                    case '}':
                        {
                            offset++;
                            if (content [offset] == ')')
                            {
                                offset++;
                                return arr;
                            }
                        }
                        break;
                    case '-':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        {
                            LPCValue v = RestoreNumber(ref content, ref offset);
                            arr.AsArray.Add(v);
                        }
                        break;
                    case '@':
                        {
                            // alias
                            offset++;
                            LPCValue v = RestoreAlias(ref content, ref offset);
                            arr.AsArray.Add(v);
                        }
                        break;
                    default:
                        offset++;
                        break;
                }
            }

            return arr;
        }

        private static void SkipSpaceChar(ref string content, ref int offset)
        {
            int len = content.Length;
            while (offset < len)
            {
                char c = content [offset];
                if (c == ' ')
                {
                    offset++;
                    continue;
                }
                break;
            }
        }
        private static bool CheckKeyCompleted(ref string content, ref int offset)
        {
            if (content [offset] == ':')
            {
                offset++;
                return true;
            }
            return false;
        }

        private static bool CheckValueCompleted(ref string content, ref int offset)
        {
            switch (content [offset])
            {
                case ',':
                    offset++;
                    return true;
                case ']':
                case '}':
                    return true;
            }

            return false;
        }

        private static LPCValue RestoreMapping(ref string content, ref int offset)
        {
            int len = content.Length;
            LPCValue m = LPCValue.CreateMapping();
            LPCValue map_key = LPCValue.Create();

            while (offset < len)
            {
                switch (content [offset])
                {
                    case '@':
                        {
                            offset++;
                            LPCValue v = RestoreAlias(ref content, ref offset);
                            SkipSpaceChar(ref content, ref offset);
                            if (CheckKeyCompleted(ref content, ref offset))
                            {
                                map_key = v;
                            } else if (CheckValueCompleted(ref content, ref offset))
                            {
                                if (map_key.IsInt)
                                    m.AsMapping.Add(map_key.AsInt, v);
                                else
                            if (map_key.IsString)
                                    m.AsMapping.Add(map_key.AsString, v);
                                else
                                {
                                    throw new Exception("Bad mapping key(int && string only)");
                                }
                            }
                        }
                        break;
                    case '"':
                        {
                            offset++;
                            LPCValue v = RestoreString(ref content, ref offset);
                            SkipSpaceChar(ref content, ref offset);
                            if (CheckKeyCompleted(ref content, ref offset))
                            {
                                map_key = v;
                            } else if (CheckValueCompleted(ref content, ref offset))
                            {
                                if (map_key.IsInt)
                                    m.AsMapping.Add(map_key.AsInt, v);
                                else
                            if (map_key.IsString)
                                    m.AsMapping.Add(map_key.AsString, v);
                                else
                                {
                                    throw new Exception("Bad mapping key(int && string only)");
                                }
                            }
                        }
                        break;
                    case ':':
                        {
                            offset++;
                            LPCValue v = RestoreBuffer(ref content, ref offset);
                            SkipSpaceChar(ref content, ref offset);
                            if (CheckKeyCompleted(ref content, ref offset))
                            {
                                map_key = v;
                            } else if (CheckValueCompleted(ref content, ref offset))
                            {
                                if (map_key.IsInt)
                                    m.AsMapping.Add(map_key.AsInt, v);
                                else
                            if (map_key.IsString)
                                    m.AsMapping.Add(map_key.AsString, v);
                                else
                                {
                                    throw new Exception("Bad mapping key(int && string only)");
                                }
                            }
                        }
                        break;
                    case '-':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        {
                            LPCValue v = RestoreNumber(ref content, ref offset);
                            SkipSpaceChar(ref content, ref offset);
                            if (CheckKeyCompleted(ref content, ref offset))
                            {
                                map_key = v;
                            } else if (CheckValueCompleted(ref content, ref offset))
                            {
                                if (map_key.IsInt)
                                    m.AsMapping.Add(map_key.AsInt, v);
                                else
                            if (map_key.IsString)
                                    m.AsMapping.Add(map_key.AsString, v);
                                else
                                {
                                    throw new Exception("Bad mapping key(int && string only)");
                                }
                            }
                        }
                        break;
                    case '(':
                        offset++;
                        if (content [offset] == '{')
                        {
                            LPCValue v = RestoreArray(ref content, ref offset);
                            SkipSpaceChar(ref content, ref offset);
                            if (CheckKeyCompleted(ref content, ref offset))
                            {
                                map_key = v;
                            } else if (CheckValueCompleted(ref content, ref offset))
                            {
                                if (map_key.IsInt)
                                    m.AsMapping.Add(map_key.AsInt, v);
                                else
                                if (map_key.IsString)
                                    m.AsMapping.Add(map_key.AsString, v);
                                else
                                {
                                    throw new Exception("Bad mapping key(int && string only)");
                                }
                            }
                        } else if (content [offset] == '[')
                        {
                            offset++;
                            LPCValue v = RestoreMapping(ref content, ref offset);
                            SkipSpaceChar(ref content, ref offset);
                            if (CheckKeyCompleted(ref content, ref offset))
                            {
                                map_key = v;
                            } else if (CheckValueCompleted(ref content, ref offset))
                            {
                                if (map_key.IsInt)
                                    m.AsMapping.Add(map_key.AsInt, v);
                                else
                                if (map_key.IsString)
                                    m.AsMapping.Add(map_key.AsString, v);
                                else
                                {
                                    throw new Exception("Bad mapping key(int && string only)");
                                }
                            }
                        }
                        break;
                    case ']':
                        {
                            offset++;
                            if (content [offset] == ')')
                            {
                                offset++;
                                return m;
                            }
                        }
                        break;
                    default:
                        offset++;
                        break;
                }
            }

            return m;
        }
    }
}
