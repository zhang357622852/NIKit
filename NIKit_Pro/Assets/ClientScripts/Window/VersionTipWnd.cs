/// <summary>
/// VersionTipWnd.cs
/// Created by cql 2015-07-04
/// 更新评论窗口
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class VersionTipWnd : WindowBase<VersionTipWnd>
{
    #region 外部拖拽属性

    public UILabel mTitle;              //标题
    public UILabel mDesc;               //描述
    public GameObject mRedirectBtn;     //跳转按钮
    public GameObject mCloseBtn;        //关闭窗口按钮
    public UILabel mBtnTip;             //按钮提示

    #endregion

    #region 内部成员

    private string redirectUrl = string.Empty;

    #endregion

    #region 公共函数

    /// <summary>
    /// 设置窗口内容.
    /// </summary>
    public void SetContent(string title, string desc, string tip, string url)
    {
        mTitle.text = title;
        mDesc.text = desc;
        mBtnTip.text = tip;
        redirectUrl = url;
    }

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        // 初始化窗口
        InitWnd();
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {

    }

    // 注册事件
    private void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnClose;
        UIEventListener.Get(mRedirectBtn).onClick = OnRedirect;
    }

    /// <summary>
    /// Raises the redirect event.
    /// </summary>
    private void OnRedirect(GameObject ob)
    {
        if (!string.IsNullOrEmpty(redirectUrl) && redirectUrl.StartsWith("http"))
        {
            Application.OpenURL(redirectUrl);
            WindowMgr.HideWindow(gameObject);
        }
    }

    /// <summary>
    /// Raises the colse event.
    /// </summary>
    private void OnClose(GameObject ob)
    {
        WindowMgr.HideWindow(gameObject);
    }

    #endregion
}
