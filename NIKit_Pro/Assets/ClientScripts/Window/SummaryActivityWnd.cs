/// <summary>
/// SummaryActivityWnd.cs
/// Created by fengsc 2018/09/25
/// 活动摘要窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class SummaryActivityWnd : WindowBase<SummaryActivityWnd>
{
    public UILabel mTimeDesc;

    public GameObject mCloseBtn;

    public UILabel mTitle;

    public UILabel mSubTitle;

    public UITexture[] mBg;

    public RichTextContent mRichTextContent;

    public LPCMapping ActivityInfo { get; private set; }

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        if (this == null)
            return;

        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    ///  注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
    }

    void Redraw()
    {
        string activityId = ActivityInfo.GetValue<string>("activity_id");
        if (string.IsNullOrEmpty(activityId))
            return;

        // 活动配置数据
        LPCMapping activityConfig = ActivityMgr.GetActivityInfo(activityId);

        // 没有该活动的配置数据
        if (activityConfig == null || activityConfig.Count == 0)
            return;

        // 有效时间段
        LPCArray validPeriod = ActivityInfo.GetValue<LPCArray>("valid_period");
        if (validPeriod == null)
            validPeriod = LPCArray.Empty;

        // 活动时间描述
        mTimeDesc.text = ActivityMgr.GetActivityTimeDesc(activityId, validPeriod);

        // 活动标题
        mTitle.text = ActivityMgr.GetActivityTitle(activityId);

        // 副标题
        mSubTitle.text = ActivityMgr.GetActivitySubTitle(activityId);

        LPCArray bgs = ActivityMgr.GetActivityBg(activityId);

        for (int i = 0; i < mBg.Length; i++)
        {
            // 结束循环
            if (i + 1 > bgs.Count)
                break;

            mBg[i].mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Activity/Background/{0}.png", bgs[i].AsString));
        }

        // 活动描述
        mRichTextContent.ParseValue(ActivityMgr.GetActivityDesc(activityId));
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="itemId">Item identifier.</param>
    public void Bind(LPCMapping activityInfo)
    {
        if (activityInfo == null)
            return;

        ActivityInfo = activityInfo;

        Redraw();
    }

}
