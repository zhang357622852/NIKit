/// <summary>
/// Download.cs
/// Created by zhaozy 2017-03-20
/// 有断点续传功能的下载
/// </summary>

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using LitJson;

public class Download
{
    #region 变量

    HttpWebRequest webRequest;
    HttpWebResponse webResponse;
    DateTime start = DateTime.Now;
    DateTime lastDownloadTime;
    int lastDownloadBytes;
    FileInfo localFileInfo;
    FileStream localFileStream;
    Stream httpStream;
    byte[] httpBuffer;

    #endregion

    #region 属性接口

    /// <summary>
    /// 下载资源地址
    /// </summary>
    /// <value>The URL.</value>
    public string url { get; private set; }

    /// <summary>
    /// 等待超时时间
    /// </summary>
    public int timeOutTime { get; private set; }

    /// <summary>
    /// 本地文件
    /// </summary>
    public string localFile { get; private set; }

    /// <summary>
    /// 已经下载大小
    /// </summary>
    public int downloadedBytes { get; private set; }

    /// <summary>
    /// 之前已经下载的大小
    /// </summary>
    public int oldLoadedBytes { get; private set; }

    /// <summary>
    /// 需要下载大小
    /// </summary>
    public int needDownloadBytes { get; private set; }

    /// <summary>
    /// 该文件下载速度
    /// </summary>
    public int downloadSpeed { get; private set; }

    /// <summary>
    /// 是否已经下载超时
    /// </summary>
    public bool isTimeOut { get { return (DateTime.Now - start).TotalSeconds > timeOutTime; } }

    /// <summary>
    /// Gets a value indicating whether this <see cref="Download"/> is get response.
    /// </summary>
    /// <value><c>true</c> if is get response; otherwise, <c>false</c>.</value>
    public bool isGetResponse { get; private set; }

    /// <summary>
    /// 是否已经下载完成
    /// </summary>
    public bool isDownloaded { get; private set; }

    /// <summary>
    /// 错误标识
    /// </summary>
    public int error { get; private set; }

    #endregion

    #region 私有接口

    /// <summary>
    /// 等待获取Response结果回调
    /// </summary>
    /// <param name="result">Result.</param>
    private void WaitGetResponse(IAsyncResult result)
    {
        try
        {
            webResponse = webRequest.EndGetResponse(result) as HttpWebResponse;
            if (webResponse != null && webResponse.ContentLength >= 0)
                needDownloadBytes = (int)webResponse.ContentLength;
        }
        catch (WebException e)
        {
            if (e.Status == WebExceptionStatus.Success)
                webResponse = e.Response as HttpWebResponse;
            else if (e.Message.IndexOf("(416) Requested Range Not Satisfiable") < 0)
                error = -1;

            // 打印详细信息
            NIDebug.LogException(e);
        }

        // 标识已经Get过Response
        isGetResponse = true;
    }

