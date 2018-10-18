/// <summary>
/// ChatPopWnd.cs
/// Created by fengsc 2016/12/16
/// 其他窗口聊天信息提示
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;
using System.Collections.Generic;

public class ChatPopWnd : WindowBase<ChatPopWnd>
{
    public GameObject mChatBtn;
    public GameObject mChatBg;

    // 图文混排插件
    public RichTextContent mRichTextContent;

    public GameObject mTextContent;

    public UILabel mContent;

    public TweenRotation mContentTweenRotation;

    public TweenRotation mTextTweenRotation;

    // 重要消息红点提示
    public GameObject mRedPoint;

    // 显示字符限制
    public int mCharacterLimit = 0;

    private Vector3 mFrom = new Vector3(90, 0, 0);

    private Vector3 mTo = new Vector3(0, 0, 0);

    private bool mContentWndShow = false;

    // 事件监听id
    private string mListenerId = string.Empty;

    // 缓存消息列表
    private LPCArray mCacheChatMessage = LPCArray.Empty;

    // 最大缓存消息数量
    private int MAX_CACHE_SIZE = 5;

    // 延迟隐藏时间
    private float mDelayHideTime = 4f;

    void Awake()
    {
        mListenerId = Game.GetUniqueName(ChatPopWnd.WndType);
    }

    void OnEnable()
    {
        // 监听msg_chat_message回调
        MsgMgr.RegisterDoneHook("MSG_CHAT_MESSAGE", mListenerId, OnMsgChatMessage);

        // 注册系统公告事件
        EventMgr.RegisterEvent(mListenerId, EventMgrEventType.EVENT_SYSTEM_AFFICHE, OnSystemAfficheEvent);

        UIEventListener.Get(mChatBtn).onClick = OnChatBtn;
        UIEventListener.Get(mChatBg).onClick = OnChatBtn;

        // 添加动画组件播放结束回调
        mContentTweenRotation.AddOnFinished(new EventDelegate.Callback(OnContentFinishTweenRotation));
        mTextTweenRotation.AddOnFinished(new EventDelegate.Callback(OnTextFinishTweenRotation));
    }

