/// <summary>
/// ConvertCodeWnd.cs
/// 兑换码窗口
/// Created by fengsc 2017/09/25
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class ConvertCodeWnd : WindowBase<ConvertCodeWnd>
{
    // 标题
    public UILabel mTitle;

    // 描述
    public UILabel mDesc;

    // 输入框
    public UIInput mInput;

    // 关闭按钮
    public GameObject mCloseBtn;

    // 领取按钮
    public GameObject mReceiveBtn;
    public UILabel mReceiveBtnLb;

    // Use this for initialization
    void Start ()
    {
        // 初始化本地化文本
        InitLabel();

        // 注册事件
        RigisterEvent();

        // 绘制窗口
        Redraw();
    }

    void OnDestroy()
    {
        MsgMgr.RemoveDoneHook("MSG_CARD_BONUS_RESULT", "ConvertCodeWnd");
    }

    /// <summary>
    /// 初始化本地文本
    /// </summary>
    void InitLabel()
    {
        mTitle.text = LocalizationMgr.Get("ConvertCodeWnd_1");
        mDesc.text = LocalizationMgr.Get("ConvertCodeWnd_2");
        mReceiveBtnLb.text = LocalizationMgr.Get("ConvertCodeWnd_3");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RigisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mReceiveBtn).onClick = OnClickReceiveBtn;

        // 关注msg_card_bonus_result消息
        MsgMgr.RegisterDoneHook("MSG_CARD_BONUS_RESULT", "ConvertCodeWnd", OnCardBonusResultMsg);
    }

    /// <summary>
    /// msg_card_bonus_result消息回调
    /// </summary>
    void OnCardBonusResultMsg(string cmd, LPCValue para)
    {
        LPCMapping data = para.AsMapping;
        if (data == null)
            return;

        // 物品名称
        LPCMapping cardInfo = data.GetValue<LPCMapping>("card_info");
        if (cardInfo == null)
            return;

        // 获取激活码name
        LPCValue name = cardInfo.GetValue<LPCValue>("name");

        // 显示领取成功弹框
        DialogMgr.ShowSingleBtnDailog(new CallBack(OnDialogCallBack),
            LocalizationMgr.Get("ConvertCodeWnd_5"),
            string.Format(LocalizationMgr.Get("ConvertCodeWnd_4"),
                LocalizationMgr.GetServerDesc(name)));
    }

    /// <summary>
    /// 弹框按钮点击回调
    /// </summary>
    void OnDialogCallBack(object para, params object[] param)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(ConvertCodeWnd.WndType);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 输入框默认显示文本
        mInput.defaultText = LocalizationMgr.Get("ConvertCodeWnd_6");
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 领取按钮点击事件
    /// </summary>
    void OnClickReceiveBtn(GameObject go)
    {
        if (string.IsNullOrEmpty(mInput.value))
            return;

        // 通知服务器兑换激活码
        Operation.CmdInputBillingCard.Go(mInput.value.Trim());

        // 重置输入框
        mInput.value = string.Empty;
    }
}
