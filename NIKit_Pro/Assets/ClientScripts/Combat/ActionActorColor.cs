/// <summary>
/// ActionActorColor.cs
/// Created by wangxw 2014-11-27
/// 角色变色节点
/// </summary>

using UnityEngine;
using System.Collections;

public class ActionActorColor : ActionBase
{
    #region 内部成员

    // 目标颜色
    private Color mColor = Color.white;

    // 目标变色类型
    private int mCCType = ColorChangerType.CCT_BRUSH;

    // 持续时间
    private float mDuration = 0f;

    // 节点已经存活时间
    private float mLiveTime = 0f;

    // 颜色id
    private string mColorID = string.Empty;

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="actor">角色对象，允许为null.</param>
    /// <param name="para">属性数据，已引入静态属性.</param>
    public ActionActorColor(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        mColor = para.GetProperty<Color>("color", Color.white);
        mDuration = para.GetProperty<float>("time");
        mCCType = para.GetProperty<int>("type", ColorChangerType.CCT_BASE);
    }

    /// <summary>
    /// 开始节点
    /// </summary>
    public override void Start()
    {
        base.Start();

        // 变色
        mColorID = Actor.PushColor(mColor, mCCType);
    }

    /// <summary>
    /// 结束节点
    /// </summary>
    /// <param name="isCancel">是否cancel方式结束</param>
    public override void End(bool isCancel = false)
    {
        // 恢复本id的变色
        Actor.PopColor(mColorID);

        // 调用base end接口
        base.End(isCancel);
    }

    /// <summary>
    /// 节点更新
    /// </summary>
    /// <param name="info">时间参数信息</param>
    public override void Update(TimeDeltaInfo info)
    {
        // 变色时间还没有结束
        if (mDuration < 0f)
            return;

        // 模型变色要使用原始时间，不受被攻击者的时间控制
        mLiveTime += info.SourceDeltaTime;
        if (mLiveTime >= mDuration)
            IsFinished = true;
    }
}
