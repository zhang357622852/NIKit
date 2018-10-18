/// <summary>
/// MyFriendWnd.cs
/// Created by fengsc 2017/01/19
/// 我的好友窗口界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class MyFriendWnd : WindowBase<MyFriendWnd>
{
    // 基础格子
    public GameObject mItem;

    // 排序组件
    public UIWrapContent mWrapContent;

    // 好友列表
    public LPCArray mFriendList = LPCArray.Empty;

    public UIScrollView mScrollView;

    public UIPanel mPanel;

    // 好友格子缓存列表
    Dictionary<string, GameObject> mItemList = new Dictionary<string, GameObject>();

    Dictionary<int, int> mIndexMap = new Dictionary<int, int>();

    // 起始位置
    Dictionary<GameObject,Vector3> rePosition = new Dictionary<GameObject,Vector3>();

    // 初始格子数量
    int mInitItemNum = 6;

    string mItemNamePrefix = "my_friend_item_";

    bool mIsReset = true;

    void Awake()
    {
        CreatedGameObject();
    }

    void OnEnable()
    {
        RegisterEvent();

        // 刷新页面, 重置面板
        Redraw();

        mIsReset = false;
    }

    void OnDisable()
    {
        mIsReset = true;

        mWrapContent.onInitializeItem -= UpdateItem;

        SpringPanel springPanel = mScrollView.transform.GetComponent<SpringPanel>();
        if (springPanel != null)
            springPanel.target = Vector3.zero;

        // 解注册事件
        EventMgr.UnregisterEvent(MyFriendWnd.WndType);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        mWrapContent.onInitializeItem = UpdateItem;

        // 监听好友操作结果事件
        EventMgr.RegisterEvent(MyFriendWnd.WndType, EventMgrEventType.EVENT_FRIEND_OPERATE_DONE, OnFriendOperateDone);

        // 监听好友更新事件
        EventMgr.RegisterEvent(MyFriendWnd.WndType, EventMgrEventType.EVENT_FRIEND_NOTIFY_LIST, OnFriendNotifyList);
    }

    /// <summary>
    /// 好友操作结果回调
    /// </summary>
    void OnFriendOperateDone(int eventId, MixedValue para)
    {
        LPCMapping map = para.GetValue<LPCMapping>();

        if (map == null)
            return;

        // 操作结果
        int result = map.GetValue<int>("result");

        // 操作类型
        string oper = map.GetValue<string>("oper");

        // 刷新好友列表
        if ((oper.Equals("agree") || oper.Equals("remove")) && result.Equals(FriendConst.ERESULT_OK))
            Redraw();
    }

    /// <summary>
    /// 好友列表变化事件回调
    /// </summary>
    void OnFriendNotifyList(int eventId, MixedValue para)
    {
        // 刷新好友列表
        Redraw();
    }

    /// <summary>
    /// 创建一批item
    /// </summary>
    void CreatedGameObject()
    {
        mItem.SetActive(false);
        for (int i = 0; i < mInitItemNum; i++)
        {
            GameObject go = Instantiate(mItem);
            go.transform.SetParent(mWrapContent.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;

            go.name = mItemNamePrefix + i;

            go.transform.localPosition = new Vector3(
                mItem.transform.localPosition.x,
                0 - i * 120, mItem.transform.localPosition.z);

            // 记录item初始位置
            rePosition.Add(go, go.transform.localPosition);

            go.SetActive(false);

            mItemList.Add(mItemNamePrefix + i, go);
        }
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    void Redraw()
    {
        // 好友列表
        mFriendList = FriendMgr.FriendList;
        if (mFriendList == null)
            mFriendList = LPCArray.Empty;

        if (mItemList == null || mItemList.Count < 1 || mFriendList.Count == 0)
        {
            foreach (GameObject item in mItemList.Values)
                item.SetActive(false);

            return;
        }

        // 复用格子
        int amount = mFriendList.Count > mInitItemNum ? mFriendList.Count : mInitItemNum;

        // 启用格子复用组件
        mWrapContent.minIndex = -(amount - 1);
        mWrapContent.maxIndex = 0;

        foreach (GameObject item in mItemList.Values)
            item.SetActive(true);

        if (! mWrapContent.enabled)
            mWrapContent.enabled = true;

        //固定滚动条
        mScrollView.DisableSpring();

        if (mFriendList.Count < mInitItemNum || mIsReset)
        {
            foreach (GameObject item in rePosition.Keys)
                item.transform.localPosition = rePosition[item];

            // 此处是要还原scrollview的位置，
            // 但是没找到scrollview具体可用的接口
            mPanel.clipOffset = Vector2.zero;
            mScrollView.transform.localPosition = Vector3.zero;

            if (mIndexMap != null)
                mIndexMap.Clear();

            for (int i = 0; i < mInitItemNum; i++)
                mIndexMap.Add(i, -i);
        }

        foreach (KeyValuePair<int, int> index in mIndexMap)
            FillData(index.Key, index.Value);
    }

    void UpdateItem(GameObject go, int wrapIndex, int realIndex)
    {
        // 将index与realindex对应关系记录下来
        mIndexMap[wrapIndex] = realIndex;

        // 填充数据
        FillData(wrapIndex, realIndex);
    }

    /// <summary>
    /// 填充数据
    /// </summary>
    void FillData(int wrapIndex, int realIndex)
    {
        // 没有获取到好友列表
        if (mFriendList == null || mFriendList.Count < 1)
            return;

        int index = Mathf.Abs(realIndex);

        GameObject item;

        if (!mItemList.TryGetValue(mItemNamePrefix + wrapIndex, out item))
            return;

        if (index + 1 > mFriendList.Count)
        {
            item.SetActive(false);

//            Vector3 pos = new Vector3(
//                              mScrollView.transform.localPosition.x,
//                              mFriendList.Count * mWrapContent.itemSize - mPanel.GetViewSize().y - 9,
//                              mScrollView.transform.localPosition.z);
//
//            mPanel.clipOffset = new Vector2(0, -pos.y);
//            mScrollView.transform.localPosition = pos;

            return;
        }

        if (item == null)
            return;

        if (!item.activeSelf)
            item.SetActive(true);

        // 绑定数据
        item.GetComponent<MyFriendItemWnd>().Bind(mFriendList[index].AsMapping);
    }
}