    /// <summary>
    /// 等待下载
    /// </summary>
    /// <param name="result">Result.</param>
    private void WaitDownload(IAsyncResult result)
    {
        // 获取httpStream长度，汇总到downloadedBytes
        int readCount = httpStream.EndRead(result);
        downloadedBytes += readCount;
        start = DateTime.Now;

        try
        {
            // 写入本地文件中
            localFileStream.Write(httpBuffer, 0, readCount);
            localFileStream.Flush();

            // 如果文件已经下载完则标识文件已经下载完成
            // 否则继续下载
            if (downloadedBytes < needDownloadBytes)
                httpStream.BeginRead(httpBuffer, 0, httpBuffer.Length, new AsyncCallback(WaitDownload), null);
            else
                isDownloaded = true;

            double ts = (DateTime.Now - lastDownloadTime).TotalSeconds;
            if (ts >= 1f)
            {
                downloadSpeed = (int)((downloadedBytes - lastDownloadBytes) / (1024.0f * ts));
                lastDownloadBytes = downloadedBytes;
                lastDownloadTime = DateTime.Now;
            }
        }
        catch (Exception e)
        {
            // 磁盘空间已满, 如果是自盘空间满了error为-2
            if (e.Message.IndexOf("Disk full") != -1)
                error = -2;
            else
                error = -1;

            // 打印详细的异常信息
            NIDebug.LogException(e);
        }
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 构造函数
    /// localFile指向本地文件（该文件是下载目录的文件和目标目录不一致，下载目录在下载文件处理完成后需要删除）
    /// </summary>
    /// <param name="_url">URL.</param>
    /// <param name="_timeOutTime">Time out time.</param>
    /// <param name="_localFile">Local file.</param>
    public Download(string _url, int _timeOutTime, string _localFile)
    {
        url = _url;
        webRequest = HttpWebRequest.Create(url) as HttpWebRequest;
        timeOutTime = _timeOutTime;
        localFile = _localFile;
    }

    /// <summary>
    /// 开始获取Response
    /// </summary>
    public void StartGetResponse()
    {
        // 记录开始时间
        start = DateTime.Now;

        try
        {
            // 首先确保文件路径存在
            Directory.CreateDirectory(Path.GetDirectoryName(localFile));
            localFileInfo = new FileInfo(localFile);

            // 如果本地已经存在该文件，则继续上一次的下载进度(否则文件从0开始下载)
            // 设置读取范围
            if (localFileInfo.Exists)
            {
                oldLoadedBytes = (int)localFileInfo.Length;
                webRequest.AddRange(oldLoadedBytes);
            }

            // 异步获取WebResponse
            webRequest.BeginGetResponse(new AsyncCallback(WaitGetResponse), this);
        }
        catch (Exception e)
        {
            // 磁盘空间已满, 如果是自盘空间满了error为-2
            if (e.Message.IndexOf("Disk full") != -1)
                error = -2;
            else
                error = -1;

            // 打印详细的异常信息
            NIDebug.LogException(e);
        }
    }

    /// <summary>
    /// 开始下载
    /// </summary>
    public void StartDownload()
    {
        start = lastDownloadTime = DateTime.Now;
        if (needDownloadBytes > 0)
        {
            try
            {
                if (localFileInfo.Exists)
                    localFileStream = new FileStream(localFile, FileMode.Append, FileAccess.Write);
                else
                    localFileStream = new FileStream(localFile, FileMode.Create, FileAccess.Write);
                localFileInfo = null;

                httpBuffer = new byte[1024];
                httpStream = webResponse.GetResponseStream();
                lastDownloadBytes = downloadedBytes;
                httpStream.BeginRead(httpBuffer, 0, httpBuffer.Length, new AsyncCallback(WaitDownload), null);
            }
            catch (Exception e)
            {
                // 磁盘空间已满, 如果是自盘空间满了error为-2
                if (e.Message.IndexOf("Disk full") != -1)
                    error = -2;
                else
                    error = -1;

                // 打印详细的异常信息
                NIDebug.LogException(e);
            }
        }
        else
        {
            isDownloaded = true;
        }
    }

    /// <summary>
    /// 清除
    /// </summary>
    public void Clear()
    {
            httpBuffer = null;

        try
        {
            if (webRequest != null)
                webRequest.Abort();
            webRequest = null;
        }
        catch
        {
        }

        try
        {
            if (httpStream != null)
                httpStream.Close();
            httpStream = null;
        }
        catch
        {
        }

        try
        {
            if (webResponse != null)
                webResponse.Close();
            webResponse = null;
        }
        catch
        {
        }

        try
        {
            if (localFileStream != null)
                localFileStream.Close();
            localFileStream = null;
        }
        catch
        {
            // 已经是关闭download了，异常不在处理
            // 这个地方主要是防止localFileStream关闭时异常导致游戏宕机
        }
    }

    /// <summary>
    /// 获取总的的下载大小(包含断线重连之后的大小)
    /// </summary>
    /// <returns>The totle size.</returns>
    public int GetTotleDownloadSize()
    {
        return downloadedBytes + oldLoadedBytes;
    }

    #endregion
}
