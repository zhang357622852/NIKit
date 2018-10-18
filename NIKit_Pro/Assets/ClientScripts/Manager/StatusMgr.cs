/// <summary>
/// StatusMgr.cs
/// Copy from zhangyg 2014-10-22
/// 状态管理
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using LPC;

/// <summary>
/// 状态管理
/// </summary>
public class StatusMgr
{
    #region 变量

    // 别名状态映射表
    private static Dictionary<string, CsvRow> statusAlias = new Dictionary<string, CsvRow>();

    // 别名->状态id
    private static Dictionary<string, int> aliasToStatusId = new Dictionary<string, int>();

    // 状态id->别名
    private static Dictionary<int, string> statusIdToAlias = new Dictionary<int, string>();

    // 状态配置表信息
    private static CsvFile mStatusCsv;

    // 死亡状态id
    private const int DIED_STATUS_ID = 1;

    // 状态cookie索引
    private static int mStatusCookie = 0;

    #endregion

    #region 属性

    /// <summary>
    /// 获取配置表信息
    /// </summary>
    public static CsvFile StatusCsv { get { return mStatusCsv; } }

    // 返回一个新的状态cookie
    public static int NewCookie { get { return mStatusCookie++; } }

    #endregion

    #region 内部接口

    /// <summary>
    /// 清除状态.
    /// </summary>
    private static bool ClearStatusNoRefreshAffect(Property who, LPCArray statusList, LPCMapping extraPara, int clearType = StatusConst.CLEAR_TYPE_END_SELF)
    {
        // 对象正在析构中
        if (who == null || who.IsDestroyed)
            return false;

        // 没有状态需要清除
        if (statusList == null || statusList.Count == 0)
            return false;

        // 获取对象rid
        string rid = who.GetRid();
        LPCArray clearStatusList = LPCArray.Empty;
        bool needRefreshAffect = false;
        int clearScriptNo;
        int clearAfterScriptNo;
        CsvRow statusInfo;
        int statusId;
        List<LPCMapping> conditionList;
        LPCValue props;

        // 获取当前状态
        List<LPCMapping> allStatus = who.GetAllStatus();
        if (allStatus.Count < statusList.Count)
        {
            // 初始化数据
            conditionList = new List<LPCMapping>();

            // 遍历当前全部状态
            foreach (LPCMapping data in allStatus)
            {
                // 不是需要清除状态
                if (statusList.IndexOf(data.GetValue<int>("status_id")) == -1)
                    continue;

                // 添加清除列表
                conditionList.Add(data);
            }

            // 清除状态
            for (int i = 0; i < conditionList.Count; i++)
            {
                // 获取状态的配置信息
                statusId = conditionList[i].GetValue<int>("status_id");
                statusInfo = GetStatusInfo(statusId);

                // 没有配置的状态不处理
                if (statusInfo == null)
                    continue;

                // 回调状态附加时触发的脚本
                clearScriptNo = statusInfo.Query<int>("clear");
                if (clearScriptNo != 0)
                    ScriptMgr.Call(clearScriptNo, who, statusId, conditionList[i], clearType, extraPara);

                // 添加清除数据列表
                clearStatusList.Add(statusId);

                // 清除状态
                who.RemoveStatus(conditionList[i]);

                // 清除以后触发执行的脚本
                clearAfterScriptNo = statusInfo.Query<int>("clear_after");
                if (clearAfterScriptNo != 0)
                    ScriptMgr.Call(clearAfterScriptNo, who, statusId, conditionList[i], clearType, extraPara);

                // 停止当前正在播放的状态action
                if (who.Actor != null && ! who.CheckStatus(statusId))
                    who.Actor.CancelActionSet(string.Format("{0}_{1}", rid, statusInfo.Query<string>("alias")));

                // 判断是否需要刷新属性
                props = statusInfo.Query<LPCValue>("props");
                if (props.IsInt ||
                    (props.IsArray && props.AsArray.Count != 0) ||
                    conditionList[i].ContainsKey("props"))
                    needRefreshAffect = true;
            }
        }
        else
        {
            // 逐个状态
            foreach(LPCValue status in statusList.Values)
            {
                if (status.IsInt)
                    statusId = status.AsInt;
                else
                    statusId = StatusMgr.GetStatusIndex(status.AsString);

                // 如果玩家有该状态需要清除
                if (! who.CheckStatus(statusId))
                    continue;

                // 获取状态的配置信息
                statusInfo = GetStatusInfo(statusId);

                // 没有配置的状态不处理
                if (statusInfo == null)
                    continue;

                // 获取当前所有状态
                allStatus = who.GetAllStatus();
                conditionList = new List<LPCMapping>();

                // 遍历当前全部状态
                foreach (LPCMapping data in allStatus)
                {
                    // 获取状态的信息
                    if (data.GetValue<int>("status_id") != statusId)
                        continue;

                    // 添加清除列表
                    conditionList.Add(data);
                }

                // 清除状态
                for (int i = 0; i < conditionList.Count; i++)
                {
                    // 回调状态附加时触发的脚本
                    clearScriptNo = statusInfo.Query<int>("clear");
                    if (clearScriptNo != 0)
                        ScriptMgr.Call(clearScriptNo, who, statusId, conditionList[i], clearType, extraPara);

                    // 添加清除数据列表
                    clearStatusList.Add(statusId);

                    // 清除状态
                    who.RemoveStatus(conditionList[i]);

                    // 清除以后触发执行的脚本
                    clearAfterScriptNo = statusInfo.Query<int>("clear_after");
                    if (clearAfterScriptNo != 0)
                        ScriptMgr.Call(clearAfterScriptNo, who, statusId, conditionList[i], clearType, extraPara);

                    // 需要刷新属性
                    if (conditionList[i].ContainsKey("props"))
                        needRefreshAffect = true;

                    // 停止当前正在播放的状态action
                    if (who.Actor != null && ! who.CheckStatus(statusId))
                        who.Actor.CancelActionSet(string.Format("{0}_{1}", rid, statusInfo.Query<string>("alias")));
                }

                // 判断是否需要刷新属性
                props = statusInfo.Query<LPCValue>("props");
                if (props.IsInt ||
                    (props.IsArray && props.AsArray.Count != 0))
                    needRefreshAffect = true;
            }
        }

        // 抛出附加状态事件
        // 构建事件参数
        if (clearStatusList.Count != 0)
        {
            LPCMapping eventPara = new LPCMapping();
            eventPara.Add("rid", rid);
            eventPara.Add("status_list", clearStatusList);
            EventMgr.FireEvent(EventMgrEventType.EVENT_CLEAR_STATUS, MixedValue.NewMixedValue<LPCMapping>(eventPara), false, true);
        }

        // 返回是否需要刷新属性
        return needRefreshAffect;
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        aliasToStatusId.Clear();

        statusIdToAlias.Clear();

        // 载入状态配置表信息
        mStatusCsv = CsvFileMgr.Load("status");

        // 构造状态别名映射表
        string alias = string.Empty;
        int statusId = 0;

        // 获取死亡状态的“exclusion”列表
        LPCArray dieExclusionList = LPCArray.Empty;

        // 遍历各个状态
        foreach (CsvRow row in StatusCsv.rows)
        {
            alias = row.Query<string>("alias");
            statusId = row.Query<int>("status");

            statusAlias[alias] = row;

            // 别名->状态id
            aliasToStatusId.Add(alias, statusId);

            // 状态id->别名
            statusIdToAlias.Add(statusId, alias);

            // 死亡清除
            if (row.Query<int>("dead_clear") == 1)
                dieExclusionList.Add(statusId);
        }

        // 获取死亡
        CsvRow DieCsv = GetStatusInfo(DIED_STATUS_ID);
        DieCsv.Add("exclusion", LPCValue.Create(dieExclusionList));

        // 遍历各个状态
        LPCArray immunityList = LPCArray.Empty;

        // 构建immunity列表
        foreach(string aliasName in aliasToStatusId.Keys)
        {
            // 如果不包含该排除信息
            if (! mStatusCsv.columns.ContainsKey(aliasName))
                continue;

            // 重新初始化数据
            immunityList = LPCArray.Empty;

            // 遍历全部状态列表
            foreach (CsvRow row in StatusCsv.rows)
            {
                // 是否免疫
                if (row.Query<int>(aliasName) != 1)
                    continue;

                // 添加到免疫列表中
                immunityList.Add(row.Query<int>("status"));
            }

            // 重置数据
            CsvRow aliasCsv = GetStatusInfo(aliasName);
            aliasCsv.Add("immunity", LPCValue.Create(immunityList));
        }
    }

