/// <summary>
/// ActivityMgr.cs
/// Created by fengsc 2017/03/22
/// 活动模块管理脚本
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LPC;

public class ActivityMgr
{
    #region 成员变量

    // 任务配置信息
    private static CsvFile mActivityCsv;

    // 活动子任务
    private static CsvFile mActivityTaskCsv;

    // 所有开启的活动列表
    private static LPCMapping mActivityMap = LPCMapping.Empty;

    // 活动子模块
    private static Dictionary<string, Activity> mActivityModMap = new Dictionary<string, Activity>();

    // 有序的活动列表
    private static List<LPCMapping> mActivityList = new List<LPCMapping>();

    private static Dictionary<int, List<CsvRow>> mGroupActivityTask = new Dictionary<int, List<CsvRow>>();

    #endregion

    #region 属性

    /// <summary>
    /// 获取活动配置表信息
    /// </summary>
    public static CsvFile ActivityCsv
    {
        get
        {
            return mActivityCsv;
        }
    }

    /// <summary>
    /// 活动子任务
    /// </summary>
    public static CsvFile ActivityTaskCsv
    {
        get
        {
            return mActivityTaskCsv;
        }
    }

    /// <summary>
    /// 设置活动列表
    /// </summary>
    public static LPCMapping ActivityMap
    {
        get
        {
            return mActivityMap;
        }

        set
        {
            // 重新排序
            SortActivity(value);

            // 缓存数据
            mActivityMap = value;

            // 是否有新活动
            mHasNewActivity = HasNewActivity(value);
        }
    }

    /// <summary>
    /// 是否有新活动
    /// </summary>
    public static bool mHasNewActivity { get; set;}

    #endregion

    #region 内部函数

    /// <summary>
    /// 载入任务子模块
    /// </summary>
    private static void LoadActivityEntry()
    {
        mActivityModMap.Clear();

        // 收集所有策略
        Assembly asse = Assembly.GetExecutingAssembly();
        Type[] types = asse.GetTypes();
        foreach (Type t in types)
        {
            // 不是策略子模块不处理
            if (! t.IsSubclassOf(typeof(Activity)))
                continue;

            // 创建对象
            Activity mod = asse.CreateInstance(t.Name) as Activity;

            // 添加到列表中
            mActivityModMap.Add(mod.GetActivityName(), mod);
        }
    }

    /// <summary>
    /// 排序活动列表
    /// </summary>
    private static void SortActivity(LPCMapping activityMap)
    {
        mActivityList.Clear();

        foreach (string cookie in activityMap.Keys)
        {
            LPCMapping tempData = LPCMapping.Empty;

            tempData.Append(activityMap[cookie].AsMapping);

            tempData.Add("cookie", cookie);

            // 配置信息
            LPCMapping config = GetActivityInfo(tempData.GetValue<string>("activity_id"));
            if (config == null)
                config = LPCMapping.Empty;

            tempData.Add("sort_rule", config.GetValue<int>("sort_rule"));

            mActivityList.Add(tempData);
        }
    }

    /// <summary>
    /// 是否有新活动
    /// </summary>
    private static bool HasNewActivity(LPCMapping newActivityMap)
    {
        LPCArray activityList = LPCArray.Empty;

        // 剔除已经关闭的无效数据
        LPCValue viewedActivityList = OptionMgr.GetOption(ME.user, "viewed_activity_list");
        if (viewedActivityList != null)
        {
            foreach (LPCValue cookie in viewedActivityList.AsArray.Values)
            {
                // 没有该数据
                if (!newActivityMap.ContainsKey(cookie.AsString))
                    continue;

                // 添加到列表中
                activityList.Add(cookie.AsString);
            }

            // 缓存到本地
            OptionMgr.SetOption(ME.user, "viewed_activity_list", LPCValue.Create(activityList));
        }

        // 遍历新活动
        foreach (string newCookie in newActivityMap.Keys)
        {
            if (activityList.IndexOf(newCookie) == -1 && IsShowActivity(newActivityMap[newCookie].AsMapping))
                return true;
        }

        // 返回false
        return false;
    }

