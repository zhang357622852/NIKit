/// <summary>
/// ShopItemWnd.cs
/// Created by fengsc 2016/11/21
/// 市集列表格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ShopItemWnd : WindowBase<ShopItemWnd>
{
    #region 成员变量

    // 商品名称
    public UILabel mName;

    // 商品价格
    public UILabel mPrice;

    // 货币图标
    public UISprite mCoinIcon;

    // 格子背景
    public UISprite mBg;

    // 星级
    public UISprite[] mStars;

    // 商品图标
    public UITexture mIcon;

    // 套装图标
    public UITexture mSuitIcon;

    // 装备等级
    public UILabel mLevel;

    // 已购买
    public UILabel mIsBuy;

    // 商品数据
    LPCValue mData = new LPCValue();

    // 商品对象
    [HideInInspector]
    public Property ob;

    [HideInInspector]
    public LPCMapping mCost;

    [HideInInspector]
    public bool mBuy = false;

    int mShopSize = 0;

    #endregion

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        if (ob != null)
            ob.Destroy();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 初始化图标显示
        foreach (UISprite sprite in mStars)
            sprite.gameObject.SetActive(false);

        mSuitIcon.gameObject.SetActive(false);
        mLevel.gameObject.SetActive(false);
        mIsBuy.gameObject.SetActive(false);

        mPrice.gameObject.SetActive(true);

        mName.gameObject.SetActive(true);

        mCoinIcon.gameObject.SetActive(true);

        mIcon.gameObject.SetActive(true);

        float rgb = 255f / 255;

        mBg.color = new Color(rgb, rgb, rgb);

        string starName = string.Empty;

        string resPath = string.Empty;

        if (mData == null)
        {
            resPath = string.Format("Assets/Art/UI/Icon/monster/addpet.png");

            mName.text = LocalizationMgr.Get("ShopWnd_4");

            if (mShopSize < 0)
                mShopSize = 0;

            // 计算购买格子的价格
            mCost = CALC_UNLOCK_SHOP_ITEM_COST.Call(mShopSize);

            mBuy = false;
        }
        else if (mData.IsInt)
        {
            mIcon.gameObject.SetActive(false);

            mIsBuy.text = LocalizationMgr.Get("ShopWnd_5");

            mIsBuy.gameObject.SetActive(true);

            mPrice.gameObject.SetActive(false);

            mName.gameObject.SetActive(false);

            mCoinIcon.gameObject.SetActive(false);

            rgb = 120f / 255;

            mBg.color = new Color(rgb, rgb, rgb);

            mBuy = true;
            return;
        }
        else
        {
            mBuy = false;
            LPCMapping data = mData.AsMapping;

            int classId = data.GetValue<int>("class_id");

            if (ob != null)
                ob.Destroy();

            LPCMapping dbase = LPCValue.Duplicate(data).AsMapping;

            dbase.Add("rid", Rid.New());
            dbase.Add("org_rid", data.GetValue<string>("rid"));

            // 克隆商品对象
            ob = PropertyMgr.CreateProperty(dbase, true);

            mCost = PropertyMgr.GetBuyPrice(ob);

            // 星级
            int star = 0;
            if (data.ContainsKey("star"))
            {
                star = data.GetValue<int>("star");
            }

            CsvRow row = null;

            string icon = string.Empty;
            if (MonsterMgr.IsMonster(ob))
            {
                row = MonsterMgr.GetRow(classId);
                if (row != null && star == 0)
                    star = row.Query<int>("star");

                starName = PetMgr.GetStarName(ob.Query<int>("rank"));

                // 获取宠物的名称
                mName.text = LocalizationMgr.Get(MonsterMgr.GetRow(classId).Query<string>("name"));

                icon = MonsterMgr.GetIcon(classId, ob.Query<int>("rank"));
                resPath = string.Format("Assets/Art/UI/Icon/monster/{0}.png", icon);
            }
            else if (ItemMgr.IsItem(ob))
            {
                row = ItemMgr.GetRow(classId);

                icon = row.Query<string>("icon");

                if (string.IsNullOrEmpty(icon))
                    icon = ItemMgr.GetIcon(classId);

                resPath = string.Format("Assets/Art/UI/Icon/item/{0}.png", icon);

                mName.text = LocalizationMgr.Get(ItemMgr.GetRow(classId).Query<string>("name"));
            }
            else
            {
                icon = EquipMgr.GetIcon(classId, ob.GetRarity());
                resPath = string.Format("Assets/Art/UI/Icon/equipment/{0}.png", icon);

                mName.text = ob.Short();

                row = EquipMgr.GetRow(classId);

                if (row != null && star == 0)
                    star = row.Query<int>("star");

                starName = "gold_start";

                // 套装类型图标
                mSuitIcon.mainTexture = EquipMgr.GetSuitTexture(ob.Query<int>("suit_id"));
                mSuitIcon.gameObject.SetActive(true);

                // 装备的强化等级
                int rank = data.GetValue<int>("rank");
                if (rank > 0)
                {
                    mLevel.text = "+" + rank;
                    mLevel.gameObject.SetActive(true);
                }
            }

            for (int i = 0; i < star; i++)
            {
                mStars[i].gameObject.SetActive(true);

                mStars[i].spriteName = starName;
            }

            for (int i = star; i < mStars.Length; i++)
                mStars[i].gameObject.SetActive(false);
        }

        mIcon.mainTexture = ResourceMgr.LoadTexture(resPath);

        if (mCost == null || mCost.Count < 1)
            return;
        string fields = FieldsMgr.GetFieldInMapping(mCost);

        // 出售价格
        mPrice.text = Game.SetMoneyShowFormat(mCost.GetValue<int>(fields));

        // 货币图标
        mCoinIcon.spriteName = FieldsMgr.GetFieldIcon(fields);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCValue data, int shopSize)
    {
        mData = data;

        mShopSize = shopSize;

        // 绘制窗口
        Redraw();

        mBg.spriteName = "summonNoSelectBg";
    }
}
