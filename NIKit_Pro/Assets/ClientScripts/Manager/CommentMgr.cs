/// <summary>
/// CommentMgr.cs
/// Created by zhaozy 2018/01/12
/// 评论管理模块
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using LPC;

/// <summary>
/// 好友管理
/// </summary>
public class CommentMgr
{
    #region 变量

    /// <summary>
    /// 评论列表
    /// </summary>
    private static LPCArray mCommentList = LPCArray.Empty;

    // 操作评论结果类型
    public const int COMMENT_OPERATE_OK = 0;   // 评论操作成功
    public const int COMMENT_OPERATE_INVALID = -1;  // 无效评论
    public const int COMMENT_OPERATE_FAILED = -2;  // 评论系统还未准备好
    public const int COMMENT_OPERATE_REPEAT = -3;  // 重复评论
    public const int COMMENT_OPERATE_OWNER = -4;  // 自己评论不能点赞
    public const int COMMENT_OPERATE_OTHER = -5;  // 别人评论
    public const int COMMENT_OPERATE_LENGTH_INVALID = -6;  // 评论长度无效
    public const int COMMENT_OPERATE_LEVEL_LIMIT = -7;  // 评论等级限制

    // 评论排序方式
    public const int COMMENT_SORT_TYPE_DEFAULT = 0;
    public const int COMMENT_SORT_TYPE_COMMEND = 1;

    #endregion

    #region 属性

    /// <summary>
    /// 评论列表
    /// </summary>
    public static LPCArray CommentList
    {
        get
        {
            return mCommentList;
        }

        private set
        {
            mCommentList = value;
        }
    }

    /// <summary>
    /// 当前查询使魔id
    /// </summary>
    public static int QueryClassID { get; private set; }

    /// <summary>
    /// 当前查询排序方式
    /// </summary>
    public static int QueryOrderType { get; private set; }

    /// <summary>
    /// 标识查询结束
    /// </summary>
    public static bool IsQueryEnd { get; private set; }

    /// <summary>
    /// 查询cookie
    /// </summary>
    /// <value>The query cookie.</value>
    public static string QueryCookie { get; private set; }

    #endregion

    #region  公共接口

    /// <summary>
    /// 发布评论
    /// </summary>
    public static void AddComment(int classId, string comment, bool share)
    {
        // 检查一下文本是否符合要求
        // 评论字数限制
        int maxLength = GameSettingMgr.GetSettingInt("max_comment_length");
        int minLength = GameSettingMgr.GetSettingInt("min_comment_length");
        if (comment.Length > maxLength || comment.Length < minLength)
        {
            DialogMgr.Notify(string.Format(LocalizationMgr.Get("Comment_1"), minLength, maxLength));
            return;
        }

        // 评论等级限制
        int limitLevel = GameSettingMgr.GetSettingInt("comment_limit_level");
        if (ME.user.GetLevel() < limitLevel)
        {
            DialogMgr.Notify(string.Format(LocalizationMgr.Get("Comment_12"), limitLevel));
            return;
        }

        // 发送消息
        Operation.CmdAddComment.Go(classId, comment, share);
    }

    /// <summary>
    /// 评论点赞
    /// </summary>
    public static void AddPraise(string rid)
    {
        if (string.IsNullOrEmpty(rid))
            return;

        // 发送消息
        Operation.CmdAddPraise.Go(rid);
    }

    /// <summary>
    /// 删除评论
    /// </summary>
    public static void DeleteComment(string rid)
    {
        if (string.IsNullOrEmpty(rid))
            return;

        // 发送消息删除评论
        Operation.CmdDeleteComment.Go(rid);
    }

    /// <summary>
    /// 获取评论分享使魔
    /// </summary>
    public static void QueryCommentSharePet(string userRid, int classID)
    {
        // 发送消息
        Operation.CmdQueryCommentSharePet.Go(userRid, classID);
    }

    /// <summary>
    /// 查询评论
    /// </summary>
    public static void QueryComments(int classId, int orderType, bool isReset)
    {
        // 判断是否发生了变化
        if (classId != QueryClassID ||
            orderType != QueryOrderType ||
            isReset)
        {
            IsQueryEnd = false;
            QueryCookie = Rid.New();
            CommentList = LPCArray.Empty;
        }

        // 记录查询方式
        QueryClassID = classId;
        QueryOrderType = orderType;

        // 发送消息
        Operation.CmdQueryComments.Go(classId, orderType, QueryCookie, CommentList.Count);
    }

    /// <summary>
    /// 执行好友操作结果
    /// </summary>
    public static void DoMsgCommentOperateDone(string oper, int result, LPCValue extraData)
    {
        // 根据不同的操作做不同的处理
        switch (oper)
        {
            // 增加评论操作结果
            case "add_comment":

                // 执行增加评论操作结果
                DoAddCommentResult(result, extraData);

                break;

            // 删除评论操作结果
            case "delete_comment":

                // 执行删除评论操作结果
                DoDeleteCommentResult(result, extraData);

                break;

            // 点赞评论操作结果
            case "add_praise":

                // 执行点赞评论操作结果
                DoAddPraiseResult(result, extraData);

                break;

            // 查询评论分享使魔操作
            case "query_comment_share_pet":

                // 执行查询评论分享使魔操作
                DoQueryCommentSharePetResult(result, extraData);

                break;

            // 查询评论操作
            case "query_comments":

                // 执行查询评论操作
                DoQueryCommentsResult(result, extraData);

                break;

            // 默认操作
            default:
                break;
        }

        LPCMapping para = LPCMapping.Empty;
        para.Add("result", result);
        para.Add("extra_data", extraData);
        para.Add("oper", oper);

        // 抛出操作结果事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_COMMENT_OPERATE_DONE, MixedValue.NewMixedValue<LPCMapping>(para));
    }

