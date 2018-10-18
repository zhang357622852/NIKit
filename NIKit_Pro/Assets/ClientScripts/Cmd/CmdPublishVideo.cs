/// <summary>
/// Created bu zhaozy 2018/02/27
/// 发布视频
/// </summary>
using System;
using LPC;

public partial class Operation
{
    public class CmdPublishVideo : CmdHandler
    {
        public string GetName()
        {
            return "cmd_publish_video";
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

            // 通知服务器发布视频
            Communicate.Send2GS("CMD_PUBLISH_VIDEO", PackArgs("rid", rid));

            // 返回成功
            return true;
        }
    }
}