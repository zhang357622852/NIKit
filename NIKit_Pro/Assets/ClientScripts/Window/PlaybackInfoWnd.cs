/// <summary>
/// PlaybackInfoWnd.cs
/// Created by fengsc 2018/03/03
/// 战斗回放信息窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class PlaybackInfoWnd : WindowBase<PlaybackInfoWnd>
{
    #region 成员变量

    // 窗口标题
    public UILabel mTitle;

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    public UILabel mLeftLeaderTips;

    public UILabel mLeftLeaderDesc;

    // 对战时间
    public UILabel mCombatTime;

    public UILabel mLeftName;

    public UILabel mLeftScore;

    public UISprite[] mLeftStars;

    public UILabel mRightLeaderTips;

    public UILabel mRightLeaderDesc;

    public UILabel mRightName;

    public UILabel mRightScore;

    public UISprite[] mRightStar;

    // 对战阵容
    public GameObject[] mLeftList;

    public GameObject[] mRightList;

    public UILabel mLeftResultTips;

    public UILabel mRightResultTips;

    public GameObject mLeftBtn;
    public UILabel mLeftBtnLb;

    public GameObject mRightBtn;
    public UILabel mRightBtnLb;

    public UILabel mTips;

    public TweenScale mTweenScale;

    public UILabel mVideoIdTipWnd;
    public UILabel mVideoIdWnd;
    public UILabel mVideoIdCopyWnd;

    // 回放视频id
    private string mVideoId;

    // 视频详细信息
    private LPCMapping mVideoDatils;

    private List<Property> mObList = new List<Property>();

    private List<Property> equipData = new List<Property>();

    private int mAttackLevel;

    private int mDefenceLevel;

    private bool mIsOperate = false;

    private CallBack mCallBack;

    private bool mIsClearance = false;

    private bool mIsPause = false;

    // 回放的有效时间
    int mVideoValidTime = 0;

    #endregion

    void Start()
    {
        // 初始化本地化文本
        InitText();

        // 注册事件
        RegisterEvent();

        mVideoValidTime = GameSettingMgr.GetSettingInt("video_valid_time");
    }

    void OnDestroy()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        // 解注册事件
        EventMgr.UnregisterEvent(PlaybackInfoWnd.WndType);

        // 析构装备对象
        for (int i = 0; i < equipData.Count; i++)
        {
            if (equipData[i] != null)
                equipData[i].Destroy();
        }

        // 析构宠物对象
        for (int i = 0; i < mObList.Count; i++)
        {
            if (mObList[i] != null)
                mObList[i].Destroy();
        }
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mTitle.text = LocalizationMgr.Get("PlaybackInfoWnd_1");
        mTips.text = LocalizationMgr.Get("PlaybackInfoWnd_3");
        mLeftResultTips.text = LocalizationMgr.Get("PlaybackInfoWnd_4");
        mRightResultTips.text = LocalizationMgr.Get("PlaybackInfoWnd_4");
        mLeftLeaderTips.text = LocalizationMgr.Get("PlaybackInfoWnd_9");
        mRightLeaderTips.text = LocalizationMgr.Get("PlaybackInfoWnd_9");
    }

    /// <summary>
    /// 视频详细信息获取事件回调
    /// </summary>
    void OnVideoDetailsEvent(int eventId, MixedValue para)
    {
        // 获取视频详细数据
        mVideoDatils = VideoMgr.VideoDetails;

        // 绘制窗口
        Redraw();
    }

    void OnTweenScaleFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        if (mIsClearance)
        {
            // 打开主场景竞技场窗口
            if (string.Equals(SceneMgr.MainScene, SceneConst.SCENE_WORLD_MAP))
                SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(OpenArenaWnd));
            else
                SceneMgr.LoadScene("Main", SceneConst.SCENE_MAIN_CITY, new CallBack(OpenArenaWnd));
        }
        else
        {
            // 执行回调
            if (mCallBack != null)
                mCallBack.Go();

            // 关闭当前窗口
            WindowMgr.DestroyWindow(gameObject.name);
        }
    }

    /// <summary>
    /// 战斗回放按钮点击回调
    /// </summary>
    void OnClickPlaybackBtn(GameObject go)
    {
        if (TimeMgr.GetServerTime() - mVideoDatils.GetValue<int>("time") >= mVideoValidTime)
        {
            DialogMgr.Notify(LocalizationMgr.Get("ArenaWnd_11"));

            return;
        }

        if (mIsOperate)
            return;

        mIsOperate = true;

        // 战斗回放
        VideoMgr.PlayVideo(mVideoId);
    }

    /// <summary>
    /// 分享回放按钮点击事件
    /// </summary>
    void OnClickSharePlaybackBtn(GameObject go)
    {
        if (TimeMgr.GetServerTime() - mVideoDatils.GetValue<int>("time") >= mVideoValidTime)
        {
            DialogMgr.Notify(LocalizationMgr.Get("ArenaWnd_15"));

            return;
        }

        // 分享回放
        VideoMgr.ShareVideo(mVideoId);
    }

    /// <summary>
    /// 继续回放按钮点击事件
    /// </summary>
    void OnClickKeepOnPlaybackBtn(GameObject go)
    {
        // 执行回调
        if (mCallBack != null)
            mCallBack.Go();

        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 重播回放按钮点击回调
    /// </summary>
    void OnClickReplayPlaybackBtn(GameObject go)
    {
        if (TimeMgr.GetServerTime() - mVideoDatils.GetValue<int>("time") >= mVideoValidTime)
        {
            DialogMgr.Notify(LocalizationMgr.Get("ArenaWnd_11"));

            return;
        }

        if (mIsOperate)
            return;

        mIsOperate = true;

        // 播放战斗回放
        VideoMgr.PlayVideo(mVideoId);
    }

    /// <summary>
    /// 离开回放按钮点击回调
    /// </summary>
    void OnClickLeavePlaybackBtn(GameObject go)
    {
        // 打开主场景竞技场窗口
        if (string.Equals(SceneMgr.MainScene, SceneConst.SCENE_WORLD_MAP))
            SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(OpenArenaWnd));
        else
            SceneMgr.LoadScene("Main", SceneConst.SCENE_MAIN_CITY, new CallBack(OpenArenaWnd));
    }

    /// <summary>
    /// 显示竞技场窗口
    /// </summary>
    private void OpenArenaWnd(object para, object[] param)
    {
        //离开副本;
        InstanceMgr.LeaveInstance(ME.user);

        // 销毁本窗口
        WindowMgr.DestroyWindow(gameObject.name);

        // 战斗回放分页
        int page = (int) ARENA_PAGE.PLAYBACK_PAGE;

        // 打开竞技场窗口
        GameObject arenaWnd = WindowMgr.OpenWnd("ArenaWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (arenaWnd == null)
            return;

        // 绑定数据
        arenaWnd.GetComponent<ArenaWnd>().BindPage(page);
    }

    /// <summary>
    /// 播放视频回调
    /// </summary>
    void OnPlayVideoEvent(int eventId, MixedValue para)
    {
        mIsOperate = false;

        bool result = para.GetValue<bool>();
        if (!result)
            return;

        WindowMgr.DestroyWindow(gameObject.name);
        WindowMgr.DestroyWindow(ChatWnd.WndType);
        WindowMgr.HideMainWnd();
        WindowMgr.DestroyWindow(ArenaWnd.WndType);
    }

    /// <summary>
    /// 分享视频事件回调
    /// </summary>
    void OnShareVideoEvent(int eventId, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();

        string videoId = data.GetValue<string>("id");

        DialogMgr.ShowSingleBtnDailog(new CallBack(OnDialogCallBack, videoId), LocalizationMgr.Get("ArenaFightSettlementWnd_9"));
    }


    void OnDialogCallBack(object para, params object[] param)
    {
        // 回放录像id
        string videoId = para as string;

        string message = string.Format(LocalizationMgr.Get("ArenaFightSettlementWnd_10"), mLeftName.text, mRightName.text);

        LPCArray publish = LPCArray.Empty;

        LPCMapping data = LPCMapping.Empty;
        data.Add("video_id", videoId);

        publish.Add(data);

        // 向聊天频道发送消息
        ChatRoomMgr.SendChatMessage(ME.user, ChatConfig.WORLD_CHANNEL, string.Empty, publish, message);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenScaleFinish);

        // 绑定mVideoIdCopyWnd
        UIEventListener.Get(mVideoIdCopyWnd.gameObject).onClick = OnClickCopyBtn;

        // 注册事件 EVENT_VIDEO_DETAILS
        EventMgr.RegisterEvent(PlaybackInfoWnd.WndType, EventMgrEventType.EVENT_VIDEO_DETAILS, OnVideoDetailsEvent);
        EventMgr.RegisterEvent(PlaybackInfoWnd.WndType, EventMgrEventType.EVENT_PLAY_VIDEO, OnPlayVideoEvent);
        EventMgr.RegisterEvent(PlaybackInfoWnd.WndType, EventMgrEventType.EVENT_SHARE_VIDEO, OnShareVideoEvent);
    }

    /// <summary>
    /// 复制VideoId剪切板
    /// </summary>
    void OnClickCopyBtn(GameObject go)
    {
        // 给出提示信息
        DialogMgr.Notify(string.Format(LocalizationMgr.Get("PlaybackInfoWnd_12")));

        // 调用平台接口复制VideoId剪切板
        QCCommonSDK.Native.NativeCall.CopyToNativeClipboard(mVideoId);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 初始化
        for (int i = 0; i < mLeftStars.Length; i++)
            mLeftStars[i].spriteName = "arena_star_bg";

        // 初始化
        for (int i = 0; i < mRightStar.Length; i++)
            mRightStar[i].spriteName = "arena_star_bg";

        ShowBtn();

        // 显示mVideoId
        mVideoIdWnd.text = mVideoId;
        mVideoIdTipWnd.text = LocalizationMgr.Get("PlaybackInfoWnd_10");
        mVideoIdCopyWnd.text = LocalizationMgr.Get("PlaybackInfoWnd_11");

        mLeftResultTips.gameObject.SetActive(false);
        mRightResultTips.gameObject.SetActive(false);

        if (mVideoDatils == null || mVideoDatils.Count == 0)
            return;

        if (mIsClearance)
        {
            if (mVideoDatils.GetValue<int>("result") == 1)
            {
                mLeftResultTips.gameObject.SetActive(true);
            }
            else
            {
                mRightResultTips.gameObject.SetActive(true);
            }
        }

        // 攻击方详细数据
        LPCMapping attack = mVideoDatils.GetValue<LPCMapping>("attack");
        if (attack != null && attack.Count != 0)
        {
            // 玩家名称
            mLeftName.text = attack.GetValue<string>("name");

            int score = attack.GetValue<int>("score");

            mAttackLevel = attack.GetValue<int>("level");

            // 竞技场积分
            mLeftScore.text = score.ToString();

            int step = ArenaMgr.GetStepByScoreAndRank(attack.GetValue<int>("rank"), score);

            CsvRow row = ArenaMgr.TopBonusCsv.FindByKey(step);
            if (row != null)
            {
                for (int i = 0; i < row.Query<int>("star"); i++)
                    mLeftStars[i].spriteName = row.Query<string>("star_name");
            }
        }

        // 攻击方详细数据
        LPCMapping defence = mVideoDatils.GetValue<LPCMapping>("defence");
        if (defence != null && defence.Count != 0)
        {
            // 玩家名称
            mRightName.text = defence.GetValue<string>("name");

            int score = defence.GetValue<int>("score");

            mDefenceLevel = defence.GetValue<int>("level");

            // 竞技场积分
            mRightScore.text = score.ToString();

            int step = ArenaMgr.GetStepByScoreAndRank(defence.GetValue<int>("rank"), score);

            for (int i = 0; i < mRightStar.Length; i++)
                mRightStar[i].spriteName = "star_bg";

            CsvRow row = ArenaMgr.TopBonusCsv.FindByKey(step);
            if (row != null)
            {
                for (int i = 0; i < row.Query<int>("star"); i++)
                    mRightStar[i].spriteName = row.Query<string>("star_name");
            }
        }

        // 战斗时间
        mCombatTime.text = TimeMgr.ConvertTimeToSimpleChinese(mVideoDatils.GetValue<int>("time"));

        // 没有战斗数据
        if (!mVideoDatils.ContainsKey("combat_data"))
            return;

        LPCMapping combatData = mVideoDatils.GetValue<LPCMapping>("combat_data");
        if (!combatData.ContainsKey("defenders") || !combatData.ContainsKey("fighter_map"))
            return;

        // 攻击数据
        LPCMapping fighterMap = combatData.GetValue<LPCMapping>("fighter_map");

        if (fighterMap.ContainsKey(FormationConst.RAW_NONE))
        {
            LPCArray attackList = fighterMap.GetValue<LPCArray>(FormationConst.RAW_NONE);

            // 显示攻击列表
            ShowAttackList(attackList);
        }

        // 防御数据
        LPCMapping defenders = combatData.GetValue<LPCMapping>("defenders");
        if (!defenders.ContainsKey("defense_list"))
            return;

        // 防御列表
        LPCArray defenceList = defenders.GetValue<LPCArray>("defense_list");

        // 显示防御列表
        ShowDefenceList(defenceList);
    }

    /// <summary>
    /// 显示查看按钮
    /// </summary>
    void ShowBtn()
    {
        if(!mLeftBtn.activeSelf)
            mLeftBtn.SetActive(true);

        if(!mRightBtn.activeSelf)
            mRightBtn.SetActive(true);

        mLeftBtn.transform.localPosition = new Vector3(-131, -87, 0);
        mRightBtn.transform.localPosition = new Vector3(125, -87, 0);

        if (mIsClearance)
        {
            // 注册重播按钮点击事件
            UIEventListener.Get(mLeftBtn).onClick = OnClickReplayPlaybackBtn;

            mLeftBtnLb.text = LocalizationMgr.Get("PlaybackInfoWnd_7");

            // 注册离开回放按钮点击事件
            UIEventListener.Get(mRightBtn).onClick = OnClickLeavePlaybackBtn;
            mRightBtnLb.text = LocalizationMgr.Get("PlaybackInfoWnd_8");
        }
        else if (mIsPause)
        {
            // 注册分享按钮点击事件
            UIEventListener.Get(mLeftBtn).onClick = OnClickSharePlaybackBtn;

            mLeftBtnLb.text = LocalizationMgr.Get("PlaybackInfoWnd_5");

            // 注册继续回放按钮点击事件
            UIEventListener.Get(mRightBtn).onClick = OnClickKeepOnPlaybackBtn;
            mRightBtnLb.text = LocalizationMgr.Get("PlaybackInfoWnd_6");
        }
        else
        {
            mRightBtn.SetActive(false);

            mLeftBtn.transform.localPosition = new Vector3(10, -87, 0);

            // 注册战斗回放按钮点击事件
            UIEventListener.Get(mLeftBtn).onClick = OnClickPlaybackBtn;

            mLeftBtnLb.text = LocalizationMgr.Get("PlaybackInfoWnd_2");
        }
    }

    /// <summary>
    /// 显示攻击列表
    /// </summary>
    void ShowAttackList(LPCArray attackList)
    {
        for (int i = 0; i < mLeftList.Length; i++)
        {
            PetItemWnd script = mLeftList[i].GetComponent<PetItemWnd>();
            if (script == null)
                continue;

            if (i + 1 > attackList.Count)
            {
                script.SetBind(null);
            }
            else
            {
                LPCMapping data = LPCValue.Duplicate(attackList[i]).AsMapping;
                if (data.Count == 0)
                {
                    script.SetBind(null);

                    // 无buff
                    mLeftLeaderDesc.text = LocalizationMgr.Get("DefenceDeployWnd_10");
                }
                else
                {
                    data["rid"].AsString = Rid.New();

                    Property ob = PropertyMgr.CreateProperty(data);

                    mObList.Add(ob);

                    // 载入下属物件
                    DoPropertyLoaded(ob);

                    // 刷新宠物属性
                    PropMgr.RefreshAffect(ob);

                    script.SetBind(ob);

                    script.mIsAttack = true;

                    if (i == 0)
                    {
                        if (ob == null)
                        {
                            // 无buff
                            mLeftLeaderDesc.text = LocalizationMgr.Get("DefenceDeployWnd_10");
                        }
                        else
                        {
                            string leaderDesc = SkillMgr.GetLeaderSkillDesc(ob);

                            mLeftLeaderDesc.text = string.IsNullOrEmpty(leaderDesc) ? LocalizationMgr.Get("DefenceDeployWnd_10") : leaderDesc;
                        }
                    }
                }
            }

            // 注册点击事件
            UIEventListener.Get(mLeftList[i]).onClick = OnClickItem;
        }
    }

    /// <summary>
    /// 显示防御列表
    /// </summary>
    void ShowDefenceList(LPCArray defenceList)
    {
        for (int i = 0; i < mRightList.Length; i++)
        {
            PetItemWnd script = mRightList[i].GetComponent<PetItemWnd>();
            if (script == null)
                continue;

            if (i + 1 > defenceList.Count)
            {
                script.SetBind(null);
            }
            else
            {
                LPCMapping data = LPCValue.Duplicate(defenceList[i]).AsMapping;
                if (data.Count == 0)
                {
                    script.SetBind(null);

                    // 无buff
                    mRightLeaderDesc.text = LocalizationMgr.Get("DefenceDeployWnd_10");
                }
                else
                {
                    data["rid"].AsString = Rid.New();

                    Property ob = PropertyMgr.CreateProperty(data);

                    mObList.Add(ob);

                    // 载入下属物件
                    DoPropertyLoaded(ob);

                    // 刷新宠物属性
                    PropMgr.RefreshAffect(ob);

                    script.SetBind(ob);
                    script.mIsAttack = false;

                    if (i == 0)
                    {
                        if (ob == null)
                        {
                            // 无buff
                            mRightLeaderDesc.text = LocalizationMgr.Get("DefenceDeployWnd_10");
                        }
                        else
                        {
                            string leaderDesc = SkillMgr.GetLeaderSkillDesc(ob);

                            mRightLeaderDesc.text = string.IsNullOrEmpty(leaderDesc) ? LocalizationMgr.Get("DefenceDeployWnd_10") : leaderDesc;
                        }
                    }
                }
            }

            // 注册点击事件
            UIEventListener.Get(mRightList[i]).onClick = OnClickItem;
        }
    }

    /// <summary>
    /// 载入下属物件
    /// </summary>
    private void DoPropertyLoaded(Property owner)
    {
        // 获取角色的附属道具
        LPCArray propertyList = owner.Query<LPCArray>("properties");

        // 角色没有附属装备信息
        if (propertyList == null ||
            propertyList.Count == 0)
            return;

        // 转换Container
        Container container = owner as Container;
        Property proOb;

        // 遍历各个附属道具
        foreach (LPCValue data in propertyList.Values)
        {
            LPCMapping dbase = LPCValue.Duplicate(data).AsMapping;

            dbase.Add("rid", Rid.New());

            // 构建对象
            proOb = PropertyMgr.CreateProperty(dbase, true);
            equipData.Add(proOb);

            // 构建对象失败
            if (proOb == null)
                continue;

            // 将道具载入包裹中
            container.LoadProperty(proOb, dbase["pos"].AsString);
        }
    }

    void OnClickItem(GameObject go)
    {
        if (TimeMgr.GetServerTime() - mVideoDatils.GetValue<int>("time") >= mVideoValidTime)
        {
            DialogMgr.Notify(LocalizationMgr.Get("ArenaWnd_11"));

            // 关闭当前界面
            WindowMgr.DestroyWindow(gameObject.name);

            return;
        }

        PetItemWnd script = go.GetComponent<PetItemWnd>();
        if (script == null)
            return;

        Property ob = script.item_ob;

        if (ob == null)
            return;

        bool isAttack = script.mIsAttack;

        // 宠物信息窗口
        GameObject wnd = WindowMgr.OpenWnd(PetInfoWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<PetInfoWnd>().Bind(ob.GetRid(), isAttack ? mLeftName.text : mRightName.text, isAttack ? mAttackLevel : mDefenceLevel);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(string videoId, CallBack cb = null, bool isClearance = false, bool isPause = false)
    {
        mVideoId = videoId;

        mCallBack = cb;

        mIsClearance = isClearance;

        mIsPause = isPause;

        mVideoDatils = VideoMgr.VideoDetails;
        if (mVideoDatils == null)
            mVideoDatils = LPCMapping.Empty;

        if (mVideoDatils.ContainsKey("id") && mVideoId == mVideoDatils.GetValue<string>("id"))
        {
            // 重绘窗口
            Redraw();
        }
        else
        {
            // 获取视频详细数据
            VideoMgr.GetVideoDetails(mVideoId);
        }
    }
}
