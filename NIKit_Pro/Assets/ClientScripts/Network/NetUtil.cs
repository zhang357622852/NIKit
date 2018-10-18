using System;
using System.Collections.Generic;

/// <summary>
/// 网络相关的工具 
/// </summary>
public class NetUtil
{
    /// <summary>
    /// 略过几个字节 
    /// </summary>
    /// <param name="srcBytes">原始字节串</param>
    /// <param name="skipCount">略过的字节数</param>
    /// <returns> 处理后的字节串 </returns>
    public static byte[] ByteArraySkip(byte[] srcBytes, int skipCount)
    {
        int size = srcBytes.Length - skipCount;
        if (size < 1)
        {
            throw new Exception(string.Format("Bad ByteArraySkip {0} {1}.", srcBytes.Length, skipCount));
        }
        
        byte[] dstBytes = new byte[size];
        try
        {
            Array.Copy(srcBytes, skipCount, dstBytes, 0, size);
        }
        catch (Exception e)
        {
            LogMgr.Exception(e);
        }
        return dstBytes;
    }
    
    /// <summary>
    /// 连接2个字节串
    /// </summary>
    public static byte[] ByteArrayConcat(byte[] bytesLeft, byte[] bytesRight)
    {
        List<byte> byteList = new List<byte>();
        try
        {
            byteList.AddRange(bytesLeft);
            byteList.AddRange(bytesRight);
        }
        catch (Exception e)
        {
            LogMgr.Exception(e);
        }

        return byteList.ToArray();
    }

    /// <summary>
    /// 抽取子字节串
    /// </summary>
    public static byte[] ByteArrayTake(byte[] srcBytes, int takeCount)
    {
        if ((takeCount < 0) || (srcBytes.Length < takeCount))
        {
            throw new Exception(string.Format("Bad ByteArrayTake {0} {1}.", srcBytes.Length, takeCount));
        }

        byte[] takeBytes = new byte[takeCount];
        try
        {
            Array.Copy(srcBytes, takeBytes, takeCount);
        }
        catch (Exception e)
        {
            LogMgr.Exception(e);
        }

        return takeBytes;
    }
}
