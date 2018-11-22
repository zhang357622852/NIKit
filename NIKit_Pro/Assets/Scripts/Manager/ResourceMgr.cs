using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading;
using LitJson;

public class ResourceMgr : Singleton<ResourceMgr>
{
    #region 变量

    // 资源缓存列表
    // key->filePath
    private Dictionary<string, Resource> mResourceMap = new Dictionary<string, Resource>();

    // AssetBundle资源列表
    // key->bundleName
    private Dictionary<string, AssetBundle> mAssetBundleMap = new Dictionary<string, AssetBundle>();

    // ab包引用列表记录
    // key->bundleName
    private Dictionary<string, List<Resource>> mAB2ResourceMap = new Dictionary<string, List<Resource>>();

    // 是否自动垃圾回收
    private bool mAutoRecycle = true;

    // 已经回收资源数量
    private int mRecycleCount = 0;

    /// <summary>
    /// etc资源包，这个名字需要定死
    /// </summary>
    public string  ETC_BUNDLE = "etc";

    // 资源更新是否完成
    private bool isUpdateResOk = false;

    // etc资源映射表
    private Dictionary<string, byte[]> etcBtyeAssetMap = new Dictionary<string, byte[]>();
    private Dictionary<string, string> etcTextAssetMap = new Dictionary<string, string>();
    private List<string> etcSkillActionList = new List<string>();

    #endregion

    #region 属性

    /// <summary>
    /// 获取资源
    /// </summary>
    /// <value>The resource map.</value>
    public Dictionary<string, Resource> ResourceMap
    {
        get
        {
            return mResourceMap;
        }
    }

    /// <summary>
    /// 获取AssetBundle资源
    /// </summary>
    /// <value>The asset bundle map.</value>
    public Dictionary<string, AssetBundle> AssetBundleMap
    {
        get
        {
            return mAssetBundleMap;
        }
    }

    /// <summary>
    /// 获取AssetBundle和资源映射关系
    /// </summary>
    /// <value>The asset bundle map.</value>
    public Dictionary<string, List<Resource>> AB2ResourceMap
    {
        get
        {
            return mAB2ResourceMap;
        }
    }

    /// <summary>
    /// 是否自动垃圾回收
    /// </summary>
    /// <value><c>true</c> if auto recycle; otherwise, <c>false</c>.</value>
    public bool AutoRecycle
    {
        get
        {
            return mAutoRecycle;
        }
        set
        {
            mAutoRecycle = value;
        }
    }

    /// <summary>
    /// ab资源AssetBundleManifest依赖关系
    /// </summary>
    public AssetBundleManifest Manifest { get; private set;}

    /// 当前已下载的资源总量
    public int DownloadedBytes { get; private set; }

    #endregion

    #region 内部接口

    /// <summary>
    /// 回收协程
    /// </summary>
    /// <returns>The daemon.</returns>
    private IEnumerator RecycleDaemon()
    {
        // 永固不停歇的回收资源
        while (true)
        {
            // TODO，还需要按“最远未使用”进行过滤resource
            yield return TimeMgr.WaitForRealSeconds(10f);

            // 不需要回收资源
            if (! mAutoRecycle)
                continue;

            // 回收资源
            Recycle(false);
        }
    }

    /// <summary>
    /// 载入AssetBundleManifest
    /// </summary>
    /// <returns>The asset bundle manifest.</returns>
    private IEnumerator LoadAssetBundleManifest()
    {
        // 本地版本文件不存在
        string filePath = string.Format("{0}/AssetBundles", ConfigMgr.ASSETBUNDLES_PATH);
        if (!File.Exists(filePath))
            yield break;

        // 载入assetBundle
        AssetBundle assetBundle = AssetBundle.LoadFromFile(filePath);
        if (assetBundle == null)
            yield break;

        // 载入AssetBundleManifest
        Manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        // 卸载掉ab资源
        assetBundle.Unload(false);
    }

    /// <summary>
    /// 获取某个Bundle依赖的Bundle列表
    /// </summary>
    private string[] GetDependentBundle(string bundle)
    {
        // 如果Manifest为null
        if (Manifest == null)
            return new string[] { };

        // 返回GetAllDependencies
        return Manifest.GetAllDependencies(bundle);
    }

