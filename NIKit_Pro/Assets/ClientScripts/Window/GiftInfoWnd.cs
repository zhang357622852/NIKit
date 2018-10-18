/// <summary>
/// GiftInfoWnd.cs
/// Created fengsc 2017/04/21
/// 礼包信息弹框
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class GiftInfoWnd : WindowBase<GiftInfoWnd>
{
    #region

    // 窗口标题
    public UILabel mTitle;

    // 道具图标
    public UITexture mIcon;

    // 道具的描述信息
    public UILabel mDesc;

    // 礼包物品基础格子
    public GameObject mItem;

    // 排序组件
    public UIGrid mGrid;

    /// <summary>
    /// 购买按钮
    /// </summary>
    public GameObject mBuyBtn;
    public UILabel mBuyBtnLb;

    // 消耗图标
    public UISprite mCostIcon;

    // 消耗
    public UILabel mCost;

    // 取消按钮
    public GameObject mCancelBtn;
    public UILabel mCancelBtnLb;

    public GameObject mMask;

    public TweenAlpha mTweenAlpha;

    public TweenScale mTweenScale;

    Property mItemOb;

    // 商城配置数据
    CsvRow mMarketData;

    #endregion

    // Use this for initialization
    void Start ()
    {
        InitLocalText();

        RegisterEvent();

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
        EventMgr.UnregisterEvent("GiftInfoWnd");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mBuyBtn).onClick = OnClickBuyBtn;
        UIEventListener.Get(mCancelBtn).onClick = OnClickCancelBtn;
        UIEventListener.Get(mMask).onClick = OnClickCancelBtn;

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

        // 注册购买成功事件
        EventMgr.RegisterEvent("GiftInfoWnd", EventMgrEventType.EVENT_BUY_ITEM_SUCCESS, OnBuyItemSuccCallBack);
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
    /// tween动画播放完后回调
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
        mTitle.text = LocalizationMgr.Get("GiftInfoWnd_1");
        mBuyBtnLb.text = LocalizationMgr.Get("GiftInfoWnd_2");
        mCancelBtnLb.text = LocalizationMgr.Get("GiftInfoWnd_3");
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mItem.SetActive(false);

        if (mItemOb == null)
            return;

        int classId = mItemOb.GetClassID();

        // 道具的描述信息
        mDesc.text = ItemMgr.GetDesc(ME.user, classId);

        mIcon.mainTexture = ItemMgr.GetTexture(classId);

        LPCMapping cost = PropertyMgr.GetBuyPrice(mItemOb, true);

        string costFields = FieldsMgr.GetFieldInMapping(cost);

        mCostIcon.spriteName = FieldsMgr.GetFieldIcon(costFields);

        mCost.text = cost.GetValue<int>(costFields).ToString();

        // 道具配置信息
        CsvRow itemRow = ItemMgr.GetRow(classId);
        if (itemRow == null)
            return;

        // 获取道具的附加属性
        LPCMapping dbase = itemRow.Query<LPCMapping>("apply_arg");
        if (dbase == null || !dbase.ContainsKey("goods_list"))
            return;

        // 礼包获得的物品列表
        LPCArray goodsList = dbase.GetValue<LPCArray>("goods_list");
        if (goodsList == null)
            return;

        Property ob = null;
        foreach (LPCValue item in goodsList.Values)
        {
            if (item == null || ! item.IsMapping)
                continue;

            GameObject clone = Instantiate(mItem);

            clone.transform.SetParent(mGrid.transform);
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localScale = Vector3.one;

            Transform iconGo = clone.transform.Find("Icon");
            if (iconGo == null)
                return;

            UITexture icon = iconGo.GetComponent<UITexture>();
            if (icon == null)
                return;

            Transform nameGo = clone.transform.Find("name");
            if (nameGo == null)
                return;

            UILabel name = nameGo.GetComponent<UILabel>();

            LPCMapping data = item.AsMapping;

            if (data.ContainsKey("class_id"))
            {
                LPCMapping para = LPCMapping.Empty;
                para.Add("rid", Rid.New());
                para.Add("class_id", data.GetValue<int>("class_id"));
                
                ob = PropertyMgr.CreateProperty(para);

                if (MonsterMgr.IsMonster(ob))
                {
                    icon.mainTexture = MonsterMgr.GetTexture(ob.GetClassID(), ob.GetRank());
                }
                else if (ItemMgr.IsItem(ob))
                {
                    icon.mainTexture = ItemMgr.GetTexture(ob.GetClassID());
                }
                else
                {
                    icon.mainTexture = EquipMgr.GetTexture(ob.GetClassID(), ob.GetRarity());
                }

                name.text = string.Format("{0}×{1}", ob.Short(), data.GetValue<int>("amount"));

            }
            else
            {
                string fields = FieldsMgr.GetFieldInMapping(data);

                int fieldsClassId = FieldsMgr.GetClassIdByAttrib(fields);

                icon.mainTexture = ItemMgr.GetTexture(fieldsClassId, true);

                name.text = string.Format("{0} + {1}", FieldsMgr.GetFieldName(fields), data.GetValue<int>(fields));
            }

            clone.SetActive(true);

            // 析构物件对象
            if(ob != null)
                ob.Destroy();
        }

        // 激活排序组件
        mGrid.Reposition();
    }

    /// <summary>
    /// 购买按钮点击事件
    /// </summary>
    void OnClickBuyBtn(GameObject go)
    {
        if (mItemOb == null)
            return;

        // 是否可以购买
        object ret = MarketMgr.IsBuy(ME.user, mMarketData.Query<int>("class_id"));

        // 无法购买
        if (ret is string)
        {
            DialogMgr.Notify((string) ret);

            return;
        }

        LPCMapping costMap = PropertyMgr.GetBuyPrice(mItemOb, true);

        string field = FieldsMgr.GetFieldInMapping(costMap);

        string desc = string.Empty;

        if (string.IsNullOrEmpty(mMarketData.Query<string>("purchase_id")))
        {
            desc = string.Format(LocalizationMgr.Get("QuickMarketWnd_2"), costMap.GetValue<int>(field), FieldsMgr.GetFieldName(field));
        }
        else
        {
            desc = string.Format(LocalizationMgr.Get("QuickMarketWnd_4"), MarketMgr.GetSkuPrice(mMarketData.Query<int>("class_id")));
        }

        LPCMapping data = MarketMgr.GetMarketDataByPos(ME.user, mMarketData.Query<string>("pos"));
        if (data == null)
            return;

        DialogMgr.ShowDailog(
            new CallBack(OnDailogCallBack, data),
            desc,
            string.Empty,
            string.Empty,
            string.Empty,
            true,
            this.transform
        );
    }

    void OnDailogCallBack(object para, params object[] param)
    {
        if (!(bool) param[0])
            return;

        LPCMapping data = para as LPCMapping;

        // 购买商品
        MarketMgr.Buy(ME.user, data, 1);
    }

    /// <summary>
    /// 取消按钮点击时间
    /// </summary>
    void OnClickCancelBtn(GameObject go)
    {
        if (this == null)
            return;

        // 销毁当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(Property itemOb)
    {
        if (itemOb == null)
            return;

        mItemOb = itemOb;

        // 商城配置数据
        mMarketData = MarketMgr.GetMarketConfig(mItemOb.GetClassID());

        // 绘制窗口
        Redraw();
    }
}