    // 刷新技能带来的附加属性
    public static void RefreshStatusAffect(Property who)
    {
        // 没有任何状态，不用刷新属性
        List<LPCMapping> allStatus = who.GetAllStatus();

        // 属性列表
        LPCArray allProps = new LPCArray();
        CsvRow statusInfo;
        LPCValue props;

        // 遍历各个状态数据
        foreach (LPCMapping condition in allStatus)
        {
            // 获取状态的配置信息
            statusInfo = GetStatusInfo(condition["status_id"].AsInt);

            // 没有配置的状态不处理
            if (statusInfo == null)
                continue;

            // 如果状态有指定属性
            if (condition.ContainsKey("props"))
            {
                allProps.Append(condition.GetValue<LPCArray>("props"));
                continue;
            }

            // 没有分配属性不处理
            props = statusInfo.Query<LPCValue>("props");

            // 如果是直接配置的属性
            if (props.IsArray)
            {
                allProps.Append(props.AsArray);
                continue;
            }

            // 配置的是脚本格式
            if (props.IsInt)
            {
                // 调用脚本判断
                LPCArray ret = ScriptMgr.Call(props.AsInt, who, statusInfo, condition) as LPCArray;

                // 脚本异常
                if (ret == null)
                    continue;

                // 将属性添加到列表中
                allProps.Append(ret);
                continue;
            }

            // 其他格式暂时不支持
        }

        // 刷新状态的属性
        PropMgr.CalcAllProps(who, allProps, "status");

        // 记录到角色状态属性上
        who.SetTemp("status_props", LPCValue.Create(allProps));
    }