    /// <summary>
    /// 载入资源
    /// </summary>
    private Resource LoadResource(string resPath, bool isDontUnload, bool isSprite = false, bool isAtlas = false)
    {
        Resource res;

        // 尝试获取已加载过的资源, 如果资源还没有加载过则直接加载
        if (! ResourceMap.TryGetValue(resPath, out res))
        {
            // 新建资源
            res = new Resource(resPath, isDontUnload);
            ResourceMap.Add(res.Path, res);
        }

        // 如果资源已经被成功载入了，直接返回资源
        if (res.State == Resource.STATE.LOADED)
        {
            res.LoadSuccessed();
            return res;
        }

        // 判断是否是ab资源，如果不在版本树上则表示需要本地加载
        string bundle = VersionMgr.GetAssetBundleName(resPath);

        // 尝试载入资源
        try
        {
            // 如果是内部资源
            if (string.IsNullOrEmpty(bundle))
            {
                // 尝试载入资源，如果是编辑器模式则通过UnityEditor.AssetDatabase.LoadAssetAtPath载入
                // 否则通过Resources.Load载入
                if (Platform.IsEditor)
                {
                    // 编辑器测试阶段，直接加载即可
                    // 不是Sprite资源默认UnityEngine.Object
                    if (isSprite)
                        res.MainAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>(resPath);
                    else if (isAtlas)
                        res.MainAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UIAtlas>(resPath);
                    else
                        res.MainAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(resPath);
                }
                else
                {
                    // 默认资源路径Assets/Prefabs/Model/m1011_d.prefab
                    // 最终的目标路径为Prefabs/Model/m1011_d.prefab
                    resPath = resPath.Replace(Path.GetExtension(resPath), string.Empty).Replace("Assets/", string.Empty);

                    // 编辑器测试阶段，直接加载即可
                    // 不是Sprite资源默认UnityEngine.Object
                    if (isSprite)
                        res.MainAsset = Resources.Load<UnityEngine.Sprite>(resPath);
                    else if (isAtlas)
                        res.MainAsset = Resources.Load<UIAtlas>(resPath);
                    else
                        res.MainAsset = Resources.Load(resPath);
                }
            }
            else
            {
                // 获取该bundle依赖资源
                AssetBundle ab;
                string[] dependentBundles = GetDependentBundle(bundle);
                foreach (string name in dependentBundles)
                {
                    // 载入依赖ab资源
                    ab = LoadAssetBundle(name);

                    // 载入资源成功
                    if (ab == null)
                        continue;

                    // 添加资源管理列表
                    if (AB2ResourceMap.ContainsKey(name))
                        AB2ResourceMap[name].Add(res);
                    else
                        AB2ResourceMap[name] = new List<Resource>() { res };

                    // 添加到资源依赖列表中
                    res.AssetBundles.Add(name);
                }

                // 载入AssetBundle资源
                ab = LoadAssetBundle(bundle);

                // 载入载入成功
                if (ab != null)
                {
                    // 添加资源管理列表
                    if (AB2ResourceMap.ContainsKey(bundle))
                        AB2ResourceMap[bundle].Add(res);
                    else
                        AB2ResourceMap[bundle] = new List<Resource>() { res };

                    // 通过AssetBundle载入资源
                    if (isSprite)
                        res.MainAsset = ab.LoadAsset<UnityEngine.Sprite>(resPath);
                    else if(isAtlas)
                        res.MainAsset = ab.LoadAsset<UIAtlas>(resPath);
                    else
                        res.MainAsset = ab.LoadAsset(resPath);

                    // 添加到资源依赖列表中
                    res.AssetBundles.Add(bundle);
                }
            }
        }
        catch (Exception e)
        {
            // 给出异常提示信息
            NIDebug.LogException(e);
        }

        // 执行资源载入结果
        if (res.MainAsset != null)
            res.LoadSuccessed();
        else
            res.LoadFaild();

        // 返回资源
        return res;
    }

