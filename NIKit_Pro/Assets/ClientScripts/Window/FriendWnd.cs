/// <summary>
/// FriendWnd.cs
/// Created by fengsc 2017/01/18
/// 好友窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class FriendWnd : WindowBase<FriendWnd>
{
    #region 成员变量

    // 标题
    public UILabel mTitle;

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    public UILabel mFriendLb;

    // 我的好友选项
    public GameObject mMyFriendBtn;
    public UILabel mMyFriendBtnLb;

    // 推荐奖励选项
    public GameObject mRecommendRewardBtn;
    public UILabel mRecommendRewardBtnLb;

    //邀请好友选项
    public GameObject mInviteFriendBtn;
    public GameObject mRedPointGo;

    // 好友请求选项
    public GameObject mFriendRequestBtn;
    public UILabel mFriendRequestBtnLb;

    // 请求提示
    public GameObject mRequestTips;

    // 好友请求的数量
    public UILabel mRequestAmount;

    // 添加好友选项
    public GameObject mAddFriendBtn;
    public UILabel mAddFriendBtnLb;

    // 存储好友界面各个子窗口对象
    public GameObject[] mWnds;

    public InviteFriendWnd mInviteFriendWndCtrl;

    // 所有选项按钮
    public GameObject[] mButtons;

    public TweenScale mTweenScale;

    bool mIsMainCity = true;

    // 选项按钮label的初始相对位置
    Vector3 mInitPos = new Vector3(8, 3, 0);

    // 请求提示的初始相对位置
    Vector3 mTipsPos = new Vector3(-60, 32, 0);

    public enum PageType
    {
        /// <summary>
        /// 我的好友
        /// </summary>
        MyFriend = 0,
        /// <summary>
        /// 邀请好友
        /// </summary>
        InviteFriend,
        /// <summary>
        /// 推荐奖励
        /// </summary>
        RecommendReward,
        /// <summary>
        /// 好友请求
        /// </summary>
        FriendRequest,
        /// <summary>
        /// 查找好友
        /// </summary>
        AddFriend
    }

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();

        // 初始化本地化文本
        InitText();

        // 绘制窗口
        Redraw();
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent(FriendWnd.WndType);

        Coroutine.StopCoroutine("DelaySwitch");

        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField("FriendWnd");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mMyFriendBtn).onClick = OnClickMyFriendBtn;
        UIEventListener.Get(mInviteFriendBtn).onClick = OnClickInviteFriendBtn;
        UIEventListener.Get(mRecommendRewardBtn).onClick = OnClickRecommendReward;
        UIEventListener.Get(mFriendRequestBtn).onClick = OnClickFriendRequest;
        UIEventListener.Get(mAddFriendBtn).onClick = OnClickAddFriend;

        // 监听好友请求列表事件
        EventMgr.RegisterEvent(FriendWnd.WndType, EventMgrEventType.EVENT_FRIEND_REQUEST, OnFriendRequest);

        EventMgr.RegisterEvent(FriendWnd.WndType, EventMgrEventType.EVENT_FRIEND_OPERATE_DONE, OnFriendOperateDone);

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);

        if (ME.user == null)
            return;

        // 角色属性变更监听
        ME.user.dbase.RegisterTriggerField("FriendWnd", new string[] { "task" }, new CallBack(OnTaskChange));
    }

    /// <summary>
    /// 动画播放完成回调
    /// </summary>
    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    void OnFriendOperateDone(int eventId, MixedValue para)
    {
        // 刷新好友请求数量
        RefreshRequestTips();
    }

    /// <summary>
    /// 好友操作结果事件回调
    /// </summary>
    void OnFriendRequest(int eventId, MixedValue para)
    {
        // 刷新好友请求数量
        RefreshRequestTips();
    }

    void RefreshRequestTips()
    {
        // 好友请求列表
        LPCArray list = FriendMgr.GetRequestList(ME.user);
        if (list == null)
            list = LPCArray.Empty;

        // 刷新好友数量提示
        int amount = list.Count;
        if (amount < 1)
        {
            mRequestTips.SetActive(false);
        }
        else
        {
            mRequestTips.SetActive(true);
            mRequestAmount.text = amount.ToString();
        }
    }

    /// <summary>
    ///  任务状态变化
    /// </summary>
    void OnTaskChange(object para, params object[] _params)
    {
        RefreshInviteTips();
    }

    /// <summary>
    /// 刷新邀请好友任务奖励提示
    /// </summary>
    private void RefreshInviteTips()
    {
        int counts = TaskMgr.GetInviteFriendTaskBounsCounts();

        mRedPointGo.SetActive(counts <= 0 ? false : true);
        mRedPointGo.GetComponentInChildren<UILabel>().text = counts.ToString();
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mTitle.text = LocalizationMgr.Get("FriendWnd_1");
        mFriendLb.text = LocalizationMgr.Get("FriendWnd_2");
        mMyFriendBtnLb.text = LocalizationMgr.Get("FriendWnd_3");
        mRecommendRewardBtnLb.text = LocalizationMgr.Get("FriendWnd_4");
        mFriendRequestBtnLb.text = LocalizationMgr.Get("FriendWnd_5");
        mAddFriendBtnLb.text = LocalizationMgr.Get("FriendWnd_6");
        mInviteFriendBtn.GetComponentInChildren<UILabel>().text = LocalizationMgr.Get("FriendWnd_7");
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 默认选择第一个选项, 显示对应的窗口
        SetButtonTextPos(mMyFriendBtn);
        OpenWnd(mWnds[0]);

        // 刷新好友请求数量
        RefreshRequestTips();

        RefreshInviteTips();
    }

    /// <summary>
    /// 窗口关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        if (mIsMainCity)
            WindowMgr.OpenWnd ("MainWnd");

        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 我的好友按钮点击事件
    /// </summary>
    void OnClickMyFriendBtn(GameObject go)
    {
        // 设置按钮label的状态
        SetButtonTextPos(go);

        // 显示对应的窗口
        OpenWnd(mWnds[0]);
    }

    /// <summary>
    /// 邀请好友按钮点击事件
    /// </summary>
    void OnClickInviteFriendBtn(GameObject go)
    {
        // 设置按钮label的状态
        SetButtonTextPos(go);

        // 显示对应的窗口
        OpenWnd(mWnds[4]);
    }

    /// <summary>
    /// 推荐奖励按钮点击事件
    /// </summary>
    void OnClickRecommendReward(GameObject go)
    {
        SetButtonTextPos(go);
        OpenWnd(mWnds[1]);
    }

    /// <summary>
    /// 好友请求按钮点击事件
    /// </summary>
    void OnClickFriendRequest(GameObject go)
    {
        SetButtonTextPos(go);
        OpenWnd(mWnds[2]);
    }

    /// <summary>
    /// 添加好友按钮点击事件
    /// </summary>
    void OnClickAddFriend(GameObject go)
    {
        SetButtonTextPos(go);
        OpenWnd(mWnds[3]);
    }

    /// <summary>
    /// 打开窗口
    /// </summary>
    void OpenWnd(GameObject wnd)
    {
        if (wnd == null)
            return;

        for (int i = 0; i < mWnds.Length; i++)
        {
            if (mWnds[i].Equals(wnd))
                mWnds[i].SetActive(true);
            else
                mWnds[i].SetActive(false);
        }
    }

    /// <summary>
    /// 设置按钮文本的位置
    /// </summary>
    void SetButtonTextPos(GameObject go)
    {
        if (go == null)
            return;

        for (int i = 0; i < mButtons.Length; i++)
        {
            GameObject button = mButtons[i];

            if (button == null)
                continue;

            Transform labelTrans = button.transform.Find("Label");

            if (labelTrans == null)
                continue;

            BoxCollider bc = button.GetComponent<BoxCollider>();

            if (bc == null)
                continue;

            if (go.Equals(button))
            {
                labelTrans.localPosition = mInitPos;
                bc.enabled = false;

                if (i == 2)
                {
                    mRequestTips.transform.localPosition = new Vector3(mTipsPos.x - 15, mTipsPos.y, mTipsPos.z);
                }
            }
            else
            {
                labelTrans.localPosition = new Vector3(mInitPos.x + 14, mInitPos.y, mInitPos.z);
                bc.enabled = true;

                if (i == 2)
                {
                    mRequestTips.transform.localPosition = mTipsPos;
                }
            }
        }
    }

    private IEnumerator DelaySwitch(PageType type)
    {
        yield return 1;

        switch (type)
        {
            case PageType.MyFriend:

                OnClickMyFriendBtn(mMyFriendBtn);

                mMyFriendBtn.GetComponent<UIToggle>().Set(true);

                yield break;

            case PageType.InviteFriend:

                OnClickInviteFriendBtn(mInviteFriendBtn);

                mInviteFriendBtn.GetComponent<UIToggle>().Set(true);

                yield break;

            case PageType.RecommendReward:

                OnClickRecommendReward(mRecommendRewardBtn);

                mRecommendRewardBtn.GetComponent<UIToggle>().Set(true);

                yield break;

            case PageType.FriendRequest:

                OnClickFriendRequest(mFriendRequestBtn);

                mFriendRequestBtn.GetComponent<UIToggle>().Set(true);

                yield break;

            case PageType.AddFriend:

                OnClickAddFriend(mAddFriendBtn);

                mAddFriendBtn.GetComponent<UIToggle>().Set(true);

                yield break;
        }
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(bool isMainCity)
    {
        mIsMainCity = isMainCity;
    }

    /// <summary>
    /// 跳转到指定分页
    /// </summary>
    /// <param name="type"></param>
    public void SwitchPage(PageType type)
    {
        Coroutine.DispatchService(DelaySwitch(type), "DelaySwitch");
    }

    /// <summary>
    /// 设置邀请id
    /// </summary>
    /// <param name="inviteId"></param>
    public void SetInviteId(string inviteId)
    {
        if (mInviteFriendWndCtrl == null)
            return;

        mInviteFriendWndCtrl.SetInviteId(inviteId);
    }
}
