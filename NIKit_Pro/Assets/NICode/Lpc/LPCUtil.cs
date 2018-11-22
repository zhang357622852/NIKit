/// <summary>
/// LPCUtil.cs
/// Copy from zhangyg 2014-10-22
/// LPC类型操作的工具
/// </summary>

using System;
using System.Diagnostics;
using LPC;

/// <summary>
/// LPC类型操作的工具
/// </summary>
public class LPCUtil
{
    public static string GetSpace(int n)
    {
        string str = "";
        for (int i = 0; i < n; i++)
            str += " ";
        return str;
    }

    /// <summary>
    /// 获取8位有符号值
    /// </summary>
    public static SByte _GET_8(byte[] buf, ref int offset)
    {
        SByte val = (SByte)buf [offset];
        offset = offset + 1;
        return val;
    }

    /// <summary>
    /// 获取16位有符号值
    /// </summary>
    public static Int16 _GET_16(byte[] buf, ref int offset)
    {
        Int16 val = BitConverter.ToInt16(buf, offset);
        val = System.Net.IPAddress.NetworkToHostOrder((short)val);
        offset = offset + 2;
        return val;
    }

    /// <summary>
    /// 获取32位有符号值
    /// </summary>
    public static Int32 _GET_32(byte[] buf, ref int offset)
    {
        Int32 val = BitConverter.ToInt32(buf, offset);
        val = System.Net.IPAddress.NetworkToHostOrder((int)val);
        offset = offset + 4;
        return val;
    }

    /// <summary>
    /// 获取8位无符号值
    /// </summary>
    public static Byte _GET_U8(byte[] buf, ref int offset)
    {
        Byte val = (Byte)buf [offset];
        offset = offset + 1;
        return val;
    }

    /// <summary>
    /// 获取16位无符号值
    /// </summary>
    public static UInt16 _GET_U16(byte[] buf, ref int offset)
    {
        UInt16 val = BitConverter.ToUInt16(buf, offset);
        val = (UInt16)System.Net.IPAddress.NetworkToHostOrder((short)val);
        offset = offset + 2;
        return val;
    }

    /// <summary>
    /// 获取32位无符号值
    /// </summary>
    public static UInt32 _GET_U32(byte[] buf, ref int offset)
    {
        UInt32 val = BitConverter.ToUInt32(buf, offset);
        val = (UInt32)System.Net.IPAddress.NetworkToHostOrder((int)val);
        offset = offset + 4;
        return val;
    }

    /// <summary>
    /// 获取字符串信息
    /// </summary>
    public static string _GET_STR(byte[] buf, ref int offset, int len)
    {
        string val = System.Text.Encoding.UTF8.GetString(buf, offset, len);
        offset = offset + len;
        return val;
    }

    /// <summary>
    /// pad 8位值
    /// </summary>
    public static int _PAD_8(int n, byte[] buf, int offset)
    {
        byte data = (byte)n;
        buf [offset] = data;
        return 1;
    }

    /// <summary>
    /// pad 16位值
    /// </summary>
    public static int _PAD_16(int n, byte[] buf, int offset)
    {
        short data = (short)n;
        data = System.Net.IPAddress.HostToNetworkOrder(data);
        byte[] dt = BitConverter.GetBytes(data);
        Debug.Assert(dt.Length == 2);
        System.Buffer.BlockCopy(dt, 0, buf, offset, 2);
        return 2;
    }

    /// <summary>
    /// pad 32位值
    /// </summary>
    public static int _PAD_32(int n, byte[] buf, int offset)
    {
        int data = System.Net.IPAddress.HostToNetworkOrder(n);
        byte[] dt = BitConverter.GetBytes(data);
        Debug.Assert(dt.Length == 4);
        System.Buffer.BlockCopy(dt, 0, buf, offset, 4);
        return 4;
    }

    /// <summary>
    /// pad string 8位值
    /// </summary>
    public static int _PAD_LEN_STRING(string s, byte[] buf, int offset)
    {
        byte[] strArr = System.Text.Encoding.UTF8.GetBytes(s);
        int len = strArr.Length > 255 ? 255 : strArr.Length;
        buf [offset] = (byte)len;
        System.Buffer.BlockCopy(strArr, 0, buf, offset + 1, len);
        return len + 1;
    }

    /// <summary>
    /// pad string 16位值
    /// </summary>
    public static int _PAD_LEN_STRING2(string s, byte[] buf, int offset)
    {
        byte[] strArr = System.Text.Encoding.UTF8.GetBytes(s);
        short len = (short)(strArr.Length > 65535 ? 65535 : strArr.Length);
        byte[] dt = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(len));
        Debug.Assert(dt.Length == 2);
        System.Buffer.BlockCopy(dt, 0, buf, offset, 2);
        System.Buffer.BlockCopy(strArr, 0, buf, offset + 2, len);
        return len + 2;
    }

    /// <summary>
    /// pad string 32位值
    /// </summary>
    public static int _PAD_LEN_STRING4(string s, byte[] buf, int offset)
    {
        byte[] strArr = System.Text.Encoding.UTF8.GetBytes(s);
        int len = strArr.Length;
        byte[] dt = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(len));
        Debug.Assert(dt.Length == 4);
        System.Buffer.BlockCopy(dt, 0, buf, offset, 4);
        System.Buffer.BlockCopy(strArr, 0, buf, offset + 4, len);
        return len + 4;
    }

    /// <summary>
    /// pad buffer值
    /// </summary>
    public static int _PAD_BUFFER(byte[] b, byte[] buf, int offset)
    {
        System.Buffer.BlockCopy(b, 0, buf, offset, b.Length);
        return b.Length;
    }

    // Get size of compact integer
    // -128-127     : 1
    // -32768-32767 : 2
    // ... 
    public static int GetSaveBinaryIntSize(int i)
    {
        int len = 0;

        if (i == 0)
        /* Zero size for 0 */
            return 0;

        len = 1;
        while (i < -128 || i > 127)
        {
            i /= 256;
            len++;
        }

        return len;
    }

    /// <summary>
    /// 两个mapping相加(不改变原来的值)
    /// </summary>
    public static LPCValue MappingAdd(LPCMapping m1, LPCMapping m2)
    {
        LPCValue v = LPCValue.CreateMapping();
        foreach (object k in m1.Keys)
        {
            if (k is string)
                v.AsMapping [k as string] = m1 [k as string];
            else
                v.AsMapping [(int)k] = m1 [(int)k];
        }
        foreach (object k in m2.Keys)
        {
            if (k is string)
                v.AsMapping [k as string] = m2 [k as string];
            else
                v.AsMapping [(int)k] = m2 [(int)k];
        }
        return v;
    }
}
