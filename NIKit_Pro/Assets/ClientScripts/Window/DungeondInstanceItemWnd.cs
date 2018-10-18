/// <summary>
/// DungeondInstanceItemWnd.cs
/// Created by fengsc 2016/12/30
/// 地下城副本列表基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class DungeondInstanceItemWnd : WindowBase<DungeondInstanceItemWnd>
{
    // 地下城副本boss头像
    public UITexture mIcon;

    // boss等级
    public UILabel mBossLevel;

    // 副本名称
    public UILabel mInstanceName;

    // 任务完成标识
    public UISprite mBox;

    // 任务数量
    public UILabel mTaskAmount;

    // 是否通关
    public UILabel mIsClearance;

    public UILabel mCanClearance;

    public UILabel mNoClearance;

    public UILabel mClearanceTips;

    // 消耗图标
    public UISprite mCostIcon;

    // 战斗消耗
    public UILabel mCost;

    // 战斗按钮
    public GameObject mBattleBtn;
    public UILabel mBattleBtnLb;

    // 已通关按钮
    public GameObject mClearancedBtn;

    // 红点提示
    public GameObject mRedPoint;

    public GameObject mMask;

    public GameObject mNormal;
    public GameObject mFriend;

    // 分享按钮
    public GameObject mShareBtn;
    public UILabel mShareBtnLb;

    // 好友名称
    public UILabel mFriendName;

    // 元素图标
    public UISprite mElement;

    // 好友地下城副本名称
    public UILabel mFriendInstanceName;

    // 倒计时
    public UILabel mTimer;

    public GameObject[] mStars;

    LPCMapping mInstanceData = LPCMapping.Empty;

    LPCMapping mExtraPara = LPCMapping.Empty;

    // 副本id
    string mInstanceId = string.Empty;

    int mInterval = 0;

    bool mIsCountDown = false;
    float mLastTime = 0;

    // 隐藏地下城宠物对象
    Property mOb = null;

    bool mIsShare = false;

    void Start()
    {
        // 注册按钮的点击事件
        UIEventListener.Get(mBattleBtn).onClick = OnClickBattleBtn;
        UIEventListener.Get(mShareBtn).onClick = OnClickShareBtn;
        UIEventListener.Get(mClearancedBtn).onClick = OnClickClearancedBtn;
    }

    void Update()
    {
        if (mIsCountDown)
        {
            if (Time.realtimeSinceStartup > mLastTime + 1)
            {
                mLastTime = Time.realtimeSinceStartup;
                CountDown();
            }
        }
    }

    void OnDestroy()
    {
        // 析构掉创建的宠物
        if (mOb != null)
            mOb.Destroy();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mRedPoint.SetActive(false);

        mIsClearance.text = LocalizationMgr.Get("DungeonsWnd_4");
        mNoClearance.text = LocalizationMgr.Get("DungeonsWnd_18");
        mClearanceTips.text = LocalizationMgr.Get("DungeonsWnd_8");
        mCanClearance.text = LocalizationMgr.Get("DungeonsWnd_5");
        mShareBtnLb.text = LocalizationMgr.Get("DungeonsWnd_19");

        for (int i = 0; i < mStars.Length; i++)
            mStars[i].SetActive(false);

        // 没有获取到副本配置数据
        if (mInstanceData.Count < 1)
            return;

        // 获取副本id
        mInstanceId = mInstanceData.GetValue<string>("instance_id");
        if (string.IsNullOrEmpty(mInstanceId))
            return;

        // 总的任务数量
        int sumTask = 0;

        // 剩余的任务数量
        int remainTask = 0;

        // TODO:显示任务数量
        mTaskAmount.text = string.Format(LocalizationMgr.Get("DungeonsWnd_10"), remainTask, sumTask);

        // 当前副本已经通关
        if (InstanceMgr.IsClearanced(ME.user, mInstanceId, mExtraPara))
        {
            // 根据副本通关情况设置副本通关情况
            if (! InstanceMgr.CanRepeatClearance(mInstanceId))
            {
                mClearancedBtn.SetActive(true);
                mBattleBtn.SetActive(false);
            }
            else
            {
                mClearancedBtn.SetActive(false);
                mBattleBtn.SetActive(true);
            }

            // 设置其他控件的显示情况
            mIsClearance.gameObject.SetActive(true);
            mNoClearance.gameObject.SetActive(false);
            mClearanceTips.gameObject.SetActive(false);
            mCanClearance.gameObject.SetActive(false);

            mBattleBtnLb.text = LocalizationMgr.Get("DungeonsWnd_6");
            mMask.SetActive(false);
        }
        else
        {
            // 隐藏以通关按钮
            mClearancedBtn.SetActive(false);
            mBattleBtn.SetActive(true);

            // 判断副本是否解锁
            if (InstanceMgr.IsUnlocked(ME.user, mInstanceId, mExtraPara))
            {
                mIsClearance.gameObject.SetActive(false);
                mNoClearance.gameObject.SetActive(false);
                mClearanceTips.gameObject.SetActive(false);
                mCanClearance.gameObject.SetActive(true);

                mBattleBtnLb.text = LocalizationMgr.Get("DungeonsWnd_6");
                mMask.SetActive(false);
            }
            else
            {
                mIsClearance.text = LocalizationMgr.Get("DungeonsWnd_8");
                mBattleBtnLb.text = LocalizationMgr.Get("DungeonsWnd_7");
                mMask.SetActive(true);
                mTaskAmount.text = LocalizationMgr.Get("DungeonsWnd_9");

                mIsClearance.gameObject.SetActive(false);
                mNoClearance.gameObject.SetActive(true);
                mClearanceTips.gameObject.SetActive(true);
                mCanClearance.gameObject.SetActive(false);
            }
        }

        // 获取副本的消耗
        LPCMapping costMap = InstanceMgr.GetInstanceCostMap(ME.user, mInstanceId, mExtraPara);

        if (costMap != null)
        {
            string fields = FieldsMgr.GetFieldInMapping(costMap);

            // 显示进入副本开销的属性图标
            mCostIcon.spriteName = FieldsMgr.GetFieldIcon(fields);

            // 显示进入副本开销
            mCost.text = costMap.GetValue<int>(fields).ToString();
        }

        // 获取副本名称
        mInstanceName.text = InstanceMgr.GetInstanceName(mInstanceId, mExtraPara);

        // 获取副本地图类型
        int mapType = InstanceMgr.GetInstanceMapType(mInstanceId);

        // 精英副本
        if (mapType == MapConst.PET_DUNGEONS_MAP)
        {
            mFriend.SetActive(false);
            mNormal.SetActive(true);

            // 显示当前副本的boss等级
            mBossLevel.text = string.Format(LocalizationMgr.Get("DungeonsWnd_11"), mInstanceData.GetValue<int>("level"));

            // 创建宠物对象
            int rank = 0;
            int classId = mExtraPara.GetValue<int>("pet_id");

            if (mExtraPara.ContainsKey("rank"))
                rank = mExtraPara.GetValue<int>("rank");
            else
                rank = MonsterMgr.GetDefaultRank(classId);

            //设置玩家头像;
            mIcon.mainTexture = MonsterMgr.GetTexture(classId, rank);

            int star = 0;
            if (mExtraPara.ContainsKey("star"))
                star = mExtraPara.GetValue<int>("star");
            else
                star = MonsterMgr.GetDefaultStar(classId);

            // 显示星级
            for (int i = 0; i < star; i++)
                mStars[i].SetActive(true);
        }
        else if(mapType == MapConst.SECRET_DUNGEONS_MAP)
        {
            mFriend.SetActive(true);
            mNormal.SetActive(false);

            // 创建宠物对象
            int rank = 0;
            int classId = mExtraPara.GetValue<int>("pet_id");

            if (mExtraPara.ContainsKey("rank"))
                rank = mExtraPara.GetValue<int>("rank");
            else
                rank = MonsterMgr.GetDefaultRank(classId);

            LPCMapping para = LPCMapping.Empty;
            para.Add("class_id", classId);
            para.Add("rank", rank);
            para.Add("rid", Rid.New());

            if (mOb != null)
                mOb.Destroy();

            // 构建宠物对象
            mOb = PropertyMgr.CreateProperty(para);

            if (mOb == null)
                return;

            mFriendInstanceName.text = LocalizationMgr.Get(mOb.GetName());

            mElement.spriteName = PetMgr.GetElementIconName(MonsterMgr.GetElement(mOb.GetClassID()));

            mFriendName.gameObject.SetActive(true);

            mTimer.transform.localPosition = new Vector3(-52.7f, -29.2f, 0);

            mFriendName.transform.localPosition = new Vector3(72, -29, 0);

            if (string.Equals(mExtraPara.GetValue<string>("owner"), ME.user.GetRid()))
            {
                if (GuideMgr.IsGuided(GuideMgr.SHARE_SHOW_GUIDE_GROUP) && ShareMgr.IsOpenShare())
                    mShareBtn.SetActive(true);
                else
                    mShareBtn.SetActive(false);

                int count = 0;

                LPCValue holder = mExtraPara.GetValue<LPCValue>("holder");
                if (holder != null && holder.IsArray)
                    count = holder.AsArray.Count;

                LPCMapping maxHolder = GameSettingMgr.GetSetting<LPCMapping>("max_dynamic_map_holder");

                // 好友名称
                mFriendName.text = string.Format(LocalizationMgr.Get("DungeonsWnd_26"), count, maxHolder.GetValue<int>(mExtraPara.GetValue<int>("type")));
            }
            else
            {
                // 好友名称
                if (mExtraPara.ContainsKey("owner_name"))
                    mFriendName.text = mExtraPara.GetValue<string>("owner_name");
                else
                {
                    mFriendName.gameObject.SetActive(false);

                    mTimer.transform.localPosition = new Vector3(10, -29, 0);
                }

                mShareBtn.SetActive(false);
            }

            for (int i = 0; i < mExtraPara.GetValue<int>("star"); i++)
                mStars[i].SetActive(true);

            string iconName = MonsterMgr.GetIcon(classId, rank);
            string resPath = string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconName);
            Texture2D iconRes = ResourceMgr.LoadTexture(resPath);

            //设置玩家头像;
            if (iconRes != null)
                mIcon.mainTexture = iconRes;

            // 秘密地下城的结束时间
            int endTime = mExtraPara.GetValue<int>("end_time");

            // 计算剩余的时间间隔
            mInterval = Mathf.Max(endTime - TimeMgr.GetServerTime(), 0);

            // 开启倒计时
            mIsCountDown = true;
        } else
        {
            mFriend.SetActive(false);
            mNormal.SetActive(true);

            // 显示当前副本的boss等级
            mBossLevel.text = string.Format(LocalizationMgr.Get("DungeonsWnd_11"), mInstanceData.GetValue<int>("level"));

            string iconName = mInstanceData.GetValue<string>("icon");
            string resPath = string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconName);
            Texture2D iconRes = ResourceMgr.LoadTexture(resPath);

            //设置玩家头像;
            if (iconRes != null)
                mIcon.mainTexture = iconRes;

            LPCMapping data = InstanceMgr.GetInstanceBossData(mInstanceId);

            for (int i = 0; i < data.GetValue<int>("star"); i++)
                mStars[i].SetActive(true);
        }
    }

    /// <summary>
    /// 倒计时
    /// </summary>
    void CountDown()
    {
        if (mInterval < 1)
        {
            // 结束调用
            mIsCountDown = false;
            return;
        }

        mInterval--;

        if (mInterval < 3600)
        {
            // 剩余多少分钟
            mTimer.text = string.Format(LocalizationMgr.Get("DungeonsWnd_21"), mInterval / 60);
        }
        else if (mInterval < 60)
        {
            // 剩余多少秒
            mTimer.text = string.Format(LocalizationMgr.Get("DungeonsWnd_22"), mInterval);
        }
        else
        {
            // 剩余多少小时
            mTimer.text = string.Format(LocalizationMgr.Get("DungeonsWnd_20"), mInterval / 3600);
        }
    }

    /// <summary>
    /// 检测隐藏剩余容量是否达到最大限制
    /// </summary>
    bool CheckMaxHolder()
    {
        int count = 0;
        LPCValue holder = mExtraPara.GetValue<LPCValue>("holder");
        if (holder != null && holder.IsArray)
            count = holder.AsArray.Count;

        LPCMapping maxHolder = GameSettingMgr.GetSetting<LPCMapping>("max_dynamic_map_holder");

        if (count != 0 && count >= maxHolder.GetValue<int>(mExtraPara.GetValue<int>("type")))
            return true;

        return false;
    }

    /// <summary>
    /// 战斗按钮点击事件
    /// </summary>
    void OnClickBattleBtn(GameObject go)
    {
        // 当前副本的前置副本未解锁
        if (!InstanceMgr.IsUnlocked(ME.user, mInstanceId, mExtraPara))
        {
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("DungeonsWnd_15"),
                string.Empty,
                string.Empty,
                true,
                WindowMgr.GetWindow(DungeonsWnd.WndType).transform
            );
            return;
        }

        if (InstanceMgr.GetInstanceMapType(mInstanceId).Equals(MapConst.SECRET_DUNGEONS_MAP))
        {
            // 判断是否能够进入秘密圣域
            if (! CanEnterSecretDungeonsMap())
            {
                DialogMgr.ShowSingleBtnDailog(
                    null,
                    LocalizationMgr.Get("DungeonsWnd_27"),
                    string.Empty,
                    string.Empty,
                    true,
                    WindowMgr.GetWindow(DungeonsWnd.WndType).transform
                );

                return;
            }
        }

        // 创建选择战斗界面
        GameObject wnd = WindowMgr.OpenWnd(SelectFighterWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
        {
            LogMgr.Trace("SelectFighterWnd窗口创建失败");
            return;
        }

        if (string.IsNullOrEmpty(mInstanceId))
            return;

        // 关闭地下城窗口
        WindowMgr.HideWindow(DungeonsWnd.WndType);

        // 绑定数据
        wnd.GetComponent<SelectFighterWnd>().Bind(DungeonsWnd.WndType, mInstanceId, mExtraPara);
    }

    /// <summary>
    /// 判断是否允许进入秘密地下城
    /// </summary>
    private bool CanEnterSecretDungeonsMap()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return false;

        // 如果地图没有owner标识不需要占有所有人都可以
        string owner = mExtraPara.GetValue<string>("owner");
        if (string.IsNullOrEmpty(owner))
            return true;

        // 判断是自己的圣域, 可以进入
        string userRid = ME.user.GetRid();
        if (string.Equals(userRid, owner))
            return true;

        // 获取holder信息
        // 如果玩家已经占有过该秘密地下城
        LPCArray holder = mExtraPara.GetValue<LPCArray>("holder");
        if (holder == null || holder.IndexOf(userRid) != -1)
            return true;

        // 如果列表已满不允许进入
        LPCMapping maxHolder = GameSettingMgr.GetSetting<LPCMapping>("max_dynamic_map_holder");
        if (holder.Count >= maxHolder.GetValue<int>(mExtraPara.GetValue<int>("type")))
            return false;

        // 可以进入
        return true;
    }

    /// <summary>
    /// 分享按钮点击事件
    /// </summary>
    void OnClickShareBtn(GameObject go)
    {
        if (mOb == null)
            return;

        if (InstanceMgr.GetInstanceMapType(mInstanceId).Equals(MapConst.SECRET_DUNGEONS_MAP))
        {
            // 好友数量达到最大数量
            if (FriendMgr.FriendList.Count >= GameSettingMgr.GetSettingInt("max_friend_amount"))
            {
                DialogMgr.ShowSingleBtnDailog(
                    null,
                    LocalizationMgr.Get("DungeonsWnd_29"),
                    string.Empty,
                    string.Empty,
                    true,
                    WindowMgr.GetWindow(DungeonsWnd.WndType).transform
                );
                return;
            }

            if (CheckMaxHolder())
            {
                // 好友容量达到上限
                DialogMgr.ShowSingleBtnDailog(
                    null,
                    LocalizationMgr.Get("DungeonsWnd_28"),
                    string.Empty,
                    string.Empty,
                    true,
                    WindowMgr.GetWindow(DungeonsWnd.WndType).transform
                );

                return;
            }
        }

        // 副本配置信息
        LPCMapping data = InstanceMgr.GetInstanceInfo(mInstanceId);
        if (data == null)
            return;

        if (ShareMgr.IsOpenShare())
        {
            // 这个地方有一个处理比较另类的地方，在确认回调中需要上传副本前缀（比如"隐藏圣域："）
            // 但是副本名字格式（隐藏圣域：火·草精），所以在设个发布前缀单独处理了一下
            DialogMgr.ShowDailog(
                new CallBack(OnClickShareDialogCallBack, InstanceMgr.GetInstanceName(mInstanceId, LPCMapping.Empty)),
                LocalizationMgr.Get("DungeonsWnd_25"),
                LocalizationMgr.Get("DungeonsWnd_19"),
                LocalizationMgr.Get("DungeonsWnd_31"),
                LocalizationMgr.Get("DungeonsWnd_32"),
                true,
                WindowMgr.GetWindow(DungeonsWnd.WndType).transform
            );
        }
        else
        {
            // 这个地方有一个处理比较另类的地方，在确认回调中需要上传副本前缀（比如"隐藏圣域："）
            // 但是副本名字格式（隐藏圣域：火·草精），所以在设个发布前缀单独处理了一下
            DialogMgr.ShowDailog(
                new CallBack(OnClickShareDialogCallBack, InstanceMgr.GetInstanceName(mInstanceId, LPCMapping.Empty)),
                LocalizationMgr.Get("DungeonsWnd_25"),
                LocalizationMgr.Get("DungeonsWnd_19"),
                LocalizationMgr.Get("DungeonsWnd_32"),
                LocalizationMgr.Get("DungeonsWnd_33"),
                true,
                WindowMgr.GetWindow(DungeonsWnd.WndType).transform
            );
        }
    }

    /// <summary>
    /// Raises the click clearanced button event.
    /// </summary>
    /// <param name="go">Go.</param>
    void OnClickClearancedBtn(GameObject go)
    {
        // 给出提示信息
        DialogMgr.Notify(string.Format(LocalizationMgr.Get("DungeonsWnd_30"),
            InstanceMgr.GetInstanceName(mInstanceId, mExtraPara)));
    }

    void DoSharePlatform()
    {
        // 使魔对象为空
        if (mOb == null)
            return;

        //分享开启隐藏圣域
        GameObject shareWnd = WindowMgr.OpenWnd(ShareOperateWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (shareWnd == null)
            return;

        shareWnd.GetComponentInChildren<ShareOperateWnd>().BindData(ShareOperateWnd.ShareOperateType.Sacred, mOb);
    }

    void DoShareChat(string instanceName)
    {
        // 发送至聊天频道
        GameObject parentWnd = WindowMgr.GetWindow(DungeonsWnd.WndType);
        if (parentWnd == null)
            return;

        // 隐藏父级窗口
        WindowMgr.HideWindow(parentWnd);

        // 打开聊天界面
        GameObject wnd = WindowMgr.OpenWnd(ChatWnd.WndType);

        if (wnd == null)
        {
            LogMgr.Trace("ChatWnd窗口创建失败");
            return;
        }

        ChatWnd script = wnd.GetComponent<ChatWnd>();

        if (script == null)
            return;

        script.Bind(DungeonsWnd.WndType, new CallBack(SendShareCallBack));

        if (mIsShare)
            return;

        string input = string.Format("{0}{1}{2}", LocalizationMgr.Get("DungeonsWnd_23"), PublishMgr.GetPublicTag(ME.user, mOb), LocalizationMgr.Get("DungeonsWnd_24"));

        // 绑定数据
        script.BindPublish(mOb, input, instanceName);

        mIsShare = true;
    }

    /// <summary>
    /// 分享按钮点击弹框确认回调
    /// </summary>
    void OnClickShareDialogCallBack(object para, params object[] param)
    {
        if (ShareMgr.IsOpenShare())
        {
            if ((bool) param[0])
            {
                // 分享至社交平台
                DoSharePlatform();
            }
            else
            {
                // 执行分享至聊天系统
                DoShareChat(para as string);
            }
        }
        else
        {
            if (!(bool)param[0])
                return;

            // 执行分享至聊天系统
            DoShareChat(para as string);
        }
    }

    /// <summary>
    /// 消息发送回调
    /// </summary>
    void SendShareCallBack(object para, params object[] param)
    {
        mIsShare = false;
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data, LPCMapping extraPara)
    {
        // 没有绑定数据
        if (data == null)
            return;

        // 记录数据
        mInstanceData = data;
        mExtraPara = extraPara;

        // 绘制窗口
        Redraw();
    }
}
