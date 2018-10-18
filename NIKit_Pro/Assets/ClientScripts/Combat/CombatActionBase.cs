/// <summary>
/// CombatActionBase.cs
/// Created by wangxw 2014-11-07
/// 行为节点基类
/// </summary>

using UnityEngine;
using System.Collections;

public abstract class ActionBase
{
    #region 属性

    // 绑定角色
    public CombatActor Actor { get; private set; }

    // 绑定所属序列
    public CombatActionSet ActionSet { get; private set; }

    // 能否取消
    public bool StopWhenCancel { get; set; }

    // 节点是否已经start()
    public bool IsStarted { get; private set; }

    // 节点是否已经完成操作，等待下一帧调用End()
    public bool IsFinished { get; protected set; }

    // 节点是否已经end()
    public bool IsEnded { get; private set; }

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="actor">角色对象，允许为null.</param>
    /// <param name="actionSet">所属序列</param>
    /// <param name="para">属性数据，已引入静态属性.</param>
    public ActionBase(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
    {
        Actor = actor;
        ActionSet = actionSet;
        StopWhenCancel = para.RefActionData.StopWhenCancel;
        IsStarted = false;
        IsFinished = false;
        IsEnded = false;
    }

    /// <summary>
    /// 开始节点
    /// </summary>
    public virtual void Start()
    {
        IsStarted = true;
    }

    /// <summary>
    /// 结束节点
    /// </summary>
    /// <param name="isCancel">是否cancel方式结束</param>
    public virtual void End(bool isCancel = false)
    {
        IsEnded = true;
    }

    /// <summary>
    /// 结束节点
    /// </summary>
    /// <param name="isCancel">是否cancel方式结束</param>
    public virtual void TriggerCombatEvent(string eventName)
    {
    }

    /// <summary>
    /// 节点更新
    /// </summary>
    /// <param name="info">时间参数信息</param>
    public abstract void Update(TimeDeltaInfo info);
}
