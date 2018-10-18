/// <summary>
/// CmdSetSharePet.cs
/// Created by fengsc 2016/11/07
/// 通知服务器设置共享魔灵
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public partial class Operation
{
    public class CmdSetSharePet
    {
        public string GetName()
        {
            return "cmd_set_share_pet";
        }

        // 消息入口
        public static bool Go(string petRid)
        {
            // 向服务器发送消息
            Communicate.Send2GS("CMD_SET_SHARE_PET", PackArgs("pet_rid", petRid));
            return true;
        }
    }
}