    /// <summary>
    /// 是否显示活动
    /// </summary>
    /// <param name="activityData"></param>
    /// <returns></returns>
    private static bool IsShowActivity(LPCMapping activityData)
    {
        if (activityData == null || ME.user == null)
            return false;

        CsvRow row = mActivityCsv.FindByKey(activityData.GetValue<string>("activity_id"));

        if (row == null)
            return false;

        int script = row.Query<int>("show_script");

        LPCValue args = row.Query<LPCValue>("show_args");

        if (script <= 0)
        {
            if (!args.IsInt || args.AsInt != 1)
                return false;
        }
        else
        {
            if (!(bool)ScriptMgr.Call(script, ME.user, args, activityData))
                return false;
        }

        return true;
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 载入任务配置表
        mActivityCsv = CsvFileMgr.Load("activity");

        // 载入任务配置表
        mActivityTaskCsv = CsvFileMgr.Load("activity_task");

        mGroupActivityTask.Clear();
        foreach (CsvRow item in mActivityTaskCsv.rows)
        {
            int group = item.Query<int>("group");

            List<CsvRow> list = null;

            if (!mGroupActivityTask.TryGetValue(group, out list))
                list = new List<CsvRow>();

            list.Add(item);

            mGroupActivityTask[group] = list;
        }

        // 载入任务子模块
        LoadActivityEntry();
    }

    // 获取子活动奖励数据
    public static LPCArray GetActivityTaskBonus(int taskId)
    {
        // 获取配置信息
        CsvRow row = ActivityTaskCsv.FindByKey(taskId);
        if (row == null)
            return LPCArray.Empty;

        // 奖励脚本
        LPCValue scriptNo = row.Query<LPCValue>("bonus_script");
        if (scriptNo == null || scriptNo.AsInt == 0)
            return LPCArray.Empty;

        // 返回描述内容
        return ScriptMgr.Call(scriptNo.AsInt, taskId, row.Query<LPCValue>("bonus_args")) as LPCArray;
    }

    /// <summary>
    /// 获取活动任务列表
    /// </summary>
    public static List<CsvRow> GetActivityTaskList(string activityId)
    {
        List<CsvRow> taskList = new List<CsvRow>();

        // 遍历数据
        foreach (CsvRow data in mActivityTaskCsv.rows)
        {
            // 不是需要查找的任务
            if (!string.Equals(activityId, data.Query<string>("activity_id")))
                continue;

            // 添加到列表中
            taskList.Add(data);
        }

        // 返回任务列表
        return taskList;
    }

    // 根据活动id和分组获取任务列表
    public static List<CsvRow> GetActivityTaskListByGroup(string activityId, int group)
    {
        List<CsvRow> list = new List<CsvRow>();

        List<CsvRow> groupList = null;
        if (!mGroupActivityTask.TryGetValue(group, out groupList))
            return list;

        foreach (CsvRow item in groupList)
        {
            if (item.Query<string>("activity_id") != activityId)
                continue;

            list.Add(item);
        }

        return list;
    }

    /// <summary>
    /// 获取活动任务描述
    /// </summary>
    public static string GetActivityTaskDesc(Property user, string cookie, int taskId)
    {
        // 活动不存在
        CsvRow row = mActivityTaskCsv.FindByKey(taskId);
        if (row == null)
            return string.Empty;

        // 描述参数
        string descArg = LocalizationMgr.Get(row.Query<string>("desc_arg"));

        // 描述脚本
        LPCValue descScript = row.Query<LPCValue>("desc_script");
        if (descScript == null || descScript.AsInt == 0)
            return descArg;

        // 返回描述内容
        return (string) ScriptMgr.Call(descScript.AsInt, user, cookie, taskId, descArg, row.Query<LPCMapping>("complete_condition"));
    }

