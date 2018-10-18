/// <summary>
/// EvaluateEntranceWnd.cs
/// Created by fengsc 2018/08/27
/// 游戏评价入口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class EvaluateEntranceWnd : WindowBase<EvaluateEntranceWnd>
{
    // 关闭按钮
    public GameObject mCloseBtn;

    public GameObject mMask;

    // 评价按钮
    public GameObject mEvaluateBtn;
    public UILabel mEvaluateLb;

    // 角色
    public UITexture mRole;

    // 对话窗口
    public TalkBoxWnd mTalkBoxWnd;

    private bool mIsClose = false;

    private CallBack mCallBack;

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();

        // 初始化本地化文本
        InitLable();

        // 绘制窗口
        Redraw();
    }

    void InitLable()
    {
        mEvaluateLb.text = LocalizationMgr.Get("EvaluateTipsWnd_0");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mMask).onClick = OnClickMask;
        UIEventListener.Get(mEvaluateBtn).onClick = OnClickEvaluateBtn;
    }

    void Redraw()
    {
        int active = 0;

        // 是否激活过评价系统
        LPCValue active_evaluate = OptionMgr.GetOption(ME.user, "active_evaluate");
        if (active_evaluate != null && active_evaluate.IsInt)
            active = active_evaluate.AsInt;

        if (active == 0)
        {
            mTalkBoxWnd.Bind(LocalizationMgr.Get("EvaluateTipsWnd_5"), LPCValue.Create(LocalizationMgr.Get("EvaluateTipsWnd_1")), true, 0, 0, null);
        }
        else
        {
            mTalkBoxWnd.Bind(LocalizationMgr.Get("EvaluateTipsWnd_5"), LPCValue.Create(LocalizationMgr.Get("EvaluateTipsWnd_4")), true, 0, 0, null);
        }

        mRole.mainTexture = ResourceMgr.LoadTexture("Assets/Art/UI/Window/Background/happy_evaluate_role.png");
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        if (mIsClose)
        {
            if (mCallBack != null)
                mCallBack.Go();

            // 关闭当前窗口
            WindowMgr.DestroyWindow(gameObject.name);
        }
        else
        {
            mRole.mainTexture = ResourceMgr.LoadTexture("Assets/Art/UI/Window/Background/unhappy_evaluate_role.png");

            mTalkBoxWnd.Bind(LocalizationMgr.Get("EvaluateTipsWnd_5"), LPCValue.Create(LocalizationMgr.Get("EvaluateTipsWnd_2")), true, 0, 0, null);

            mIsClose = true;
        }
    }

    /// <summary>
    /// 遮罩点击回调
    /// </summary>
    void OnClickMask(GameObject go)
    {
        if (mIsClose)
        {
            if (mCallBack != null)
                mCallBack.Go();

            // 关闭当前窗口
            WindowMgr.DestroyWindow(gameObject.name);
        }
    }

    /// <summary>
    /// 好评按钮点击回调
    /// </summary>
    void OnClickEvaluateBtn(GameObject go)
    {
        // 保存数据, 已激活评价
        OptionMgr.SetOption(ME.user, "active_evaluate", LPCValue.Create(1));

        // 更新本地评价标识
        string evaluateID = string.Empty;
        LPCValue evaluate_id = ME.user.QueryTemp<LPCValue>("evaluate_id");
        if (evaluate_id != null && evaluate_id.IsString)
            evaluateID = evaluate_id.AsString;

        if (string.IsNullOrEmpty(evaluateID))
            return;

        // 保存设置
        OptionMgr.SetOption(ME.user, "evaluate_id", LPCValue.Create(evaluateID));

        // 打开评价界面
        Application.OpenURL(ConfigMgr.Get<string>("evaluate_url"));

        if (mCallBack != null)
            mCallBack.Go();

        // 关闭当前界面
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(CallBack cb)
    {
        mCallBack = cb;
    }
}
