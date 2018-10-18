/// <summary>
/// TowerRankBonusWnd.cs
/// Created by lic 2017/08/31
/// 通天之塔排名奖励窗口
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class TowerRankBonusWnd : WindowBase<TowerRankBonusWnd>
{
    #region 成员变量

    // 排名按钮
    public GameObject mRankToggleBtn;
    public UILabel mRankToggleBtnLb;

    // 奖励按钮
    public GameObject mBonusToggleBtn;
    public UILabel mBonusToggleBtnLb;

    // 排名窗口
    public GameObject mRankWnd;

    // 奖励窗口
    public GameObject mBonusWnd;

    #endregion

    #region 私有变量

    // 奖励页为0，排名页为1,默认为奖励页
    int mCurPage = 0;

    // 当前通天塔难度
    int mDifficulty = 0;

    #endregion 

    #region 内部函数

    void Awake()
    {
        // 注册事件
        RegisterEvent();
    }

    // Use this for initialization
    void Start ()
    {
        // 初始化文本信息
        InitLabel();

        // 刷新数据
        Redarw();
    }

    void OnDestroy()
    {
        EventMgr.UnregisterEvent("TowerRankBonusWnd");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mRankToggleBtn).onClick = OnRankToggleBtn;
        UIEventListener.Get(mBonusToggleBtn).onClick = OnBonusToggleBtn;

        // 注册EVENT_SWITCH_TOWER_DIFFICULTY事件
        EventMgr.RegisterEvent("TowerRankBonusWnd", EventMgrEventType.EVENT_SWITCH_TOWER_DIFFICULTY, OnEventSwitchDifficulty);

        // 注册EVENT_SWITCH_TOWER_DIFFICULTY事件
        EventMgr.RegisterEvent("TowerRankBonusWnd", EventMgrEventType.EVENT_OPEN_TOWER_SCENE, OnEventSwitchDifficulty);
    }

    /// <summary>
    /// EVENT_SWITCH_TOWER_DIFFICULTY事件回调
    /// </summary>
    void OnEventSwitchDifficulty(int eventId, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();

        // 通天塔当前选择的难度
        mDifficulty = data.GetValue<int>("difficulty");

        // 刷新数据
        Redarw();
    }

    /// <summary>
    /// 初始化文本信息
    /// </summary>
    void InitLabel()
    {
        mRankToggleBtnLb.text = LocalizationMgr.Get("TowerRankBonusWnd_1");
        mBonusToggleBtnLb.text = LocalizationMgr.Get("TowerRankBonusWnd_2");
    }

    /// <summary>
    ///  排名按钮被点击
    /// </summary>
    /// <param name="go">Go.</param>
    void OnRankToggleBtn(GameObject go)
    {
        // 当前页面不处理
        if (mCurPage == 1)
            return;

        mCurPage =1;

        mRankWnd.SetActive(true);
        mBonusWnd.SetActive(false);

        // 刷新页面
        Redarw();
    }

    /// <summary>
    /// 奖励按钮被点击
    /// </summary>
    /// <param name="go">Go.</param>
    void OnBonusToggleBtn(GameObject go)
    {
        // 当前页面不处理
        if (mCurPage == 0)
            return;

        mCurPage = 0;

        mRankWnd.SetActive(false);
        mBonusWnd.SetActive(true);

        // 刷新页面
        Redarw();
    }

    // 刷新窗口
    void Redarw()
    {
        if (mCurPage == 0)
        {
            if(! mBonusWnd.activeInHierarchy)
                mBonusWnd.SetActive(true);

            mBonusWnd.GetComponent<TowerViewBonusWnd>().Redraw(mDifficulty);
        }
        else
        {
            mRankWnd.GetComponent<TowerRankWnd>().Redraw(mDifficulty);
        }

    }

    #endregion 

    #region 外部接口

    #endregion 
}