    /// <summary>
    /// 获取活动任务列表
    /// </summary>
    public static CsvRow GetActivityTaskInfo(int taskId)
    {
        // 没有配置表信息
        if (mActivityTaskCsv == null)
            return null;

        // 返回配置信息
        return mActivityTaskCsv.FindByKey(taskId);
    }

    /// <summary>
    /// 领取活动任务奖励
    /// </summary>
    public static void ReceiveActivityTaskBonus(Property user, string cookie, int taskId)
    {
        // 没有奖励可以领取
        if (! HasActivityTaskBonus(user, cookie, taskId))
            return;

        // 返回该任务是否可以领取奖励
        Operation.CmdReceiveActivityTaskBonus.Go(cookie, taskId);
    }

    /// <summary>
    /// 是否可以领取奖励
    /// </summary>
    public static bool HasActivityTaskBonus(Property user, string cookie, int taskId)
    {
        // 获取玩家的当前活动奖励列表
        LPCArray bonus = user.Query<LPCArray>(string.Format("activity_data/{0}/task_bonus", cookie));
        if (bonus == null)
            return false;

        // 返回该任务是否可以领取奖励
        return (bonus.IndexOf(taskId) != -1);
    }

    /// <summary>
    /// 获取任务进度
    /// </summary>
    public static int GetActivityTaskProgress(Property user, string cookie, int taskId)
    {
        // 获取玩家的当前活动数据进度信息
        LPCMapping progressMap = user.Query<LPCMapping>(string.Format("activity_data/{0}/progress", cookie));
        if (progressMap == null)
            return 0;

        // 获取任务进度
        return progressMap.GetValue<int>(taskId);
    }

    /// <summary>
    /// 判断活动任务是否已经完成
    /// </summary>
    public static int GetCompletedActivityTaskTimes(Property user, string cookie, int taskId)
    {
        // 获取玩家的当前活动数据
        LPCMapping activityData = user.Query<LPCMapping>(string.Format("activity_data/{0}", cookie));
        if (activityData == null)
            return 0;

        // 记录该任务完成次数
        LPCMapping complateTimes = activityData.GetValue<LPCMapping>("complate_times");
        if (complateTimes == null)
            return 0;

        // 获取完成次数
        return complateTimes.GetValue<int>(taskId);
    }

    /// <summary>
    /// 判断活动任务是否已经完成
    /// </summary>
    public static bool IsCompletedActivityTask(Property user, string cookie, int taskId)
    {
        // 获取玩家的当前活动数据
        LPCMapping activityData = user.Query<LPCMapping>(string.Format("activity_data/{0}", cookie));
        if (activityData == null)
            return false;

        // 没有配置的活动任务不处理
        CsvRow data = GetActivityTaskInfo(taskId);
        if (data == null)
            return false;

        // 如果任务是循环任务
        if (data.Query<int>("cycle") > 0)
            return false;

        // 任务没有完成
        LPCArray complate = activityData.GetValue<LPCArray>("complate");
        if (complate == null ||
            complate.IndexOf(taskId) == -1)
            return false;

        // 任务已经完成
        return true;
    }

    /// <summary>
    /// 获取活动信息
    /// </summary>
    public static LPCMapping GetActivityInfo(string activityId)
    {
        if (ActivityCsv == null)
            return null;

        CsvRow row = ActivityCsv.FindByKey(activityId);
        if (row == null)
            return null;

        return row.ConvertLpcMap();
    }

    /// <summary>
    /// 获取活动列表
    /// </summary>
    public static List<LPCMapping> GetActivityList()
    {
        if (mActivityList == null)
            return new List<LPCMapping>();

        return mActivityList;
    }

    /// <summary>
    /// 获取活动背景列表
    /// </summary>
    public static LPCArray GetActivityBg(string activityId)
    {
        CsvRow row = mActivityCsv.FindByKey(activityId);
        if (row == null)
            return LPCArray.Empty;

        return row.Query<LPCArray>("bg");
    }

