using LPC;

/// <summary>
/// 有人在当前频道说话
/// </summary>
public class MsgGrantCmdList : MsgHandler
{
    public string GetName()
    {
        return "msg_grant_cmd_list";
    }

    public void Go(LPCValue para)
    {
        // 设置玩家的GM权限字段
        ME.user.SetTemp("privilege", para.AsMapping ["privilege"]);

        // 玩家权限操作operations列表
    }
}
