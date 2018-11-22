using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using LPC;

/// <summary>
/// 1. 读取内置配置，作为全局配置
/// 2. 合并配置信息，保存
/// </summary>
public class ConfigMgr : Singleton<ConfigMgr>, IInit, IRelease
{
    #region 成员变量

    // 引用资源路径
    public static string ETC_PATH = "Assets/Etc";

    // 相对于本地和服务器路径的AssetBundle名字
    public static string ASSETBUNDLES_NAME = "AssetBundles";

    // 相对于本地下载路径的AssetBundle名字
    public static string DOWNLOAD_NAME = "DownLoad";

    // 内置和服务器配置名
    public static string CONFIG_PATH = "Assets/LocalSet/config.txt";

    /// <summary>
    /// 技能统计文件
    /// </summary>
    public static string SKILL_ACTION_DICT = "Etc/skill_action_dict.txt";

    // 初始化是否成功
    private static bool initSuccessed = false;

    // 全局配置
    static Config mConfig = new Config();
    public static Config config { get { return mConfig; } }

    public static bool InitSuccessed { get { return initSuccessed; } }

    /// <summary>
    /// 是否需要更新客户端
    /// </summary>
    public static bool IsNeedUpdateClient { get; set; }

    // 版本号
    public static string ClientVersion { get { return mConfig.Get<string>("client_version"); } }

    #endregion

    #region 属性

    /// <summary>
    /// 本地根目录
    /// </summary>
    /// <value>The LOCA l_ ROO t_ PAT.</value>
    public static string LOCAL_ROOT_PATH
    {
        get
        {
            if (Platform.IsEditor)
                return Application.persistentDataPath;
            // Windows平台的将StreamingAssets目录作为资源目录
            else if (Platform.IsStandaloneWin || Platform.IsStandaloneLinux)
                return Application.streamingAssetsPath;
            else if (Platform.IsAndroid)
                return Application.persistentDataPath;
            else if (Platform.IsIphone)
                return Application.temporaryCachePath;
            else
                return string.Empty;
         }
    }

    /// <summary>
    /// 本地AssetBundles位置
    /// </summary>
    /// <value>The ASSETBUNDLE s_ PAT.</value>
    public static string ASSETBUNDLES_PATH
    {
        get
        {
            return LOCAL_ROOT_PATH + "/" + ASSETBUNDLES_NAME;
        }
    }

    /// <summary>
    /// 本地AssetBundles下载的位置
    /// </summary>
    /// <value>The ASSETBUNDLE s_ PAT.</value>
    public static string DOWNLOAD_PATH
    {
       get
       {
            return LOCAL_ROOT_PATH + "/" + DOWNLOAD_NAME + "/";
       }
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 更新客户端
    /// </summary>
    /// <param name="url">URL.</param>
    private static void UpdateClient(string url)
    {
        // 标识客户端正在更新
        IsNeedUpdateClient = true;

        // 打开链接
        Application.OpenURL(url);

        // 1秒后关闭客户端
        System.Threading.Thread.Sleep(1000);

        // 关闭客户端
        Application.Quit();
    }

    #endregion

    #region 外部接口

    public void Init()
    {
    }

    public void Release()
    {
    }

    /// <summary>
    /// Gets the streaming path WW.
    /// </summary>
    /// <returns>The streaming path WW.</returns>
    /// <param name="file">File.</param>
    public static string GetStreamingPathWWW(string file)
    {
        if (Platform.IsAndroid && !Platform.IsEditor)
            return Application.streamingAssetsPath + "/" + file;
        else
            return "file://" + Application.streamingAssetsPath + "/" + file;
    }

    /// <summary>
    /// Gets the persistent path WW.
    /// </summary>
    /// <returns>The persistent path WW.</returns>
    /// <param name="file">File.</param>
    public static string GetLocalRootPathWWW(string file)
    {
        if (Platform.IsEditor || Platform.IsStandaloneWin)
            return "file:///" + Application.persistentDataPath + "/" + file;
        else if (Platform.IsAndroid)
            return "file://" + Application.persistentDataPath + "/" + file;
        else if (Platform.IsIphone)
            return "file://" + Application.temporaryCachePath + "/" + file;
        else
            return file;
    }

    // 创建配置
    public static void Create(string content)
    {
        mConfig = new Config(content);
    }

    // 销毁
    public static void Destroy()
    {
        mConfig = new Config();
    }

    // 添加一个配置信息
    public static void Add(string k, object v)
    {
        if (mConfig != null)
            mConfig.Add(k, v);
    }

    // 取得配置信息
    public static T Get<T>(string k)
    {
        return mConfig.Get<T>(k);
    }

    // 取得配置信息
    public static T Get<T>(string k, T def)
    {
        return mConfig.Get<T>(k, def);
    }

    // 从其他配置合并
    public static void Merge(string content)
    {
        mConfig.Merge(content);
    }

    // 从其他配置合并
    public static void Merge(Config cfg)
    {
        mConfig.Merge(cfg);
    }

    // 序列化
    public static void Serialize(Stream stream)
    {
        mConfig.Serialize(stream);
    }

    // 序列化
    public static bool Serialize()
    {
        try
        {
            using (FileStream fs = new FileStream(CONFIG_PATH, FileMode.Create, FileAccess.Write))
            {

                // 写入本地配置
                bool ret = mConfig.Serialize(fs);
                fs.Close();
                return ret;
            }
        }
        catch (Exception e)
        {
            NIDebug.LogException(e);
            return false;
        }
    }
    #endregion
}
