/// <summary>
/// NotifyWnd.cs
/// Created by fucj 2014-12-17
/// 通知窗口
/// </summary>

using UnityEngine;
using System.Collections;

public partial class NotifyWnd : WindowBase<NotifyWnd>
{
    #region 成员变量
    public UILabel desc_lab;
    public float existTime = 3.0f;

    bool isStart = false;
    float passTime = 0.0f;
    #endregion

    #region 公共函数

    /// <summary>
    /// 显示提示内容
    /// </summary>
    public void Notify(string desc, string color)
    {
        isStart = true;
        passTime = 0.0f;

        desc_lab.text = string.Format("[{0}]{1}[-]", color, desc);
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

    // Update is called once per frame
    void Update()
    {
        if (! isStart)
            return;

        passTime += Time.unscaledDeltaTime;
        if (passTime >= existTime)
        {
            isStart = false;
            passTime = 0.0f;
            WindowMgr.DestroyWindow(gameObject.name);
        }
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
    }
    #endregion
}
