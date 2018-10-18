/// <summary>
/// AccountWnd.cs
/// Created by fucj 2015-01-26
/// 登陆界面账号、密码输入界面
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public partial class AccountWnd : WindowBase<AccountWnd>
{
    #region 成员变量

    public GameObject AccountInput;

    public GameObject PassWordInput;

    public GameObject EnterBtn;

    public GameObject mConnectingServerWnd;

    /// <summary>
    ///账号默认显示文本
    /// </summary>
    public UILabel mAccountLabel;

    /// <summary>
    ///密码默认显示文本
    /// </summary>
    public UILabel mPassWordLabel;

    /// <summary>
    ///登陆按钮文字
    /// </summary>
    public UILabel mLoginBtnLabel;

    /// <summary>
    ///注册账号文字
    /// </summary>
    public UILabel mRegisterLabel;

    /// <summary>
    ///忘记密码文字
    /// </summary>
    public UILabel mForgetPwdLabel;

    #endregion

    #region 公共函数

    /// <summary>
    /// 进入游戏
    /// </summary>
    void OnEnterBtn(GameObject ob)
    {
        CheckLogin();
    }

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        //初始化Label的显示文字;
        InitLabelContent();

        // 初始化窗口
        InitWnd();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(EnterBtn).onClick = OnEnterBtn;
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        //获取本地账号;
        string account = OptionMgr.GetPublicOption("account").AsString;
        AccountInput.GetComponent<UIInput>().value = account;

        //获取本地密码;
        string password = OptionMgr.GetPublicOption("password").AsString;
        PassWordInput.GetComponent<UIInput>().value = password;
    }

    /// <summary>
    ///初始化Label的显示文字
    /// </summary>
    private void InitLabelContent()
    {
        mAccountLabel.text = LocalizationMgr.Get("LoginWnd_1");

        mPassWordLabel.text = LocalizationMgr.Get("LoginWnd_2");

        mLoginBtnLabel.text = LocalizationMgr.Get("LoginWnd_3");

        mRegisterLabel.text = LocalizationMgr.Get("LoginWnd_4");

        mForgetPwdLabel.text = LocalizationMgr.Get("LoginWnd_5");
    }

    /// <summary>
    /// 检测登陆
    /// </summary>
    /// <returns>The init.</returns>
    private void CheckLogin()
    {
        // 取得输入的账户
        string account = AccountInput.GetComponent<UIInput>().value;
        if (string.IsNullOrEmpty(account))
        {
            DialogMgr.Notify(LocalizationMgr.Get("AccountWnd_4"));
            return;
        }

        // 取得输入的密码
        string password = PassWordInput.GetComponent<UIInput>().value;
        if (string.IsNullOrEmpty(password))
        {
            DialogMgr.Notify(LocalizationMgr.Get("AccountWnd_5"));
            return;
        }
            
        // 隐藏界面
        gameObject.SetActive(false);

        // 显示点击开始游戏
        GameObject wnd = WindowMgr.GetWindow(LoginWnd.WndType);
        wnd.GetComponent<LoginWnd>().SetLoginState();

        // 验证账号（账号信息去掉\r\n， 去掉‘ ’）
        AccountMgr.CheckAccount(account.Replace((char)10, ' ').Replace((char)13, ' ').Trim(),
            password.Replace((char)10, ' ').Replace((char)13, ' ').Trim());
    }

    #endregion
}
