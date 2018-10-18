using System;
using System.IO;
using ComponentAce.Compression.Libs.zlib;

/// <summary>
/// 支持zlib压缩的工具类 
/// </summary>
public class Zlib
{
    #region 内部变量

    /// <summary>
    /// Copies the stream.
    /// </summary>
    /// <param name="input">Input.</param>
    /// <param name="output">Output.</param>
    private static void CopyStream(System.IO.Stream input, System.IO.Stream output, int buffer_size)
    {
        // 一次解压缩数据大小
        byte[] buffer = new byte[buffer_size];

        int len;
        while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, len);
        }

        output.Flush();
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 压缩函数 
    /// </summary>
    /// <param name="bytes">待压缩的数据</param>
    /// <returns>压缩完的数据</returns>
    public static byte[] Compress(byte[] bytes)
    {
        // 压缩
        MemoryStream outMemoryStream = new MemoryStream();
        ZOutputStream outZStream = new ZOutputStream(outMemoryStream, zlibConst.Z_BEST_SPEED);
        Stream inMemoryStream = new MemoryStream(bytes);
        CopyStream(inMemoryStream, outZStream, 2048);
        outZStream.finish();

        // 生成返回值
        byte[] outData = new byte[sizeof(int) + outMemoryStream.Length];

        // 在头部4个字节附带原长度信息
        int inSize = bytes.Length;
        int data = System.Net.IPAddress.HostToNetworkOrder(inSize);
        byte[] dt = BitConverter.GetBytes(data);
        System.Buffer.BlockCopy(dt, 0, outData, 0, sizeof(int));

        // 写入压缩后数据 
        outMemoryStream.Position = 0;
        outMemoryStream.Read(outData, sizeof(int), (int)outMemoryStream.Length);

        // 返回结果
        return outData;
    }

    /// <summary>
    /// 解压缩函数 
    /// </summary>
    /// <param name="bytes">待解压缩的数据</param>
    /// <returns>解压缩完的数据</returns>
    public static byte[] Decompress(byte[] bytes)
    {
        // 尝试解压缩做多尝试5次，每次失败后解压buff扩充pow(2, i)倍数
        // 为什么这个地方需要这么处理，主要是由于当前这个zlib解压算法会导致解压异常
        // 比如同样的数据2048的缓存空间解压缩失败，但是1024就有可能解压缩成功
        // 所以在这个地方处理一下如果异常了就多事两次，每次的缓存buffer大小都调整一下
        // 默认缓存
        for(int i = 0; i < 5; i++)
        {
            try
            {
                if (bytes.Length <= sizeof(int))
                {
                    throw new Exception(string.Format("Decompress error, bad input={0}.", 
                                                      bytes.Length));
                }

                // 读取原始数据长度
                int val = BitConverter.ToInt32(bytes, 0);
                int rawSize = (int)(UInt32)System.Net.IPAddress.NetworkToHostOrder(val);

                // 解压缩
                MemoryStream outMemoryStream = new MemoryStream();
                ZOutputStream outZStream = new ZOutputStream(outMemoryStream);
                Stream inMemoryStream = new MemoryStream(bytes);
                inMemoryStream.Position = sizeof(int);
                CopyStream(inMemoryStream, outZStream, 2048 * Game.Pow(2, i));
                outZStream.finish();
                if (outMemoryStream.Length != rawSize)
                {
                    throw new Exception(string.Format("Decompress error, bad raw size={0}.", rawSize));
                }

                // 生成返回值
                byte[] outData = new byte[outMemoryStream.Length];
                outMemoryStream.Position = 0;
                outMemoryStream.Read(outData, 0, (int)outMemoryStream.Length);

                // 返回数据
                return outData;
            }
            catch(Exception e)
            {
                // 抛出异常信息
                LogMgr.Exception(e);
            }
        }

        // 返回null
        return null;
    }

    #endregion
}