    /// <summary>
    /// 附加状态.
    /// </summary>
    public static void ApplyStatus(Property who, string status, LPCMapping condition)
    {
        // 对象正在析构中
        if (who == null || who.IsDestroyed)
            return;

        // 获取状态的配置信息
        CsvRow statusInfo = GetStatusInfo(status);

        // 没有配置的状态不处理
        if (statusInfo == null)
            return;

        // 更新相应状态的位
        int statusIndex = statusInfo.Query<int>("status");

        // 判断是否有检测脚本，如果有检查脚本需要检查脚本功过了才能附加状态
        int scriptNo = statusInfo.Query<int>("check_script");
        if (scriptNo != 0)
        {
            // 调用脚本判断
            bool checkRet = (bool)ScriptMgr.Call(scriptNo, who, statusIndex, statusInfo.Query<LPCValue>("check_args"));

            // 脚本没有通过检测
            if (!checkRet)
                return;
        }

        // 获取玩家当前全部状态
        List<LPCMapping> allStatus = who.GetAllStatus();
        CsvRow info;
        LPCArray immunity;

        // 判断角色身上是否有免疫该状态的状态存在
        foreach (LPCMapping data in allStatus)
        {
            // 获取状态的信息
            info = GetStatusInfo(data.GetValue<int>("status_id"));

            // 没有配置的状态不处理
            if (info == null)
                continue;

            // 获取状态的免疫列表, 只要有一个状态对该状态免疫就不能附加该状态
            immunity = info.Query<LPCArray>("immunity");
            if (immunity == null || immunity.Count == 0)
                continue;

            // 该状态免疫statusIndex
            if (immunity.IndexOf(statusIndex) == -1)
                continue;

            // 新增针对某状态的免疫失效脚本，用于某些有特殊属性的状态需要对应从免疫列表中剔除时，或者其他行为时
            LPCValue immunityScriptNo = info.Query<LPCValue>("immunity_script");

            // 1. 没有免疫检查脚本，不能附加状态
            // 2. 免疫检查脚本判断需要无视免疫，不能附加状态
            if (!immunityScriptNo.IsInt ||
                immunityScriptNo.AsInt == 0 ||
                ! (bool)ScriptMgr.Call(immunityScriptNo.AsInt, condition))
                return;
        }

        // 构建事件参数
        string rid = who.GetRid();
        LPCMapping extraPara = new LPCMapping();
        extraPara.Add("rid", rid);
        extraPara.Add("status", statusIndex);
        extraPara.Add("condition", condition);

        // 清除本状态排斥的状态， 不需要刷新属性
        bool needRefreshAffect = ClearStatusNoRefreshAffect(
            who,
            statusInfo.Query<LPCArray>("exclusion"),
            extraPara,
            StatusConst.CLEAR_TYPE_EXCLUSION);

        // 如果是TYPE_BUFF
        int statusType = statusInfo.Query<int>("status_type");
        if (StatusConst.TYPE_BUFF == statusType && condition.ContainsKey("round"))
        {
            // 重置状态回合
            condition.Add("round",
                who.QueryAttrib("add_buff_round") + condition.GetValue<int>("round"));
        }

        // 记录此附加的状态
        condition.Add("status_type", statusType);
        who.AddStatus(statusIndex, NewCookie, condition);

        // 判断是否需要刷新属性
        LPCValue props = statusInfo.Query<LPCValue>("props");
        if (needRefreshAffect ||
            props.IsInt ||
            (props.IsArray && props.AsArray.Count != 0) ||
            condition.ContainsKey("props"))
            PropMgr.RefreshAffect(who, "status");

        // 回调状态附加时触发的脚本
        int applyScriptNo = statusInfo.Query<int>("apply");
        if (applyScriptNo != 0)
            ScriptMgr.Call(applyScriptNo, who, statusIndex, condition);

        // 在播放状态光效之前，先取消状态动作，防止重复播放
        if (who.Actor != null)
        {
            // 重新播放表现序列
            string actionCookie = string.Format("{0}_{1}", rid, status);
            who.Actor.CancelActionSet(actionCookie);

            // 播放状态序列
            if (CombatActionMgr.HasActionSetData(status))
                who.Actor.DoActionSet(status, actionCookie, extraPara);
        }

        // 抛出附加状态事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_APPLY_STATUS, MixedValue.NewMixedValue<LPCMapping>(extraPara), false, true);
    }

    /// <summary>
    /// 副本过图清除状态
    /// </summary>
    public static void CrossMapClearStatus(Property who)
    {
        // 对象正在析构中
        if (who == null || who.IsDestroyed)
            return;

        int statusId = 0;
        CsvRow statusInfo = null;
        List<LPCMapping> allStatus = who.GetAllStatus();
        List<LPCMapping> conditionList = new List<LPCMapping>();

        // 获取当前所有状态
        for (int i = 0; i < allStatus.Count; i++)
        {
            // 获取状态id
            statusId = allStatus[i].GetValue<int>("status_id");

            // 获取状态的配置信息
            statusInfo = GetStatusInfo(statusId);

            // 没有配置的状态不处理
            if (statusInfo == null)
                continue;

            // 过图不需要清除
            if (statusInfo.Query<int>("cross_map_clear") != 1)
                continue;

            // 添加到列表
            conditionList.Add(allStatus[i]);
        }

        // 清除状态
        if (conditionList.Count == 0)
            return;

        bool refreshAffect = false;
        LPCArray statusList = LPCArray.Empty;
        string rid = who.GetRid();

        // 清除状态
        for (int i = 0; i < conditionList.Count; i++)
        {
            // 获取状态id
            statusId = conditionList[i].GetValue<int>("status_id");

            // 获取状态的配置信息
            statusInfo = GetStatusInfo(statusId);

            // 没有配置的状态不处理
            if (statusInfo == null)
                continue;

            // 回调状态附加时触发的脚本
            int clearScriptNo = statusInfo.Query<int>("clear");
            if (clearScriptNo != 0)
                ScriptMgr.Call(clearScriptNo, who, statusId, conditionList[i], StatusConst.CLEAR_TYPE_END_SELF, LPCMapping.Empty);

            // 添加清除数据列表
            statusList.Add(statusId);

            // 清除状态
            who.RemoveStatus(conditionList[i]);

            // 清除以后触发执行的脚本
            int clearAfterScriptNo = statusInfo.Query<int>("clear_after");
            if (clearAfterScriptNo != 0)
                ScriptMgr.Call(clearAfterScriptNo, who, statusId, conditionList[i], StatusConst.CLEAR_TYPE_END_SELF, LPCMapping.Empty);

            // 判断是否需要刷新属性
            LPCValue props = statusInfo.Query<LPCValue>("props");
            if (props.IsInt ||
                (props.IsArray && props.AsArray.Count != 0) ||
                conditionList[i].ContainsKey("props"))
                refreshAffect = true;

            // 停止当前正在播放的状态action
            if (who.Actor != null && ! who.CheckStatus(statusId))
                who.Actor.CancelActionSet(string.Format("{0}_{1}", rid, statusInfo.Query<string>("alias")));
        }

        // 判断是否需要刷新属性，如果需要则刷新
        if (refreshAffect)
            PropMgr.RefreshAffect(who, "status");

        // 抛出状态清除事件
        if (statusList.Count != 0)
        {
            LPCMapping eventPara = new LPCMapping();
            eventPara.Add("rid", rid);
            eventPara.Add("status_list", statusList);
            EventMgr.FireEvent(EventMgrEventType.EVENT_CLEAR_STATUS, MixedValue.NewMixedValue<LPCMapping>(eventPara), false, true);
        }
    }

