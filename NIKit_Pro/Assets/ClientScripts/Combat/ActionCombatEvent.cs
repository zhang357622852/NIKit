/// <summary>
/// ActionCombatEvent.cs
/// Created by zhaozy 2018-06-06
/// 战斗系统内部触发事件触发点
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class ActionCombatEvent : ActionBase
{
    #region 成员变量

    // 事件类型
    private string mEventName;
    private string mEventCookie;

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="actor">角色对象，允许为null.</param>
    /// <param name="para">属性数据，已引入静态属性.</param>
    public ActionCombatEvent(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        mEventName = para.GetProperty<string>("event", string.Empty);
        mEventCookie = para.GetProperty<string>("cookie", string.Empty);
    }

    /// <summary>
    /// 开始节点
    /// </summary>
    public override void Start()
    {
        base.Start();

        // 抛出战斗系统内部事件
        if (! string.IsNullOrEmpty(mEventName))
            Actor.TriggerCombatEvent(mEventName, mEventCookie);

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
