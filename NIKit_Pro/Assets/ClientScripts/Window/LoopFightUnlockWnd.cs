/// <summary>
/// LoopFightUnlockWnd.cs
/// Created by fengsc 2018/08/30
/// 循环战斗解锁提示窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopFightUnlockWnd : WindowBase<LoopFightUnlockWnd>
{
    public UILabel mTitle;

    public GameObject mMask;

    public GameObject mCloseBtn;

    public UILabel mTips;

    public UILabel mInstanceName;

    // Use this for initialization
    void Start ()
    {
        // 初始化文本
        mTitle.text = LocalizationMgr.Get("LoopFightUnlockWnd_0");
        mTips.text = LocalizationMgr.Get("LoopFightUnlockWnd_1");
        mInstanceName.text = LocalizationMgr.Get("LoopFightUnlockWnd_2");

        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mMask).onClick = OnClickCloseBtn;
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    /// <param name="go">Go.</param>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }
}
