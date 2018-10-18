/// <summary>
/// FriendMgr.cs
/// Created by zhaozy 2017/01/19
/// 好友管理
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using LPC;

/// <summary>
/// 好友管理
/// </summary>
public class FriendMgr
{
    #region 变量

    /// <summary>
    /// 好友列表
    /// </summary>
    private static LPCArray mFriendList = LPCArray.Empty;

    /// <summary>
    /// 好友请求列表（玩家请求和其他玩家的请求）
    /// </summary>
    private static LPCArray mRequestList = LPCArray.Empty;

    #endregion

    #region 属性

    /// <summary>
    /// 好友列表
    /// </summary>
    public static LPCArray FriendList
    {
        get
        {
            return mFriendList;
        }

        set
        {
            mFriendList = value;
        }
    }

    /// <summary>
    /// 好友数据owner
    /// </summary>
    public static string OwnerRid { get; set; }

    /// <summary>
    /// 好友请求列表
    /// </summary>
    public static LPCArray RequestList
    {
        get
        {
            return mRequestList;
        }

        set
        {
            mRequestList = value;
        }
    }

    #endregion

    #region  公共接口

    /// <summary>
    /// 重置好友数据
    /// </summary>
    public static void DoResetAll()
    {
        // 初始化邮件系统
        OwnerRid = string.Empty;
        mFriendList = LPCArray.Empty;
        mRequestList = LPCArray.Empty;;
    }

    /// <summary>
    /// 获取申请好友列表(玩家发送给其他人)
    /// </summary>
    public static LPCArray GetApplyList(Property user)
    {
        LPCArray list = LPCArray.Empty;
        for (int i = 0; i < mRequestList.Count; i++)
        {
            // 没有获取到申请数据
            if (mRequestList[i] == null || !mRequestList[i].IsMapping)
                continue;

            // "user" 的值等于当前玩家的rid表示是当前玩家发送给别人的好友申请,反之则是别人发送给当前玩家的请求
            if (! mRequestList[i].AsMapping.GetValue<string>("user").Equals(user.GetRid()))
                continue;

            list.Add(mRequestList[i]);
        }
        return list;
    }

    /// <summary>
    /// 获取好友请求列表(其他人发送给玩家)
    /// </summary>
    public static LPCArray GetRequestList(Property user)
    {
        LPCArray list = LPCArray.Empty;
        for (int i = 0; i < mRequestList.Count; i++)
        {
            // 没有获取到申请数据
            if (mRequestList[i] == null || !mRequestList[i].IsMapping)
                continue;

            // "opp" 的值等于当前玩家的rid表示是当前玩家发送给别人的好友申请,反之则是别人发送给当前玩家的请求
            if (! mRequestList[i].AsMapping.GetValue<string>("opp").Equals(user.GetRid()))
                continue;

            list.Add(mRequestList[i]);
        }
        return list;
    }

    /// <summary>
    /// 是否有好友请求信息
    /// </summary>
    public static bool HasFriendRequest(Property user)
    {
        if (user == null)
            return false;

        LPCArray requestList = GetRequestList(user);
        if (requestList == null || requestList.Count == 0)
            return false;

        return true;
    }

    /// <summary>
    /// 查找申请
    /// </summary>
    public static LPCMapping FindRequest(string user, string opp)
    {
        // 请求列表为空
        if (mRequestList.Count == 0)
            return null;

        // 查找数据
        for (int i = 0; i < mRequestList.Count; i++)
        {
            // opp不匹配
            LPCMapping request = mRequestList[i].AsMapping;
            if (! string.Equals(request.GetValue<string>("opp"), opp) ||
                ! string.Equals(request.GetValue<string>("user"), user))
                continue;

            // 返回数据
            return request;
        }

        // 没有查找的请求数据
        return null;
    }

    /// <summary>
    /// 判断是否已经发送过好友申请
    /// </summary>
    public static bool IsSendRequest(string rid)
    {
        // 请求列表为空
        if (mRequestList.Count == 0)
            return false;

        // 查找数据
        for (int i = 0; i < mRequestList.Count; i++)
        {
            // opp不匹配
            LPCMapping request = mRequestList[i].AsMapping;
            if (! string.Equals(request.GetValue<string>("opp"), rid))
                continue;

            // 返回数据
            return true;
        }

        // 没有查找的请求数据
        return false;
    }

