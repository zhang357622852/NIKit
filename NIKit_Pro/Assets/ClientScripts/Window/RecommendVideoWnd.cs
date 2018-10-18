/// <summary>
/// RecommendVideoWnd.cs
/// Created by fengsc 2018/03/05
/// 推荐战斗
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class RecommendVideoWnd : WindowBase<RecommendVideoWnd>
{
    public GameObject mItem;

    public UIGrid mGrid;

    public UIScrollView mUIScrollView;

    public PlaybackWnd mPlaybackWnd;

    List<GameObject> mItems = new List<GameObject>();

    void Awake()
    {
        // 注册事件
        RegisterEvent();

        mItem.SetActive(false);
    }

    void OnEnable()
    {
        // 刷新推荐列表
        if (mPlaybackWnd.mIsRefreshRecommend)
        {
            VideoMgr.RefreshRecommendVideo(false);

            mPlaybackWnd.mIsRefreshRecommend = false;
        }

        RedrawRecommendList();
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("RecommendVideoWnd");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册EVENT_REFRESH_PLAYBACK_LIST事件
        EventMgr.RegisterEvent("RecommendVideoWnd", EventMgrEventType.EVENT_REFRESH_PLAYBACK_LIST, OnRefreshPlaybackList);
    }

    void CreateGameObject()
    {
        GameObject item = Instantiate(mItem);

        item.transform.SetParent(mGrid.transform);

        item.transform.localPosition = Vector3.zero;

        item.transform.localScale = Vector3.one;

        mItems.Add(item);
    }

    /// <summary>
    /// 绘制推荐对战列表
    /// </summary>
    void RedrawRecommendList()
    {
        for (int i = 0; i < mItems.Count; i++)
            mItems[i].SetActive(false);

        // 获取推荐列表
        LPCArray recommendList = VideoMgr.RecommendVideos;

        int amount = recommendList.Count - mItems.Count;
        if (amount > 0)
        {
            for (int i = 0; i < amount; i++)
                CreateGameObject();
        }

        for (int i = 0; i < recommendList.Count; i++)
        {
            if (!recommendList[i].IsMapping)
                continue;

            mItems[i].GetComponent<PlaybackItemWnd>().Bind(recommendList[i].AsMapping);

            mItems[i].SetActive(true);
        }

        mUIScrollView.ResetPosition();

        mGrid.Reposition();
    }

    /// <summary>
    /// 刷新回放列表回调
    /// </summary>
    void OnRefreshPlaybackList(int eventId, MixedValue para)
    {
        // 绘制推荐列表
        RedrawRecommendList();
    }
}
