/// <summary>
/// GuideWnd.cs
/// Created by fengsc 2017/10/27
/// 指引窗口
/// </summary>

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class GuideWnd : WindowBase<GuideWnd>
{
    // 确认按钮
    public GameObject mConfirmBtn;

    public GameObject[] mButtons;

    public GameObject mTipWnd;

    // 指引描述
    public UILabel mDescLb;

    string mDesc = string.Empty;

    CallBack mCallBack;

    CallBack[] mCallaBackList;

    Transform mBindWnd;
    string mBindWndPath;

    // Use this for initialization
    void Start ()
    {
        if (mConfirmBtn != null)
            UIEventListener.Get(mConfirmBtn).onClick = OnClickConfirmBtn;

        if (mButtons == null)
            return;

        if (mButtons.Length > 0)
            UIEventListener.Get(mButtons[0]).onClick = OnClickButton1;
        if (mButtons.Length > 1)
            UIEventListener.Get(mButtons[1]).onClick = OnClickButton2;
        if (mButtons.Length > 2)
            UIEventListener.Get(mButtons[2]).onClick = OnClickButton3;
    }

    void Update()
    {
        // 获取绑定窗口
        if (mTipWnd == null || string.IsNullOrEmpty(mBindWndPath))
            return;

        // 如果窗口还没有或得到，不处理
        if (mBindWnd == null)
            mBindWnd = GetBindWnd(mBindWndPath);

        // mBindWnd还不存在
        if (mBindWnd == null)
            return;

        // 还没有显示过显示一下
        if (! mTipWnd.activeSelf)
            mTipWnd.SetActive(true);

        // 判断位置是否需要变化
        if (Game.FloatEqual((mBindWnd.position - mTipWnd.transform.position).sqrMagnitude, 0f))
            return;

        // 重置位置
        mTipWnd.transform.position = mBindWnd.position;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        if (mDescLb != null)
            mDescLb.text = LocalizationMgr.Get(mDesc);
    }

    /// <summary>
    /// 确认按钮点击事件
    /// </summary>
    void OnClickConfirmBtn(GameObject go)
    {
        // 执行回调
        if (mCallBack != null)
            mCallBack.Go();

        // 重置回调
        mCallBack = null;
    }

    void OnClickButton1(GameObject go)
    {
        // 执行回调
        if (mCallaBackList[0] != null)
            mCallaBackList[0].Go();

        // 重置回调
        mCallaBackList[0] = null;
    }

    void OnClickButton2(GameObject go)
    {
        // 执行回调
        if (mCallaBackList[1] != null)
            mCallaBackList[1].Go();

        // 重置回调
        mCallaBackList[1] = null;
    }

    void OnClickButton3(GameObject go)
    {
        // 执行回调
        if (mCallaBackList[2] != null)
            mCallaBackList[2].Go();

        // 重置回调
        mCallaBackList[2] = null;
    }

    /// <summary>
    /// Gets the bind window.
    /// </summary>
    /// <returns>The bind window.</returns>
    /// <param name="bindWnd">Bind window.</param>
    private Transform GetBindWnd(string bindWnd)
    {
        // 如果没有绑定窗口
        if (string.IsNullOrEmpty(bindWnd))
            return null;

        // 拆分节点名
        string[] wndPaths = bindWnd.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

        // 查找根窗口
        GameObject rootWnd = GameObject.Find(wndPaths[0]);

        // 没有跟窗口
        if (rootWnd == null)
            return null;

        Transform finalWnd = rootWnd.transform;

        // 逐级查找对象
        for (int i = 1; i < wndPaths.Length; i++)
        {
            // 查找节点
            Transform ob = finalWnd.Find(wndPaths[i]);
            if (ob == null)
            {
                finalWnd = null;
                break;
            }

            // 记录目标对象
            finalWnd = ob;
        }

        // 返回最终的目标窗口对象
        return finalWnd;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Bind(string guideDesc, LPCMapping data, CallBack cb)
    {
        // 绑定数据
        mDesc = guideDesc;

        // 获取绑定窗口
        mBindWndPath = data.GetValue<string>("bind_wnd");
        mBindWnd = GetBindWnd(data.GetValue<string>("bind_wnd"));
        if (mBindWnd != null && mTipWnd != null)
        {
            // 还没有显示过显示一下
            if (! mTipWnd.activeSelf)
                mTipWnd.SetActive(true);

            // 设置位置
            mTipWnd.transform.position = mBindWnd.position;
        }

        // 设置点击回调
        mCallBack = cb;

        // 绘制窗口
        Redraw();
    }

    public void BindMultipleChoice(string desc, LPCMapping data, CallBack[] cb)
    {
        mDesc = desc;

        // 获取绑定窗口
        mBindWndPath = data.GetValue<string>("bind_wnd");
        mBindWnd = GetBindWnd(data.GetValue<string>("bind_wnd"));
        if (mBindWnd != null && mTipWnd != null)
        {
            // 还没有显示过显示一下
            if (! mTipWnd.activeSelf)
                mTipWnd.SetActive(true);

            // 设置位置
            mTipWnd.transform.position = mBindWnd.position;
        }

        // 设置回调
        mCallaBackList = cb;

        // 绘制窗口
        Redraw();
    }
}
