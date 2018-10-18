/// <summary>
/// CmdGiftGiving.cs
/// Created by fengsc 2017/02/09
/// 赠送友情点数
/// </summary>
using UnityEngine;

public partial class Operation
{
    public class CmdGiftGiving : CmdHandler
    {
        public string GetName()
        {
            return "cmd_gift_giving";
        }

        // 消息入口
        public static bool Go(string target)
        {
            // 向服务器发送消息
            Communicate.Send2GS("CMD_GIFT_GIVING", PackArgs("target", target));
            return true;
        }
    }
}
