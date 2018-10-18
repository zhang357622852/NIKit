/// <summary>
/// AccountInitWnd.cs
/// Created by fengsc 2016/07/05
/// 账号初始化窗口
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class AccountInitWnd : WindowBase<AccountInitWnd>
{

    #region 成员变量

    public UILabel mWarning;

    public UILabel mInitTips;

    public UILabel mInputTips;

    public UIInput mInput;

    /// <summary>
    ///账号初始化按钮
    /// </summary>
    public GameObject mInitBtn;

    public UILabel mInitBtnLabel;

    /// <summary>
    ///取消按钮
    /// </summary>
    public GameObject mCancelBtn;

    public UILabel mCancelBtnLabel;

    /// <summary>
    ///关闭按钮
    /// </summary>
    public GameObject mCloseBtn;

    public TweenScale mTweenScale;

    public TweenAlpha mTweenAlpha;

    int mNumber = 0;

    #endregion 

    // Use this for initialization
    void Start () 
    {
        //初始化Label;
        InitLabel();

        //注册事件;
        RegisterEvent();

        // 重绘窗口
        Redraw();
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        // 移除消息关注
        MsgMgr.RemoveDoneHook("MSG_RESET", "AccountSetWnd");
    }

    /// <summary>
    ///初始化Label
    /// </summary>
    void InitLabel()
    {
        mWarning.text = LocalizationMgr.Get("SystemWnd_27");
        mInitTips.text = LocalizationMgr.Get("SystemWnd_19");
        mInput.defaultText = LocalizationMgr.Get("SystemWnd_24");
        mInitBtnLabel.text = LocalizationMgr.Get("SystemWnd_25");
        mCancelBtnLabel.text = LocalizationMgr.Get("SystemWnd_26");
    }

    /// <summary>
    ///注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mInitBtn).onClick = OnClickInitBtn;
        UIEventListener.Get(mCancelBtn).onClick = OnClickCancelBtn;
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        MsgMgr.RegisterDoneHook("MSG_RESET", "AccountSetWnd", OnResetMsg);
    }

    /// <summary>
    /// OnResetMsg消息回调
    /// </summary>
    void OnResetMsg(string cmd, LPCValue para)
    {
        if (! para.IsMapping)
            return;

        LPCMapping data = para.AsMapping;

        // 重置失败
        if (data.GetValue<int>("result") != 1)
            return;

        //清空本地缓存
        OptionMgr.DeleteAllOption(ME.user);

        // 通知服务器退出游戏
        Operation.CmdLogout.Go();

        // 等待退出登陆消息到达服务器
        // 消息线程处理时间是间隔是0.02s，故这个地方等待时间必须要大于这个时间
        Coroutine.DispatchService(DenyBackToLogin(), "DenyBackToLogin");
    }

    /// <summary>
    /// 延迟退出游戏
    /// </summary>
    /// <returns>The wait sever.</returns>
    IEnumerator DenyBackToLogin()
    {
        // 延迟0.1s返回到登陆界面
        yield return new WaitForSeconds(0.1f);

        // 返回到登陆界面
        LoginMgr.ExitGame();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mNumber = Random.Range(100000,1000000);

        mInputTips.text = string.Format(LocalizationMgr.Get("SystemWnd_21"), mNumber);

        if (mTweenAlpha == null || mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnFinish);

        // 播放动画
        mTweenScale.PlayForward();
        mTweenAlpha.PlayForward();

        // 重置动画
        mTweenAlpha.ResetToBeginning();
        mTweenScale.ResetToBeginning();
    }

    void OnFinish()
    {
        // 移除正在打开窗口列表中的缓存
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 账号初始化按钮点击事件
    /// </summary>
    void OnClickInitBtn(GameObject go)
    {
        string value = mInput.value.Trim();

        if (string.IsNullOrEmpty(value))
            return;

        // 验证码输入错误
        if (!value.Equals(mNumber.ToString()))
        {
            DialogMgr.Notify(LocalizationMgr.Get("SystemWnd_32"));

            mInput.value = string.Empty;

            return;
        }

        // 通知服务器账号重置
        Operation.CmdReset.Go();
    }

    /// <summary>
    ///取消按钮点击事件
    /// </summary>
    void OnClickCancelBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    ///关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }
}
