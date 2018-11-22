/// <summary>
/// VersionMgr.cs
/// Create by zhaozy 2017/03/04
/// 版本控制管理模块
/// </summary>

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;
using LPC;

public class VersionMgr
{
    // 版本树文件名
    public static string VERSION_NAME = "version_tree";

    // 资源映射关系
    public static string RESOURCE_DICT = "resource_dict";

    // 资源版本名称
    public static string RES_VERSION_NAME = "res_version";

    // 版本文件是否载入成功
    private static bool isVersionFileLoadSuccess = false;

    // 版本信息
    private static CsvFile mVersionCsv = new CsvFile(VERSION_NAME);

    // 新的版本信息
    private static CsvFile mNewVersionCsv = new CsvFile("new_version");

    // 资源映射关系
    private static CsvFile mResourceDictCsv = new CsvFile(RESOURCE_DICT);

    // 更新资源的列表
    private static Dictionary<string, List<string>> mUpdateResDict = new Dictionary<string, List<string>>();

    #region 内部接口

    #region 属性

    /// <summary>
    /// 需要更新列表
    /// </summary>
    public static  Dictionary<string, List<string>> UpdateResDict
    {
        get
        {
            return mUpdateResDict;
        }
    }

    /// <summary>
    /// 需要更新文件总字节数
    /// </summary>
    public static int DownloadSize { get; set; }

    /// <summary>
    /// 需要原始文件大小
    /// </summary>
    public static int UnzipSize { get; set; }

    /// <summary>
    /// 是否需要更新客户端
    /// </summary>
    public static bool IsNeedUpdateRes { get; set; }

    #endregion

    /// <summary>
    /// 载入地资源映射目录
    /// </summary>
    private static IEnumerator LoadResourceDictFile()
    {
        // 本地版本文件不存在
        string filePath = string.Format("{0}/{1}.bytes", ConfigMgr.ASSETBUNDLES_PATH, RESOURCE_DICT);
        if (!File.Exists(filePath))
        {
            NIDebug.Log("找不到本地资源映射目录。");
            yield break;
        }

        // 载入本地版本文件
        try
        {
            // 读取文件
            byte[] verData = File.ReadAllBytes(filePath);

            // 反序列化
            MemoryStream csvStr = new MemoryStream(verData, 0, verData.Length, true, true);
            mResourceDictCsv = CsvFileMgr.Deserialize(csvStr);
            csvStr.Close();
            verData = null;
        }
        catch (Exception e)
        {
            NIDebug.LogException(e);
        }
    }

    /// <summary>
    /// 下载远程版本文件
    /// </summary>
    private static IEnumerator UpdateNewVersionFile(int urlIndex)
    {
        // 获取配置的ab下载地址
        IList abUrls = (IList)ConfigMgr.Get<JsonData>("ab_urls", null);

        if (abUrls == null || abUrls.Count == 0)
        {
            NIDebug.Log("获取assetbundle资源路径失败");
            isVersionFileLoadSuccess = true;
            yield break;
        }

        // 尝试载入服务器上的版本文件
        string versionUrl = string.Format("{0}/{1}.bytes", abUrls[urlIndex], VERSION_NAME);
        WWW www = new WWW (versionUrl);
        yield return www;

        // 下载文件失败，切换其他地址下载
        if (! string.IsNullOrEmpty (www.error))
        {
            // 切换ab更新地址
            Coroutine.DispatchService(UpdateNewVersionFile(urlIndex >= abUrls.Count - 1 ? 0 : ++urlIndex));

            NIDebug.Log("{0}超时或错误", versionUrl);
            yield break;
        }

        // 反序列化
        MemoryStream csvStr = new MemoryStream(www.bytes, 0, www.bytes.Length, true, true);
        mNewVersionCsv = CsvFileMgr.Deserialize(csvStr);
        csvStr.Close();
        isVersionFileLoadSuccess = true;

        // 释放www
        www.Dispose();

        NIDebug.Log("下载版本文件成功");

        yield break;
    }

    /// <summary>
    /// Checks the version.
    /// </summary>
    /// <returns><c>true</c>, if version was checked, <c>false</c> otherwise.</returns>
    private static bool CheckResourceVersion(bool checkResVersion = false)
    {
        // 没有新版本文件
        if(mNewVersionCsv == null || mNewVersionCsv.rows == null)
            return false;

        // 如果本地版本文件为空
        if(mVersionCsv == null || mVersionCsv.rows == null)
            return true;

        // 不需要检查ResVersion
        // 只有在客户端启动尝试解压缩的时候才需要
        if (! checkResVersion)
            return true;

        // 读取版本文件资源版本号
        CsvRow newData = mNewVersionCsv.FindByKey(RES_VERSION_NAME);
        if(newData == null)
            return false;

        // 获取本地资源版本信息
        CsvRow resData = mVersionCsv.FindByKey(RES_VERSION_NAME);
        if(resData == null)
            return true;

        // 对比版本
        return (string.Compare(newData.Query<string>("md5"), resData.Query<string>("md5")) > 0);
    }