    /// <summary>
    /// 清除状态.
    /// </summary>
    public static void ClearStatusByCookie(Property who, List<int> cookieList, LPCMapping extraPara, int clearType = StatusConst.CLEAR_TYPE_END_SELF)
    {
        // 对象正在析构中
        if (who == null || who.IsDestroyed)
            return;

        // 没有状态需要清除
        if (cookieList == null || cookieList.Count == 0)
            return;

        string rid = who.GetRid();
        LPCArray statusList = LPCArray.Empty;
        bool needRefreshAffect = false;
        LPCMapping condition;
        int statusId;
        CsvRow statusInfo;
        int clearScriptNo;
        int clearAfterScriptNo;
        LPCValue props;

        // 逐个cookie清除
        foreach (int cookie in cookieList)
        {
            // 获取当前cookie的condition
            condition = who.GetStatusCondition(cookie);

            // 没有该状态
            if (condition == null)
                continue;

            // 获取状态id
            statusId = condition.GetValue<int>("status_id");

            // 获取状态的配置信息
            statusInfo = GetStatusInfo(statusId);

            // 没有配置的状态不处理
            if (statusInfo == null)
                continue;

            // 回调状态附加时触发的脚本
            clearScriptNo = statusInfo.Query<int>("clear");
            if (clearScriptNo != 0)
                ScriptMgr.Call(clearScriptNo, who, statusId, condition, clearType, extraPara);

            // 添加清除数据列表
            statusList.Add(statusId);

            // 清除状态
            who.RemoveStatus(condition);

            // 清除以后触发执行的脚本
            clearAfterScriptNo = statusInfo.Query<int>("clear_after");
            if (clearAfterScriptNo != 0)
                ScriptMgr.Call(clearAfterScriptNo, who, statusId, condition, clearType, extraPara);

            // 判断是否需要刷新属性
            props = statusInfo.Query<LPCValue>("props");
            if (props.IsInt ||
                (props.IsArray && props.AsArray.Count != 0) ||
                condition.ContainsKey("props"))
                needRefreshAffect = true;

            // 停止当前正在播放的状态action
            if (who.Actor != null && !who.CheckStatus(statusId))
                who.Actor.CancelActionSet(string.Format("{0}_{1}", rid, statusInfo.Query<string>("alias")));
        }

        // 如果需要刷新状态属性则刷新之，否则不需要刷新
        if (needRefreshAffect)
            PropMgr.RefreshAffect(who, "status");

        // 抛出附加状态事件
        LPCMapping eventPara = new LPCMapping();
        eventPara.Add("rid", rid);
        eventPara.Add("status_list", statusList);

        // 抛出受创事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_CLEAR_STATUS, MixedValue.NewMixedValue<LPCMapping>(eventPara), false, true);
    }

