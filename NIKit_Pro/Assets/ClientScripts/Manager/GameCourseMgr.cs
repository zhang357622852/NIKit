/// <summary>
/// GameCourseMgr.cs
/// Created by fengsc 2018/08/16
/// 游戏历程管理模块
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class GameCourseMgr
{
    // 配置表信息
    private static CsvFile mGameCourseCsv;

    private static Dictionary<int, Dictionary<int, List<CsvRow>>> mPageDic = new Dictionary<int, Dictionary<int, List<CsvRow>>>();

    private static Dictionary<int, List<CsvRow>> mPageListDic = new Dictionary<int, List<CsvRow>>();

    // 配置表信息
    public static CsvFile GameCourseCsv { get { return mGameCourseCsv; } }

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 清空数据
        mPageDic.Clear();
        mPageListDic.Clear();

        // 载入配置表信息
        mGameCourseCsv = CsvFileMgr.Load("game_course");

        // 遍历每行的配置数据
        foreach (CsvRow row in mGameCourseCsv.rows)
        {
            if (row == null)
                continue;

            // 获取分组
            int group = row.Query<int>("group");

            // 分页
            int page = row.Query<int>("page");

            Dictionary<int, List<CsvRow>> dic = null;

            List<CsvRow> list = null;

            if (!mPageDic.TryGetValue(page, out dic))
                dic = new Dictionary<int, List<CsvRow>>();

            if (!dic.TryGetValue(group, out list))
                list = new List<CsvRow>();

            // 将数据缓存至列表
            list.Add(row);

            dic[group] = list;

            mPageDic[page] = dic;

            List<CsvRow> pages = null;

            if (!mPageListDic.TryGetValue(page, out pages))
                pages = new List<CsvRow>();

            pages.Add(row);

            // 缓存数据
            mPageListDic[page] = pages;
        }
    }

    /// <summary>
    /// 获取游戏历程配置数据
    /// </summary>
    public static CsvRow GetConfigCourse(int courseId)
    {
        if (mGameCourseCsv == null)
            return null;

        return mGameCourseCsv.FindByKey(courseId);
    }

    /// <summary>
    /// 获取分页名称
    /// </summary>
    public static string GetPageName(Property who, int page)
    {
        // 玩家对象不存在
        if (who == null)
            return string.Empty;

        List<CsvRow> list = null;

        if (!mPageListDic.TryGetValue(page, out list))
            return string.Empty;

        string page_name = list[0].Query<string>("page_name");

        // 分页名称脚本
        LPCValue scriptV = list[0].Query<LPCValue>("page_name_script");
        if (scriptV == null || scriptV.AsInt == 0)
            return LocalizationMgr.Get(page_name);

        // 调用脚本获取描述
        return ScriptMgr.Call(scriptV.AsInt, who, page_name) as string;
    }

    /// <summary>
    /// 获取分页标题
    /// </summary>
    public static string GetPageTitle(Property who, int page)
    {
        // 玩家对象不存在
        if (who == null)
            return string.Empty;

        List<CsvRow> list = null;

        if (!mPageListDic.TryGetValue(page, out list))
            return string.Empty;

        string group_title = list[0].Query<string>("group_title");

        // 分页名称脚本
        LPCValue scriptV = list[0].Query<LPCValue>("group_title_script");
        if (scriptV == null || scriptV.AsInt == 0)
            return LocalizationMgr.Get(group_title);

        // 调用脚本获取描述
        return ScriptMgr.Call(scriptV.AsInt, who, group_title) as string;
    }

    /// <summary>
    /// 根据分页获取配置列表
    /// </summary>
    public static Dictionary<int, List<CsvRow>> GetCourseListByPage(int page)
    {
        Dictionary<int, List<CsvRow>> dic = null;

        if (!mPageDic.TryGetValue(page, out dic))
            dic = new Dictionary<int, List<CsvRow>>();

        return dic;
    }

    /// <summary>
    /// 获取游戏历程数据
    /// </summary>
   public static LPCArray GetCourseData(Property who, int courseId)
    {
        if (who == null)
            return LPCArray.Empty;

        // 查询玩家游戏历程数据
        LPCValue v = who.Query<LPCValue>("game_course");
        if (v == null || ! v.IsMapping)
            return LPCArray.Empty;

        LPCMapping game_course = v.AsMapping;
        if(! game_course.ContainsKey(courseId))
            return LPCArray.Empty;

        return game_course.GetValue<LPCArray>(courseId);
    }

    /// <summary>
    /// 获取指定历程的描述
    /// </summary>
    public static string GetDesc(Property who, int courseId)
    {
        if (who == null)
            return string.Empty;

        // 获取配置数据
        CsvRow row = GetConfigCourse(courseId);
        if (row == null)
            return string.Empty;

        // 描述参数
        string descArg = row.Query<string>("desc_arg");

        // 获取描述脚本
        LPCValue descScript = row.Query<LPCValue>("desc_script");
        if (descScript == null || descScript.AsInt == 0)
            return LocalizationMgr.Get(descArg);

        return ScriptMgr.Call(descScript.AsInt, who, courseId, descArg) as string;
    }

    /// <summary>
    /// 获取小标题描述
    /// </summary>
    public static string GetSubTitle(Property who, int courseId)
    {
        if (who == null)
            return string.Empty;

        // 获取配置数据
        CsvRow row = GetConfigCourse(courseId);
        if (row == null)
            return string.Empty;

        // 描述参数
        string descArg = row.Query<string>("sub_title_arg");

        // 获取描述脚本
        LPCValue descScript = row.Query<LPCValue>("sub_title_script");
        if (descScript == null || descScript.AsInt == 0)
            return LocalizationMgr.Get(descArg);

        return ScriptMgr.Call(descScript.AsInt, who, courseId, descArg) as string;
    }

    /// <summary>
    /// 是否有新数据
    /// </summary>
    public static bool IsNewGameCourse(Property who)
    {
        // 刷新标识
        LPCValue v = who.Query<LPCValue>("refresh_game_course");

        // 没有新的数据不处理
        if (v != null && v.AsInt == 0)
            return false;

        return true;
    }

    /// <summary>
    /// 解锁新历程
    /// </summary>
    public static void AddUnlockNewCourse(Property who, int courseId)
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 获取配置数据
        CsvRow row = GetConfigCourse(courseId);
        if (row == null)
            return;

        // 获取本地缓存结束新历程列表
        LPCArray unlockNewCourse = who.QueryTemp<LPCArray>("unlock_new_course");

        // 获取该历程所属页面
        int page = row.Query<int>("page");

        // 如果该历程已经在列表中
        if (unlockNewCourse != null && unlockNewCourse.IndexOf(page) != -1)
            return;

        // 如果本地还没有数据
        if (unlockNewCourse == null)
            unlockNewCourse = new LPCArray(page);
        else
            unlockNewCourse.Add(page);

        // 记录解锁列表
        who.SetTemp("unlock_new_course", LPCValue.Create(unlockNewCourse));
    }

    /// <summary>
    /// 删除新解锁历程
    /// </summary>
    public static void RemoveNewUnlockCourse(Property who, int page)
    {
        // 获取本地缓存结束新历程列表
        LPCArray unlockNewCourse = who.QueryTemp<LPCArray>("unlock_new_course");

        // 如果没有新解锁历程，或者没有page的新历程
        if (unlockNewCourse == null || unlockNewCourse.IndexOf(page) == -1)
            return;

        // 记录解锁列表
        unlockNewCourse.Remove(page);
        who.SetTemp("unlock_new_course", LPCValue.Create(unlockNewCourse));
    }

    /// <summary>
    /// 获取新解锁历程
    /// </summary>
    public static LPCArray GetNewUnlockCourses(Property who)
    {
        // 获取本地缓存结束新历程列表
        LPCArray unlockNewCourse = who.QueryTemp<LPCArray>("unlock_new_course");

        // 如果没有新解锁历程
        if (unlockNewCourse == null)
            return LPCArray.Empty;

        // 记录解锁列表
        return unlockNewCourse;
    }

    /// <summary>
    /// 是否有新解锁的历程
    /// </summary>
    /// <param name="who">Who.</param>
    /// <param name="page">Page.</param>
    public static bool IsNewUnLockCourse(Property who, int page)
    {
        // 获取本地缓存结束新历程列表
        LPCArray unlockNewCourse = who.QueryTemp<LPCArray>("unlock_new_course");

        // 如果没有新解锁历程
        if (unlockNewCourse == null)
            return false;

        return unlockNewCourse.IndexOf(page) != -1;
    }

    /// <summary>
    /// 请求游戏历程数据
    /// </summary>
    public static void RequestGameCourse(Property who)
    {
        // 玩家对象不存在
        if (who == null)
            return;

        // 没有新数据
        if (! IsNewGameCourse(who))
            return;

        // 通知服务器请求游戏历程数据
        Operation.CmdGetGameCourse.Go();
    }
}