    /// <summary>
    /// 更新资源
    /// </summary>
    /// <returns>The daemon.</returns>
    private IEnumerator UpdateAllResources()
    {
        // 重置下载字节数标识
        DownloadedBytes = 0;

        // 获取资源下载地址
        IList abUrls = (IList)ConfigMgr.Get<JsonData> ("ab_urls", null);

        // 没有资源更新地址
        if (abUrls == null || abUrls.Count == 0 || VersionMgr.UpdateResDict.Count == 0)
        {
            // 标识资源更新完成
            isUpdateResOk = true;

            yield break;
        }

        // 抛出开始下载资源事件
        //LoadingMgr.ChangeState(ResourceLoadingConst.LOAD_TYPE_UPDATE, ResourceLoadingStateConst.LOAD_STATE_UPDATE);

        // 获取需要更新资源大小
        int totalDownloadBytes = VersionMgr.DownloadSize;
        int downloadedBytes = 0;
        int idx = 0;

        // 逐个资源下载
        foreach (string fileName in VersionMgr.UpdateResDict.Keys)
        {
            // 获取资源名
            if (string.IsNullOrEmpty(fileName))
                continue;

            // 保证该资源的download成功
            while (true)
            {
                // 如果idx已经超过了abUrls范围修正一下
                if (idx >= abUrls.Count)
                    idx = 0;

                // 创建下载器
                Download download = new Download(
                    string.Format("{0}/{1}", abUrls[idx], fileName),
                    10,
                    ConfigMgr.DOWNLOAD_PATH + fileName);

                Debug.Log(string.Format("下载补丁包 {0}", download.url));

                // 等待获取头部
                download.StartGetResponse();
                while (!download.isGetResponse && !download.isTimeOut && download.error == 0)
                    yield return null;

                // GetResponse失败，使用新的链接地址重新下载
                if (!download.isGetResponse || download.error != 0 || download.isTimeOut)
                {
                    // 如果是磁盘空间满的情况，直接退出游戏
                    if (download.error == -2)
                    {
                        // 弹出窗口玩家让玩家确认一下，玩家确认后等待一下会在重试
                        bool isConfirmed = false;
                        //DialogMgr.ShowSimpleSingleBtnDailog(
                        //    new CallBack((para, obj) =>{ isConfirmed = true; }),
                        //    LocalizationMgr.Get("ResourceCheckWnd_8", LocalizationConst.START));

                        // 等到玩家确认
                        while (! isConfirmed)
                            yield return null;

                        // 退出应用
                        Application.Quit();
                        yield break;
                    }

                    idx++;
                    download.Clear();

                    // 等待一会重试
                    yield return new UnityEngine.WaitForSeconds(0.1f); // 等待一会重试
                    continue;
                }

                // 开始下载资源
                download.StartDownload();

                // 等待资源下载完成
                while (!download.isDownloaded && !download.isTimeOut && download.error == 0)
                {
                    // 等待0.1s，尽量不要一帧等待
                    yield return new UnityEngine.WaitForSeconds(0.1f);

                    DownloadedBytes = download.GetTotleDownloadSize() + downloadedBytes;
                    //LoadingMgr.SetProgress (DownloadedBytes/(float)totalDownloadBytes);
                }

                // 如果下载资源失败
                if (download.isTimeOut || download.error != 0)
                {
                    // 如果是磁盘空间满的情况，直接退出游戏
                    if (download.error == -2)
                    {
                        // 弹出窗口玩家让玩家确认一下，玩家确认后等待一下会在重试
                        bool isConfirmed = false;
                        //DialogMgr.ShowSimpleSingleBtnDailog(
                        //    new CallBack((para, obj) =>{ isConfirmed = true; }),
                        //    LocalizationMgr.Get("ResourceCheckWnd_8", LocalizationConst.START));

                        // 等到玩家确认
                        while (! isConfirmed)
                            yield return null;

                        Application.Quit();
                        yield break;
                    }

                    idx++;
                    download.Clear();

                    NIDebug.Log("{0}响应超时或错误", fileName);
                    continue;
                }

                // 累计下载进度
                downloadedBytes += download.GetTotleDownloadSize();

                NIDebug.Log("downloadedBytes 大小{0},oldLoadedBytes 大小为{1}", download.downloadedBytes, download.oldLoadedBytes);
                NIDebug.Log("已下载总量{0}", downloadedBytes);

                // 释放下载器
                download.Clear();

                // 退出循环
                break;
            }
        }

        // 设定更新总进度
        //LoadingMgr.SetProgress (1.0f);

        //// 等待进度条结束
        //while(!LoadingMgr.IsLoadingEnd(LoadingType.LOAD_TYPE_UPDATE_RES,
        //    ResourceLoadingConst.LOAD_TYPE_UPDATE))
        //    yield return null;

        //LoadingMgr.ChangeState(ResourceLoadingConst.LOAD_TYPE_DECOMPRESS, ResourceLoadingStateConst.LOAD_STATE_CHECK);

        //// 切换流程
        //LoadingMgr.ChangeState(ResourceLoadingConst.LOAD_TYPE_DECOMPRESS, ResourceLoadingStateConst.LOAD_STATE_UPDATE);

        // 当前解压缩文件进度
        int unzipSize = 0;
        bool isUnzipFailed = false;
        string targetPath = ConfigMgr.ASSETBUNDLES_PATH + "/";

        // 资源解压缩
        foreach (string fileName in VersionMgr.UpdateResDict.Keys)
        {
            // 文件名为空
            if (string.IsNullOrEmpty(fileName))
                continue;

            // 构建解压缩
            Unzip zip = new Unzip(
                            ConfigMgr.GetLocalRootPathWWW(ConfigMgr.DOWNLOAD_NAME + "/" + fileName),
                            targetPath,
                            VersionMgr.UpdateResDict[fileName]);

            // 开始解压缩
            zip.Start();

            // 等待解压缩结束
            while (!zip.IsUnziped)
            {
                // 更新进度
                //LoadingMgr.SetProgress((float)(unzipSize + zip.UnzipBytes) / VersionMgr.UnzipSize);
                yield return null;
            }

            // 累计解压缩数量
            unzipSize += zip.UnzipBytes;
            //LoadingMgr.SetProgress((float)unzipSize / VersionMgr.UnzipSize);

            // 释放zip
            zip.Clear();

            // 回收内存
            DoRecycleGC();

            // 解压缩文件成功
            if (zip.Error == 0)
            {
                // 解压缩成功, 删除补丁文件
               // FileMgr.DeleteFile(ConfigMgr.DOWNLOAD_PATH + "/" + fileName);

                // 更新本地版本文件
                VersionMgr.SyncVersion(VersionMgr.UpdateResDict[fileName]);

                continue;
            }

            // 如果解压缩失败
            /// 0  : 解压缩成功
            /// -1 : 压缩文件载入失败
            /// -2 : 内存分配失败
            /// -3 : 文件写入失败
            /// -4 : 其他异常信息
            string msg = string.Empty;
            //if (zip.Error == -2)
            //    msg = LocalizationMgr.Get("ResourceCheckWnd_22", LocalizationConst.START);
            //else if (zip.Error == -3)
            //    msg = LocalizationMgr.Get("ResourceCheckWnd_8", LocalizationConst.START);

            // 给出提示信息
            if (! string.IsNullOrEmpty(msg))
            {
                // 弹出窗口玩家让玩家确认一下，玩家确认后等待一下会在重试
                bool isConfirmed = false;
                //DialogMgr.ShowSimpleSingleBtnDailog(new CallBack((para, obj) =>
                //        {
                //            isConfirmed = true;
                //        }), msg);

                // 等到玩家确认
                while (!isConfirmed)
                    yield return null;

                // 退出游戏
                Application.Quit();
                yield break;
            }

            // 标识有资源解压缩失败
            isUnzipFailed = true;

            // 解压缩成功, 删除补丁文件
            //FileMgr.DeleteFile(ConfigMgr.DOWNLOAD_NAME + "/" + fileName);

            // 给出提示信息
            Debug.Log(string.Format("补丁包{0}结果解压缩{1}", fileName, zip.Error));
        }

        // 如果有资源解压缩失败，需要玩家确认重启游戏重新下载资源
        //if (isUnzipFailed)
        //{
        //    // 弹出窗口玩家让玩家确认一下，玩家确认后等待一下会在重试
        //    bool isConfirmed = false;
        //    DialogMgr.ShowSimpleSingleBtnDailog(new CallBack((para, obj) =>
        //        {
        //            isConfirmed = true;
        //        }), LocalizationMgr.Get("ResourceCheckWnd_9", LocalizationConst.START));

        //    // 等到玩家确认
        //    while (! isConfirmed)
        //        yield return null;

        //    // 退出游戏
        //    Application.Quit();
        //    yield break;
        //}


        //// 等待进度条结束
        //while(!LoadingMgr.IsLoadingEnd(LoadingType.LOAD_TYPE_UPDATE_RES,
        //    ResourceLoadingConst.LOAD_TYPE_DECOMPRESS))
            yield return null;

        // 标识资源更新完成
        isUpdateResOk = true;
    }

