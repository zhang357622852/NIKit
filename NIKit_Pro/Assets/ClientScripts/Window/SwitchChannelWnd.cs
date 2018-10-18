/// <summary>
/// SwitchChannelWnd.cs
/// Created by fengsc 2016/12/02
/// 频道切换窗口
/// </summary>
using UnityEngine;
using System.Collections;

public class SwitchChannelWnd : WindowBase<SwitchChannelWnd>
{
    // 频道输入框
    public UIInput mInput;

    public UILabel mTitle;

    // 确认按钮
    public GameObject mConfirmBtn;
    public UILabel mConfirmBtnLb;

    public TweenScale mTweenScale;

    // Use this for initialization
    void Start ()
    {
        // 初始化显示的文本
        InitText();

        // 注册事件
        UIEventListener.Get(mConfirmBtn).onClick = OnClickConfirmBtn;

        // 监听字段变化
        ME.user.dbase.RegisterTriggerField("SwitchChannelWnd_chatroom", new string[]{"chatroom"}, new CallBack(OnFieldsChange));

        if (mTweenScale == null)
            return;

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    void OnDestroy()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 移除字段变化事件
        ME.user.dbase.RemoveTriggerField("SwitchChannelWnd_chatroom");
    }

    /// <summary>
    /// 字段变化回调
    /// </summary>
    void OnFieldsChange(object para, params object[] param)
    {
        // 关闭窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 初始化固定显示的文本
    /// </summary>
    void InitText()
    {
        mTitle.text = LocalizationMgr.Get("ChatWnd_8");
        mConfirmBtnLb.text = LocalizationMgr.Get("ChatWnd_9");
        mInput.defaultText = string.Format(LocalizationMgr.Get("ChatWnd_10"), GameSettingMgr.GetSettingInt("max_chat_channel"));
    }

    /// <summary>
    /// 确认按钮点击事件
    /// </summary>
    void OnClickConfirmBtn(GameObject go)
    {
        int channel = 0;

        if (string.IsNullOrEmpty(mInput.value))
        {
            // 关闭切换频道窗口
            WindowMgr.DestroyWindow(gameObject.name);
            return;
        }

        // 输入框内的文本不是数字
        if (!int.TryParse(mInput.value.Trim(), out channel))
        {
            // 弹出提示文字
            DialogMgr.Notify(LocalizationMgr.Get("ChatWnd_11"));
            mInput.value = string.Empty;
            return;
        }

        if (channel > GameSettingMgr.GetSettingInt("max_chat_channel") || channel <= 0)
        {
            // 输入的频道不正确
            DialogMgr.Notify(LocalizationMgr.Get("ChatWnd_11"));
            mInput.value = string.Empty;
            return;
        }

        if (ME.user.Query<int>("chatroom").Equals(channel))
        {
            WindowMgr.DestroyWindow(gameObject.name);
            return;
        }

        // 转换成功，通知服务器切换频道
        Operation.CmdSwitchChannel.Go(channel);
    }
}