    /// <summary>
    /// Compares the version file.
    /// </summary>
    /// <param name="isCheckPatch">If set to <c>true</c> 是否需要检查patch包 </param>
    /// 如果isCheckPatch表示需要检查patch，只有需要更新资源全部在patch包中时才下载patch包否则直接单个资源下载
    private static void CompareVersionFile(bool isCheckPatch = true, bool checkResVersion = false, string[] patchRes = null)
    {
        // 清除原始数据
        UpdateResDict.Clear();
        DownloadSize = 0;
        UnzipSize = 0;

        // 比较两个版本树的资源版本编号
        // 如果版本一致不需要处理
        if (! CheckResourceVersion(checkResVersion))
            return;

        // 汇总整包资源详情
        Dictionary<string, int> patchResCount = new Dictionary<string, int>();
        Dictionary<string, List<string>> updateResMap = new Dictionary<string, List<string>>();

        // 遍历装备数据
        foreach (CsvRow data in mNewVersionCsv.rows)
        {
            // 如果没有patch信息，则表示不是资源
            // 资源分包信息也在该配置表中
            string patch = data.Query<string>("patch");
            if (patch.Equals("0") || string.IsNullOrEmpty(patch))
                continue;

            // 汇总patch包包含资源数量
            if (patchResCount.ContainsKey(patch))
                patchResCount[patch] = patchResCount[patch] + 1;
            else
                patchResCount[patch] = 1;

            // 获取资源名
            string ABName = data.Query<string>("bundle");

            // 获取就版本信息
            CsvRow row = mVersionCsv.FindByKey(ABName);
            string md5 = data.Query<string>("md5");

            // 包含路径且md5编号一致，必须保证AB文件也存在（以防止AB数据被玩家误删）
            if (row != null &&
                row.Query<string> ("md5").Equals(md5) &&
                File.Exists(Path.Combine (ConfigMgr.ASSETBUNDLES_PATH, ABName)))
                continue;

            // 检查随包资源
            if(!isCheckPatch && patchRes != null)
            {
                CsvRow patchRow = mNewVersionCsv.FindByKey(patch);

                string patchFile = string.Format("{0}_{1}.zip", patch,  patchRow.Query<string>("md5"));

                // patch没有包含在随包资源中
                if(patchRes != null && Array.IndexOf(patchRes, patchFile) == -1)
                    continue;
            }

            // 汇总需要更新资源信息
            if (updateResMap.ContainsKey(patch))
                updateResMap[patch].Add(ABName);
            else
                updateResMap[patch] = new List<string>() { ABName };

            // 累计原始大小
            UnzipSize += data.Query<int>("unzip_size");
        }

        string fileName;
        CsvRow csvRowData;

        // 确定更新方式
        foreach(string patch in updateResMap.Keys)
        {
            // 不需要检查patch包
            if (!isCheckPatch)
            {
                // 获取配置信息
                csvRowData = mNewVersionCsv.FindByKey(patch);
                fileName = string.Format("{0}_{1}.zip", patch, csvRowData.Query<string>("md5"));

                UpdateResDict[fileName] = updateResMap[patch];

                // 累计压缩大小
                DownloadSize += csvRowData.Query<int>("zip_size");
                continue;
            }

            // 判断是否需要更新该完整包
            if (updateResMap[patch].Count == patchResCount[patch])
            {
                // 获取配置信息
                csvRowData = mNewVersionCsv.FindByKey(patch);
                fileName = string.Format("{0}_{1}.zip", patch, csvRowData.Query<string>("md5"));
                UpdateResDict[fileName] = updateResMap[patch];

                // 累计压缩大小
                DownloadSize += csvRowData.Query<int>("zip_size");
                continue;
            }

            // 否则需要单个文件更新
            foreach (string bundle in updateResMap[patch])
            {
                // 获取配置信息
                csvRowData = mNewVersionCsv.FindByKey(bundle);
                fileName = string.Format("{0}_{1}.zip", bundle, csvRowData.Query<string>("md5"));
                UpdateResDict[fileName] = new List<string>() { bundle };

                // 累计压缩大小
                DownloadSize += csvRowData.Query<int>("zip_size");
            }
        }
    }

