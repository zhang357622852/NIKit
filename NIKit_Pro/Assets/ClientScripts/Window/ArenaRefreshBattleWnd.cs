/// <summary>
/// ArenaRefreshBattleWnd.cs
/// Created by fengsc 2016/09/26
/// 竞技场排位战刷新对战列表窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ArenaRefreshBattleWnd : WindowBase<ArenaRefreshBattleWnd>
{

    #region 成员变量

    public UILabel mTitle;
    // 窗口标题
    public GameObject mCloseBtn;
    // 关闭窗口按钮
    public UILabel mWinsEffectDesc;
    // 连胜效果描述

    public UILabel mNormalRefershDesc;
    // 普通刷新描述
    public GameObject mNormalRefreshBtn;
    // 普通刷新按钮
    public UILabel mNormalRefreshBtnLb;

    public UILabel mKeepGainDesc;
    // 维持增益刷新描述
    public GameObject mKeepGainBtn;
    // 维持增益刷新按钮
    public UILabel mKeepGainBtnLb;

    public UISprite mCostIcon;
    // 刷新消耗的物品图标
    public UILabel mCost;
    // 刷新需要的消耗
    public UILabel mRefreshTimer;
    // 普通刷新的时间间隔

    public GameObject mMask;

    public TweenScale mTweenScale;

    public TweenAlpha mTweenAlpha;

    LPCMapping mCostData = new LPCMapping();

    int mRemainTime = 0;
    // 剩余的刷新时间

    bool IsNormalRefresh = false;

    bool mIsCountDown = false;

    float mLastTime = 0;

    #endregion

    // Use this for initialization
    void Start()
    {
        // 初始化本地化文本;
        InitLocalText();

        // 注册事件
        RegisterEvent();

        // 绘制窗口
        Redraw();

        if (mTweenAlpha == null || mTweenScale == null)
            return;

        // 播放动画
        mTweenAlpha.PlayForward();

        mTweenScale.PlayForward();

        // 重置动画组件
        mTweenAlpha.ResetToBeginning();
        mTweenScale.ResetToBeginning();
    }

    void Update()
    {
        if (mIsCountDown)
        {
            if (Time.realtimeSinceStartup > mLastTime + 1)
            {
                mLastTime = Time.realtimeSinceStartup;

                RefreshCountDown();
            }
        }
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    void OnDisable()
    {
        // 解注册字段变化的事件
        if (ME.user != null)
            ME.user.dbase.RemoveTriggerField("ArenaRefreshBattleWnd_Refresh_battle");

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mMask).onClick = OnClickCloseBtn;
        UIEventListener.Get(mNormalRefreshBtn).onClick = OnClickNormalRefresh;
        UIEventListener.Get(mKeepGainBtn).onClick = OnClickKeepGainRefresh;

        if (mTweenScale != null)
            EventDelegate.Add(mTweenScale.onFinished, OnFinish);

        // 注册字段变化的事件
        ME.user.dbase.RegisterTriggerField("ArenaRefreshBattleWnd_Refresh_battle", new string[] { "arena_opponent" }, new CallBack(OnArenaOpponentFieldChange));
    }

    void OnFinish()
    {
        // 移除正在打开的窗口列表中的缓存
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 刷新成功回调
    /// </summary>
    void OnArenaOpponentFieldChange(object para, params object[] _params)
    {
        // 审核模式下记录竞技场刷新数据
        if(ME.user.QueryTemp<int>("gapp_world") == 1)
        {
            int type = ME.user.Query<int>("arena_opponent/type");
            if(type == ArenaConst.ARENA_MATCH_TYPE_RETAIN)
                SetKeepGainRefreshTimes();
        }

        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 获取玩家竞技场连胜数据
        LPCMapping arenaRecord = ME.user.Query<LPCMapping>("arena_record");

        if (arenaRecord != null || arenaRecord.Count > 0)
        {
            // 连胜次数
            int winTimes = arenaRecord.GetValue<int>("win_times");

            // 获取当前的连胜buff
            mWinsEffectDesc.text = string.Format(LocalizationMgr.Get("ArenaRefershListWnd_6"), winTimes, ArenaMgr.GetArenaBuffDesc(winTimes, false));
        }
        else
        {
            mWinsEffectDesc.text = string.Empty;
        }

        // 获取刷新对战列表的消耗
        mCostData = CALC_ARENA_RETAIN_BUFF_REFRESH_COST.CALL(0);

        // 显示消耗
        string fields = FieldsMgr.GetFieldInMapping(mCostData);
        mCost.text = mCostData.GetValue<int>(fields).ToString();

        // 当前的本地时间
        int currentTime = TimeMgr.GetServerTime();

        // 获取消耗图片
        mCostIcon.spriteName = fields;

        // 上次刷新的时间
        int lastRefreshTime = ME.user.Query<LPCMapping>("arena_opponent").GetValue<int>("match_time");

        // 计算是否可以进行普通刷新
        IsNormalRefresh = ArenaMgr.IsNormalRefresh(lastRefreshTime, currentTime);

        if (IsNormalRefresh)
            return;

        // 计算下次普通刷新的剩余时间
        mRemainTime = ArenaMgr.CalcNextNormalRefreshRemainTime(lastRefreshTime, currentTime);

        // 获取普通刷新的时间间隔
        int timeSpace = GameSettingMgr.GetSettingInt("battle_list_normal_refresh_time");

        if (mRemainTime <= 0)
            mRemainTime = timeSpace;

        // 处于倒计时阶段,置灰刷新按钮
        SetBtnState(120f, false);

        mIsCountDown = true;
    }

    /// <summary>
    /// 刷新倒计时
    /// </summary>
    void RefreshCountDown()
    {
        if (mRemainTime < 0)
        {
            SetBtnState(255f, true);

            mRefreshTimer.text = string.Empty;

            // 停止倒计时
            mIsCountDown = false;

            return;
        }

        // 以00:00 的格式显示倒计时
        mRefreshTimer.text = TimeMgr.ConvertTime(mRemainTime);

        mRemainTime--;
    }

    /// <summary>
    /// 设置按钮的状态
    /// </summary>
    void SetBtnState(float RGB, bool isEnableBtn)
    {
        if (isEnableBtn)
            UIEventListener.Get(mNormalRefreshBtn).onClick = OnClickNormalRefresh;
        else
            UIEventListener.Get(mNormalRefreshBtn).onClick -= OnClickNormalRefresh;

        float value = RGB / 255;

        // 设置按钮的图片颜色
        mNormalRefreshBtn.GetComponent<UISprite>().color = new Color(value, value, value);
    }

    /// <summary>
    /// 窗口关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 普通刷新按钮点击事件
    /// </summary>
    void OnClickNormalRefresh(GameObject go)
    {
        // 向服务器请求刷新列表
        ArenaMgr.RequestArenaBattleList(ArenaConst.ARENA_MATCH_TYPE_NORMAL);

        // 标记为普通刷新
        IsNormalRefresh = true;
    }

    /// <summary>
    /// 维持增益刷新按钮点击事件
    /// </summary>
    void OnClickKeepGainRefresh(GameObject go)
    {
        // 最大刷新次数
        // 审核模式下记录竞技场刷新数据
        if(ME.user.QueryTemp<int>("gapp_world") == 1)
        {
            int maxCount = GameSettingMgr.GetSettingInt("max_arena_keep_gain_refresh");

            if(GetKeepGainRefreshTimes() >= maxCount)
            {
                DialogMgr.Notify(LocalizationMgr.Get("ArenaRefershListWnd_8"));
                return;
            }   
        }

        string fields = FieldsMgr.GetFieldInMapping(mCostData);

        if (ME.user.Query<int>(fields) < mCostData.GetValue<int>(fields))
        {
            DialogMgr.Notify(LocalizationMgr.Get("RankingBattleWnd_13"));
            return;
        }

        // 向服务器请求刷新列表
        ArenaMgr.RequestArenaBattleList(ArenaConst.ARENA_MATCH_TYPE_RETAIN);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLocalText()
    {
        mTitle.text = LocalizationMgr.Get("ArenaRefershListWnd_1");
        mNormalRefershDesc.text = LocalizationMgr.Get("ArenaRefershListWnd_2");
        mNormalRefreshBtnLb.text = LocalizationMgr.Get("ArenaRefershListWnd_3");
        mKeepGainBtnLb.text = LocalizationMgr.Get("ArenaRefershListWnd_5");

        RefreshGaminDesc();
    }

    /// <summary>
    /// 刷新竞技场钻石使用次数描述
    /// </summary>
    void RefreshGaminDesc()
    {
        if(ME.user.QueryTemp<int>("gapp_world") == 1)
        {
            // 最大刷新次数
            int maxCount = GameSettingMgr.GetSettingInt("max_arena_keep_gain_refresh");

            mKeepGainDesc.text = string.Format(LocalizationMgr.Get("ArenaRefershListWnd_7"), GetKeepGainRefreshTimes(), maxCount);
        } 
        else
        {
            mKeepGainDesc.text = LocalizationMgr.Get("ArenaRefershListWnd_4");
        }

    }

    /// <summary>
    /// 获取钻石刷新次数
    /// </summary>
    int GetKeepGainRefreshTimes()
    {
        LPCValue data = OptionMgr.GetLocalOption(ME.user, "ArenaKeepGainRefresh");

        if(data == null || !data.IsMapping)
            return 0;

        LPCMapping refreshData = data.AsMapping;

        if(!Game.IsSameDay(refreshData.GetValue<int>("time"), TimeMgr.GetServerTime()))
            return 0;

        return refreshData.GetValue<int>("count");
    }

    /// <summary>
    /// 设置钻石刷新次数
    /// </summary>
    void SetKeepGainRefreshTimes()
    {
        LPCMapping data = LPCMapping.Empty;

        LPCValue Refreshdata = OptionMgr.GetLocalOption(ME.user, "ArenaKeepGainRefresh");

        if(Refreshdata == null || !Refreshdata.IsMapping)
        {
            data.Add("count", 1);
            data.Add("time", TimeMgr.GetServerTime());
        } 
        else
        {
            LPCMapping refreshDataMap = Refreshdata.AsMapping;

            if(!Game.IsSameDay(refreshDataMap.GetValue<int>("time"), TimeMgr.GetServerTime()))
            {
                data.Add("count", 1);
                data.Add("time", TimeMgr.GetServerTime());
            } 
            else
            {
                data.Add("count", refreshDataMap.GetValue<int>("count") + 1);
                data.Add("time", TimeMgr.GetServerTime());
            }
        }
        
        //保存数据
        OptionMgr.SetLocalOption(ME.user, "ArenaKeepGainRefresh", LPCValue.Create(data));
    }
}
