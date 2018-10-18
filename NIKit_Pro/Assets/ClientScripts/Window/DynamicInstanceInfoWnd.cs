/// <summary>
/// DynamicInstanceInfoWnd.cs
/// Created by fengsc 2017/09/21
/// 动态副本信息弹框
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class DynamicInstanceInfoWnd : WindowBase<DynamicInstanceInfoWnd>
{
    // 星级
    public UISprite[] mStras;

    // 宠物头像
    public UITexture mIcon;

    // 名称
    public UILabel mName;

    // 关闭按钮
    public GameObject mCloseBtn;

    // 弹框遮罩
    public GameObject mMask;

    // 描述信息
    public UILabel mTopDesc;

    public UILabel mBottomDesc;

    // 前往按钮
    public GameObject mGotoBtn;
    public UILabel mGotoBtnLb;

    // 分享按钮
    public GameObject mShareBtn;
    public UILabel mShareLb;

    private Property mOb;

    // 当前副本的id
    private string mCurInstanceId;

    private CallBack mCallBack;

    private bool mIsShare;

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mMask).onClick = OnClickCloseBtn;
        UIEventListener.Get(mGotoBtn).onClick = OnClickGotoBtn;
        UIEventListener.Get(mShareBtn).onClick = OnClickShareBtn;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mShareBtn.SetActive(mIsShare);
        if (mIsShare)
        {
            mShareBtn.transform.localPosition = new Vector3(-110, -161, 0);
            mGotoBtn.transform.localPosition = new Vector3(110, -161, 0);
        }
        else
        {
            mGotoBtn.transform.localPosition = new Vector3(0, -161, 0);
        }

        // 当前副本的配置数据
        LPCMapping curInstance = InstanceMgr.GetInstanceInfo(mCurInstanceId);
        if (curInstance != null)
            mTopDesc.text = string.Format(LocalizationMgr.Get("DynamicInstanceInfoWnd_1"), LocalizationMgr.Get(curInstance.GetValue<string>("name")));

        mBottomDesc.text = LocalizationMgr.Get("DynamicInstanceInfoWnd_2");

        mGotoBtnLb.text = LocalizationMgr.Get("DynamicInstanceInfoWnd_3");

        mShareLb.text = LocalizationMgr.Get("DynamicInstanceInfoWnd_6");

        for (int i = 0; i < mStras.Length; i++)
            mStras[i].gameObject.SetActive(false);

        if (mOb == null)
            return;

        mName.text = string.Format("{0}「{1}」", LocalizationMgr.Get("DynamicInstanceInfoWnd_5"), LocalizationMgr.Get(mOb.GetName()));

        for (int i = 0; i < mOb.GetStar(); i++)
        {
            if (i + 1 > mStras.Length)
                break;

            mStras[i].spriteName = PetMgr.GetStarName(mOb.GetRank());

            mStras[i].gameObject.SetActive(true);
        }

        // 显示宠物头像
        mIcon.mainTexture = MonsterMgr.GetTexture(mOb.GetClassID(), mOb.GetRank());
    }

    /// <summary>
    /// 分享按钮点击回调
    /// </summary>
    void OnClickShareBtn(GameObject go)
    {
        if (mCallBack != null)
            mCallBack.Go(true, MesssgeBoxConst.SHARE);
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        if (mCallBack != null)
            mCallBack.Go(false);

        // 销毁当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 立即前往按钮点击事件
    /// </summary>
    void OnClickGotoBtn(GameObject go)
    {
        if (mCallBack != null)
            mCallBack.Go(true, MesssgeBoxConst.GOTO);

        // 销毁当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(Property ob, string instanceId, CallBack callback, bool isShare)
    {
        mOb = ob;

        mCurInstanceId = instanceId;

        mCallBack = callback;

        mIsShare = isShare;

        // 重绘窗口
        Redraw();
    }
}