    /// <summary>
    /// 获取任务的描述信息
    /// </summary>
    public static string GetActivityDesc(string activityId)
    {
        CsvRow row = mActivityCsv.FindByKey(activityId);
        if (row == null)
            return string.Empty;

        // 返回描述信息
        return LocalizationMgr.Get(row.Query<string>("desc"));
    }

    /// <summary>
    /// 获取活动前置标题
    /// </summary>
    public static string GetActivityPreTitle(string activityId, LPCMapping extraPara = null)
    {
        // 没有获取到配置数据
        CsvRow row = mActivityCsv.FindByKey(activityId);
        if (row == null)
            return string.Empty;

        // 副标题参数
        string preTitleArg = LocalizationMgr.Get(row.Query<string>("pre_title_arg"));

        // 没有配置脚本
        LPCValue scriptNo = row.Query<LPCValue>("pre_title_script");
        if (scriptNo == null || scriptNo.AsInt == 0)
            return preTitleArg;

        return (string) ScriptMgr.Call(scriptNo.AsInt, preTitleArg, extraPara);
    }

    /// <summary>
    /// 获取活动标题
    /// </summary>
    public static string GetActivityTitle(string activityId, LPCMapping extraPara = null)
    {
        // 没有获取到配置数据
        CsvRow row = mActivityCsv.FindByKey(activityId);
        if (row == null)
            return string.Empty;

        // 副标题参数
        string subTitleArg = LocalizationMgr.Get(row.Query<string>("title_arg"));

        // 没有配置脚本
        LPCValue scriptNo = row.Query<LPCValue>("title_script");
        if (scriptNo == null || scriptNo.AsInt == 0)
            return subTitleArg;

        return (string) ScriptMgr.Call(scriptNo.AsInt, subTitleArg, extraPara);
    }

    /// <summary>
    /// 获取活动副标题
    /// </summary>
    public static string GetActivitySubTitle(string activityId, LPCMapping extraPara = null)
    {
        // 没有获取到配置数据
        CsvRow row = mActivityCsv.FindByKey(activityId);
        if (row == null)
            return string.Empty;

        // 副标题参数
        string subTitleArg = LocalizationMgr.Get(row.Query<string>("sub_title_arg"));

        // 没有配置脚本
        LPCValue scriptNo = row.Query<LPCValue>("sub_title_script");
        if (scriptNo == null || scriptNo.AsInt == 0)
            return subTitleArg;

        return (string) ScriptMgr.Call(scriptNo.AsInt, subTitleArg, extraPara);
    }

    /// <summary>
    /// 获取活动最大分数上限
    /// </summary>
    /// <param name="activityId"></param>
    /// <returns></returns>
    public static int GetActivityDbaseMaxScore(string activityId)
    {
        // 没有获取到配置数据
        CsvRow row = mActivityCsv.FindByKey(activityId);

        if (row == null)
            return 0;

        LPCMapping dbase = row.Query<LPCMapping>("dbase");

        if (dbase == null)
            return 0;

        if (dbase.ContainsKey("max_score"))
            return dbase.GetValue<int>("max_score");
        else
            return 0;
    }

    /// <summary>
    /// 奖励领取提示描述
    /// </summary>
    public static string GetActivityBonusTipDesc(string activityId, LPCMapping activityData)
    {
        // 活动配置数据
        CsvRow row = ActivityCsv.FindByKey(activityId);

        // 没有配置信息
        if (row == null)
            return string.Empty;

        // 描述脚本
        LPCValue scriptNo = row.Query<LPCValue>("bonus_tip_script");

        // 没有描述脚本
        if (scriptNo == null || ! scriptNo.IsInt || scriptNo.AsInt == 0)
            return string.Empty;

        // 返回描述信息
        return (string) ScriptMgr.Call(scriptNo.AsInt, row.Query<LPCValue>("bonus_tip_args"), activityData);
    }

