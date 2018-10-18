/// <summary>
/// SetUserNameWnd.cs
/// Created by fucj 2014-11-10
/// 设置玩家名称
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;
using System.Text;

public partial class SetUserNameWnd : WindowBase<SetUserNameWnd>
{
    #region 成员变量

    public GameObject mStartGame;

    public GameObject NameInput;

    public GameObject mRandomBtn;

    public GameObject mCloseBtn;

    /// <summary>
    ///提示文本
    /// </summary>
    public UILabel mHintLabel;

    /// <summary>
    ///确认按钮文本
    /// </summary>
    public UILabel mConfirmBtnLabel;

    /// <summary>
    ///输入框默认文本
    /// </summary>
    public UILabel mInputLabel;

    // 性别选择框
    public UIToggle mMaleBtn;
    public UILabel mMaleBtnLb;

    public UIToggle mFemaleBtn;
    public UILabel mFemaleBtnLb;

    CallBack mCallBack;

    LPCMapping mData = LPCMapping.Empty;

    private bool mIsModifyName = false;

    int mGender = 0;

    string mName = string.Empty;

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        // 初始化窗口
        InitWnd();

        //初始话文本显示的内容;
        InitLabelContent();
    }

    void OnDestroy()
    {
        if (ME.user == null)
            return;

        // 移除字段关注
        ME.user.dbase.RemoveTriggerField("SetUserNameWnd");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        // 开始冒险按钮注册点击事件
        UIEventListener.Get(mStartGame).onClick += OnmStartGameClicked;

        // 随机名字按钮点击事件
        UIEventListener.Get(mRandomBtn).onClick += OnRandomName;

        UIEventListener.Get(mMaleBtn.gameObject).onClick = OnClickMaleBtn;

        UIEventListener.Get(mFemaleBtn.gameObject).onClick = OnClickFeMaleBtn;

        if (ME.user == null)
            return;

        // 关注字段变化
        ME.user.dbase.RegisterTriggerField("SetUserNameWnd", new string[] {"name"}, new CallBack(OnMsgModifyCharName));
    }

    /// <summary>
    /// 改名消息监听回调
    /// </summary>
    void OnMsgModifyCharName(object para, params object[] param)
    {
        if (mIsModifyName)
        {
            DialogMgr.Notify(LocalizationMgr.Get("LoginWnd_12"));
        }

        if (this.gameObject == null)
            return;

        WindowMgr.DestroyWindow(this.gameObject.name);
    }

    /// <summary>
    /// 随机名字按钮点击事件
    /// </summary>
    private void OnRandomName(GameObject ob)
    {
        // 赋值到输入框
        NameInput.GetComponent<UIInput>().value = RandomNameMgr.RandomName();
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        // 初始化名称框
        NameInput.GetComponent<UIInput>().value = string.Empty;

        //限制输入框的最大字符数;
        NameInput.GetComponent<UIInput>().characterLimit = GameSettingMgr.GetSettingInt("max_char_name_len");

        if (mMaleBtn.value)
            mGender = CharConst.MALE;
        else if (mFemaleBtn.value)
            mGender = CharConst.FEMALE;
    }

    /// <summary>
    /// 男性选项按钮点击事件
    /// </summary>
    void OnClickMaleBtn(GameObject go)
    {
        if (mMaleBtn.value)
            mGender = CharConst.MALE;
    }

    /// <summary>
    /// 女性选项按钮点击事件
    /// </summary>
    void OnClickFeMaleBtn(GameObject go)
    {
        if (mFemaleBtn.value)
            mGender = CharConst.FEMALE;
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        if (this != null)
            WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 开始冒险
    /// </summary>
    void OnmStartGameClicked(GameObject ob)
    {
        // 取得输入的名字
        mName = NameInput.GetComponent<UIInput>().value;
        mName = mName.Trim();
        if (string.IsNullOrEmpty(mName))
        {
            DialogMgr.Notify(LocalizationMgr.Get("LoginWnd_10"));
            return;
        }

        // 统计字符串长度
        if (Game.GetStrLength(mName) > GameSettingMgr.GetSettingInt("max_char_name_len"))
        {
            DialogMgr.Notify(LocalizationMgr.Get("LoginWnd_11"));
            return;
        }

        // 包含非法字符
        if (BanWordMgr.ContainsBanWords(mName))
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("LoginWnd_15"));

            return;
        }

        if (mIsModifyName)
        {
            if (ModifyName())
            {
                // 执行回调
                mCallBack.Go(true, mName, mGender);
                return;
            }
        }
        else
        {
            // 登陆新职业玩家
            LoginMgr.CreateNewUser(0, mName, mGender);
        }
    }

    /// <summary>
    /// 修改角色名称
    /// </summary>
    bool ModifyName()
    {
        if (mData == null || ! mData.ContainsKey("can_modify_char_name"))
            return false;

        int canModify = mData.GetValue<int>("can_modify_char_name");

        if (canModify != 1)
            return false;

        return true;
    }

    /// <summary>
    ///初始化文本显示的内容
    /// </summary>
    void InitLabelContent()
    {
        mHintLabel.text = LocalizationMgr.Get("LoginWnd_7");
        mConfirmBtnLabel.text = LocalizationMgr.Get("LoginWnd_8");
        mInputLabel.text = LocalizationMgr.Get("LoginWnd_9");
        mMaleBtnLb.text = LocalizationMgr.Get("LoginWnd_13");

        mFemaleBtnLb.text = LocalizationMgr.Get("LoginWnd_14");
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 设置回调
    /// </summary>
    public void SetCallBack(CallBack callBack)
    {
        if (callBack == null)
            return;

        mCallBack = callBack;
    }

    /// <summary>
    /// 开启关闭按钮
    /// </summary>
    public void OpenColseWndEvent()
    {
        if (mCloseBtn != null)
            UIEventListener.Get(mCloseBtn.gameObject).onClick = OnClickCloseBtn;
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data, bool isModifyName = false)
    {
        if (data == null)
            return;

        mIsModifyName = isModifyName;

        mData = data;

        if(isModifyName)
        {
            mMaleBtn.gameObject.SetActive(false);
            mFemaleBtn.gameObject.SetActive(false);
        }
    }

    #endregion
}
