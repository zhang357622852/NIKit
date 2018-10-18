/// <summary>
/// TaskWnd.cs
/// Created by lic 11/10/2016
/// 任务窗口
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class TaskWnd : WindowBase<TaskWnd>
{
    #region 成员变量

    // 标题
    public UILabel mTitle;

    // 签到
    public GameObject mSignBtn;
    public UILabel mSignLb;
    public GameObject mSignRedPoint;

    // toggle
    public GameObject[] mToggleBtns;
    public GameObject[] mToggleRedPoints;
    public UILabel[] mToggleLbs;

    public GameObject mCloseBtn;

    public TweenRotation mSignBgTweenRotation;

    public TweenScale mTweenScale;

    // 成长手册按钮
    public GameObject mGrowthManualBtn;
    public UILabel mGrowthManualBtnLb;

    public TweenRotation mManualEffect;

    public GameObject mManualRedPoint;

    public GameObject mDailyTaskWnd;

    public GameObject mAchieveTaskWnd;

    // 当前选中页面
    [HideInInspector]
    public int mCurPage = 0;

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        //初始化窗口
        InitWnd();

        // 检查是否有奖励可领取
        CheckHasBonus();

        // 设置位置
        SetTogglesLbPosition();

        // 刷新签到背景
        RefreshSignBg();

        // 刷新成长手册背景效果
        RefreshGrowthManualEffect();
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;

        for(int i = 0; i < mToggleBtns.Length; i++)
            UIEventListener.Get(mToggleBtns[i]).onClick = OnToggleBtn;

        UIEventListener.Get(mSignBtn).onClick = OnSignBtn;
        UIEventListener.Get(mGrowthManualBtn).onClick = OnClickGrowthManualBtn;

        EventMgr.RegisterEvent("TaskWnd", EventMgrEventType.EVENT_REFRESH_GROWTH_TASK_TIPS, OnRefeshGrowthTaskTips);

        // 解注册事件
        ME.user.dbase.RemoveTriggerField("TaskWnd");
        ME.user.dbase.RegisterTriggerField("TaskWnd", new string[] {"task"}, new CallBack (OnTaskChange));
        ME.user.dbase.RegisterTriggerField("TaskWnd", new string[] {"daily_task"}, new CallBack (OnDailyTaskChange));
        ME.user.dbase.RegisterTriggerField("TaskWnd", new string[] { "sign_bonus" }, new CallBack (OnSignBonusChange));

        if (mTweenScale == null)
            return;

        float scale = Game.CalcWndScale();

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    void OnRefeshGrowthTaskTips(int eventId, MixedValue para)
    {
        RefreshGrowthManualEffect();
    }

    /// <summary>
    /// tween动画播放完后回调
    /// </summary>
    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        mTitle.text = LocalizationMgr.Get("TaskWnd_1");
        mSignLb.text = LocalizationMgr.Get("TaskWnd_2");
        mToggleLbs[0].text = LocalizationMgr.Get("TaskWnd_3");
        mToggleLbs[1].text = LocalizationMgr.Get("TaskWnd_4");
        mToggleLbs[2].text = LocalizationMgr.Get("TaskWnd_5");

        mGrowthManualBtnLb.text = LocalizationMgr.Get("TaskWnd_11");

        mSignRedPoint.SetActive(CommonBonusMgr.HasNewSign(ME.user));

        if (mCurPage.Equals(TaskConst.DAILY_TASK))
        {
            mDailyTaskWnd.SetActive(true);
            mAchieveTaskWnd.SetActive(false);
        }
        else if(mCurPage.Equals(TaskConst.ACHIEVE_TASK))
        {
            mAchieveTaskWnd.SetActive(true);
            mDailyTaskWnd.SetActive(false);
        }
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("TaskWnd");

        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 移除属性字段关注回调
        ME.user.dbase.RemoveTriggerField("TaskWnd");
    }

    /// <summary>
    /// 成长手册按钮点击回调
    /// </summary>
    void OnClickGrowthManualBtn(GameObject go)
    {
        // 打开成长手册界面
        WindowMgr.OpenWnd(GrowthManualWnd.WndType);
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        WindowMgr.OpenWnd ("MainWnd");

        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 标签按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnToggleBtn(GameObject ob)
    {
        for(int i = 0; i < mToggleBtns.Length; i++)
        {
            if(mToggleBtns[i] != ob)
                continue;

            // 点击当前页面不响应
            if(i == mCurPage)
                continue;

            mCurPage = i;

            SetTogglesLbPosition();

            break;
        }

        if (mCurPage.Equals(TaskConst.DAILY_TASK))
        {
            mDailyTaskWnd.SetActive(true);
            mAchieveTaskWnd.SetActive(false);
        }
        else if(mCurPage.Equals(TaskConst.ACHIEVE_TASK))
        {
            mAchieveTaskWnd.SetActive(true);
            mDailyTaskWnd.SetActive(false);
        }
    }

    /// <summary>
    /// 任务item被点击
    /// </summary>
    void OnTaskItemClicked(GameObject ob)
    {
        GameObject wnd = WindowMgr.GetWindow(TaskInfoWnd.WndType);

        if (wnd == null)
            wnd = WindowMgr.CreateWindow(TaskInfoWnd.WndType, TaskInfoWnd.PrefebResource);

        if (wnd == null)
        {
            LogMgr.Trace("TaskInfoWnd打开失败");
            return;
        }

        // 显示邮件窗口
        WindowMgr.ShowWindow(wnd);

        wnd.GetComponent<TaskInfoWnd>().BindData(ob.GetComponent<TaskItemWnd>().TaskId);
    }

    /// <summary>
    /// task字段变化回调
    /// </summary>
    void OnTaskChange(object para, params object[] _params)
    {
        CheckHasBonus();

        RefreshGrowthManualEffect();
    }

    void OnSignBonusChange(object para, params object[] param)
    {
        mSignRedPoint.SetActive(CommonBonusMgr.HasNewSign(ME.user));

        RefreshSignBg();
    }

    /// <summary>
    /// DailyTask字段变化回调
    /// </summary>
    void OnDailyTaskChange(object para, params object[] _params)
    {
        CheckHasBonus();
    }

    /// <summary>
    /// 签到被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnSignBtn(GameObject ob)
    {
        GameObject wnd = WindowMgr.GetWindow(SignWnd.WndType);

        if (wnd == null)
            wnd = WindowMgr.CreateWindow(SignWnd.WndType, SignWnd.PrefebResource);

        if (wnd == null)
        {
            LogMgr.Trace("SignWnd打开失败");
            return;
        }

        // 显示邮件窗口
        WindowMgr.ShowWindow(wnd);
    }

    /// <summary>
    /// 刷新签到背景
    /// </summary>
    void RefreshSignBg()
    {
        if (CommonBonusMgr.HasNewSign(ME.user))
        {
            mSignBgTweenRotation.PlayForward();
            mSignBgTweenRotation.ResetToBeginning();
        }
        else
        {
            mSignBgTweenRotation.gameObject.SetActive(false);
        }
    }

    /// <summary>
    ///刷新成长手册背景效果
    /// </summary>
    void RefreshGrowthManualEffect()
    {
        if (TaskMgr.CheckHasBonus(ME.user, TaskConst.GROWTH_TASK_LEVEL_STAR) ||
            TaskMgr.CheckHasBonus(ME.user, TaskConst.GROWTH_TASK_SUIT_INTENSIFY) ||
            TaskMgr.CheckHasBonus(ME.user, TaskConst.GROWTH_TASK_AWAKE) ||
            TaskMgr.CheckHasBonus(ME.user, TaskConst.GROWTH_TASK_ARENA) ||
            TaskMgr.CheckHasBonus(ME.user, TaskConst.GROWTH_TASK))
        {
            mManualEffect.PlayForward();
            mManualEffect.ResetToBeginning();

            mManualEffect.gameObject.SetActive(true);

            mManualRedPoint.SetActive(true);
        }
        else
        {
            mManualEffect.gameObject.SetActive(false);

            mManualRedPoint.SetActive(false);
        }
    }

    /// <summary>
    /// 刷新当前页面
    /// </summary>
    /// <param name="type">Type.</param>
    void Redraw(bool resetPosition = false)
    {
    }

    /// <summary>
    /// 检查是否有奖励未领取
    /// </summary>
    void CheckHasBonus()
    {
        // 每日任务红点提示
        mToggleRedPoints[0].SetActive(TaskMgr.CheckHasBonus(ME.user, TaskConst.DAILY_TASK));

        mToggleRedPoints[1].SetActive(TaskMgr.CheckHasBonus(ME.user, TaskConst.CHALLENGE_TASK));

        if (TaskMgr.CheckHasBonus(ME.user, TaskConst.ACHIEVE_TASK)
            || TaskMgr.CheckHasBonus(ME.user, TaskConst.ADVEN_TASK)
            || TaskMgr.CheckHasBonus(ME.user, TaskConst.PET_TASK)
            || TaskMgr.CheckHasBonus(ME.user, TaskConst.EQPT_TASK)
            || TaskMgr.CheckHasBonus(ME.user, TaskConst.ARENA_TASK))
            mToggleRedPoints[2].SetActive(true);
        else
            mToggleRedPoints[2].SetActive(false);
    }

    /// <summary>
    /// 设置toggle文本位置
    /// </summary>
    void SetTogglesLbPosition()
    {
        for(int i = 0; i < mToggleLbs.Length; i++)
        {
            // 点击当前页面不响应
            if(i == mCurPage)
                mToggleLbs[i].transform.localPosition = new Vector3(-3.8f, 8f, 0f);
            else
                mToggleLbs[i].transform.localPosition = new Vector3(-3.8f, 4f, 0f);
        }
    }

    #endregion
}
