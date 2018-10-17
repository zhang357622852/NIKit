/// <summary>
/// ConfigMgr.cs
/// Created by wangxw 2014-10-22
/// 配置信息管理器
/// </summary>

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
public static class ConfigMgr
{
    #region 成员变量

    // 游戏运行模式
    // 本地服务器模式
    // 正式发布模式
    public const string MODE_LOCAL_HOST = "LOCAL_HOST";
    public const string MODE_PUBLISH = "PUBLISH";

    // 是否关闭内购系统
    public static bool IsClosePurchase = false;

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

    // 是否是中文版本（图标，版署）
    public static bool IsCN { get { return Get<bool>("is_cn", true); } }

    // 游戏运行方式
    // 1. LOCAL_HOST : 本地服务器模式
    // 2. PUBLISH : 正式发布模式
    public static string GameRunMode
    {
        get
        {
            return Get<string>("game_run_mode", ConfigMgr.MODE_LOCAL_HOST);
        }
    }

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
#if UNITY_EDITOR
            return Application.persistentDataPath;
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
            // Windows平台的将StreamingAssets目录作为资源目录
            return Application.streamingAssetsPath;
#elif UNITY_ANDROID
            return Application.persistentDataPath;
#elif UNITY_IPHONE
            return Application.temporaryCachePath;
#else
            return string.Empty;
#endif
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

    /// <summary>
    /// Gets the streaming path WW.
    /// </summary>
    /// <returns>The streaming path WW.</returns>
    /// <param name="file">File.</param>
    public static string GetStreamingPathWWW(string file)
    {
#if UNITY_ANDROID && ! UNITY_EDITOR
        return Application.streamingAssetsPath + "/" + file;
#else
        return "file://" + Application.streamingAssetsPath + "/" + file;
#endif
    }

