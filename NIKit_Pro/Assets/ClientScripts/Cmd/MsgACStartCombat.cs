using LPC;
using System.Collections.Generic;

/// <summary>
/// 服务器通知战斗客户端开始战斗
/// </summary>
public class MsgACStartCombat : MsgHandler
{
    public string GetName()
    {
        return "msg_ac_start_combat";
    }

    /// <summary>
    /// 消息入口
    /// </summary>
    public void Go(LPCValue para)
    {
        // 转换数据格式
        LPCMapping args = para.AsMapping;

        // 开始验证战斗
        AuthClientMgr.DoAuthCombat(args);
    }
}
