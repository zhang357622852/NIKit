/// <summary>
/// SignWnd.cs
/// Created by fengsc 2016/11/02
/// 每日签到界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class SignWnd : WindowBase<SignWnd>
{
    #region 成员变量

    // 界面标题
    public UILabel mTitle;

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    // 签到重置时间提示
    public UILabel mResetTimeTips;

    // 物品基础格子
    public GameObject mItem;

    // 排序组件
    public UIGrid mGrid;

    List<CsvRow> mSignList = new List<CsvRow>();

    // 签到的次数
    int mSignTimes = 0;

    // 是否有新的签到
    bool mHasNewSign = false;

    // 是否是登入开启此界面，并且自动签到
    bool mIsLoginOpen = false;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        // sign_bonus字段的变化
        ME.user.dbase.RegisterTriggerField(
            "SignWnd",
            new string[]{ "sign_bonus" },
            new CallBack(OnFieldsChange));

        // 绘制窗口
        Redraw();

        TweenScale mTweenScale = transform.GetComponent<TweenScale>();

        if (mTweenScale == null)
            return;

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    void OnDisable()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 解注册
        ME.user.dbase.RemoveTriggerField("SignWnd");
    }

    private void OnDestroy()
    {
        if (mIsLoginOpen)
            MarketMgr.ShowGiftLoginTips();
    }

    /// <summary>
    /// 字段变化的回调
    /// </summary>
    void OnFieldsChange(object para, params object[] param)
    {
        // 签到奖励数据
        LPCMapping signBonus = ME.user.Query<LPCMapping>("sign_bonus");

        // 签到次数
        mSignTimes = signBonus.GetValue<int>("sign_times");

        // 今天是否有新的签到
        mHasNewSign = CommonBonusMgr.HasNewSign(ME.user);

        // 刷新数据
        RefreshData(signBonus);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mTitle.text = LocalizationMgr.Get("SignWnd_1");

        // 获取当前时间
        int currentTime = TimeMgr.GetServerTime();

        // 重置时间提示
        mResetTimeTips.text = string.Format(LocalizationMgr.Get("SignWnd_3"), Game.GetDaysInMonth(currentTime) - Game.GetDaysMonth(currentTime));

        // 获取签到列表
        mSignList = CommonBonusMgr.GetBonusList(CommonBonusMgr.SIGN_BONUS);

        // 没有获取到签到列表
        if (mSignList == null || mSignList.Count < 1)
            return;

        // 今天是否有新的签到
        mHasNewSign = CommonBonusMgr.HasNewSign(ME.user);

        // 签到数据
        LPCMapping bonusMap = ME.user.Query<LPCMapping>("sign_bonus");
        if (bonusMap == null)
            bonusMap = LPCMapping.Empty;

        // 签到次数
        mSignTimes = bonusMap.GetValue<int>("sign_times");

        mItem.SetActive(false);
        foreach (CsvRow row in mSignList)
        {
            if (row == null)
                continue;

            // 是否已经签到
            bool isSign = false;
            int loginDay = row.Query<int>("id");
            if (loginDay <= mSignTimes)
                isSign = true;

            GameObject wnd = GameObject.Instantiate(mItem);

            if (wnd == null)
                continue;

            wnd.transform.SetParent(mGrid.transform);

            wnd.transform.localScale = Vector3.one;
            wnd.transform.localPosition = Vector3.zero;

            wnd.name = row.Query<int>("id").ToString();

            wnd.SetActive(true);

            SignItemWnd script = wnd.GetComponent<SignItemWnd>();

            bool isShow = false;
            if (!mHasNewSign)
            {
                // 当前的签到显示动画
                if (loginDay == mSignTimes)
                    isShow = true;
                else
                    isShow = false;
            }

            LPCArray bonusList = row.Query<LPCArray>("bonus_list");

            // 绑定数据
            script.Bind(bonusList[0].AsMapping, LocalizationMgr.Get("SignWnd_4"), isShow, isSign, loginDay);

            if (mSignTimes + 1 != loginDay)
                continue;

            // 通知服务器执行签到
            CommonBonusMgr.DoSign(ME.user);
        }

        mGrid.repositionNow = true;
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    void RefreshData(LPCMapping signBonus)
    {
        int index = 0;
        foreach (CsvRow row in mSignList)
        {
            if (mSignList.Count > mGrid.transform.childCount)
                continue;

            index++;
            if (row == null)
                continue;

            bool isSign = false;
            int loginDay = row.Query<int>("id");
            if (loginDay <= mSignTimes)
                isSign = true;

            GameObject wnd = mGrid.transform.GetChild(index).gameObject;

            if (wnd == null)
                continue;

            SignItemWnd script = wnd.GetComponent<SignItemWnd>();

            bool isShow = false;
            if (!mHasNewSign)
            {
                // 当前的签到显示动画
                if (loginDay == mSignTimes)
                    isShow = true;
                else
                    isShow = false;
            }

            LPCArray bonusList = row.Query<LPCArray>("bonus_list");
            // 绑定数据
            script.Bind(bonusList[0].AsMapping, LocalizationMgr.Get("SignWnd_4"), isShow, isSign, loginDay);

            if (mSignTimes != loginDay)
                continue;

            script.ShowTipsEffect();
        }
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 隐藏签到窗口
        WindowMgr.DestroyWindow(gameObject.name );
    }

    /// <summary>
    /// 设置登入开启标识
    /// </summary>
    /// <param name="isLogin"></param>
    public void SetLoginFlag(bool isLogin)
    {
        mIsLoginOpen = isLogin;
    }
}
