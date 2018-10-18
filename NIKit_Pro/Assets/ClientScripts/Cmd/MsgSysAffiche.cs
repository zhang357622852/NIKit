/// <summary>
/// MsgSysAffiche.cs
/// Created by fengsc 2016/12/22
/// 系统公告
/// </summary>
using LPC;
using System.Collections.Generic;
using System.Collections;

public class MsgSysAffiche : MsgHandler
{
    public string GetName()
    {
        return "msg_sys_affiche";
    }

    /// <summary>
    /// 入口
    /// </summary>
    public void Go(LPCValue para)
    {
        // 转换消息格式
        LPCMapping args = para.AsMapping;

        // 获取公告消息
        LPCArray afficheList = args.GetValue<LPCArray>("affiche_list");
        List<LPCMapping> messageList = new List<LPCMapping>();
        LPCMapping newAffiche = null;

        // 遍历消息
        for (int i = 0; i < afficheList.Count; i++)
        {
            LPCMapping message = afficheList[i].AsMapping;
            message.Add("type", ChatConfig.GAME_NOTIFY);

            // 添加到列表中
            messageList.Add(message);

            // 不是新公告
            if (! ChatRoomMgr.IsNewSysAffiche(message))
                continue;

            // newAffiche还没有初始化
            if (newAffiche == null)
            {
                newAffiche = message;
                continue;
            }

            // 如果发送时间更早
            if (message.GetValue<int>("send_time") <= newAffiche.GetValue<int>("send_time"))
                continue;

            // 记录数据
            newAffiche = message;
        }

        // 缓存消息
        ChatRoomMgr.SetSysAffiche(messageList);

        // 抛出系统公告事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_SYSTEM_AFFICHE, MixedValue.NewMixedValue<LPCMapping>(newAffiche));
    }
}
