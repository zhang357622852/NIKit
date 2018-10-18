/// <summary>
/// FriendViewWnd.cs
/// Created by fengsc 2016 /12/05
/// 聊天系统好友查看界面
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class FriendViewWnd : WindowBase<FriendViewWnd>
{
    #region 成员变量

    // 玩家名称
    public UILabel mName;

    // 玩家头像
    public UITexture mUserIcon;

    // 举报按钮
    public GameObject mReportBtn;
    public UILabel mReportBtnLb;

    // 访问按钮
    public GameObject mAccessBtn;
    public UILabel mAccessBtnLb;

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    // 竞技场阶位
    public UILabel mRank;

    // 阶位背景
    public UISprite mRankBg;

    // 阶位图标
    public UISprite mRankIcon;

    // 阶位星级
    public UISprite[] mRankStars;

    // 无竞技场阶位
    public UILabel mUnRank;

    // 共享使魔
    public GameObject mSharePet;
    public UILabel mSharePetLb;

    // 使魔星级
    public UISprite[] mPetStars;

    // 使魔图标
    public UITexture mPetIcon;

    // 屏蔽按钮
    public GameObject mShieldBtn;
    public UILabel mShieldBtnLb;

    // 好友申请按钮
    public GameObject mApplicationBtn;
    public UILabel mApplicationBtnLb;
    public UISprite mApplicationBtnMask;

    // 私信按钮
    public GameObject mPrivateLetterBtn;
    public UILabel mPrivateLetterBtnLb;

    public GameObject mGameCourseBtn;
    public UILabel mGameCourseBtnLb;

    public TweenAlpha mTweenAlpha;

    public TweenScale mTweenScale;

    Property mUserOb;

    // 共享宠物对象
    Property mPetOb;
    //聊天消息map
    private LPCMapping mChatMessageMap = LPCMapping.Empty;

    #endregion

    #region 内部函数

    private void Awake()
    {
        // 设置举报按钮和屏蔽按钮状态
        SetReportAndShieldBtnState(false);
    }

    void Start()
    {
        // 初始化界面
        InitRedraw();

        // 绘制窗口
        Redraw();

        // 注册事件
        RegisterEvent();

        // 设置申请好友按钮的状态
        SetApplyButtonState();

        if (mTweenAlpha == null || mTweenScale == null)
            return;

        // 播放动画
        mTweenAlpha.PlayForward();

        mTweenScale.PlayForward();

        // 重置动画组件
        mTweenAlpha.ResetToBeginning();

        mTweenScale.ResetToBeginning();
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    void OnDestroy()
    {
        // 注销窗口注册事件
        EventMgr.UnregisterEvent("FriendViewWnd");

        // 析构临时宠物对象
        if (mPetOb != null)
            mPetOb.Destroy();

        // 析构玩家对象
        if (mUserOb != null)
            mUserOb.Destroy();
    }

    /// <summary>
    /// 初始化文本
    /// </summary>
    void InitText()
    {
        mReportBtnLb.text = LocalizationMgr.Get("FriendViewWnd_1");
        mAccessBtnLb.text = LocalizationMgr.Get("FriendViewWnd_2");
        mShieldBtnLb.text = LocalizationMgr.Get("FriendViewWnd_4");
        mApplicationBtnLb.text = LocalizationMgr.Get("FriendViewWnd_5");
        mPrivateLetterBtnLb.text = LocalizationMgr.Get("FriendViewWnd_6");
        mUnRank.text = LocalizationMgr.Get("FriendViewWnd_7");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mReportBtn).onClick = OnClickReportBtn;
        UIEventListener.Get(mAccessBtn).onClick = OnClickAccessBtn;
        UIEventListener.Get(mSharePet).onClick = OnClickSharePet;
        UIEventListener.Get(mShieldBtn).onClick = OnClickShieldBtn;
        UIEventListener.Get(mPrivateLetterBtn).onClick = OnCickPrivateLetterBtn;
        UIEventListener.Get(mGameCourseBtn).onClick = OnClickGameCourseBtn;

        // 监听玩家信息请求事件
        EventMgr.RegisterEvent("FriendViewWnd", EventMgrEventType.EVENT_SEARCH_DETAIL_INFO_SUCC, OnEventCallBack);

        // 注册窗口关闭的点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);
    }

    /// <summary>
    /// tween动画播放完后回调
    /// </summary>
    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 事件监听回调
    /// </summary>
    void OnEventCallBack(int eventId, MixedValue para)
    {
        LPCMapping args = para.GetValue<LPCMapping>();

        if (args == null || args.Count == 0)
            return;

        // 获取玩家的外观信息
        LPCMapping data = args ["data"].AsMapping;

        // 创建玩家对象
        if (mUserOb != null)
            mUserOb.Destroy();

        mUserOb = UserMgr.CreateUser(data.GetValue<string>("rid"), data);

        // 绘制窗口
        Redraw();

        SetApplyButtonState();
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void InitRedraw()
    {
        //初始化文本
        InitText();

        mUserIcon.mainTexture = null;

        mPetIcon.mainTexture = null;
        for (int i = 0; i < mPetStars.Length; i++)
            mPetStars[i].gameObject.SetActive(false);

        mRankIcon.spriteName = string.Empty;
        for (int i = 0; i < mRankStars.Length; i++)
            mRankStars[i].gameObject.SetActive(false);

        mRankBg.gameObject.SetActive(false);

        mRank.text = string.Empty;
        mSharePetLb.text = string.Empty;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        if (mUserOb == null)
            return;

        LPCValue v = mUserOb.Query<LPCValue>("share_pet");

        if (v != null && v.IsArray && v.AsArray.Count > 0)
        {
            // 获取共享宠物的外观信息
            LPCMapping shareData = v.AsArray[0].AsMapping;

            if (shareData == null || shareData.Count == 0)
                return;

            mUserOb.Set("share_pet", shareData.GetValue<LPCValue>("rid"));

            LPCMapping dbase = LPCValue.Duplicate(shareData).AsMapping;

            dbase.Add("rid", Rid.New());

            // 创建宠物对象
            mPetOb = PropertyMgr.CreateProperty(shareData);
        }

        // 初始化星级
        for (int i = 0; i < mPetStars.Length; i++)
            mPetStars[i].gameObject.SetActive(false);

        if (mPetOb != null)
        {
            // 获取宠物星级
            int petStar = mPetOb.Query<int>("star");

            // 获取星级的图标名称
            string spriteName = PetMgr.GetStarName(mPetOb.Query<int>("rank"));

            for (int i = 0; i < petStar; i++)
            {
                mPetStars[i].spriteName = spriteName;
                mPetStars[i].gameObject.SetActive(true);
            }

            string iconName = MonsterMgr.GetIcon(mPetOb.Query<int>("class_id"), mPetOb.Query<int>("rank"));
            string path = string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconName);
            mPetIcon.mainTexture = ResourceMgr.LoadTexture(path);

            mPetIcon.gameObject.SetActive(true);
        }

        // 加载玩家头像
        LPCValue iconValue = mUserOb.Query<LPCValue>("icon");
        if (iconValue != null && iconValue.IsString)
            mUserIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconValue.AsString));
        else
            mUserIcon.mainTexture = null;

        mUserIcon.gameObject.SetActive(true);

        // 获取玩家名称和等级
        mName.text = string.Format(LocalizationMgr.Get("FriendViewWnd_8"), mUserOb.Query<int>("level"), mUserOb.Query<string>("name"));

        if (mUserOb.Query<int>("gender") == CharConst.MALE)
            mGameCourseBtnLb.text = string.Format(LocalizationMgr.Get("FriendViewWnd_35"), LocalizationMgr.Get("FriendViewWnd_33"));
        else
            mGameCourseBtnLb.text = string.Format(LocalizationMgr.Get("FriendViewWnd_35"), LocalizationMgr.Get("FriendViewWnd_34"));

        for (int i = 0; i < mRankStars.Length; i++)
        {
            mRankStars[i].spriteName = "arena_star_bg";
            mRankStars[i].gameObject.SetActive(true);
        }

        mRankIcon.spriteName = "ordinary_icon";

        mRankBg.spriteName = "ordinary_bg";

        mSharePetLb.text = LocalizationMgr.Get("FriendViewWnd_3");

        // 显示玩家的竞技场信息
        LPCValue arenaTop = mUserOb.Query<LPCValue>("arena_top");
        if (arenaTop != null && arenaTop.IsMapping)
        {
            int rank = arenaTop.AsMapping.GetValue<int>("rank") + 1;
            int score = arenaTop.AsMapping.GetValue<int>("score");

            // 玩家当前的阶位
            int step = ArenaMgr.GetStepByScoreAndRank(rank - 1, score);

            // 获取配置表数据
            CsvRow row = ArenaMgr.TopBonusCsv.FindByKey(step);

            if (row == null)
                return;

            // 阶位名称
            mRank.text = LocalizationMgr.Get(row.Query<string>("name"));

            // 阶位背景
            mRankBg.spriteName = row.Query<string>("rank_bg");
            mRankBg.gameObject.SetActive(true);

            // 阶位图标
            mRankIcon.spriteName = row.Query<string>("rank_icon");

            int star = row.Query<int>("star");

            for (int i = 0; i < star; i++)
                mRankStars[i].spriteName = row.Query<string>("star_name");
        }
    }

    /// <summary>
    /// 设置申请按钮的状态
    /// </summary>
    void SetApplyButtonState()
    {
        if (mUserOb == null)
            return;

        // 查找好友
        LPCMapping data = FriendMgr.FindFriend(mUserOb.Query<string>("rid"));

        float rgb1 = 128 / 255f;
        float rgb2 = 255 / 255f;

        UISprite spriteBtn = mApplicationBtn.GetComponent<UISprite>();
        mApplicationBtnMask.gameObject.SetActive(data == null ? false : true);

        // 不是好友
        if (data == null)
        {
            spriteBtn.color = new Color(rgb2, rgb2, rgb2);
            UIEventListener.Get(mApplicationBtn).onClick = OnClickApplicationBtn;
            return;
        }

        spriteBtn.color = new Color(rgb1, rgb1, rgb1);
        UIEventListener.Get(mApplicationBtn).onClick -= OnClickApplicationBtn;
    }

    /// <summary>
    /// 举报按钮点击事件
    /// </summary>
    void OnClickReportBtn(GameObject go)
    {
        if (mUserOb == null || mChatMessageMap == null || mChatMessageMap.Count < 1)
            return;

        // 打开窗口
        GameObject wndGo = WindowMgr.OpenWnd(ReportWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wndGo == null)
            return;

        // 绑定数据
        wndGo.GetComponent<ReportWnd>().BindData(mUserOb, mChatMessageMap);
    }

    /// <summary>
    /// 访问按钮点击事件
    /// </summary>
    void OnClickAccessBtn(GameObject go)
    {
        if (mUserOb == null)
            return;

        // 创建玩家信息查看窗口
        GameObject wnd = WindowMgr.OpenWnd(ViewUserWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<ViewUserWnd>().Bind(mUserOb);
    }

    /// <summary>
    /// 共享使魔点击事件
    /// </summary>
    void OnClickSharePet(GameObject go)
    {
        if (mUserOb == null)
            return;

        if (mPetOb == null)
            return;

        GameObject wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
        {
            LogMgr.Trace("PetSimpleInfoWnd窗口创建失败");
            return;
        }

        PetSimpleInfoWnd petScript = wnd.GetComponent<PetSimpleInfoWnd>();

        if (petScript == null)
            return;

        // 绑定数据
        petScript.Bind(mPetOb);
        petScript.ShowBtn(true);
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
    /// 屏蔽按钮点击事件
    /// </summary>
    void OnClickShieldBtn(GameObject go)
    {
        if (mUserOb == null)
            return;

        // 点击屏蔽按钮显示提示弹框
        DialogMgr.ShowDailog(
            new CallBack(ShieldDialogCallBack),
            LocalizationMgr.Get("FriendViewWnd_9"),
            string.Empty,
            string.Empty,
            string.Empty,
            true,
            this.transform
        );
    }

    /// <summary>
    /// 屏蔽弹框回调函数
    /// </summary>
    void ShieldDialogCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        // 没有玩家数据
        if (mUserOb == null)
            return;

        // 屏蔽该玩家的聊天信息
        ChatRoomMgr.AddShieldUser(mUserOb.Query<string>("rid"));
    }

    /// <summary>
    /// 申请好友按钮点击事件
    /// </summary>
    void OnClickApplicationBtn(GameObject go)
    {
        // 没有玩家数据
        if (mUserOb == null)
            return;

        string userRid = mUserOb.Query<string>("rid");

        // 如果玩家自己提示FriendViewWnd_27
        if (string.Equals(userRid, ME.user.GetRid()))
        {
            DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_27"));
            return;
        }

        LPCArray array = FriendMgr.FriendList;

        // 好友数量达到最大数量
        if (array.Count >= GameSettingMgr.GetSettingInt("max_friend_amount"))
        {
            DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_13"));
            return;
        }

        // 已经发送过好友申请
        if(FriendMgr.IsSendRequest(userRid))
        {
            DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_11"));
            return;
        }

        // 已经发送过好友申请
        if(FriendMgr.IsReceiveRequest(userRid))
        {
            DialogMgr.Notify(LocalizationMgr.Get("FriendViewWnd_14"));
            return;
        }

        DialogMgr.ShowDailog(
            new CallBack(ApllyDialogCallBack),
            string.Format(LocalizationMgr.Get("FriendViewWnd_12"), mUserOb.Query<string>("name")),
            string.Empty,
            string.Empty,
            string.Empty,
            true,
            this.transform
        );
    }

    void ApllyDialogCallBack(object para, params object[] param)
    {
        if (! (bool)param[0])
        {
            // 销毁当前窗口
            WindowMgr.DestroyWindow(gameObject.name);

            return;
        }

        // 通知服务器发送好友请求
        Operation.CmdFriendSendRequest.Go(mUserOb.Query<string>("rid"));

        // 销毁当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 游戏历程按钮点击回调
    /// </summary>
    void OnClickGameCourseBtn(GameObject go)
    {
        if (mUserOb == null)
            return;

        // 打开游戏历程窗口
        GameObject wnd = WindowMgr.OpenWnd(GameCourseWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<GameCourseWnd>().Bind(mUserOb);
    }

    /// <summary>
    /// 私信按钮点击事件
    /// </summary>
    void OnCickPrivateLetterBtn(GameObject go)
    {
        if (mUserOb == null)
            return;

        GameObject wnd = WindowMgr.GetWindow(ChatWnd.WndType);

        // 界面获取失败
        if (wnd == null)
            wnd = WindowMgr.OpenWnd(ChatWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        else
            WindowMgr.ShowWindow(wnd);

        // 窗口打开失败
        if (wnd == null)
            return;

        LPCArray data = new LPCArray();
        data.Add(mUserOb.Query<string>("rid"));
        data.Add(mUserOb.Query<string>("name"));

        // 绑定数据
        wnd.GetComponent<ChatWnd>().BindWhisperData(data);

        // 关闭窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 记录聊天信息节点
    /// </summary>
    /// <param name="messageMap"></param>
    public void SetChatMessageInfo(LPCMapping messageMap)
    {
        mChatMessageMap = messageMap;
    }

    /// <summary>
    /// 设置举报按钮和屏蔽按钮状态
    /// </summary>
    /// <param name="isActive"></param>
    public void SetReportAndShieldBtnState(bool isActive)
    {
        mReportBtn.SetActive(isActive);
        mShieldBtn.SetActive(isActive);
    }
    #endregion
}
