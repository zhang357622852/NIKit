/// <summary>
/// ActionMove.cs
/// Created by wangxw 2014-11-25
/// 角色移动
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class ActionMove : ActionBase
{
    #region 成员变量

    // 移动目标位置，空间坐标
    private Vector3 mTargetPos = Vector3.zero;

    // 移动速度
    private float mNativeMoveSpeed = -1f;

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="actor">角色对象，允许为null.</param>
    /// <param name="para">属性数据，已引入静态属性.</param>
    public ActionMove(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        mTargetPos = para.GetProperty<Vector3>("target_pos", actor.GetPosition());
        mNativeMoveSpeed = para.GetProperty<float>("native_speed");
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
        // 主动回到idle
        Actor.SetAnimation(CombatConfig.DEFAULT_PLAY, SpeedControlType.SCT_CONSTANT);

        // 调用基类的end
        base.End(isCancel);

        // 到达目的地，给出事件
        LPCMapping para = LPCMapping.Empty;
        para.Add("rid", Actor.ActorName);
        para.Append(ActionSet.ExtraArgs);

        // 抛出事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_MOVE_ARRIVED, MixedValue.NewMixedValue<LPCMapping>(para), true, true);
    }

    /// <summary>
    /// 节点更新
    /// </summary>
    /// <param name="info">时间参数信息</param>
    public override void Update(TimeDeltaInfo info)
    {
        if (Actor == null)
        {
            IsFinished = true;
            return;
        }

        // 本帧移动的距离
        // info.DeltaTime已经是速度缩放之后的值了，直接乘基准常量即可
        // 流失的时间 * 速度百分比 * 速度基准单位
        float distance;
        if (mNativeMoveSpeed > 0)
            distance = info.DeltaTime * mNativeMoveSpeed;
        else
            distance = info.DeltaTime * Actor.MoveSpeed * Actor.TimeScaleMap[SpeedControlType.SCT_MOVE_RATE];

        // 到达目标
        Vector3 currentPos = Actor.GetPosition();
        if ((mTargetPos - currentPos).sqrMagnitude < (distance * distance))
        {
            Actor.SetPosition(mTargetPos);
            IsFinished = true;
        }
        else
        {
            // 继续移动
            Actor.SetPosition(Vector3.MoveTowards(currentPos, mTargetPos, distance));
        }
    }
}
