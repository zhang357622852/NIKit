/// <summary>
/// CmdGetRecommendUser.cs
/// 获取公会推荐玩家
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdGetRecommendUser
    {
        public string GetName()
        {
            return "cmd_get_recommend_user";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go(int step)
        {
            // 通知服务器获取公会推荐玩家
            Communicate.Send2GS("CMD_GET_RECOMMEND_USER", PackArgs("step",step));

            return true;
        }
    }
}
