/// <summary>
/// FriendPointWnd.cs
/// Created by fengsc 2017/02/09
/// 友情点数
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class FriendPointWnd : WindowBase<FriendPointWnd>
{
    public UILabel mValues;

    private TweenScale[] mTweenScale;

    string eventName = string.Empty;

    void OnEnable()
    {
        RedarwFP();

        RegisterEvent();
    }

    void Start()
    {
        // 获取所有的TweenScale动画组件
        mTweenScale = mValues.GetComponents<TweenScale>();
    }

    void OnDestroy()
    {
        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField(eventName);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        eventName = Game.GetUniqueName(gameObject.name);

        // 注册入场券变化回调;
        ME.user.dbase.RegisterTriggerField(eventName, new string[] { "fp" }, new CallBack(OnFieldChange));
    }

    /// <summary>
    /// 字段变化回调
    /// </summary>
    void OnFieldChange(object pare, params object[] param)
    {
        EventDelegate.Add(mTweenScale[0].onFinished, new EventDelegate.Callback(OnTweenScaleFinish));

        mTweenScale[0].PlayForward();

        mTweenScale[0].ResetToBeginning();
    }

    void OnTweenScaleFinish()
    {
        // 刷新友情点数
        RedarwFP();

        mTweenScale[1].PlayForward();

        mTweenScale[1].ResetToBeginning();
    }

    void RedarwFP()
    {
        //玩家对象不存在;
        if (ME.user == null)
        {
            mValues.text = string.Empty;
            return;
        }

        // 获取玩家竞技场入场券;
        mValues.text = ME.user.Query<int>("fp").ToString();
    }
}
