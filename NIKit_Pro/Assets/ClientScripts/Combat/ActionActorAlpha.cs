/// <summary>
/// ActionActorAlpha.cs
/// Created by wangxw 2014-12-9
/// 角色透明度
/// 指定时间内，平滑过度到目标透明度
/// </summary>

using UnityEngine;
using System.Collections;

public class ActionActorAlpha : ActionBase
{
    #region 成员变量

    // 变换的目标透明度
    private float mTargetAlpha = 1f;

    // 变换时间
    private float mChangeTime = 0f;

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="actor">角色对象，允许为null.</param>
    /// <param name="para">属性数据，已引入静态属性.</param>
    public ActionActorAlpha(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        mTargetAlpha = para.GetProperty<float>("target_alpha", 1f);
        mChangeTime = para.GetProperty<float>("change_time", 0f);
    }

    /// <summary>
    /// 开始节点
    /// </summary>
    public override void Start()
    {
        base.Start();

        // 设置Alpha渐变
        Actor.SetTweenAlpha(mTargetAlpha, mChangeTime);

        // 标识结束
        IsFinished = true;
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
    }
}
