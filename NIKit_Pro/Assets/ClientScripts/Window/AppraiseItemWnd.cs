/// <summary>
/// AppraiseItemWnd.cs
/// Created by fengsc 2018/01/09
/// 玩家评价基础格子
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class AppraiseItemWnd : WindowBase<AppraiseItemWnd>
{
    // 推荐
    public UILabel mRecommendLb;

    // 玩家名称
    public UILabel mName;

    // 玩家等级
    public UILabel mLevel;

    // 评价内容
    public UILabel mContent;

    // 删除按钮
    public UISprite mDeleteBtn;

    // 点赞按钮
    public GameObject mApproveBtn;

    // 点赞数量
    public UILabel mApproveAmount;

    public UISprite mBG;

    // 评论数据
    LPCMapping mData = LPCMapping.Empty;

    public AppraiseWnd mAppraiseWnd;

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();

        mRecommendLb.text = LocalizationMgr.Get("AppraiseWnd_2");
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 推荐标识
        if (mData.GetValue<int>("recommend") == 1)
            mRecommendLb.gameObject.SetActive(true);
        else
            mRecommendLb.gameObject.SetActive(false);

        if (mData.GetValue<int>("share") == 1)
        {
            // 玩家名称
            mName.text = string.Format("[00CAFF][u]{0}[/u][-]", mData.GetValue<string>("owner_name"));
        }
        else
        {
            // 玩家名称
            mName.text = string.Format("[FFDD8A]{0}[-]", mData.GetValue<string>("owner_name"));
        }

        if (ME.user.GetRid() == mData.GetValue<string>("owner_rid"))
            mDeleteBtn.gameObject.SetActive(true);
        else
            mDeleteBtn.gameObject.SetActive(false);

        // 玩家等级
        mLevel.text = string.Format(LocalizationMgr.Get("AppraiseWnd_14"), mData.GetValue<int>("owner_level"));

        // 点赞数
        mApproveAmount.text = mData.GetValue<int>("commend").ToString();

        // 评论
        mContent.text = mData.GetValue<string>("comment");

        mContent.UpdateNGUIText();

        // 计算评价的总长度
        Vector2 size = NGUIText.CalculatePrintedSize(mContent.text);

        int count = (int) size.x / mContent.width;

        int remain = Mathf.RoundToInt(size.x % mContent.width);

        // 修正行数
        if (remain > 0)
            count++;

        float mOffsetX = 25f;

        if (mContent.width - remain <= mDeleteBtn.width + mOffsetX)
        {
            // 另起一行计算删除按钮的位置
            mDeleteBtn.transform.localPosition = new Vector3(
                mContent.transform.localPosition.x + mDeleteBtn.width / 2,
                mContent.transform.localPosition.y - mContent.height - mDeleteBtn.height / 2,
                0
            );
        }
        else
        {
            // 计算删除按钮的位置（最后一行评论的末尾）
            mDeleteBtn.transform.localPosition = new Vector3(
                mContent.transform.localPosition.x + remain + mOffsetX + mDeleteBtn.width / 2,
                mContent.transform.localPosition.y - Mathf.Max(count - 1, 0) * mContent.fontSize - Mathf.Max(count - 1, 0) * mContent.spacingY - mContent.fontSize / 2,
                0
            );
        }

        // 上边界间距
        int topBorder = 30;

        // 计算背景的高度
        mBG.height = mContent.height + topBorder * 2;
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mName.gameObject).onClick = OnClickName;
        UIEventListener.Get(mDeleteBtn.gameObject).onClick = OnClickDeleteBtn;
        UIEventListener.Get(mApproveBtn).onClick = OnClickApproveBtn;
    }

    /// <summary>
    /// 删除按钮点击事件回调
    /// </summary>
    void OnClickDeleteBtn(GameObject go)
    {
        DialogMgr.ShowDailog(new CallBack(OnDeleteDialogCallBack), LocalizationMgr.Get("AppraiseWnd_8"));
    }

    /// <summary>
    /// 删除评论回调
    /// </summary>
    void OnDeleteDialogCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        // 禁止删除其他人的评论
        if (mData.GetValue<string>("owner_rid") != ME.user.GetRid())
            return;

        // 删除评论
        CommentMgr.DeleteComment(mData.GetValue<string>("rid"));
    }

    /// <summary>
    /// 点赞按钮点击回调
    /// </summary>
    void OnClickApproveBtn(GameObject go)
    {
        // 无法对自己的评价点赞
        if (ME.user.GetRid() == mData.GetValue<string>("owner_rid"))
        {
            DialogMgr.Notify(LocalizationMgr.Get("AppraiseWnd_17"));

            return;
        }

        // 点赞评价
        CommentMgr.AddPraise(mData.GetValue<string>("rid"));
    }

    /// <summary>
    /// 玩家名称点击事件回调
    /// </summary>
    void OnClickName(GameObject go)
    {
        int classId = mData.GetValue<int>("class_id");

        if (mData.GetValue<int>("share") != 1)
            return;

        // 玩家没有收集该使魔
        if (!ManualMgr.IsCompleted(ME.user, mAppraiseWnd.mPetOb.GetClassID(), mAppraiseWnd.mPetOb.GetRank()))
        {
            // 您未收集到该使魔，无法查看其他人该使魔的信息。
            DialogMgr.Notify(LocalizationMgr.Get("AppraiseWnd_15"));

            return;
        }

        // 刷新数据
        if (mAppraiseWnd != null)
            mAppraiseWnd.mData = mData;

        // 查询玩家使魔信息
        CommentMgr.QueryCommentSharePet(mData.GetValue<string>("owner_rid"), classId);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data, bool isShowBg)
    {
        if (data == null || data.Count == 0)
            return;

        mData = data;

        mBG.alpha = isShowBg ? 0.11f : 0.01f;

        // 绘制界面
        Redraw();
    }
}
