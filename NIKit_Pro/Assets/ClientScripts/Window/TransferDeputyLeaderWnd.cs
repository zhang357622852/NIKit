/// <summary>
/// TransferDeputyLeaderWnd.cs
/// Created by fengsc 2018/01/27
/// 任命副会长窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class TransferDeputyLeaderWnd : WindowBase<TransferDeputyLeaderWnd>
{
    #region 成员变量

    // 窗口标题
    public UILabel mTitle;

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    // 副会长数量提示
    public UILabel mTips;

    // 成员管理基础格子
    public GameObject mMemberManageItemWnd;

    // 排序组件
    public UIGrid mGrid;

    public TweenScale mTweenScale;

    List<GameObject> mItemList = new List<GameObject> ();

    // 成员列表
    LPCArray mMemberList = LPCArray.Empty;

    // 副会长最大人数限制
    int mMaxDeputyLeaderNum = 0;

    // 副会长数量
    int mDeputyLeaderNum = 0;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();

        // 公会数据
        LPCValue v = ME.user.Query<LPCValue>("my_gang_info");
        if (v != null && v.IsMapping && v.AsMapping.Count != 0)
        {
            // 获取公会成员列表
            GangMgr.GetGangMemberList(v.AsMapping.GetValue<string>("relation_tag"));
        }

        // 重绘窗口
        Redraw();
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("TransferDeputyLeaderWnd");

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 窗口关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    void OnFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        // 注册获取公会成员列表事件
        EventMgr.RegisterEvent("TransferDeputyLeaderWnd", EventMgrEventType.EVENT_GET_GANG_MEMBER_LIST, OnGetGangMemberListEvent);

        // 注册任命副会长成功事件
        EventMgr.RegisterEvent("TransferDeputyLeaderWnd", EventMgrEventType.EVENT_TRANSFER_DEPUTY_LEADER, OntransferDeputyLeader);

        EventDelegate.Add(mTweenScale.onFinished, OnFinish);
    }

    /// <summary>
    /// 任命副会长成功事件回调
    /// </summary>
    void OntransferDeputyLeader(int eventId, MixedValue para)
    {
        // 刷新成员信息
        RefreshMemberData();

        // 刷新提示
        RedrawTips();
    }

    /// <summary>
    /// 获取公会成员列表事件回调 
    /// </summary>
    void OnGetGangMemberListEvent(int eventId, MixedValue para)
    {
        // 刷新成员信息
        RefreshMemberData();

        // 刷新提示
        RedrawTips();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mTitle.text = LocalizationMgr.Get("GangWnd_42");

        LPCMapping info = GangMgr.GangDetail;

        // 副会长人数限制
        mMaxDeputyLeaderNum = CALC_DEPUTY_LEADER_NUM.Call(info.GetValue<int>("level"));
    }

    /// <summary>
    /// 刷新副会长数量提示
    /// </summary>
    void RedrawTips()
    {
        // 重置数据
        mDeputyLeaderNum = 0;

        for (int i = 0; i < mMemberList.Count; i++)
        {
            // 累计副会长数量
            if (mMemberList[i].AsMapping.GetValue<string>("station") == "gang_deputy_leader")
                mDeputyLeaderNum++;
        }

        mTips.text = string.Format(LocalizationMgr.Get("GangWnd_43"), mDeputyLeaderNum, mMaxDeputyLeaderNum);
    }

    /// <summary>
    /// 刷新成员信息
    /// </summary>
    void RefreshMemberData()
    {
        mMemberManageItemWnd.SetActive(false);

        // 公会成员列表
        mMemberList = GangMgr.GangMemberList;

        // 创建基础格子
        if (mMemberList.Count > mItemList.Count)
        {
            // 创建基础格子
            for (int i = mItemList.Count; i < mMemberList.Count; i++)
                CreatedGameObject();
        }

        for (int i = 0; i < mMemberList.Count; i++)
        {
            LPCMapping data = mMemberList[i].AsMapping;

            if (i + 1 > mItemList.Count)
                continue;

            // 过滤会长
            if (data.GetValue<string>("station") == "gang_leader")
            {
                mItemList[i].SetActive(false);
                continue;
            }

            MemberManageItemWnd script = mItemList[i].GetComponent<MemberManageItemWnd>();
            if (script == null)
                continue;

            // 绑定数据
            script.Bind(data, new Vector3(84.1f, -5.3f, 0), new Vector3(84.1f, -5.3f, 0), true);

            script.SetCallBack(new CallBack(OnTransferDeputyLeaderCallBack));
        }

        // 隐藏多余的基础格子
        for (int i = mMemberList.Count; i < mItemList.Count; i++)
            mItemList[i].SetActive(false);

        // 排序子控件
        mGrid.Reposition();
    }

    /// <summary>
    /// 任命副会长点击回调
    /// </summary>
    void OnTransferDeputyLeaderCallBack(object para, params object[] param)
    {
        LPCMapping data = param[0] as LPCMapping;
        if (data == null || data.Count == 0)
            return;

        // 公会数据
        // 已经加入公会
        LPCValue v = ME.user.Query<LPCValue>("my_gang_info");
        if (v != null && v.IsMapping && v.AsMapping.Count != 0)
        {
            // 不是会长没有权限操作
            if (v.AsMapping.GetValue<string>("station") != "gang_leader")
                return;
        }

        // 人数达到上限
        if (mDeputyLeaderNum >= mMaxDeputyLeaderNum)
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_44"));

            return;
        }

        // 该玩家已经是副会长，无法任命！
        if (data.GetValue<string>("station") == "gang_deputy_leader")
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_47"));
            return;
        }

        string targetRid = data.GetValue<string>("rid");

        for (int i = 0; i < mMemberList.Count; i++)
        {
            LPCMapping member = mMemberList[i].AsMapping;

            if (member.GetValue<string>("rid") == targetRid)
            {
                DialogMgr.ShowDailog(new CallBack(OnTransferDupetyLeader, targetRid), string.Format(LocalizationMgr.Get("GangWnd_45"), member.GetValue<string>("name")));
                return;
            }
        }

        // 玩家已经不在公会中
        DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("GangWnd_46"), data.GetValue<string>("name")));
    }

    void OnTransferDupetyLeader(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        // 任命副会长
        GangMgr.AppointDeputyLeader(para as string, true);
    }

    /// <summary>
    /// 创建基础格子
    /// </summary>
    void CreatedGameObject()
    {
        GameObject clone = Instantiate(mMemberManageItemWnd);

        // 设置父级
        clone.transform.SetParent(mGrid.transform);

        clone.transform.localScale = Vector3.one;
        clone.transform.localPosition = Vector3.zero;

        clone.SetActive(true);

        mItemList.Add(clone);
    }
}
