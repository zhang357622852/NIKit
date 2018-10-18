/// <summary>
/// InviteFriendWnd.cs
/// Created by zhangwm 2018/06/25
/// 邀请好友
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class InviteFriendWnd : WindowBase<InviteFriendWnd>
{
    #region 成员变量
    //我的ID    我的ID : {0}
    public UILabel mMyIDLab;
    //邀请人ID  邀请人 : {0}
    public UILabel mInviteIDLab;
    //邀请人信息按钮
    public GameObject mInvitePlayerInfoBtn;

    public GameObject mInputPart;
    public UIInput mInput;
    public GameObject mSureBtn;

    // 发布推荐人按钮
    public GameObject mPublishBtn;
    public UILabel mPublishLab;

    //注:等级超过{0}级以后无法填入邀请人ID。
    public UILabel mTipLab;
    //输入邀请人成功后立即获得:
    public UILabel mRewardTitleLab;
    //未获得
    public UILabel mNoneGetLab;
    //已获得
    public GameObject mGetedGo;
    public GameObject mRewardItemPrefab;//scale->0.8
    public UIGrid mRewardGrid;
    //即可邀请按钮
    public GameObject mInviteFriendBtn;

    public GameObject mTaskItemPrefab;
    public UIScrollView mContentSV;
    public UIGrid mContentGrid;

    private int mTaskIndex = 0;
    private Property mPropOb = null;
    private List<GameObject> mTaskCacheList = new List<GameObject>();
    #endregion

    private void Start()
    {
        // 初始化文本
        InitText();

        // 注册事件
        RegisterEvent();

        // 绘制窗口
        Redraw();
    }

    private void OnDestroy()
    {
        if (mPropOb != null)
            mPropOb.Destroy();

        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField("InviteFriendWnd");
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    private void InitText()
    {
        mMyIDLab.text = string.Format(LocalizationMgr.Get("InviteFriendWnd_1"), ME.GetRid());
        if (ME.user != null)
        {
            LPCValue inviteId = ME.user.Query<LPCValue>("invite_id");
            mInviteIDLab.text = string.Format(LocalizationMgr.Get("InviteFriendWnd_2"), inviteId.AsString);
        }
        mSureBtn.GetComponentInChildren<UILabel>().text = LocalizationMgr.Get("RewardEquipInfoWnd_7");
        mRewardTitleLab.text = LocalizationMgr.Get("InviteFriendWnd_5");
        mInviteFriendBtn.GetComponentInChildren<UILabel>().text = LocalizationMgr.Get("InviteFriendWnd_6");
        mNoneGetLab.text = LocalizationMgr.Get("InviteFriendWnd_7");
        mInput.defaultText = LocalizationMgr.Get("InviteFriendWnd_3");
        mInvitePlayerInfoBtn.GetComponentInChildren<UILabel>().text = LocalizationMgr.Get("InviteFriendWnd_8");
        mGetedGo.GetComponentInChildren<UILabel>().text = LocalizationMgr.Get("InviteFriendWnd_9");
        mPublishLab.text = LocalizationMgr.Get("InviteFriendWnd_10");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mSureBtn).onClick = OnSureBtn;
        UIEventListener.Get(mInviteFriendBtn).onClick = OnInviteFriendBtn;
        UIEventListener.Get(mInvitePlayerInfoBtn).onClick = OnInvitePlayerInfoBtn;
        UIEventListener.Get(mPublishBtn).onClick = OnPublishBtn;

        if (ME.user == null)
            return;

        // 角色属性变更监听
        ME.user.dbase.RegisterTriggerField("InviteFriendWnd", new string[] { "invite_id" }, new CallBack(OnUserInviteChange));
        ME.user.dbase.RegisterTriggerField("InviteFriendWnd", new string[] { "task" }, new CallBack(OnTaskChange));
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    private void Redraw()
    {
        mInviteFriendBtn.SetActive(ShareMgr.IsOpenShare());

        if (ME.user == null)
            return;

        //邀请人ID
        RefreshInviteID();

        //受邀人奖励
        LPCArray attry = GameSettingMgr.GetSetting<LPCArray>("invite_bonus");
        foreach (var item in attry.Values)
        {
            GameObject go = NGUITools.AddChild(mRewardGrid.gameObject, mRewardItemPrefab);
            go.transform.localScale = new Vector3(1f, 1f, 1f);
            go.GetComponent<SignItemWnd>().Bind(item.AsMapping, string.Empty, false, false, -1, string.Empty);
            UIEventListener.Get(go).onClick = OnItemBtn;
        }
        mRewardItemPrefab.SetActive(false);
        mRewardGrid.Reposition();

        //限制等级
        LPCValue limitLev = GameSettingMgr.GetSetting<LPCValue>("limit_invite_level");
        mTipLab.text = string.Format(LocalizationMgr.Get("InviteFriendWnd_4"), limitLev.AsInt);

        //任务
        RefreshTask();
        mContentSV.ResetPosition();
    }

    private void RefreshTask()
    {
        RecycleItems();
        mTaskCacheList.Clear();
        List<int> inviteTaskList = TaskMgr.GetStandardTasksByType(TaskConst.INVITE_TASK, ME.user);
        for (int i = 0; i < inviteTaskList.Count; i++)
        {
            Transform tran = GetItem();
            tran.GetComponent<InviteFriendItemWnd>().BindData(inviteTaskList[i]);
            mTaskCacheList.Add(tran.gameObject);
        }
        mContentGrid.Reposition();
    }

    private Transform GetItem()
    {
        Transform tran = mContentGrid.transform.Find(mTaskIndex.ToString());
        if (tran == null)
        {
            GameObject go = NGUITools.AddChild(mContentGrid.gameObject, mTaskItemPrefab);
            tran = go.transform;
            tran.name = mTaskIndex.ToString();
            mTaskItemPrefab.SetActive(false);
        }
        mTaskIndex++;
        tran.gameObject.SetActive(true);

        return tran;
    }

    private void RecycleItems()
    {
        mTaskIndex = 0;
        for (int i = 0; i < mContentGrid.transform.childCount; i++)
        {
            Transform tran = mContentGrid.transform.GetChild(i);
            tran.gameObject.SetActive(false);
        }
    }

    private void RefreshInviteID()
    {
        string inviteId = ME.user.Query<string>("invite_id");
        mInviteIDLab.text = string.Format(LocalizationMgr.Get("InviteFriendWnd_2"), inviteId);
        mInputPart.SetActive(string.IsNullOrEmpty(inviteId) ? true : false);
        mInvitePlayerInfoBtn.SetActive(string.IsNullOrEmpty(inviteId) ? false : true);
        mNoneGetLab.gameObject.SetActive(string.IsNullOrEmpty(inviteId) ? true : false);
        mGetedGo.SetActive(string.IsNullOrEmpty(inviteId) ? false : true);
    }

    /// <summary>
    ///  任务状态变化
    /// </summary>
    void OnTaskChange(object para, params object[] _params)
    {
        for (int i = 0; i < mTaskCacheList.Count; i++)
        {
            mTaskCacheList[i].GetComponent<InviteFriendItemWnd>().Redraw();
        }
    }

    /// <summary>
    /// 物体被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnItemBtn(GameObject ob)
    {
        // 获取奖励数据
        LPCMapping itemData = ob.GetComponent<SignItemWnd>().mData;
        if (itemData == null)
            return;

        if (itemData.ContainsKey("class_id"))
        {
            int classId = itemData.GetValue<int>("class_id");

            // 构造参数
            LPCMapping dbase = LPCMapping.Empty;

            dbase.Append(itemData);
            dbase.Add("rid", Rid.New());

            // 克隆物件对象
            if (mPropOb != null)
                mPropOb.Destroy();

            mPropOb = PropertyMgr.CreateProperty(dbase);

            if (MonsterMgr.IsMonster(classId))
            {
                // 显示宠物悬浮窗口
                GameObject wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return;

                PetSimpleInfoWnd script = wnd.GetComponent<PetSimpleInfoWnd>();

                script.Bind(mPropOb);
                script.ShowBtn(true, false, false);
            }
            else if (EquipMgr.IsEquipment(classId))
            {
                GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return;

                RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

                script.SetEquipData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));

                script.SetMask(true);
            }
            else
            {
                GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return;

                RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

                script.SetPropData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));

                script.SetMask(true);
            }
        }
        else
        {
            string fields = FieldsMgr.GetFieldInMapping(itemData);

            int classId = FieldsMgr.GetClassIdByAttrib(fields);

            // 构造参数
            LPCMapping dbase = LPCMapping.Empty;
            dbase.Add("class_id", classId);
            dbase.Add("amount", itemData.GetValue<int>(fields));
            dbase.Add("rid", Rid.New());

            if (mPropOb != null)
                mPropOb.Destroy();

            mPropOb = PropertyMgr.CreateProperty(dbase);

            GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (wnd == null)
                return;

            RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

            script.SetPropData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));

            script.SetMask(true);
        }
    }

    /// <summary>
    /// 玩家邀请人ID数据变化
    /// </summary>
    private void OnUserInviteChange(object para, params object[] param)
    {
        RefreshInviteID();
    }

    /// <summary>
    /// 确定输入邀请者ID
    /// </summary>
    private void OnSureBtn(GameObject go)
    {
        if (ME.user == null) return;

        FriendMgr.AddInvite(ME.user, mInput.value);
    }

    /// <summary>
    /// 邀请朋友
    /// </summary>
    private void OnInviteFriendBtn(GameObject go)
    {
        WindowMgr.OpenWnd(ShareWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 发送邀请人ID
    /// </summary>
    private void OnPublishBtn(GameObject go)
    {
        if (ME.user == null)
            return;

        // 获取玩家的发言时间
        int talkTime = ME.user.Query<int>("publish_invite_id", true);

        int curTime = TimeMgr.GetTime();

        // 还处于CD时间限制中
        if (talkTime != 0 && (curTime - talkTime) < 0)
        {
            DialogMgr.Notify(LocalizationMgr.Get("InviteFriendWnd_12"));

            return;
        }

        // 获取发言时间间隔
        int talkInterval = GameSettingMgr.GetSetting<int>("send_invite_id_interval");

        // 记录下一次发言时间
        if (talkInterval > 0)
            ME.user.Set("publish_invite_id", LPCValue.Create(curTime + talkInterval));

        // 数据
        int limitLevel = GameSettingMgr.GetSetting<int>("limit_invite_level");

        string message = string.Format(LocalizationMgr.Get("InviteFriendWnd_11"), limitLevel);

        LPCArray publish = LPCArray.Empty;

        LPCMapping data = LPCMapping.Empty;
        data.Add("invite_id", ME.user.GetRid());

        publish.Add(data);

        // 向聊天频道发送消息
        ChatRoomMgr.SendChatMessage(ME.user, ChatConfig.WORLD_CHANNEL, string.Empty, publish, message);

        DialogMgr.Notify(string.Format(LocalizationMgr.Get("InviteFriendWnd_13"), ME.user.Query<int>("chatroom")));
    }

    /// <summary>
    /// 邀请人信息
    /// </summary>
    private void OnInvitePlayerInfoBtn(GameObject go)
    {
        if (ME.user == null) return;

        // 先显示界面后填写数据
        GameObject wnd = WindowMgr.OpenWnd(FriendViewWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (wnd == null)
        {
            LogMgr.Trace("FriendViewWnd窗口创建失败");
            return;
        }

        // 通知服务器请求数据
        Operation.CmdDetailAppearance.Go(DomainAddress.GenerateDomainAddress("c@" + ME.user.Query<string>("invite_id"), "u", 0));
    }

    /// <summary>
    /// 设置邀请id
    /// </summary>
    /// <param name="inviteId"></param>
    public void SetInviteId(string inviteId)
    {
        if (string.IsNullOrEmpty(inviteId))
            return;

        mInput.value = inviteId;
    }

}
