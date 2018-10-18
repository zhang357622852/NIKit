/// <summary>
/// Created bu zhaozy 2018/02/27
/// 查询发布视频列表
/// </summary>
using System;
using LPC;

public partial class Operation
{
    public class CmdQueryPublishVideo : CmdHandler
    {
        public string GetName()
        {
            return "cmd_query_publish_video";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go(int startIndex)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 通知服务器发布视频
            Communicate.Send2GS("CMD_QUERY_PUBLISH_VIDEO", PackArgs("start_index", startIndex));

            // 返回成功
            return true;
        }
    }
}