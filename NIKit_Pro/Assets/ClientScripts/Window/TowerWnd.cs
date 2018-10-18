/// <summary>
/// TowerWnd.cs
/// 通天之塔界面
/// Created by fengsc 2017/08/21
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class TowerWnd : WindowBase<TowerWnd>
{
    #region 成员变量

    // 难度按钮
    public UISprite mDiffBtn;
    public UILabel mDiffLb;

    // 返回地图按钮
    public GameObject mReturnMapBtn;
    public UILabel mReturnMapBtnLb;

    // 计时描述
    public UILabel mTimerDesc;

    // 计时器
    public UILabel mTimer;

    // 窗口标题
    public UILabel mTitle;

    // 标题阴影
    public UILabel mTitleShadow;

    private bool mIsCountDown = false;

    private float mLastTime = 0;

    // 剩余时间
    private float mRemainTime = 0;

    // 当前通天塔选择的难度
    private int mDifficulty;

    private bool mIsFire = false;

    private string mInstanceId = string.Empty;

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();

        // 初始化文本
        InitLabel();

        float scale = Game.CalcWndScale();
        transform.localScale = new Vector3(scale, scale, scale);
    }

    void Update()
    {
        if (mIsCountDown)
        {
            if (Time.realtimeSinceStartup > mLastTime + 1)
            {
                mLastTime = Time.realtimeSinceStartup;

                // 刷新倒计时
                CountDown();
            }
        }
    }

    /// <summary>
    /// 倒计时
    /// </summary>
    void CountDown()
    {
        if (mRemainTime < 86400)
        {
            mIsCountDown = false;

            mIsFire = false;

            mInstanceId = string.Empty;

            // 重绘窗口
            Redraw();

            return;
        }

        string desc = string.Empty;

        if (mRemainTime > 60 + 86400)
        {
            desc = string.Format(LocalizationMgr.Get("TowerWnd_4"), (int) mRemainTime / 86400);
        }
        else
        {
            desc = string.Format(LocalizationMgr.Get("TowerWnd_7"), mRemainTime - 86400);
        }

        mTimer.text = desc;

        mRemainTime--;
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mDiffBtn.gameObject).onClick = OnClickDiffBtn;
        UIEventListener.Get(mReturnMapBtn).onClick = OnClickReturnMapBtn;
    }

    /// <summary>
    /// 初始化文本
    /// </summary>
    void InitLabel()
    {
        mTitle.text = LocalizationMgr.Get("TowerWnd_1");

        mTitleShadow.text = LocalizationMgr.Get("TowerWnd_1");

        mTimerDesc.text = LocalizationMgr.Get("TowerWnd_3");

        mReturnMapBtnLb.text = LocalizationMgr.Get("TowerWnd_2");
    }

    /// <summary>
    /// 重绘窗口
    /// </summary>
    void Redraw()
    {
        // 距离剩余刷新时间
        mRemainTime = Mathf.Max(TowerMgr.CalcNextTime() - TimeMgr.GetServerTime(), 0) + 86400;

        // 开启倒计时
        if (mRemainTime > 86400)
            mIsCountDown = true;
        else
            mTimer.text = string.Format("{0}{1}", 0, LocalizationMgr.Get("TowerWnd_4"));

        if (mDifficulty.Equals(TowerConst.EASY_TOWER))
        {
            mDiffBtn.spriteName = "tower_yellow_btn";

            mDiffLb.text = LocalizationMgr.Get("TowerWnd_6");
        }
        else
        {
            mDiffBtn.spriteName = "tower_red_btn";
            mDiffLb.text = LocalizationMgr.Get("TowerWnd_5");
        }

        // 构建参数
        LPCMapping para = LPCMapping.Empty;

        para.Add("difficulty", mDifficulty);

        para.Add("instance_id", mInstanceId);

        if (!mIsFire)
        {
            EventMgr.FireEvent(EventMgrEventType.EVENT_SWITCH_TOWER_DIFFICULTY, MixedValue.NewMixedValue<LPCMapping>(para));

            return;
        }

        // 抛出打开通天塔事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_OPEN_TOWER_SCENE, MixedValue.NewMixedValue<LPCMapping>(para));
    }

    /// <summary>
    /// 难度按钮点击回调
    /// </summary>
    void OnClickDiffBtn(GameObject go)
    {
        if (mDifficulty.Equals(TowerConst.EASY_TOWER))
        {
            mDifficulty = TowerConst.HARD_TOWER;

            mDiffLb.text = LocalizationMgr.Get("TowerWnd_5");

            mDiffBtn.spriteName = "tower_red_btn";
        }
        else
        {
            mDifficulty = TowerConst.EASY_TOWER;

            mDiffLb.text = LocalizationMgr.Get("TowerWnd_6");

            mDiffBtn.spriteName = "tower_yellow_btn";
        }

        LPCMapping para = LPCMapping.Empty;

        para.Add("difficulty", mDifficulty);

        // 抛出切换通天塔难度事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_SWITCH_TOWER_DIFFICULTY, MixedValue.NewMixedValue<LPCMapping>(para));
    }

    /// <summary>
    /// 返回地图按钮地点击回调
    /// </summary>
    void OnClickReturnMapBtn(GameObject go)
    {
        // 抛出关闭通天塔事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_CLOSE_TOWER_SCENE, null);

        // 关闭通天塔界面
        WindowMgr.DestroyWindow(TowerWnd.WndType);

        // 显示主城界面
        WindowMgr.OpenMainWnd();
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(int diff, bool isFire = true, string instanceId = "")
    {
        mDifficulty = diff;

        mIsFire = isFire;

        mInstanceId = instanceId;

        // 重绘窗口
        Redraw();
    }

    #endregion
}
