/// <summary>
/// ActionHit.cs
/// Created by wangxw 2014-12-01
/// 打击命中节点
/// 立即触发，常用于近战
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class ActionHit : ActionBase
{
    #region 成员变量

    // 事件参数
    private int mHitIndex = 1;
    private LPCValue mDependHit = null;

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="actor">角色对象，允许为null.</param>
    /// <param name="para">属性数据，已引入静态属性.</param>
    public ActionHit(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        mHitIndex = para.GetProperty<int>("hit_index", 1);
        mDependHit = para.GetProperty<LPCValue>("depend_hit", LPCValue.Create());
    }

    /// <summary>
    /// 开始节点
    /// </summary>
    public override void Start()
    {
        base.Start();

        // 组织参数
        LPCMapping args = new LPCMapping();
        args.Add("hit_index", mHitIndex);
        args.Add("cookie", ActionSet.Cookie);
        args.Add("rid", Actor.ActorName);
        args.Append(ActionSet.ExtraArgs);

        // 添加依赖depend_hit信息
        if (mDependHit.IsInt || mDependHit.IsArray)
            args.Add("depend_hit", mDependHit);

        // 发送受创事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_HIT, MixedValue.NewMixedValue<LPCMapping>(args), true, true);

        // 标识节点结束
        IsFinished = true;
    }

    /// <summary>
    /// 结束节点
    /// </summary>
    /// <param name="isCancel">是否cancel方式结束</param>
    public override void End(bool isCancel = false)
    {
        base.End(isCancel);

        // 通知序列结束
        ACTION_HIT_END.Call(mHitIndex, ActionSet.Cookie, ActionSet.ExtraArgs);
    }

    /// <summary>
    /// 节点更新
    /// </summary>
    /// <param name="info">时间参数信息</param>
    public override void Update(TimeDeltaInfo info)
    {
    }
}
