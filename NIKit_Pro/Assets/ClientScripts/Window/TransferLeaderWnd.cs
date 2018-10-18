/// <summary>
/// TransferLeaderWnd.cs
/// Created by fengsc 2018/01/27
/// 转让公会会长界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class TransferLeaderWnd : WindowBase<TransferLeaderWnd>
{
    #region 成员变量

    // 窗口标题
    public UILabel mTitle;

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    // 描述
    public UILabel mDesc;

    // 名称输入框
    public UIInput mNameInput;

    // 确认按钮
    public GameObject mConfirmBtn;
    public UILabel mConfirmBtnLb;

    public TweenScale mTweenScale;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 初始化本地化文本
        InitText();

        // 注册事件
        RegisterEvent();
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("TransferLeaderWnd");

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mTitle.text = LocalizationMgr.Get("GangWnd_35");
        mDesc.text = LocalizationMgr.Get("GangWnd_36");
        mNameInput.defaultText = LocalizationMgr.Get("GangWnd_37");
        mConfirmBtnLb.text = LocalizationMgr.Get("GangWnd_38");
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 确认按钮点击事件
    /// </summary>
    void OnClickConfirmBtn(GameObject go)
    {
        // 获取公会成员列表
        LPCArray list = GangMgr.GangMemberList;

        string name = mNameInput.value.Trim();

        string targetRid = string.Empty;

        for (int i = 0; i < list.Count; i++)
        {
            LPCMapping data = list[i].AsMapping;

            if (data.GetValue<string>("name") == name)
            {
                targetRid = data.GetValue<string>("rid");
                break;
            }
        }

        // 不是公会成员
        if (string.IsNullOrEmpty(targetRid))
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_39"));
            return;
        }

        // 公会数据
        LPCMapping gangInfo = GangMgr.GangDetail;
        if (gangInfo != null && gangInfo.Count != 0)
        {
            // 无法将公会会长转让给自己
            if (gangInfo.GetValue<string>("station") == "gang_leader" && ME.user.GetRid() == targetRid)
            {
                DialogMgr.ShowSingleBtnDailog(new CallBack(OnTransferLeaderCallBack, targetRid), LocalizationMgr.Get("GangWnd_41"));
                return;
            }
        }

        // 通知服务器转让公会长
        GangMgr.AbdicateGangLeader(targetRid);
    }

    /// <summary>
    /// 转让公会长确认回调
    /// </summary>
    void OnTransferLeaderCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        GangMgr.AbdicateGangLeader(para as string);
    }

    void OnTweenFinish()
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
        UIEventListener.Get(mConfirmBtn).onClick = OnClickConfirmBtn;

        EventMgr.RegisterEvent("TransferLeaderWnd", EventMgrEventType.EVENT_TRANSFER_LEADER, OnTransferLeaderEvent);

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);
    }

    /// <summary>
    /// 转让会长成功事件回调
    /// </summary>
    void OnTransferLeaderEvent(int eventId, MixedValue para)
    {
        DialogMgr.ShowSingleBtnDailog(new CallBack(OnCallBack), string.Format(LocalizationMgr.Get("GangWnd_40"), mNameInput.value.Trim()));
    }

    void OnCallBack(object para, params object[] param)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }
}
