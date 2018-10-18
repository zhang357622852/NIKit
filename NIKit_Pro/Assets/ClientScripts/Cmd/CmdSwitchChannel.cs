/// <summary>
/// CmdSwitchChannel.cs
/// Created by fengsc 2016/12/15
/// 切换频道
/// </summary>
using UnityEngine;
using System.Collections;

public partial class Operation
{
    public class CmdSwitchChannel : CmdHandler
    {

        public string GetName()
        {
            return "cmd_switch_channel";
        }

        // 消息入口
        public static bool Go(int channel)
        {
            // 向服务器发送消息
            Communicate.Send2GS("CMD_SWITCH_CHANNEL", PackArgs("channel", channel));
            return true;
        }
    }
}