    /// <summary>
    /// 判断是否已经收到过好友申请
    /// </summary>
    public static bool IsReceiveRequest(string rid)
    {
        // 请求列表为空
        if (mRequestList.Count == 0)
            return false;

        // 查找数据
        for (int i = 0; i < mRequestList.Count; i++)
        {
            // opp不匹配
            LPCMapping request = mRequestList[i].AsMapping;
            if (! string.Equals(request.GetValue<string>("user"), rid))
                continue;

            // 返回数据
            return true;
        }

        // 没有查找的请求数据
        return false;
    }

    /// <summary>
    /// 查找好友信息
    /// </summary>
    public static LPCMapping FindFriend(string rid)
    {
        // 请求列表为空
        if (mFriendList.Count == 0)
            return null;

        // 查找数据
        for (int i = 0; i < mFriendList.Count; i++)
        {
            // rid不匹配
            if (!string.Equals(mFriendList[i].AsMapping.GetValue<string>("rid"), rid))
                continue;

            // 返回数据
            return mFriendList[i].AsMapping;
        }

        // 没有查找的请求数据
        return null;
    }

    /// <summary>
    /// 添加邀请人
    /// </summary>
    public static void AddInvite(Property user, string rid)
    {
        // 玩家对象不存在，或者输入邀请人id为空
        if (user == null ||
            string.IsNullOrEmpty(rid))
            return;

        // 如果玩家等级操作15级，不允许填写邀请人
        // 您已经超过15级，无法绑定邀请人。
        int limitLevel = GameSettingMgr.GetSettingInt("limit_invite_level");
        if (user.GetLevel() > limitLevel)
        {
            DialogMgr.Notify(string.Format(LocalizationMgr.Get("FriendViewWnd_32"), limitLevel));
            return;
        }

        // 邀请人不能设置为自己，设置失败
        string inviteId = user.Query<string>("invite_id");
        if (string.Equals(rid, user.GetRid()))
        {
            DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_31"));
            return;
        }

        // 如果玩家已经添加过了invite_id
        if (! string.IsNullOrEmpty(inviteId))
        {
            DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_29"));
            return;
        }

        // 通知服务器添加数据
        Operation.CmdAddInvite.Go(rid);
    }

    /// <summary>
    /// 是否是好友的共享宠物
    /// </summary>
    public static bool IsFriendSharePet(string sharePetRid)
    {
        for (int i = 0; i < mFriendList.Count; i++)
        {
            LPCValue sharePet = mFriendList[i].AsMapping.GetValue<LPCValue>("share_pet");

            if (sharePet == null || !sharePet.IsMapping)
                continue;

            if (! sharePetRid.Equals(sharePet.AsMapping.GetValue<string>("rid")))
                continue;

            return true;
        }
        return false;
    }

    /// <summary>
    /// 排序好友列表
    /// </summary>
    public static LPCArray SortFriendList(LPCArray friendList)
    {
        List<LPCValue> tempList = new List<LPCValue>();

        List<LPCValue> offlineList = new List<LPCValue>();

        // 在线玩家放在列表前面
        for (int i = 0; i < friendList.Count; i++)
        {
            if (friendList[i].AsMapping.GetValue<int>("online") == 1)
                tempList.Add(friendList[i]);
            else
                offlineList.Add(friendList[i]);
        }

        // 将离线列表添加到在线列表末端
        tempList.AddRange(offlineList);

        LPCArray tempArray = LPCArray.Empty;

        for (int i = 0; i < tempList.Count; i++)
            tempArray.Add(tempList[i]);

        return tempArray;
    }

    /// <summary>
    /// 执行好友操作结果
    /// </summary>
    public static void DoMsgFriendOperateDone(string oper, int result, LPCMapping extraData)
    {
        // 根据不同的操作做不同的处理
        switch (oper)
        {
            // 删除好友操作
            case "remove":

                // 执行删除好友操作结果
                DoRemoveOperateResult(result, extraData);

                break;

            // 拒绝好友操作
            case "reject":

                // 执行拒绝好友操作结果
                DoRejectOperateResult(result, extraData);

                break;

            // 同意好友请求操作
            case "agree":

                // 执行同意好友请求操作结果
                DoAgreeOperateResult(result, extraData);

                break;

            // 申请好友请求
            case "request":

                // 执行申请好友请求操作结果
                DoRequestOperateResult(result, extraData);

                break;

            // 取消好友申请请求
            case "cancel_request":

                // 执行取消好友申请操作结果
                DoCancelApplyOperateResult(result, extraData);

                break;

            // 添加邀请
            case "invite":

                // 执行添加邀请操作结果
                DoInviteOperateResult(result, extraData);

                break;

            // 默认操作
            default:
                break;
        }

        LPCMapping para = LPCMapping.Empty;
        para.Add("result", result);
        para.Add("extra_data", extraData);
        para.Add("oper", oper);

        // 抛出好友操作结果事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_FRIEND_OPERATE_DONE, MixedValue.NewMixedValue<LPCMapping>(para));
    }