    /// <summary>
    /// 初始化版本树
    /// </summary>
    private static void InitVersionCsv()
    {
        // mVersionCsv已经初始化过了, 则不处理
        // 否则按照mNewVersionCsv初始化数据
        if (!string.IsNullOrEmpty(mVersionCsv.primaryKey) ||
            mVersionCsv.rows != null)
            return;

        // 按照mNewVersionCsv初始化数据
        // 主key
        mVersionCsv.primaryKey = mNewVersionCsv.primaryKey;

        // 列名字对应的索引
        foreach (KeyValuePair<string, int> mks in mNewVersionCsv.columns)
            mVersionCsv.columns.Add(mks.Key, mks.Value);
    }

    /// <summary>
    /// 打开登陆场景之前
    /// </summary>
    private static void OnLoginSceneBefore(object para, object[] param)
    {
        // 打开闪屏界面
        //WindowMgr.OpenWnd("WhiteFlash");
    }

    /// <summary>
    /// 打开登陆场景之后
    /// </summary>
    private static void OnLoginSceneAfter(object para, object[] param)
    {
    }

    #endregion

    #region 接口申明

    /// <summary>
    /// 对比随包资源版本
    /// </summary>
    public static IEnumerator ComparePackageVersion(string[] patchRes)
    {
        // 载入随包资源版本
        yield return Coroutine.DispatchService(LoadStreamingVersionFile());

        // 比对两个版本文件确定是否需要更新
        CompareVersionFile(false, true, patchRes);

        // 初始化版本数据
        InitVersionCsv();

        yield break;
    }

    /// <summary>
    /// 对比在线资源版本
    /// </summary>
    public static IEnumerator CompareOnlineVersion()
    {
        // 更新在线版本文件
        yield return Coroutine.DispatchService(UpdateNewVersionFile(0));
        while(! isVersionFileLoadSuccess)
            yield return null;

        // 比对两个版本文件确定是否需要更新
        CompareVersionFile();

        // 初始化版本数据
        InitVersionCsv();

        yield break;
    }

    /// <summary>
    /// 载入随包资源版本
    /// </summary>
    /// <returns>The streaming version file.</returns>
    public static IEnumerator LoadStreamingVersionFile()
    {
        // 重新new一个数据
        mNewVersionCsv = new CsvFile("new_version");

        // 载入文件
        WWW www = new WWW(ConfigMgr.GetStreamingPathWWW(string.Format("{0}.bytes", VERSION_NAME)));
        yield return www;

        // 等待资源加载完成
        while (!www.isDone)
            yield return null;

        // 文件载入失败, 获取文件不存在
        if (!string.IsNullOrEmpty(www.error) || www.bytes.Length <= 0)
            yield break;

        // 反序列化
        MemoryStream csvStr = new MemoryStream(www.bytes, 0, www.bytes.Length, true, true);
        mNewVersionCsv = CsvFileMgr.Deserialize(csvStr);
        csvStr.Close();

        // 释放www
        www.Dispose();
    }

    /// <summary>
    /// 载入本地版本文件
    /// </summary>
    public static IEnumerator LoadVersionFile()
    {
        // 本地版本文件不存在
        string filePath = string.Format("{0}/{1}.bytes", ConfigMgr.ASSETBUNDLES_PATH, VERSION_NAME);
        if (! File.Exists(filePath))
        {
            NIDebug.Log("找不到本地版本文件");
            yield break;
        }

        // 载入文件
        WWW www = new WWW(ConfigMgr.GetLocalRootPathWWW(string.Format("{0}/{1}.bytes", ConfigMgr.ASSETBUNDLES_NAME, VERSION_NAME)));
        yield return www;

        // 等待资源加载完成
        while (!www.isDone)
            yield return null;

        // 文件载入失败, 获取文件不存在
        if (! string.IsNullOrEmpty(www.error) || www.bytes.Length <= 0)
            yield break;

        // 反序列化
        MemoryStream csvStr = new MemoryStream(www.bytes, 0, www.bytes.Length, true, true);
        mVersionCsv = CsvFileMgr.Deserialize(csvStr);
        csvStr.Close();

        // 释放www
        www.Dispose();
    }

    /// <summary>
    /// 获取资源所在AssetBundle的名字
    /// </summary>
    public static string GetAssetBundleName(string path)
    {
        // 查找版本树，确认该资源是否是版本树上的资源
        CsvRow data = mResourceDictCsv.FindByKey(path);

        // 该资源是内部资源
        if (data == null)
            return string.Empty;

        // 返回该资源所属assetbundle
        return data.Query<string>("bundle");
    }

