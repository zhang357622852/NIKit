/// <summary>
/// LocalizationMgr.cs
/// Created by fengsc 2017/03/01
/// 本地化管理类
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public static class LocalizationMgr
{
    // <language, <key, text>>
    private static Dictionary<string, Dictionary<string, string>> mTextDictionary = new Dictionary<string, Dictionary<string, string>>();

    // 当前语言本地化文本名称
    private static string mLanguage = string.Empty;

    #region 属性

    public static string Language
    {
        get
        {
            return mLanguage;
        }
        set
        {
            if (mLanguage != value)
            {
                if (LoadText(value))
                    mLanguage = value;
            }
        }
    }

    #endregion

    #region 内部函数

    /// <summary>
    /// 解析数组中的文本信息
    /// </summary>
    private static Dictionary<string, string> Analize(string[] lines)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        char[] separator = new char[] { '=' };

        // 缓存的值
        string cacheVal = string.Empty;

        // 缓存的键
        string cacheKey = string.Empty;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrEmpty(line))
                continue;

            // 过滤掉注释行
            if (line.StartsWith("//"))
            {
                continue;
            }
            // 不需要分割
            else if (line.StartsWith("#"))
            {
                // 累计字符串
                cacheVal += (line.Substring(1, line.Length - 1) + "<br>");

                continue;
            }
            else if (line.Trim().StartsWith("])"))
            {
                dict[cacheKey] = cacheVal.Substring(0, cacheVal.Length - 4);

                continue;
            }
            else
            {
            }

            // 按照固定格式解析文本
            string[] split = line.Split(separator, 2, System.StringSplitOptions.RemoveEmptyEntries);

            // 格式不不正确
            if (split == null || split.Length == 0)
                continue;

            // 获取key
            string key = split[0].Trim();

            // 字典中已经包含该键值
            if (dict.ContainsKey(key))
            {
                LogMgr.Trace("{0}:字典中已经包含该键值", key);
                continue;
            }

            // 没有详细描述信息
            if (split.Length != 2)
            {
                dict[key] = string.Empty;
                continue;
            }

            string val = split[1].Replace("\\n", "\n");
            if (val.Equals("(["))
            {
                cacheVal = string.Empty;

                cacheKey = string.Empty;

                cacheKey = key;

                continue;
            }

            dict[key] = val;
        }

        return dict;
    }

    /// <summary>
    /// 加载本地化文本
    /// </summary>
    private static bool LoadText(string textName)
    {
        if (string.IsNullOrEmpty(textName))
        {
            LogMgr.Trace("需要加载的文本名称为空");
            return false;
        }

        // 缓存中存在该文本数据
        if (mTextDictionary.ContainsKey(textName))
            return false;

        string resPath = string.Format("{0}/{1}.txt", ConfigMgr.ETC_PATH, textName);
        string text = ResourceMgr.LoadText(resPath);
        if (string.IsNullOrEmpty(text))
        {
            LogMgr.Trace("{0}加载失败", textName);
            return false;
        }

        // 切割资源
        string[] lines = Game.Explode(text, "\n");
        if (lines == null)
            return false;

        // 解析
        mTextDictionary.Add(textName, Analize(lines));
        return true;
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 初始化语言
    /// </summary>
    public static void Init()
    {
        // 移除数据
        RemoveLanguage(LocalizationConst.ZH_CN);

        // 加载本地化文本
        Language = LocalizationConst.ZH_CN;
    }

    /// <summary>
    /// 初始化开始场景语言
    /// </summary>
    public static void InitStartRes()
    {
        // 移除数据
        RemoveLanguage(LocalizationConst.START);

        Language = LocalizationConst.START;
    }

    /// <summary>
    /// 获取本地化的文本
    /// </summary>
    public static string Get (string key, string language = "")
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        if (mTextDictionary == null)
            return string.Empty;

        if (string.IsNullOrEmpty(language))
            language = mLanguage;

        if (string.IsNullOrEmpty(language))
            return string.Empty;

        Dictionary<string, string> fileTextDic;

        // 没有该类型language数据
        if (! mTextDictionary.TryGetValue(language, out fileTextDic))
        {
            LogMgr.Trace("字典中不包含{0}.txt的缓存数据", language);
            return string.Empty;
        }

        string text;

        // 字典中不包含该字段的本地化资源
        if (! fileTextDic.TryGetValue(key, out text))
            return key;

        // 返回本地配置资源
        return text;
    }

    /// <summary>
    /// 移除本地化文本
    /// </summary>
    public static void RemoveLanguage(string languageName)
    {
        if (string.IsNullOrEmpty(languageName))
            return;

        if (!mTextDictionary.ContainsKey(languageName))
            return;

        if (languageName.Equals(mLanguage))
            mLanguage = string.Empty;

        mTextDictionary.Remove(languageName);
    }

    /// <summary>
    /// 服务器消息提示获取本地化文本
    /// </summary>
    public static string GetServerDesc(LPCValue msg)
    {
        Dictionary<string, string> dic = new Dictionary<string, string>();
        if (mTextDictionary.ContainsKey(mLanguage))
            dic = mTextDictionary[mLanguage];

        if (msg == null)
            return string.Empty;

        // 直接输入的是文本编号
        if(msg.IsInt)
            return Get(msg.AsString);

        // 如果直接直接是输入文本编号或者文本信息
        if (msg.IsString)
        {
            if (!dic.ContainsKey(msg.AsString))
                return msg.AsString;
            else
                return Get(msg.AsString);
        }

        // 如果是array格式，需要详细处理
        if (msg.IsArray)
        {
            LPCArray array = msg.AsArray;

            string localText = string.Empty;

            // 获取第一参数，根据不同的类型做不同的解析
            if (array[0].IsString)
            {
                string textId = array[0].AsString;

                // 如果是脚本本地解析SCRIPT_
                if (textId.Contains("SCRIPT_"))
                {
                    int scriptNo;

                    // 脚本解析异常
                    if (!int.TryParse(textId.Replace("SCRIPT_", ""), out scriptNo))
                    {
                        LogMgr.Trace("脚本名字 {0} 非法，必须类似 SCRIPT_1234", textId);
                        return string.Empty;
                    }

                    // 调用脚本构建消息
                    return (string) ScriptMgr.Call(scriptNo, array);
                }

                // 其他兼容方式
                if (!dic.ContainsKey(textId))
                    localText = textId;
                else
                    localText = Get(textId);
            }
            else if (array[0].IsInt)
            {
                localText = Get(array[0].AsString);
            }
            else
            {
                localText = GetServerDesc(array[0]);
            }

            // 如果只是一个参数
            if (array.Count == 1)
                return localText;

            string[] value = new string[array.Count - 1];

            for (int i = 1; i < array.Count; i++)
            {
                if (array[i].IsArray)
                {
                    value[i-1] = GetServerDesc(array[i]);
                    continue;
                }

                if (array[i].IsString)
                {
                    value[i-1] = Get(array[i].AsString);
                    continue;
                }

                // 替他格式直接作为string拼接
                value[i-1] = array[i].AsString;
            }

            // 组装字符串
            return string.Format(localText, value);
        }
        else
        {
            return string.Empty;
        }
    }

    #endregion
}
