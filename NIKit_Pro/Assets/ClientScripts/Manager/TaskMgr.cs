/// <summary>
/// TaskMgr.cs
/// Created by zhaozy 2016-10-31
/// 任务管理模块
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using LPC;

/// <summary>
///  任务基类
/// </summary>
public abstract class Task
{
    // 模块初始化
    public abstract void Init();

    // 检测是否可以完成成就
    public abstract bool CheckCompleteTask(Property user, int taskId);
}

public static class TaskMgr
{
    #region 变量

    // 任务配置信息
    private static CsvFile mTaskCsv;

    // 各类型任务配置信息
    private static Dictionary<int, List<int>> mTaskMap = new Dictionary<int, List<int>>();

    // 子任务类型集合
    private static Dictionary<string, Task> mTaskEntryMap = new Dictionary<string, Task>();

    // 完成规则列表
    // 各类型任务配置信息
    private static Dictionary<string, List<int>> mCompleteRuleMap = new Dictionary<string, List<int>>();

    /// <summary>
    /// 缓存任务进度数据
    /// </summary>
    private static Dictionary<string, LPCMapping> mCacheTaskProgressMap = new Dictionary<string, LPCMapping>();

    /// <summary>
    /// 主线任务副本奖励数据
    /// </summary>
    private static Dictionary<string, int> mAssignClearanceMap = new Dictionary<string, int>();

    private static Dictionary<int, List<int>> mPreIdMap = new Dictionary<int, List<int>>();

    // 缓存的探索奖励数据
    private static LPCMapping mCacheResearchBonus = new LPCMapping();


    #endregion

    #region 属性

    /// <summary>
    /// 新邮件标识
    /// </summary>
    public static bool HasNewBonus { get; private set; }

    #endregion


    #region 私有接口

    /// <summary>
    /// 登陆成功回调
    /// </summary>
    private static void WhenLoginOk(int eventId, MixedValue para)
    {
        if (! HasNewBonus)
            return;

        if (mCacheResearchBonus.GetValue<int> ("is_first_receive") == 0)
        {
            LPCMapping bonusData = ReceiveCacheBonus ();

            GameObject wnd = WindowMgr.OpenWnd ("ShowResearchBonusWnd");

            wnd.GetComponent<ShowResearchBonusWnd> ().BindData (bonusData.GetValue<LPCArray>("bonus_data"));
        }
    }

    /// <summary>
    /// 初始化任务配置信息
    /// </summary>
    private static void LoadTaskFile()
    {
        mTaskMap.Clear();
        mCompleteRuleMap.Clear();

        mAssignClearanceMap.Clear();

        mPreIdMap.Clear();

        // 载入任务配置表
        mTaskCsv = CsvFileMgr.Load("task");

        // 任务分类
        for(int i = 0; i < mTaskCsv.count; i++)
        {
            if(mTaskCsv[i] == null)
                continue;

            // 获取完成成就规则，如果规则限制不能完成就不能完成
            string complete = mTaskCsv[i].Query<string>("complete");
            int type = mTaskCsv[i].Query<int>("type");
            int task_id = mTaskCsv[i].Query<int>("task_id");

            LPCValue preId = mTaskCsv[i].Query<LPCValue>("pre_id");

            List<int> list = null;

            if (preId.IsInt)
            {
                if (!mPreIdMap.TryGetValue(preId.AsInt, out list))
                    list = new List<int>();

                list.Add(task_id);

                mPreIdMap[preId.AsInt] = list;
            }
            else if(preId.IsArray)
            {
                for (int j = 0; j < preId.AsArray.Count; j++)
                {
                    list = null;

                    int id = preId.AsArray[j].AsInt;

                    if (!mPreIdMap.TryGetValue(id, out list))
                        list = new List<int>();

                    list.Add(task_id);

                    mPreIdMap[id] = list;
                }
            }

            // 添加任务类型分类
            if(mTaskMap.ContainsKey(type))
                mTaskMap[type].Add(task_id);
            else
                mTaskMap.Add(type, new List<int>(){ task_id });

            // 添加
            if ((mTaskCsv[i].Query<int>("type") == TaskConst.EASY_TASK) &&
               mTaskCsv[i].Query<string>("complete").Equals("clearance"))
            {
                LPCMapping completeConditon = mTaskCsv[i].Query<LPCMapping>("complete_condition");

                if (completeConditon.ContainsKey("instance_id"))
                {
                    LPCValue instanceIdValue = completeConditon.GetValue<LPCValue>("instance_id");

                    string instanceId;

                    if (instanceIdValue.IsString)
                    {
                        instanceId = instanceIdValue.AsString;

                        if (!mAssignClearanceMap.ContainsKey (instanceId))
                            mAssignClearanceMap.Add (instanceId, task_id);
                    }
                    else if(instanceIdValue.IsArray)
                    {
                        for (int j = 0; j < instanceIdValue.AsArray.Count; j++)
                        {
                            instanceId = instanceIdValue.AsArray [j].AsString;

                            if (!mAssignClearanceMap.ContainsKey (instanceId))
                                mAssignClearanceMap.Add (instanceId, task_id);
                        }
                    }

                }
            }

            // 如果不是客户端需要处理的完成任务不处理
            if (mTaskCsv[i].Query<int>("is_client") == 0)
                continue;

            // 添加任务完成类型分类
            if(mCompleteRuleMap.ContainsKey(complete))
                mCompleteRuleMap[complete].Add(task_id);
            else
                mCompleteRuleMap.Add(complete, new List<int>(){ task_id });
        }
    }

