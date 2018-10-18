/// <summary>
/// RecentVideoWnd.cs
/// Created by fengsc 2018/03/05
/// 最近对战
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class RecentVideoWnd : WindowBase<RecentVideoWnd>
{
    public GameObject mItem;

    public UIGrid mGrid;

    public UIScrollView mUIScrollView;

    public PlaybackWnd mPlaybackWnd;

    public UILabel mLoadTips;

    public UIScrollBar mUIScrollBar;

    bool mIsLoadFinish = false;

    List<GameObject> mItems = new List<GameObject>();

    LPCArray mRecentList = LPCArray.Empty;

    void Awake()
    {
        // 注册事件
        RegisterEvent();

        mItem.SetActive(false);
    }

    void OnEnable()
    {
        // 绘制最近对战列表
        if(mPlaybackWnd.mIsGetPublishVideo)
            RedrawRecentCombatList();
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("RecentVideoWnd");
    }

    void OnUIScrollBarChange()
    {
        RefreshLoadTips();
    }

    void RefreshLoadTips()
    {
        if (mIsLoadFinish)
            mLoadTips.text = LocalizationMgr.Get("PlaybackWnd_7");
        else
            mLoadTips.text = LocalizationMgr.Get("PlaybackWnd_6");

        if (mUIScrollBar.value >= 0.99f)
        {
            mLoadTips.gameObject.SetActive(true);
        }
        else
        {
            mLoadTips.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 加载提示按钮点击事件
    /// </summary>
    void OnClickLoadTips(GameObject go)
    {
        if (mIsLoadFinish)
            return;

        // 获取发布视频列表
        VideoMgr.QueryPublishVideo(mRecentList.Count);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mLoadTips.gameObject).onClick = OnClickLoadTips;

        EventMgr.RegisterEvent("RecentVideoWnd", EventMgrEventType.EVNT_QUERY_PUBLISH_VIDEO_LIST, OnQueryPublishVideos);

        EventDelegate.Add(mUIScrollBar.onChange, OnUIScrollBarChange);
    }

    void CreateGameObject()
    {
        GameObject item = Instantiate(mItem);

        item.transform.SetParent(mGrid.transform);

        item.transform.localPosition = Vector3.zero;

        item.transform.localScale = Vector3.one;

        item.SetActive(true);

        mItems.Add(item);
    }

    /// <summary>
    /// 绘制最近对战列表
    /// </summary>
    void RedrawRecentCombatList()
    {
        for (int i = 0; i < mItems.Count; i++)
            mItems[i].SetActive(false);

        LPCArray list = VideoMgr.PublishVideos;
        if (list.Count == mRecentList.Count)
            mIsLoadFinish = true;
        else
            mIsLoadFinish = false;

        // 发布视频列表
        mRecentList = list;

        int amount = mRecentList.Count - mItems.Count;
        if (amount > 0)
        {
            for (int i = 0; i < amount; i++)
                CreateGameObject();
        }

        for (int i = 0; i < mRecentList.Count; i++)
        {
            if (!mRecentList[i].IsMapping)
                continue;

            mItems[i].GetComponent<PlaybackItemWnd>().Bind(mRecentList[i].AsMapping);

            mItems[i].SetActive(true);
        }

        mUIScrollView.ResetPosition();

        mGrid.Reposition();
    }

    /// <summary>
    /// 查询发布视频
    /// </summary>
    void OnQueryPublishVideos(int eventId, MixedValue para)
    {
        mPlaybackWnd.mIsGetPublishVideo = true;

        // 绘制发布列表
        RedrawRecentCombatList();
    }

}