    /// <summary>
    /// 清除状态.
    /// </summary>
    public static void ClearStatusByCookie(Property who, LPCArray cookieList, LPCMapping extraPara, int clearType = StatusConst.CLEAR_TYPE_END_SELF)
    {
        // 对象正在析构中
        if (who == null || who.IsDestroyed)
            return;

        // 没有状态需要清除
        if (cookieList == null || cookieList.Count == 0)
            return;

        string rid = who.GetRid();
        LPCArray statusList = LPCArray.Empty;
        bool needRefreshAffect = false;
        LPCMapping condition;
        int statusId;
        CsvRow statusInfo;
        int clearScriptNo;
        int clearAfterScriptNo;
        LPCValue props;

        // 逐个cookie清除
        foreach (LPCValue cookie in cookieList.Values)
        {
            // 获取当前cookie的condition
            condition = who.GetStatusCondition(cookie.AsInt);

            // 没有该状态
            if (condition == null)
                continue;

            // 获取状态id
            statusId = condition.GetValue<int>("status_id");

            // 获取状态的配置信息
            statusInfo = GetStatusInfo(statusId);

            // 没有配置的状态不处理
            if (statusInfo == null)
                continue;

            // 回调状态附加时触发的脚本
            clearScriptNo = statusInfo.Query<int>("clear");
            if (clearScriptNo != 0)
                ScriptMgr.Call(clearScriptNo, who, statusId, condition, clearType, extraPara);

            // 添加清除数据列表
            statusList.Add(statusId);

            // 清除状态
            who.RemoveStatus(condition);

            // 清除以后触发执行的脚本
            clearAfterScriptNo = statusInfo.Query<int>("clear_after");
            if (clearAfterScriptNo != 0)
                ScriptMgr.Call(clearAfterScriptNo, who, statusId, condition, clearType, extraPara);

            // 判断是否需要刷新属性
            props = statusInfo.Query<LPCValue>("props");
            if (props.IsInt ||
                (props.IsArray && props.AsArray.Count != 0) ||
                condition.ContainsKey("props"))
                needRefreshAffect = true;

            // 停止当前正在播放的状态action
            if (who.Actor != null && !who.CheckStatus(statusId))
                who.Actor.CancelActionSet(string.Format("{0}_{1}", rid, statusInfo.Query<string>("alias")));
        }

        // 如果需要刷新状态属性则刷新之，否则不需要刷新
        if (needRefreshAffect)
            PropMgr.RefreshAffect(who, "status");

        // 抛出附加状态事件
        LPCMapping eventPara = new LPCMapping();
        eventPara.Add("rid", rid);
        eventPara.Add("status_list", statusList);

        // 抛出EVENT_CLEAR_STATUS事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_CLEAR_STATUS, MixedValue.NewMixedValue<LPCMapping>(eventPara), false, true);
    }

    /// <summary>
    /// 清除状态.
    /// </summary>
    public static void ClearStatusByCookie(Property who, int cookie, LPCMapping extraPara, int clearType = StatusConst.CLEAR_TYPE_END_SELF)
    {
        // 对象正在析构中
        if (who == null || who.IsDestroyed)
            return;

        // 获取当前cookie的condition
        LPCMapping condition = who.GetStatusCondition(cookie);

        // 没有该状态
        if (condition == null)
            return;

        // 获取状态id
        int statusId = condition.GetValue<int>("status_id");

        // 获取状态的配置信息
        CsvRow statusInfo = GetStatusInfo(statusId);

        // 没有配置的状态不处理
        if (statusInfo == null)
            return;

        // 回调状态附加时触发的脚本
        int clearScriptNo = statusInfo.Query<int>("clear");
        if (clearScriptNo != 0)
            ScriptMgr.Call(clearScriptNo, who, statusId, condition, clearType, extraPara);

        // 清除状态
        who.RemoveStatus(condition);

        // 清除以后触发执行的脚本
        int clearAfterScriptNo = statusInfo.Query<int>("clear_after");
        if (clearAfterScriptNo != 0)
            ScriptMgr.Call(clearAfterScriptNo, who, statusId, condition, clearType, extraPara);

        // 判断是否需要刷新属性
        LPCValue props = statusInfo.Query<LPCValue>("props");
        if (props.IsInt ||
            (props.IsArray && props.AsArray.Count != 0) ||
            condition.ContainsKey("props"))
            PropMgr.RefreshAffect(who, "status");

        // 抛出附加状态事件
        string rid = who.GetRid();

        // 停止当前正在播放的状态action
        if (who.Actor != null && ! who.CheckStatus(statusId))
            who.Actor.CancelActionSet(string.Format("{0}_{1}", rid, statusInfo.Query<string>("alias")));

        // 抛出EVENT_CLEAR_STATUS事件
        LPCMapping eventPara = new LPCMapping();
        eventPara.Add("rid", rid);
        eventPara.Add("status_list", new LPCArray(statusId));
        EventMgr.FireEvent(EventMgrEventType.EVENT_CLEAR_STATUS, MixedValue.NewMixedValue<LPCMapping>(eventPara), false, true);
    }

