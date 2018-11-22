/// <summary>
/// Unzip.cs
/// Created by zhaozy 2017-03-20
/// 文件解压缩类
/// </summary>

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;

public class Unzip
{
    #region 变量

    ZipInputStream zipInStream;
    string zipFile;
    string localPath;
    List<string> fileList;

    #endregion

    #region 属性接口

    /// <summary>
    /// 已经解压缩字节数
    /// </summary>
    public int UnzipBytes { get; private set; }

    /// <summary>
    /// 错误标识
    /// </summary>
    /// 0  : 解压缩成功
    /// -1 : 压缩文件载入失败
    /// -2 : 内存分配失败
    /// -3 : 文件写入失败
    /// -4 : 其他异常信息
    public int Error { get; private set; }

    /// <summary>
    /// 是否已经解压缩结束
    /// </summary>
    public bool IsUnziped { get; private set; }

    #endregion

    #region 私有接口

    /// <summary>
    /// 载入zip file
    /// </summary>
    /// <returns>The file.</returns>
    private IEnumerator LoadFile()
    {
        // WWW方式载入文件
        WWW www = new WWW(zipFile);
        yield return www;

        // 等待资源加载完成
        while (!www.isDone)
            yield return null;

        // 文件载入失败, 获取文件不存在
        if (!string.IsNullOrEmpty(www.error) ||
            www.bytes.Length <= 0)
        {
            // 压缩文件载入失败
            Error = -1;

            // 标识解压缩结束
            IsUnziped = true;
            yield break;
        }

        try
        {
            // ZipInputStream
            zipInStream = new ZipInputStream(new MemoryStream(www.bytes));

            // 释放www
            www.Dispose();

            // 回收一下GC
            GC.Collect();

            // 开启解压缩线程
            Thread decompressThread = new Thread(new ThreadStart(DecompressThread));
            decompressThread.Start();
        }
        catch(OutOfMemoryException)
        {
            // 分配内存失败
            Error = -2;

            // 标识解压缩结束
            IsUnziped = true;
        }
        catch (Exception)
        {
            // 其他异常信息
            Error = -4;

            // 标识解压缩结束
            IsUnziped = true;
        }
    }

    /// <summary>
    /// 解压缩补丁包线程
    /// </summary>
    private void DecompressThread()
    {
        // 解压Zip包
        ZipEntry entry = null;
        while (null != (entry = zipInStream.GetNextEntry()))
        {
            // 获取文件失败
            if (string.IsNullOrEmpty(entry.Name))
                continue;

            // 获取文件路径
            string fileName = Path.GetFileName(entry.Name);
            string filePath = Path.Combine(localPath, Path.GetFileName(fileName));

            // 写入文件
            try
            {
                // 创建文件目录
                if (entry.IsDirectory)
                {
                    Directory.CreateDirectory(filePath);
                    continue;
                }

                // 判断是否有指定解压缩文件列表, 如果不在列表中能解压缩
                if (fileList.Count > 0 && fileList.IndexOf(fileName) == -1)
                    continue;

                // 创建文件流
                FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

                // 每次读取字节数
                byte[] bytes = new byte[2048];
                while (true)
                {
                    // 尝试解压缩
                    int count = zipInStream.Read(bytes, 0, bytes.Length);

                    // 记录解压缩大小字节数
                    UnzipBytes += count;

                    if (count > 0)
                    {
                        // 将数据写入fileStream
                        fileStream.Write(bytes, 0, count);
                        fileStream.Flush();
                    }
                    else
                    {
                        // 文件读取结束
                        break;
                    }
                }

                // 写入文件流，关闭IO
                fileStream.Close();

                // fileStream
                fileStream = null;
            }
            catch(OutOfMemoryException)
            {
                // 分配内存失败
                Error = -2;

                // 标识解压缩结束
                IsUnziped = true;

                return;
            }
            catch (IOException)
            {
                // 文件操作失败, 磁盘空间已满
                Error = -3;

                // 标识解压缩结束
                IsUnziped = true;

                return;
            }
            catch (Exception)
            {
                // 其他异常信息
                Error = -4;

                // 标识解压缩结束
                IsUnziped = true;

                return;
            }

            // 回收一下GC
            GC.Collect();
        }

        // 标识解压缩结束
        IsUnziped = true;
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// Initializes a new instance of the <see cref="Unzip"/> class.
    /// </summary>
    /// <param name="_zipPath">Zip path.</param>
    /// <param name="_localFile">Local file.</param>
    /// <param name="_fileList">File list.</param>
    public Unzip(string _zipFile, string _localPath, List<string> _fileList)
    {
        zipFile = _zipFile;
        localPath = _localPath;
        fileList = _fileList;
    }

    /// <summary>
    /// 开始解压缩文件
    /// </summary>
    public void Start()
    {
        // 在协程中载入文件
        Coroutine.DispatchService(LoadFile());
    }

    /// <summary>
    /// Clear this instance.
    /// </summary>
    public void Clear()
    {
        try
        {
            // 释放资源
            if (zipInStream != null)
                zipInStream.Close();

            zipInStream = null;
        }
        catch
        {
        }
    }

    #endregion
}