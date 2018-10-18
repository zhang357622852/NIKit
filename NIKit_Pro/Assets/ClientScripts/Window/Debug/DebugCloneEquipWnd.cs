/// <summary>
/// DebugCloneEquipWnd.cs
/// Created by fengsc 2017/05/26
/// GM 功能克隆装备
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class DebugCloneEquipWnd : WindowBase<DebugCloneEquipWnd>
{
    #region 成员变量

    // 套装下拉列表
    public UIPopupList mSuitList;

    // 装备部位（类型）下拉列表
    public UIPopupList mEquipTypeList;

    // 装备属性下拉列表
    public UIPopupList mPropList;

    // 装备品质下拉列表
    public UIPopupList mRarityList;

    // 词缀属性下拉列表
    public UIPopupList mPrefixList;

    // 星级下拉列表
    public UIPopupList mStarList;

    // 强化等级下拉列表
    public UIPopupList mRankList;

    // 克隆数量
    public UIInput mCloneNumInput;

    // 取消按钮
    public GameObject mCancelBtn;

    // 确认按钮
    public GameObject mConfirmBtn;

    Dictionary<string, int> mSuitDic = new Dictionary<string, int>();

    Dictionary<string, int> mEquipTypeDic = new Dictionary<string, int>();

    Dictionary<string, int> mPropDic = new Dictionary<string, int>();

    Dictionary<string, int> mRarityDic = new Dictionary<string, int>();

    Dictionary<string, bool> mPreficDic = new Dictionary<string, bool>();

    #endregion

    #region 内部函数

    void Start()
    {
        // 注册事件
        ReisterEvent();

        // 绘制窗口
        Redraw();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void ReisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCancelBtn).onClick = OnClickCancelBtn;
        UIEventListener.Get(mConfirmBtn).onClick = OnClickConfirmBtn;

        // 监听控件value变化
        EventDelegate.Add(mEquipTypeList.onChange, OnEquipTypeValueChange);
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void Redraw()
    {
        // 套装配置信息
        CsvFile cf = EquipMgr.SuitTemplateCsv;
        for (int i = 0; i < cf.rows.Length; i++)
        {
            if (cf.rows[i] == null)
                continue;

            string desc = string.Format("{0}{1}",
                LocalizationMgr.Get(cf.rows[i].Query<string>("name")),
                LocalizationMgr.Get(cf.rows[i].Query<string>("desc")));

            // 添加套装可选列表
            mSuitList.AddItem(desc);

            mSuitDic.Add(desc, cf.rows[i].Query<int>("suit_id"));
        }

        // 添加部位可选列表
        mEquipTypeList.AddItem("All");
        mEquipTypeList.AddItem("武器");
        mEquipTypeList.AddItem("护甲");
        mEquipTypeList.AddItem("鞋子");
        mEquipTypeList.AddItem("护符");
        mEquipTypeList.AddItem("项链");
        mEquipTypeList.AddItem("戒指");

        mEquipTypeDic.Add("武器", EquipConst.WEAPON);
        mEquipTypeDic.Add("护甲", EquipConst.ARMOR);
        mEquipTypeDic.Add("鞋子", EquipConst.SHOES);
        mEquipTypeDic.Add("护符", EquipConst.AMULET);
        mEquipTypeDic.Add("项链", EquipConst.NECKLACE);
        mEquipTypeDic.Add("戒指", EquipConst.RING);

        // 添加装备主属性可选列表
        mPropList.AddItem("随机");

        // 添加装备品质可选列表
        mRarityList.AddItem("白色");
        mRarityList.AddItem("绿色");
        mRarityList.AddItem("蓝色");
        mRarityList.AddItem("紫色");
        mRarityList.AddItem("橙色");

        mRarityDic.Add("白色", 0);
        mRarityDic.Add("绿色", 1);
        mRarityDic.Add("蓝色", 2);
        mRarityDic.Add("紫色", 3);
        mRarityDic.Add("橙色", 4);

        // 添加到词缀可选列表
        mPrefixList.AddItem("有");
        mPrefixList.AddItem("无");

        mPreficDic.Add("有", true);
        mPreficDic.Add("无", false);

        // 添加星级可选列表
        mStarList.AddItem(1.ToString());
        mStarList.AddItem(2.ToString());
        mStarList.AddItem(3.ToString());
        mStarList.AddItem(4.ToString());
        mStarList.AddItem(5.ToString());
        mStarList.AddItem(6.ToString());

        // 添加装备强化可选列表
        for (int i = 0; i <= GameSettingMgr.GetSettingInt("equip_intensify_limit_level"); i++)
            mRankList.AddItem(i.ToString());
    }

    /// <summary>
    /// 装备部位变化回调
    /// </summary>
    void OnEquipTypeValueChange()
    {
        if (mEquipTypeList.value.Equals("All"))
        {
            mPropList.value = "随机";
            mPropList.gameObject.GetComponent<BoxCollider>().enabled = false;
        }
        else
        {
            mPropList.gameObject.GetComponent<BoxCollider>().enabled = true;

            mPropList.value = string.Empty;

            foreach (var item in mPropDic.Keys)
            {
                if (string.IsNullOrEmpty(item))
                    continue;

                mPropList.RemoveItem(item);
            }

            if (!mEquipTypeDic.ContainsKey(mEquipTypeList.value))
                return;

            // 根据部位添加该装备可选的主属性
            List<int> propList = PropMgr.GetPropByEquipType(mEquipTypeDic[mEquipTypeList.value], EquipConst.MAIN_PROP);
            if (propList == null)
                return;

            mPropDic.Clear();

            for (int i = 0; i < propList.Count; i++)
            {
                CsvRow row = PropMgr.GetPropInfo(propList[i]);
                if (row == null)
                    continue;

                string memo = row.Query<string>("memo");

                mPropList.AddItem(memo);

                if(!mPropDic.ContainsKey(memo))
                    mPropDic.Add(memo, propList[i]);
            }
        }
    }

    /// <summary>
    /// 取消按钮点击事件
    /// </summary>
    void OnClickCancelBtn(GameObject go)
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// 确认按钮点击事件
    /// </summary>
    void OnClickConfirmBtn(GameObject go)
    {
        // 构建参数
        LPCMapping para = LPCMapping.Empty;

        string mainPropValue = mPropList.value;

        int mainProp = -1;
        if (mPropDic.ContainsKey(mainPropValue))
            mainProp = mPropDic[mainPropValue];

        string rarityValue = mRarityList.value;
        if (string.IsNullOrEmpty(rarityValue))
        {
            LogMgr.Trace("装备品质为空");
            return;
        }

        if (!mRarityDic.ContainsKey(rarityValue))
            return;

        int rarity = mRarityDic[rarityValue];

        string prefixValue = mPrefixList.value;
        if (string.IsNullOrEmpty(prefixValue))
        {
            LogMgr.Trace("装备是否有词缀为空");
            return;
        }

        if (!mPreficDic.ContainsKey(prefixValue))
            return;

        bool isPrefix = mPreficDic[prefixValue];

        int star = 1;
        if (!int.TryParse(mStarList.value, out star))
            return;

        // 强化等级
        int rank = 0;
        if (!int.TryParse(mRankList.value, out rank))
            return;

        string suitValue = mSuitList.value;
        if (string.IsNullOrEmpty(suitValue))
        {
            LogMgr.Trace("套装为空");
            return;
        }

        if (!mSuitDic.ContainsKey(suitValue))
            return;

        string equipTypeValue = mEquipTypeList.value;
        if (string.IsNullOrEmpty(equipTypeValue))
        {
            LogMgr.Trace("装备部位为空");
            return;
        }

        int amount = 0;
        if (!int.TryParse(mCloneNumInput.value, out amount))
        {
            DialogMgr.Notify("克隆数量不是int");
            return;
        }

        if (mEquipTypeDic.ContainsKey(equipTypeValue))
        {
            int classId = EquipMgr.GetClassId(mSuitDic[suitValue], mEquipTypeDic[equipTypeValue]);

            para.Add("class_id", classId);

            if (mainProp > 0)
                para.Add("main_prop", mainProp);
        }
        else
        {
            if (!equipTypeValue.Equals("All"))
                return;

            para.Add("suit_id", mSuitDic[suitValue]);
        }

        para.Add("star", star);
        para.Add("rank", rank);
        para.Add("rarity", rarity);
        para.Add("is_prefix", isPrefix.Equals(true) ? 1 : 0);

        if (amount <= 0)
        {
            DialogMgr.Notify("克隆数量小于1");
            return;
        }

        // 通知服务器克隆装备
        Operation.CmdAdminCloneEquip.Go(amount, para);
    }

    #endregion
}