    /// <summary>
    /// 初始化开始场景中的资源
    /// </summary>
    /// <param name="_files">Files.</param>
    private void InitStartRes()
    {
        // 初始化开始场景语言
       // LocalizationMgr.InitStartRes();

        // 初始化start场景csv
        CsvFileMgr.InitStartCsv();
    }

    /// <summary>
    /// 载入本地etc资源线程
    /// </summary>
    /// <param name="_files">Files.</param>
    private void DeLoadEtcTextThread(object _path)
    {
        // 目录不存在，不处理
        string path = (string) _path;
        if (!Directory.Exists((string) path))
            return;

        // 载入全部资源
        foreach (string filePath in Directory.GetFiles(path, "*"))
        {
            // meta文件不处理
            if (filePath.Contains(".meta"))
                continue;

            // 转换路径
            string tempPath = filePath.Replace("\\", "/");
            string ex = Path.GetExtension(tempPath);

            // 文本资源分类CSV_EXT需要和其他资源区分开
            // 技能序列
            if (string.Equals(ex, CsvFileMgr.CSV_EXT))
                etcBtyeAssetMap.Add(tempPath.ToLower(), File.ReadAllBytes(tempPath));
            else if (string.Equals(ex, ".xml") && Path.GetFileName(tempPath).Contains("skill_action"))
                etcSkillActionList.Add(File.ReadAllText(tempPath));
            else if (Path.GetFileName(tempPath).Contains(".lua.txt"))
            {
                string scriptName = Path.GetFileName(tempPath);
                string strSptNo = scriptName.Replace(".lua.txt", "");
                int sptNo;
                if (! int.TryParse(strSptNo, out sptNo))
                {
                    // 如果是lua脚本公式
                   // FormulaMgr.AddLuaFormula(strSptNo, File.ReadAllText(tempPath));
                } else
                {
                    // 如果是lua脚本
                   // ScriptMgr.AddLuaScript(sptNo, File.ReadAllText(tempPath));
                }
            }
            else
                etcTextAssetMap.Add(tempPath.ToLower(), File.ReadAllText(tempPath));
        }
    }

    /// <summary>
    /// 加载技能文件（仅在验证客户端模式下使用，直接加载resource下的资源）
    /// </summary>
    /// <param name="_files">Files.</param>
    private void LoadSkillActionFile()
    {
        if (Platform.IsEditor)
        {
            // 载入全部资源
            foreach (string filePath in Directory.GetFiles(ConfigMgr.ETC_PATH, "*"))
            {
                // meta文件不处理
                if (filePath.Contains(".meta"))
                    continue;

                // 转换路径
                string tempPath = filePath.Replace("\\", "/");
                string ex = Path.GetExtension(tempPath);

                if (string.Equals(ex, ".xml") && Path.GetFileName(tempPath).Contains("skill_action"))
                    etcSkillActionList.Add(ResourceMgr.Instance.Load(tempPath).ToString());
            }
        }
        else
        {
            // 取得skill_action综合文件，加载所有skill_action
            string resPath = string.Format("Assets/{0}", ConfigMgr.SKILL_ACTION_DICT);

            string resText = ResourceMgr.Instance.LoadText(resPath);

            if (string.IsNullOrEmpty(resText))
            {
                NIDebug.Log("加载skill_action_dict文件失败。");
                return;
            }

            string[] lines = GameUtility.Explode(resText, "\n");
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                resText = ResourceMgr.Instance.LoadText(line);

                if (string.IsNullOrEmpty(resText))
                {
                    NIDebug.Log("加载{0}文件失败。", line);
                    continue;
                }

                etcSkillActionList.Add(ResourceMgr.Instance.LoadText(line));
            }
        }
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// Encypt the specified targetData.
    /// </summary>
    /// <param name="targetData">Target data.</param>
    public byte[] Encypt(byte[] targetData)
    {
        byte[] encyptBytes = new byte[targetData.Length];
        byte encyptKey1 = 157;
        byte encyptKey2 = 126;

        // 字符偏移，简单的加密处理
        for (int i = 0; i < targetData.Length; ++i)
        {
            if ((i % 2) == 0)
                encyptBytes[i] = (byte)(targetData[i] ^ encyptKey1);
            else
                encyptBytes[i] = (byte)(targetData[i] ^ encyptKey2);
        }

        // 返回偏移后的byte[]
        return encyptBytes;
    }

