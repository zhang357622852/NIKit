/// <summary>
/// ActionTransitionAnimation.cs
/// Created by zhaozy 2018-06-07
/// 动画行为节点
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class ActionTransitionAnimation : ActionBase
{
    #region 成员变量

    // 动画名
    private string mAnimationName = string.Empty;

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="actor">角色对象，允许为null.</param>
    /// <param name="actionSet">所属序列</param>
    /// <param name="para">属性数据，已引入静态属性.</param>
    public ActionTransitionAnimation(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        mAnimationName = para.GetProperty<string>("name", string.Empty);
    }

    /// <summary>
    /// 开始节点
    /// </summary>
    public override void Start()
    {
        // 调用基类Start
        base.Start();

        // 角色没有载入不处理
        if (! Actor.IsLoaded)
        {
            IsFinished = true;
            return;
        }

        // 对于动画名为空的情况，直接结束
        if (string.IsNullOrEmpty(mAnimationName))
        {
            IsFinished = true;
            return;
        }

        // 开始播放动画
        Actor.SetAnimation(mAnimationName, SpeedControlType.SCT_CONSTANT);

        // 标识已经结束
        IsFinished = true;
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
    }
}
