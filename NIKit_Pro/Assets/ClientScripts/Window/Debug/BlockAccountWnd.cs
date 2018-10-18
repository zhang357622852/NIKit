/// <summary>
/// BlockAccountWnd.cs
/// Created by zhaozy Mar/26/2015
/// Debug封禁账号
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class BlockAccountWnd : WindowBase<BlockAccountWnd>
{
    #region 公共字段
    public UIInput    mAccountInput;    // Account输入框
    public UIInput    mReasonInput;     // 原因输入框
    public GameObject mSureBtn;         // 确认按钮
    public GameObject mCancelBtn;       // 取消按钮
    public GameObject mCloseBtn;       // 取消按钮
    #endregion

    #region 私有字段
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
        UIEventListener.Get(mCloseBtn).onClick += OnCloseBtn;
    }

    /// <summary>
    /// 确认封禁点击回调
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnSureBtn(GameObject go)
    {
        // 解析输入
        string account = mAccountInput.label.text.Trim();
        if (string.IsNullOrEmpty(account))
        {
            DialogMgr.Notify("请输入账号。");
            return;
        }

        // 获取封禁原因
        string reason = mReasonInput.label.text.Trim();

        // 发送封禁消息
        Operation.CmdAdminBlockAccount.Go(account, 1, reason);
    }

    /// <summary>
    /// 取消封禁点击回调
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnCancelBtn(GameObject go)
    {
        // 解析输入
        string account = mAccountInput.label.text.Trim();
        if (string.IsNullOrEmpty(account))
        {
            DialogMgr.Notify("请输入账号。");
            return;
        }

        // 获取封禁原因
        string reason = mReasonInput.label.text.Trim();

        // 发送解除封禁消息
        Operation.CmdAdminBlockAccount.Go(account, 0, reason);
    }

    /// <summary>
    /// 关闭点击回调
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnCloseBtn(GameObject go)
    {
        Destroy(gameObject);
    }
}