    /// <summary>
    /// 获取活动时间描述
    /// </summary>
    /// <param name="activityId">活动id</param>
    /// <param name="timeList">活动有效时间段列表</param>
    public static string GetActivityTimeDesc(string activityId, LPCArray validPeriod)
    {
        // 活动配置数据
        CsvRow row = ActivityCsv.FindByKey(activityId);

        // 没有配置信息
        if (row == null)
            return string.Empty;

        // 描述参数
        LPCValue descArg = row.Query<LPCValue>("time_desc_arg");

        // 描述脚本
        LPCValue scriptNo = row.Query<LPCValue>("time_desc_script");

        // 没有描述脚本
        if (scriptNo == null || scriptNo.AsInt == 0)
            return LocalizationMgr.Get(descArg.AsString);

        if (validPeriod == null)
            validPeriod = LPCArray.Empty;

        // 返回描述信息
        return (string) ScriptMgr.Call(scriptNo.AsInt, descArg, validPeriod);
    }

    /// <summary>
    /// 获取有序活动列表
    /// </summary>
    /// <returns>The order activity list.</returns>
    public static List<LPCMapping> GetOrderActivityList()
    {
        // 按照开始时间从大到小排序列表,最新开的在最前面
        mActivityList.Sort((Comparison<LPCMapping>)delegate(LPCMapping a, LPCMapping b)
            {
                string weighta = string.Format("{0}{1:D10}{2}",
                    IsNewActivity(ME.user, a.GetValue<string>("cookie")) ? 1 : 0,
                    ConstantValue.MAX_VALUE - a.GetValue<int>("sort_rule"),
                    a.GetValue<string>("cookie"));

                string weightb = string.Format("{0}{1:D10}{2}",
                    IsNewActivity(ME.user, b.GetValue<string>("cookie")) ? 1 : 0,
                    ConstantValue.MAX_VALUE - b.GetValue<int>("sort_rule"),
                    b.GetValue<string>("cookie"));

                return string.Compare(weightb, weighta);
            }
        );

        // 返回排序后后的列表
        return mActivityList;
    }

    /// <summary>
    /// 是否是新活动
    /// </summary>
    /// <returns><c>true</c> if is new activity the specified cookie; otherwise, <c>false</c>.</returns>
    /// <param name="cookie">Cookie.</param>
    public static bool IsNewActivity(Property user, string cookie)
    {
        // 获取玩家本地设置
        LPCValue viewedActivityList = OptionMgr.GetOption(user, "viewed_activity_list");

        // 获取本地数据异常
        if (viewedActivityList == null)
            return false;

        // 转换数据格式
        LPCArray activityList = viewedActivityList.AsArray;

        // 判断是否是new activity
        return activityList.IndexOf(cookie) == -1;
    }

    /// <summary>
    /// 设置活动是否为新的状态
    /// </summary>
    /// <returns><c>true</c> if cancel activity new state the specified user cookie; otherwise, <c>false</c>.</returns>
    /// <param name="user">User.</param>
    /// <param name="cookie">Cookie.</param>
    public static void CancelActivityNewState(Property user, string cookie)
    {
        if (string.IsNullOrEmpty(cookie))
            return;

        // 获取本地Option信息
        LPCValue viewedActivityList = OptionMgr.GetOption(user, "viewed_activity_list");

        // 获取本地数据异常
        if (viewedActivityList == null)
            return;

        // 判断是否已经在列表中
        LPCArray activityList = viewedActivityList.AsArray;
        if (activityList.IndexOf(cookie) != -1)
            return;

        // 增加数据
        activityList.Add(cookie);

        // 缓存到本地
        OptionMgr.SetOption(user, "viewed_activity_list", LPCValue.Create(activityList));
    }

