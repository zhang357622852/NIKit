/// <summary>
/// CombatActor.cs
/// Created by wangxw 2014-11-05
/// 战斗角色对象
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class CombatActor : CombatObject
{
    #region 成员变量

    // 行为列表(AST_ACT)
    private List<CombatActionSet> mActionSetActList = new List<CombatActionSet>();

    // 行为列表(AST_STATUS)
    private List<CombatActionSet> mActionSetStatusList = new List<CombatActionSet>();

    // 缓存的行为序列
    private List<KeyValuePair<CacheActionType, object>> mCacheActionList = new List<KeyValuePair<CacheActionType, object>>();

    // 缓存操作类型
    private enum CacheActionType
    {
        DO_ACTION,
        CANCEL_ACTION_COOKIE,
        CANCEL_ACTION_TYPE,
        CANCEL_ACTION_NAME,
        STOP_ACTION_TYPE,
    }

    #endregion

    #region 属性

    // 战斗对象名
    public string ActorName { get; private set; }

    // 行为序列驱动标识
    public bool IsDriving { get; private set; }

    #endregion

    #region 内部函数

    /// <summary>
    /// 驱动指定列表
    /// </summary>
    /// <param name="List">List.</param>
    /// <param name="deltaTime">Delta time.</param>
    private void DriveList(List<CombatActionSet> List, float deltaTime)
    {
        int i = 0;

        do
        {
            // 数据已经处理结束
            if (i >= List.Count)
                break;

            // 获取CombatActionSet对象
            CombatActionSet cas = List[i];

            // 移除非法数据
            if (cas == null)
            {
                List.RemoveAt(i);
                continue;
            }

            // 优先尝试结束操作
            if (cas.ShouldEnded())
            {
                cas.End();
                List.RemoveAt(i);

                // DestroyActionSet
                CombatActionMgr.DestroyActionSet(cas);

                continue;
            }

            // 驱动一次cas
            cas.Update(new TimeDeltaInfo(deltaTime, TimeScaleMap[cas.TimeType]));

            // i++
            i++;

        } while(true);
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="name">角色名.</param>
    /// <param name="prefeb">预设资源路径.</param>
    public CombatActor(string name, string prefeb)
        : base(prefeb)
    {
        ActorName = name;
    }

    /// <summary>
    /// 加载角色对象
    /// 内部加载prefeb，创建GameObject
    /// </summary>
    public void Load()
    {
        base.Load(ActorName);
    }

    /// <summary>
    /// 驱动角色
    /// </summary>
    /// <param name="deltaTime">Delta time.</param>
    public void Update(float deltaTime)
    {
        // 更新TweenAlpha
        this.UpdateTweenAlpha(deltaTime);

        // 标识正在驱动中
        IsDriving = true;

        // 驱动行为列表
        DriveList(mActionSetActList, deltaTime);

        // 驱动状态列表
        DriveList(mActionSetStatusList, deltaTime);

        // 标识已经驱动结束
        IsDriving = false;

        do
        {
            // 缓存列表为空
            if (mCacheActionList.Count == 0)
                break;

            // 获取序列中第一个数据
            KeyValuePair<CacheActionType, object> cacheAction = mCacheActionList[0];
            mCacheActionList.RemoveAt(0);

            // 执行操作
            switch (cacheAction.Key)
            {
                case CacheActionType.DO_ACTION:
                    DoActionSet(cacheAction.Value as CombatActionSet);
                    break;

                case CacheActionType.CANCEL_ACTION_COOKIE:
                    List<object> para = cacheAction.Value as List<object>;
                    CancelActionSet((string)para[0], (int)para[1]);
                    break;

                case CacheActionType.CANCEL_ACTION_TYPE:
                    CancelActionSet((int)cacheAction.Value);
                    break;

                case CacheActionType.CANCEL_ACTION_NAME:
                    CancelActionSet((string)cacheAction.Value);
                    break;

                case CacheActionType.STOP_ACTION_TYPE:
                    StopActionSet((int)cacheAction.Value);
                    break;

                default:
                    break;
            }

        } while(true);
    }

    /// <summary>
    /// 驱动角色
    /// </summary>
    /// <param name="deltaTime">Delta time.</param>
    public bool GetActionSet(string cookie, out CombatActionSet actionSet)
    {
        // 查询主行为序列
        foreach (CombatActionSet cas in mActionSetActList)
        {
            // 不是需要查找数据
            if (!string.Equals(cookie, cas.Cookie))
                continue;

            actionSet = cas;
            return true;
        }

        // 查询副行为序列
        foreach (CombatActionSet cas in mActionSetStatusList)
        {
            // 不是需要查找数据
            if (!string.Equals(cookie, cas.Cookie))
                continue;

            actionSet = cas;
            return true;
        }

        // 没有找到actionSet
        actionSet = null;
        return false;
    }

    /// <summary>
    /// 驱动角色
    /// </summary>
    /// <param name="deltaTime">Delta time.</param>
    public void AddActionSetExtraArgs(string cookie, string key, LPCMapping args)
    {
        // 查询指定序列
        CombatActionSet actionSet = null;
        if (!GetActionSet(cookie, out actionSet))
            return;

        // 不包含该数据
        if (!actionSet.ExtraArgs.ContainsKey(key))
        {
            actionSet.ExtraArgs.Add(key, args);
            return;
        }

        if (actionSet.ExtraArgs[key] == null ||
            !actionSet.ExtraArgs[key].IsMapping)
        {
            actionSet.ExtraArgs.Add(key, args);
            return;
        }

        // 添加数据
        actionSet.ExtraArgs[key].AsMapping.Append(args);
    }

    /// <summary>
    /// 执行指定的行为序列
    /// </summary>
    public bool DoActionSet(CombatActionSet actionSet)
    {
        // 如果序列为null
        if (actionSet == null)
            return false;

        if (actionSet.ASType == ActionSetType.AST_ACT)
        {
            // 行为序列，保持唯一
            CancelActionSet(ActionSetType.AST_ACT);
            mActionSetActList.Add(actionSet);
        }
        else
        {
            // 状态序列，按cookie互斥
            CancelActionSet(actionSet.Cookie, ActionSetType.AST_STATUS);
            mActionSetStatusList.Add(actionSet);
        }

        // 创建成功，即时开启
        actionSet.Start();

        // 返回成功
        return true;
    }

    /// <summary>
    /// 执行指定的行为序列
    /// </summary>
    /// <param name="actionSetName">序列配置名</param>
    /// <param name="cookie">唯一标识cookie</param>
    /// <param name="args">脚本参数</param>
    public bool DoActionSet(string actionSetName, string cookie, LPCMapping args)
    {
        // 添加上Actor的名字
        args.Add("actor_name", ActorName);

        // 为本cookie操作创建一个ActionSet实例
        CombatActionSet actionSet = CombatActionMgr.CreateActionSet(actionSetName, cookie, this, args);
        if (actionSet == null)
            return false;

        // 如果正在驱动，不能修改数据，则放到缓存列表中等到驱动结束是处理
        if (IsDriving)
        {
            // 添加缓存数据
            mCacheActionList.Add(new KeyValuePair<CacheActionType, object>(CacheActionType.DO_ACTION, actionSet));
            return true;
        }

        // 添加行为序列
        return DoActionSet(actionSet);
    }

    /// <summary>
    /// 取消本角色所有技能序列
    /// </summary>
    public void CancelAllActionSet()
    {
        CancelActionSet(ActionSetType.AST_STATUS);
        CancelActionSet(ActionSetType.AST_ACT);
    }

    /// <summary>
    /// 取消本角色指定的技能行为序列
    /// 取消所有名为cookie的序列
    /// </summary>
    /// <param name="cookie">标识cookie</param>
    public void CancelActionSet(string cookie)
    {
        CancelActionSet(cookie, ActionSetType.AST_ACT);
        CancelActionSet(cookie, ActionSetType.AST_STATUS);
    }

    /// <summary>
    /// 结束本角色所有技能序列
    /// </summary>
    public void StopActionSet(int ast)
    {
        // 如果正在驱动，不能修改数据，则放到缓存列表中等到驱动结束是处理
        if (IsDriving)
        {
            // 添加缓存数据
            mCacheActionList.Add(new KeyValuePair<CacheActionType, object>(CacheActionType.STOP_ACTION_TYPE, ast));
            return;
        }

        // 获取行为列表
        List<CombatActionSet> list = (ast == ActionSetType.AST_ACT) ? mActionSetActList : mActionSetStatusList;

        // 将所有执行中的ActionSet进行End操作，行为列表
        foreach (CombatActionSet cas in list)
            cas.End();

        // 清除数据
        list.Clear();
    }

    /// <summary>
    /// 取消本角色指定的技能行为序列
    /// 只取消指定类型的cookie序列
    /// </summary>
    /// <param name="cookie">标识cookie</param>
    /// <param name="ast">ActionSetType类型过滤</param>
    public void CancelActionSet(string cookie, int ast)
    {
        // 如果正在驱动，不能修改数据，则放到缓存列表中等到驱动结束是处理
        if (IsDriving)
        {
            // 添加缓存数据
            mCacheActionList.Add(new KeyValuePair<CacheActionType, object>(CacheActionType.CANCEL_ACTION_COOKIE,
                    new List<object>() { cookie, ast }));
            return;
        }

        // 获取驱动序列
        List<CombatActionSet> list = (ast == ActionSetType.AST_ACT) ? mActionSetActList : mActionSetStatusList;

        // 查询指定序列
        CombatActionSet actionSet = null;
        foreach (CombatActionSet cas in list)
        {
            // 不是需要查找的CombatActionSet
            if (! string.Equals(cas.Cookie, cookie))
                continue;

            // 记录actionSet
            actionSet = cas;
            break;
        }

        // 没有找到相应的actionSet
        if (actionSet == null)
            return;

        // 取消节点播放，转给管理器驱动
        actionSet.Cancel();
        CombatActionMgr.TransferActionSet(actionSet);
        list.Remove(actionSet);
    }

    /// <summary>
    /// 删除指定类型列表中的所有ActionSet
    /// </summary>
    /// <param name="ast">ActionSetType 类型</param>
    public void CancelActionSet(int ast)
    {
        // 如果正在驱动，不能修改数据，则放到缓存列表中等到驱动结束是处理
        if (IsDriving)
        {
            // 添加缓存数据
            mCacheActionList.Add(new KeyValuePair<CacheActionType, object>(CacheActionType.CANCEL_ACTION_TYPE, ast));
            return;
        }

        List<CombatActionSet> list = (ast == ActionSetType.AST_ACT) ? mActionSetActList : mActionSetStatusList;

        // 将所有执行中的ActionSet进行取消操作，行为列表
        foreach (CombatActionSet cas in list)
        {
            cas.Cancel();
            CombatActionMgr.TransferActionSet(cas);
        }

        // 清除数据
        list.Clear();
    }

    /// <summary>
    /// 取消指定名称的行为序列
    /// </summary>
    public void CancelActionSetByName(string asName)
    {
        // 如果正在驱动，不能修改数据，则放到缓存列表中等到驱动结束是处理
        if (IsDriving)
        {
            // 添加缓存数据
            mCacheActionList.Add(new KeyValuePair<CacheActionType, object>(CacheActionType.CANCEL_ACTION_NAME, asName));
            return;
        }

        // 没有静态名称的索引，直接遍历，目前需求不多
        HashSet<string> asList = new HashSet<string>();

        // act列表
        foreach (CombatActionSet cas in mActionSetActList)
        {
            if (string.Equals(cas.ActionSetDataName, asName))
                asList.Add(cas.Cookie);
        }

        // statuc列表
        foreach (CombatActionSet cas in mActionSetStatusList)
        {
            if (string.Equals(cas.ActionSetDataName, asName))
                asList.Add(cas.Cookie);
        }

        // 遍历移除
        foreach (string cookie in asList)
            CancelActionSet(cookie);
    }

    /// <summary>
    /// Triggers the combat event.
    /// </summary>
    /// <param name="eventName">Event name.</param>
    /// <param name="cookie">Cookie.</param>
    public void TriggerCombatEvent(string eventName, string cookie)
    {
        // act列表
        foreach (CombatActionSet cas in mActionSetActList)
        {
            // cookie不一致
            if (! string.Equals(cookie, cas.Cookie))
                continue;

            // 触发事件
            cas.TriggerCombatEvent(eventName);
        }

        // statuc列表
        foreach (CombatActionSet cas in mActionSetStatusList)
        {
            // cookie不一致
            if (! string.Equals(cookie, cas.Cookie))
                continue;

            // 触发事件
            cas.TriggerCombatEvent(eventName);
        }
    }

    /// <summary>
    /// 触发器触发事件
    /// </summary>
    /// <param name="eventName">Event name.</param>
    public void TriggerEvent(string eventName, string cookie)
    {
        // 遍历所有ActionSet，通知事件触发

        // act列表
        foreach (CombatActionSet cas in mActionSetActList)
        {
            // cookie不一致
            if (! string.Equals(cookie, cas.Cookie))
                continue;

            // 触发事件
            cas.TriggerEvent(eventName);
        }

        // statuc列表
        foreach (CombatActionSet cas in mActionSetStatusList)
        {
            // cookie不一致
            if (! string.Equals(cookie, cas.Cookie))
                continue;

            // 触发事件
            cas.TriggerEvent(eventName);
        }
    }

    #endregion
}
