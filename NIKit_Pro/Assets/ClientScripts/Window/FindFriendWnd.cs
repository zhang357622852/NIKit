/// <summary>
/// FindFriendWnd.cs
/// Created by fengsc 2017/01/19
/// 查找好友窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class FindFriendWnd : WindowBase<FindFriendWnd>
{
    //通过昵称查找按钮
    public UIToggle mNameFindBtn;

    //通过ID查找按钮
    public UIToggle mIDFindBtn;

    // 所有选项按钮
    public GameObject[] mButtons;

    public UIInput mInput;

    // 查找按钮
    public GameObject mFindBtn;
    public UILabel mFindBtnLb;

    // 申请的数量
    public UILabel mApplyAmount;

    // 基础格子
    public GameObject mItem;

    // 排序组件
    public UIWrapContent mWrapContent;

    public UIScrollView mScrollView;

    string mItemNamePrefix = "apply_item_";

    Dictionary<string, GameObject> mItemList = new Dictionary<string, GameObject>();

    // 申请列表
    LPCArray mApplyList = LPCArray.Empty;


    private FindType mCurFindType = FindType.None;
    private enum FindType
    {
        None = 0,
        NameFind,
        IDFind,
    }

    // Use this for initialization
    void Start ()
    {
        CreatedGameObject();

        InitText();

        RegisterEvent();

        Redraw();
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    void OnEnable()
    {
        // 监听好友请求事件
        EventMgr.RegisterEvent(FindFriendWnd.WndType, EventMgrEventType.EVENT_FRIEND_REQUEST, OnFriendRequest);

        // 监听好友操作结果事件
        EventMgr.RegisterEvent(FindFriendWnd.WndType, EventMgrEventType.EVENT_FRIEND_OPERATE_DONE, OnFriendOperateDone);

        //默认是按昵称查找
        OnClickNameFindBtn(mNameFindBtn.gameObject);
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    void OnDisable()
    {
        // 解注册事件
        EventMgr.UnregisterEvent(FindFriendWnd.WndType);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mFindBtnLb.text = LocalizationMgr.Get("FindFriendWnd_2");
        mNameFindBtn.GetComponentInChildren<UILabel>().text = LocalizationMgr.Get("FindFriendWnd_8");
        mIDFindBtn.GetComponentInChildren<UILabel>().text = LocalizationMgr.Get("FindFriendWnd_9");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mFindBtn).onClick = OnClickFindBtn;
        UIEventListener.Get(mNameFindBtn.gameObject).onClick = OnClickNameFindBtn;
        UIEventListener.Get(mIDFindBtn.gameObject).onClick = OnClickIDFindBtn;
    }

    /// <summary>
    /// 好友请求信息事件回调
    /// </summary>
    void OnFriendRequest(int eventId, MixedValue para)
    {
        // 刷新申请列表
        Redraw();

        mInput.value = string.Empty;

        // 移除焦点
        mInput.RemoveFocus();

        // 更新输入框
        mInput.UpdateLabel();
    }

    /// <summary>
    /// 好友操作结果事件回调
    /// </summary>
    void OnFriendOperateDone(int eventId, MixedValue para)
    {
        LPCMapping map = para.GetValue<LPCMapping>();

        // 操作失败不处理
        if (map == null ||
            map.GetValue<int>("result") != FriendConst.ERESULT_OK)
            return;

        // 刷新好友申请列表
        Redraw();
    }

    /// <summary>
    /// 创建一批item
    /// </summary>
    void CreatedGameObject()
    {
        mItem.SetActive(false);
        for (int i = 0; i < GameSettingMgr.GetSettingInt("max_friend_request_amount"); i++)
        {
            GameObject go = Instantiate(mItem);
            go.transform.SetParent(mWrapContent.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;

            go.name = mItemNamePrefix + i;

            go.transform.localPosition = new Vector3(
                mItem.transform.localPosition.x,
                0 - i * 110, mItem.transform.localPosition.z);

            go.SetActive(false);

            mItemList.Add(mItemNamePrefix + i, go);
        }
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    void Redraw()
    {
        mScrollView.ResetPosition();

        // 获取申请列表
        mApplyList = FriendMgr.GetApplyList(ME.user);
        if (mApplyList == null)
            mApplyList = LPCArray.Empty;

        int amount = GameSettingMgr.GetSettingInt("max_friend_request_amount");

        // 显示申请好友的数量
        mApplyAmount.text = string.Format(
            LocalizationMgr.Get("FindFriendWnd_6"), mApplyList.Count, amount);

        if (amount < mApplyList.Count)
            return;

        if (mItemList == null || mItemList.Count < 1)
            return;

        for (int i = 0; i < mApplyList.Count; i++)
        {
            if (!mItemList.ContainsKey(mItemNamePrefix + i))
                continue;

            GameObject go = mItemList[mItemNamePrefix + i];

            if (go == null)
                continue;

            go.SetActive(true);

            go.GetComponent<ApplyFriendItemWnd>().Bind(mApplyList[i].AsMapping);
        }

        for (int i = mApplyList.Count; i < amount; i++)
        {
            if (!mItemList.ContainsKey(mItemNamePrefix + i))
                continue;

            mItemList[mItemNamePrefix + i].SetActive(false);
        }

    }

    /// <summary>
    /// 设置按钮文本的位置
    /// </summary>
    private void SetButtonTextPos(GameObject go)
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
                bc.enabled = false;
                labelTrans.localPosition = new Vector3(labelTrans.transform.localPosition.x, 11,labelTrans.transform.localPosition.z);
            }
            else
            {
                bc.enabled = true;
                labelTrans.localPosition = new Vector3(labelTrans.transform.localPosition.x, 7, labelTrans.transform.localPosition.z);
            }
        }
    }

    /// <summary>
    /// 查找按钮点击事件
    /// </summary>
    void OnClickFindBtn(GameObject go)
    {
        string str = string.Empty;
        str = mInput.value;
        //除去头尾两边的空格
        str = str.Trim();

        switch (mCurFindType)
        {
            case FindType.NameFind:
                if (string.IsNullOrEmpty(str))
                {
                    DialogMgr.Notify(LocalizationMgr.Get("FindFriendWnd_1"));
                    return;
                }

                // 无法给自己发送好友申请
                if (str.Equals(ME.user.GetName()))
                {
                    DialogMgr.Notify(LocalizationMgr.Get("FindFriendWnd_7"));
                    return;
                }

                if (WindowMgr.OpenWnd(FriendViewWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND) == null)
                    return;

                //通过昵称
                Operation.CmdDetailAppearance.Go(string.Empty, str);
                break;

            case FindType.IDFind:
                if (string.IsNullOrEmpty(str))
                {
                    DialogMgr.Notify(LocalizationMgr.Get("FindFriendWnd_11"));
                    return;
                }

                // 无法给自己发送好友申请
                if (str.Equals(ME.user.GetRid()))
                {
                    DialogMgr.Notify(LocalizationMgr.Get("FindFriendWnd_7"));
                    return;
                }

                if (WindowMgr.OpenWnd(FriendViewWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND) == null)
                    return;

                //通过ID查找
                Operation.CmdDetailAppearance.Go(DomainAddress.GenerateDomainAddress("c@" + str, "u", 0));
                break;
        }
    }

    /// <summary>
    /// 通过昵称查找
    /// </summary>
    /// <param name="go"></param>
    private void OnClickNameFindBtn(GameObject go)
    {
        mCurFindType = FindType.NameFind;
        SetButtonTextPos(go);
        mInput.value = string.Empty;
        mInput.defaultText = LocalizationMgr.Get("FindFriendWnd_3");
    }

    /// <summary>
    /// 通过ID查找
    /// </summary>
    /// <param name="go"></param>
    private void OnClickIDFindBtn(GameObject go)
    {
        mCurFindType = FindType.IDFind;
        SetButtonTextPos(go);
        mInput.value = string.Empty;
        mInput.defaultText = LocalizationMgr.Get("FindFriendWnd_10");
    }
}
