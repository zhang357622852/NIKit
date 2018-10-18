/// <summary>
/// ActionAnimation.cs
/// Created by wangxw 2014-11-18
/// 动画行为节点
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class ActionAnimation : ActionBase
{
    #region 成员变量

    // 动画名
    private string mAnimationName = string.Empty;
    private string mTransitionAnimation = string.Empty;

    // 同名切换重播时间点
    // 如果动画树正好也在播同一个动作，再次播放的时候从这个时间点开始播
    // 此变量<0的时候，表示不重播，一般用于循环状态动画
    private float mResetTimePosition = 0.0f;

    // 是否循环
    private bool mIsLoop = false;

    // 是否随机起点，常用于循环动画
    private bool mIsRandomStart = false;

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
        if (Actor.IsPlayingAnimation(mAnimationName))
        {
            // 如果动画树正在播放同一个动作
            // 且重播时间点有效，则定点重播；
            // 反之，无需处理
            if (!mIsLoop && mResetTimePosition >= 0)
                Actor.SetAnimation(mAnimationName, ActionSet.TimeType, mResetTimePosition);
        }
        else
        {
            if (mIsRandomStart)
                // 随机起点播放起点
                Actor.SetAnimation(mAnimationName, ActionSet.TimeType, Random.Range(0f, 1f));
            else
                // 直接播放
                Actor.SetAnimation(mAnimationName, ActionSet.TimeType);
        }

        // 获取动画信息
        LPCMapping info = MonsterMgr.GetModelAnimationInfo(Actor.ModelId, mAnimationName);
        mAnimationEvents = info.GetValue<LPCArray>("event", LPCArray.Empty);
        mAnimationLength = info["length"].AsFloat;
        mElapsedTime = 0f;
        mEventIndex = 0;

        // 后续的操作需要延后到StartAnimation()的下一帧执行，
        // 否则Unity的延迟处理导致数据字段尚未更新
    }

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="actor">角色对象，允许为null.</param>
    /// <param name="actionSet">所属序列</param>
    /// <param name="para">属性数据，已引入静态属性.</param>
    public ActionAnimation(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        mAnimationName = para.GetProperty<string>("name", string.Empty);
        mResetTimePosition = para.GetProperty<float>("reset_time_position", 0.0f);
        mIsLoop = para.GetProperty<bool>("is_loop", false);
        mIsRandomStart = para.GetProperty<bool>("is_random_start", mIsLoop); // 循环动作默认随机起点
        mTransitionAnimation = para.GetProperty<string>("transition_animation", CombatConfig.DEFAULT_PLAY);
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
        StartAnimation();
    }

    /// <summary>
    /// 结束节点
    /// </summary>
    /// <param name="isCancel">是否cancel方式结束</param>
    public override void End(bool isCancel = false)
    {
        // 如果还是当前播放动画则切换为该动作结束动画
        if (Actor.IsPlayingAnimation(mAnimationName) &&
            ! string.IsNullOrEmpty(mTransitionAnimation))
            Actor.SetAnimation(mTransitionAnimation, SpeedControlType.SCT_CONSTANT);

        // 调用基类的end
        base.End(isCancel);
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
        if (! Actor.IsPlayingAnimation(mAnimationName))
        {
            IsFinished = true;
            return;
        }

        // 获取动画播放时间长度
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

        // 循环动画也无需处理什么，等到cancel操作之后自然会结束节点
        if (mIsLoop)
        {
            // 如果是循环动画需要重置mElapsedTime时间
            mElapsedTime -= mAnimationLength;
            return;
        }

        // 结束本动画节点
        IsFinished = true;
    }
}
