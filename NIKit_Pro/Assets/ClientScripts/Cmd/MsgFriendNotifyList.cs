using LPC;
using System.Collections.Generic;

/// <summary>
/// 好友列表
/// </summary>
public class MsgFriendNotifyList : MsgHandler
{
    public string GetName() { return "msg_friend_notify_list"; }

    public void Go(LPCValue para)
    {
        // 如果OwnerRid不是当前玩家，这需要重置好友请求数据
        if (!string.Equals(FriendMgr.OwnerRid, ME.GetRid()))
        {
            FriendMgr.FriendList = para.AsMapping.GetValue<LPCArray>("friend_detail");

            FriendMgr.OwnerRid = ME.GetRid();

            EventMgr.FireEvent(EventMgrEventType.EVENT_FRIEND_REQUEST, MixedValue.NewMixedValue<LPCMapping>(para.AsMapping));
            return;
        }

        // 新增好友请求
        string rid;
        LPCArray friendList = para.AsMapping["friend_detail"].AsArray;
        for (int i = 0; i < friendList.Count ; i++)
        {
            // 数据不正确不处理
            if (!friendList[i].IsMapping)
                continue;

            // 获取请求数据的rid
            rid = friendList[i].AsMapping.GetValue<string>("rid");

            // 请求数据没有rid
            if (string.IsNullOrEmpty(rid))
                continue;

            // 查看历史请求数据，如果已经在缓存数据中了，不在处理
            LPCMapping friendData = FriendMgr.FindFriend(rid);
            if (friendData != null)
            {
                friendData.Append(friendList[i].AsMapping);
                continue;
            }

            // 添加数据
            FriendMgr.FriendList.Add(friendList[i]);
        }

        // 缓存排序的好友列表数据
        FriendMgr.FriendList = FriendMgr.SortFriendList(FriendMgr.FriendList);

        // 抛出好友列表更新事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_FRIEND_NOTIFY_LIST, MixedValue.NewMixedValue<LPCMapping>(para.AsMapping));
    }
}
