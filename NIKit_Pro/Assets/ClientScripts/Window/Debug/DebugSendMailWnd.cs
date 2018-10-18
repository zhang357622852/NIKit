/// <summary>
/// DebugSendMailWnd.cs
/// Created by cql 2015/04/22
/// Debug 发送邮件窗口
/// </summary>

using System;
using UnityEngine;
using System.Collections;
using LPC;

public class DebugSendMailWnd : MonoBehaviour
{
    #region 公共字段
    public UILabel mTitle;              // 窗口标题
    public UIInput InputAddressee;      // 目标输入框
    public UIInput InputExpire;         // 过期时间输入框
    public UIInput InputTitle;          // 标题输入框
    public UIInput InputBody;           // 内容输入框
    public UIInput InputItems;          // 奖励项输入框
    public GameObject mSureBtn;         // 确认按钮
    public GameObject mCancelBtn;       // 取消按钮

    public delegate void BtnClickedDelegate(LPCMapping para);     // 确认按钮点击事件
    #endregion

    #region 私有字段
    private BtnClickedDelegate OnSureBtnClicked;           // 确认按钮点击代理
    private bool mIsToAll = false;
    #endregion

    // Use this for initialization
    void Start()
    {
        RegisterEvent();
    }

    /// <summary>
    /// 注册窗口事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(mSureBtn).onClick += OnSureBtn;
        UIEventListener.Get(mCancelBtn).onClick += OnCancelBtn;
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    public void InitWnd(string title, bool toAll, BtnClickedDelegate onSureBtn)
    {
        mTitle.text = title;
        OnSureBtnClicked = onSureBtn;
        mIsToAll = toAll;
        InputAddressee.label.text = toAll ? string.Empty : "请输入user_name";
    }

    /// <summary>
    /// 确认按钮点击回调
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnSureBtn(GameObject go)
    {
        Destroy(gameObject);

        if (OnSureBtnClicked == null)
            return;

        string addressee = string.Empty;
        if (mIsToAll)
            addressee = "@all";
        else
            addressee = string.Format("@name{0}", InputAddressee.label.text.Trim());

        string expire = InputExpire.label.text.Trim();
        string title = InputTitle.label.text.Trim();
        string message = InputBody.label.text.Trim();
        string items = InputItems.label.text.Trim();

        if (addressee == string.Empty)
        {
            DialogMgr.Notify("目标不能为空");
            return;
        }

        int expireTime = 0;
        if (expire == string.Empty || !int.TryParse(expire, out expireTime))
        {
            DialogMgr.Notify("过期时间为空或格式不正确");
            return;
        }

        if (title == string.Empty)
        {
            DialogMgr.Notify("标题不能为空");
            return;
        }

        try
        {
            // 附件标题
            LPCValue belonging_list = LPCRestoreString.RestoreFromString(items);

            if (! belonging_list.IsArray)
                belonging_list = LPCValue.CreateArray();

            LPCMapping map = new LPCMapping();
            map.Add("addressee", LPCValue.Create(addressee));
            map.Add("expire", LPCValue.Create(expireTime));
            map.Add("title", LPCValue.Create(title));
            map.Add("message", LPCValue.Create(message));
            map.Add("belonging_list", belonging_list);

            OnSureBtnClicked(map);
        } catch (Exception ex)
        {
            DialogMgr.Notify(ex.Message);
        }
    }

    /// <summary>
    /// 取消按钮点击回调
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnCancelBtn(GameObject go)
    {
        Destroy(gameObject);
    }
}
