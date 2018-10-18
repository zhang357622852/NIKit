/// <summary>
/// BanWordMgr.cs
/// created by zhaozy 2015/09/07
/// 屏蔽词管理模块
/// </summary>

using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.IO;
using LPC;

/// <summary>
/// 屏蔽词处理
/// </summary>

public class BanWordMgr
{
    #region 变量申明

    private static string[] ignoreChars = new string[] { " ", "*" };

    // 屏蔽词
    private static Dictionary<string, List<string>> mBanWords = new Dictionary<string, List<string>>();

    #endregion

    #region 私有变量

    /// <summary>
    /// 载入屏蔽词
    /// </summary>
    private static void DoLoadBanWord()
    {
        mBanWords.Clear();

        // 屏蔽词列表
        List<string> banWords = new List<string>();
        string wordsGroup = string.Empty;
        string word = string.Empty;

        // 载入简易屏蔽词\
        string[] lines = Game.Explode(ResourceMgr.LoadText(ConfigMgr.ETC_PATH + "/ban_words.txt"), "\n");
        foreach (string line in lines)
        {
            // 去掉空格信息
            word = line.Trim();

            // 空字符串不需要处理
            if (word.Length <= 0)
                continue;

            // 判断是否是"[段名]"
            if (word [0] == '[' && word [word.Length - 1] == ']')
            {
                banWords = new List<string>();
                wordsGroup = word.Substring(1, word.Length - 2);

                if (! string.IsNullOrEmpty(wordsGroup))
                    mBanWords.Add(wordsGroup, banWords);

                continue;
            }

            // 已经在列表中
            if (banWords.Contains(word))
                continue;

            // 添加列表中
            banWords.Add(word);
        }
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        // 载入屏蔽词
        DoLoadBanWord();
    }

    // 将一句话中的屏蔽词过滤（替换为 **）
    public static string CleanBySingleBanWord(string rawWord, string ban)
    {
        string ret = rawWord;
        StringBuilder patt = new StringBuilder();
        char[] chars = ban.ToCharArray();
        foreach (char c in chars)
        {
            patt.Append(c);
            patt.Append("[ \\*]*");
        }

        string spatt = patt.ToString();
        Match m = Regex.Match(ret, spatt);

        while (m.Success)
        {
            if (m.Value.Length > 0)
                ret = ret.Replace(m.Value, "**");
            m = m.NextMatch();
        }

        return ret;
    }

    /// <summary>
    /// 获取指定分组的屏蔽词
    /// </summary>
    public static List<string> GetBanWords(string wordsGroup)
    {
        if (! mBanWords.ContainsKey(wordsGroup))
            return new List<string>();

        // 返回数据
        return mBanWords[wordsGroup];
    }

    /// <summary>
    /// 顾虑屏蔽词
    /// </summary>
    public static string GenHarmoniousWord(string rawWord)
    {
        string word = rawWord;
        string ret = rawWord;

        // 替换掉无意义符号
        foreach (string ignore in ignoreChars)
            word = word.Replace(ignore, "");

        // 获取屏蔽词列表
        List<string> banWords = GetBanWords("sensitive_words");

        // 针对过滤列表中的每个单词，进行处理
        for (int i = 0; i < banWords.Count; i++)
        {
            // 不包含这个屏蔽词
            if (! word.Contains(banWords [i]))
                continue;

            // 包含了，尝试处理掉,
            ret = CleanBySingleBanWord(ret, banWords [i]);
        }

        // 返回最终的信息
        return ret;
    }

    /// <summary>
    /// 是否包含屏蔽词
    /// </summary>
    public static bool ContainsBanWords(string content)
    {
        string word = content;

        // 替换掉无意义符号
        foreach (string ignore in ignoreChars)
            word = word.Replace(ignore, "");

        // 获取屏蔽词列表
        List<string> banWords = GetBanWords("sensitive_words");

        // 针对过滤列表中的每个单词，进行处理
        for (int i = 0; i < banWords.Count; i++)
        {
            // 不包含这个屏蔽词
            if (! word.Contains(banWords [i]))
                continue;

            return true;
        }

        return false;
    }

#if UNITY_EDITOR

    // 更新配置文件
    public static void DoSyncBanWord()
    {
        string word = string.Empty;

        // 首先保证根目录存在
        Directory.CreateDirectory(ConfigMgr.ETC_PATH);

        // 收集文件内容
        string ban_words = string.Empty;
        string filePath = Application.dataPath + "/../../server/server_scripts/etc/gs/ban_words.txt";
        foreach (string line in FileMgr.ReadLines(filePath))
        {
            word = line.Trim();

            // 空字符串不处理
            if (word.Length < 1)
                continue;

            // 添加到字符串
            ban_words += word + "\n";
        }

        // 写文件
        var fs = new FileStream(ConfigMgr.ETC_PATH + "/ban_words.txt", FileMode.Create, FileAccess.Write);
        var data = Encoding.UTF8.GetBytes(ban_words);
        fs.Write(data, 0, data.Length);
        fs.Close();
    }

#endif

    #endregion
}
