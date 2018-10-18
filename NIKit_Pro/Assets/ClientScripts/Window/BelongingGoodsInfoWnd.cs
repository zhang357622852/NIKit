/// <summary>
/// BelongingGoodsInfoWnd.cs
/// Created fengsc by 2016/11/18
/// 邮件附加物品信息显示窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class BelongingGoodsInfoWnd : WindowBase<BelongingGoodsInfoWnd>
{

    #region 成员变量

    // 关闭按钮
    public GameObject mCloseBtn;

    // 物品对象
    public SignItemWnd mItem;

    // 标题
    public UILabel mName;

    // 获取位置
    public UILabel mGetPart;

    // 描述信息
    public UILabel mDesc;

    // 套装描述
    public UILabel mSuitDesc;

    // 选择按钮
    public GameObject mSelectBtn;
    public UILabel mSelectBtnLb;

    public UILabel mTips;

    public GameObject mMask;

    // 物品数据信息
    LPCMapping mData = new LPCMapping();

    CallBack mCallBack;

    // 附件可领取的数量
    int mReceiveAmount = 0;

    // 已经选择的数量
    int mSelectAmount = 0;

    bool mIsSelect = false;

    Property mItemOb = null;

    #endregion

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮的点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mMask).onClick = OnClickCloseBtn;
        UIEventListener.Get(mSelectBtn).onClick = OnClickSelectBtn;
    }

    void OnDestroy()
    {
        if (mItemOb != null)
            mItemOb.Destroy();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        UISprite mSelectBtnIcon = mSelectBtn.GetComponent<UISprite>();

        float full = 225f / 255;

        float zero = 0 / 255f;

        mSelectBtnLb.text = LocalizationMgr.Get("BelongingGoodsInfoWnd_4");

        mTips.gameObject.SetActive(false);

        mSelectBtn.SetActive(true);

        if (mReceiveAmount <= 1)
        {
            mSelectBtnIcon .color = new Color(full, full, full);
        }
        else
        {
            if (mSelectAmount != mReceiveAmount)
            {
                if(mIsSelect)
                {
                    mSelectBtnLb.text = LocalizationMgr.Get("BelongingGoodsInfoWnd_7");

                    mSelectBtnIcon.color = new Color(full, zero, zero);
                }
                else
                {
                    mSelectBtnIcon.color = new Color(zero, full, zero);
                }
            }
            else
            {
                if (!mIsSelect)
                {
                    mSelectBtn.SetActive(false);

                    mTips.text = LocalizationMgr.Get("BelongingGoodsInfoWnd_6");
                    mTips.gameObject.SetActive(true);
                }
                else
                {
                    mSelectBtnLb.text = LocalizationMgr.Get("BelongingGoodsInfoWnd_7");

                    mSelectBtnIcon.color = new Color(full, zero, zero);
                }
            }
        }

        // 获取物品对象
        SignItemWnd item = mItem.GetComponent<SignItemWnd>();

        if (item == null)
            return;

        // 绑定数据
        item.NormalItemBind(mData, false);

        // 套装id
        int suitId = mData.GetValue<int>("suit_id");

        // 稀有度
        int rarity = mData.GetValue<int>("rarity");

        // 星级
        int star = mData.GetValue<int>("star");

        // 装备名称
        mName.text = EquipMgr.GetShortDesc(suitId, rarity, star);

        // 显示获取位置信息
        mGetPart.text = ItemMgr.GetDesc(mData.GetValue<int>("class_id"));

        // 获取套装配置信息
        CsvRow row = EquipMgr.SuitTemplateCsv.FindByKey(suitId);

        if (row == null)
            return;

        // 套装组件数量
        int subCount = row.Query<int>("sub_count");

        // 套装的附加属性
        LPCArray props = row.Query<LPCArray>("props");

        LPCMapping map = new LPCMapping();
        map.Add("class_id", mData.GetValue<int>("class_id"));
        map.Add("suit_id", suitId);
        map.Add("star", star);
        map.Add("rid", Rid.New());

        // 克隆一个道具对象
        if (mItemOb != null)
            mItemOb.Destroy();

        mItemOb = PropertyMgr.CreateProperty(map);

        mDesc.text = ItemMgr.GetApplyDesc(mItemOb);

        string suitDesc = string.Empty;

        //获取套装描述信息;
        foreach (LPCValue prop in props.Values)
            suitDesc += PropMgr.GetPropDesc(prop.AsArray);

        // 套装附加属性描述
        mSuitDesc.text = string.Format(LocalizationMgr.Get("BelongingGoodsInfoWnd_5"), subCount, suitDesc);
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);

        if (mCallBack == null)
            return;

        mCallBack.Go(false);
    }

    /// <summary>
    /// 选择按钮点击事件
    /// </summary>
    void OnClickSelectBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);

        if (mCallBack == null)
            return;

        mCallBack.Go(true);
    }

    /// <summary>
    /// 取消选择按钮点击事件
    /// </summary>
    void OnClickCancelSelectBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);

        if (mCallBack == null)
            return;

        mCallBack.Go(true);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data, int receiveAmount, int selectAmount, bool isSelect, CallBack callBack)
    {
        mData = data;

        if (mData == null)
            return;

        mCallBack = callBack;

        mIsSelect = isSelect;

        mReceiveAmount = receiveAmount;

        mSelectAmount = selectAmount;

        // 注册事件
        RegisterEvent();

        // 绘制窗口
        Redraw();
    }
}
