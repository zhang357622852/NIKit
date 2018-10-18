/// <summary>
/// ChatNoticeItemWnd.cs
/// Created by fengsc 2016/12/21
/// 聊天系统游戏通知基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ChatNoticeItemWnd : WindowBase<ChatNoticeItemWnd>
{
    public RichTextContent mRichTextContent;

    public UILabel mContent;

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
        NGUIText.dynamicFont = mContent.trueTypeFont;

        LPCValue chatType = mMessageData.GetValue<LPCValue>("chat_type");

        if (chatType == null || ! chatType.IsString)
            chatType = mMessageData.GetValue<LPCValue>("type");

        // 解析字符串
        string str = ChatRoomMgr.AnalyzeMessage(mMessageData, chatType.AsString);

        LPCArray array = ChatRoomMgr.GetPropertyDataList;

        // 绑定物件数据
        mRichTextContent.Bind(array);
        mRichTextContent.ParseValue(str);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping msgData)
    {
        if (msgData == null)
            return;

        mMessageData = msgData;
    }
}
