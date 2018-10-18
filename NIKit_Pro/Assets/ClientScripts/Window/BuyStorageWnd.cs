/// <summary>
/// BuyStorageWnd.cs
/// Created by lic 2016-6-20
/// 购买包裹/仓库格子界面
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class BuyStorageWnd : MonoBehaviour
{
    #region 成员变量

    public UILabel mTitle;
    public GameObject mCloseBtn;
    public UILabel mAmountDesc;

    public UILabel mMoneyDesc;
    public GameObject mMoneyBtn;
    public UILabel mMoneyBtnLb;
    public UILabel mMoneyBtnNum;

    public UILabel mGoldCoinDesc;
    public GameObject mGoldCoinBtn;
    public UILabel mGoldCoinBtnLb;
    public UILabel mGoldCoinBtnNum;

    public GameObject mMask;

    public TweenScale mTweenScale;

    public TweenAlpha mTweenAlpha;

    #endregion

    #region 私有变量

    private int mType;
    private LPCMapping mCost = new LPCMapping();

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 初始化本地化文本;
        InitLocalText();

        // 注册方法
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

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;
        UIEventListener.Get(mMask).onClick = OnCloseBtn;
        UIEventListener.Get(mMoneyBtn).onClick = OnMoneyBtn;
        UIEventListener.Get(mGoldCoinBtn).onClick = OnGoldCoinBtn;

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);
    }

    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        int addAmount = 0;

        int petAmount = BaggageMgr.GetItemsByPage(ME.user, mType).Count;
        int size = ME.user.baggage.ContainerSize[mType].AsInt;

        switch (mType)
        {
            case ContainerConfig.POS_PET_GROUP:
                mCost = CACL_UPGRADE_BAGGAGE_COST.CALL(ME.user);
                mTitle.text = string.Format(LocalizationMgr.Get("BuyStorageWnd_3"), LocalizationMgr.Get("BuyStorageWnd_7"));
                mAmountDesc.text = string.Format(LocalizationMgr.Get("BuyStorageWnd_4"), LocalizationMgr.Get("BuyStorageWnd_7"), petAmount, size);
                addAmount = GameSettingMgr.GetSettingInt("upgrade_pet_baggage_add_size");
                break;
            case ContainerConfig.POS_STORE_GROUP:
                mCost = CACL_UPGRADE_STORE_COST.CALL(ME.user);
                mTitle.text = string.Format(LocalizationMgr.Get("BuyStorageWnd_3"), LocalizationMgr.Get("BuyStorageWnd_8"));
                mAmountDesc.text = string.Format(LocalizationMgr.Get("BuyStorageWnd_4"), LocalizationMgr.Get("BuyStorageWnd_8"), petAmount, size);
                addAmount = GameSettingMgr.GetSettingInt("upgrade_store_baggage_add_size");
                break;
            default :
                break;
        }

        mMoneyBtnLb.text = string.Format(LocalizationMgr.Get("BuyStorageWnd_5"), addAmount);
        mGoldCoinBtnLb.text = string.Format(LocalizationMgr.Get("BuyStorageWnd_5"), addAmount);

        mMoneyBtnNum.text = mCost.GetValue<int>("money").ToString();
        mGoldCoinBtnNum.text = mCost.GetValue<int>("gold_coin").ToString();
    }

    /// <summary>
    /// 窗口关闭按钮点击事件
    /// </summary>
    void OnCloseBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 选项1点击
    /// </summary>
    void OnMoneyBtn(GameObject go)
    {
        // 检查金钱是否足够
        if(mCost.GetValue<int>("money") > ME.user.Query<int>("money"))
        {
            DialogMgr.ShowDailog(
                new CallBack(GotoShop, ShopConfig.MONEY_GROUP),
                string.Format(LocalizationMgr.Get("SummonWnd_19"), FieldsMgr.GetFieldName("money")),
                string.Empty,
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
            return;
        }

        if(BaggageMgr.CheckCanUpgradeBaggage(ME.user, mType))
            Operation.CmdUpgradeBaggage.Go(ShopConfig.BUY_TYPE_MONEY, mType);
        else
            DialogMgr.Notify(string.Format(LocalizationMgr.Get("BuyStorageWnd_6"), 
                (mType == ContainerConfig.POS_PET_GROUP) ? LocalizationMgr.Get("BuyStorageWnd_7") : LocalizationMgr.Get("BuyStorageWnd_8")));

        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 选项2点击
    /// </summary>
    void OnGoldCoinBtn(GameObject go)
    {
        // 检查钻石是否足够
        // 检查金钱是否足够
        if(mCost.GetValue<int>("gold_coin") > ME.user.Query<int>("gold_coin"))
        {
            DialogMgr.ShowDailog(
                new CallBack(GotoShop, ShopConfig.GOLD_COIN_GROUP),
                string.Format(LocalizationMgr.Get("SummonWnd_19"), FieldsMgr.GetFieldName("gold_coin")),
                string.Empty,
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
            return;
        }

        if(BaggageMgr.CheckCanUpgradeBaggage(ME.user, mType))
            Operation.CmdUpgradeBaggage.Go(ShopConfig.BUY_TYPE_GOLD_COIN, mType);
        else
            DialogMgr.Notify(string.Format(LocalizationMgr.Get("BuyStorageWnd_6"), 
                (mType == ContainerConfig.POS_PET_GROUP) ? LocalizationMgr.Get("BuyStorageWnd_7") : LocalizationMgr.Get("BuyStorageWnd_8")));

        WindowMgr.DestroyWindow(gameObject.name); 
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLocalText()
    {
        mMoneyDesc.text = LocalizationMgr.Get("BuyStorageWnd_1");
        mGoldCoinDesc.text = LocalizationMgr.Get("BuyStorageWnd_2");
    }

    /// <summary>
    /// 前往商店
    /// </summary>
    void GotoShop(object para, params object[] _params)
    {
        if (!(bool)_params[0])
            return;

        // 通关兰达平原普通所有副本
        if (! GuideMgr.IsGuided(4))
        {
            DialogMgr.Notify(LocalizationMgr.Get("GuideWnd_1"));

            return;
        }

        // 前往商店
        GameObject wnd = WindowMgr.OpenWnd(QuickMarketWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;
        wnd.GetComponent<QuickMarketWnd>().Bind(para as string);
    }

    #region 私有变量

    public void BindData(int type)
    {   
        // 检测type合法性
        if ((type != ContainerConfig.POS_PET_GROUP) && (type != ContainerConfig.POS_STORE_GROUP))
            return;

        mType = type;

        Redraw();
    }

    #endregion
}
