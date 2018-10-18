/// <summary>
/// Created bu zhaozy 2018/02/27
/// 刷新推荐视频列表
/// </summary>
using System;
using LPC;

public partial class Operation
{
    public class CmdRefreshRecommendVideo : CmdHandler
    {
        public string GetName()
        {
            return "cmd_refresh_recommend_video";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go(bool force)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 通知服务器发布视频
            Communicate.Send2GS("CMD_REFRESH_RECOMMEND_VIDEO", PackArgs(
                "force", (force ? 1 : 0))
            );

            // 返回成功
            return true;
        }
    }
}