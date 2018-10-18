using LPC;

/// <summary>
/// 对话才对关闭了
/// </summary>
public class MsgMenuClosed : MsgHandler
{
    public string GetName() { return "msg_menu_closed"; }

    public void Go(LPCValue para)
    {
        LogMgr.Trace("对话菜单被关闭了。");
        ME.dbase.Delete("current_menu");

        // 关闭菜单
        MenuMgr.CloseMenu();
    }
}
