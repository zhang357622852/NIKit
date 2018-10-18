/// <summary>
/// ApplicationItemWnd.cs
/// Created by fengsc 2018/01/31
/// 申请、邀请基础格子
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class ApplicationItemWnd : WindowBase<ApplicationItemWnd>
{
    #region 成员变量

    // 公会旗帜
    public FlagItemWnd mFlagItemWnd;

    // 公会名称
    public UILabel mName;

    // 公会实力
    public UISprite[] mStars;

    // 拒绝按钮
    public GameObject mRefuseBtn;

    // 公会人数
    public UILabel mAmount;

    public UILabel mTips;

    // 查看按钮
    public GameObject mViewBtn;
    public UILabel mViewBtnLb;

    // 同意按钮
    public GameObject mAgreeBtn;

    LPCMapping mData = LPCMapping.Empty;

    CallBack[] mCallBacks;

    bool mIsOperateGang = false;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 注册按钮点击事件
        RegisterEvent();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mRefuseBtn).onClick = OnClickRefuseBtn;
        UIEventListener.Get(mViewBtn).onClick = OnClickViewBtn;
        UIEventListener.Get(mAgreeBtn).onClick = OnClickAgreeBtn;
    }

    /// <summary>
    /// 拒绝按钮点击事件回调
    /// </summary>
    void OnClickRefuseBtn(GameObject go)
    {
        if (mIsOperateGang)
            return;

        mIsOperateGang = true;

        if (mCallBacks[1] != null)
            mCallBacks[1].Go(mData);

        mIsOperateGang = false;
    }

    /// <summary>
    /// 查看按钮点击回调
    /// </summary>
    void OnClickViewBtn(GameObject go)
    {
        // 获取公会详情
        GangMgr.GetGangDetails(mData.GetValue<string>("gang_name"));
    }

    /// <summary>
    /// 同意按钮点击回调
    /// </summary>
    void OnClickAgreeBtn(GameObject go)
    {
        if (mIsOperateGang)
            return;

        mIsOperateGang = true;

        if (mCallBacks[0] != null)
            mCallBacks[0].Go(mData);

        mIsOperateGang = false;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mViewBtnLb.text = LocalizationMgr.Get("GangWnd_28");

        // 公会名称
        mName.text = mData.GetValue<string>("gang_name");

        // TODO:
        for (int i = 0; i < mStars.Length; i++)
            mStars[i].spriteName = "arena_star_bg";

        // 公会人数
        mAmount.text = string.Format("{0}/{1}", mData.GetValue<int>("amount"), mData.GetValue<int>("max_count"));

        int state = mData.GetValue<int>("state");

        // 是否是申请信息
        bool isApplication = state == 0 ? true : false;

        if (isApplication)
        {
            mAgreeBtn.SetActive(false);

            mViewBtn.transform.localPosition = new Vector3(
                187,
                mViewBtn.transform.localPosition.y,
                mViewBtn.transform.localPosition.z
            );

            mTips.text = LocalizationMgr.Get("GangWnd_61");
        }
        else
        {
            mViewBtn.transform.localPosition = new Vector3(
                118,
                mViewBtn.transform.localPosition.y,
                mViewBtn.transform.localPosition.z
            );

            mAgreeBtn.SetActive(true);

            mTips.text = LocalizationMgr.Get("GangWnd_60");
        }
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data)
    {
        mData = data;

        if (mData == null || mData.Count == 0)
            return;

        // 绘制窗口
        Redraw();
    }

    /// <summary>
    /// 设置回调
    /// </summary>
    public void SetCallBack(params CallBack[] para)
    {
        mCallBacks = para;
    }
}
