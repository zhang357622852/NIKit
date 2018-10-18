/// <summary>
/// Created bu zhaozy 2018/02/27
/// 获取战斗视频详情
/// </summary>
using System;
using LPC;

public partial class Operation
{
    public class CmdGetVideoDetails : CmdHandler
    {
        public string GetName()
        {
            return "cmd_get_video_details";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        /// <param name="rid">Rid.</param>
        public static bool Go(string rid)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 通知服务器获取战斗视频详情
            Communicate.Send2GS("CMD_GET_VIDEO_DETAILS", PackArgs("rid", rid));

            // 返回成功
            return true;
        }
    }
}