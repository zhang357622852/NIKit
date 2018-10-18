/// <summary>
/// ChatWnd.cs
/// Created by fengsc 2016/11/23
/// 聊天界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LPC;

public class ChatWnd : WindowBase<ChatWnd>
{
    #region 成员变量

    // 切换频道按钮
    public GameObject mSwitchChannelBtn;

    // 聊天频道
    public UILabel mChannel;

    // 最近私信按钮
    public GameObject mLatelyPrivateLetterBtn;

    // 私信玩家名称
    public UILabel mPrivateLetterName;

    // 清除私信玩家名称按钮
    public GameObject mClearNameBtn;

    // 聊天输入框
    public UIInput mChatInput;

    // 聊天输入框背景
    public UISprite mChatInputBg;

    // 聊天表情界面打开按钮
    public GameObject mExpressionBtn;

    // 聊天信息发送按钮
    public GameObject mSendBtn;
    public UILabel mSendBtnLb;

    // 综合聊天按钮
    public GameObject mGeneralBtn;
    public UILabel mGeneralBtnLb;

    // 公会聊天按钮
    public GameObject mGuildBtn;
    public UILabel mGuildBtnLb;

    // 私信聊天按钮
    public GameObject mPrivateLetterBtn;
    public UILabel mPrivateLetterBtnLb;

    // 关闭聊天窗口按钮
    public GameObject mCloseBtn;

    // 表情
    public UISprite mExpressionItem;

    // 排序组件
    public UIGrid mGrid;

    // 表情选择界面
    public GameObject mExpressionWnd;

    public GameObject mExpressCloseBtn;

    // 最近私信好友窗口
    public GameObject mLatelyPrivateLetterWnd;

    public UIScrollView mMessageScrollView;

    // 玩家聊天消息基础格子
    public GameObject mUserChatItem;

    // 系统消息基础格子
    public GameObject mSystemChatItem;

    // 游戏通知基础格子
    public GameObject mNoticeItem;

    // 表格排序
    public UITable mChatTable;
    public UITable mNoticeTable;

    public GameObject mChatMask;

    public GameObject mInputMask;

    public UIPanel mNoticePanel;

    public UIPanel mMessagePanel;

    public RichTextContent mNoticeRichTextCotent;

    public UIWidget mContainer;

    public UISprite mNoticeBg;

    public GameObject[] mCheckMark;

    // 聊天类型
    string mChatType = string.Empty;

    // 私聊玩家的rid
    string mToRid = string.Empty;

    // 私聊玩家的名称
    string mToName = string.Empty;

    int mCurPage = 0;

    Vector3 mNoticePos = Vector3.zero;

    Vector2 mNoticeViewSize = Vector2.zero;

    LPCArray mPulishList = new LPCArray();

    // 最近的私信玩家列表
    LPCArray mWhisperUserList = new LPCArray();

    // 物件对象
    Property mOb;

    string mWndName = string.Empty;

    bool isExpressDrawed = false;

    CallBack mCallBack = null;

    List<GameObject> mChatItems = new List<GameObject>();

    // 消息索引
    int mMessageIndex = 0;

    int mMaxAmount = 0;

    // 缓存消息队列
    Dictionary<string, LPCArray> mCacheChatMessage = new Dictionary<string, LPCArray>();

    // item最大数量
    int mMaxItemCount = 18;

    bool mIsFrist = true;

    #endregion

    void Awake()
    {
        // 注册事件
        RegisterEvent();

        // 初始化本地化文本
        InitLocalText();

        mUserChatItem.SetActive(false);
        mSystemChatItem.SetActive(false);
        mNoticeItem.SetActive(false);

        // 消息缓存的最大数量
        mMaxAmount = GameSettingMgr.GetSettingInt("max_cache_chat_message_amount");
    }

    void Start()
    {
        float scale = Game.CalcWndScale();
        transform.localScale = new Vector3(scale, scale, scale);
    }

    void OnEnable()
    {
        // 消息监听
        MsgMgr.RegisterDoneHook("MSG_PUBLISH_ENTITY", "ChatWnd", OnMsgPublishEntity);
        MsgMgr.RegisterDoneHook("MSG_CHAT_MESSAGE", "ChatWnd", OnChatMsg);

        // 监听chatroom字段变化
        ME.user.dbase.RegisterTriggerField("ChatWnd_chatroom", new string[]{"chatroom"}, new CallBack(OnFieldsChange));

        // 事件监听
        EventMgr.RegisterEvent("ChatWnd", EventMgrEventType.EVENT_SYSTEM_AFFICHE, OnAfficheEvent);

        // 系统公告道具点击事件监听
        EventMgr.RegisterEvent("ChatWnd", EventMgrEventType.EVENT_CHAT_ITEM_CLICK, OnClickEvent);
        EventMgr.RegisterEvent("ChatWnd", EventMgrEventType.EVENT_CHAT_MONSTER_CLICK, OnMonsterClickEvent);
        EventMgr.RegisterEvent("ChatWnd", EventMgrEventType.EVENT_GANG_BUTTON_CLICK, EventClickGangButton);
        EventMgr.RegisterEvent("ChatWnd", EventMgrEventType.EVENT_VIEW_COMBAT_VIDEO, OnViewCombatVideo);
        EventMgr.RegisterEvent("ChatWnd", EventMgrEventType.EVENT_GET_GANG_DETAILS, OnGetGangDetailsEvent);
        EventMgr.RegisterEvent("ChatWnd", EventMgrEventType.EVENT_PUBLISH_INVITE_ID, OnPublishInviteIdEvent);

        // 判断当前是否在战斗回放中
        // 如果在战斗回放中打开聊天窗口需要暂停回放
        if (ME.user.Query<int>("instance/playback") != 0)
            TimeMgr.DoPauseCombatLogic("ChatWndPause");

        // 绘制聊天界面
        Redraw();

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        MsgMgr.RemoveDoneHook("MSG_PUBLISH_ENTITY", "ChatWnd");
        MsgMgr.RemoveDoneHook("MSG_CHAT_MESSAGE", "ChatWnd");

        // 解注册事件
        EventMgr.UnregisterEvent("ChatWnd");

        // 设置当前lock的类型null
        ChatRoomMgr.LockChatType = string.Empty;

        // 恢复ChatWnd带来的战斗暂停
        TimeMgr.DoContinueCombatLogic("ChatWndPause");

        mMessageIndex = 0;
        mIsFrist = true;

        mCurPage = 0;

        if (mOb != null)
            mOb.Destroy();

        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 移除字段变化监听
        ME.user.dbase.RemoveTriggerField("ChatWnd_chatroom");
    }

