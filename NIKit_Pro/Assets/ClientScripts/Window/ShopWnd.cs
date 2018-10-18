/// <summary>
/// ShopWnd.cs
/// Created by fengsc 2016/11/21
/// 市集购买界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ShopWnd : WindowBase<ShopWnd>
{
    #region 成员变量

    // 标题
    public UILabel mTitle;

    // 关闭按钮
    public GameObject mCloseBtn;

    // 刷新提示
    public UILabel mRefreshTips;

    // 刷新计时器
    public UILabel mRefreshTimer;

    // 刷新按钮
    public GameObject mRefreshBtn;
    public UILabel mRefreshBtnLb;

    // 刷新消耗图标
    public UITexture mCostIcon;

    // 刷新消耗数值
    public UILabel mCost;

    // 商品格子
    public GameObject mShopItem;

    // 排序组件
    public UIGrid mGrid;

    // 遮罩
    public GameObject mMask;

    public TweenScale mTweenScale;

    // 限制刷新次数
    public UILabel mLimitRefreshCount;

    // 商店刷新剩余时间
    int mRemainTime = 0;

    // 当前拥有的商品格子数量
    int mShopSize = 0;

    // 选中的格子
    ShopItemWnd mSelect = null;

    // 创建的物品信息窗口
    GameObject mInfoWnd = null;

    // 刷新消耗
    LPCMapping mRefreshCost = new LPCMapping();

    // 缓存集市列表格子
    List<GameObject> mObjects = new List<GameObject>();

    bool mIsCountDown = false;

    float mLastTime = 0;

    int mMaxShopRefresh = 0;

    int mLastRefreshFlag = 0;

    #endregion

    // Use this for initialization
    void Start()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        GameObject go = WindowMgr.GetWindow(MainWnd.WndType);
        if (go == null)
            return;
        WindowMgr.HideWindow(go);

        // 注册事件
        RegisterEvent();

        // 绘制窗口
        Redraw();

        // 每天零点刷新一次
        InvokeRepeating("RefreshLimitData", (float) Game.GetZeroClock(1), 86400);
    }

    void Update()
    {
        if (mIsCountDown)
        {
            if ((Time.realtimeSinceStartup > mLastTime + 1.0f))
            {
                mLastTime = Time.realtimeSinceStartup;
                CountDown();
            }
        }
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        // 移除事件监听
        EventMgr.UnregisterEvent("ShopWnd");

        CancelInvoke("RefreshLimitData");

        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 取消字段监听
        ME.user.dbase.RemoveTriggerField("ShopWnd");
        ME.user.tempdbase.RemoveTriggerField("ShopWnd");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mMask).onClick = OnClickCloseBtn;
        UIEventListener.Get(mRefreshBtn).onClick = OnClickRefreshBtn;

        if (ME.user != null)
        {
            // 监听字段的变化
            ME.user.dbase.RegisterTriggerField("ShopWnd", new string[]{ "refresh_shop" }, new CallBack(OnRefreshShopChange));
            ME.user.dbase.RegisterTriggerField("ShopWnd", new string[]{ "shop_goods" }, new CallBack(OnShopGoodsChange));
            ME.user.tempdbase.RegisterTriggerField("ShopWnd", new string[]{ "gapp_world" }, new CallBack(OnGappWorldFieldsChange));
        }

        EventMgr.RegisterEvent("ShopWnd", EventMgrEventType.EVENT_BUY_ITEM_SUCCESS, OnBuyItemSuccCallBack);

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    void OnGappWorldFieldsChange(object para, params object[] param)
    {
        // 刷新限制数据
        RefreshLimitData();
    }

    /// <summary>
    /// 道具购买成功
    /// </summary>
    void OnBuyItemSuccCallBack(int eventId, MixedValue para)
    {
        // 销毁宠物信息查看窗口
        WindowMgr.DestroyWindow(PetSimpleInfoWnd.WndType);

        // 销毁道具信息查看界面
        WindowMgr.DestroyWindow(RewardItemInfoWnd.WndType);

        // 刷新商品数据
        RefreshShopData();
    }

    /// <summary>
    /// tween动画回调
    /// </summary>
    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLocalText()
    {
        mTitle.text = LocalizationMgr.Get("ShopWnd_1");
        mRefreshTips.text = LocalizationMgr.Get("ShopWnd_2");
        mRefreshBtnLb.text = LocalizationMgr.Get("ShopWnd_3");
    }

    /// <summary>
    /// 字段变化事件回调
    /// </summary>
    void OnRefreshShopChange(object para, params object[] param)
    {
        // 延迟刷新计时器
        MergeExecuteMgr.DispatchExecute(RefreshTimer);

        // 自动刷新不累计刷新次数
        if (mLastRefreshFlag == 1)
            return;

        if (ME.user != null)
        {
            // 开启版署模式累计刷新次数
            if (ME.user.QueryTemp<int>("gapp_world") == 1 && mMaxShopRefresh != 0)
            {
                LPCMapping limitData = LPCMapping.Empty;

                LPCValue v = OptionMgr.GetLocalOption(ME.user, "limit_shop_refresh");
                if (v != null && v.IsMapping)
                    limitData = v.AsMapping;

                if (!TimeMgr.IsSameDay(limitData.GetValue<int>("refresh_time"), TimeMgr.GetServerTime()))
                    limitData = LPCMapping.Empty;

                // 累计次数
                limitData.Add("amount", Mathf.Min(limitData.GetValue<int>("amount") + 1, mMaxShopRefresh));

                // 缓存本次抽奖的时间
                limitData.Add("refresh_time", TimeMgr.GetServerTime());

                // 将数据缓存到本地
                OptionMgr.SetLocalOption(ME.user, "limit_shop_refresh", LPCValue.Create(limitData));
            }

            // 刷新限制次数
            ShowLimitRefreshCount();
        }
    }

    /// <summary>
    /// 字段变化事件回调
    /// </summary>
    void OnShopGoodsChange(object para, params object[] param)
    {
        // 刷新商品数据
        RefreshShopData();

        // 显示市集刷新消耗
        ShowCost();

        if (mInfoWnd != null)
            WindowMgr.DestroyWindow(mInfoWnd.name);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 创建商品列表格子
        CreateShopItem();

        // 刷新商品数据
        RefreshShopData();

        // 刷新计时器
        RefreshTimer();

        // 显示市集刷新消耗
        ShowCost();

        // 市集最大刷新次数
        mMaxShopRefresh = GameSettingMgr.GetSettingInt("max_shop_refresh");

        // 刷新限制数据
        RefreshLimitData();

        LPCMapping refreshShop = ME.user.Query<LPCMapping>("refresh_shop");

        if (refreshShop.GetValue<int>("refresh_flag") == 1)
        {
            mLastRefreshFlag = 1;

            Operation.CmdRefreshShop.Go();
        }
    }

    void RefreshLimitData()
    {
        // 开启版署模式累计许愿次数
        if (ME.user.QueryTemp<int>("gapp_world") == 1 && mMaxShopRefresh != 0)
        {
            LPCMapping limitData = LPCMapping.Empty;

            LPCValue v = OptionMgr.GetLocalOption(ME.user, "limit_shop_refresh");
            if (v != null && v.IsMapping)
                limitData = v.AsMapping;

            // 重置数据
            if (!TimeMgr.IsSameDay(TimeMgr.GetServerTime(), limitData.GetValue<int>("refresh_time")))
                OptionMgr.SetLocalOption(ME.user, "limit_shop_refresh", LPCValue.Create(LPCMapping.Empty));
        }

        // 限制数据
        ShowLimitRefreshCount();
    }

    /// <summary>
    /// 显示限制刷新次数
    /// </summary>
    void ShowLimitRefreshCount()
    {
        if (ME.user.QueryTemp<int>("gapp_world") == 1 && mMaxShopRefresh != 0)
        {
            LPCMapping limitData = LPCMapping.Empty;

            LPCValue v = OptionMgr.GetLocalOption(ME.user, "limit_shop_refresh");
            if (v != null && v.IsMapping)
                limitData = v.AsMapping;

            mLimitRefreshCount.text = string.Format(
                LocalizationMgr.Get("ShopWnd_10"),
                limitData.GetValue<int>("amount"),
                mMaxShopRefresh
            );

            mLimitRefreshCount.gameObject.SetActive(true);
        }
        else
        {
            mLimitRefreshCount.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 显示市集刷新消耗
    /// </summary>
    void ShowCost()
    {
        // 计算刷新需要的消耗
        mRefreshCost = CALC_REFRESH_GOODS_COST.Call(ME.user);

        string fields = FieldsMgr.GetFieldInMapping(mRefreshCost);

        int value = mRefreshCost.GetValue<int>(fields);

        mCostIcon.mainTexture = ItemMgr.GetTexture(FieldsMgr.GetFieldTexture(fields));

        mCost.text = value.ToString();
    }

    /// <summary>
    /// 创建商品格子
    /// </summary>
    void CreateShopItem()
    {
        mShopItem.SetActive(false);

        mObjects.Clear();

        // 市集商品列表最大数量
        int maxShopSize = GameSettingMgr.GetSettingInt("max_shop_size");

        for (int i = 0; i < maxShopSize; i++)
        {
            GameObject go = GameObject.Instantiate(mShopItem);
            go.transform.SetParent(mGrid.transform);

            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.name = "shop_item_" + i;

            go.transform.localPosition = new Vector3(go.transform.localPosition.x,
                go.transform.localPosition.y - i * 110,
                go.transform.localPosition.z);

            mObjects.Add(go);

            // 注册点击事件
            UIEventListener.Get(go).onClick = OnClickShopItemBtn;
        }
    }

    // 刷新商品数据
    void RefreshShopData()
    {
        mSelect = null;

        // 获取商品列表
        LPCValue shopList = ME.user.Query<LPCValue>("shop_goods");

        // 没有获取到商品列表信息
        if (shopList == null || !shopList.IsArray || mObjects.Count < 1)
            return;

        mShopSize = shopList.AsArray.Count;
        LPCValue data = new LPCValue();
        for (int i = 0; i < mObjects.Count; i++)
        {
            if (i + 1 <= mShopSize)
                data = shopList.AsArray[i];
            else
            {
                data = null;
            }

            GameObject go = mObjects[i];

            ShopItemWnd shopItem = go.GetComponent<ShopItemWnd>();

            // 绑定数据
            shopItem.Bind(data, i + 1);

            go.SetActive(true);
        }
    }

    /// <summary>
    /// 刷新计时器
    /// </summary>
    void RefreshTimer()
    {
        // 当前窗口已析构
        if (WindowMgr.GetWindow("ShopWnd") == null)
            return;

        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 获取集市刷新数据
        LPCMapping refreshShop = ME.user.Query<LPCMapping>("refresh_shop");

        // 商店上一次刷新的时间
        int lastRefreshTime = refreshShop.GetValue<int>("refresh_time");

        // 距离市集下次刷新的剩余时间
        mRemainTime = GameSettingMgr.GetSettingInt("refresh_shop_interval") - (TimeMgr.GetServerTime() - lastRefreshTime);

        // 开启倒计时
        mIsCountDown = true;
    }

    /// <summary>
    /// 刷新倒计时
    /// </summary>
    void CountDown()
    {
        if (mRemainTime < 0)
        {
            LPCMapping refreshShop = ME.user.Query<LPCMapping>("refresh_shop");

            // 允许免费自动刷新，请求服务器刷新商品列表
            if (refreshShop.GetValue<int>("refresh_flag") == 1)
            {
                mLastRefreshFlag = 1;

                Operation.CmdRefreshShop.Go();
            }

            // 取消倒计时
            mIsCountDown = false;
        }

        mRefreshTimer.text = TimeMgr.ConvertTimeToChineseTimer(mRemainTime);

        mRemainTime--;
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 执行关闭操作
        DoClose();
    }

    /// <summary>
    /// 刷新按钮点击事件
    /// </summary>
    void OnClickRefreshBtn(GameObject go)
    {
        // 开启版署模式
        if (ME.user.QueryTemp<int>("gapp_world") == 1 && mMaxShopRefresh != 0)
        {
            LPCMapping limitData = LPCMapping.Empty;

            LPCValue v = OptionMgr.GetLocalOption(ME.user, "limit_shop_refresh");
            if (v != null && v.IsMapping)
                limitData = v.AsMapping;

            // 今日许愿已达到上限次数
            if (limitData.GetValue<int>("amount") >= GameSettingMgr.GetSettingInt("max_shop_refresh")
                && TimeMgr.IsSameDay(TimeMgr.GetServerTime(), limitData.GetValue<int>("refresh_time")))
            {
                DialogMgr.Notify(LocalizationMgr.Get("ShopWnd_11"));
                return;
            }
        }

        string fields = FieldsMgr.GetFieldInMapping(mRefreshCost);

        if (mRefreshCost.ContainsKey("shop_refresh_card"))
        {
            // 使用刷新卷刷新
            DialogMgr.ShowDailog(
                new CallBack(RefreshCardCallback, mRefreshCost),
                string.Format(LocalizationMgr.Get("ShopWnd_9"), ME.user.Query<int>("shop_refresh_card")),
                string.Empty,
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
        }
        else
        {
            DialogMgr.ShowDailog(
                new CallBack(ConfirmRefreshCallBack, mRefreshCost),
                string.Format(LocalizationMgr.Get("ShopWnd_7"), FieldsMgr.GetFieldName(fields), mRefreshCost.GetValue<int>(fields)),
                string.Empty,
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
        }
    }

    /// <summary>
    /// 市集刷新卷刷新确认弹框回调
    /// </summary>
    void RefreshCardCallback(object para, params object[] param)
    {
        if (!(bool) param[0])
            return;

        LPCMapping cost = para as LPCMapping;

        // 执行刷新
        DoRefresh(cost);
    }

    void ConfirmRefreshCallBack(object para, params object[] param)
    {
        if (!(bool) param[0])
            return;

        LPCMapping cost = para as LPCMapping;

        // 执行刷新
        DoRefresh(cost);
    }

    /// <summary>
    /// 执行刷新
    /// </summary>
    void DoRefresh(LPCMapping cost)
    {
        string fields = FieldsMgr.GetFieldInMapping(cost);

        LPCValue v = ME.user.Query<LPCValue>(fields);
        if (v == null || !v.IsInt)
            return;

        if (v.AsInt < cost.GetValue<int>(fields))
        {
            DialogMgr.Notify(string.Format(LocalizationMgr.Get("ShopWnd_8"), FieldsMgr.GetFieldName(fields)));
            return;
        }

        mIsCountDown = false;

        LPCMapping refreshShop = ME.user.Query<LPCMapping>("refresh_shop");

        mLastRefreshFlag = refreshShop.GetValue<int>("refresh_flag");

        ShopMgr.UseCoinRefreshGoods(ME.user);
    }

    /// <summary>
    /// 商品列表格子点击事件
    /// </summary>
    void OnClickShopItemBtn(GameObject go)
    {
        // 如果窗口对象不存在
        if (WindowMgr.GetWindow("ShopWnd") == null)
            return;

        // 获取格子对象
        ShopItemWnd item = go.GetComponent<ShopItemWnd>();

        // 已经购买的商品
        if (item.mBuy)
            return;

        Property ob = item.ob;
        LPCMapping cost = new LPCMapping();

        cost = item.mCost;

        string fields = FieldsMgr.GetFieldInMapping(cost);

        if (mShopSize == GameSettingMgr.GetSettingInt("max_shop_size") && ob == null)
            return;
        else if (ob == null && go.Equals(mObjects[mShopSize]))
        {
            DialogMgr.ShowDailog(
                new CallBack(ClickDialogBtnCallBack, cost),
                string.Format(LocalizationMgr.Get("ShopWnd_6"), cost.GetValue<int>(fields), FieldsMgr.GetFieldName(fields)),
                string.Empty,
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
            return;
        }
        else if (ob != null)
        {
            if (mSelect != null && mSelect.ob == null)
                return;

            if (mSelect != null && mSelect.ob.GetRid().Equals(ob.GetRid()))
                return;

            if (mInfoWnd != null)
                WindowMgr.DestroyWindow(mInfoWnd.name);

            // 绘制物品信息窗口
            mInfoWnd = RedrawGoodsInfoWnd(ob, item);
            if (mInfoWnd == null)
                return;

            // 设置位置
            mInfoWnd.transform.localPosition = new Vector3(-290, mInfoWnd.transform.localPosition.y, 0);
        }
    }


    /// <summary>
    /// 绘制物品信息窗口
    /// </summary>
    GameObject RedrawGoodsInfoWnd(Property ob, ShopItemWnd item)
    {
        if (mSelect != null && mSelect.ob.GetRid().Equals(ob.GetRid()))
            return mInfoWnd;

        int classId = ob.GetClassID();

        RewardItemInfoWnd script = null;

        GameObject wnd = null;

        if (!MonsterMgr.IsMonster(classId))
        {
            // 打开窗口
            wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

            if (wnd == null)
            {
                LogMgr.Trace("RewardItemInfoWnd窗口创建失败");
                return wnd;
            }

            wnd.transform.localPosition = new Vector3(-290,
                wnd.transform.localPosition.y, wnd.transform.localPosition.z);

            script = wnd.GetComponent<RewardItemInfoWnd>();

            if (script == null)
            {
                LogMgr.Trace("获取脚本对象失败");
                return wnd;
            }

            script.SetCallBack(new CallBack(OnDialogCallBack, ob));
        }

        if (MonsterMgr.IsMonster(classId))
        {
            // 宠物
            wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

            if (wnd == null)
            {
                LogMgr.Trace("PetSimpleInfoWnd窗口创建失败");
                return wnd;
            }
            PetSimpleInfoWnd petScript = wnd.GetComponent<PetSimpleInfoWnd>();

            petScript.ShowBtn(false);
            petScript.Bind(ob, false, false, false);

            petScript.SetCallBack(new CallBack(OnDialogCallBack, ob));
        }
        else if (EquipMgr.IsEquipment(classId))
        {
            script.SetCallBack(new CallBack(OnDialogCallBack, ob));
            // 装备
            script.SetEquipData(ob, false, true, string.Empty, false, false);
        }
        else
        {
            // 道具
            script.SetPropData(ob, false, true, string.Empty, false, false);
            script.SetCallBack(new CallBack(OnDialogCallBack, ob));
        }

        item.mBg.spriteName = "summonSelectBg";

        if (mSelect != null)
            mSelect.mBg.spriteName = "summonNoSelectBg";

        mSelect = item;

        return wnd;
    }

    void BuyItem(Property ob)
    {
        int freePos = 0;

        Container container = ME.user as Container;

        if (MonsterMgr.IsMonster(ob))
        {
            freePos = container.baggage.GetFreePosCount(ContainerConfig.POS_PET_GROUP);
        }
        else if (EquipMgr.IsEquipment(ob))
        {
            freePos = container.baggage.GetFreePosCount(ContainerConfig.POS_ITEM_GROUP);
        }
        else
        {
            // 道具不作处理
            freePos = 1;
        }

        if (freePos <= 0)
        {
            // 包裹空间不足无法购买
            DialogMgr.Notify(LocalizationMgr.Get("MarketWnd_23"));
            return;
        }

        ShopMgr.ShopBuy(ob);
    }

    void OnDialogCallBack(object para, params object[] param)
    {
        if (! (bool)param[0])
        {
            mSelect.mBg.spriteName = "summonNoSelectBg";

            mSelect = null;
            return;
        }
        BuyItem(para as Property);
    }

    /// <summary>
    /// 购买列表格子回调
    /// </summary>
    void ClickDialogBtnCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        LPCMapping cost = para as LPCMapping;

        string fields = FieldsMgr.GetFieldInMapping(cost);

        if (ME.user.Query<int>(fields) < cost.GetValue<int>(fields))
        {
            // 消耗不足
            DialogMgr.Notify(string.Format(LocalizationMgr.Get("ShopWnd_8"), FieldsMgr.GetFieldName(fields)));

            return;
        }

        // 通知服务器购买格子
        Operation.CmdUpgradeShop.Go();
    }

    public void DoClose()
    {
        // 打开主界面
        WindowMgr.OpenWnd(MainWnd.WndType);

        // 关闭窗口
        if (this != null)
            WindowMgr.DestroyWindow(gameObject.name);

        SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();

        if (control != null)
            control.MoveCamera(SceneMgr.SceneCamera.transform.localPosition, SceneMgr.SceneCameraFromPos);

        if (mInfoWnd != null)
            WindowMgr.DestroyWindow(mInfoWnd.name);
    }
}
