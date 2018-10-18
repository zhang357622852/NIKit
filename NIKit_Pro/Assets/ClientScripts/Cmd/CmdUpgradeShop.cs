/// <summary>
/// CmdUpgradeShop.cs
/// Created by zhaozy 2016/11/19
/// 升级商店
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public partial class Operation
{
    public class CmdUpgradeShop
    {
        public string GetName()
        {
            return "cmd_upgrade_shop";
        }

        // 消息入口
        public static bool Go()
        {
            // 向服务器发送消息
            Communicate.Send2GS("CMD_UPGRADE_SHOP", PackArgs());
            return true;
        }
    }
}
