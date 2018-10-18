/// <summary>
/// PlaybackWnd.cs
/// Created by fengsc 2018/03/05
/// 战斗回放窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class PlaybackWnd : WindowBase<PlaybackWnd>
{
    // 推荐战斗
    public UIToggle mRecommendBtn;
    public UILabel mRecommendBtnLb;

    // 最近战斗
    public UIToggle mRecentCombatBtn;
    public UILabel mRecentCombatBtnLb;

    // 刷新按钮
    public UILabel mRefreshBtn;

    public GameObject mItem;

    public UIGrid mGrid;

    public UIScrollView mUIScrollView;

    public GameObject mRecommendVideosWnd;

    public UIScrollView mRecommendVideosSV;

    public GameObject mRecentVideosWnd;

    public UIScrollView mRecentVideosSV;

    public UIDragScrollView mBgDragScrollView;

    // 输入框
    public UIInput mInput;
    public GameObject mSearchBtn;

    int mCurPage = 1;

    bool mIsOperate = false;

    [HideInInspector]
    public bool mIsGetPublishVideo = false;

    [HideInInspector]
    public bool mIsRefreshRecommend = false;

    void Awake()
    {
        // 初始化本地化文本
        InitText();

        // 注册事件
        RegisterEvent();

        mItem.SetActive(false);
    }

    void OnEnable()
    {
        mIsRefreshRecommend = true;

        // 注册事件
        EventMgr.RegisterEvent("PlaybackWndVideoDetails", EventMgrEventType.EVENT_VIDEO_DETAILS, OnVideoDetailsEvent);
    }

    void OnDisable()
    {
        mIsGetPublishVideo = false;

        mIsRefreshRecommend = true;

        // 注销事件
        EventMgr.UnregisterEvent("PlaybackWndVideoDetails");
    }

    /// <summary>
    /// 视频详细信息获取事件回调
    /// </summary>
    void OnVideoDetailsEvent(int eventId, MixedValue para)
    {
        // 打开视屏录像详细窗口
        GameObject wnd = WindowMgr.OpenWnd(PlaybackInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<PlaybackInfoWnd>().Bind(VideoMgr.VideoDetails.GetValue<string>("id"));
    }

    void Start()
    {
        mRecommendVideosWnd.SetActive(false);
        mRecentVideosWnd.SetActive(false);

        // 初始化选项
        mCurPage = ArenaMgr.PlaybackPage;
        if(mCurPage == 2)
        {
            mRecentCombatBtn.Set(true);
            mRecommendBtn.Set(false);

            mRecentVideosWnd.SetActive(true);

            if (mIsGetPublishVideo)
                return;

            // 获取发布视频列表
            VideoMgr.QueryPublishVideo(0);
        }
        else
        {
            mRecommendBtn.Set(true);
            mRecentCombatBtn.Set(false);

            mRecommendVideosWnd.SetActive(true);
        }

        RefreshDragScrollView();

        ArenaMgr.PlaybackPage = mCurPage;
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("PlaybackWnd");
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mRecommendBtnLb.text = LocalizationMgr.Get("PlaybackWnd_1");
        mRecentCombatBtnLb.text = LocalizationMgr.Get("PlaybackWnd_2");
        mRefreshBtn.text = LocalizationMgr.Get("PlaybackWnd_3");

        // 输入框默认显示文本
        mInput.defaultText = LocalizationMgr.Get("PlaybackWnd_8");
    }

    void RefreshDragScrollView()
    {
        BoxCollider bc = mBgDragScrollView.GetComponent<BoxCollider>();
        if (bc == null)
            return;

        if (mCurPage == 2)
        {
            mBgDragScrollView.scrollView = mRecentVideosSV;

            RecentVideoWnd recent = mRecentVideosWnd.GetComponent<RecentVideoWnd>();

            bc.size = new Vector3(bc.size.x, mRecentVideosSV.panel.GetViewSize().y, bc.size.z);

            bc.center = new Vector3(bc.center.x, bc.center.y + recent.mLoadTips.height / 2, bc.center.z);
        }
        else
        {
            mBgDragScrollView.scrollView = mRecommendVideosSV;

            bc.size = new Vector3(bc.size.x, mRecommendVideosSV.panel.GetViewSize().y, bc.size.z);

            bc.center = Vector3.zero;
        }
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mRecommendBtn.gameObject).onClick = OnClickRecommendBtn;
        UIEventListener.Get(mRecentCombatBtn.gameObject).onClick = OnClickRecentCombatBtn;
        UIEventListener.Get(mRefreshBtn.gameObject).onClick = OnClickRefreshBtn;
        UIEventListener.Get(mSearchBtn).onClick = OnClickmSearchBtn;

        // 注册EVENT_REFRESH_PLAYBACK_LIST事件
        EventMgr.RegisterEvent("PlaybackWnd", EventMgrEventType.EVENT_PLAY_VIDEO, OnPlayVideoEvent);
    }

    void OnPlayVideoEvent(int eventId, MixedValue para)
    {
        bool result = para.GetValue<bool>();
        if (!result)
            return;

        WindowMgr.HideMainWnd();
        WindowMgr.DestroyWindow(ArenaWnd.WndType);
    }

    /// <summary>
    /// 推荐对战按钮点击回调
    /// </summary>
    void OnClickRecommendBtn(GameObject go)
    {
        if (mCurPage == 1)
            return;

        mRefreshBtn.gameObject.SetActive(true);

        mCurPage = 1;

        ArenaMgr.PlaybackPage = mCurPage;

        mRecommendVideosWnd.SetActive(true);
        mRecentVideosWnd.SetActive(false);

        RefreshDragScrollView();
    }

    /// <summary>
    /// 最近对战按钮点击回调
    /// </summary>
    void OnClickRecentCombatBtn(GameObject go)
    {
        if (mCurPage == 2)
            return;

        mCurPage = 2;

        ArenaMgr.PlaybackPage = mCurPage;

        mRefreshBtn.gameObject.SetActive(false);

        mRecommendVideosWnd.SetActive(false);
        mRecentVideosWnd.SetActive(true);

        RefreshDragScrollView();

        if (mIsGetPublishVideo)
            return;

        // 获取发布视频列表
        VideoMgr.QueryPublishVideo(0);
    }

    /// <summary>
    /// 刷新按钮点击回调
    /// </summary>
    void OnClickRefreshBtn(GameObject go)
    {
        if (mIsOperate)
            return;

        mIsOperate = true;

        // 强制刷新列表
        VideoMgr.RefreshRecommendVideo(true);

        mIsOperate = false;
    }

    /// <summary>
    /// Raises the clickm search button event.
    /// </summary>
    /// <param name="go">Go.</param>
    void OnClickmSearchBtn(GameObject go)
    {
        // 获取输入查找searchVideoId
        string searchVideoId = mInput.value.Trim();

        // 重置输入框
        mInput.value = string.Empty;

        // 没有输入有效录像id
        if (string.IsNullOrEmpty(searchVideoId))
        {
            // 给出提示信息
            DialogMgr.Notify(string.Format(LocalizationMgr.Get("PlaybackWnd_9")));
            return;
        }

        // 尝试获取录像详细信息
        VideoMgr.GetVideoDetails(searchVideoId);
    }
}
