/// <summary>
/// MailSelectBonusWnd.cs
/// Created by zhangwm 2018/08/15
/// 邮箱选择奖励界面: 套装选择
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class MailSelectBonusWnd : WindowBase<MailSelectBonusWnd>
{

    #region 成员变量

    // 6★ 传说套装选择
    public UILabel mTitle;

    public UIScrollView mContentSV;

    public UIGrid mContentGrid;

    public GameObject mItemPrefab;

    public GameObject mCloseBtn;

    public TweenScale mTweenScale;

    private CallBack mCallback;

    #endregion

    private void Start()
    {
        InitText();

        RegisterEvent();

        Redraw();
    }

    private void OnDestroy()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    private void InitText()
    {
        mTitle.text = LocalizationMgr.Get("MailSelectBonusWnd_1");

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    private void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    private void Redraw()
    {
        CsvFile list = EquipMgr.SuitTemplateCsv;

        if (list == null || list.count <= 0)
            return;

        CsvRow row;
        GameObject item;
        mItemPrefab.SetActive(true);
        for (int i = 0; i < list.count; i++)
        {
            row = list[i];
            item = NGUITools.AddChild(mContentGrid.gameObject, mItemPrefab);
            item.GetComponent<MailSelectBonusItemWnd>().BindData(row);
            item.GetComponent<MailSelectBonusItemWnd>().SetCallback(new CallBack(OnSelectSuitCallback));
        }
        mItemPrefab.SetActive(false);
        mContentGrid.Reposition();
    }

    void OnSelectSuitCallback(object para, params object[] _params)
    {
        int suitId = (int)_params[0];

        if (mCallback != null)
            mCallback.Go(suitId);

        OnClickCloseBtn(null);
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    private void OnClickCloseBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    #region 外部接口

    /// <summary>
    /// 设置回调
    /// </summary>
    public void SetCallBack(CallBack callBack)
    {
        if (callBack == null)
            return;

        mCallback = callBack;
    }

    #endregion

}
