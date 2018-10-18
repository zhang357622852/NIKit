using LPC;
using System.Collections.Generic;

/// <summary>
/// 角色列表数据
/// </summary>
public class MsgExistedCharList : MsgHandler
{
    public string GetName()
    {
        return "msg_existed_char_list";
    }

    public void Go(LPCValue para)
    {
        // 取得服务器数据
        LPCMapping data = para.AsMapping;

        // 重新记录角色对象前先释放原来的对象，清空角色列表
        Communicate.CurrInfo ["char_list"] = new List<LPCValue>();
        Communicate.CurrInfo ["delete_list"] = new List<LPCValue>();

        int i = 0;
        LogMgr.Trace("目前角色列表：");
        foreach (LPCValue m in data["user_info"].AsArray.Values)
        {
            // 角色数量+1
            i++;
            LogMgr.Trace("{0}. Name={1} Level={2}", i, m.AsMapping ["name"].AsString, m.AsMapping ["level"].AsInt);

            // 记录这个角色对象数据
            ((List<LPCValue>)Communicate.CurrInfo ["char_list"]).Add(m);
        }

        foreach (LPCValue n in data["delete_info"].AsArray.Values)
        {
            //记录这个角色对象数据
            ((List<LPCValue>)Communicate.CurrInfo ["delete_list"]).Add(n);
        }
    }
}