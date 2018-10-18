/// <summary>
/// ChatUserItemWnd.cs
/// Created by fengsc 2016/11/30
/// 玩家聊天信息基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;
using System;

public class ChatUserItemWnd : WindowBase<ChatUserItemWnd>
{
    #region 成员变量

    public GameObject mItem;

    // 玩家头像
    public UITexture mIcon;

    // 聊天类型图标
    public UISprite mChatTypeIcon;
    public UILabel mChatType;

    public UILabel mChatLabel;

    // 玩家名称
    public UILabel mUserName;

    public UISprite mChatBg;

    public UISprite mGender;

    // 图文混排插件
    public RichTextContent mRichTextContent;

    // 聊天信息
    LPCMapping mMessageData = new LPCMapping();

    #endregion

    // Use this for initialization
    void Start ()
    {
        UIEventListener.Get(mItem).onClick = OnClickUserItem;

        // 绘制窗口
        Redraw();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        string name = string.Empty;
        if (mMessageData.GetValue<string>("rid").Equals(ME.GetRid()))
            name = string.Format("[adffa7]{0}", mMessageData.GetValue<string>("name"));
        else
            name = string.Format("[d6ccbb]{0}", mMessageData.GetValue<string>("name"));

        // 玩家名称
        mUserName.text = name;

        mGender.spriteName = UserMgr.GetGenderIcon(mMessageData.GetValue<int>("gender"));

        // 设置玩家头像
        string path = string.Empty;

        LPCValue v = mMessageData.GetValue<LPCValue>("icon");

        if (v != null && v.IsString)
        {
            path = string.Format("Assets/Art/UI/Icon/monster/{0}.png", v.AsString);
            Texture tex = ResourceMgr.LoadTexture(path);

            mIcon.mainTexture = tex;
        }
        else
        {
            mIcon.mainTexture = null;
        }

        mIcon.gameObject.SetActive(true);

        LPCMapping data = GetChatType(mMessageData.GetValue<string>("type"));

        mChatType.text = data.GetValue<string>("desc");
        mChatTypeIcon.spriteName = data.GetValue<string>("type_icon");
        mChatBg.spriteName = data.GetValue<string>("chat_bg");

        // 更新控件属性
        NGUIText.dynamicFont = mChatLabel.trueTypeFont;

        LPCValue chatType = mMessageData.GetValue<LPCValue>("chat_type");

        if (chatType == null || ! chatType.IsString)
            chatType = mMessageData.GetValue<LPCValue>("type");

        // 解析字符串
        string str = ChatRoomMgr.AnalyzeMessage(mMessageData, chatType.AsString);

        LPCArray array = ChatRoomMgr.GetPropertyDataList;

        mRichTextContent.clearContent();

        // 绑定物件数据
        mRichTextContent.Bind(array);

        // 调用插件显示文本信息
        mRichTextContent.ParseValue(str);
    }

    LPCMapping GetChatType(string type)
    {
        LPCMapping typeData = new LPCMapping();
        switch(type)
        {
            case ChatConfig.WORLD_CHANNEL :
                typeData.Add("desc", LocalizationMgr.Get("ChatWnd_12"));
                typeData.Add("chat_bg", "chat_bg");
                typeData.Add("type_icon", "general_tag");
                break;
            case ChatConfig.GUILD_CHANNEL :
                typeData.Add("desc", LocalizationMgr.Get("ChatWnd_13"));
                typeData.Add("chat_bg", "chat_bg");
                typeData.Add("type_icon", "general_tag");
                break;
            case ChatConfig.WHISPER :
                typeData.Add("desc", LocalizationMgr.Get("ChatWnd_14"));
                typeData.Add("chat_bg", "private_letter_bg");
                typeData.Add("type_icon", "private_letter_tag");
                break;
        }
        return typeData;
    }

    /// <summary>
    /// 玩家格子点击事件
    /// </summary>
    void OnClickUserItem(GameObject go)
    {
        // 点击玩家自己的头像不做处理...
        if (mMessageData.GetValue<string>("rid").Equals(ME.GetRid()))
            return;

        // 先显示界面后填写数据
        GameObject wnd = WindowMgr.OpenWnd(FriendViewWnd.WndType);

        // 窗口创建失败
        if (wnd == null)
        {
            LogMgr.Trace("FriendViewWnd窗口创建失败");
            return;
        }
        //只有世界聊天才开启举报和屏蔽
        if (mMessageData.GetValue<string>("type") == ChatConfig.WORLD_CHANNEL)
        {
            wnd.GetComponent<FriendViewWnd>().SetChatMessageInfo(mMessageData);
            wnd.GetComponent<FriendViewWnd>().SetReportAndShieldBtnState(true);
        }

        // 通知服务器请求数据
        Operation.CmdDetailAppearance.Go(DomainAddress.GenerateDomainAddress("c@" + mMessageData.GetValue<string>("rid"), "u", 0));
    }

    #region 外部函数

    public void Bind(LPCMapping messageData)
    {
        if (messageData == null || messageData.Count < 1)
            return;

        mMessageData = messageData;
    }

    #endregion
}
