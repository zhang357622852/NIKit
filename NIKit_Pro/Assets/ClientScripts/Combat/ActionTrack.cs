/// <summary>
/// ActionTrack.cs
/// Created by zhaozy 2015-10-16
/// 角色ActionTrack
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ActionTrack : ActionBase
{
    #region 成员变量

    // track id
    private string mTrackName = string.Empty;

    // 位移终点位置
    private Vector3 mTargetPos = Vector3.one;
    private Vector3 mStartPos = Vector3.one;

    // 位移结束事件
    private string mEventName = string.Empty;

    // 位移时间
    private float mDuration = 0f;

    // 流逝时间
    private float mElapseTime = 0f;

    // track动画轨迹
    private TrackInfo mTrack;

    #endregion

    #region 内部接口

    /// <summary>
    /// 根据track驱动
    /// </summary>
    private void DoDrive()
    {
        // 根据mTrackAnimation计算适时位置
        float rate = mElapseTime / mDuration;

        // 计算位置差值
        Vector3 pos = mTrack.Evaluate(rate, mStartPos, mTargetPos);

        // 设置位置
        Actor.SetPosition(pos);

        // 设置阴影位置
        Vector3 offset = mTrack.Evaluate(rate);
        Actor.SetShadowPosition(new Vector3(pos.x, pos.y - offset.y, pos.z));
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="actor">角色对象，允许为null.</param>
    /// <param name="para">属性数据，已引入静态属性.</param>
    public ActionTrack(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        mTrackName = para.GetProperty<string>("track", string.Empty);
        mTargetPos = para.GetProperty<Vector3>("target_pos", Actor.GetPosition()) +
        para.GetProperty<Vector3>("offset", Vector3.zero);
        mStartPos = para.GetProperty<Vector3>("start_pos", Actor.GetPosition());
        mDuration = para.GetProperty<float>("duration", 0f);
        mEventName = para.GetProperty<string>("event", "TRACK_END");
    }

    /// <summary>
    /// 开始节点
    /// </summary>
    public override void Start()
    {
        base.Start();

        // 获取轨迹文件
        mTrack = TrackMgr.GetTrack(mTrackName);
        if (mTrack == null)
        {
            IsFinished = true;
            return;
        }
    }

    /// <summary>
    /// 结束节点
    /// </summary>
    /// <param name="isCancel">是否cancel方式结束</param>
    public override void End(bool isCancel = false)
    {
        // 调用基类的end
        base.End(isCancel);
    }

    /// <summary>
    /// 节点更新
    /// </summary>
    /// <param name="info">时间参数信息</param>
    public override void Update(TimeDeltaInfo info)
    {
        // 如果Actor
        if (Actor == null)
        {
            // 标识已经结束
            IsFinished = true;

            // 抛出eventName
            Actor.TriggerEvent(mEventName, ActionSet.Cookie);

            return;
        }

        // 累计流逝时间
        mElapseTime += info.DeltaTime;

        // track还没有结束
        DoDrive();

        // 抛出eventName
        if (!Game.FloatGreat(mElapseTime, mDuration))
            return;

        // 标识已经结束
        IsFinished = true;

        // 抛出eventName
        Actor.TriggerEvent(mEventName, ActionSet.Cookie);
    }

    #endregion
}