    /// <summary>
    /// Start this instance.
    /// </summary>
    void Start()
    {
        mChatBg.SetActive(false);
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    void OnDisable()
    {
        // 移除事件
        MsgMgr.RemoveDoneHook("MSG_CHAT_MESSAGE", mListenerId);

        EventMgr.UnregisterEvent(mListenerId);
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        // 取消倒计时效果
        CancelInvoke("WaitInvoke");
    }

    /// <summary>
    /// 打开聊天界面
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnChatBtn(GameObject go)
    {
        // 通关兰达平原普通所有副本
        if (! GuideMgr.IsGuided(4))
        {
            DialogMgr.Notify(LocalizationMgr.Get("GuideWnd_1"));

            return;
        }

        // 获得聊天窗口
        GameObject wnd = WindowMgr.GetWindow(ChatWnd.WndType);

        // 打开聊天窗口
        if (wnd == null)
            wnd = WindowMgr.OpenWnd(ChatWnd.WndType);
        else
            WindowMgr.ShowWindow(wnd);

        if (wnd == null)
            return;

        mRedPoint.SetActive(false);

        string wndName = this.transform.parent.name;

        // 绑定数据
        wnd.GetComponent<ChatWnd>().Bind(wndName);

        // 获取父级窗口
        GameObject parentWnd = WindowMgr.GetWindow(wndName);

        if (parentWnd == null)
            return;

        // 隐藏父级窗口
        WindowMgr.HideWindow(parentWnd);
    }

    /// <summary>
    /// 事件监听回调
    /// </summary>
    private void OnSystemAfficheEvent(int eventId, MixedValue para)
    {
        LPCMapping msgData = para.GetValue<LPCMapping>();

        if (msgData == null)
            return;

        // 添加到缓存列表中
        mCacheChatMessage.Add(msgData);

        // 尝试显示新消息
        TryShowNewMsg();
    }

    /// <summary>
    /// 监听msg_chat_message回调
    /// </summary>
    private void OnMsgChatMessage(string cmd, LPCValue para)
    {
        LPCMapping args = para.AsMapping;
        LPCArray messageList = args.GetValue<LPCArray>("message_list");

        //过滤屏蔽用户名单:只屏蔽世界频道
        LPCArray mesList;
        if (args.GetValue<string>("type").Equals(ChatConfig.WORLD_CHANNEL))
            mesList = ChatRoomMgr.FilterChaterByMesList(messageList);
        else
            mesList = messageList;

        // 没有新消息不处理
        if (mesList.Count <= 0)
            return;

        //私聊添加红点提示
        if (args.GetValue<string>("type").Equals(ChatConfig.WHISPER))
            mRedPoint.SetActive(true);

        // 添加到缓存列表中
        mCacheChatMessage.Append(mesList);

        // 尝试显示新消息
        TryShowNewMsg();
    }

    /// <summary>
    /// Refreshs the message.
    /// </summary>
    void TryShowNewMsg()
    {
        // 如果有消息正在表现中
        if (mTextTweenRotation.enabled)
            return;

        // 有新消息取消定时器
        CancelInvoke("WaitInvoke");

        // 没有消息需要显示了
        if (mCacheChatMessage.Count == 0)
        {
            // 4秒后没有新的消息调用该方法
            Invoke("WaitInvoke", mDelayHideTime);
            return;
        }

        // 如果背景窗口已经显示了不在重复显示
        if (! mContentWndShow)
        {
            // 重置控件
            mContentTweenRotation.ResetToBeginning();
            mContentTweenRotation.from = mFrom;
            mContentTweenRotation.to = mTo;

            // 启用动画控件
            mContentTweenRotation.PlayForward();

            // 显示背景
            mChatBg.SetActive(true);

            // 标识mContentWndShow已经显示
            mContentWndShow = true;
        }

        // 抽取一条需要显示的消息
        do
        {
            // 只显示最新的五条消息
            if (mCacheChatMessage.Count > MAX_CACHE_SIZE)
            {
                // 移除数据
                mCacheChatMessage.RemoveAt(0);
                continue;
            }

            // 从列表中拿去一条数据显示
            LPCMapping data = mCacheChatMessage[0].AsMapping;

            // 移除数据
            mCacheChatMessage.RemoveAt(0);

            // 显示消息
            RefreshMsgWnd(data);

            // 退出循环
            break;

        } while(true);
    }

    /// <summary>
    /// Shows the new message.
    /// </summary>
    /// <returns>The new message.</returns>
    /// <param name="msgData">Message data.</param>
    void RefreshMsgWnd(LPCMapping msgData)
    {
        // 获取聊天类型
        LPCValue chatType = msgData.GetValue<LPCValue>("chat_type");
        if (chatType == null || ! chatType.IsString)
            chatType = msgData.GetValue<LPCValue>("type");

        // 没有聊天类型
        if (chatType == null)
        {
            // 手动触发显示下一条消息
            TryShowNewMsg();
            return;
        }

        // 清空之前保留的数据
        mRichTextContent.clearContent();

        // 更新NGUIText中label控件的属性
        mContent.UpdateNGUIText();

        // 如果是公会消息
        if (chatType.AsString == ChatConfig.SYSTEM_MESSAGE_GANG)
        {
            // 构建参数
            LPCMapping data = LPCMapping.Empty;
            data.Add("gang_name", msgData.GetValue<string>("gang_name"));
            data.Add("relation_tag", msgData.GetValue<string>("relation_tag"));

            LPCArray newList = LPCArray.Empty;
            newList.Add(data);
            newList.Append(msgData.GetValue<LPCArray>("message"));

            LPCMapping messageData = LPCMapping.Empty;
            messageData.Add("message", newList);
            messageData.Add("chat_type", msgData.GetValue<string>("chat_type"));
            messageData.Add("type", msgData.GetValue<string>("type"));
            msgData = messageData;
            mContent.color = new Color(163 / 255f, 255 / 255f, 156 / 255f);
        }
        else if (chatType.AsString.Equals(ChatConfig.SYSTEM_CHAT))
        {
            mContent.color = new Color(242 / 255f, 160 / 255f, 160 / 255f);
        }
        else if (chatType.AsString.Equals(ChatConfig.SYSTEM_NOTIFY) || chatType.AsString.Equals(ChatConfig.GAME_NOTIFY))
        {
            mContent.color = new Color(98 / 255f, 255 / 255f, 255 / 255f);
        }
        else
        {
            mContent.color = new Color(1, 1, 1);
        }

        // 解析聊天消息
        string message = ChatRoomMgr.AnalyzeMessage(msgData, "tips", chatType.AsString, false, mCharacterLimit);

        LPCValue v = msgData.GetValue<LPCValue>("name");

        string content = string.Empty;

        if (v != null && v.IsString)
            content = string.Format("[{0}] {1}", v.AsString, message);
        else
            content = string.Format("{0}", message);

        LPCArray array = ChatRoomMgr.GetPropertyDataList;

        // 绑定物件数据
        mRichTextContent.Bind(array);

        // 显示聊天信息
        mRichTextContent.ParseValue(content);

        for (int i = 0; i < mTextContent.transform.childCount; i++)
            mTextContent.transform.GetChild(i).localRotation = Quaternion.Euler(Vector3.zero);

        // 播放文本控件的动画效果
        mTextTweenRotation.ResetToBeginning();
        mTextTweenRotation.from = mFrom;
        mTextTweenRotation.to = mTo;
        mTextTweenRotation.PlayForward();
    }

    /// <summary>
    /// 等待调用,隐藏聊天消息提示
    /// </summary>
    private void WaitInvoke()
    {
        // 重置控件
        mContentTweenRotation.ResetToBeginning();
        mContentTweenRotation.from = mTo;
        mContentTweenRotation.to = mFrom;

        // 启用动画控件
        mContentTweenRotation.PlayForward();

        // 标识ContentWnd隐藏
        mContentWndShow = false;
    }

    /// <summary>
    /// 动画播放完成回调
    /// </summary>
    void OnContentFinishTweenRotation()
    {
        // 聊天窗口弹出
        if (mContentTweenRotation.from.Equals(mFrom))
            return;

        // 隐藏背景窗口
        mChatBg.SetActive(false);
    }

    /// <summary>
    /// 文本动画播放完成回调
    /// </summary>
    void OnTextFinishTweenRotation()
    {
        // 尝试显示下一条新消息
        TryShowNewMsg();
    }
}