    /// <summary>
    /// 清除状态.
    /// </summary>
    public static void ClearStatus(Property who, LPCArray statusList, LPCMapping extraPara, int clearType = StatusConst.CLEAR_TYPE_END_SELF)
    {
        // 对象正在析构中
        if (who == null || who.IsDestroyed)
            return;

        // 没有状态需要清除
        if (statusList == null || statusList.Count == 0)
            return;

        // 获取对象rid
        string rid = who.GetRid();
        LPCArray clearStatusList = LPCArray.Empty;
        bool needRefreshAffect = false;
        int clearScriptNo;
        int clearAfterScriptNo;
        CsvRow statusInfo;
        int statusId;
        List<LPCMapping> allStatus;
        List<LPCMapping> conditionList;
        LPCValue props;

        // 逐个状态
        foreach(LPCValue status in statusList.Values)
        {
            if (status.IsInt)
                statusId = status.AsInt;
            else
                statusId = StatusMgr.GetStatusIndex(status.AsString);

            // 如果玩家有该状态需要清除
            if (! who.CheckStatus(statusId))
                continue;

            // 获取状态的配置信息
            statusInfo = GetStatusInfo(statusId);

            // 没有配置的状态不处理
            if (statusInfo == null)
                continue;

            // 获取当前所有状态
            allStatus = who.GetAllStatus();
            conditionList = new List<LPCMapping>();

            // 遍历当前全部状态
            foreach (LPCMapping data in allStatus)
            {
                // 获取状态的信息
                if (data.GetValue<int>("status_id") != statusId)
                    continue;

                // 添加清除列表
                conditionList.Add(data);
            }

            // 清除状态
            for (int i = 0; i < conditionList.Count; i++)
            {
                // 回调状态附加时触发的脚本
                clearScriptNo = statusInfo.Query<int>("clear");
                if (clearScriptNo != 0)
                    ScriptMgr.Call(clearScriptNo, who, statusId, conditionList[i], clearType, extraPara);

                // 清除状态
                who.RemoveStatus(conditionList[i]);

                // 清除以后触发执行的脚本
                clearAfterScriptNo = statusInfo.Query<int>("clear_after");
                if (clearAfterScriptNo != 0)
                    ScriptMgr.Call(clearAfterScriptNo, who, statusId, conditionList[i], clearType, extraPara);

                // 需要刷新属性
                if (conditionList[i].ContainsKey("props"))
                    needRefreshAffect = true;

                // 停止当前正在播放的状态action
                if (who.Actor != null && ! who.CheckStatus(statusId))
                    who.Actor.CancelActionSet(string.Format("{0}_{1}", rid, statusInfo.Query<string>("alias")));
            }

            // 添加清除数据列表
            clearStatusList.Add(statusId);

            // 判断是否需要刷新属性
            props = statusInfo.Query<LPCValue>("props");
            if (props.IsInt ||
                (props.IsArray && props.AsArray.Count != 0))
                needRefreshAffect = true;
        }

        // 需要刷新状态属性
        if (needRefreshAffect)
            PropMgr.RefreshAffect(who, "status");

        // 抛出附加状态事件
        // 构建事件参数
        if (clearStatusList.Count != 0)
        {
            LPCMapping eventPara = new LPCMapping();
            eventPara.Add("rid", rid);
            eventPara.Add("status_list", clearStatusList);
            EventMgr.FireEvent(EventMgrEventType.EVENT_CLEAR_STATUS, MixedValue.NewMixedValue<LPCMapping>(eventPara), false, true);
        }
    }

    /// <summary>
    /// 清除状态.
    /// </summary>
    public static void ClearStatus(Property who, string status, LPCMapping extraPara, int clearType = StatusConst.CLEAR_TYPE_END_SELF)
    {
        // 对象正在析构中
        if (who == null || who.IsDestroyed)
            return;

        // 如果玩家有该状态需要清除
        if (!who.CheckStatus(status))
            return;

        // 获取状态的配置信息
        CsvRow statusInfo = GetStatusInfo(status);

        // 没有配置的状态不处理
        if (statusInfo == null)
            return;

        // 更新相应状态的位
        int statusId = statusInfo.Query<int>("status");

        // 获取当前所有状态
        List<LPCMapping> allStatus = who.GetAllStatus();
        List<LPCMapping> conditionList = new List<LPCMapping>();
        bool refreshAffect = false;

        // 遍历当前全部状态
        foreach (LPCMapping data in allStatus)
        {
            // 获取状态的信息
            if (data.GetValue<int>("status_id") != statusId)
                continue;

            // 添加清除列表
            conditionList.Add(data);
        }

        // 获取对象rid
        string rid = who.GetRid();

        // 清除状态
        for (int i = 0; i < conditionList.Count; i++)
        {
            // 回调状态附加时触发的脚本
            int clearScriptNo = statusInfo.Query<int>("clear");
            if (clearScriptNo != 0)
                ScriptMgr.Call(clearScriptNo, who, statusId, conditionList[i], clearType, extraPara);

            // 清除状态
            who.RemoveStatus(conditionList[i]);

            // 清除以后触发执行的脚本
            int clearAfterScriptNo = statusInfo.Query<int>("clear_after");
            if (clearAfterScriptNo != 0)
                ScriptMgr.Call(clearAfterScriptNo, who, statusId, conditionList[i], clearType, extraPara);

            // 需要刷新属性
            if (conditionList[i].ContainsKey("props"))
                refreshAffect = true;

            // 停止当前正在播放的状态action
            if (who.Actor != null && ! who.CheckStatus(statusId))
                who.Actor.CancelActionSet(string.Format("{0}_{1}", rid, statusInfo.Query<string>("alias")));
        }

        // 判断是否需要刷新属性
        LPCValue props = statusInfo.Query<LPCValue>("props");
        if (props.IsInt ||
            (props.IsArray && props.AsArray.Count != 0) ||
            refreshAffect)
            PropMgr.RefreshAffect(who, "status");

        // 抛出附加状态事件
        // 构建事件参数
        LPCMapping eventPara = new LPCMapping();
        eventPara.Add("rid", rid);
        eventPara.Add("status_list", new LPCArray(statusId));

        // 抛出受创事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_CLEAR_STATUS, MixedValue.NewMixedValue<LPCMapping>(eventPara), false, true);
    }