    /// <summary>
    /// 对任务数据排序
    /// 按照奖励列表，未完成列表，已完成列表显示
    /// 每日任务的任务id是无序的，主线任务的id是有序的
    /// </summary>
    /// <param name="isTaskIdOrdered">If set to <c>true</c> is task identifier ordered.</param>
    private static List<int> SortTaskData(Property user, int type, List<int> data)
    {
        List<int> task_data = new List<int>();

        // 已完成列表
        List<int> completeList = new List<int>();

        // 领取奖励列表
        List<int> bonusList = new List<int>();

        // 未完成列表
        List<int> unCompleteList = new List<int>();

        foreach(int task_id in data)
        {
            // 还未完成
            if(!IsCompleted(user, task_id))
            {
                unCompleteList.Add(task_id);
                continue;
            }

            // 奖励已领取
            if(isBounsReceived(user, task_id))
            {
                completeList.Add(task_id);
                continue;
            }

            bonusList.Add(task_id);
        }

        // 对三种list进行排序
        if(type == TaskConst.DAILY_TASK)
        {
            bonusList.Sort();
            unCompleteList.Sort();
            completeList.Sort();
        }

        // 加到列表中(按照奖励列表，未完成列表，已完成列表进行排序)
        task_data.AddRange(bonusList);
        task_data.AddRange(unCompleteList);
        task_data.AddRange(completeList);

        return task_data;
    }

