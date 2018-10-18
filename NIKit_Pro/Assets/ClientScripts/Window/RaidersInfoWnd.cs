/// <summary>
/// RaidersInfoWnd.cs
/// Created by fengsc 2017/08/22
/// 通天之塔攻略信息
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class RaidersInfoWnd : WindowBase<RaidersInfoWnd>
{
    public enum TOWER_TOGGLE
    {
        BOSS_INFO,
        RAIDERS_PET,
    }

    #region 成员变量

    // boss信息按钮
    public GameObject mBossInfoBtn;
    public UILabel mBossInfoBtnLb;

    // 攻略使魔按钮
    public GameObject mRaidersPetBtn;
    public UILabel mRaidersPetBtnLb;

    // 首领信息窗口
    public GameObject mBossInfoWnd;

    // 攻略使魔信息窗口
    public GameObject mTowerRaidersPetWnd;

    // 当前难度
    private int mDifficulty = 1;

    // 当前所在的boss层
    private int mCurBossLayer = 0;

    private TOWER_TOGGLE mCurSelect = TOWER_TOGGLE.BOSS_INFO;

    #endregion

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

        // 重绘窗口
        Redraw();
    }

    void OnDestroy()
    {
        EventMgr.UnregisterEvent("RaidersInfoWnd");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mBossInfoBtn).onClick = OnClickBossInfoBtn;
        UIEventListener.Get(mRaidersPetBtn).onClick = OnClickRaidersPetBtn;

        // 注册EVENT_SWITCH_TOWER_DIFFICULTY事件
        EventMgr.RegisterEvent("RaidersInfoWnd", EventMgrEventType.EVENT_SWITCH_TOWER_DIFFICULTY, OnEventSwitchDifficulty);

        EventMgr.RegisterEvent("RaidersInfoWnd", EventMgrEventType.EVENT_OPEN_TOWER_SCENE, OnEventSwitchDifficulty);

        // 监听通天塔滑动事件
        EventMgr.RegisterEvent("RaidersInfoWnd", EventMgrEventType.EVENT_TOWER_SLIDE, OnTowerSlideEvent);
    }

    /// <summary>
    /// EVENT_TOWER_SLIDE事件回调
    /// </summary>
    void OnTowerSlideEvent(int eventID, MixedValue para)
    {
        mCurBossLayer = para.GetValue<int>();
    }

    /// <summary>
    /// EVENT_SWITCH_TOWER_DIFFICULTY事件回调
    /// </summary>
    void OnEventSwitchDifficulty(int eventId, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();

        // 通天塔当前选择的难度
        mDifficulty = data.GetValue<int>("difficulty");

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 初始化文本信息
    /// </summary>
    void InitLabel()
    {
        mBossInfoBtnLb.text = LocalizationMgr.Get("RaidersInfoWnd_1");
        mRaidersPetBtnLb.text = LocalizationMgr.Get("RaidersInfoWnd_2");
    }

    /// <summary>
    /// 重绘窗口
    /// </summary>
    void Redraw()
    {
        // 当前最高通关层数
        int maxClearanceLayer = TowerMgr.GetMaxClearanceLayer(ME.user, mDifficulty) + 1;

        if (maxClearanceLayer == 100)
            maxClearanceLayer -= 1;

        // 当前的boss层
        mCurBossLayer = maxClearanceLayer / 10 * 10 + 10 - 1;

        // 刷新数据
        RefreshData();
    }

    /// <summary>
    /// 首领信息按钮点击回调
    /// </summary>
    void OnClickBossInfoBtn(GameObject go)
    {
        if (mCurSelect.Equals(TOWER_TOGGLE.BOSS_INFO))
            return;

        mCurSelect = TOWER_TOGGLE.BOSS_INFO;

        RefreshData();
    }

    /// <summary>
    /// 攻略使魔按钮点击回调
    /// </summary>
    void OnClickRaidersPetBtn(GameObject go)
    {
        if (mCurSelect.Equals(TOWER_TOGGLE.RAIDERS_PET))
            return;

        mCurSelect = TOWER_TOGGLE.RAIDERS_PET;

        RefreshData();
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    void RefreshData()
    {
        if (mCurSelect.Equals(TOWER_TOGGLE.BOSS_INFO))
        {
            if (!mBossInfoWnd.activeInHierarchy)
                mBossInfoWnd.SetActive(true);

            TowerBossInfoWnd script = mBossInfoWnd.GetComponent<TowerBossInfoWnd>();
            if (script == null)
                return;

            script.Bind(mDifficulty, mCurBossLayer);

            if (mTowerRaidersPetWnd.activeInHierarchy)
                mTowerRaidersPetWnd.SetActive(false);
        }
        else
        {
            if (mBossInfoWnd.activeInHierarchy)
                mBossInfoWnd.SetActive(false);

            if (!mTowerRaidersPetWnd.activeInHierarchy)
                mTowerRaidersPetWnd.SetActive(true);

            TowerRaidersPetWnd script = mTowerRaidersPetWnd.GetComponent<TowerRaidersPetWnd>();
            if (script == null)
                return;

            script.Bind(mDifficulty, mCurBossLayer);
        }
    }
}
