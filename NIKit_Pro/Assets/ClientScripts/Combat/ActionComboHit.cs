/// <summary>
/// ActionComboHit.cs
/// Created by zhaozy 2016-8-20
/// 连击打击命中节点
/// 立即触发，常用于近战
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class ActionComboHit : ActionBase
{
    #region 成员变量

    // 事件参数
    private int mHitIndex = 1;
    private LPCValue mDependHit = null;

    // 连击次数
    private int mComboTimes = 1;
    private int mCurComboTimes = 0;

    // 时间间隔
    private float mHitInterval = 0f;

    // 下一次命中剩余时间
    private float mNextHitRemainTime = 0f;

    // 命中目标脚本
    private int mHitEntityScript = -1;

    #endregion

    #region 内部函数

    /// <summary>
    /// 命中连击
    /// </summary>
    private void DoComboHit(TimeDeltaInfo info)
    {
        // 每隔一段时间定时收集目标命中
        mNextHitRemainTime = mNextHitRemainTime - info.DeltaTime;

        // 如果时间小于0
        if (mNextHitRemainTime > 0)
            return;

        // 重置下一次命中收集剩余时间
        mNextHitRemainTime = mHitInterval;

        // 组织参数
        LPCMapping args = new LPCMapping();
        args.Add("hit_index", mHitIndex);
        args.Add("combo_times", mCurComboTimes);
        args.Add("cookie", ActionSet.Cookie);
        args.Add("rid", Actor.ActorName);
        args.Append(ActionSet.ExtraArgs);

        // 添加依赖depend_hit信息
        if (mDependHit.IsInt || mDependHit.IsArray)
            args.Add("depend_hit", mDependHit);

        // 每个hit点计算命中目标
        if (mHitEntityScript != -1)
            args.Add("hit_entity_map", ScriptMgr.Call(mHitEntityScript, Actor.ActorName,
                ActionSet.Cookie, mHitIndex, mCurComboTimes, ActionSet.ExtraArgs));

        // 发送受创事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_HIT, MixedValue.NewMixedValue<LPCMapping>(args), true, true);

        // 抛出eventName
        Actor.TriggerEvent("COMBO_HIT", ActionSet.Cookie);

        // 连击次数增加1
        mCurComboTimes++;

        // 通知序列结束
        ACTION_HIT_END.Call(mHitIndex, ActionSet.Cookie, ActionSet.ExtraArgs);
    }

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="actor">角色对象，允许为null.</param>
    /// <param name="para">属性数据，已引入静态属性.</param>
    public ActionComboHit(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        mHitIndex = para.GetProperty<int>("hit_index", 1);
        mDependHit = para.GetProperty<LPCValue>("depend_hit", LPCValue.Create());
        mComboTimes = para.GetProperty<int>("combo_times", 1);
        mHitInterval = para.GetProperty<float>("hit_interval", 0f);
        mHitEntityScript = para.GetProperty<int>("hit_entity_script", -1);
    }

    /// <summary>
    /// 开始节点
    /// </summary>
    public override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// 结束节点
    /// </summary>
    /// <param name="isCancel">是否cancel方式结束</param>
    public override void End(bool isCancel = false)
    {
        base.End(isCancel);
    }

    /// <summary>
    /// 节点更新
    /// </summary>
    /// <param name="info">时间参数信息</param>
    public override void Update(TimeDeltaInfo info)
    {
        // 连击结束
        if (mCurComboTimes >= mComboTimes)
        {
            IsFinished = true;
            return;
        }

        // 执行连击处理
        DoComboHit(info);
    }
}
