/// <summary>
/// ArenaRewarItemWnd.cs
/// Created by fengsc 2016/09/24
/// 竞技场排名奖励物品格子
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class RankRewardGoodsItemWnd : WindowBase<RankRewardGoodsItemWnd>
{

    #region 成员变量

    [HideInInspector]
    public Property itemOb;

    LPCMapping mRewardData;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();
    }
    void RegisterEvent()
    {
        UIEventListener.Get(gameObject).onClick = OnClickGrid;
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        if (itemOb != null)
            itemOb.Destroy();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        UITexture icon = transform.Find("icon").GetComponent<UITexture>();

        UILabel amountLb = transform.Find("amount").GetComponent<UILabel>();

        // 加载icon的路径
        int amount = 0;
        //星级
        int star = 0;
        int goodsRank = 0;

        if (mRewardData.ContainsKey("class_id"))
        {
            int class_id = mRewardData.GetValue<int>("class_id");

            // 构建参数
            LPCMapping dbase = new LPCMapping ();
            dbase.Add("class_id", class_id);
            dbase.Add("rid", Rid.New());

            if (itemOb != null)
                itemOb.Destroy();

            // 创建对象
            itemOb = PropertyMgr.CreateProperty(dbase);

            // 获取对象的star和rank属性
            star = itemOb.Query<int>("star");
            goodsRank = itemOb.Query<int>("rank");

            if (PropertyMgr.GetPropertyType(class_id) == ObjectType.OBJECT_TYPE_MONSTER)
                icon.mainTexture = MonsterMgr.GetTexture(class_id, goodsRank);

            if (PropertyMgr.GetPropertyType(class_id) == ObjectType.OBJECT_TYPE_ITEM)
                icon.mainTexture = ItemMgr.GetTexture(class_id);

            if (PropertyMgr.GetPropertyType(class_id) == ObjectType.OBJECT_TYPE_EQUIP)
                icon.mainTexture = EquipMgr.GetTexture(class_id, itemOb.GetRarity());

            amountLb.gameObject.SetActive(false);
        }
        else
        {
            amountLb.gameObject.SetActive(true);

            string fields = FieldsMgr.GetFieldInMapping(mRewardData);
            icon.mainTexture = ItemMgr.GetTexture(FieldsMgr.GetFieldTexture(fields));

            amount = mRewardData.GetValue<int>(fields);

            amountLb.text = string.Format("{0}{1}", LocalizationMgr.Get("RankingBattleWnd_5"), amount);
        }

        Transform stars = transform.Find("stars");

        for (int i = 0; i < star; i++)
            stars.GetChild(i).GetComponent<UISprite>().spriteName = PetMgr.GetStarName(goodsRank);
        for (int j = star; j < stars.childCount; j++)
            stars.GetChild(j).gameObject.SetActive(false);
    }

    /// <summary>
    /// 格子点击事件
    /// </summary>
    void OnClickGrid(GameObject go)
    {
        if (itemOb == null)
            return;

        if (MonsterMgr.IsMonster(itemOb))
        {
            if (itemOb == null)
                return;

            // 创建窗口
            GameObject wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

            // 创建窗口失败
            if (wnd == null)
                return;

            PetSimpleInfoWnd script = wnd.GetComponent<PetSimpleInfoWnd>();

            if (script == null)
                return;

            script.Bind(itemOb);
            script.ShowBtn(true);
        }
        else if (EquipMgr.IsEquipment(itemOb) || ItemMgr.IsItem(itemOb))
        {
            if (itemOb == null)
                return;

            // 创建窗口
            GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (wnd == null)
                return;

            wnd.GetComponent<RewardItemInfoWnd>().SetPropData(itemOb, true);
        }
        else
        {
        }
    }

    #region 外部接口

    public void Bind(LPCMapping arr)
    {
        mRewardData = arr;

        // 绘制窗口
        Redraw();
    }

    #endregion
}
