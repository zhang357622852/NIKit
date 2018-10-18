/// <summary>
/// FriendRequestWnd.cs
/// Created by fengsc 2017/01/19
/// 好友请求窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class FriendRequestWnd : WindowBase<FriendRequestWnd>
{
    // 收到请求的个数
    public UILabel mRequestAmount;

    // 拒绝所有玩家按钮
    public GameObject mRefuseAllBtn;
    public UILabel mRefuseAllBtnLb;

    // 基础格子
    public GameObject mItem;

    // 排序控件
    public UIWrapContent mWrapContent;

    public UIPanel mPanel;

    // 好友请求列表
    LPCArray mRequestList = LPCArray.Empty;

    // 初始格子数量
    int mInitItemNum = 7;

    string mItemNamePrefix = "request_item_";

    Dictionary<string, GameObject> mItemList = new Dictionary<string, GameObject>();

    Dictionary<int, int> mIndexMap = new Dictionary<int, int>();

    // Use this for initialization
    void Start ()
    {
        RegisterEvent();

        CreatedGameObject();

        Redraw();
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent(FriendRequestWnd.WndType);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mRefuseAllBtn).onClick = OnClickRefuseBtn;

        // 监听好友操作结果事件
        EventMgr.RegisterEvent(FriendRequestWnd.WndType, EventMgrEventType.EVENT_FRIEND_OPERATE_DONE, OnFriendOperateDone);

        // 监听好友请求事件
        EventMgr.RegisterEvent(FriendRequestWnd.WndType, EventMgrEventType.EVENT_FRIEND_REQUEST, OnFriendRequest);

        mWrapContent.onInitializeItem = UpdateItem;
    }

    /// <summary>
    /// 好友操作结果回调
    /// </summary>
    void OnFriendOperateDone(int eventId, MixedValue para)
    {
        LPCMapping map = para.GetValue<LPCMapping>();

        // 操作失败
        if (map == null ||
            map.GetValue<int>("result") != FriendConst.ERESULT_OK)
            return;

        // 刷新好友请求列表
        Redraw();
    }

    /// <summary>
    /// 好友请求信息回调
    /// </summary>
    void OnFriendRequest(int eventId, MixedValue para)
    {
        // 刷新好友请求列表
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

            go.SetActive(false);

            mItemList.Add(mItemNamePrefix + i, go);
        }
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    void Redraw()
    {
        // 好友请求列表
        mRequestList = FriendMgr.GetRequestList(ME.user);
        if (mRequestList == null)
            mRequestList = LPCArray.Empty;

        // 好友请求数量显示
        mRequestAmount.text = string.Format(
            LocalizationMgr.Get("FriendRequestWnd_1"), mRequestList.Count,
            GameSettingMgr.GetSettingInt("max_friend_receive_amount"));

        if (mItemList == null || mItemList.Count < 1)
            return;

        Reposition();

        if (mRequestList.Count < mInitItemNum)
        {
            for (int i = 0; i < mInitItemNum; i++)
            {
                if (!mItemList.ContainsKey(mItemNamePrefix + i))
                    continue;

                // 获取缓存列表中的格子对象
                GameObject go = mItemList[mItemNamePrefix + i];

                if (go == null)
                    continue;

                if (i + 1 > mRequestList.Count)
                {
                    go.SetActive(false);
                }
                else
                {
                    go.SetActive(true);

                    go.GetComponent<FriendRequestItemWnd>().Bind(mRequestList[i].AsMapping);
                }
            }

            mWrapContent.enabled = false;
        }
        else
        {
            int amount = mRequestList.Count > mInitItemNum ? mRequestList.Count : mInitItemNum;

            // 启用格子复用组件
            mWrapContent.minIndex = -(amount - 1);
            mWrapContent.maxIndex = 0;

            foreach (GameObject item in mItemList.Values)
                item.SetActive(true);

            mWrapContent.enabled = true;

            foreach (KeyValuePair<int, int> index in mIndexMap)
            {
                FillData(index.Key, index.Value);
            }
        }
    }

    void Reposition()
    {
        mPanel.transform.localPosition = new Vector3(85, -55, 0);

        mPanel.clipOffset = Vector2.zero;

        mIndexMap.Clear();

        for (int i = 0; i < mInitItemNum; i++)
        {
            mIndexMap.Add(i, -i);
        }

        int index = 0;
        foreach (GameObject item in mItemList.Values)
        {
            item.transform.localPosition = new Vector3(0, -mWrapContent.itemSize * index, 0);
            index++;
        }
    }

    void UpdateItem(GameObject go, int wrapIndex, int realIndex)
    {
        // 将index与realindex对应关系记录下来
        if(!mIndexMap.ContainsKey(wrapIndex))
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
        // 没有获取到好友列表
        if (mRequestList == null || mRequestList.Count < 1)
            return;

        int index = Mathf.Abs(realIndex);

        if (index + 1 > mRequestList.Count)
            return;

        GameObject item = mItemList[mItemNamePrefix + wrapIndex];

        if (item == null)
            return;

        if (!item.activeSelf)
            item.SetActive(true);

        // 绑定数据
        item.GetComponent<FriendRequestItemWnd>().Bind(mRequestList[index].AsMapping);
    }

    /// <summary>
    /// 拒绝所有玩家按钮点击事件
    /// </summary>
    void OnClickRefuseBtn(GameObject go)
    {
        if (FriendMgr.GetRequestList(ME.user).Count < 1)
            return;

        // 通知服务器拒绝所有玩家好友申请
        Operation.CmdRejectAllRequest.Go();
    }
}
