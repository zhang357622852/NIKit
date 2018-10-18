/// <summary>
/// FriendRequestItemWnd.cs
/// Created by fengsc 2017/01/19
/// 好友请求基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class FriendRequestItemWnd : WindowBase<FriendRequestItemWnd>
{
    // 玩家头像
    public UITexture mIcon;

    // 玩家等级
    public UILabel mLevel;

    // 玩家名称
    public UILabel mName;

    //来自邀请ID
    public UILabel mInviteLab;

    // 拒绝按钮
    public GameObject mRefuseBtn;

    // 同意按钮
    public GameObject mAgreeBtn;

    LPCMapping mUser = LPCMapping.Empty;

    // Use this for initialization
    void Start ()
    {
        InitText();
        // 注册事件
        RegisterEvent();
    }

    /// <summary>
    /// 初始化文本
    /// </summary>
    private void InitText()
    {
        mInviteLab.text = LocalizationMgr.Get("FriendRequestWnd_5");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mRefuseBtn).onClick = OnClickRefuseBtn;
        UIEventListener.Get(mAgreeBtn).onClick = OnClickAgreeBtn;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 加载玩家头像
        LPCValue iconValue = mUser.GetValue<LPCValue>("icon");
        if (iconValue != null && iconValue.IsString)
            mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconValue.AsString));
        else
            mIcon.mainTexture = null;

        // 等级
        mLevel.text = string.Format(LocalizationMgr.Get("FriendRequestWnd_3"), mUser.GetValue<int>("level"));

        // 名称
        mName.text = mUser.GetValue<string>("name");

        //来自邀请ID
        mInviteLab.gameObject.SetActive(ME.user.GetRid().Equals(mUser.GetValue<string>("invite_id")));
    }

    /// <summary>
    /// 拒绝按钮点击事件
    /// </summary>
    void OnClickRefuseBtn(GameObject go)
    {
        // 通知服务器拒绝好友请求
        Operation.CmdFriendReject.Go(mUser.GetValue<string>("user"));
    }

    /// <summary>
    /// 同意按钮点击事件
    /// </summary>
    void OnClickAgreeBtn(GameObject go)
    {
        DialogMgr.ShowDailog(
            new CallBack(AgreeDialogCallBack),
            string.Format(LocalizationMgr.Get("FriendRequestWnd_4"), mUser.GetValue<string>("name")),
            string.Empty,
            string.Empty,
            string.Empty,
            true,
            WindowMgr.GetWindow(FriendWnd.WndType).transform
        );
    }

    /// <summary>
    /// 同意弹框按钮点击回调
    /// </summary>
    void AgreeDialogCallBack(object para, params object[] param)
    {
        if (!(bool) param[0])
            return;

        // 好友列表
        LPCArray array = FriendMgr.FriendList;
        if (array == null)
            array = LPCArray.Empty;

        // 好友数量达到最大数量
        if (array.Count >= GameSettingMgr.GetSettingInt("max_friend_amount"))
        {
            DialogMgr.Notify(string.Format(LocalizationMgr.Get("FriendViewWnd_13"), ME.user.GetName()));

            return;
        }

        // 通知服务器添加好友
        Operation.CmdFriendRequestAgree.Go(mUser.GetValue<string>("user"));
    }

    public void Bind(LPCMapping data)
    {
        if (data == null || data.Count < 1)
            return;

        mUser = data;

        Redraw();
    }
}