    /// <summary>
    /// 获取活动奖励
    /// </summary>
    /// <returns>The activity bonus.</returns>
    /// <param name="cookie">Cookie.</param>
    public static LPCValue GetActivityBonus(LPCMapping activityInfo)
    {
        if (activityInfo == null || activityInfo.Count == 0)
            return null;

        // 配置了奖励
        if (activityInfo.ContainsKey("bonus_args"))
            return activityInfo["bonus_args"];

        string activity_id = activityInfo.GetValue<string>("activity_id");

        // 没有直接配置奖励，直接去配置表中的奖励
        return mActivityCsv.FindByKey(activity_id).Query<LPCValue>("bonus_args");
    }

    /// <summary>
    /// 获取额外活动奖励
    /// </summary>
    /// <returns>The activity bonus.</returns>
    /// <param name="cookie">Cookie.</param>
    public static LPCValue GetActivityExtraBonus(LPCMapping activityInfo)
    {
        if (activityInfo == null || activityInfo.Count == 0)
            return null;

        // 配置了奖励
        if (activityInfo.ContainsKey("extra_bonus_args"))
            return activityInfo["extra_bonus_args"];

        string activity_id = activityInfo.GetValue<string>("activity_id");

        // 没有直接配置奖励，直接去配置  表中的奖励
        return mActivityCsv.FindByKey(activity_id).Query<LPCValue>("extra_bonus_args");
    }
    /// 取消新的活动标记
    /// </summary>
    /// <returns><c>true</c> if cancel new the specified cookie; otherwise, <c>false</c>.</returns>
    /// <param name="cookie">Cookie.</param>
    public static void CancelNew(string cookie)
    {
        LPCMapping activityInfo = mActivityMap.GetValue<LPCMapping>(cookie);

        if (activityInfo == null)
            return;

        if (activityInfo.GetValue<int>("is_new") == 1)
            activityInfo["is_new"] = LPCValue.Create(0);

        mActivityMap[cookie] = LPCValue.Create(activityInfo);
    }

    /// <summary>
    /// 获取显示的活动列表
    /// </summary>
    /// <returns>The show activity list.</returns>
    /// <param name="activityList">Activity list.</param>
    public static List<LPCMapping> GetShowActivityList(List<LPCMapping> activityList)
    {
        List<LPCMapping> list = new List<LPCMapping>();

        foreach (LPCMapping item in activityList)
        {
            CsvRow row = mActivityCsv.FindByKey(item.GetValue<string>("activity_id"));

            if (row == null)
                continue;

            int script = row.Query<int>("show_script");

            LPCValue args = row.Query<LPCValue> ("show_args");

            if (script <= 0)
            {
                if (! args.IsInt || args.AsInt != 1)
                    continue;
            }
            else
            {
                if (!(bool) ScriptMgr.Call (script, ME.user, args, item))
                    continue;
            }

            // 添加显示列表中
            list.Add(item);
        }

        return list;
    }

