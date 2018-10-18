/// <summary>
/// BlockTopWnd.cs
/// Created by cql 2015/04/11
/// Debug 屏蔽上榜
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class BlockTopWnd : WindowBase<BlockTopWnd>
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
    /// 确认屏蔽点击回调
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnSureBtn(GameObject go)
    {
        // 解析输入
        string rid = mAccountInput.label.text.Trim();
        if (string.IsNullOrEmpty(rid))
        {
            DialogMgr.Notify("请输入账号对应的RID");
            return;
        }

        // 获取屏蔽原因
        string reason = mReasonInput.label.text.Trim();

        // 发送屏蔽消息
        Operation.CmdAdminBlockTop.Go(rid, 1, reason);
    }

    /// <summary>
    /// 取消解除点击回调
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnCancelBtn(GameObject go)
    {
        // 解析输入
        string rid = mAccountInput.label.text.Trim();
        if (string.IsNullOrEmpty(rid))
        {
            DialogMgr.Notify("请输入账号对应的RID");
            return;
        }

        // 获取屏蔽原因
        string reason = mReasonInput.label.text.Trim();

        // 发送屏蔽消息
        Operation.CmdAdminBlockTop.Go(rid, 0, reason);
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
