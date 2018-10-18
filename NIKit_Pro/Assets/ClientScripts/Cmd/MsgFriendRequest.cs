using LPC;
using System;

/// <summary>
/// 请求信息
/// </summary>
public class MsgFriendRequest : MsgHandler
{
    public string GetName() { return "msg_friend_request"; }

    public void Go(LPCValue para)
    {
        // 如果OwnerRid不是当前玩家，这需要重置好友请求数据
        if (! string.Equals(FriendMgr.OwnerRid, ME.GetRid()))
        {
            FriendMgr.RequestList = para.AsMapping.GetValue<LPCArray>("request_list");

            FriendMgr.OwnerRid = ME.GetRid();

            EventMgr.FireEvent(EventMgrEventType.EVENT_FRIEND_REQUEST, MixedValue.NewMixedValue<LPCMapping>(para.AsMapping));
            return;
        }

        // 新增好友请求
        LPCArray requestList = para.AsMapping["request_list"].AsArray;
        LPCArray newRequestList = LPCArray.Empty;
        for (int i = 0; i < requestList.Count ; i++)
        {
            // 数据不正确不处理
            if (! requestList[i].IsMapping)
                continue;

            // 获取请求数据的rid
            LPCMapping request = requestList[i].AsMapping;
            string opp = request.GetValue<string>("opp");
            string user = request.GetValue<string>("user");

            // 判断是否已经在列表表中
            if (FriendMgr.FindRequest(user, opp) != null)
                continue;

            // 添加新请求列表中
            newRequestList.Add(requestList[i]);
        }

        // 添加数据
        FriendMgr.RequestList.Append(newRequestList);

        // 抛出好友请求事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_FRIEND_REQUEST, MixedValue.NewMixedValue<LPCMapping>(para.AsMapping));
    }
}