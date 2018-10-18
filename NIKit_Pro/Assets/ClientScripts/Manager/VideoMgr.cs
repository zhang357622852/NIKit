/// <summary>
/// VideoMgr.cs
/// Created from zhaozy 2018/02/27
/// 视频管理
/// </summary>

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using LPC;

public class VideoMgr
{
    #region 变量

    /// <summary>
    /// 缓存推荐视频列表
    /// </summary>
    private static LPCArray mRecommendVideos = LPCArray.Empty;

    /// <summary>
    /// 缓存发布视频列表
    /// </summary>
    private static LPCArray mPublishVideos = LPCArray.Empty;

    /// <summary>
    /// 缓存临时视频详细信息
    /// </summary>
    private static LPCMapping mVideoDetails = LPCMapping.Empty;

    #endregion

    #region 属性

    /// <summary>
    /// 缓存推荐视频列表
    /// </summary>
    public static LPCArray RecommendVideos
    {
        get
        {
            return mRecommendVideos;
        }
        set
        {
            mRecommendVideos = value;
        }
    }

    /// <summary>
    /// 缓存发布视频列表
    /// </summary>
    public static LPCArray PublishVideos
    {
        get
        {
            return mPublishVideos;
        }
        set
        {
            mPublishVideos = value;
        }
    }

    /// <summary>
    /// 缓存临时视频详细信息
    /// </summary>
    public static LPCMapping VideoDetails
    {
        get
        {
            return mVideoDetails;
        }
        set
        {
            mVideoDetails = value;

            // 尝试还原数据
            LPCValue combatData = mVideoDetails["combat_data"];
            if (combatData != null && combatData.IsBuffer)
            {
                // 还原战斗数据
                LPCValue.RestoreFromBuffer(
                    Zlib.Decompress(combatData.AsBuffer),
                    0,
                    out combatData);

                // 重置数据
                mVideoDetails.Add("combat_data", combatData);
            }
        }
    }

    /// <summary>
    /// 标识查询结束
    /// </summary>
    public static bool IsQueryEnd { get; private set; }

    #endregion

    #region 私有接口

