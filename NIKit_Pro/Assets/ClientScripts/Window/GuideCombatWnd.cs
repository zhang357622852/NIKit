/// <summary>
/// GuideCombatWnd.cs
/// Created by fengsc 2017/11/07
/// 战斗指引窗口
/// </summary>
using UnityEngine;
using System.Collections;

public class GuideCombatWnd : WindowBase<GuideCombatWnd>
{
    // 关闭按钮
    public GameObject mCloseBtn;

    public UILabel mTitle;

    public UILabel mDesc;

    // 优势标题
    public UILabel mOddsTitle;

    // 优势描述
    public UILabel mDddsDesc1;
    public UILabel mDddsDesc2;
    public UILabel mDddsDesc3;
    public UILabel mDddsDesc4;

    // 均势标题
    public UILabel mParityTitle;

    // 均势描述
    public UILabel mParityDesc1;

    // 劣势标题
    public UILabel  mInferiorTitle;

    // 劣势描述
    public UILabel  mInferiorDesc1;
    public UILabel  mInferiorDesc2;
    public UILabel  mInferiorDesc3;
    public UILabel  mInferiorDesc4;

    // Use this for initialization
    void Start ()
    {
        InitLabel();

        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
    }

    /// <summary>
    /// 初始化文本
    /// </summary>
    void InitLabel()
    {
        mTitle.text = LocalizationMgr.Get("GuideCombatWnd_1");
        mDesc.text = LocalizationMgr.Get("GuideCombatWnd_2");

        mOddsTitle.text = LocalizationMgr.Get("GuideCombatWnd_3");
        mDddsDesc1.text = LocalizationMgr.Get("GuideCombatWnd_4");
        mDddsDesc2.text = LocalizationMgr.Get("GuideCombatWnd_5");
        mDddsDesc3.text = LocalizationMgr.Get("GuideCombatWnd_6");
        mDddsDesc4.text = LocalizationMgr.Get("GuideCombatWnd_7");

        mParityTitle.text = LocalizationMgr.Get("GuideCombatWnd_8");
        mParityDesc1.text = LocalizationMgr.Get("GuideCombatWnd_9");

        mInferiorTitle.text = LocalizationMgr.Get("GuideCombatWnd_10");
        mInferiorDesc1.text = LocalizationMgr.Get("GuideCombatWnd_11");
        mInferiorDesc2.text = LocalizationMgr.Get("GuideCombatWnd_12");
        mInferiorDesc3.text = LocalizationMgr.Get("GuideCombatWnd_13");
        mInferiorDesc4.text = LocalizationMgr.Get("GuideCombatWnd_14");
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }
}
