using LPC;

/// <summary>
/// 菜单打开了
/// </summary>
public class MsgMenuOpened : MsgHandler
{
    public string GetName() { return "msg_menu_opened"; }

    public void Go(LPCValue para)
    {
        // 记录当前菜单
        ME.dbase.Set("current_menu", para);

        // 记录当前菜单列表
        LPCValue v = LPCValue.CreateArray();
        v.AsArray.Add(para);
        ME.dbase.Set("current_menu_list", v);

        // 解析菜单
        MenuMgr.Parse();
    }
}
