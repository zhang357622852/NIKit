/// <summary>
/// CmdGetGameCourse.cs
/// Created by fengsc 2018/08/16
/// 获取游戏历程数据
/// </summary>
using System;
using LPC;

public partial class Operation
{
    public class CmdGetGameCourse : CmdHandler
    {
        public string GetName()
        {
            return "cmd_get_game_course";
        }

        public static bool Go()
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 通知服务器获取游戏历程数据
            Communicate.Send2GS("CMD_GET_GAME_COURSE", PackArgs());

            return true;
        }
    }
}