    /// <summary>
    /// 增加状态作用回合
    /// </summary>
    public static void AddStatusRounds(Property who, List<int> cookieList, int times)
    {
        // 对象正在析构中
        if (who == null || who.IsDestroyed)
            return;

        LPCArray clearStatusList = LPCArray.Empty;
        bool needRefreshAffect = false;

        // 逐个cookie处理
        foreach (int cookie in cookieList)
        {
            // 获取当前cookie的condition
            LPCMapping condition = who.GetStatusCondition(cookie);

            // 没有该状态
            if (condition == null)
                continue;

            // 没有回合限制
            if (!condition.ContainsKey("round"))
                continue;

            // 增加回合数据
            int round = condition.GetValue<int>("round") + times;
            condition.Add("round", round);

            // 如果回合已经结束
            if (round <= 0)
            {
                // 获取状态的配置信息
                int statusId = condition.GetValue<int>("status_id");
                CsvRow statusInfo = GetStatusInfo(statusId);

                // 回调状态附加时触发的脚本
                int clearScriptNo = statusInfo.Query<int>("clear");
                if (clearScriptNo != 0)
                    ScriptMgr.Call(clearScriptNo, who, statusId, condition, StatusConst.CLEAR_TYPE_BREAK, LPCMapping.Empty);

                // 添加清除数据列表
                clearStatusList.Add(statusId);

                // 清除状态
                who.RemoveStatus(condition);

                // 清除以后触发执行的脚本
                int clearAfterScriptNo = statusInfo.Query<int>("clear_after");
                if (clearAfterScriptNo != 0)
                    ScriptMgr.Call(clearAfterScriptNo, who, statusId, condition, StatusConst.CLEAR_TYPE_BREAK, LPCMapping.Empty);

                // 判断是否需要刷新属性
                LPCValue props = statusInfo.Query<LPCValue>("props");
                if (props.IsInt ||
                (props.IsArray && props.AsArray.Count != 0) ||
                condition.ContainsKey("props"))
                    needRefreshAffect = true;

                // 停止当前正在播放的状态action
                if (who.Actor != null && !who.CheckStatus(statusId))
                    who.Actor.CancelActionSet(string.Format("{0}_{1}", who.GetRid(), statusInfo.Query<string>("alias")));
            }
        }

        // 判断是否需要刷新属性
        if (needRefreshAffect)
            PropMgr.RefreshAffect(who, "status");

        // 抛出状态清除事件
        if (clearStatusList.Count != 0)
        {
            LPCMapping eventPara = new LPCMapping();
            eventPara.Add("rid", who.GetRid());
            eventPara.Add("status_list", clearStatusList);
            EventMgr.FireEvent(EventMgrEventType.EVENT_CLEAR_STATUS, MixedValue.NewMixedValue<LPCMapping>(eventPara), false, true);
        }

        // 抛出状态回合改变事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_STATUS_ROUND_UPDATE, MixedValue.NewMixedValue<string>(who.GetRid()), false, true);
    }

    /// <summary>
    /// 增加状态作用回合
    /// </summary>
    public static void AddStatusRounds(Property who, int cookie, int times)
    {
        // 对象正在析构中
        if (who == null || who.IsDestroyed)
            return;

        // 获取当前cookie的condition
        LPCMapping condition = who.GetStatusCondition(cookie);

        // 没有该状态
        if (condition == null)
            return;

        // 没有回合限制
        if (! condition.ContainsKey("round"))
            return;

        // 增加回合数据
        int round = condition.GetValue<int>("round") + times;
        condition.Add("round", round);

        // 如果回合已经结束
        if (round <= 0)
        {
            // 获取状态的配置信息
            int statusId = condition.GetValue<int>("status_id");
            CsvRow statusInfo = GetStatusInfo(statusId);

            // 回调状态附加时触发的脚本
            int clearScriptNo = statusInfo.Query<int>("clear");
            if (clearScriptNo != 0)
                ScriptMgr.Call(clearScriptNo, who, statusId, condition, StatusConst.CLEAR_TYPE_BREAK, LPCMapping.Empty);

            // 清除状态
            who.RemoveStatus(condition);

            // 清除以后触发执行的脚本
            int clearAfterScriptNo = statusInfo.Query<int>("clear_after");
            if (clearAfterScriptNo != 0)
                ScriptMgr.Call(clearAfterScriptNo, who, statusId, condition, StatusConst.CLEAR_TYPE_BREAK, LPCMapping.Empty);

            // 判断是否需要刷新属性
            LPCValue props = statusInfo.Query<LPCValue>("props");
            if (props.IsInt ||
                (props.IsArray && props.AsArray.Count != 0) ||
                condition.ContainsKey("props"))
                PropMgr.RefreshAffect(who, "status");

            // 停止当前正在播放的状态action
            if (who.Actor != null && ! who.CheckStatus(statusId))
                who.Actor.CancelActionSet(string.Format("{0}_{1}", who.GetRid(), statusInfo.Query<string>("alias")));

            // 抛出状态清除事件
            LPCMapping eventPara = new LPCMapping();
            eventPara.Add("rid", who.GetRid());
            eventPara.Add("status_list", new LPCArray(statusId));
            EventMgr.FireEvent(EventMgrEventType.EVENT_CLEAR_STATUS, MixedValue.NewMixedValue<LPCMapping>(eventPara), false, true);
        }

        // 抛出状态回合改变事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_STATUS_ROUND_UPDATE, MixedValue.NewMixedValue<string>(who.GetRid()), false, true);
    }

