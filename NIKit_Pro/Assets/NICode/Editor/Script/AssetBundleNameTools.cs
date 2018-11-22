/// <summary>
/// AssetBundleNameTools.cs
/// Created by lic 2017/12/22
/// AssetBundleNameTools
/// </summary>

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;

public class AssetBundleNameTools
{
    /// <summary>
    /// AB(最后一层文件名使用正则表达式匹配)
    /// </summary>
    public static Dictionary<string, string> aBPathDict = new Dictionary<string, string>()
    {
        // UITexurte
        {"Assets/Art/UI/Forms/Textures$", "forms_common_textures"},

        // font
        {"Assets/Art/Font/.+\\.prefab$", "font"},

        // atlas(相关的材质球和图集会被一起打到这个AssetBundle里)
        {"Assets/Art/UI/Forms/.+\\.prefab$", "forms_atlas"},

        // prfab窗口预制体
        {"Assets/Prefabs/UI/Forms/.+\\.prefab$", "forms_prefab"},
    };

    // AB黑名单
    public static Dictionary<string, string> blackABDict = new Dictionary<string, string>()
    {
    };

    public static Dictionary<string, List<string>> patchAbDict = new Dictionary<string, List<string>>()
    {
        {"patch_1", new List<string>{"^icon_.+$", "^curve$", "^etc$", "^resource_dict\\.bytes$", "^AssetBundles$" }},
        {"patch_2", new List<string>{"^window_.+$", "^font$", "^atlas_.+$", "^shader$"}},
        {"patch_3", new List<string>{"^sound_.+$"}},  // 音效
        {"patch_4", new List<string>{"^model_.+$"}},  // 模型
        {"patch_5", new List<string>{"^effect_.+$"}}, // 包含光效预置了光效资源
        {"patch_6", new List<string>{"^map_.+$"}},  // 包含地图光效和地图资源
    };

    /// <summary>
    /// 清除所有的AssetBundleName
    /// </summary>
    public static void ClearAllABName()
    {
        string[] allNames = AssetDatabase.GetAllAssetBundleNames();

        for (int j = 0; j < allNames.Length; j++)
            AssetDatabase.RemoveAssetBundleName(allNames[j], true);

        NIDebug.Log("清除完成");
    }

    public static void GenerSceneRes()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);

        string[] depends = AssetDatabase.GetDependencies(path, false);

        foreach (string depend in depends)
        {
            if(depend.EndsWith(".cs"))
                continue;

            NIDebug.Log(depend);
        }

        NIDebug.Log("查找{0}的依赖完成", path);
    }

    /// <summary>
    /// 打包AB资源
    /// </summary>
    /// <returns><c>true</c>, if A was built, <c>false</c> otherwise.</returns>
    public static bool SetABName()
    {
        Dictionary<string, string> abNameDict = GetFilterDirectories();

        if(abNameDict == null)
            return false;

        // 清除本地所有的ab
        ClearAllABName();

        // 设置单个资源的ABName
        foreach (KeyValuePair<string, string> item in abNameDict)
        {
            // 黑名单中的不设置
            if(blackABDict.ContainsKey(item.Key))
                continue;

            if(!SetABName(item.Key, item.Value))
                return false;
        }

        NIDebug.Log("设置AssetBundle Name完成");

        // 刷新编辑器
        AssetDatabase.Refresh();

        return true;
    }

    /// <summary>
    /// 收集AB字典
    /// </summary>
    /// <returns><c>true</c>, if filter directories was gotten, <c>false</c> otherwise.</returns>
    private static Dictionary<string, string> GetFilterDirectories()
    {
        Dictionary<string, string> mFilterDic = new Dictionary<string, string>();

        foreach (KeyValuePair<string, string> kv in aBPathDict)
        {
            string dic = kv.Key.Substring(0, kv.Key.LastIndexOf("/"));

            string filter = kv.Key.Substring(kv.Key.LastIndexOf("/") + 1);

            DirectoryInfo di = new DirectoryInfo(dic);

            List<FileSystemInfo> fss = di.GetFileSystemInfos().Where(fs => (Regex.IsMatch(fs.Name, filter))).ToList<FileSystemInfo>();

            foreach (FileSystemInfo fs in fss)
            {
                if(fs.Name.EndsWith(".meta"))
                    continue;

                Match match = Regex.Match(fs.Name, filter);

                string matchName = string.Format(kv.Value, match.Groups[1].Value);

                string path = string.Format("{0}/{1}", dic, fs.Name);

                if(mFilterDic.ContainsKey(path))
                {
                    NIDebug.LogError("文件{0}设置ABName:{2}失败,已经设置了ABName:{1}", path, matchName, mFilterDic[path]);
                    return null;
                }

                mFilterDic.Add(path, matchName);
            }
        }

        return mFilterDic;
    }

    /// <summary>
    /// 设置ABname
    /// </summary>
    /// <returns><c>true</c>, if res AB name was set, <c>false</c> otherwise.</returns>
    /// <param name="filePath">File path.</param>
    /// <param name="abName">Ab name.</param>
    private static bool SetABName(string filePath, string abName)
    {
        // 获取指定路径的资源
        AssetImporter impoter = AssetImporter.GetAtPath(filePath);

        if(!impoter)
        {
            NIDebug.Log("文件{0}获取impoter失败，设置ABName失败。", filePath);
            return false;
        }

        impoter.assetBundleName = abName;

        // 暂时不需要设置assetBundleVariant

        return true;
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
}
