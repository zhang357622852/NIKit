/// <summary>
/// ActionComboAnimation.cs
/// Created by zhaozy 2016-8-20
/// 连击动画行为节点
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ActionComboAnimation : ActionBase
{
    #region 成员变量

    // 动画名
    private string mAnimationName = string.Empty;
    private string[] mAnimationNameList;

    // 连击次数
    private int mComboTimes = 1;
    private int mTimes = 0;

    // 验证客户端需要用到的动画数据
    private LPCArray mAnimationEvents;
    private int mEventIndex = 0;
    private float mAnimationLength = 0f;
    private float mElapsedTime = 0f;

    #endregion

    #region 内部函数

    /// <summary>
    /// 开始动画播放
    /// </summary>
    /// <returns><c>true</c>, 播放成功, <c>false</c> 播放失败，需要结束.</returns>
    private void StartAnimation()
    {
        // 选择移动动画播放
        mAnimationName = mAnimationNameList[mTimes % mAnimationNameList.Length];

        // 累计播放次数
        mTimes++;

        // 直接播放
        Actor.SetAnimation(mAnimationName, ActionSet.TimeType);

        // 获取动画信息
        LPCMapping info = MonsterMgr.GetModelAnimationInfo(Actor.ModelId, mAnimationName);
        mAnimationEvents = info.GetValue<LPCArray>("event", LPCArray.Empty);
        mAnimationLength = info["length"].AsFloat;
        mElapsedTime = 0f;
        mEventIndex = 0;
    }

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="actor">角色对象，允许为null.</param>
    /// <param name="actionSet">所属序列</param>
    /// <param name="para">属性数据，已引入静态属性.</param>
    public ActionComboAnimation(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        // 获取配置动画名
        mAnimationNameList = para.GetProperty<string[]>("name");

        // 获取配置的连击次数
        mComboTimes = para.GetProperty<int>("combo_times", 1);
    }

    /// <summary>
    /// 开始节点
    /// </summary>
    public override void Start()
    {
        base.Start();

        // 角色还没有加载
        if (! Actor.IsLoaded)
        {
            IsFinished = true;
            return;
        }

        // 没有动画可以选择
        if (mAnimationNameList == null ||
            mAnimationNameList.Length == 0)
        {
            IsFinished = true;
            return;
        }

        // 开始播放动画
        StartAnimation();
    }

    /// <summary>
    /// 结束节点
    /// </summary>
    /// <param name="isCancel">是否cancel方式结束</param>
    public override void End(bool isCancel = false)
    {
        // 主动回到idle
        Actor.SetAnimation(CombatConfig.DEFAULT_PLAY, SpeedControlType.SCT_CONSTANT);

        // 结束战斗
        base.End(isCancel);

        // 抛出eventName
        Actor.TriggerEvent("COMBO_END", ActionSet.Cookie);
    }

    /// <summary>
    /// 节点更新
    /// </summary>
    /// <param name="info">时间参数信息</param>
    public override void Update(TimeDeltaInfo info)
    {
        // 如果当前动画已经被暂停，直接结束
        if (Actor.IsAnimationPaused())
        {
            IsFinished = true;
            return;
        }

        // 已经不是当前动画了，直接结束
        if (!Actor.IsPlayingAnimation(mAnimationName))
        {
            IsFinished = true;
            return;
        }

        // 每隔一段时间定时收集目标命中
        mElapsedTime = mElapsedTime + info.DeltaTime * Actor.GetSpeedFactor(ActionSet.TimeType);

        // 尝试触发动画事件
        do
        {
            // 没有事件需要触发了
            if (mEventIndex >= mAnimationEvents.Count)
                break;

            // 判断是否需要触发动画事件, 如果事件还不到不处理
            LPCArray eventData = mAnimationEvents[mEventIndex].AsArray;
            if (mElapsedTime < eventData[0].AsFloat)
                break;

            // mEventIndex++
            mEventIndex ++;

            // 触发事件
            Actor.TriggerEvent(eventData[1].AsString, ActionSet.Cookie);

        } while(true);

        // 还没结束，无需处理
        if (mElapsedTime < mAnimationLength)
            return;

        // 还没有到达指定的播放次数
        if (mComboTimes <= mTimes)
        {
            IsFinished = true;
            return;
        }

        // 播放动画
        StartAnimation();
    }
}