    /// <summary>
    /// 执行回合制状态作用脚本
    /// </summary>
    public static void DoRoundApplyStatus(Property who, int type, string roundCookie)
    {
        // 对象正在析构中
        if (who == null || who.IsDestroyed)
            return;

        int round = 0;
        int statusId = 0;
        int applyScriptNo = 0;
        CsvRow statusInfo = null;
        List<LPCMapping> allStatus = who.GetAllStatus();
        List<LPCMapping> conditionList = new List<LPCMapping>();
        bool refreshAffect = false;

        // 获取当前所有状态
        for (int i = 0; i < allStatus.Count; i++)
        {
            // 需要剔除roundCookie的状态
            if (!string.IsNullOrEmpty(roundCookie) &&
                allStatus[i].GetValue<string>("round_cookie") == roundCookie)
                continue;

            // 获取状态id
            statusId = allStatus[i].GetValue<int>("status_id");

            // 获取状态的配置信息
            statusInfo = GetStatusInfo(statusId);

            // 没有配置的状态不处理
            if (statusInfo == null)
                continue;

            // 判断作用阶段是否一致
            if (statusInfo.Query<int>("round_apply_type") != type)
                continue;

            // 没有回合限制
            if (!allStatus[i].ContainsKey("round"))
                continue;

            // 添加清除列表
            round = allStatus[i].GetValue<int>("round");
            if (round <= 0)
            {
                conditionList.Add(allStatus[i]);
                continue;
            }

            // 回合数减-1
            if (round == 1)
                conditionList.Add(allStatus[i]);
            else
            {
                round = round - 1;
                allStatus[i].Add("round", round);
            }

            // 回调状态附加时触发的脚本
            applyScriptNo = statusInfo.Query<int>("round_apply_script");
            if (applyScriptNo == 0)
                continue;

            // 调用作用脚本
            ScriptMgr.Call(applyScriptNo, who, statusId, allStatus[i]);
        }

        // 获取对象rid
        string rid = who.GetRid();

        LPCArray statusList = LPCArray.Empty;
        for (int i = 0; i < conditionList.Count; i++)
        {
            // 获取状态id
            statusId = conditionList[i].GetValue<int>("status_id");

            // 获取状态的配置信息
            statusInfo = GetStatusInfo(statusId);

            // 没有配置的状态不处理
            if (statusInfo == null)
                continue;

            // 回调状态附加时触发的脚本
            int clearScriptNo = statusInfo.Query<int>("clear");
            if (clearScriptNo != 0)
                ScriptMgr.Call(clearScriptNo, who, statusId, conditionList[i], StatusConst.CLEAR_TYPE_END_SELF, LPCMapping.Empty);

            // 添加清除数据列表
            statusList.Add(statusId);

            // 清除状态
            who.RemoveStatus(conditionList[i]);

            // 清除以后触发执行的脚本
            int clearAfterScriptNo = statusInfo.Query<int>("clear_after");
            if (clearAfterScriptNo != 0)
                ScriptMgr.Call(clearAfterScriptNo, who, statusId, conditionList[i], StatusConst.CLEAR_TYPE_END_SELF, LPCMapping.Empty);

            // 判断是否需要刷新属性
            LPCValue props = statusInfo.Query<LPCValue>("props");
            if (props.IsInt ||
                (props.IsArray && props.AsArray.Count != 0) ||
                conditionList[i].ContainsKey("props"))
                refreshAffect = true;

            // 停止当前正在播放的状态action
            if (who.Actor != null && ! who.CheckStatus(statusId))
                who.Actor.CancelActionSet(string.Format("{0}_{1}", rid, statusInfo.Query<string>("alias")));
        }

        // 判断是否需要刷新属性
        if (refreshAffect)
            PropMgr.RefreshAffect(who, "status");

        // 抛出状态清除事件
        if (statusList.Count != 0)
        {
            LPCMapping eventPara = new LPCMapping();
            eventPara.Add("rid", who.GetRid());
            eventPara.Add("status_list", statusList);
            EventMgr.FireEvent(EventMgrEventType.EVENT_CLEAR_STATUS, MixedValue.NewMixedValue<LPCMapping>(eventPara), false, true);
        }

        // 抛出状态回合改变事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_STATUS_ROUND_UPDATE, MixedValue.NewMixedValue<string>(who.GetRid()), false, true);
    }

    /// <summary>
    /// 取得状态信息
    /// </summary>
    public static CsvRow GetStatusInfo(int status)
    {
        return StatusCsv.FindByKey(status);
    }

    /// <summary>
    /// 根据状态别名取得状态信息
    /// </summary>
    /// <returns>The status info.</returns>
    /// <param name="status">Status.</param>
    public static CsvRow GetStatusInfo(string status)
    {
        CsvRow data;

        // 获取配置信息
        if (statusAlias.TryGetValue(status, out data))
            return data;

        // 配置信息不存在返回null
        return null;
    }

    /// <summary>
    /// 根据状态名取得状态索引值
    /// </summary>
    public static int GetStatusIndex(string status)
    {
        int statusId;

        // 没有配置的状态
        if (aliasToStatusId.TryGetValue(status, out statusId))
            return statusId;

        // 默认返回-1
        return -1;
    }

    /// <summary>
    /// 取得状态别名
    /// </summary>
    public static string GetStatusAlias(int status)
    {
        string alias;

        // 没有配置的状态
        if (statusIdToAlias.TryGetValue(status, out alias))
            return alias;

        // 没有配置的状态string.Empty;
        return string.Empty;
    }


    /// <summary>
    /// 该状态是否显示状态图标
    /// </summary>
    /// <returns><c>true</c> if is status show the specified status; otherwise, <c>false</c>.</returns>
    /// <param name="status">Status.</param>
    public static bool IsStatusShow(int status)
    {
        // 获取状态的配置信息
        CsvRow statusInfo = GetStatusInfo(status);

        // 没有配置的状态不处理
        if (statusInfo == null)
            return false;

        if (statusInfo.Query<int>("is_show") == 1)
            return true;
        else
            return false;
    }

    #endregion
}
