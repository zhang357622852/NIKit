/// <summary>
/// ShowBonusWnd.cs
/// Created by fengsc 2017/06/14
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class ShowBonusWnd : WindowBase<ShowBonusWnd>
{
#region 成员变量
    public UITexture mIcon;

    public TweenAlpha mIconAlpha;

    public TweenAlpha mDescBgAlpha;

    public TweenAlpha mSmallBgAlpha;

    public TweenScale mSmallbgScale;

    public TweenAlpha mBigAlpha;

    public TweenScale mBigScale;

    public TweenAlpha mWhiteLightAlpha;

    public TweenScale mWhiteLightScale;

    public UILabel mDesc;

    public GameObject mBg;

    public TweenAlpha mPanelAlha;

    LPCMapping mData = LPCMapping.Empty;
#endregion

    void Start()
    {
        RegisterEvent();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mBg).onClick = OnClickBg;
    }

    void Redraw()
    {
        string fields = FieldsMgr.GetFieldInMapping(mData);

        int classId = FieldsMgr.GetClassIdByAttrib(fields);

        string iconName = ItemMgr.GetClearIcon(classId);
        mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/item/{0}.png", iconName));

        mDesc.text = string.Format("{0}×{1}", FieldsMgr.GetFieldName(fields), mData.GetValue<int>(fields));


        mIconAlpha.PlayForward();
        mIconAlpha.ResetToBeginning();

        mDescBgAlpha.PlayForward();
        mDescBgAlpha.ResetToBeginning();

        mSmallBgAlpha.PlayForward();
        mSmallBgAlpha.ResetToBeginning();

        mSmallbgScale.PlayForward();
        mSmallbgScale.ResetToBeginning();

        mBigAlpha.PlayForward();
        mBigAlpha.ResetToBeginning();

        mBigScale.PlayForward();
        mBigScale.ResetToBeginning();

        mWhiteLightAlpha.PlayForward();
        mWhiteLightAlpha.ResetToBeginning();

        mWhiteLightAlpha.PlayForward();
        mWhiteLightScale.ResetToBeginning();

        Invoke("PlayPanelTweenAlpha", 6.0f);
    }

    /// <summary>
    /// 背景点击回调
    /// </summary>
    void OnClickBg(GameObject go)
    {
        PlayPanelTweenAlpha();
    }

    void PlayPanelTweenAlpha()
    {
        EventDelegate.Add(mPanelAlha.onFinished, OnTweenAlphaFinshed);
        mPanelAlha.PlayForward();
        mPanelAlha.ResetToBeginning();
    }

    void OnTweenAlphaFinshed()
    {
        WindowMgr.DestroyWindow(ShowBonusWnd.WndType);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping para)
    {
        mData = para;

        // 绘制窗口
        Redraw();
    }
}
