/// <summary>
/// DebugForbidChat.cs
/// Created by zhaozy 2015/09/10
/// GD禁言功能
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class DebugForbidChat : MonoBehaviour
{
    #region 公共字段

    public UIInput    mUserRidInput;    // 输入框
    public UIInput    mTimeInput;       // 输入框
    public GameObject mSureBtn;         // 确认按钮
    public GameObject mCancelBtn;       // 取消按钮

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
    /// 确认按钮点击回调
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnSureBtn(GameObject go)
    {
        string userRid = mUserRidInput.label.text.Trim();
        if (string.IsNullOrEmpty(userRid))
        {
            DialogMgr.Notify("请输入玩家rid。");
            return;
        }

        int forbidTime = 0;
        if (! int.TryParse(mTimeInput.label.text.Trim(), out forbidTime))
        {
            DialogMgr.Notify("禁言时间输入格式不正确, {0}", mTimeInput.label.text.Trim());
            return;
        }

        // 通知服务器禁言
        Operation.CmdAdminSetForbidChat.Go(userRid, forbidTime);

        // 销毁窗口
        Destroy(gameObject);
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
