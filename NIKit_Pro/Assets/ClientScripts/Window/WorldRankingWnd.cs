/// <summary>
/// WorldRankingWnd.cs
/// Created by fengsc 2016/09/23
/// 竞技场世界排名窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class WorldRankingWnd : WindowBase<WorldRankingWnd>
{
    #region 成员变量

    public GameObject mItem;                    // 基础格子;

    public UIWrapContent mWrapContent;          // 基础格子复用组件

    LPCMapping mRankingList = new LPCMapping ();    // 排行榜列表;

    int mAmountItem = 7;                        // 实例化7个基础格子复用

    // 缓存创建的复用格子
    Dictionary<string, GameObject> mGoList = new Dictionary<string, GameObject> ();

    Dictionary<int, int> mIndexMap = new Dictionary<int, int>();

    /// <summary>
    /// The m scroll view.
    /// </summary>
    public UIScrollView mScrollView;

    // 起始位置
    private Dictionary<GameObject,Vector3> rePosition = new Dictionary<GameObject,Vector3>();

    #endregion

    void Awake()
    {
        // 注册事件
        RegisterEvent();

        // 初始化排名列表格子
        InitItem();
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    void OnEnable()
    {
        // 界面激活请求排行榜数据
        ArenaMgr.RequestArenaTopList();

        // 重置面板位置
        ResetScrollView();

        // 刷新数据
        RefreshData();
    }

    /// <summary>
    /// Resets the scroll view.
    /// </summary>
    private void ResetScrollView()
    {
        // 重新设置item的初始位置
        foreach (GameObject item in rePosition.Keys)
        {
            item.transform.localPosition = rePosition[item];
        }

        // 整理位置
        mScrollView.ResetPosition();

        // 重新初始化indexMap
        if (mIndexMap != null)
        {
            mIndexMap.Clear();
            for (int i = 0; i < mAmountItem; i++)
                mIndexMap.Add(i, -i);
        }
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        // 移除事件监听
        EventMgr.UnregisterEvent("WorldRankingWnd");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 向委托中添加方法
        mWrapContent.onInitializeItem = UpdateItem;

        EventMgr.RegisterEvent("WorldRankingWnd", EventMgrEventType.EVENT_GET_ARENA_TOP_LIST, OnGetTopListEvent);
    }

    /// <summary>
    /// 获取排行榜列表事件回调
    /// </summary>
    void OnGetTopListEvent(int eventId, MixedValue para)
    {
        // 刷新界面数据
        RefreshData();
    }

    /// <summary>
    /// 初始化复用的列表格子
    /// </summary>
    void InitItem()
    {
        mItem.SetActive(false);

        // 初始化控件
        for (int i = 0; i < mAmountItem; i++)
        {
            GameObject clone = Instantiate(mItem);
            clone.transform.SetParent(mWrapContent.transform);
            clone.transform.localScale = Vector3.one;
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localPosition = new Vector3 (clone.transform.localPosition.x,
                clone.transform.localPosition.y - i * 110, 1);

            clone.name = "ranking_item_" + i;

            clone.SetActive(false);

            rePosition.Add(clone, clone.transform.localPosition);

            mGoList.Add(clone.name, clone);
        }
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    void RefreshData()
    {
        // 没有控件
        if(mGoList.Count == 0)
            return;

        // 获取排行榜列表
        mRankingList = ArenaMgr.GetTopList();
        int count = mRankingList.Count >= mAmountItem ? mAmountItem : mRankingList.Count;

        for (int i = 0; i < count; i++)
            mGoList["ranking_item_" + i].SetActive(true);

        for (int i = count; i < mAmountItem; i++)
            mGoList["ranking_item_" + i].SetActive(false);

        for (int i = 0; i < count; i++)
        {
            if(mRankingList.Count >= mAmountItem)
                continue;

            string name = "ranking_item_" + i;

            GameObject go = null;

            if (!mGoList.TryGetValue(name, out go))
                continue;

            if (go == null)
                continue;

            go.GetComponent<WorldRankingItemWnd>().Bind(mRankingList[i].AsMapping);
        }

        if (mRankingList.Count < mAmountItem)
            return;

        mWrapContent.minIndex = -mRankingList.Count + 1;
        mWrapContent.maxIndex = 0;

        if (!mWrapContent.enabled)
            mWrapContent.enabled = true;

        // 填充数据
        foreach (KeyValuePair<int, int> index in mIndexMap)
            FillData(index.Key, index.Value);
    }

    /// <summary>
    /// 回调函数
    /// </summary>
    void UpdateItem(GameObject go, int wrapIndex, int realIndex)
    {
        // 将index与realindex对应关系记录下来
        if(! mIndexMap.ContainsKey(wrapIndex))
            mIndexMap.Add(wrapIndex, realIndex);
        else
            mIndexMap[wrapIndex] = realIndex;

        // 填充数据
        FillData(wrapIndex, realIndex);
    }

    /// <summary>
    /// 填充数据
    /// </summary>
    void FillData(int wrapIndex, int realIndex)
    {
        // 没有获取到排行榜数据
        if(mRankingList == null || mGoList.Count < 1)
            return;

        int index = Mathf.Abs(realIndex);

        if(index + 1 > mRankingList.Count)
            return;

        string name = "ranking_item_" + wrapIndex;

        GameObject wnd = null;

        if (!mGoList.TryGetValue(name, out wnd))
            return;

        if (wnd == null)
            return;

        wnd.GetComponent<WorldRankingItemWnd>().Bind(mRankingList[index].AsMapping);
    }
}