    /// <summary>
    /// 载入有文本资源
    /// 1. Etc目录下的所有资源csv资源
    /// 2. Etc目录下的所有资源txt资源
    /// 3. Language目录下的所有资源txt资源
    /// 4. ActionSet目录下的所有资源xml资源
    /// </summary>
    /// <returns>The all text.</returns>
    public IEnumerator LoadEtcText()
    {
        // 本地版本文件不存在
        string filePath = string.Format("{0}/{1}", ConfigMgr.ASSETBUNDLES_PATH, ETC_BUNDLE);
        if (!File.Exists(filePath))
        {
            // 载入etc资源线程
            Thread loadEtcTextThread = new Thread(new ParameterizedThreadStart(DeLoadEtcTextThread));
            loadEtcTextThread.Start(ConfigMgr.ETC_PATH);

            // 等待载入资源完毕
            while (loadEtcTextThread.IsAlive)
                yield return null;

            // 开启线程载入资源
            yield break;
        }

        // www载入资源
        WWW www = new WWW(ConfigMgr.GetLocalRootPathWWW(string.Format("{0}/{1}", ConfigMgr.ASSETBUNDLES_NAME, ETC_BUNDLE)));

        // 等待资源载入完成
        yield return www;

        // 解密资源
        byte[] unEncyptbytes = Encypt(www.bytes);

        // 直接载入etc.ab资源
        AssetBundle ab = AssetBundle.LoadFromMemory(unEncyptbytes);

        // 资源载入失败
        if (ab == null)
            yield break;

        // 异步载入该ab下所有子资源
        AssetBundleRequest assetLoadRequest = ab.LoadAllAssetsAsync();
        yield return assetLoadRequest;

        // 遍历各个资源进行分类处理
        // bytes需要和其他资源区分开
        foreach (string assetPath in ab.GetAllAssetNames())
        {
            // 载入资源
            TextAsset textAsset = ab.LoadAsset<TextAsset>(assetPath);
            if (textAsset == null)
                continue;

            // 获取资源扩展名
            string ex = Path.GetExtension(assetPath);

            // 文本资源分类CSV_EXT需要和其他资源区分开
            if (string.Equals(ex, CsvFileMgr.CSV_EXT))
                etcBtyeAssetMap.Add(assetPath, textAsset.bytes);
            else if (string.Equals(ex, ".xml") && Path.GetFileName(assetPath).Contains("skill_action"))
                etcSkillActionList.Add(textAsset.text);
            else if (Path.GetFileName(assetPath).Contains(".lua.txt"))
            {
                string scriptName = Path.GetFileName(assetPath);
                string strSptNo = scriptName.Replace(".lua.txt", "");
                int sptNo;
                if (! int.TryParse(strSptNo, out sptNo))
                {
                    // 如果是lua脚本公式
                    //FormulaMgr.AddLuaFormula(strSptNo, textAsset.text);
                } else
                {
                    // 如果是lua脚本
                    //ScriptMgr.AddLuaScript(sptNo, textAsset.text);
                }
            }
            else
                etcTextAssetMap.Add(assetPath, textAsset.text);
        }

        // 释放etc.ab资源
        ab.Unload(false);
    }