    /// <summary>
    /// 是否开启某个活动
    /// </summary>
    public static bool IsOpenAcitvity(string activityId)
    {
        if (string.IsNullOrEmpty(activityId))
            return false;

        foreach (LPCValue data in ActivityMap.Values)
        {
            LPCMapping map = data.AsMapping;
            if (map == null)
                continue;

            if (!map.ContainsKey("activity_id"))
                continue;

            if (map.GetValue<string>("activity_id").Equals(activityId))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 获取活动的关闭时间
    /// </summary>
    /// <returns>The activity close time.</returns>
    public static int GetActivityCloseTime(string activityId)
    {
        // 如果没有活动id
        if (string.IsNullOrEmpty(activityId))
            return 0;

        // 遍历当前已经开启活动列表
        foreach (LPCValue data in ActivityMap.Values)
        {
            LPCMapping map = data.AsMapping;
            if (map == null)
                continue;

            // 不是需要查找活动
            if (! map.ContainsKey("activity_id") ||
                ! map.GetValue<string>("activity_id").Equals(activityId))
                continue;

            return map.GetValue<int>("close_time");
        }

        return 0;
    }

    /// <summary>
    /// 获取活动当前有效状态结束时间
    /// </summary>
    public static int GetAcitvityValidEndTime(string activityId)
    {
        int endTime = 0;

        // 如果没有活动id
        if (string.IsNullOrEmpty(activityId))
            return endTime;

        // 遍历当前已经开启活动列表
        foreach (LPCValue data in ActivityMap.Values)
        {
            LPCMapping map = data.AsMapping;
            if (map == null)
                continue;

            // 不是需要查找活动
            if (! map.ContainsKey("activity_id") ||
                ! map.GetValue<string>("activity_id").Equals(activityId))
                continue;

            // 遍历活动的有效时间段数据
            foreach(LPCValue period in map.GetValue<LPCArray>("valid_period", LPCArray.Empty).Values)
            {
                // 没有有效id
                if (! period.AsMapping.ContainsKey("valid_id"))
                    continue;

                // 获取最大的结束时间
                endTime = Math.Max(period.AsMapping.GetValue<int>("end"), endTime);
            }
        }

        // 返回活动有效时段结束时间
        return endTime;
    }

    /// <summary>
    /// 根据活动cookie判断活动是否开启有效
    /// </summary>
    public static bool IsAcitvityValidByCookie(string activityCookie)
    {
        // 如果没有活动id
        if (string.IsNullOrEmpty(activityCookie))
            return false;

        // 获取活动数据
        LPCMapping data = ActivityMap.GetValue<LPCMapping>(activityCookie);
        if (data == null)
            return false;

        // 遍历活动的有效时间段数据
        foreach(LPCValue period in data.GetValue<LPCArray>("valid_period", LPCArray.Empty).Values)
        {
            // 没有有效id
            if (! period.AsMapping.ContainsKey("valid_id"))
                continue;

            // 活动已经有效
            return true;
        }

        // 返回不在活动有效期内
        return false;
    }

    /// <summary>
    /// 某个活动是否有效
    /// </summary>
    public static bool IsAcitvityValid(string activityId)
    {
        // 如果没有活动id
        if (string.IsNullOrEmpty(activityId))
            return false;

        // 遍历当前已经开启活动列表
        foreach (LPCValue data in ActivityMap.Values)
        {
            LPCMapping map = data.AsMapping;
            if (map == null)
                continue;

            // 不是需要查找活动
            if (! map.ContainsKey("activity_id") ||
                ! map.GetValue<string>("activity_id").Equals(activityId))
                continue;

            // 遍历活动的有效时间段数据
            foreach(LPCValue period in map.GetValue<LPCArray>("valid_period", LPCArray.Empty).Values)
            {
                // 没有有效id
                if (! period.AsMapping.ContainsKey("valid_id"))
                    continue;

                // 活动已经有效
                return true;
            }
        }

        // 返回不在活动有效期内
        return false;
    }

    /// <summary>
    /// 领取活动奖励
    /// </summary>
    public static void ReceiveActivityBonus(string cookie, LPCValue para)
    {
        // 通知服务器领取活动奖励
        Operation.CmdReceiveScoreBonus.Go(cookie, para);
    }

    /// <summary>
    /// 活动奖励是否已经领取
    /// </summary>
    /// <returns><c>true</c> if this instance has activity bonus the specified user cookie score; otherwise, <c>false</c>.</returns>
    /// <param name="user">User.</param>
    /// <param name="cookie">Cookie.</param>
    /// <param name="score">Score.</param>
    public static bool ActivityBonusIsReceived(Property user, string cookie, int score)
    {
        // 根据cookie 获取活动数据
        LPCMapping activityMap = user.Query<LPCMapping>(string.Format("activity_data/{0}", cookie));

        // 活动数据为空  返回false
        if (activityMap == null)
            return false;

        // 获取已领取的积分列表
        LPCArray receiveList = activityMap.GetValue<LPCArray>("receive_list");

        // 已领取的积分列表不为空
        if (receiveList == null)
            return false;

        // 判断是否在已领取的积分列表
        return (receiveList.IndexOf(score) != -1);
    }

    #endregion
}
