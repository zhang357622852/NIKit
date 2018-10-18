/// <summary>
/// ActionEvent.cs
/// Created by wangxw 2014-12-2
/// 事件触发点
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class ActionEvent : ActionBase
{
    #region 成员变量

    // 事件类型
    private int mEventType;

    // 附加数据
    private LPCMapping mAddedArgs;

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="actor">角色对象，允许为null.</param>
    /// <param name="para">属性数据，已引入静态属性.</param>
    public ActionEvent(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        mEventType = para.GetProperty<int>("event_type", EventMgrEventType.EVENT_NULL);
        mAddedArgs = para.GetProperty<LPCValue>("added_args").AsMapping;
    }

    /// <summary>
    /// 开始节点
    /// </summary>
    public override void Start()
    {
        base.Start();

        // 直接发送事件即可
        LPCMapping args = new LPCMapping();
        args.Add("rid", LPCValue.Create(Actor.ActorName));
        args.Add("cookie", LPCValue.Create(ActionSet.Cookie));
        args.Add("is_cancel", LPCValue.Create(0));
        args.Append(ActionSet.ExtraArgs);
        args.Append(mAddedArgs);
        EventMgr.FireEvent(mEventType, MixedValue.NewMixedValue(args), true, true);

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