    #endregion

    #region 内部函数

    /// <summary>
    /// 发布评论操作结果
    /// </summary>
    private static void DoAddCommentResult(int result, LPCValue extraData)
    {
        switch (result)
        {
            // 发布评论失败，请重试
            case CommentMgr.COMMENT_OPERATE_FAILED:
                DialogMgr.Notify(LocalizationMgr.Get("Comment_2"));
                break;

            // 评论信息超过字数限制
            case CommentMgr.COMMENT_OPERATE_LENGTH_INVALID:
                DialogMgr.Notify(string.Format(LocalizationMgr.Get("Comment_1"),
                    GameSettingMgr.GetSettingInt("min_comment_length"),
                    GameSettingMgr.GetSettingInt("max_comment_length")));
                break;

            // 评论等级限制
            case CommentMgr.COMMENT_OPERATE_LEVEL_LIMIT:
                DialogMgr.Notify(string.Format(LocalizationMgr.Get("Comment_12"),
                    GameSettingMgr.GetSettingInt("comment_limit_level")));
                break;

            // 发布评论成功
            case CommentMgr.COMMENT_OPERATE_OK:
                // 如果还在评论界面需要刷新界面
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 删除评论操作结果
    /// </summary>
    private static void DoDeleteCommentResult(int result, LPCValue extraData)
    {
        switch (result)
        {
            // 删除评论失败，请重试
            case CommentMgr.COMMENT_OPERATE_FAILED:
                DialogMgr.Notify(LocalizationMgr.Get("Comment_7"));
                break;

            // 不能删除其他玩家发布的评论
            case CommentMgr.COMMENT_OPERATE_OTHER:
                DialogMgr.Notify(LocalizationMgr.Get("Comment_8"));
                break;

            // 清除本地数据
            case CommentMgr.COMMENT_OPERATE_INVALID:
            case CommentMgr.COMMENT_OPERATE_OK:
                string rid = extraData.AsString;
                int i = 0;
                while (true)
                {
                    // CommentList已经遍历结束
                    if (CommentList.Count == 0 || i >= CommentList.Count)
                        break;

                    // 获取数据
                    LPCValue data = CommentList[i];

                    i++;

                    // 不是目标评论
                    if (!string.Equals(rid, data.AsMapping.GetValue<string>("rid")))
                    {
                        i++;
                        continue;
                    }

                    // 删除数据
                    CommentList.Remove(data);
                }

                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 评论点赞操作结果
    /// </summary>
    private static void DoAddPraiseResult(int result, LPCValue extraData)
    {
        switch (result)
        {
            // 评论点赞失败
            case CommentMgr.COMMENT_OPERATE_FAILED:
                DialogMgr.Notify(LocalizationMgr.Get("Comment_3"));
                break;

            // 重复点赞
            case CommentMgr.COMMENT_OPERATE_REPEAT:

                // 你已经对该评价做出了评价！
                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("AppraiseWnd_9"));

                break;

            // 无效评论点赞失败
            case CommentMgr.COMMENT_OPERATE_INVALID:
                DialogMgr.Notify(LocalizationMgr.Get("Comment_5"));
                break;

            // 自己发布的评论不允许点赞
            case CommentMgr.COMMENT_OPERATE_OWNER:
                DialogMgr.Notify(LocalizationMgr.Get("Comment_6"));
                break;

            // 点赞成功
            case CommentMgr.COMMENT_OPERATE_OK:
                string rid = extraData.AsString;
                foreach (LPCValue data in CommentList.Values)
                {
                    // 不是目标评论
                    if (!string.Equals(rid, data.AsMapping.GetValue<string>("rid")))
                        continue;

                    // 直接修改点赞次数
                    data.AsMapping.Add("commend", data.AsMapping.GetValue<int>("commend") + 1);
                }
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 查看玩家分享使魔操作结果
    /// </summary>
    private static void DoQueryCommentSharePetResult(int result, LPCValue extraData)
    {
        switch (result)
        {
            // 查看玩家分享使魔失败，请重试。
            case CommentMgr.COMMENT_OPERATE_FAILED:
                DialogMgr.Notify(LocalizationMgr.Get("Comment_9"));
                break;

            // 查看玩家分享使魔成功
            case CommentMgr.COMMENT_OPERATE_OK:
                // 1. 获取到了宠物详细信息
                // 2. 玩家当前已经没有该使魔了
                LPCMapping sharePet = extraData.AsMapping;
                if (sharePet.Count == 0)
                    DialogMgr.Notify(LocalizationMgr.Get("Comment_10"));
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 获取评论列表操作结果
    /// </summary>
    private static void DoQueryCommentsResult(int result, LPCValue extraData)
    {
        switch (result)
        {
            // 查看玩家分享使魔失败，请重试。
            case CommentMgr.COMMENT_OPERATE_FAILED:
                DialogMgr.Notify(LocalizationMgr.Get("Comment_11"));
                break;

            // 查看玩家分享使魔成功
            case CommentMgr.COMMENT_OPERATE_OK:
                LPCMapping para = extraData.AsMapping;

                // 判断当前QueryCookie是否一致
                string cookie = para.GetValue<string>("cookie");
                if (! string.Equals(cookie, QueryCookie))
                    return;

                // 获取评论信息
                LPCArray comments = para.GetValue<LPCArray>("comments");

                // 标识已经查询结束, 或者
                int maxPieces = GameSettingMgr.GetSettingInt("max_comment_pieces");
                if (comments.Count == 0 || comments.Count < maxPieces)
                    IsQueryEnd = true;

                // 添加到列表中
                CommentList.Append(comments);

                break;

            default:
                break;
        }
    }
    #endregion
}