    /// <summary>
    /// 获取视频详细信息结果
    /// </summary>
    private static void DoGetVideoDetailsOperateDone(bool result, LPCValue extraData)
    {
        // 获取视频详细信息失败
        if (!result)
        {
            // 视频已经过期
            if (extraData.AsMapping.ContainsKey("invalid"))
            {
                // 提示视频过期
                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("ArenaWnd_16"));
            }
            else
            {
                // 提示重试
                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("ArenaWnd_17"));
            }
            return;
        }

        // 缓存数据
        VideoDetails = extraData.AsMapping;

        // 抛出获取视频详细信息事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_VIDEO_DETAILS, null);
    }

    /// <summary>
    /// 播放视频结果
    /// </summary>
    private static void DoPlayVideoOperateDone(bool result, LPCValue extraData)
    {
        // 播放视频失败
        if (!result)
        {
            // 视频已经过期
            if (extraData.AsMapping.ContainsKey("invalid"))
            {
                // 提示视频过期
                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("ArenaWnd_11"));
            }
            else
            {
                // 提示重试
                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("ArenaWnd_12"));
            }
        }
        else
        {
            // 缓存视频详细数据
            VideoDetails = extraData.AsMapping;

            // 开始播放视频
            DoPlayVideo(extraData.AsMapping);
        }

        EventMgr.FireEvent(EventMgrEventType.EVENT_PLAY_VIDEO, MixedValue.NewMixedValue<bool>(result));
    }

    /// <summary>
    /// 发布视频结果
    /// </summary>
    private static void DoRefreshRecommendVideoOperateDone(bool result, LPCValue extraData)
    {
        // 刷新失败，请重试
        if (!result)
            return;

        // 记录数据
        mRecommendVideos = extraData.AsMapping.GetValue<LPCArray>("video_list");

        // 刷新列表成功，抛出事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_REFRESH_PLAYBACK_LIST, null);
    }

    /// <summary>
    /// 获取发布视频结果
    /// </summary>
    private static void DoQueryPublishVideoOperateDone(bool result, LPCValue extraData)
    {
        // 获取失败，请重试
        if (!result)
            return;

        // 获取视频信息
        LPCArray videoList = extraData.AsMapping.GetValue<LPCArray>("video_list");
        List<string> videoIds = new List<string>();

        // 判断是否已经结果
        if (videoList.Count < GameSettingMgr.GetSettingInt("fetch_video_amount"))
            IsQueryEnd = true;

        // 获取已经存在的视频列表
        foreach(LPCValue data in mPublishVideos.Values)
            videoIds.Add(data.AsMapping.GetValue<string>("id"));

        // 剔除重复数据
        foreach (LPCValue data in videoList.Values)
        {
            // 已经存在列表中
            if (videoIds.IndexOf(data.AsMapping.GetValue<string>("id")) != -1)
                continue;

            // 添加到列表中
            mPublishVideos.Add(data);
        }

        // 抛出事件
        EventMgr.FireEvent(EventMgrEventType.EVNT_QUERY_PUBLISH_VIDEO_LIST, null);
    }

    /// <summary>
    /// 发布视频结果
    /// </summary>
    private static void DoPublishVideoOperateDone(bool result, LPCValue extraData)
    {
        LPCMapping para = LPCMapping.Empty;
        para.Add("result", result ? 1 : 0);

        if (result)
            para.Add("id", extraData.AsMapping.GetValue<string>("id"));

        // 抛出视频发布事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_PUBLISH_VIDEO, MixedValue.NewMixedValue<LPCMapping>(para));
    }

    /// <summary>
    /// 分享视频结果
    /// </summary>
    private static void DoShareVideoOperateDone(bool result, LPCValue extraData)
    {
        // 播放视频失败
        if (!result)
        {
            // 视频已经过期
            if (extraData.AsMapping.ContainsKey("invalid"))
            {
                // 提示视频过期
                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("ArenaWnd_11"));
            }
            else
            {
                // 提示重试
                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("ArenaWnd_12"));
            }
        }
        else
        {
            LPCMapping para = LPCMapping.Empty;
            para.Add("id", extraData.AsMapping.GetValue<string>("id"));

            // 分享成功,抛出事件
            EventMgr.FireEvent(EventMgrEventType.EVENT_SHARE_VIDEO, MixedValue.NewMixedValue<LPCMapping>(para));
        }
    }

    /// <summary>
    /// 设置本地缓存视频详细信息
    /// </summary>
    private static void DoPlayVideo(LPCMapping videoData)
    {
        // 获取视频id
        string videoId = videoData.GetValue<string>("id");

        // 获取战斗数据
        LPCMapping CombatData = videoData.GetValue<LPCMapping>("combat_data");

        // 获取参数
        string instanceId = CombatData.GetValue<string>("instance_id");

        // 打开副本加载界面
        InstanceMgr.OpenInstance(ME.user, instanceId, videoId);

        // 构建副本参数
        LPCMapping dbase = new LPCMapping();
        dbase.Add("rid", videoId);
        dbase.Add("instance_id", instanceId);
        dbase.Add("random_seed", CombatData["random_seed"]);
        dbase.Add("fighter_map", CombatData["fighter_map"]);
        dbase.Add("defenders", CombatData["defenders"]);
        dbase.Add("revive_times", CombatData["revive_times"]);
        dbase.Add("level_actions", CombatData["level_actions"]);
        dbase.Append(CombatData.GetValue<LPCMapping>("extra_para"));

        // 创建副本对象
        InstanceMgr.DoPlaybackInstance(ME.user, instanceId, dbase);
    }


    #endregion

    #region 公共接口

    /// <summary>
    /// 重置相关数据数据
    /// </summary>
    public static void DoResetAll()
    {
        // 缓存推荐视频列表
        RecommendVideos = LPCArray.Empty;

        // 缓存发布视频列表
        PublishVideos = LPCArray.Empty;

        // 缓存临时视频详细信息
        VideoDetails = LPCMapping.Empty;

        // 标识查询结束
        IsQueryEnd = false;
    }

    /// <summary>
    /// 播放视频
    /// </summary>
    public static void PlayVideo(string videoId)
    {
        // 播放视频
        Operation.CmdPlayVideo.Go(videoId);
    }

    /// <summary>
    /// 刷新推荐视频
    /// </summary>
    public static void RefreshRecommendVideo(bool force)
    {
        // 如果不是强制刷新需要判断是否需要正真刷新
        if (!force)
        {
            // 抽取视频数量
            int fetchVideoAmount = GameSettingMgr.GetSettingInt("fetch_video_amount");
            int videoValidTime = GameSettingMgr.GetSettingInt("video_valid_time");
            int curTime = TimeMgr.GetServerTime();
            bool overdue = false;

            // 遍历本地视频是否已经过期
            foreach (LPCValue data in RecommendVideos.Values)
            {
                // 视频没有过期
                if (curTime < (data.AsMapping.GetValue<int>("time") + videoValidTime))
                    continue;

                // 有视频已经过期
                overdue = true;
                break;
            }

            // 本地视频没有过期而且本地缓存数量达到了缓存最大数量
            if (!overdue && RecommendVideos.Count >= fetchVideoAmount)
                return;
        }

        // 刷新推荐视频
        Operation.CmdRefreshRecommendVideo.Go(force);
    }

    /// <summary>
    /// 查询发布视频列表
    /// </summary>
    public static void QueryPublishVideo(int startIndex)
    {
        // 如果是重新查询，则需要清除原始数据
        if (startIndex == 0)
        {
            IsQueryEnd = false;
            PublishVideos = LPCArray.Empty;
        }

        // 查询发布视频列表
        Operation.CmdQueryPublishVideo.Go(startIndex);
    }

    /// <summary>
    /// 获取战斗详情
    /// </summary>
    public static void GetVideoDetails(string videoId)
    {
        if (VideoDetails == null ||
            ! string.Equals(VideoDetails.GetValue<string>("id"), videoId))
        {
            // 查询发布视频列表
            Operation.CmdGetVideoDetails.Go(videoId);
            return;
        }

        // 抛出获取视频详细信息事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_VIDEO_DETAILS, null);
    }

    /// <summary>
    /// 发布视频
    /// </summary>
    public static void PublishVideo(Property user)
    {
        // 获取玩家当前副本rid
        string rid = user.Query<string>("instance/rid");

        // 当前没有副本id
        if (string.IsNullOrEmpty(rid))
            return;

        // 发布视频
        Operation.CmdPublishVideo.Go(rid);
    }

    /// <summary>
    /// 分享视频
    /// </summary>
    public static void ShareVideo(string videoId)
    {
        // 分享视频
        Operation.CmdShareVideo.Go(videoId);
    }

    /// <summary>
    /// 执行视频操作结果
    /// </summary>
    public static void DoVideoOperateDone(string oper, bool result, LPCValue extraData)
    {
        // 根据不同的操作做不同的处理
        switch (oper)
        {
            // 获取视频详细信息
            case "get_video_details":
                DoGetVideoDetailsOperateDone(result, extraData);
                break;

            // 播放视频
            case "play_video":
                DoPlayVideoOperateDone(result, extraData);
                break;

            // 刷新推荐视频
            case "refresh_recommend_video":
                DoRefreshRecommendVideoOperateDone(result, extraData);
                break;

            // 查询发布视频列表
            case "query_publish_video":
                DoQueryPublishVideoOperateDone(result, extraData);
                break;

            // 发布视频
            case "publish_video":
                DoPublishVideoOperateDone(result, extraData);
                break;

            // 分享视频
            case "share_video":
                DoShareVideoOperateDone(result, extraData);
                break;
        }
    }

    #endregion
}