    #endregion

    #region 内部函数

    /// <summary>
    /// 执行删除好友操作结果
    /// </summary>
    private static void DoRemoveOperateResult(int result, LPCMapping extraData)
    {
        switch (result)
        {
            // 删除好友成功
            case FriendConst.ERESULT_OK:

                // 获取rid
                string rid = extraData.GetValue<string>("rid");
                int index = -1;

                // 查找数据
                for (int i = 0; i < FriendList.Count; i++)
                {
                    // rid不匹配
                    if (!string.Equals(FriendList[i].AsMapping.GetValue<string>("rid"), rid))
                        continue;

                    // 返回数据
                    index = i;
                    break;
                }

                // 没有找到数据
                if (index == -1)
                    break;

                // 删除数据
                FriendList.RemoveAt(index);

                break;

            // 删除好友失败
            case FriendConst.ERESULT_FAILED:
                DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_15"));
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 执行拒绝好友操作结果
    /// </summary>
    private static void DoRejectOperateResult(int result, LPCMapping extraData)
    {
        switch (result)
        {
            // 拒绝好友成功
            case FriendConst.ERESULT_OK:

                // 获取
                string rid = extraData.GetValue<string>("rid");
                List<LPCValue> removeList = new List<LPCValue>();
                LPCMapping request;

                // 清除全部（别人发送的请求）请求
                if (string.IsNullOrEmpty(rid))
                {
                    // 查找数据
                    for (int i = 0; i < RequestList.Count; i++)
                    {
                        // rid不匹配
                        request = RequestList[i].AsMapping;
                        if (! string.Equals(request.GetValue<string>("opp"), OwnerRid))
                            continue;

                        // 返回数据
                        removeList.Add(RequestList[i]);
                    }
                }
                else
                {
                    // 查找数据
                    for (int i = 0; i < RequestList.Count; i++)
                    {
                        // user和opp都没有关系
                        request = RequestList[i].AsMapping;
                        if (! string.Equals(request.GetValue<string>("user"), rid))
                            continue;

                        // 返回数据
                        removeList.Add(RequestList[i]);
                    }
                }

                // 删除需要删除的数据
                for (int i = 0; i < removeList.Count; i++)
                    RequestList.Remove(removeList[i]);

                break;

            // 拒绝好友失败
            case FriendConst.ERESULT_FAILED:
                DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_16"));
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 执行同意好友请求操作结果
    /// </summary>
    private static void DoAgreeOperateResult(int result, LPCMapping extraData)
    {
        switch (result)
        {
            // 同意好友请求成功
            case FriendConst.ERESULT_OK:
                // 删除好友清除列表

                // 获取rid
                string user = extraData.GetValue<string>("user");
                string opp = extraData.GetValue<string>("opp");
                int index = -1;

                // 查找数据
                for (int i = 0; i < RequestList.Count; i++)
                {
                    // rid不匹配
                    LPCMapping request = RequestList[i].AsMapping;
                    if (! string.Equals(request.GetValue<string>("user"), user) ||
                        ! string.Equals(request.GetValue<string>("opp"), opp))
                        continue;

                    // 返回数据
                    index = i;
                    break;
                }

                // 没有找到数据
                if (index == -1)
                    break;

                // 删除数据
                RequestList.RemoveAt(index);

                break;

            // 已经达到了最大好友数量
            case FriendConst.ERESULT_MAX_FRIEND:
                if (extraData.GetValue<string>("rid") == ME.user.GetRid())
                    DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_13"));
                else
                    DialogMgr.Notify(string.Format(LocalizationMgr.Get("FriendViewWnd_18"), extraData.GetValue<string>("name")));
                break;

            // 已经是好友关系，同意请求失败
            case FriendConst.ERESULT_BEEN_FRIEND:
                // 原则上是不会出现这种情况暂时不给任何提示
                break;

            // 同意请求失败
            case FriendConst.ERESULT_FAILED:
                DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_17"));
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 执行申请好友请求操作结果
    /// </summary>
    private static void DoRequestOperateResult(int result, LPCMapping extraData)
    {
        switch (result)
        {
            // 发送申请好友成功
            case FriendConst.ERESULT_OK:
                DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_10"));
                break;

            // 已经达到了最大好友数量，发送申请好友失败
            case FriendConst.ERESULT_MAX_FRIEND:
                if (extraData.GetValue<string>("rid") == ME.user.GetRid())
                    DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_13"));
                else
                    DialogMgr.Notify(string.Format(LocalizationMgr.Get("FriendViewWnd_18"), extraData.GetValue<string>("name")));
                break;

            // 已经达到了接收请求最大，发送申请好友失败
            case FriendConst.ERESULT_MAX_RECEIVE:
                if (extraData.GetValue<string>("rid") == ME.user.GetRid())
                    DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_19"));
                else
                    DialogMgr.Notify(string.Format(LocalizationMgr.Get("FriendViewWnd_20"), extraData.GetValue<string>("name")));
                break;

            // 已经达到了发出请求最大，发送申请好友失败
            case FriendConst.ERESULT_MAX_REQUEST:
                if (extraData.GetValue<string>("rid") == ME.user.GetRid())
                    DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_21"));
                else
                    DialogMgr.Notify(string.Format(LocalizationMgr.Get("FriendViewWnd_22"), extraData.GetValue<string>("name")));
                break;

            // 重复发送好友请求，发送申请好友失败
            case FriendConst.ERESULT_REQUEST_DUPLICATE:
                DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_23"));
                break;

            // 已经是好友关系，发送申请好友失败
            case FriendConst.ERESULT_BEEN_FRIEND:
                DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_24"));
                break;

            // 发送申请好友失败
            case FriendConst.ERESULT_FAILED:
                DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_25"));
                break;

            // 已经被请求过了
            case FriendConst.ERESULT_BEEN_REQUESTED:
                DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_14"));
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 执行取消好友申请操作结果
    /// </summary>
    private static void DoCancelApplyOperateResult(int result, LPCMapping extraData)
    {
        switch (result)
        {
            // 拒绝好友成功
            case FriendConst.ERESULT_OK:

                // 获取
                string user = extraData.GetValue<string>("user");
                string opp = extraData.GetValue<string>("opp");
                List<LPCValue> removeList = new List<LPCValue>();
                LPCMapping request;

                // 查找数据
                for (int i = 0; i < RequestList.Count; i++)
                {
                    // rid不匹配, 不是需要取消申请的玩家
                    request = RequestList[i].AsMapping;
                    if(! string.Equals(request.GetValue<string>("user"), user) ||
                       ! string.Equals(request.GetValue<string>("opp"), opp))
                        continue;

                    // 返回数据
                    removeList.Add(RequestList[i]);
                }

                // 删除需要删除的数据
                for (int i = 0; i < removeList.Count; i++)
                    RequestList.Remove(removeList[i]);

                break;

            // 拒绝好友失败
            case FriendConst.ERESULT_FAILED:
                DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_26"));
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 执行添加邀请操作结果
    /// </summary>
    private static void DoInviteOperateResult(int result, LPCMapping extraData)
    {
        switch (result)
        {
            // 执行添加邀请操作成功
            case FriendConst.ERESULT_OK:
                DialogMgr.ShowSimpleSingleBtnDailog(null, LocalizationMgr.Get("FriendViewWnd_28"));
                break;

            // 执行添加邀请操作失败
            case FriendConst.ERESULT_FAILED:
                if (extraData.ContainsKey("invite_id"))
                    DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_29"));
                else if (extraData.ContainsKey("limit_level"))
                {
                    DialogMgr.Notify(string.Format(LocalizationMgr.Get("FriendViewWnd_32"),
                        extraData.GetValue<int>("limit_level")));
                }
                else
                    DialogMgr.ShowSimpleSingleBtnDailog(null, LocalizationMgr.Get("FriendViewWnd_30"));
                break;

            default:
                break;
        }
    }

    #endregion
}
