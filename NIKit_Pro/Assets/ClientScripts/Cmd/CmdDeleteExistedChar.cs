using System;
using LPC;
using System.Collections.Generic;

public partial class Operation
{
    /// <summary>
    /// 删除角色
    /// </summary>
    public class CmdDeleteExistedChar : CmdHandler
    {
        public string GetName()
        {
            return "cmd_delete_existed_char";
        }

        public static bool Go(string ridOrName, string password)
        {
            // 没有玩家列表不处理
            if (!Communicate.CurrInfo.ContainsKey("char_list"))
                return false;

            // 遍历各个玩家简易数据
            List<LPCValue> charList = (List<LPCValue>)Communicate.CurrInfo["char_list"];
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

            // 没有找到需要删除的角色
            if (string.IsNullOrEmpty(charRid))
                return false;

            // 通知服务器删除角色
            Communicate.Send2GS("CMD_DELETE_EXISTED_CHAR", PackArgs("char_rid", charRid, "password", password));

            // 删除角色成功
            return true;
        }
    }
}