    /// <summary>
    /// Gets the persistent path WW.
    /// </summary>
    /// <returns>The persistent path WW.</returns>
    /// <param name="file">File.</param>
    public static string GetLocalRootPathWWW(string file)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        return "file:///" + Application.persistentDataPath + "/" + file;
#elif UNITY_ANDROID
        return "file://" + Application.persistentDataPath + "/" + file;
#elif UNITY_IPHONE
        return "file://" + Application.temporaryCachePath + "/" + file;
#else
        return file;
#endif
    }

    /// <summary>
    /// Checks the client update.
    /// </summary>
    /// <returns>The client update.</returns>
    public static IEnumerator CheckClientUpdate()
    {
        // 标识不需要更新
        IsNeedUpdateClient = false;

        // 确认操作标识
        bool isConfirmed = false;

        // 获取最低要求版本
        string lowestClientVersion = ConfigMgr.Get<string>("lowest_client_version", string.Empty);

        // 获取当前客户端版本
        string clientVersion = ConfigMgr.Get<string>("client_version", string.Empty);

        // 判断是否需要更新0相等，1更高，-1更低
        int compareLowestRet = Config.CompareVersion(clientVersion, lowestClientVersion);

        // 需要更新客户端, 这个地方需要更新客户端
        // 小于要求的最低版本
        if (compareLowestRet < 0)
        {
            // 如果没有提示信息直接跳转
            // 弹出提示信息
            DialogMgr.ShowSimpleSingleBtnDailog(new CallBack((para, obj) =>
            {
                UpdateClient(ConfigMgr.Get<string>("client_url", string.Empty));
            }),
            LocalizationMgr.Get("ResourceCheckWnd_18", LocalizationConst.START), 
            string.Format(LocalizationMgr.Get("ResourceCheckWnd_16", LocalizationConst.START),
            lowestClientVersion));
        } 
        else 
        {
            // 获取当前最新客户端版本
            string newClientVersion = ConfigMgr.Get<string>("new_client_version", string.Empty);

            // 判断是否需要更新0相等，1更高，-1更低
            int compareNewRet = Config.CompareVersion(clientVersion, newClientVersion);

            // 小于最新版本
            if (compareNewRet < 0)
            {
                // 如果没有提示信息直接跳转
                DialogMgr.ShowSimpleDailog(new CallBack((para, obj) =>
                {
                    if ((bool)obj[0])
                        UpdateClient(ConfigMgr.Get<string>("client_url", string.Empty));
                    else
                        isConfirmed = true;
                }),
                LocalizationMgr.Get("ResourceCheckWnd_17", LocalizationConst.START),
                string.Format(LocalizationMgr.Get("ResourceCheckWnd_15", LocalizationConst.START), newClientVersion),
                LocalizationMgr.Get("ResourceCheckWnd_19", LocalizationConst.START),
                LocalizationMgr.Get("ResourceCheckWnd_20", LocalizationConst.START));
            }
            else
                isConfirmed = true;
        }

        // 等到玩家确认
        while (! isConfirmed)
            yield return null;
    }

    // 从服务器下载最新的配置和代码
    public static IEnumerator Init()
    {
         // 重置初始状态
        initSuccessed = false;

        // 载入本地配置信息
        while (true)
        {
            // 读取内置配置
            UnityEngine.Object cfgResOb = ResourceMgr.Load(CONFIG_PATH);

            // 载入资源失败
            if (cfgResOb == null)
            {
                LogMgr.Trace("载入本地config配置失败");
                continue;
            }

            // 合并资源
            Merge((cfgResOb as TextAsset).text);

            // 释放资源
            ResourceMgr.UnLoad(CONFIG_PATH);
            break;
        }

        // 如果是验证客户端
        if (AuthClientMgr.IsAuthClient)
        {
            // 标识初始化成功
            initSuccessed = true;
            yield break;
        }

        // 获取配置信息
        string[] cfgUrls = new string[]{};
        string bundleId = string.Empty;
        string clientName = string.Empty;

#if UNITY_EDITOR

        cfgUrls = Get<string[]>("config_url", new string[]{});
        bundleId = Get<string>("bundle_id", string.Empty);
        clientName = "QCPLAY";

#else

        // 内测版本直接取本地的配置
        cfgUrls = Get<string[]>("config_url", new string[]{});

        // 获取应用bundleId
        bundleId = QCCommonSDK.QCCommonSDK.GetBundleId();

        // 通过sdk获取Client_Name
        clientName = QCCommonSDK.QCCommonSDK.FindNativeSetting("Client_Name");

        // 如果没有指定Client_Name，则用默认的QCPLAY
        if (string.IsNullOrEmpty(clientName))
            clientName = "QCPLAY";

#endif

        // 获取不到配置信息
        if (cfgUrls.Length == 0 || string.IsNullOrEmpty(bundleId))
        {
            // 标识初始化成功
            initSuccessed = true;
            yield break;
        }

        // 获取客户端版本
        string clientVersion = Get<string> ("client_version", string.Empty);
        Config serverCfg = null;
        int idx = 0;

        // 尝试下载资源指到等配置信息下载成功
        while(true)
        {
            if (idx >= cfgUrls.Length)
                idx = 0;

            WWWForm form = new WWWForm ();
            form.AddField ("language", (int)Application.systemLanguage);
            form.AddField ("client_version", clientVersion);
            form.AddField ("bundle_id", bundleId);
            form.AddField ("client_name", clientName);

            WWW www = new WWW(cfgUrls[idx], form);

            yield return www;

            // 下载成功
            if (! string.IsNullOrEmpty(www.error))
            {
                idx++;

                yield return new WaitForSeconds(1f);

                continue;
            }

            bool jsonIsOK = true;
            try
            {
                JsonData data = JsonMapper.ToObject(www.text);
                if (data.IsObject)
                {
                    // 构建服务器配置
                    serverCfg = new Config(data);

                    // 释放掉www
                    www.Dispose();
                }
            }
            catch (Exception ex)
            {
                jsonIsOK = false;
                LogMgr.Exception(ex);
            }

            // 解析json格式失败，重新下载配置文件
            if (! jsonIsOK)
            {
                LogMgr.Trace("配置文件格式错误");
                idx++;
                yield return new WaitForSeconds(1f); // 等待一会重试
                continue;
            }

            // 分析client_version
            if (string.IsNullOrEmpty(serverCfg.Get("lowest_client_version", string.Empty)))
            {
                LogMgr.Trace(string.Format("配置文件client_version错误"));
                idx++;
                yield return new WaitForSeconds(1f); // 等待一会重试
                continue;
            }

            // 分析version
            if (string.IsNullOrEmpty(serverCfg.version))
            {
                LogMgr.Trace(string.Format("配置文件version错误"));
                idx++;
                yield return new WaitForSeconds(1f); // 等待一会重试
                continue;
            }

            // 分析ip
            if (string.IsNullOrEmpty(serverCfg.Get("login_ip", string.Empty)))
            {
                LogMgr.Trace(string.Format("配置文件ip错误"));
                idx++;
                yield return new WaitForSeconds(1f); // 等待一会重试
                continue;
            }

            // 分析Port
            if (string.IsNullOrEmpty(serverCfg.Get("login_port", string.Empty)))
            {
                LogMgr.Trace(string.Format("配置文件Port错误"));
                idx++;
                yield return new WaitForSeconds(1f); // 等待一会重试
                continue;
            }

            // 分析client_url
            if (string.IsNullOrEmpty(serverCfg.Get<string>("client_url")))
            {
                LogMgr.Trace(string.Format("配置文件client_url错误"));
                idx++;
                yield return new WaitForSeconds(1f); // 等待一会重试
                continue;
            }

            // 分析ab_urls
            if (serverCfg.Get<JsonData>("ab_urls", null) == null)
            {
                LogMgr.Trace(string.Format("配置文件ab_urls错误"));
                idx++;
                yield return new WaitForSeconds(1f); // 等待一会重试
                continue;
            }

            // ipv6 映射
            if (serverCfg.Get<JsonData>("ipMapDomain", null) != null)
                NetConnectorImpl.IpMapDomain = serverCfg.Get<JsonData>("ipMapDomain");

            break;
        }

        // 合并服务器下载资源 
        Merge(serverCfg);

        // 标识初始化成功
        initSuccessed = true;
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
            LogMgr.Exception(e);
            return false;
        }
    }

#endregion
}
