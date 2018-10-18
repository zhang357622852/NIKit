/// <summary>
/// ReportWnd.cs
/// Created by zhangwm 2018/07/27
/// 举报界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class ReportWnd : WindowBase<ReportWnd>
{
    #region 成员变量

    //举报对象
    public UILabel mUpTitleLab;

    //玩家头像
    public UITexture mUserIcon;

    //LV.35
    public UILabel mUserLvLab;

    //玩家名字
    public UILabel mUserNameLab;

    //举报后将会临时屏蔽该玩家聊天内容。
    public UILabel mUpTitleDesLab;

    //举报内容
    public UILabel mDownTitleLab;

    //说话内容
    public RichTextContent mRichText;

    //------以上为您所举报玩家的发言信息-------
    public UILabel mDownTitleDesLab;

    //举报按钮
    public GameObject mReportBtn;

    //算了按钮
    public GameObject mCancelBtn;

    //关闭界面按钮
    public GameObject mCloseBtn;

    public TweenScale mTweenScale;

    //被举报的玩家数据
    private Property mUserOb;

    //被举报玩家的聊天信息数据
    private LPCMapping mChatMessageDataMap;

    #endregion

    private void Start()
    {
        InitText();
        RegisterEvent();
    }

    private void OnDestroy()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    #region 内部函数
    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    private void InitText()
    {
        mUpTitleLab.text = LocalizationMgr.Get("ReportWnd_1");
        mUpTitleDesLab.text = LocalizationMgr.Get("ReportWnd_2");
        mDownTitleLab.text = LocalizationMgr.Get("ReportWnd_3");
        mDownTitleDesLab.text = LocalizationMgr.Get("ReportWnd_4");
        mReportBtn.GetComponentInChildren<UILabel>().text = LocalizationMgr.Get("FriendViewWnd_1");
        mCancelBtn.GetComponentInChildren<UILabel>().text = LocalizationMgr.Get("ReportWnd_5");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mReportBtn).onClick = OnClickReportBtn;
        UIEventListener.Get(mCancelBtn).onClick = OnClickCancelBtn;
        EventDelegate.Add(mTweenScale.onFinished, OnFinish);
    }

    void OnFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    private void Redraw()
    {
        // 设置玩家头像
        string path = string.Empty;

        LPCValue v = mChatMessageDataMap.GetValue<LPCValue>("icon");

        if (v != null && v.IsString)
        {
            path = string.Format("Assets/Art/UI/Icon/monster/{0}.png", v.AsString);
            Texture tex = ResourceMgr.LoadTexture(path);

            mUserIcon.mainTexture = tex;
        }
        else
        {
            mUserIcon.mainTexture = null;
        }

        // 获取玩家名称和等级
        mUserNameLab.text = mUserOb.Query<string>("name");
        mUserLvLab.text = string.Format(LocalizationMgr.Get("ReportWnd_7"), mUserOb.Query<int>("level"));

        //聊天信息
        LPCArray array = ChatRoomMgr.GetPropertyDataList;

        mRichText.clearContent();

        // 绑定物件数据
        mRichText.Bind(array);

        // 调用插件显示文本信息
        LPCValue chatType = mChatMessageDataMap.GetValue<LPCValue>("chat_type");

        if (chatType == null || !chatType.IsString)
            chatType = mChatMessageDataMap.GetValue<LPCValue>("type");

        // 解析字符串
        string str = ChatRoomMgr.AnalyzeMessage(mChatMessageDataMap, chatType.AsString);
        mRichText.ParseValue(str);
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    private void OnClickCloseBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 举报点击回调
    /// </summary>
    private void OnClickReportBtn(GameObject go)
    {
        if (mUserOb == null || mChatMessageDataMap == null)
            return;

        DialogMgr.ShowDailog(new CallBack(OnReportCallBack), LocalizationMgr.Get("ReportWnd_6"));
    }

    /// <summary>
    /// 对话框回调
    /// </summary>
    /// <param name="para"></param>
    /// <param name="param"></param>
    private void OnReportCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        if (ChatRoomMgr.ReportUserChat(mUserOb.Query<string>("rid"), mUserOb.Query<string>("name"), mChatMessageDataMap.GetValue<string>("chat_id")))
            WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 取消按钮点击回调
    /// </summary>
    private void OnClickCancelBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }
    #endregion

    #region 外部接口
    public void BindData(Property user, LPCMapping messageData)
    {
        mUserOb = user;

        mChatMessageDataMap = messageData;

        Redraw();
    }
    #endregion
}