    void InitWnd()
    {
        // 销毁子物体
        NGUITools.DestroyChildren(mChatTable.transform);

        // 清空缓存列表
        mChatItems.Clear();

        // 重置panel裁剪范围的偏移
        mMessagePanel.clipOffset = Vector2.zero;

        mMessagePanel.transform.localPosition = new Vector3(-8, -29, 0);

        mContainer.transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mExpressionBtn).onClick = OnClickShowExpression;
        UIEventListener.Get(mSwitchChannelBtn).onClick = OnClickChannelBtn;
        UIEventListener.Get(mLatelyPrivateLetterBtn).onClick = OnClickLatelyPrivateLetterBtn;
        UIEventListener.Get(mSendBtn).onClick = OnClickSendBtn;
        UIEventListener.Get(mGeneralBtn).onClick = OnClickGeneralBtn;
        UIEventListener.Get(mGuildBtn).onClick = OnClickGuildBtn;
        UIEventListener.Get(mPrivateLetterBtn).onClick = OnClickPrivateLetterBtn;
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mExpressCloseBtn).onClick = OnClickExpressionCloseBtn;
        UIEventListener.Get(mClearNameBtn).onClick = OnCickClearNameBtn;
        UIEventListener.Get(mChatMask).onClick = OnClickCloseBtn;

        if (mMessageScrollView != null)
            mMessageScrollView.onMomentumMove = OnMove;
    }

    /// <summary>
    /// 裁剪区域panel移动回调
    /// </summary>
    private void OnMove ()
    {
        // 聊天消息列表
        LPCArray messageList = ChatRoomMgr.GetChatMessage(mChatType);

        // 面板没有满不处理
        if (messageList.Count <= mMaxItemCount)
            return;

        Vector2 currentClipOffset = mMessagePanel.clipOffset;

        GameObject item = null;

        bool isRefresh = false;

        bool isTop = false;

        if (mMessageScrollView.verticalScrollBar.value >= 0.9)
        {
            if (mMessageIndex == 0)
                mMessageIndex += mMaxItemCount - 1;

            if (mMessageIndex + 1 >= messageList.Count)
                return;

            isTop = false;

            isRefresh = true;

            mMessageIndex += 1;

            // 销毁第一个聊天控件
            NGUITools.Destroy(mChatItems[0]);

            // 从缓存列表中移除
            mChatItems.RemoveAt(0);

            item = AddChatMessage(messageList[mMessageIndex].AsMapping, mChatTable.transform);

            // 添加新的基础格子（因为聊天消息中不同类型基础格子不一样）
            mChatItems.Add(item);
        }
        else if (mMessageScrollView.verticalScrollBar.value <= 0.1)
        {
            if (mMessageIndex == messageList.Count - 1)
                mMessageIndex -= mMaxItemCount;

            if (mMessageIndex <= 0)
                return;

            isRefresh = true;

            isTop = true;

            mMessageIndex -= 1;

            // 销毁末尾的聊天控件
            NGUITools.Destroy(mChatItems[mChatItems.Count - 1]);

            // 从缓存列表中移除
            mChatItems.RemoveAt(mChatItems.Count - 1);

            item = AddChatMessage(messageList[mMessageIndex].AsMapping, mChatTable.transform);

            // 添加新的基础格子（因为聊天消息中不同类型item不一样）
            mChatItems.Insert(0, item);
        }
        else
        {
            // 中间情况不处理
        }

        if (! isRefresh)
            return;

        // 排序子控件
        RepositionVariableSize(mChatItems);

        if (item == null)
            return;

        // 计算item的边界
        Bounds b = NGUIMath.CalculateRelativeWidgetBounds(item.transform, false);

        Vector3 scrollViewPos = mMessageScrollView.transform.localPosition;

        if (isTop)
        {
            mMessageScrollView.transform.localPosition = new Vector3(scrollViewPos.x, scrollViewPos.y + b.size.y, scrollViewPos.z);

            mMessageScrollView.panel.clipOffset = new Vector2(currentClipOffset.x, currentClipOffset.y - b.size.y);
        }
        else
        {
            mMessageScrollView.transform.localPosition = new Vector3(scrollViewPos.x, scrollViewPos.y - b.size.y, scrollViewPos.z);

            mMessageScrollView.panel.clipOffset = new Vector2(currentClipOffset.x, currentClipOffset.y + b.size.y);
        }
    }

    /// <summary>
    /// 查看战斗回放
    /// </summary>
    void OnViewCombatVideo(int eventId, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();
        if (data == null || !data.ContainsKey("video_id"))
            return;

        // 在副本中
        if (InstanceMgr.IsInInstance(ME.user))
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("ArenaWnd_13"));
            return;
        }

        // 打开对战信息窗口
        GameObject wnd = WindowMgr.OpenWnd(PlaybackInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<PlaybackInfoWnd>().Bind(data.GetValue<string>("video_id"));
    }

    /// <summary>
    /// 填写推荐邀请人id
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="para"></param>
    void OnPublishInviteIdEvent(int eventId, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();
        if (data == null || !data.ContainsKey("invite_id") || ME.user == null)
            return;

        string inviteId = data.GetValue<string>("invite_id");

        // 不可以添加自己为自己的邀请人
        if (ME.user.GetRid().Equals(inviteId))
        {
            DialogMgr.Notify(LocalizationMgr.Get("ChatWnd_25"));

            return;
        }

        //超过15级
        int limitLevel = GameSettingMgr.GetSetting<int>("limit_invite_level");
        if (ME.user.GetLevel() > limitLevel)
        {
            DialogMgr.Notify(string.Format(LocalizationMgr.Get("ChatWnd_23"), limitLevel));

            return;
        }

        //已经有邀请人
        if (!string.IsNullOrEmpty(ME.user.Query<string>("invite_id")))
        {
            DialogMgr.Notify(LocalizationMgr.Get("ChatWnd_24"));

            return;
        }

        //在副本中
        if (InstanceMgr.IsInInstance(ME.user))
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("ChatWnd_22"));
            return;
        }

        OnClickCloseBtn(null);

        // 打开好友信息窗口
        GameObject wnd = WindowMgr.OpenWnd(FriendWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<FriendWnd>().SetInviteId(inviteId);
        wnd.GetComponent<FriendWnd>().SwitchPage(FriendWnd.PageType.InviteFriend);
    }


    /// <summary>
    /// 获取公会详情事件回调
    /// </summary>
    void OnGetGangDetailsEvent(int eventId, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();
        if (data == null || data.Count == 0)
            return;

        GameObject wnd = WindowMgr.OpenWnd(GangDetailsWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        wnd.GetComponent<GangDetailsWnd>().Bind(data.GetValue<LPCMapping>("details"));
    }

    /// <summary>
    /// 公会按钮点击事件
    /// </summary>
    void EventClickGangButton(int eventId, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();

        // 获取公会详情
        GangMgr.GetGangDetails(data.GetValue<string>("gang_name"));
    }

    /// <summary>
    /// 使魔系统公告道具点击事件回调
    /// </summary>
    void OnMonsterClickEvent(int eventId, MixedValue para)
    {
        // 获取发布的物品的详细信息
        LPCMapping entity = para.GetValue<LPCMapping>();

        if (entity == null || entity.Count < 1)
            return;

        int classId = entity.GetValue<int>("class_id");

        LPCMapping data = LPCMapping.Empty;

        data.Add("class_id", classId);
        data.Add("rid", Rid.New());

        // 先析构掉原先的对象
        if (mOb != null)
            mOb.Destroy();

        // 创建一个物件对象
        mOb = PropertyMgr.CreateProperty(data);

        if (mOb == null)
            return;

        // 创建窗口显示发布物品的信息
        GameObject wnd = WindowMgr.OpenWnd(DetailedPetInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return;

        DetailedPetInfoWnd petScriptOb = wnd.GetComponent<DetailedPetInfoWnd>();

        if (petScriptOb == null)
            return;

        petScriptOb.Bind(mOb);
    }

    /// <summary>
    /// 系统公告道具点击事件回调
    /// </summary>
    void OnClickEvent(int eventId, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();

        LPCMapping map = LPCMapping.Empty;

        map.Add("class_id", data.GetValue<int>("class_id"));
        map.Add("amount", data.GetValue<int>("amount"));
        map.Add("rid", Rid.New());

        // 创建一个道具对象
        if (mOb != null)
            mOb.Destroy();

        mOb = PropertyMgr.CreateProperty(map);
        if (mOb == null)
            return;

        // 创建道具悬浮框
        GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();
        if (script == null)
            return;

        script.SetPropData(mOb, true, false, LocalizationMgr.Get("DungeonsDropGoodsWnd_4"));

        script.SetMask(true);

        script.SetCallBack(new CallBack(OnItemDialogCallBack, mOb));
    }

    /// <summary>
    /// 道具悬浮弹框回调
    /// </summary>
    void OnItemDialogCallBack(object para, params object[] _param)
    {
        if (!(bool)_param[0])
        {
            Property ob = para as Property;
            ob.Destroy();
        }
    }

    /// <summary>
    /// 发布物品消息监听回调函数
    /// </summary>
    void OnMsgPublishEntity(string cmd, LPCValue para)
    {
        // 获取发布的物品的详细信息
        LPCMapping entity = para.AsMapping.GetValue<LPCMapping>("entity");

        if (entity == null || entity.Count < 1)
            return;

        int classId = entity.GetValue<int>("class_id");

        LPCMapping data = LPCMapping.Empty;

        if (MonsterMgr.IsMonster(classId))
        {
            data.Add("class_id", classId);
            data.Add("rid", Rid.New());
        }
        else
        {
            data = LPCValue.Duplicate(entity).AsMapping;
            data.Add("rid", Rid.New());
        }

        // 创建一个道具对象
        if (mOb != null)
            mOb.Destroy();

        // 创建一个物件对象
        mOb = PropertyMgr.CreateProperty(data);

        if (mOb == null)
            return;

        // 创建窗口显示发布物品的信息
        GameObject wnd = null;
        if (MonsterMgr.IsMonster(mOb))
        {
            wnd = WindowMgr.OpenWnd(DetailedPetInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

            if (wnd == null)
                return;

            DetailedPetInfoWnd petScriptOb = wnd.GetComponent<DetailedPetInfoWnd>();

            if (petScriptOb == null)
                return;

            petScriptOb.Bind(mOb);
        }
        else if (EquipMgr.IsEquipment(mOb))
        {
            wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

            if (wnd == null)
                return;

            RewardItemInfoWnd itemScriptOb = wnd.GetComponent<RewardItemInfoWnd>();

            if (itemScriptOb == null)
                return;

            itemScriptOb.SetEquipData(mOb, true, false, LocalizationMgr.Get("ChatWnd_15"));
            itemScriptOb.SetMask(true);
        }
    }

    /// <summary>
    /// 系统公告事件回调
    /// </summary>
    void OnAfficheEvent(int eventId, MixedValue para)
    {
        // 绘制公告信息
        RedrawAffiche();
    }

    /// <summary>
    /// 字段变化监听回调
    /// </summary>
    void OnFieldsChange(object para, params object[] param)
    {
        mChannel.text = string.Format(LocalizationMgr.Get("ChatWnd_1"), ME.user.Query<int>("chatroom"));

        // 清空上一个频道缓存的聊天消息
        ChatRoomMgr.ClearChatMessage();

        // 重绘聊天消息
        RedrawChatMessage();
    }

    /// <summary>
    /// 聊天消息监听回调
    /// </summary>
    void OnChatMsg(string cmd, LPCValue para)
    {
        // 执行回调
        if (mCallBack != null)
            mCallBack.Go();

        LPCMapping arg = para.AsMapping;

        // 不是当前频道的消息并且不是综合频道，不处理
        if (!mChatType.Equals(arg["type"].AsString) && ! mChatType.Equals(ChatConfig.WORLD_CHANNEL))
            return;

        // 获取消息列表
        LPCArray messageList = arg.GetValue<LPCArray>("message_list");

        // 获取用户的屏蔽列表
        List<string> shieldList = ChatRoomMgr.GetShieldList;

        for (int i = 0; i < messageList.Count; i++)
        {
            LPCMapping msgData = messageList[i].AsMapping;

            if (msgData == null)
                continue;

            if (msgData.ContainsKey("rid") && msgData.GetValue<string>("rid").Equals(ME.GetRid()))
            {
                mChatInput.value = string.Empty;
                mPulishList = LPCArray.Empty;
            }

            // 该用户的信息被屏蔽
            if (shieldList.Contains(msgData.GetValue<string>("rid")))
                continue;

            mMessageIndex = Mathf.Min(mMessageIndex + 1, mMaxAmount - 1);

            // 消息全部显示完成
            if (mChatItems.Count < mMaxItemCount || mMessageScrollView.verticalScrollBar.value >= 0.99f)
            {
                if (mChatItems.Count >= mMaxItemCount)
                {
                    // 销毁第一个聊天控件
                    NGUITools.Destroy(mChatItems[0]);

                    // 从缓存列表中移除
                    mChatItems.RemoveAt(0);
                }

                // 添加新的基础格子（因为聊天消息中不同类型基础格子不一样）
                mChatItems.Add(AddChatMessage(messageList[i].AsMapping, mChatTable.transform));

                // 排序控件
                RepositionVariableSize(mChatItems);
            }
        }

        if (!string.IsNullOrEmpty(mToRid))
        {
            // 保存玩家最近的私信列表
            OptionMgr.SetOption(ME.user, "whisper_list", LPCValue.Create(mWhisperUserList));
        }

        mMessagePanel.ConstrainTargetToBounds(mChatTable.transform, true);

        // 开启协程
        Coroutine.DispatchService(UpdateControl(false));
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLocalText()
    {
        mPrivateLetterName.text = LocalizationMgr.Get("ChatWnd_2");
        mChatInput.defaultText = LocalizationMgr.Get("ChatWnd_3");
        mSendBtnLb.text = LocalizationMgr.Get("ChatWnd_4");
        mGeneralBtnLb.text = LocalizationMgr.Get("ChatWnd_5");
        mGuildBtnLb.text = LocalizationMgr.Get("ChatWnd_6");
        mPrivateLetterBtnLb.text = LocalizationMgr.Get("ChatWnd_7");
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 初始化控件
        InitWnd();

        // 绘制公告信息
        RedrawAffiche();

        Select();

        mClearNameBtn.SetActive(false);
        mInputMask.SetActive(false);

        // 显示玩家进入的默认聊天室频道
        mChannel.text = string.Format(LocalizationMgr.Get("ChatWnd_1"), ME.user.Query<int>("chatroom"));
    }

    /// <summary>
    /// 重绘聊天信息
    /// </summary>
    void RedrawChatMessage()
    {
        // 销毁子物体
        NGUITools.DestroyChildren(mChatTable.transform);

        // 清空缓存列表
        mChatItems.Clear();

        // 设置当前lock的类型
        ChatRoomMgr.LockChatType = mChatType;

        // 初始化
        mMessageIndex = 0;

        // 获取某个频道的消息列表
        LPCArray array = ChatRoomMgr.GetChatMessage(mChatType);

        if (array == null || array.Count < 1)
            return;

        // 缓存消息列表
        mCacheChatMessage[mChatType] = array;

        // 初始化控件
        InitWnd();

        int value = array.Count - mMaxItemCount;
        if (value > 0)
            mMessageIndex = value;
        else
            mMessageIndex = 0;

        for (int i = mMessageIndex; i < array.Count; i++)
        {
            LPCMapping messageData = array[i].AsMapping;

            mChatItems.Add(AddChatMessage(messageData, mChatTable.transform));

            mMessageIndex = Mathf.Min(mMessageIndex + 1, mMaxAmount - 1);
        }

        mChatTable.repositionNow = true;

        // 设置input放入输入框的字符长度限制
        if (mChatType == ChatConfig.SYSTEM_CHAT)
            mChatInput.characterLimit = GameSettingMgr.GetSettingInt("max_system_message_length");
        else
            mChatInput.characterLimit = GameSettingMgr.GetSettingInt("max_chat_message_length");

        Coroutine.DispatchService(UpdateControl(false));
    }

    /// <summary>
    /// 绘制公告信息
    /// </summary>
    void RedrawAffiche()
    {
        // 销毁控件
        NGUITools.DestroyChildren(mNoticeTable.transform);

        mNoticePanel.transform.localPosition = new Vector3(-8, 256, 0);

        mNoticePos = mNoticePanel.transform.localPosition;

        mNoticeViewSize = mNoticePanel.GetViewSize();

        // 获取游戏公告列表
        LPCMapping sysAffiche = ChatRoomMgr.GetLatelySystemAffiche();

        if (sysAffiche != null && sysAffiche.Count > 0)
        {
            // 显示mNoticePanel
            if (!mNoticePanel.gameObject.activeSelf)
            {
                mNoticePanel.gameObject.SetActive(true);

                mNoticeBg.gameObject.SetActive(true);
            }

            // 显示游戏公告
            AddChatMessage(sysAffiche, mNoticeTable.transform);

            mNoticeTable.repositionNow = true;
        }
        else
        {
            // 隐藏mNoticePanel
            mNoticePanel.gameObject.SetActive(false);

            mNoticeBg.gameObject.SetActive(false);
        }

        Coroutine.DispatchService(UpdateControl(true));
    }

    /// <summary>
    /// Gets the basic chat control.
    /// </summary>
    /// <returns>The basic chat control.</returns>
    /// <param name="message">Message.</param>
    private GameObject GetBasicChatControl(LPCMapping message)
    {
        // 获取消息类型
        string type = message.GetValue<string>("type");

        // 如果系统公告提示信息
        if (string.Equals(type, ChatConfig.GAME_NOTIFY))
            return mNoticeItem;

        string chatType = message.GetValue<string>("chat_type");
        if (string.Equals(chatType, ChatConfig.SYSTEM_CHAT) ||
            string.Equals(chatType, ChatConfig.SYSTEM_NOTIFY) ||
            string.Equals(chatType, ChatConfig.GUILD_CHANNEL))
            return mSystemChatItem;

        // mUserChatItem
        return mUserChatItem;
    }

    /// <summary>
    /// 增加聊天信息
    /// </summary>
    GameObject AddChatMessage(LPCMapping messageData, Transform parent)
    {
        // 根据消息选择不同的控件
        GameObject basicControl = GetBasicChatControl(messageData);

        // 将新的消息添加到列表的末尾
        GameObject item = Instantiate(basicControl);
        item.transform.SetParent(parent);
        item.transform.localScale = Vector3.one;
        item.transform.localPosition = Vector3.zero;

        LPCValue chatType = messageData.GetValue<LPCValue>("chat_type");

        if (chatType == null || ! chatType.IsString)
            chatType = messageData.GetValue<LPCValue>("type");

        item.name = chatType.AsString;

        ChatUserItemWnd chatScript = item.GetComponent<ChatUserItemWnd>();
        ChatSystemItemWnd systemScript = item.GetComponent<ChatSystemItemWnd>();
        ChatNoticeItemWnd noticeScript = item.GetComponent<ChatNoticeItemWnd>();

        item.SetActive(true);

        // 绑定数据
        if (chatScript != null)
            chatScript.Bind(messageData);
        else if (systemScript != null)
            systemScript.Bind(messageData);
        else
            noticeScript.Bind(messageData);

        return item;
    }

    /// <summary>
    /// 设置panel的偏移和裁剪范围的大小
    /// </summary>
    void SetPanelRect()
    {
        UIScrollView scrollView = mNoticePanel.transform.GetComponent<UIScrollView>();

        scrollView.ResetPosition();

        // 获取公告面板显示内容后的容器的边界
        Bounds bounds = scrollView.bounds;

        // 获取游戏通知面板控件的高度
        float height = bounds.size.y;

        float offset = 10f;

        // 设置panel的矩形
        mNoticePanel.SetRect(0, 0, mNoticeViewSize.x, height + offset);

        // 设置panel的transform的位置
        mNoticePanel.transform.localPosition = new Vector3(
            mNoticePos.x,
            mNoticePos.y - (height + offset) * 0.5f,
            mNoticePos.z);

        mNoticePanel.clipOffset = Vector2.zero;

        scrollView.ResetPosition();

        if (height < 1)
        {
            mNoticePanel.gameObject.SetActive(false);

            mNoticeBg.gameObject.SetActive(false);
        }
        else
        {
            mNoticeBg.height = (int) (height + 30);

            mNoticeBg.gameObject.SetActive(true);

            mNoticePanel.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 绘制所有的表情
    /// </summary>
    void RedrawExpression()
    {
        if (mGrid.transform.childCount > 0)
            return;

        if (mExpressionItem == null)
            return;

        mExpressionItem.gameObject.SetActive(false);

        foreach (List<CsvRow> ids in ChatRoomMgr.ExpressionDic.Values)
        {
            CsvRow row = ids[0];
            if (row == null)
                continue;

            // 克隆表情图标
            GameObject item = Instantiate(mExpressionItem).gameObject;

            if (item == null)
                continue;

            item.transform.SetParent(mGrid.transform);

            item.transform.localScale = Vector3.one;
            item.transform.localPosition = Vector3.zero;

            UISprite sprite = item.GetComponent<UISprite>();

            sprite.spriteName = row.Query<string>("icon");

            // 动态表情, 添加帧动画控件
            if (ids.Count > 1)
            {
                FrameAnimation frameAnimation = item.AddComponent<FrameAnimation>();

                frameAnimation.Group = row.Query<int>("group");
            }

            item.SetActive(true);

            // 注册表情点击事件
            UIEventListener.Get(item).onClick = OnClickExpression;

            item.GetComponent<UIEventListener>().parameter = row.Query<int>("group");
        }

        // 激活排序组件
        mGrid.repositionNow = true;

        isExpressDrawed = true;
    }

    /// <summary>
    /// 更新控件的位置
    /// </summary>
    IEnumerator UpdateControl(bool isSysAffiche, float barValue = 1.0f)
    {
        // 等待一帧
        yield return null;

        if (mMessageScrollView == null)
            yield break;

        // 设置聊天面板panel的位置
        SetPanelRect();

        if (isSysAffiche)
            yield break;

        if(mMessageScrollView.verticalScrollBar.value == 0f || mMessageScrollView.verticalScrollBar.value >= 0.98f || mIsFrist)
        {
            mMessageScrollView.UpdatePosition();

            mMessageScrollView.verticalScrollBar.value = barValue;
        }

        if (mMessageScrollView.verticalScrollBar.value >= 0.98f)
            mIsFrist = false;

        // 修正聊天面板的大小
        mContainer.height = (int) mMessagePanel.GetViewSize().y;
        mContainer.width = (int) mMessagePanel.GetViewSize().x;

        float offset_y = mMessagePanel.clipOffset.y;

        // 修正聊天面板容器的位置
        mContainer.transform.localPosition = new Vector3(
            mMessagePanel.transform.localPosition.x,
            offset_y,
            mMessagePanel.transform.localPosition.z);
    }

    /// <summary>
    /// 表情点击事件
    /// </summary>
    void OnClickExpression(GameObject go)
    {
        int group = (int) go.GetComponent<UIEventListener>().parameter;

        // 获取表情的配置表数据
        List<CsvRow> rowList = ChatRoomMgr.GetExpressionListByGroup(group);
        if (rowList == null)
            return;

        mChatInput.value += LocalizationMgr.Get(rowList[0].Query<string>("ch_string"));
    }

    /// <summary>
    /// 显示表情界面按钮点击事件
    /// </summary>
    void OnClickShowExpression(GameObject go)
    {
        // 绘制所有的表情
        if(!isExpressDrawed)
            RedrawExpression();

        // 打开表情显示界面
        mExpressionWnd.SetActive(true);
    }

    /// <summary>
    /// 频道选择按钮点击事件
    /// </summary>
    void OnClickChannelBtn(GameObject go)
    {
        // 打开窗口
        WindowMgr.OpenWnd(SwitchChannelWnd.WndType);
    }

    /// <summary>
    /// 清除当前私聊玩家按钮点击事件
    /// </summary>
    void OnCickClearNameBtn(GameObject go)
    {
        mPrivateLetterName.text = LocalizationMgr.Get("ChatWnd_2");
        mToRid = string.Empty;
        mToName = string.Empty;
        mInputMask.SetActive(true);
        mClearNameBtn.SetActive(false);

        if (mCurPage == 0)
            mChatType = ChatConfig.WORLD_CHANNEL;
    }

    /// <summary>
    /// 最近私信按钮点击事件
    /// </summary>
    void OnClickLatelyPrivateLetterBtn(GameObject go)
    {
        // 获取最近的私信列表
        LPCValue whisperList = OptionMgr.GetOption(ME.user, "whisper_list");
        if (whisperList == null)
            return;

        // 没有whisperList列表
        LPCArray list = whisperList.AsArray;
        if (list == null || list.Count == 0)
            return;

        if (mLatelyPrivateLetterWnd == null)
            return;

        mLatelyPrivateLetterWnd.SetActive(true);

        // 绑定数据
        mLatelyPrivateLetterWnd.GetComponent<PrivateLetterListWnd>().Bind(list, new CallBack(PrivateLetterCallBack));

        mLatelyPrivateLetterWnd.SetActive(true);
    }

    /// <summary>
    /// 私信列表界面回调
    /// </summary>
    void PrivateLetterCallBack(object para, params object[] param)
    {
        LPCArray array = (LPCArray) param[0];

        if (array == null)
            return;

        BindWhisperData(array);
    }

    /// <summary>
    /// 发送按钮点击事件
    /// </summary>
    void OnClickSendBtn(GameObject go)
    {
        // 去除一些无效的表情符号
        string input = Regex.Replace(mChatInput.value.Trim(), @"\p{Cs}", "");

        // 如果是私聊需要记录私聊的玩家数据
        if (ChatConfig.WHISPER.Equals(mChatType))
        {
            if (string.IsNullOrEmpty(mToRid))
                return;

            // 获取最近的私信列表
            LPCValue whisperList = OptionMgr.GetOption(ME.user, "whisper_list");
            if (whisperList == null)
                mWhisperUserList = LPCArray.Empty;
            else
                mWhisperUserList = whisperList.AsArray;

            int index = 0;
            bool isEquals = false;
            foreach (LPCValue item in mWhisperUserList.Values)
            {
                if (item.AsArray[0].AsString.Equals(mToRid))
                {
                    isEquals = true;

                    break;
                }

                index++;
            }

            if (isEquals && mWhisperUserList.Count >= index)
                mWhisperUserList.RemoveAt(index);

            LPCArray data = new LPCArray();
            data.Add(mToRid);
            data.Add(mToName);

            mWhisperUserList.Add(data);

            // 如果缓存列表中的数量达到最大值，清除最早加入列表的玩家
            if (mWhisperUserList.Count > GameSettingMgr.GetSettingInt("whisper_user_cache_limit"))
                mWhisperUserList.RemoveAt(0);
        }

        // 如果是在聊天栏中输入了DEBUG，则表示玩家需要上传combat bug信息
        if (string.Equals(input, "#DEBUG"))
        {
            // 构建一条消息
            LPCMapping messageMap = LPCMapping.Empty;
            messageMap.Add("icon", ME.user.Query("icon"));
            messageMap.Add("message", new LPCArray(input));
            messageMap.Add("type", mChatType);
            messageMap.Add("name", ME.user.GetName());
            messageMap.Add("rid", ME.GetRid());

            // 模拟服务器下发消息
            LPCMapping msgArgs = new LPCMapping();
            msgArgs.Add("type", mChatType);
            msgArgs.Add("message_list", new LPCArray(messageMap));

            // 模拟服务器下发MSG_CHAT_MESSAGE消息
            MsgMgr.Execute("MSG_CHAT_MESSAGE", LPCValue.Create(msgArgs));

            return;
        }

        // 通知服务器
        ChatRoomMgr.SendChatMessage(ME.user, mChatType, mToRid, mPulishList, input);
    }

    /// <summary>
    /// 综合按钮点击事件
    /// </summary>
    void OnClickGeneralBtn(GameObject go)
    {
        if (mCurPage == 0)
            return;

        // 综合频道
        mCurPage = 0;

        // 选择频道
        Select();
    }

    void DoClickGeneral()
    {
        mMessageIndex = 0;

        mIsFrist = true;

        mChatType = ChatConfig.WORLD_CHANNEL;

        mToRid = string.Empty;
        mToName = string.Empty;
        mInputMask.SetActive(false);
        mClearNameBtn.SetActive(false);

        mChatInputBg.spriteName = "chat_bg";
        mPrivateLetterName.text = LocalizationMgr.Get("ChatWnd_2");
        mChatInput.defaultText = LocalizationMgr.Get("ChatWnd_3");

        // 重绘聊天信息
        RedrawChatMessage();
    }

    /// <summary>
    /// 公会按钮点击事件
    /// </summary>
    void OnClickGuildBtn(GameObject go)
    {
        if (mCurPage == 1)
            return;

        mCurPage = 1;

        // 选择频道
        Select();
    }

    void DoClickGuild()
    {
        mMessageIndex = 0;

        mIsFrist = true;

        mChatType = ChatConfig.GUILD_CHANNEL;

        mToRid = string.Empty;
        mToName = string.Empty;
        mInputMask.SetActive(false);
        mClearNameBtn.SetActive(false);

        mChatInputBg.spriteName = "chat_bg";
        mPrivateLetterName.text = LocalizationMgr.Get("ChatWnd_2");
        mChatInput.defaultText = LocalizationMgr.Get("ChatWnd_3");

        // 重绘聊天信息
        RedrawChatMessage();

        mCurPage = 1;
    }

    /// <summary>
    /// 私信选项点击事件
    /// </summary>
    void OnClickPrivateLetterBtn(GameObject go)
    {
        if (mCurPage == 2)
            return;

        mCurPage = 2;

        // 选择频道
        Select();
    }

    void DoClickPrivateLetter()
    {
        mMessageIndex = 0;

        mIsFrist = true;

        mChatType = ChatConfig.WHISPER;

        mChatInputBg.spriteName = "private_letter_bg";
        mPrivateLetterName.text = LocalizationMgr.Get("ChatWnd_2");

        // 输入框默认显示的文本信息
        mChatInput.defaultText = LocalizationMgr.Get("ChatWnd_16");

        mInputMask.SetActive(true);

        // 重绘聊天信息
        RedrawChatMessage();

        if (string.IsNullOrEmpty(mToName))
            return;

        mInputMask.SetActive(false);

        // 获取最近的私信列表
        LPCValue whisperList = OptionMgr.GetOption(ME.user, "whisper_list");
        if (whisperList == null)
            return;

        LPCArray array = whisperList.AsArray;
        if (array == null)
            return;

        foreach (LPCValue value in array.Values)
        {
            if (value.AsArray[1].AsString.Equals(mToName))
            {
                mPrivateLetterName.text = value.AsArray[1].AsString;
                mToRid = value.AsArray[0].AsString;
                break;
            }
        }
    }

    /// <summary>
    /// 关闭聊天窗口按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 隐藏聊天窗口
        WindowMgr.HideWindow(gameObject);

        if (mExpressionWnd.activeSelf)
            mExpressionWnd.SetActive(false);

        if (string.IsNullOrEmpty(mWndName))
            return;

        // 获取窗口
        GameObject wnd = WindowMgr.GetWindow(mWndName);

        if (wnd == null)
            return;

        // 显示窗口
        WindowMgr.ShowWindow(wnd);
    }

    /// <summary>
    /// 关闭表情界面按钮点击事件
    /// </summary>
    void OnClickExpressionCloseBtn(GameObject go)
    {
        mExpressionWnd.SetActive(false);
    }

    /// <summary>
    /// Positions the grid items, taking their own size into consideration.
    /// </summary>
    void RepositionVariableSize (List<GameObject> children)
    {
        float xOffset = 0;
        float yOffset = 0;

        int columns = mChatTable.columns;

        int cols = columns > 0 ? children.Count / columns + 1 : 1;
        int rows = columns > 0 ? columns : children.Count;

        Bounds[,] bounds = new Bounds[cols, rows];
        Bounds[] boundsRows = new Bounds[rows];
        Bounds[] boundsCols = new Bounds[cols];

        int x = 0;
        int y = 0;

        for (int i = 0, imax = children.Count; i < imax; ++i)
        {
            if (children[i] == null)
                continue;

            Transform t = children[i].transform;
            Bounds b = NGUIMath.CalculateRelativeWidgetBounds(t, ! mChatTable.hideInactive);

            Vector3 scale = t.localScale;
            b.min = Vector3.Scale(b.min, scale);
            b.max = Vector3.Scale(b.max, scale);
            bounds[y, x] = b;

            boundsRows[x].Encapsulate(b);
            boundsCols[y].Encapsulate(b);

            if (++x >= columns && columns > 0)
            {
                x = 0;
                ++y;
            }
        }

        x = 0;
        y = 0;

        Vector2 po = NGUIMath.GetPivotOffset(mChatTable.cellAlignment);

        for (int i = 0, imax = children.Count; i < imax; ++i)
        {
            if (children[i] == null)
                continue;

            Transform t = children[i].transform;
            Bounds b = bounds[y, x];
            Bounds br = boundsRows[x];
            Bounds bc = boundsCols[y];

            Vector3 pos = t.localPosition;
            pos.x = xOffset + b.extents.x - b.center.x;
            pos.x -= Mathf.Lerp(0f, b.max.x - b.min.x - br.max.x + br.min.x, po.x) - mChatTable.padding.x;

            if (mChatTable.direction == UITable.Direction.Down)
            {
                pos.y = -yOffset - b.extents.y - b.center.y;
                pos.y += Mathf.Lerp(b.max.y - b.min.y - bc.max.y + bc.min.y, 0f, po.y) - mChatTable.padding.y;
            }
            else
            {
                pos.y = yOffset + b.extents.y - b.center.y;
                pos.y -= Mathf.Lerp(0f, b.max.y - b.min.y - bc.max.y + bc.min.y, po.y) - mChatTable.padding.y;
            }

            xOffset += br.size.x + mChatTable.padding.x * 2f;

            t.localPosition = pos;

            if (++x >= columns && columns > 0)
            {
                x = 0;
                ++y;

                xOffset = 0f;
                yOffset += bc.size.y + mChatTable.padding.y * 2f;
            }
        }

        // Apply the origin offset
        if (mChatTable.pivot != UIWidget.Pivot.TopLeft)
        {
            po = NGUIMath.GetPivotOffset(mChatTable.pivot);

            float fx, fy;

            Bounds b = NGUIMath.CalculateRelativeWidgetBounds(transform);

            fx = Mathf.Lerp(0f, b.size.x, po.x);
            fy = Mathf.Lerp(-b.size.y, 0f, po.y);

            Transform myTrans = mChatTable.transform;

            for (int i = 0; i < myTrans.childCount; ++i)
            {
                Transform t = myTrans.GetChild(i);
                SpringPosition sp = t.GetComponent<SpringPosition>();

                if (sp != null)
                {
                    sp.enabled = false;
                    sp.target.x -= fx;
                    sp.target.y -= fy;
                    sp.enabled = true;
                }
                else
                {
                    Vector3 pos = t.localPosition;
                    pos.x -= fx;
                    pos.y -= fy;
                    t.localPosition = pos;
                }
            }
        }
    }

    /// <summary>
    /// 选择频道
    /// </summary>
    void Select()
    {
        // 更新滑动条数据
        mMessageScrollView.UpdatePosition();

        // 重置面板滚动位置信息
        mMessageScrollView.ResetPosition();

        mChatTable.Reposition();

        for (int i = 0; i < mCheckMark.Length; i++)
        {
            if (mCurPage == i)
                mCheckMark[i].SetActive(true);
            else
                mCheckMark[i].SetActive(false);
        }

        if (mCurPage == 0)
        {
            // 综合频道
            DoClickGeneral();
        }
        else if (mCurPage == 1)
        {
            // 公会频道
            DoClickGuild();
        }
        else if (mCurPage == 2)
        {
            // 私信频道
            DoClickPrivateLetter();
        }
    }

    #region 外部接口

    /// <summary>
    /// 绑定发布数据
    /// </summary>
    public void BindPublish(Property ob, string input = "", string prefixDesc = "")
    {
        if (ob == null)
            return;

        if (string.IsNullOrEmpty(input))
            mChatInput.value += PublishMgr.GetPublicTag(ME.user, ob);
        else
            mChatInput.value += input;

        LPCMapping data = PublishMgr.GetPublishData(ME.user, ob, prefixDesc);

        if (data == null)
            return;

        mPulishList.Add(data);
    }

    /// <summary>
    /// 绑定私聊数据
    /// </summary>
    public void BindWhisperData(LPCArray data)
    {
        if (data == null)
            return;

        mToRid = data[0].AsString;
        mToName = data[1].AsString;

        if (string.IsNullOrEmpty(mToRid) || string.IsNullOrEmpty(mToName))
            return;

        mCurPage = 2;

        Select();

        mPrivateLetterName.text = mToName;

        // 更换聊天输入框背景
        mChatInputBg.spriteName = "private_letter_bg";

        // 设置聊天类型
        mChatType = ChatConfig.WHISPER;

        RedrawChatMessage();

        mClearNameBtn.SetActive(true);

        mInputMask.SetActive(false);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(string wndName, CallBack callBack = null)
    {
        mWndName = wndName;

        mCallBack = callBack;
    }

    #endregion
}