    /// <summary>
    /// 载入任务子模块
    /// </summary>
    private static void LoadTaskEntry()
    {
        mTaskEntryMap.Clear();

        // 收集所有策略
        Assembly asse = Assembly.GetExecutingAssembly();
        Type[] types = asse.GetTypes();
        foreach (Type t in types)
        {
            // 不是策略子模块不处理
            if (! t.IsSubclassOf(typeof(Task)))
                continue;

            // 创建对象
            Task mod = asse.CreateInstance(t.Name) as Task;

            // 初始化模块
            mod.Init();

            // 添加到列表中
            mTaskEntryMap.Add(t.Name.ToLower(), mod);
        }
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化借口
    /// </summary>
    public static void Init()
    {
        // 载入副本配置表
        LoadTaskFile();

        // 载入任务子模块
        LoadTaskEntry();

        // 注册登陆成功回调
        EventMgr.UnregisterEvent("TaskMgr");
        EventMgr.RegisterEvent("TaskMgr", EventMgrEventType.EVENT_LOGIN_OK, WhenLoginOk);
    }

    /// <summary>
    /// 重置任务数据
    /// </summary>
    public static void DoResetAll()
    {
        HasNewBonus = false;

        mCacheResearchBonus = new LPCMapping();
    }

    /// <summary>
    /// 领取缓存奖励
    /// </summary>
    public static LPCMapping ReceiveCacheBonus()
    {
        HasNewBonus = false;

        LPCMapping cacheBonus = mCacheResearchBonus;

        mCacheResearchBonus = new LPCMapping();

        return cacheBonus;
    }

    /// <summary>
    /// 缓存奖励信息
    /// </summary>
    public static void DoCacheResearchhBonus(LPCMapping bonusData)
    {
        // 玩家对象不存在
        if (ME.user == null || ME.user.IsDestroyed)
            return;

        if (bonusData == null || bonusData.Count == 0)
            return;

        // 重置标识
        HasNewBonus = true;

        mCacheResearchBonus = bonusData;
    }

    /// <summary>
    /// 根据任务类型获取任务数据(默认已解锁)
    /// </summary>
    /// <returns>The un locked tasks list.</returns>
    /// <param name="user">User.</param>
    /// <param name="type">Type.</param>
    /// <param name="unlocked">If set to <c>true</c> unlocked.</param>
    public static List<int> GetTasksData(Property user,int type, bool unlocked = true)
    {
        // 无此类型的任务信息
        if(!mTaskMap.ContainsKey(type))
            return new List<int>();

        List<int> taskList = new List<int>();
        foreach(int task_id in mTaskMap[type])
        {
            if (unlocked && !isUnlocked (user, task_id))
                continue;

             taskList.Add (task_id);
        }

        return taskList;
    }

    /// <summary>
    /// 获取任务列表
    /// </summary>
    public static List<int> GetTasksByType(int type)
    {
        if (!mTaskMap.ContainsKey(type))
            return null;

        return mTaskMap[type];
    }

    /// <summary>
    /// 根据任务id获取后置任务
    /// </summary>
    public static List<int> GetPostpositionTasksById(int taskId)
    {
        if (!mPreIdMap.ContainsKey(taskId))
            return null;

        return mPreIdMap[taskId];
    }

    /// <summary>
    /// 获取任务图标
    /// </summary>
    public static string GetIcon(int taskId)
    {
        CsvRow row = mTaskCsv.FindByKey(taskId);
        if (row == null)
            return string.Empty;

        return row.Query<string>("icon");
    }

    /// <summary>
    /// 判断任务是否解锁
    /// </summary>
    public static bool isUnlocked(Property user, int taskId)
    {
        // 获取配置信息
        CsvRow data = mTaskCsv.FindByKey(taskId);

        // 没有配置的任务
        if (data == null)
            return false;

        // 如果任务的前驱任务没有完成，则该任务不能解锁
        LPCValue preId = data.Query<LPCValue>("pre_id");
        if (preId.IsInt && ! TaskMgr.IsCompleted(user, preId.AsInt))
            return false;

        // 没有配置就执行奖励
        LPCValue scriptNo = data.Query<LPCValue>("unlock_script");
        if (!scriptNo.IsInt || scriptNo.AsInt == 0)
            return true;

        // 调用脚本判断是否可以解锁
        return (bool)ScriptMgr.Call(scriptNo.AsInt, user, data.Query<LPCValue>("unlock_args"));
    }

    /// <summary>
    /// 判断任务奖励是否已领取
    /// </summary>
    /// <returns><c>true</c>, if bouns received was ised, <c>false</c> otherwise.</returns>
    public static bool isBounsReceived(Property user, int taskId)
    {
        // 获取配置信息
        CsvRow data = mTaskCsv.FindByKey(taskId);

        // 没有配置的任务
        if (data == null)
            return false;

        // 任务没有完成
        if(! IsCompleted(user, taskId))
            return false;

        LPCArray taskArray = LPCArray.Empty;
        if (data.Query<int>("type") == TaskConst.DAILY_TASK)
            taskArray = user.Query<LPCArray>("daily_task/bonus");
        else
            taskArray = user.Query<LPCArray>("task/bonus");

        // 没有任务数据则初始化
        if (taskArray == null || taskArray.Count == 0)
            return true;

        // 获取任务进度
        return taskArray.IndexOf(taskId) >= 0 ? false:true;
    }

    /// <summary>
    /// 检查是否有未领取的奖励
    /// </summary>
    /// <returns><c>true</c>, if has bonus was checked, <c>false</c> otherwise.</returns>
    /// <param name="type">Type.</param>
    public static bool CheckHasBonus(Property user, int type)
    {
        LPCArray bonusArray = LPCArray.Empty;

        if (type == TaskConst.DAILY_TASK)
            bonusArray = user.Query<LPCArray>("daily_task/bonus");
        else if(type == TaskConst.RESEARCH_TASK)
            bonusArray = user.Query<LPCArray>("research_task/bonus");
        else
            bonusArray = user.Query<LPCArray>("task/bonus");

        if(bonusArray == null || bonusArray.Count == 0)
            return false;

        if (type == TaskConst.DAILY_TASK || type == TaskConst.RESEARCH_TASK)
            return true;

        bool HasBonus = false;

        for(int i = 0; i < bonusArray.Count; i++)
        {
            int taskId = bonusArray[i].AsInt;

            CsvRow row = GetTaskInfo(taskId);

            if (row == null)
                continue;

            if(row.Query<int>("type") == type)
            {
                HasBonus = true;
                break;
            }
        }

        return HasBonus;
    }

    /// <summary>
    /// 是否有未领取的奖励
    /// </summary>
    public static bool CheckHasBonus(Property user)
    {
        // 日常任务奖励
        LPCArray bonusArray = user.Query<LPCArray>("daily_task/bonus");
        if (bonusArray != null && bonusArray.Count > 0)
            return true;

        // 主线任务奖励
        LPCArray taskBonus = user.Query<LPCArray>("task/bonus");
        if (taskBonus != null && taskBonus.Count > 0)
            return true;

        // 没有任务奖励
        return false;
    }

    /// <summary>
    /// 获取邀请好友任务完成奖励没领条数
    /// exp: 小红点数目
    /// </summary>
    /// <returns></returns>
    public static int GetInviteFriendTaskBounsCounts()
    {
        if (ME.user == null)
            return 0;

        int counts = 0;
        List<int> inviteTaskList = TaskMgr.GetStandardTasksByType(TaskConst.INVITE_TASK, ME.user);

        for (int i = 0; i < inviteTaskList.Count; i++)
        {
            CsvRow item = TaskMgr.GetTaskInfo(inviteTaskList[i]);
            if (item == null)
                continue;

            //  任务已完成
            if (TaskMgr.IsCompleted(ME.user, inviteTaskList[i]))
            {
                // 奖励还未领取
                if (!TaskMgr.isBounsReceived(ME.user, inviteTaskList[i]))
                    counts++;
            }
        }

        return counts;
    }

    /// <summary>
    /// 判断任务是否已经完成
    /// </summary>
    public static bool IsCompleted(Property user, int taskId)
    {
        // 获取配置信息
        CsvRow data = mTaskCsv.FindByKey(taskId);

        // 没有配置的任务
        if (data == null)
            return false;

        // 如果是日常
        if (data.Query<int> ("type") == TaskConst.DAILY_TASK)
        {
            // 获取玩家的daily_task信息
            LPCMapping taskMap = user.Query<LPCMapping> ("daily_task/task_map");

            // 没有任务数据则初始化
            if (taskMap == null || !taskMap [taskId].IsMapping)
                return false;

            // 返回该日常任务的状态
            return taskMap [taskId].AsMapping.GetValue<int> ("state") == TaskConst.COMPLETED_STATE;
        }
        else if (data.Query<int> ("type") == TaskConst.RESEARCH_TASK)
        {
            // 获取普通主线任务
            LPCArray complete = user.Query<LPCArray> ("research_task/complete");

            if (complete == null)
                return false;

            return complete.IndexOf (taskId) > -1;
        }
        else
        {
            // 获取普通主线任务
            LPCArray complete = user.Query<LPCArray> ("task/complete");

            // 取不到任何记录肯定是未完成
            if (complete == null)
                return false;

            // 计算当前任务的存储位置
            int flag = data.Query<int> ("flag");
            int index = flag / 31;

            // 当前分配的长度已经足够，说明任务还没有完成过
            if (complete.Count <= index)
                return false;

            // 计算偏移量
            int offset = 1 << (flag % 31);

            // 如果没有完成不处理
            if ((complete [index].AsInt & offset) == 0)
                return false;

            // 检测一下是否已经完成
            return true;
        }
    }

    /// <summary>
    /// 获取领取任务
    /// </summary>
    public static LPCMapping GetAssignTasks(Property user)
    {
        // 获取玩家身上的分配任务数据
        LPCValue assignTask = user.Query<LPCValue>("assign_task");

        // 没有任务数据
        if (assignTask == null || !assignTask.IsMapping)
            return LPCMapping.Empty;

        // 返回当前领取任务数据
        return assignTask.AsMapping;
    }

    /// <summary>
    /// 获取任务进度
    /// </summary>
    public static LPCValue GetTaskProgress(Property user, int taskId)
    {
        // 玩家对象不存在
        if (user == null)
            return LPCValue.Create();

        // 获取配置信息
        CsvRow data = mTaskCsv.FindByKey(taskId);

        // 没有配置的任务
        if (data == null)
            return LPCValue.Create();

        // 返回任务进度
        if (IsCompleted(user, taskId))
        {
            // 任务进度获取脚本
            LPCValue scriptNo = data.Query<LPCValue>("get_progerss_script");
            if (scriptNo == null || !scriptNo.IsInt)
                return LPCValue.Create();

            return ScriptMgr.Call(scriptNo.AsInt, user, data) as LPCValue;
        }

        // 获取任务进度信息
        LPCMapping taskMap = LPCMapping.Empty;
        if (data.Query<int>("type") == TaskConst.DAILY_TASK)
            taskMap = user.Query<LPCMapping>("daily_task/task_map");
        else if (data.Query<int>("type") == TaskConst.RESEARCH_TASK)
            taskMap = user.Query<LPCMapping>("research_task/task_map");
        else
            taskMap = user.Query<LPCMapping>("task/task_map");

        // 没有任务数据则初始化
        if (taskMap == null || !taskMap.ContainsKey(taskId)
            || !taskMap[taskId].IsMapping)
            return LPCValue.Create();

        // 获取任务进度
        return taskMap[taskId].AsMapping.GetValue<LPCValue>("progress", LPCValue.Create());
    }

    /// <summary>
    /// 获取任务具体描述
    /// </summary>
    /// <returns>The task desc.</returns>
    /// <param name="user">User.</param>
    /// <param name="taskId">Task identifier.</param>
    public static string GetTaskDesc(Property user, int taskId)
    {
        CsvRow taskData = GetTaskInfo(taskId);

        if(taskData == null)
            return string.Empty;

        LPCValue desc = taskData.Query<LPCValue>("desc");

        if(desc.IsString)
            return LocalizationMgr.Get(desc.AsString);
        else if(desc.IsInt)
        {
            return (string)ScriptMgr.Call(desc.AsInt, user, taskId);
        }

        return string.Empty;
    }

    /// <summary>
    /// 获取奖励列表
    /// </summary>
    /// <returns>The bonus.</returns>
    public static LPCArray GetBonus(Property user, int taskId)
    {
        CsvRow taskData = GetTaskInfo(taskId);

        if(taskData == null)
            return LPCArray.Empty;

        int scriptNo = taskData.Query<int>("bonus_script");

        if(scriptNo <= 0)
            return taskData.Query<LPCArray>("bonus_args");

        return (LPCArray)ScriptMgr.Call(scriptNo, user, taskData.Query<LPCArray>("bonus_args"));
    }

    /// <summary>
    /// 获取任务配置信息
    /// </summary>
    public static CsvRow GetTaskInfo(int taskId)
    {
        return mTaskCsv.FindByKey(taskId);
    }

    /// <summary>
    /// 获取任务数据
    /// </summary>
    /// <returns>The task data by type.</returns>
    /// <param name="type">Type.</param>
    public static List<int> GetStandardTasksByType(int type, Property user)
    {
        List<int> taskData = new List<int>();

        // 每日任务查询存储在玩家身上的数据
        if(type == TaskConst.DAILY_TASK)
        {
            LPCMapping taskMap = user.Query<LPCMapping>("daily_task/task_map");

            foreach(int task_id in taskMap.Keys)
                taskData.Add(task_id);

            // 对数据排序
            taskData = SortTaskData(user ,type ,taskData);
        }
        else
        {
            // 主线任务直接取配置表中的信息
            taskData = SortTaskData(user ,type, GetTasksData(user, type));
        }

        return taskData;
    }

    /// <summary>
    /// Checks the can receive bonus.
    /// </summary>
    /// <returns><c>true</c>, if can receive bonus was checked, <c>false</c> otherwise.</returns>
    /// <param name="taskId">Task identifier.</param>
    public static bool CheckCanReceiveBonus(Property user, int taskId)
    {
        CsvRow data = GetTaskInfo(taskId);

        if(data == null)
            return false;

        int scriptNo = data.Query<int>("check_bonus_script");
        if(scriptNo > 0)
            return (bool)ScriptMgr.Call(scriptNo, user);

        return true;
    }

    /// <summary>
    /// 完成任务
    /// </summary>
    /// 该接口不做任何检测（如果需要调用完成任务，需要自己检测完成后在调用该接口）
    public static void CompleteTask(Property user, int taskId)
    {
        // 获取任务配置表
        CsvRow taskData = GetTaskInfo(taskId);

        // 该任务没有配置过不处理
        // 该任务不是客户端任务
        if(taskData == null || taskData.Query<int>("is_client") == 0)
            return;

        // 如果是研究任务
        if (taskData.Query<int> ("type") == TaskConst.RESEARCH_TASK)
            return;

        LPCMapping taskMap = LPCMapping.Empty;

        // 如果是日常
        if (taskData.Query<int>("type") == TaskConst.DAILY_TASK)
        {
            // 获取玩家的daily_task信息
            taskMap = user.Query<LPCMapping>("daily_task/task_map");

            // 没有任务
            if (taskMap == null || ! taskMap.ContainsKey(taskId))
                return;

            // 标识任务的状态
            taskMap[taskId].AsMapping.Add("state", TaskConst.COMPLETED_STATE);
            user.Set("daily_task/task_map", LPCValue.Create(taskMap));

            // 添加可领取奖励列表
            LPCArray bonusList = user.Query<LPCArray>("daily_task/bonus");
            if (bonusList == null)
                bonusList = new LPCArray(taskId);
            else
            {
                // 已经在可以领取列表中
                if (bonusList.IndexOf(taskId) != -1)
                    return;

                // 添加可以领取奖励列表
                bonusList.Add(taskId);
            }

            // 记录奖励列表
            user.Set("daily_task/bonus", LPCValue.Create(bonusList));
        }
        else
        {
            // 获取玩家的任务数据
            LPCValue task = user.Query<LPCValue>("task");
            if (task == null || ! task.IsMapping)
                return;

            // 获取任务数据
            taskMap = task.AsMapping.GetValue<LPCMapping>("task_map");

            // 没有任务
            if (taskMap == null || ! taskMap.ContainsKey(taskId))
                return;

            // 删除任务数据
            taskMap.Remove(taskId);

            // 增加bonus task_id
            LPCArray bonus = task.AsMapping.GetValue<LPCArray>("bonus");
            if (bonus == null)
                bonus = new LPCArray(taskId);
            else
            {
                if (bonus.IndexOf(taskId) == -1)
                    bonus.Add(taskId);
            }

            // 重置bonus
            task.AsMapping.Add("bonus", bonus);

            // 取玩家身上的任务记录
            LPCArray complete = task.AsMapping.GetValue<LPCArray>("complete");
            if (complete == null)
                complete = LPCArray.Empty;

            // 获取完成成就数据长度
            int lenth = complete.Count;

            // 计算当前任务的存储位置
            int flag = taskData.Query<int>("flag");
            int index = flag / 31;

            // 计算偏移量
            int offset = 1 << (flag % 31);

            // 当前分配的buffer长度已经足够，直接返回
            if (lenth > index)
            {
                // 每一个数字只能表示31位的整数
                complete[index].AsInt = (int) complete[index].AsInt | offset;
            } else
            {
                // 数据位数不够增加长度
                int allocateLen = (int)index - lenth + 1;
                for (int i = 0; i < allocateLen; i++)
                    complete.Add(0);

                // 重新赋值
                complete[index].AsInt = (int) complete[index].AsInt | offset;
            }

            // 重置bonus
            task.AsMapping.Add("complete", complete);

            // 重置已经完成任务数据
            user.Set("task", task);
        }
    }

    // 更新任务进度
    public static void UpdateTaskProgress(Property user, int taskId, int progress)
    {
        // 获取任务配置表
        CsvRow taskData = GetTaskInfo(taskId);

        // 该任务没有配置过不处理
        // 该任务不是客户端任务
        if(taskData == null || taskData.Query<int>("is_client") == 0)
            return;

        // 如果是研究任务
        if (taskData.Query<int> ("type") == TaskConst.RESEARCH_TASK)
            return;

        LPCMapping taskMap = LPCMapping.Empty;

        // 如果是日常
        if (taskData.Query<int>("type") == TaskConst.DAILY_TASK)
            taskMap = user.Query<LPCMapping>("daily_task/task_map");
        else
            taskMap = user.Query<LPCMapping>("task/task_map");

        // 进度更新addMap
        LPCMapping addMap = LPCMapping.Empty;
        string userRid = user.GetRid();
        if (mCacheTaskProgressMap.ContainsKey(userRid))
            addMap = mCacheTaskProgressMap[userRid];
        else
            mCacheTaskProgressMap.Add(userRid, addMap);

        // 没有任务数据则初始化
        if (taskMap == null || ! taskMap.ContainsKey(taskId))
        {
            LPCMapping data = LPCMapping.Empty;
            data.Add("state", TaskConst.ASSIGN_STATE);
            data.Add("progress", progress);

            // 添加任务数据
            taskMap.Add(taskId, data);

            // 记录任务更新进度
            addMap.Add(taskId, addMap.GetValue<int>(taskId) + progress);
        }
        else
        {
            // 记录任务更新进度
            int addProgress = progress - taskMap[taskId].AsMapping.GetValue<int>("progress");
            if (addProgress > 0)
                addMap.Add(taskId, addMap.GetValue<int>(taskId) + progress);

            // 重置本地任务进度数据
            taskMap[taskId].AsMapping.Add("progress", progress);
        }

        // 更新玩家数据
        if (taskData.Query<int>("type") == TaskConst.DAILY_TASK)
            user.Set("daily_task/task_map", LPCValue.Create(taskMap));
        else
            user.Set("task/task_map", LPCValue.Create(taskMap));

        // 如果在副本中，则需要等到副本通关是统一通知服务器更新任务进度
        // 否则立即通知服务器更新任务进度
        if (InstanceMgr.IsInInstance(user))
            return;

        // 发送缓存的任务更新进度
        DoSendCacheTaskProgress(user);
    }

    /// <summary>
    /// Dos the update task progress.
    /// </summary>
    public static void DoSendCacheTaskProgress(Property user)
    {
        string userRid = user.GetRid();

        // 没有缓存数据
        if (! mCacheTaskProgressMap.ContainsKey(userRid))
            return;

        // 获取缓存的数据
        LPCMapping progressMap = mCacheTaskProgressMap[userRid];

        // 删除数据
        mCacheTaskProgressMap.Clear();

        // 没有缓存数据
        if (progressMap.Count == 0)
            return;

        // 通知服务器成就进度更新
        Operation.CmdUpdateTaskProgress.Go(progressMap);
    }

    /// <summary>
    /// 检测是否可以完成任务
    /// </summary>
    public static bool CheckCompleteTask(Property user, int taskId)
    {
        // 获取任务配置表
        CsvRow taskData = GetTaskInfo(taskId);

        // 该任务没有配置过不处理
        // 该任务不是客户端任务
        if(taskData == null || taskData.Query<int>("is_client") == 0)
            return false;

        // 如果任务已经完成
        if (IsCompleted(user, taskId))
            return false;

        // 任务未解锁不能完成任务
        if (! isUnlocked(user, taskId))
            return false;

        // 获取完成任务规则，如果规则限制不能完任务不能完成
        string complete = taskData.Query<string>("complete");

        // 调用子模块
        Task mod;
        if (! mTaskEntryMap.TryGetValue(complete, out mod))
            return false;

        // 子模块处理不能完成成就
        return mod.CheckCompleteTask(user, taskId);
    }

    /// <summary>
    /// 根据完成类型获取成就列表
    /// </summary>
    public static List<int> GetTaskList(string completeRule)
    {
        if (mCompleteRuleMap.ContainsKey(completeRule))
            return mCompleteRuleMap [completeRule];

        // 没有返回空
        return new List<int>();
    }

    /// <summary>
    /// 获取副本对应的通关类型的主线任务
    /// </summary>
    /// <returns>The assign clearance.</returns>
    /// <param name="instance">Instance.</param>
    public static int GetAssignClearanceTask(string instance)
    {
        if (!mAssignClearanceMap.ContainsKey(instance))
            return -1;

        return mAssignClearanceMap[instance];
    }

    /// <summary>
    /// 是否有新的成长任务（指的是没有查看过的任务）
    /// </summary>
    public static bool HasNewGrowthTask(Property who, int type)
    {
        if (who == null)
            return false;

        LPCArray list = LPCArray.Empty;

        LPCValue v = OptionMgr.GetOption(who, "task_view_data");
        if (v != null && v.IsArray)
            list = v.AsArray;

        List<int> taskList = TaskMgr.GetTasksByType(type);
        for (int i = 0; i < taskList.Count; i++)
        {
            // 未解锁
            if (! TaskMgr.isUnlocked(who, taskList[i]))
                continue;

            // 奖励已经领取
            if (TaskMgr.isBounsReceived(who, taskList[i]))
                continue;

            if (list.IndexOf(taskList[i]) == -1)
                return true;
        }

        return false;
    }

    #endregion
}
