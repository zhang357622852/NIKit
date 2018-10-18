/// <summary>
/// ChatSystemItemWnd.cs
/// Created by fengsc 2016/12/19
/// 聊天系统消息基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ChatSystemItemWnd : WindowBase<ChatSystemItemWnd>
{
    // 聊天类型标签
    public UISprite mTypeTag;
    public UILabel mTypeLb;

    // 图文混排插件
    public RichTextContent mRichTextContent;

    public UILabel mContent;

    // 系统消息数据
    LPCMapping mMessageData = LPCMapping.Empty;

    void Start()
    {
        // 绘制窗口
        Redraw();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 聊天系统消息提示
        string type = mMessageData.GetValue<string>("chat_type");

        if (type.Equals(ChatConfig.SYSTEM_CHAT))
        {
            mTypeTag.spriteName = "system_tag";

            mTypeLb.text = LocalizationMgr.Get("ChatWnd_18");
            Color color = new Color(242 / 255f, 160 / 255f, 160 / 255f);

            mContent.color = color;
            mTypeLb.color = color;
        }
        else if(type.Equals(ChatConfig.SYSTEM_MESSAGE_GANG))
        {
            mTypeTag.spriteName = "SuitNormalBtn";
            Color color = new Color(163 / 255f, 255 / 255f, 156 / 255f);

            mTypeLb.text = LocalizationMgr.Get("ChatWnd_13");

            mContent.color = color;
            mTypeLb.color = color;
            mTypeTag.color = color;

            LPCArray message = mMessageData.GetValue<LPCArray>("message");

            // 构建参数
            LPCMapping data = LPCMapping.Empty;
            data.Add("gang_name", mMessageData.GetValue<string>("gang_name"));
            data.Add("relation_tag", mMessageData.GetValue<string>("relation_tag"));

            LPCArray newList = LPCArray.Empty;
            newList.Add(data);
            newList.Append(message);

            LPCMapping messageData = LPCMapping.Empty;
            messageData.Add("message", newList);
            messageData.Add("chat_type", mMessageData.GetValue<string>("chat_type"));
            messageData.Add("type", mMessageData.GetValue<string>("type"));

            mMessageData = messageData;
        }
        else
        {
            mTypeTag.spriteName = "notice_tag";
            Color color = new Color(98 / 255f, 255 / 255f, 255 / 255f);

            mTypeLb.text = LocalizationMgr.Get("ChatWnd_17");

            mContent.color = color;
            mTypeLb.color = color;
        }

        // 更新NGUIText中lable控件的属性
        NGUIText.dynamicFont = mContent.trueTypeFont;

        LPCValue chatType = mMessageData.GetValue<LPCValue>("chat_type");

        if (chatType == null || ! chatType.IsString)
            chatType = mMessageData.GetValue<LPCValue>("type");

        // 解析字符串
        string str = ChatRoomMgr.AnalyzeMessage(mMessageData, chatType.AsString);

        LPCArray array = ChatRoomMgr.GetPropertyDataList;

        mRichTextContent.clearContent();

        // 绑定物件数据
        mRichTextContent.Bind(array);
        mRichTextContent.ParseValue(str);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping messageData)
    {
        if (messageData == null)
            return;

        mMessageData = messageData;
    }
}