    /// <summary>
    /// 同步资源结束(释放随包资源和同步在线资源)
    /// </summary>
    public static IEnumerator DoSycnResEnd(bool loadResDict = true)
    {
        // 写入版本文件
        SyncVersion(RES_VERSION_NAME);

        // 清除和更新相关数据
        mNewVersionCsv = new CsvFile("new_version");
        mUpdateResDict.Clear();
        mUpdateResDict = new Dictionary<string, List<string>>();
        DownloadSize = 0;
        UnzipSize = 0;

        // 不需要载入ResourceDict
        if (!loadResDict)
            yield break;

        // 载入本地资源映射表
        yield return Coroutine.DispatchService(LoadResourceDictFile());
    }

    /// <summary>
    /// 更新本地版本信息
    /// </summary>
    /// <param name="ab">Ab.</param>
    public static void SyncVersion(List<string> abs)
    {
        // 遍历需要更新的ab信息
        foreach (string ab in abs)
        {
            // 新版树中没有该版本信息
            CsvRow data = mNewVersionCsv.FindByKey(ab);
            if (data == null)
                return;

            // 修正本地版本信息
            CsvRow oldData = mVersionCsv.FindByKey(ab);

            // 如果是新增资源
            if (oldData == null)
            {
                // 构建一列数据
                CsvRow row = new CsvRow(mNewVersionCsv);
                row.Add("bundle", data.Query<LPCValue>("bundle"));
                row.Add("md5", data.Query<LPCValue>("md5"));
                row.Add("patch", data.Query<LPCValue>("patch"));
                row.Add("unzip_size", data.Query<LPCValue>("unzip_size"));
                row.Add("zip_size", data.Query<LPCValue>("zip_size"));

                // 添加新数据
                mVersionCsv.AddNewRow(row);
            }
            else
            {
                oldData.Add("md5", data.Query<LPCValue>("md5"));
                oldData.Add("patch", data.Query<LPCValue>("patch"));
                oldData.Add("unzip_size", data.Query<LPCValue>("unzip_size"));
                oldData.Add("zip_size", data.Query<LPCValue>("zip_size"));
            }
        }

        // 序列化
        MemoryStream ms = new MemoryStream();
        CsvFileMgr.Serialize(ms, mVersionCsv);

        // 版本文件存放路径
        string versionFilePath = string.Format ("{0}/{1}.bytes", ConfigMgr.ASSETBUNDLES_PATH, VERSION_NAME);
        FileStream fs = new FileStream(versionFilePath, FileMode.Create, FileAccess.Write);
        fs.Write(ms.GetBuffer(), 0, (int)ms.Length);
        fs.Close();
    }

    /// <summary>
    /// 更新本地版本信息
    /// </summary>
    /// <param name="ab">Ab.</param>
    public static void SyncVersion(string ab)
    {
        CsvRow data = mNewVersionCsv.FindByKey(ab);

        // 新版树中没有该版本信息
        if (data == null)
            return;

        // 修正本地版本信息
        CsvRow oldData = mVersionCsv.FindByKey(ab);

        // 如果是新增资源
        if (oldData == null)
        {
            // 构建一列数据
            CsvRow row = new CsvRow(mNewVersionCsv);
            row.Add("bundle", data.Query<LPCValue>("bundle"));
            row.Add("md5", data.Query<LPCValue>("md5"));
            row.Add("patch", data.Query<LPCValue>("patch"));
            row.Add("unzip_size", data.Query<LPCValue>("unzip_size"));
            row.Add("zip_size", data.Query<LPCValue>("zip_size"));

            // 添加新数据
            mVersionCsv.AddNewRow(row);
        }
        else
        {
            oldData.Add("md5", data.Query<LPCValue>("md5"));
            oldData.Add("patch", data.Query<LPCValue>("patch"));
            oldData.Add("unzip_size", data.Query<LPCValue>("unzip_size"));
            oldData.Add("zip_size", data.Query<LPCValue>("zip_size"));
        }

        // 序列化
        MemoryStream ms = new MemoryStream();
        CsvFileMgr.Serialize(ms, mVersionCsv);

        // 版本文件存放路径
        string versionFilePath = string.Format ("{0}/{1}.bytes", ConfigMgr.ASSETBUNDLES_PATH, VERSION_NAME);

        // 写入文件
        FileStream fs = new FileStream(versionFilePath, FileMode.Create, FileAccess.Write);
        fs.Write(ms.GetBuffer(), 0, (int)ms.Length);
        fs.Close();
    }

    #endregion
}
