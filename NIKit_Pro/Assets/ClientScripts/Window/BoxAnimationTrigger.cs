/// <summary>
/// BoxAnimationTrigger.cs
/// Created by fengsc 2016/07/14
/// 结算界面宝箱开启动画帧事件
/// </summary>
using UnityEngine;
using System.Collections;

public class BoxAnimationTrigger : MonoBehaviour
{
    public UITexture mCover;

    public UISprite mWhiteMask;

    public UILabel mBoxTips;

    bool mIsLoopFight = false;

    int mRemainTime = 0;

    bool mIsCountDown = false;

    float mLastTime = 0;

    /// <summary>
    ///动画控制器
    /// </summary>
    Animator mAnim;

    void Start()
    {
        // 获取当前副本rid
        string instanceId = ME.user.Query<string>("instance/id");

        mIsLoopFight = InstanceMgr.GetLoopFightByInstanceId(instanceId);

        mAnim = GetComponent<Animator>();

        mBoxTips.text = LocalizationMgr.Get("FightSettlementWnd_2");
    }

    void Update()
    {
        if (mIsCountDown)
        {
            if ((Time.realtimeSinceStartup > mLastTime + 1.0f))
            {
                mLastTime = Time.realtimeSinceStartup;
                CountDown();
            }
        }
    }

    void CountDown()
    {
        if (mRemainTime < 1)
        {
            // 开启宝箱
            mAnim.Play(CombatConfig.ANIMATION_BASE_LAYER + "open",
                CombatConfig.ANIMATION_BASE_LAYER_INEDX,
                0f);

            //点击开启宝箱关闭开启提示;
            mBoxTips.gameObject.SetActive(false);

            // 取消倒计时
            mIsCountDown = false;

            return;
        }

        mBoxTips.text = string.Format(LocalizationMgr.Get("FightSettlementWnd_14"), mRemainTime);

        mRemainTime--;
    }

    void OnFinished()
    {
        // 抛出宝箱开启完成的事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_BOX_OPEN_FINISH, null);
    }

    /// <summary>
    ///开启宝箱动画完成执行的事件
    /// </summary>
    public void OpenBoxTriggerEvent()
    {
        // 设置图片
        mCover.mainTexture = ResourceMgr.LoadTexture("Assets/Art/UI/Window/Background/OpenBox.png");

        //开宝箱的瞬间全屏闪一下
        mWhiteMask.GetComponent<UISprite>().alpha = 255;

        TweenAlpha alpha = mWhiteMask.gameObject.GetComponent<TweenAlpha>();

        alpha.ResetToBeginning();

        alpha.duration = 0.6f;

        if (mIsLoopFight)
            EventDelegate.Add(alpha.onFinished, OnFinished);

        alpha.enabled = true;
    }

    /// <summary>
    ///宝箱下落动画完成执行的事件
    /// </summary>
    public void FallBoxTriggerEvent()
    {
        mBoxTips.gameObject.SetActive(true);

        TweenAlpha alpha = mBoxTips.GetComponent<TweenAlpha>();

        if (mIsLoopFight)
        {
            mRemainTime = 3;

            // 开始倒计时
            mIsCountDown = true;

            alpha.style = UITweener.Style.Once;
        }
        else
        {
            alpha.style = UITweener.Style.PingPong;

            // 抛出宝箱下落动画播放完成的事件
            EventMgr.FireEvent(EventMgrEventType.EVENT_BOX_FALL_FINISH, null);
        }

        alpha.enabled = true;
    }
}
