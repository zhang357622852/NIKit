/// <summary>
/// CmdFetchPublishEntity.cs
/// create by zhaozy 2015/09/01
/// 获取发布信息
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdFetchPublishEntity : CmdHandler
    {
        public string GetName()
        {
            return "cmd_fetch_publish_entity";
        }

        // 消息入口
        public static bool Go(string publishId)
        {
            // 向服务器发送消息
            Communicate.Send2GS("CMD_FETCH_PUBLISH_ENTITY", PackArgs("publish_id", publishId));
            return true;
        }
    }
}
