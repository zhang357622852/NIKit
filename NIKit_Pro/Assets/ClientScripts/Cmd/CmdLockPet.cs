/// <summary>
/// CmdLockPet.cs
/// Created by fengsc 2016/11/07
/// 锁定魔灵
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public partial class Operation
{
    public class CmdLockPet
    {
        public string GetName()
        {
            return "cmd_lock_pet";
        }

        // 消息入口
        public static bool Go(string petRid, int isLock)
        {
            // 向服务器发送消息
            Communicate.Send2GS("CMD_LOCK_PET", PackArgs("pet_rid", petRid, "is_lock", isLock));
            return true;
        }
    }
}
