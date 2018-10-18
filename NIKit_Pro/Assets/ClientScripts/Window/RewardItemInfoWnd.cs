/// <summary>
/// RewardEquipInfoWnd.cs
/// Created by fengsc 2016/07/29
///战斗结算奖励装备信息界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class RewardItemInfoWnd : WindowBase<RewardItemInfoWnd>
{

    #region 成员变量

    /// <summary>
    /// 关闭信息界面按钮
    /// </summary>
    public GameObject mCloseBtn;

    /// <summary>
    /// 装备名称
    /// </summary>
    public UILabel mEquipName;

    public EquipItemWnd mEquipItemWnd;

    /// <summary>
    /// 套装描述
    /// </summary>
    public UILabel mSuitDesc;

    /// <summary>
    /// 装备出售按钮
    /// </summary>
    public GameObject mSellBtn;
    public UILabel mSellBtnLb;

    /// <summary>
    /// 装备出售价格
    /// </summary>
    public UILabel mSellPrice;

    // $12
    public UILabel mBuyRmbCost;

    public UISprite mSellIcon;

    /// <summary>
    /// 获取道具按钮
    /// </summary>
    public GameObject mGetPropBtn;
    public UILabel mGetPropBtnLb;

    /// <summary>
    /// 主属性加成
    /// </summary>
    public UILabel mMainAttriPlus;

    /// <summary>
    /// 词缀属性
    /// </summary>
    public UILabel mPrefixProp;

    /// <summary>
    /// 附加属性
    /// </summary>
    public UILabel mAddAttri;

    /// <summary>
    /// 排序组件
    /// </summary>
    public UIGrid mGrid;

    public GameObject mEquipPanel;

    public GameObject mPropPanel;

    public UITexture mItemIcon;

    public UILabel mItemName;

    public UILabel mItemDesc;

    public UITexture mItemSuitIcon;

    public UISprite mMask;

    public UILabel mSuitAddDesc;

    public GameObject mSoulAmount;

    public GameObject[] mSoul;

    public UISprite mLine;

    public TweenScale mTweenScale;

    public TweenAlpha mTweenAlpha;

    private bool mIsShowMask = false;

    private bool mIsMarket = false;

    /// <summary>
    /// 装备
    /// </summary>
    private Property mProperty;

    CallBack mCallBack;

    GameObject mWnd;

    /// <summary>
    /// The m cooke.
    /// </summary>
    string mCooke = string.Empty;

    #endregion

    #region 内部函数

    void Start()
    {
        mWnd = this.gameObject;
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mGetPropBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mMask.gameObject).onClick = OnClickCloseBtn;

        // 关注msg_sell_equip消息
        MsgMgr.RegisterDoneHook("MSG_SELL_ITEM", "RewardItemInfoWnd", OnMsgSellEquip);

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnFinish);

        // 注册购买成功事件
        EventMgr.RegisterEvent("RewardItemInfoWnd", EventMgrEventType.EVENT_BUY_ITEM_SUCCESS, OnBuyItemSuccCallBack);
    }

    /// <summary>
    /// 道具购买成功回调
    /// </summary>
    void OnBuyItemSuccCallBack(int eventId, MixedValue para)
    {
        if (ME.user == null)
            return;

        LPCMapping itemData = para.GetValue<LPCMapping>().GetValue<LPCMapping>("item_data");

        int classId = itemData.GetValue<int>("class_id");

        CsvRow data = MarketMgr.GetMarketConfig(classId);
        if (data == null)
            return;

        int showDialog = data.Query<int>("show_dialog");

        if (showDialog != 1)
        {
            // 关闭当前窗口
            if (this != null)
                WindowMgr.DestroyWindow(gameObject.name);

            DialogMgr.Notify(LocalizationMgr.Get(data.Query<string>("buy_tips")));
        }
        else
        {
            DialogMgr.ShowSimpleSingleBtnDailog(
                new CallBack(OnDialogCallBack),
                LocalizationMgr.Get(data.Query<string>("buy_tips")),
                LocalizationMgr.Get("MarketWnd_14"),
                string.Empty
            );
        }
    }

    void OnDialogCallBack(object para, params object[] param)
    {
        if (this == null)
            return;

        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// tween动画播放完成回调
    /// </summary>
    void OnFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 播放tween动画
    /// </summary>
    void PlayTweenAnima()
    {
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
        Coroutine.DispatchService(FixedRemove());

        EventMgr.UnregisterEvent("RewardItemInfoWnd");
    }

    IEnumerator FixedRemove()
    {
        yield return null;

        MsgMgr.RemoveDoneHook("MSG_SELL_ITEM", "RewardItemInfoWnd");

        EventMgr.UnregisterEvent("RewardItemInfoWnd");
    }

    /// <summary>
    /// 装备出售成功的回调
    /// </summary>
    void OnMsgSellEquip(string cmd, LPCValue para)
    {
        if (mWnd == null)
            return;

        // 销毁窗口对象
        WindowMgr.DestroyWindow(mWnd.name);
    }

    /// <summary>
    /// 绘制装备信息窗口
    /// </summary>
    void RedarwEquipWnd()
    {
        mEquipPanel.SetActive(true);
        mPropPanel.SetActive(false);

        mSellPrice.text = string.Empty;

        mEquipName.text = string.Empty;

        mGetPropBtn.transform .localPosition = new Vector3(119, -161, 0);

        if(mProperty == null)
            return;

        // 绑定数据
        mEquipItemWnd.SetBind(mProperty);


        //获取装备的套装ID;
        int suitId = mProperty.Query<int>("suit_id");

        //获取套装配置表;
        CsvFile suitCsv = EquipMgr.SuitTemplateCsv;

        //根据套装id获取这一行的配置信息;
        CsvRow row = suitCsv.FindByKey(suitId);

        if(row != null)
        {

            mSuitDesc.gameObject.SetActive(true);
            mSuitDesc.text = LocalizationMgr.Get(row.Query<string>("desc"));
        }

        // 获取装备的稀有度
        int rarity = mProperty.GetRarity();

        //获取装备的属性;
        LPCMapping equipMap = mProperty.Query<LPCMapping>("prop");

        //获取装备的主属性数据;
        LPCArray mainProp = equipMap.GetValue<LPCArray>(EquipConst.MAIN_PROP);

        //获取装备的词缀属性;
        LPCArray prefixProp = equipMap.GetValue<LPCArray>(EquipConst.PREFIX_PROP);

        //获取次要属性;
        LPCArray minorProp = equipMap.GetValue<LPCArray>(EquipConst.MINOR_PROP);

        string mainDesc = string.Empty;

        if(mainProp != null)
        {
            foreach (LPCValue item in mainProp.Values)
                mainDesc += PropMgr.GetPropDesc(item.AsArray, EquipConst.MAIN_PROP);

            //主属性加成;
            mMainAttriPlus.text = mainDesc;
        }
        else
            mMainAttriPlus.text = string.Empty;

        string prefixPropValue = string.Empty;

        //装备短描述
        string shortDesc = mProperty.Short();

        //有词缀属性
        if(prefixProp != null)
        {
            foreach (LPCValue item in prefixProp.Values)
                prefixPropValue += PropMgr.GetPropDesc(item.AsArray, EquipConst.PREFIX_PROP);

            //词缀属性;
            mPrefixProp.text = prefixPropValue;

            mEquipName.text = string.Format("[{0}]{1}[-]", ColorConfig.GetColor(rarity), shortDesc);
        }
        else
        {
            mEquipName.text = string.Format("[{0}]{1}[-]", ColorConfig.GetColor(rarity), shortDesc);
            mPrefixProp.text = string.Empty;
        }

        //没有附加属性;
        if(minorProp == null || minorProp.Count < 1)
        {
            mAddAttri.gameObject.SetActive(false);
            return;
        }

        mAddAttri.gameObject.SetActive(false);

        foreach (LPCValue item in minorProp.Values)
        {
            GameObject clone = Instantiate(mAddAttri.gameObject) as GameObject;

            clone.gameObject.SetActive(true);

            clone.transform.SetParent(mGrid.transform);

            clone.transform.localScale = Vector3.one;

            clone.transform.localPosition = Vector3.zero;

            UILabel label = clone.GetComponent<UILabel>();

            label.text = PropMgr.GetPropDesc(item.AsArray, EquipConst.MINOR_PROP);
        }

        mGrid.repositionNow = true;
    }

    /// <summary>
    /// 绘制道具信息窗口
    /// </summary>
    void RedarwPropWnd()
    {
        mEquipPanel.SetActive(false);
        mPropPanel.SetActive(true);
        mSuitAddDesc.gameObject.SetActive(false);

        mSoulAmount.SetActive(false);

        // 隐藏套装角标
        mItemSuitIcon.gameObject.SetActive(false);

        int classId = mProperty.GetClassID();
        string resPath = string.Empty;

        int petId = mProperty.Query<int>("pet_id");
        if (petId != 0)
            resPath = string.Format("Assets/Art/UI/Icon/monster/{0}.png", MonsterMgr.GetIcon(petId, MonsterMgr.GetDefaultRank(petId)));
        else
            resPath = string.Format("Assets/Art/UI/Icon/item/{0}.png", ItemMgr.GetIcon(classId));

        mItemIcon.mainTexture = ResourceMgr.LoadTexture(resPath);

        mItemName.text = string.Format("{0}x{1}", ItemMgr.GetName(classId), mProperty.GetAmount());

        mItemDesc.text = ItemMgr.GetDesc(ME.user, classId);

        // 获取道具的配置信息
        CsvRow row = ItemMgr.GetRow(mProperty.GetClassID());
        if (row == null)
            return;

        // 道具分组
        string group = row.Query<string>("group");
        if (string.IsNullOrEmpty(group))
            return;

        if (! ItemConst.SoulMap.Contains(group))
            return;

        List<int> itemList = ItemMgr.GetClassIdByGroup(group);
        if (itemList == null)
            return;

        float totalWidth = 0f;

        float offsetX = 0f;

        for (int i = 0; i < mSoul.Length; i++)
        {
            int id = itemList[i];

            // 属性道具字段
            string fields = FieldsMgr.GetAttribByClassId(id);

            // 道具数量
            int amount = ME.user.Query<int>(fields);

            // 道具icon的路径
            string path = string.Format("Assets/Art/UI/Icon/item/{0}.png", ItemMgr.GetClearIcon(id));

            GameObject go = mSoul[i];
            if (go == null)
                continue;

            Transform iconTrans = go.transform.Find("icon");
            if (iconTrans == null)
                continue;

            UITexture tex = iconTrans.GetComponent<UITexture>();
            if (tex == null)
                continue;

            tex.mainTexture = ResourceMgr.LoadTexture(path);

            Transform amountTrans = go.transform.Find("amount");
            if (amountTrans == null)
                continue;

            UILabel amountLb = amountTrans.GetComponent<UILabel>();
            if (amountLb == null)
                continue;

            amountLb.text = string.Format(" ×{0}", amount);

            totalWidth += tex.localSize.x;

            if (i + 1 == mSoul.Length)
                totalWidth += amountLb.localSize.x;

            if (i == 0)
                offsetX = amountLb.localSize.x / 2;
        }

        totalWidth += (100 * (mSoul.Length - 1));

        float startX = mLine.transform.localPosition.x - mLine.localSize.x / 2 + (mLine.localSize.x - totalWidth) / 2 + offsetX / 2;

        for (int i = 0; i < mSoul.Length; i++)
        {
            GameObject go = mSoul[i];
            if (go == null)
                continue;

            go.transform.localPosition = new Vector3(
                startX + i * 146,
                go.transform.localPosition.y,
                go.transform.localPosition.z);
        }

        mSoulAmount.SetActive(true);
    }

    /// <summary>
    /// 设置按钮的信息
    /// </summary>
    void SetBtnInfo(bool isSingle, bool isBuy, string single_text)
    {
        mGetPropBtn.transform .localPosition = new Vector3(120, -161, 0);

        LPCMapping priceMap = new LPCMapping();

        if (isSingle && !isBuy)
        {
            // 单个确定按钮
            mSellBtn.SetActive(false);
            mGetPropBtn.transform .localPosition = new Vector3(8, -161, 0);
            mGetPropBtnLb.text = single_text;

            UIEventListener.Get(mGetPropBtn).onClick = OnClickCloseBtn;
        }
        if (isSingle && isBuy)
        {
            // 单个确定按钮
            mSellBtn.SetActive(false);
            mGetPropBtn.transform .localPosition = new Vector3(8, -161, 0);
            mGetPropBtnLb.text = single_text;

            // 注册购买点击事件
            UIEventListener.Get(mGetPropBtn).onClick = OnClickSelect;
        }
        else if (!isSingle && isBuy)
        {
            // 购买
            mSellBtnLb.text = LocalizationMgr.Get("RewardEquipInfoWnd_5");
            mGetPropBtnLb.text = LocalizationMgr.Get("RewardEquipInfoWnd_6");

            priceMap = PropertyMgr.GetBuyPrice(mProperty, mIsMarket);

            // 注册购买点击事件
            UIEventListener.Get(mSellBtn).onClick = OnClickBuyBtn;
        }
        else if (!isSingle && !isBuy)
        {
            // 出售
            mSellBtnLb.text = LocalizationMgr.Get("RewardEquipInfoWnd_2");
            mGetPropBtnLb.text = LocalizationMgr.Get("RewardEquipInfoWnd_3");
            priceMap = PropertyMgr.GetSellPrice(mProperty);

            // 注册点击事件
            UIEventListener.Get(mSellBtn).onClick = OnClickSellBtn;
        }

        if (priceMap == null || priceMap.Count < 1)
            return;

        string sellField = FieldsMgr.GetFieldInMapping(priceMap);

        //获取装备的出售价格;
        if(mIsMarket)
        {
            CsvRow marketData = MarketMgr.GetMarketConfig(mProperty.GetClassID());

            if (marketData != null && mBuyRmbCost != null)
            {
                if (!string.IsNullOrEmpty(marketData.Query<string>("purchase_id")))
                {
                    mSellIcon.spriteName = mSellPrice.text = string.Empty;

                    // #12购买
                    mBuyRmbCost.text = MarketMgr.GetSkuPrice(marketData.Query<int>("class_id"));

                    return;
                }
            }
        }

        if (mBuyRmbCost != null)
            mBuyRmbCost.text = string.Empty;

        mSellPrice.text = Game.SetMoneyShowFormat(priceMap.GetValue<int>(sellField));

        mSellIcon.spriteName = sellField;
    }

    /// <summary>
    /// 关闭窗口按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        //关闭当前窗口;
        if (this != null)
            WindowMgr.DestroyWindow(gameObject.name);

        if (mCallBack != null)
            mCallBack.Go(false, mCooke, RewardItemInfoWnd.WndType);
    }

    void OnClickSelect(GameObject go)
    {
        // 关闭当前窗口;
        if (this != null)
            WindowMgr.DestroyWindow(gameObject.name);

        if (mCallBack != null)
            mCallBack.Go(true, mCooke, RewardItemInfoWnd.WndType);
    }

    /// <summary>
    /// 出售按钮点击事件
    /// </summary>
    void OnClickSellBtn(GameObject go)
    {
        if (mCallBack == null)
            return;

        mCallBack.Go(true, mCooke, RewardItemInfoWnd.WndType);
    }

    /// <summary>
    /// 点击购买按钮
    /// </summary>
    void OnClickBuyBtn(GameObject go)
    {
        if (mCallBack == null)
            return;

        mCallBack.Go(true, mCooke, RewardItemInfoWnd.WndType);
    }
    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="args">Arguments.</param>
    public void ShowWindow(Dictionary<string, object> args)
    {
        // 检查数据规范
        if (!args.ContainsKey("data"))
            return;

        Property data = (Property)args["data"];

        if (EquipMgr.IsEquipment(data))
            SetEquipData(data, (args.ContainsKey("is_singleBtn") ? (bool)args["is_singleBtn"] :false), (args.ContainsKey("is_buy") ? (bool)args["is_buy"] :false),
                (args.ContainsKey("single_text") ? (string)args["single_text"] :""), (args.ContainsKey("is_market") ? (bool)args["is_market"] :false), (args.ContainsKey("is_clickMask") ? (bool)args["is_clickMask"] :true));
        else
            SetPropData(data, (args.ContainsKey("is_singleBtn") ? (bool)args["is_singleBtn"] :false), (args.ContainsKey("is_buy") ? (bool)args["is_buy"] :false),
                (args.ContainsKey("single_text") ? (string)args["single_text"] :""), (args.ContainsKey("is_market") ? (bool)args["is_market"] :false), (args.ContainsKey("is_clickMask") ? (bool)args["is_clickMask"] :true));

        if (args.ContainsKey("call_back"))
            SetCallBack((CallBack)args["call_back"]);

        // 获取打开窗口的cookie
        if (args.ContainsKey("cookie"))
            mCooke = (string) args["cookie"];

        if (args.ContainsKey("is_showMask"))
            SetMask((bool)args["is_showMask"]);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void SetPropData(Property data, bool isSingleBtn = false, bool isBuy = false, string single_text = "", bool isMarket = false, bool isClickMask = true)
    {
        mIsMarket = isMarket;
        mProperty = data;

        if (data == null)
            return;

        RegisterEvent();

        PlayTweenAnima();

        RedarwPropWnd();

        SetBtnInfo(isSingleBtn, isBuy, single_text);

        if (!isClickMask)
        {
            mMask.gameObject.SetActive(false);
            return;
        }

        // 部分需求不需要mask，但是保留boxcollider
        mMask.alpha = mIsShowMask ? 0.5f : 0.01f;
    }

    public void SetEquipData(Property data, bool isSingleBtn = false, bool isBuy = false, string single_text = "", bool isMarket = false, bool isClickMask = true)
    {
        mIsMarket = isMarket;
        mProperty = data;

        if (data == null)
            return;

        // 注册事件
        RegisterEvent();

        // 播放动画
        PlayTweenAnima();

        // 绘制装备窗口
        RedarwEquipWnd();

        // 设置按钮信息
        SetBtnInfo(isSingleBtn, isBuy, single_text);

        if (!isClickMask)
        {
            mMask.gameObject.SetActive(false);
            return;
        }

        // 部分需求不需要mask，但是保留boxcollider
        mMask.alpha = mIsShowMask ? 0.5f : 0.01f;
    }

    /// <summary>
    /// 设置套装数据
    /// </summary>
    public void SetSuitData(LPCMapping data, bool isSingleBtn = false, bool isBuy = false, string single_text = "", bool isMarket = false, bool isClickMask = true)
    {
        mEquipPanel.SetActive(false);
        mPropPanel.SetActive(true);
        mSoulAmount.SetActive(false);
        mIsMarket = isMarket;

        SetBtnInfo(isSingleBtn, isBuy, single_text);

        if (data == null || data.Count < 1)
            return;

        // 注册事件
        RegisterEvent();

        // 播放动画
        PlayTweenAnima();

        // 获取套装的配置信息
        CsvRow suitRow = EquipMgr.SuitTemplateCsv.FindByKey(data.GetValue<int>("suit_id"));

        if (suitRow == null)
            return;

        // 显示套装图表
        mItemSuitIcon.mainTexture = EquipMgr.GetSuitTexture(data.GetValue<int>("suit_id"));
        mItemSuitIcon.gameObject.SetActive(true);

        string icon = "50400";
        string resPath = string.Format("Assets/Art/UI/Icon/item/{0}.png", icon);
        mItemIcon.mainTexture = ResourceMgr.LoadTexture(resPath);

        mItemName.text = string.Format("{0}{1}", LocalizationMgr.Get(suitRow.Query<string>("name")), LocalizationMgr.Get("RewardEquipInfoWnd_4"));

        mItemDesc.text = suitRow.Query<string>("use_desc");

        mSuitAddDesc.gameObject.SetActive(true);
        mSuitAddDesc.text = LocalizationMgr.Get(suitRow.Query<string>("desc"));
    }

    public void SetMask(bool isShowMask)
    {
        mMask.alpha = isShowMask ? 0.5f : 0.01f;
    }

    public void SetCallBack(CallBack callBack)
    {
        mCallBack = callBack;
    }

    /// <summary>
    /// 指引关闭窗口
    /// </summary>
    public void GuideCloseWnd()
    {
        OnClickCloseBtn(mCloseBtn);
    }

    #endregion
}