    /// <summary>
    /// 卸载所有文本资源
    /// </summary>
    public void UnloadEtcText()
    {
        // 清空缓存资源
        if (Platform.IsEditor)
        {
            etcBtyeAssetMap.Clear();
            etcTextAssetMap.Clear();
            etcSkillActionList.Clear();
        }
        else
        {
            // etc资源映射表
            etcBtyeAssetMap = new Dictionary<string, byte[]>();
            etcTextAssetMap = new Dictionary<string, string>();
            etcSkillActionList = new List<string>();
        }

        // 主动回收一下资源
        ResourceMgr.Instance.Recycle(true);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public IEnumerator Init()
    {
        // 开始不停的回收资源
        Coroutine.DispatchService(RecycleDaemon());

        // 载入版本控制文件
        yield return Coroutine.DispatchService(VersionMgr.LoadVersionFile());

        // 尝试解压资源, 如果是第一次启动游戏则需要将附在包体中的资源
        yield return Coroutine.DispatchService(ResourceMgr.Instance.DoDecompressRes());

        // 抛出开始下载资源事件
        //LoadingMgr.ChangeState(ResourceLoadingConst.LOAD_TYPE_UPDATE, ResourceLoadingStateConst.LOAD_STATE_CHECK);

        // 初始化版本控制
        yield return Coroutine.DispatchService(VersionMgr.CompareOnlineVersion());

        // 如果下载的大小大于20M，并且在非wifi情况下，给提示
        if(VersionMgr.DownloadSize >= 20 * 1024 * 1024 &&
            Application.internetReachability != NetworkReachability.ReachableViaLocalAreaNetwork)
        {
            bool isConfirmed = false;

            // 可选择更新或者是不更新,不更新直接关闭客户端
            //DialogMgr.ShowSimpleDailog(new CallBack((para, obj) =>
            //{
            //    if ((bool)obj[0])
            //        isConfirmed = true;
            //    else
            //        // 关闭客户端
            //        Application.Quit();
            //}),
            //    string.Format(LocalizationMgr.Get("ResourceLoadingWnd_7", LocalizationConst.START), VersionMgr.DownloadSize/(1024*1024)),
            //    LocalizationMgr.Get("ResourceLoadingWnd_6", LocalizationConst.START),
            //    LocalizationMgr.Get("ResourceLoadingWnd_8", LocalizationConst.START));

            while(!isConfirmed)
                yield return null;
        }

        // 下载所有需要更新的资源
        yield return Coroutine.DispatchService(UpdateAllResources());

        // 等待资源更新完成
        while (!isUpdateResOk)
            yield return null;

        // 执行更新结束处理
        yield return Coroutine.DispatchService(VersionMgr.DoSycnResEnd(true));

        // 载入AssetBundle依赖关系
        yield return Coroutine.DispatchService(LoadAssetBundleManifest());

        yield return null;
    }

    /// <summary>
    /// 尝试解压资源, 如果是第一次启动游戏则需要将附在包体中的资源
    /// 所有随包资源配置在patch_res中
    /// 具体配置格式patch_res={1.7z|2.7z...}
    /// </summary>
    /// <returns>The daemon.</returns>
    public IEnumerator DoDecompressRes()
    {
        //// 抛出开始解压包体中资源事件
        //LoadingMgr.ChangeState(ResourceLoadingConst.LOAD_TYPE_START_DECOMPRESS, ResourceLoadingStateConst.LOAD_STATE_CHECK);

        //// 获取需要解压缩资源列表
        //// 如果没有随包资源，不处理
        //string[] patchRes = ConfigMgr.Get<string[]>("patch_res", new string[]{});
        //if (patchRes.Length == 0)
        //    yield break;

        //// 对比随包版本文件和本地资源版本文件对比判断需要解压具体那些资源
        //yield return Coroutine.DispatchService(VersionMgr.ComparePackageVersion(patchRes));

        //// 如果不需要解压缩不处理
        //if (VersionMgr.UpdateResDict.Count == 0)
        //    yield break;

        //// 抛出开始解压包体中资源事件
        //LoadingMgr.ChangeState(ResourceLoadingConst.LOAD_TYPE_START_DECOMPRESS, ResourceLoadingStateConst.LOAD_STATE_UPDATE);

        //int unzipSize = 0;
        //string targetPath = ConfigMgr.ASSETBUNDLES_PATH + "/";

        //// 获取版本需要更新列表
        //foreach(string file in VersionMgr.UpdateResDict.Keys)
        //{
        //    // 构建解压缩
        //    Unzip zip = new Unzip(ConfigMgr.GetStreamingPathWWW(file), targetPath, VersionMgr.UpdateResDict[file]);

        //    // 开始解压缩
        //    zip.Start();

        //    // 等待解压缩结束
        //    while (! zip.IsUnziped)
        //    {
        //        // 更新进度
        //        LoadingMgr.SetProgress((float) (unzipSize + zip.UnzipBytes) / VersionMgr.UnzipSize);
        //        yield return null;
        //    }

        //    // 释放zip
        //    zip.Clear();

        //    // 主动回收一下资源
        //    DoRecycleGC();

        //    // 如果解压缩失败
        //    /// 0  : 解压缩成功
        //    /// -1 : 压缩文件载入失败
        //    /// -2 : 内存分配失败
        //    /// -3 : 文件写入失败
        //    /// -4 : 其他异常信息
        //    if (zip.Error != 0)
        //    {
        //        string msg = string.Empty;
        //        if (zip.Error == -1)
        //            msg = string.Format(LocalizationMgr.Get("ResourceCheckWnd_21", LocalizationConst.START), file);
        //        else if (zip.Error == -2)
        //            msg = LocalizationMgr.Get("ResourceCheckWnd_22", LocalizationConst.START);
        //        else if (zip.Error == -3)
        //            msg = LocalizationMgr.Get("ResourceCheckWnd_8", LocalizationConst.START);
        //        else
        //        {
        //            // 解压缩失败
        //            msg = string.Format(LocalizationMgr.Get("ResourceCheckWnd_23", LocalizationConst.START), file);
        //        }

        //        // 弹出窗口玩家让玩家确认一下，玩家确认后等待一下会在重试
        //        bool isConfirmed = false;
        //        DialogMgr.ShowSimpleSingleBtnDailog(new CallBack((para, obj) =>
        //                {
        //                    isConfirmed = true;
        //                }), msg);

        //        // 等到玩家确认
        //        while (!isConfirmed)
        //            yield return null;

        //        // 退出游戏
        //        Application.Quit();
        //        yield break;
        //    }
        //    else
        //    {
        //        // 记录解压缩进度
        //        unzipSize += zip.UnzipBytes;

        //        // 更新本地版本文件
        //        VersionMgr.SyncVersion(VersionMgr.UpdateResDict[file]);

        //        // 更新进度
        //        LoadingMgr.SetProgress((float)unzipSize / VersionMgr.UnzipSize);
        //    }
        //}

        //// 更新进度解压缩进度
        //LoadingMgr.SetProgress(1f);

        //// 等待进度条结束
        //while(!LoadingMgr.IsLoadingEnd(LoadingType.LOAD_TYPE_UPDATE_RES,
        //          ResourceLoadingConst.LOAD_TYPE_START_DECOMPRESS))
        //    yield return null;

        // 写入版本文件
        yield return Coroutine.DispatchService(VersionMgr.DoSycnResEnd(false));

        yield break;
    }

    /// <summary>
    /// 回收资源
    /// </summary>
    public void Recycle(bool force)
    {
        Resource[] reses = new Resource[mResourceMap.Count];

        mResourceMap.Values.CopyTo(reses, 0);

        // 遍历当前资源
        for (int i = 0; i < reses.Length; i++)
        {
            Resource res = reses[i];

            // 资源不需要卸载
            if (res.IsDontUnload)
                continue;

            // 还不到释放时间，定时10s释放
            if (! force && (Time.time - res.LastAccessTime) < 10f)
                continue;

            // 还有资源正在引用中不释放主要资源
            res.References.RemoveAll(o => { return o == null; });

            if (res.References.Count > 0)
            {
                res.Unload(false);
            }
            else
            {
                // 卸载资源
                res.Unload(true);
                ResourceMap.Remove(res.Path);
                mRecycleCount++;
            }
        }

        // 达到了清理条件
        if (force || mRecycleCount >= 20)
        {
            // 重置mRecycleCount数量
            mRecycleCount = 0;

            // 回收资源
            Resources.UnloadUnusedAssets();
        }
    }

    /// <summary>
    /// 回收资源
    /// </summary>
    public void DoRecycleGC()
    {
        GC.Collect();
    }

    /// <summary>
    /// 加载sprite资源
    /// 返回加载完成的sprite对象
    /// </summary>
    public Sprite LoadSprite(string assetPath, bool isDontUnload = false)
    {
        // 载入资源
        Resource res = LoadResource(assetPath, isDontUnload, true);

        // 资源载入失败
        if (res == null)
            return null;

        // 返回图片资源
        return res.MainAsset as Sprite;
    }

    /// <summary>
    /// 加载图集
    /// </summary>
    public UIAtlas LoadAtlas(string assetPath, bool isDontUnload = false)
    {
        // 载入资源
        Resource res = LoadResource(assetPath, isDontUnload, false, true);

        // 资源载入失败
        if (res == null)
            return null;

        // 返回图片资源
        return res.MainAsset as UIAtlas;
    }

    /// <summary>
    /// 加载texture资源
    /// 返回texture2D文件
    /// </summary>
    /// <param name="assetPath">Asset path.</param>
    public UnityEngine.Texture2D LoadTexture(string resPath, bool isDontUnload = false)
    {
        // 载入资源
        Resource res = LoadResource(resPath, isDontUnload);

        // 资源载入失败
        if (res == null)
            return null;

        // 返回图片资源
        return res.MainAsset as UnityEngine.Texture2D;
    }

    /// <summary>
    /// 加载text资源
    /// </summary>
    public string LoadText(string resPath)
    {
        string text = string.Empty;

        // 先看看缓存etc资源是否存在
        if (etcTextAssetMap.TryGetValue(resPath.ToLower(), out text))
            return text;

        // 尝试载入资源
        UnityEngine.Object assetOb = Load(resPath);

        // 资源载入失败
        if (assetOb == null)
            return text;

        // 返回资源
        return (assetOb as TextAsset).text;
    }

    /// <summary>
    /// 加载text资源
    /// </summary>
    public byte[] LoadByte(string resPath)
    {
        byte[] bytes;

        // 先看看缓存etc资源是否存在
        if (etcBtyeAssetMap.TryGetValue(resPath.ToLower(), out bytes))
            return bytes;

        // 尝试载入资源
        UnityEngine.Object assetOb = Load(resPath);

        // 资源载入失败
        if (assetOb == null)
            return null;

        // 返回资源
        return (assetOb as TextAsset).bytes;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void UnLoad(string resPath)
    {
        Resource res;

        // 如果资源没有加载过，不处理
        if (! ResourceMap.TryGetValue(resPath, out res))
            return;

        // 卸载资源
        res.Unload(false);
    }

    /// <summary>
    /// 载入AssetBundle资源
    /// </summary>
    public AssetBundle LoadAssetBundle(string bundle)
    {
        AssetBundle ab;

        // 尝试获取已加载过的AssetBundle
        if (AssetBundleMap.TryGetValue(bundle, out ab) && ab != null)
            return ab;

        try
        {
            // 载入assetBundle
            ab = AssetBundle.LoadFromFile(ConfigMgr.ASSETBUNDLES_PATH + "/" + bundle);

            // 添加到资源管理列表中
            AssetBundleMap[bundle] = ab;
        }
        catch (Exception e)
        {
            // 给出异常提示信息
            NIDebug.LogException(e);
        }

        // 返回资源
        return ab;
    }

    /// <summary>
    /// 加载资源
    /// </summary>
    public UnityEngine.Object Load(string resPath, bool isDontUnload = false)
    {
        // 载入资源
        Resource res = LoadResource(resPath, isDontUnload);

        // 资源载入失败
        if (res == null)
            return null;

        // 获取资资源
        return res.MainAsset;
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    public IEnumerator LoadAsync(string resPath, bool isDontUnload = false, bool isSprite = false)
    {
        Resource res;

        // 尝试获取已加载过的资源, 如果资源还没有加载过则直接加载
        if (! ResourceMap.TryGetValue(resPath, out res))
        {
            // 新建资源
            res = new Resource(resPath, isDontUnload);
            ResourceMap.Add(res.Path, res);
        }

        // 如果资源已经被成功载入了，直接返回资源
        if (res.State == Resource.STATE.LOADED)
        {
            // 资源载入成功
            res.LoadSuccessed();
            yield break;
        }

        // 判断是否是ab资源，如果不在版本树上则表示需要本地加载
        string bundle = VersionMgr.GetAssetBundleName(resPath);

        // 如果是内部资源
        if (string.IsNullOrEmpty(bundle))
        {
            // 尝试载入资源，如果是编辑器模式则通过UnityEditor.AssetDatabase.LoadAssetAtPath载入
            // 否则通过Resources.Load载入
            if (Platform.IsEditor)
            {
                // 编辑器测试阶段，直接加载即可
                // 不是Sprite资源默认UnityEngine.Object
                if (!isSprite)
                    res.MainAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(resPath);
                else
                    res.MainAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>(resPath);
            }
            else
            {
                // 资源异步加载
                ResourceRequest request = null;

                // 默认资源路径Assets/Prefabs/Model/m1011_d.prefab
                // 最终的目标路径为Prefabs/Model/m1011_d
                resPath = resPath.Replace(Path.GetExtension(resPath), string.Empty).Replace("Assets/", string.Empty);

                // 编辑器测试阶段，直接加载即可
                // 不是Sprite资源默认UnityEngine.Object
                if (!isSprite)
                    request = Resources.LoadAsync(resPath);
                else
                    request = Resources.LoadAsync<UnityEngine.Sprite>(resPath);

                // 等待资源加载完成
                yield return request;

                // 获取asset资源对象
                res.MainAsset = request.asset;
            }
        }
        else
        {
            // 获取该bundle依赖资源
            AssetBundle ab;
            string[] dependentBundles = GetDependentBundle(bundle);
            foreach (string name in dependentBundles)
            {
                // 载入依赖ab资源
                ab = LoadAssetBundle(name);

                // 载入资源成功
                if (ab == null)
                    continue;

                // 添加资源管理列表
                if (AB2ResourceMap.ContainsKey(name))
                    AB2ResourceMap[name].Add(res);
                else
                    AB2ResourceMap[name] = new List<Resource>() { res };

                // 添加到资源依赖列表中
                res.AssetBundles.Add(name);
            }

            // 载入AssetBundle资源
            ab = LoadAssetBundle(bundle);

            // 载入载入成功
            if (ab != null)
            {
                // 添加资源管理列表
                if (AB2ResourceMap.ContainsKey(bundle))
                    AB2ResourceMap[bundle].Add(res);
                else
                    AB2ResourceMap[bundle] = new List<Resource>() { res };

                // 添加到资源依赖列表中
                if (! res.AssetBundles.Contains(bundle))
                    res.AssetBundles.Add(bundle);

                // 异步载入资源
                AssetBundleRequest assetLoadRequest = null;
                if (! isSprite)
                    assetLoadRequest = ab.LoadAssetAsync(resPath);
                else
                    assetLoadRequest = ab.LoadAssetAsync<UnityEngine.Sprite>(resPath);

                // 等待资源载入完成
                yield return assetLoadRequest;

                // 记录主要资源
                res.MainAsset = assetLoadRequest.asset;
            }
        }

        // 执行资源载入结果
        if (res.MainAsset != null)
            res.LoadSuccessed();
        else
            res.LoadFaild();
    }

    #endregion
}

/// <summary>
/// 本类负责缓存Unity资源
/// </summary>
public class Resource : IYieldObject
{
    public enum STATE
    {
        /// <summary>
        /// 卸载，资源初始状态，以及被回收后的状态
        /// </summary>
        UNLOAD,

        /// <summary>
        /// 加载成功
        /// </summary>
        LOADED,

        /// <summary>
        /// 加载失败
        /// </summary>
        LOAD_FAILED,
    }

    /// <summary>
    /// 资源路径
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// 资源状态
    /// </summary>
    STATE mState;
    public STATE State
    {
        get
        {
            return mState;
        }
        set
        {
            mState = value;
        }
    }

    /// <summary>
    /// MainAsset资源
    /// </summary>
    UnityEngine.Object mMainAsset;

    public UnityEngine.Object MainAsset
    {
        get
        {
            return mMainAsset;
        }
        set
        {
            // 设置mMainAsset
            mMainAsset = value;

            // assetOb不是GameObject
            GameObject gobj = mMainAsset as GameObject;

            if (gobj == null)
                return;

            // 设置资源引用计数
            ReferencesCounter rc = gobj.AddMissingComponent<ReferencesCounter>();

            rc.resPath = Path;
        }
    }

    /// <summary>
    /// 资源依赖AssetBundle列表
    /// 记录本身的assetBundleName和依赖的assetBundleName
    /// </summary>
    List<string> mAssetBundles = new List<string>();

    public List<string> AssetBundles
    {
        get
        {
            return mAssetBundles;
        }
        set
        {
            mAssetBundles = value;
        }
    }

    /// <summary>
    /// 该资源引用关系列表
    /// </summary>
    List<GameObject> mReferences = new List<GameObject>();

    public List<GameObject> References
    {
        get
        {
            return mReferences;
        }
    }

    // 资源是否不卸载
    public bool IsDontUnload { get; private set; }

    // 最后一次访问时间
    float mLastAccessTime = Time.time;

    public float LastAccessTime { get { return mLastAccessTime; } }

    // 构造
    public Resource(string path, bool isDontUnload)
    {
        Path = path;

        IsDontUnload = isDontUnload;

        mState = STATE.UNLOAD;
    }

    /// <summary>
    /// 资源载入标识
    /// </summary>
    public bool IsDone()
    {
        return mState == STATE.LOADED || mState == STATE.LOAD_FAILED;
    }

    // 更新访问时间
    public void UpdateAccessTime()
    {
        mLastAccessTime = Time.time;
    }

    /// <summary>
    /// 卸载资源
    /// </summary>
    public void Unload(bool unloadMainAsset = false)
    {
        // 资源不允许被卸载
        if (IsDontUnload)
            return;

        // 如果资源没有载入不处理
        if (mState != STATE.LOADED)
            return;

        // 指明卸载主资源
        if (unloadMainAsset)
            MainAsset = null;

        // 如果主资源为空，设置为卸载状态
        if (MainAsset == null)
            mState = STATE.UNLOAD;

        // 删除AssetBundle资源引用
        foreach (string ab in AssetBundles)
        {
            // 如果AB2ResourceMap中不需要处理
            if (!ResourceMgr.Instance.AB2ResourceMap.ContainsKey(ab))
                continue;

            // 删除资源引用关系
            ResourceMgr.Instance.AB2ResourceMap[ab].Remove(this);

            // 如果AssetBundle已经不存在引用了，直接卸载AssetBundle
            // 否则不能卸载ab资源
            if (ResourceMgr.Instance.AB2ResourceMap[ab].Count != 0)
                continue;

            // 删除引用关系
            ResourceMgr.Instance.AB2ResourceMap.Remove(ab);

            // 卸载资源
            ResourceMgr.Instance.AssetBundleMap[ab].Unload(false);
        }

        // 清空AssetBundles
        AssetBundles.Clear();
    }

    // 加载成功了
    public void LoadSuccessed()
    {
        // 标识资源载入成功
        mState = STATE.LOADED;

        // 设置访问时间
        mLastAccessTime = Time.time;
    }

    // 加载失败了
    public void LoadFaild()
    {
        // 卸载资源
        Unload();

        // 标识资源载入失败
        mState = STATE.LOAD_FAILED;
    }
}
