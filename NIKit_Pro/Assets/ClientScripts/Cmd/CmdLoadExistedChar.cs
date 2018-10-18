using System;
using System.Collections.Generic;
using LPC;

public partial class Operation
{
    /// <summary>
    /// 载入角色指令
    /// </summary>
    public class CmdLoadExistedChar : CmdHandler
    {
        public string GetName()
        {
            return "cmd_load_existed_char";
        }

        public static bool Go(string rid, LPCValue extraInfo)
        {
            // 客户端操作系统
            if (!extraInfo.AsMapping["client_os"].IsUndefined &&
                extraInfo.AsMapping["client_os"].AsString.Length > 0)
            {
                extraInfo.AsMapping["client_os"] = LPCValue.Create(Encrypt.Des(extraInfo.AsMapping["client_os"].AsString));
            }

            Communicate.Send2GS("CMD_LOAD_EXISTED_CHAR", PackArgs("char_rid", rid,
                    "auth_key", Communicate.AccountInfo.Query("auth_key", 0),
                    "seed", Communicate.AccountInfo.Query("seed", 0),
                    "extra_info", extraInfo));
            return true;
        }
    }
}