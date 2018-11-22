using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

public class AssetBundleEditor : EditorWindow
{
    public static string MAIN_ASSET_BUNDLE_NAME = "AssetBundles";

    [MenuItem("Tools/AssetBundle")]
    private static void CreateWindow()
    {
        AssetBundleEditor wnd = EditorWindow.GetWindow<AssetBundleEditor>(true);
        wnd.minSize = new Vector2(500, 780);
        wnd.Show();
    }

    private Vector2 _scrollPos;
    private void OnGUI()
    {
        //内容
        GUI.backgroundColor = new Color32(150, 200, 255, 255);
        GUILayout.BeginVertical("AS TextArea", GUILayout.Height(500));
        {
            if (GUILayout.Button("AssetBundle打包"))
            {
                BuildABRes();
            }
        }
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 打包AB资源
    /// </summary>
    /// <param name="target">Target.</param>
    private static void BuildABRes()
    {
        if (!AssetBundleNameTools.SetABName())
        {
            NIDebug.LogError("设置ABName失败，打包失败");
            return;
        }

        // 资源版本直接取当前系统时间
        System.DateTime currentTime = System.DateTime.Now;

        string res_version = string.Format("1.{0}{1:D2}{2:D2}.{3:D2}{4:D2}", currentTime.Year.ToString().Substring(currentTime.Year.ToString().Length - 2, 2),
            currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute);

        // 生成AssetBundles
        DoBuildPublish(BuildTarget.StandaloneWindows64, res_version, true, "test");

        // 重新清空所有的abName
        AssetBundleNameTools.ClearAllABName();

        // 刷新编辑器
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 发布资源
    /// </summary>
    private static void DoBuildPublish(BuildTarget target, string resVersion, bool isNewAssets, string channel)
    {
        string rootPath = GetABRootPath(target, channel);
        string publishPath = string.Format("{0}/PublishAssetBundles/", rootPath);
        string abPath = string.Format("{0}/AssetBundles/", rootPath);
        string patchPath = string.Format("{0}/PatchAssetBundles/", rootPath);
        string encryptPath = string.Format("{0}/EncryptAssetBundles/", rootPath);

        // 如果需要重新打包资源则删除原来的版本文件
        if (isNewAssets)
            FileMgr.DeleteDirectory(abPath);

        // ab路径是否存在，不存在则创建一个
        DirectoryInfo abInfo = new DirectoryInfo(abPath);
        if (!abInfo.Exists)
            abInfo.Create();

        // 确保目录存在
        FileMgr.DeleteDirectory(encryptPath);
        FileMgr.CreateDirectory(encryptPath);
        FileMgr.DeleteDirectory(publishPath);
        FileMgr.CreateDirectory(publishPath);
        FileMgr.DeleteDirectory(patchPath);
        FileMgr.CreateDirectory(patchPath);

        BuildAssetBundleOptions opt =
            BuildAssetBundleOptions.DeterministicAssetBundle |      // 保证同样的资源多次打包出的assetbundle相同
            BuildAssetBundleOptions.UncompressedAssetBundle;        // 不压缩资源，我们会压缩，然后在目标平台解压缩，最后使用的是没有压缩的资源，保证资源快速加载

        // AssetBundle打包，Unity5.x之后都是用这个方法打包，而且还会自动生成依赖关系
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(abPath, opt, target);

        // 载入旧版本树
        CsvFile oldVersionCsv = LoadVersion(abPath);

        // 生成资源映射表
        GenResourceDictFile(abPath, manifest);

        //// 加密ab资源包
        DoEncryptAssetBundle(encryptPath, abPath, manifest);

        // 收集需要发布的ab资源包
        Dictionary<string, string> newABVersion = DoCollectNewABVersion(encryptPath, manifest);

        // 资源大小映射表
        Dictionary<string, List<object>> versionItemInfo = new Dictionary<string, List<object>>();

        // 是否需要更新标识
        bool isABChanged = false;

        byte[] unzipBuf;
        byte[] buf;

        // 发布资源
        foreach (string abName in newABVersion.Keys)
        {
            // 获取旧版本信息
            CsvRow data = oldVersionCsv.FindByKey(abName);

            if ((data != null) && string.Equals(data.Query<string>("md5"), newABVersion[abName]))
            {
                // 添加到列表中
                versionItemInfo.Add(abName, new List<object>() { newABVersion[abName], data.Query<int>("unzip_size"), data.Query<int>("zip_size") });
                continue;
            }

            // 标识数据已经变化了
            isABChanged = true;

            // 取得未压缩大小
            unzipBuf = File.ReadAllBytes(string.Format("{0}{1}", encryptPath, abName));
            if (unzipBuf.Length == 0)
            {
                NIDebug.Log(string.Format("读取{0}{1}失败!", encryptPath, abName));
                continue;
            }

            NIEditorUtility.Zip(new string[] { string.Format("{0}{1}", encryptPath, abName) }, string.Format("{0}{1}_{2}.zip", publishPath, abName, newABVersion[abName]));

            // 读取文件
            buf = File.ReadAllBytes(string.Format("{0}{1}_{2}.zip", publishPath, abName, newABVersion[abName]));
            if (buf.Length == 0)
            {
                NIDebug.Log(string.Format("读取{0}{1}失败!", publishPath, abName));
                continue;
            }

            // 添加到列表中
            versionItemInfo.Add(abName, new List<object>() { newABVersion[abName], unzipBuf.Length, buf.Length });
        }

        // 如果ab资源没有变化, 不处理
        if (isABChanged)
        {
            string resourceDictPath = string.Format("{0}{1}", encryptPath, "resource_dict.bytes");

            // 发布资源到发布资源目录
            string md5 = NIEditorUtility.GetFileMD5(resourceDictPath);

            // 取得未压缩大小
            unzipBuf = File.ReadAllBytes(string.Format("{0}{1}", encryptPath, "resource_dict.bytes"));
            if (unzipBuf.Length == 0)
            {
                NIDebug.Log(string.Format("读取{0}{1}失败!", encryptPath, "resource_dict.bytes"));
                return;
            }

            NIEditorUtility.Zip(new string[] { string.Format("{0}{1}", encryptPath, "resource_dict.bytes") }, string.Format("{0}{1}_{2}.zip", publishPath, "resource_dict.bytes", md5));

            // 读取文件
            buf = File.ReadAllBytes(string.Format("{0}{1}_{2}.zip", publishPath, "resource_dict.bytes", md5));
            if (buf.Length == 0)
            {
                NIDebug.Log(string.Format("读取{0}{1}失败!", publishPath, "resource_dict.bytes"));
                return;
            }

            // 添加到列表中
            versionItemInfo.Add("resource_dict.bytes", new List<object>() { md5, unzipBuf.Length, buf.Length });
        }

        bool isPatchChanged = false;

        Dictionary<string, string> abPatchDic = new Dictionary<string, string>();

        // 生成压缩patch文件
        Dictionary<string, string> newPatchVersion = GenPatchFiles(patchPath, encryptPath, versionItemInfo, ref abPatchDic);

        // 发布资源
        foreach (string patchName in newPatchVersion.Keys)
        {
            // 获取旧版本信息
            CsvRow data = oldVersionCsv.FindByKey(patchName);

            if ((data != null) && string.Equals(data.Query<string>("md5"), newPatchVersion[patchName]))
            {
                versionItemInfo.Add(patchName, new List<object>() { data.Query<string>("md5"), 0, data.Query<int>("zip_size") });
                continue;
            }

            // 标识数据已经变化了
            isPatchChanged = true;

            string filePath = string.Format("{0}{1}_{2}.zip", patchPath, patchName, newPatchVersion[patchName]);

            // 读取文件
            buf = File.ReadAllBytes(filePath);
            if (buf.Length == 0)
            {
                NIDebug.LogError(string.Format("读取{0}失败!", filePath));
                continue;
            }

            versionItemInfo.Add(patchName, new List<object>() { newPatchVersion[patchName], 0, buf.Length });

            File.Copy(filePath, string.Format("{0}{1}_{2}.zip", publishPath, patchName, newPatchVersion[patchName]), true);
        }

        // 有变化重新生成版本文件
        if (isABChanged || isPatchChanged)
        {
            // 将资源版本添加到列表中
            versionItemInfo.Add("res_version", new List<object>() { resVersion, 0, 0 });

            // 生成某个目录下的版本控制文件
            GenVersionFile(abPath, publishPath, versionItemInfo, abPatchDic);
        }

        // 标识Build AssetBundle OK
        NIDebug.Log("生成AssetBundle完成");

        // 刷新编辑器
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 获取AB资源根目录
    /// </summary>
    /// <param name="target">Target.</param>
    /// <param name="channel">Channel.</param>
    public static string GetABRootPath(BuildTarget target, string channel)
    {
        string rootPath = string.Empty;

        // 获取发布路径
        switch (target)
        {
            case BuildTarget.Android:
                rootPath = string.Format("Publish/Android/{0}", channel);

                break;

            case BuildTarget.iOS:
                rootPath = string.Format("Publish/iOS/{0}", channel);
                break;

            default:
                rootPath = string.Format("Publish/Window/{0}", channel);
                break;
        }

        return rootPath;
    }

    /// <summary>
    /// 载入版本文件
    /// </summary>
    private static CsvFile LoadVersion(string path)
    {
        string versionPath = string.Format("{0}version_tree.bytes", path);

        // 如果文件不存在不处理
        if (!File.Exists(versionPath))
            return new CsvFile("version_tree");

        // 获取byte信息
        byte[] csvBytes = File.ReadAllBytes(versionPath);

        // 反序列化
        MemoryStream csvStr = new MemoryStream(csvBytes, 0, csvBytes.Length, true, true);
        CsvFile csv = CsvFileMgr.Deserialize(csvStr);
        csvStr.Close();

        // 返回配置信息
        return csv;
    }

    /// <summary>
    /// 生成资源-->AssetBundle映射表
    /// </summary>
    private static void GenResourceDictFile(string sourcePath, AssetBundleManifest manifest)
    {
        // 保存的路径是否存在，不存在则创建一个
        // csv保存的地址
        string dirpath = string.Format("{0}resource_dict.csv", sourcePath);
        FileInfo fi = new FileInfo(dirpath);
        if (!fi.Directory.Exists)
            fi.Directory.Create();

        // csv文件写入
        FileStream fs = new FileStream(dirpath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
        StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

        // 创建csv文件前面几行
        sw.WriteLine(WriteRow(new string[] { "# resource_dict.csv", "" }));
        sw.WriteLine(WriteRow(new string[] { "# 资源目录", "" }));
        sw.WriteLine(WriteRow(new string[] { "# 资源路径", "bundle名" }));
        sw.WriteLine(WriteRow(new string[] { "string", "string" }));
        sw.WriteLine(WriteRow(new string[] { "path", "bundle" }));

        // 遍历该文件下的所有AssetBundles
        foreach (string abName in manifest.GetAllAssetBundles())
        {
            // 载入assetBundle
            string abPath = string.Format("{0}{1}", sourcePath, abName);
            AssetBundle assetBundle = AssetBundle.LoadFromFile(abPath);

            // 遍历该assetBundle下所有资源
            foreach (string bundle in assetBundle.GetAllAssetNames())
            {
                // 根据路径载入资源
                UnityEngine.Object ob = AssetDatabase.LoadMainAssetAtPath(bundle);
                string path = AssetDatabase.GetAssetPath(ob);

                // 写入数据
                sw.WriteLine(WriteRow(new string[] { path, abName }));
            }

            // 释放资源
            assetBundle.Unload(true);
        }

        //关闭写入
        sw.Close();
        fs.Close();

        // 讲csv文件转成byte文件
        CsvFileMgr.Save(dirpath, false, sourcePath);
    }

    /// <summary>
    /// 加密ab资源
    /// </summary>
    /// <param name="abPath">Ab path.</param>
    /// <param name="manifest">Manifest.</param>
    private static void DoEncryptAssetBundle(string encryptPath, string abPath, AssetBundleManifest manifest)
    {
        // CopyDirectory
        FileMgr.CopyDirectory(abPath, encryptPath);

        // 遍历该文件下的所有AssetBundles
        foreach (string abName in manifest.GetAllAssetBundles())
        {
            // 目前只是etc需要加密
            if (!string.Equals(abName, "etc"))
                continue;

            // 读取文件
            byte[] abBytes = File.ReadAllBytes(string.Format("{0}/{1}", abPath, abName));

            // 加密资源
            byte[] encyptBytes = ResourceMgr.Instance.Encypt(abBytes);

            // 重新输入文件
            File.WriteAllBytes(string.Format("{0}/{1}", encryptPath, abName), encyptBytes);
        }
    }

    /// <summary>
    /// 解析字符串数组
    /// </summary>
    /// <returns>The row.</returns>
    /// <param name="strs">Strs.</param>
    private static string WriteRow(string[] strs)
    {
        string data = "";
        for (int i = 0; i < strs.Length; i++)
        {
            if (!string.IsNullOrEmpty(strs[i]))
            {
                data += "\"" + strs[i] + "\"";
            }
            if (i < strs.Length - 1)
            {
                data += ",";
            }
        }
        return data;
    }

    /// <summary>
    /// 收集新资源版本信息
    /// </summary>
    private static Dictionary<string, string> DoCollectNewABVersion(string abPath, AssetBundleManifest manifest)
    {
        // 版本信息
        Dictionary<string, string> versionDict = new Dictionary<string, string>();

        // 收集新变化的ab
        // 将主AssetBundle也加入到version中
        string md5 = NIEditorUtility.GetFileMD5(string.Format("{0}/{1}", abPath, MAIN_ASSET_BUNDLE_NAME));
        versionDict.Add(MAIN_ASSET_BUNDLE_NAME, md5);

        // 遍历该文件下的所有AssetBundles
        foreach (string abName in manifest.GetAllAssetBundles())
        {
            md5 = NIEditorUtility.GetFileMD5(string.Format("{0}/{1}", abPath, abName));

            // 记录版本信息
            versionDict.Add(abName, md5);
        }

        // 返回新生成的版本信息
        return versionDict;
    }

    /// <summary>
    /// Gens the patch files.
    /// </summary>
    /// <returns>The patch files.</returns>
    /// <param name="patchPatch">Patch patch.</param>
    /// <param name="versionItemSize">Version item size.</param>
    /// <param name="abPatchDic">Ab patch dic.</param>
    private static Dictionary<string, string> GenPatchFiles(string patchPath, string abPath,
        Dictionary<string, List<object>> versionItemInfo, ref Dictionary<string, string> abPatchDic)
    {
        Dictionary<string, string> patchVersionDic = new Dictionary<string, string>();

        List<string> filterAB = new List<string>();

        foreach (KeyValuePair<string, List<string>> item in AssetBundleNameTools.patchAbDict)
        {
            filterAB.Clear();

            string md5Str = string.Empty;

            foreach (string reg in item.Value)
            {
                List<string> bundles = versionItemInfo.Keys.Where(bundle => (Regex.IsMatch(bundle, reg))).ToList<string>();

                if (bundles.Count == 0)
                {
                    NIDebug.LogError("生成patch时 {0} 筛选出来的文件个数为0，请检查！", reg);
                    continue;
                }

                // 对文件按文件名称进行排序
                bundles.Sort(delegate (string b1, string b2)
                {
                    return b1.CompareTo(b2);
                });

                foreach (string bundle in bundles)
                {
                    filterAB.Add(string.Format("{0}{1}", abPath, bundle));

                    md5Str += versionItemInfo[bundle][0];

                    if (abPatchDic.ContainsKey(bundle))
                    {
                        NIDebug.LogError(string.Format("{0}重复放在了{1}和{2}中，请检查!", bundle, abPatchDic[bundle], item.Key));
                        continue;
                    }

                    abPatchDic.Add(bundle, item.Key);
                }
            }

            string md5 = NIEditorUtility.GetStrMD5(md5Str);

            NIEditorUtility.Zip(filterAB.ToArray(), string.Format("{0}{1}_{2}.zip", patchPath, item.Key, md5));

            patchVersionDic.Add(item.Key, md5);
        }

        return patchVersionDic;
    }

    /// <summary>
    /// 生成某个目录下的版本控制文件
    /// </summary>
    private static void GenVersionFile(string sourcePath, string publishPath, Dictionary<string, List<object>> assetBundleInfo, Dictionary<string, string> abPatchDic)
    {
        // 保存的路径是否存在，不存在则创建一个
        // csv保存的地址
        string dirpath = string.Format("{0}version_tree.csv", sourcePath);
        FileInfo fi = new FileInfo(dirpath);
        if (!fi.Directory.Exists)
            fi.Directory.Create();

        // csv文件写入
        FileStream fs = new FileStream(dirpath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
        StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

        //创建csv文件前面几行
        sw.WriteLine(WriteRow(new string[] { "# version_tree.csv", "", "", "", "" }));
        sw.WriteLine(WriteRow(new string[] { "# 版本树", "", "", "", "" }));
        sw.WriteLine(WriteRow(new string[] { "# assetbundle名", "MD5编号", "所在patch", "未压缩资源大小", "压缩资源大小" }));
        sw.WriteLine(WriteRow(new string[] { "string", "string", "string", "int", "int" }));
        sw.WriteLine(WriteRow(new string[] { "bundle", "md5", "patch", "unzip_size", "zip_size" }));

        // 遍历该文件下的所有AssetBundles
        foreach (string abName in assetBundleInfo.Keys)
        {
            List<object> info = assetBundleInfo[abName];

            string patch = abPatchDic.ContainsKey(abName) ? abPatchDic[abName] : "0";

            // 写入数据
            sw.WriteLine(WriteRow(new string[] { abName, info[0].ToString(), patch, info[1].ToString(), info[2].ToString() }));
        }

        //关闭写入
        sw.Close();
        fs.Close();

        // 讲csv文件转成byte文件
        CsvFileMgr.Save(dirpath, false, sourcePath);

        // 讲csv文件转成byte文件
        CsvFileMgr.Save(dirpath, false, publishPath);
    }

}
