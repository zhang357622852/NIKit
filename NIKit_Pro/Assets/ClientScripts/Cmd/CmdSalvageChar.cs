/// <summary>
/// CmdSalva.cs
/// Created by fengxl 2015-3-25
/// Note
/// </summary>

using System;
using LPC;
using System.Collections.Generic;

public partial class Operation
{
    /// <summary>
    /// 恢复角色
    /// </summary>
    public class CmdSalvageChar : CmdHandler
    {
        public string GetName()
        {
            return "cmd_salvage_char";
        }

        public static bool Go(string ridOrName)
        {

            // 没有玩家列表不处理
            if (!Communicate.CurrInfo.ContainsKey("delete_list"))
                return false;

            // 遍历各个玩家简易数据
            List<LPCValue> charList = (List<LPCValue>)Communicate.CurrInfo["delete_list"];
            string charRid = string.Empty;

            // 遍历各个玩家简易数据
            foreach (LPCValue user in charList)
            {
                LPCMapping userMap = user.AsMapping;
                if (ridOrName != userMap.GetValue<string>("name") &&
                    ridOrName != userMap.GetValue<string>("rid"))
                    continue;

                // 获取rid
                charRid = userMap.GetValue<string>("rid");
                break;
            }

            // 没有找到需要恢复的角色
            if (string.IsNullOrEmpty(charRid))
                return false;

            // 通知服务器恢复角色
            Communicate.Send2GS("CMD_SALVAGE_CHAR", PackArgs("char_rid", charRid));

            // 恢复角色成功
            return true;
        }
    }
}