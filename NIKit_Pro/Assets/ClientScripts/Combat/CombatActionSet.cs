/// <summary>
/// CombatActionSet.cs
/// Created by wangxw 2014-11-06
/// 行为集合
/// 一般来说以及能为单位
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class CombatActionSet
{
    #region 成员变量

    // 绑定的角色对象
    private CombatActor mActor = null;

    // 持有的资源数据
    private ActionSetData mData = null;

    // 真实化后的动态数据
    private PropertiesParameter mPara = null;

    // 执行中的Action列表
    private List<ActionBase> mActionList = new List<ActionBase>();

    // 下一个待创建的Action节点编号
    private int mNextCreateIndex = 0;

    #endregion

    #region 属性

    // 节点是否已经被取消
    public bool IsCanceled { get; private set; }

    // 序列已经存活的时间, 用于判断节点的启动和关闭时间点
    // 参考了缩放因子之后的虚拟时间
    public float LiveTime { get; private set; }

    // 本序列的时间控制类型
    public int TimeType { get { return mData.SCType; } }

    // 本序列的当前时间缩放参数
    public float TimeScaleFactor { get { return mActor.TimeScaleMap[TimeType]; } }

    // 本序列的集合类型
    public int ASType { get { return mData.ASType; } }

    // 本序列的cookie编号
    public string Cookie { get; private set; }

    // 外部数据句柄
    public LPC.LPCMapping ExtraArgs { get; set; }

    // 本序列的数据名
    public string ActionSetDataName { get { return mData.Name; } }

    #endregion

    #region 内部函数

    /// <summary>
    /// 销毁Action列表
    /// </summary>
    private void DestroyActionList()
    {
        int index = 0;

        // 遍历当前全部action，逐个结束
        // 这个地方为什么需要使用这种循环方式，主要是在有些节点end中还需要创建其他节点
        // 由于创建的节点都是添加到mActionList结尾的位置，在后续的处理中会把新添加进来的action结束
        while (true)
        {
            // 节点已经遍历结束
            if (index >= mActionList.Count)
                break;

            // 获取节点
            ActionBase action = mActionList[index];

            // 执行节点结束操作
            if (action.IsStarted && !action.IsEnded)
                action.End(true);

            // 销毁节点
            CombatActionMgr.DestroyAction(action);

            // index++
            index++;
        }

        // 清空数据
        mActionList.Clear();
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 构造函数 <see cref="CombatActionSet"/> class.
    /// </summary>
    /// <param name="cookieValue">序列编号</param>
    /// <param name="actor">绑定的角色对象</param>
    /// <param name="data">资源数据</param>
    /// <param name="para">实时参数</param>
    public CombatActionSet(string cookieValue, CombatActor actor, ActionSetData data, PropertiesParameter para)
    {
        Cookie = cookieValue;
        mActor = actor;
        mData = data;
        mPara = para;
        IsCanceled = false;
        LiveTime = 0.0f;
        ExtraArgs = new LPC.LPCMapping();

        System.Diagnostics.Debug.Assert(mActionList.Count == 0);
    }

    /// <summary>
    /// 序列开始
    /// </summary>
    public void Start()
    {
        mNextCreateIndex = 0;
    }

    /// <summary>
    /// 序列结束
    /// </summary>
    public void End()
    {
        // 销毁战斗序列
        DestroyActionList();

        // 通知序列结束
        ACTION_SEQUENCE_END.Call(Cookie, ExtraArgs);

        // 发序列结束的事件
        LPCMapping args = new LPCMapping();
        args.Add("rid", mActor.ActorName);
        args.Add("cookie", Cookie);
        args.Add("is_cancel", IsCanceled ? 1 : 0);
        args.Append(ExtraArgs);
        EventMgr.FireEvent(EventMgrEventType.EVENT_SEQUENCE_END, MixedValue.NewMixedValue(args), true, true);
    }

    /// <summary>
    /// 序列取消
    /// </summary>
    public void Cancel()
    {
        IsCanceled = true;

        int index = 0;

        // Cancel操作就是结束掉所有可以取消的节点
        // 这个地方为什么需要使用这种循环方式，主要是在有些节点end中还需要创建其他节点
        // 由于创建的节点都是添加到mActionList结尾的位置，在后续的处理中会把新添加进来的action结束
        while (true)
        {
            // 节点已经遍历结束
            if (index >= mActionList.Count)
                break;

            // 获取节点action
            ActionBase action = mActionList[index];

            // 无需结束的节点，不用处理
            if (!action.StopWhenCancel)
            {
                index++;
                continue;
            }

            // 执行节点结束操作
            if (action.IsStarted && !action.IsEnded)
                action.End(true);

            // 销毁节点
            CombatActionMgr.DestroyAction(action);
            mActionList.RemoveAt(index);
        }
    }

    /// <summary>
    /// 是否需要结束本序列
    /// </summary>
    /// <returns><c>true</c>, if ended was shoulded, <c>false</c> otherwise.</returns>
    public bool ShouldEnded()
    {
        // 没有等待播放的节点
        // 正在播放的节点为空
        // EventActionDataList不能作为判断条件，原因如下
        // 事件触发都是依赖ActionDataList和mActionList，如果这两个满足条件了则必定需要结束
        return (mNextCreateIndex >= mData.ActionDataList.Count) && (mActionList.Count == 0);
    }

    /// <summary>
    /// 驱动战斗序列
    /// </summary>
    /// <param name="info">Info.</param>
    public void Update(TimeDeltaInfo info)
    {
        // 1. 驱动所有节点
        for (int index = mActionList.Count - 1; index >= 0; index--)
        {
            ActionBase action = mActionList[index];

            // 优先执行End()操作
            if (action.IsFinished && !action.IsEnded)
            {
                // 结束并销毁节点
                action.End();
                CombatActionMgr.DestroyAction(action);
                mActionList.RemoveAt(index);
                continue;
            }

            // 驱动节点
            action.Update(info);
        }

        // 2. 计时累加
        LiveTime += info.DeltaTime;

        // 3. 新节点创建, 只要时间到了统一创建
        while (true)
        {
            // 没有节点需要创建
            // 或者还没有达到节点创建时间
            if (mNextCreateIndex >= mData.ActionDataList.Count ||
                LiveTime < mData.ActionDataList[mNextCreateIndex].StartTime)
                break;

            // 没有被取消，或者该节点无视取消
            if (! IsCanceled || mData.ActionDataList[mNextCreateIndex].CreateWhenCanceled)
            {
                // 创建节点
                ActionBase action = CombatActionMgr.CreateAction(mActor, this, mData.ActionDataList[mNextCreateIndex], mPara);
                System.Diagnostics.Debug.Assert(action != null);
                mActionList.Add(action);
                action.Start();
            }

            // mNextCreateIndex索引累加1
            mNextCreateIndex++;
        }
    }

    /// <summary>
    /// Triggers the combat event.
    /// </summary>
    /// <param name="eventName">Event name.</param>
    public void TriggerCombatEvent(string eventName)
    {
        int index = 0;

        // 逐个节点触发事件
        do
        {
            // 节点已经遍历结束
            if (index >= mActionList.Count)
                break;

            // 获取节点
            ActionBase action = mActionList[index];

            // 节点触发事件
            action.TriggerCombatEvent(eventName);

            // index++
            index++;

        } while(true);
    }

    /// <summary>
    /// Triggers the event.
    /// </summary>
    /// <param name="eventName">Event name.</param>
    public void TriggerEvent(string eventName)
    {
        // 获取本事件对应的触发节点列表
        List<ActionData> list;
        if (!mData.EventActionDataList.TryGetValue(eventName, out list))
            return;

        // 遍历触发节点列表，创建之
        foreach (ActionData ad in list)
        {
            // 该节点已经cancel，并且设定为cancel之后不创建
            // 则直接跳过本节点
            if (IsCanceled && !ad.CreateWhenCanceled)
                continue;

            // 创建节点
            ActionBase action = CombatActionMgr.CreateAction(mActor, this, ad, mPara);
            System.Diagnostics.Debug.Assert(action != null);
            mActionList.Add(action);
            action.Start();
        }
    }

    #endregion
}
