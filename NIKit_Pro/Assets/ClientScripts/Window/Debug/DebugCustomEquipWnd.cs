/// <summary>
/// DebugCustomEquipWnd.cs
/// Created by fengsc 2017/10/20
/// 自定义克隆装备
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class DebugCustomEquipWnd : WindowBase<DebugCustomEquipWnd>
{
    // 套装下拉列表
    public UIPopupList mSuitList;

    // 装备部位（类型）下拉列表
    public UIPopupList mEquipTypeList;

    // 装备主属性下拉列表
    public UIPopupList mMainPropList;

    // 主属性属性值
    public UIInput mMainPropValue;

    // 词缀属性下列表
    public UIPopupList mPrefixPropList;

    // 词缀属性属性值
    public UIInput mPreficPropValue;

    // 次要属性下拉列表
    public UIPopupList mMinorPropList1;

    // 属性值
    public UIInput mMinorPropValue1;

    // 次要属性下拉列表
    public UIPopupList mMinorPropList2;

    // 属性值
    public UIInput mMinorPropValue2;

    // 次要属性下拉列表
    public UIPopupList mMinorPropList3;

    // 属性值
    public UIInput mMinorPropValue3;

    // 次要属性下拉列表
    public UIPopupList mMinorPropList4;

    // 属性值
    public UIInput mMinorPropValue4;

    public GameObject mCancelBtn;

    public GameObject mConfirmBtn;

    Dictionary<string, int> mSuitDic = new Dictionary<string, int>();

    Dictionary<string, int> mEquipTypeDic = new Dictionary<string, int>();

    Dictionary<string, int> mMainPropDic = new Dictionary<string, int>();

    Dictionary<string, int> mMinorPropDic = new Dictionary<string, int>();

    // Use this for initialization
    void Start ()
    {
        // 绘制窗口
        Redraw();

        // 注册事件
        RegisterEvent();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCancelBtn).onClick = OnClickCancelBtn;
        UIEventListener.Get(mConfirmBtn).onClick = OnClickConfirmBtn;

        // 监听控件value变化
        EventDelegate.Add(mEquipTypeList.onChange, OnEquipTypeValueChange);
    }

    /// <summary>
    /// 转隔壁部位变化回调
    /// </summary>
    void OnEquipTypeValueChange()
    {
        mMainPropList.value = string.Empty;

        foreach (var item in mMainPropDic.Keys)
        {
            if (string.IsNullOrEmpty(item))
                continue;

            mMainPropList.RemoveItem(item);
        }

        if (!mEquipTypeDic.ContainsKey(mEquipTypeList.value))
            return;

        // 根据部位添加该装备可选的主属性
        List<int> propList = PropMgr.GetPropByEquipType(mEquipTypeDic[mEquipTypeList.value], EquipConst.MAIN_PROP);
        if (propList == null)
            return;

        mMainPropDic.Clear();

        for (int i = 0; i < propList.Count; i++)
        {
            CsvRow row = PropMgr.GetPropInfo(propList[i]);
            if (row == null)
                continue;

            string memo = row.Query<string>("memo");

            mMainPropList.AddItem(memo);

            if(!mMainPropDic.ContainsKey(memo))
                mMainPropDic.Add(memo, propList[i]);
        }

        // 
        mPrefixPropList.value = string.Empty;
        mMinorPropList1.value = string.Empty;
        mMinorPropList2.value = string.Empty;
        mMinorPropList3.value = string.Empty;
        mMinorPropList4.value = string.Empty;

        foreach (var item in mMinorPropDic.Keys)
        {
            if (string.IsNullOrEmpty(item))
                continue;

            mPrefixPropList.RemoveItem(item);

            mMinorPropList1.RemoveItem(item);

            mMinorPropList2.RemoveItem(item);

            mMinorPropList3.RemoveItem(item);

            mMinorPropList4.RemoveItem(item);
        }

        // 根据部位添加该装备可选的次要属性和词缀属性
        List<int> prefixPropList = PropMgr.GetPropByEquipType(mEquipTypeDic[mEquipTypeList.value], EquipConst.MINOR_PROP);
        if (prefixPropList == null)
            return;

        mMinorPropDic.Clear();

        for (int i = 0; i < prefixPropList.Count; i++)
        {
            CsvRow row = PropMgr.GetPropInfo(prefixPropList[i]);
            if (row == null)
                continue;

            string memo = row.Query<string>("memo");

            mPrefixPropList.AddItem(memo);
            mMinorPropList1.AddItem(memo);
            mMinorPropList2.AddItem(memo);
            mMinorPropList3.AddItem(memo);
            mMinorPropList4.AddItem(memo);

            if(!mMinorPropDic.ContainsKey(memo))
                mMinorPropDic.Add(memo, prefixPropList[i]);
        }
    }

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

        LPCMapping prop = LPCMapping.Empty;

        // 主属性
        LPCArray mainProp = LPCArray.Empty;
        int mainPropValue = 0;
        if (int.TryParse(mMainPropValue.value, out mainPropValue))
        {
            if (!mMainPropDic.ContainsKey(mMainPropList.value))
                mainPropValue = 0;

            // 属性id
            mainProp.Add(mMainPropDic[mMainPropList.value]);

            // 属性值
            mainProp.Add(mainPropValue);
        }

        // 词缀属性
        LPCArray prefix = LPCArray.Empty;
        int prefixPropValue = 0;
        if (int.TryParse(mPreficPropValue.value, out prefixPropValue))
        {
            if (!mMinorPropDic.ContainsKey(mPrefixPropList.value))
                prefixPropValue = 0;

            // 属性id
            prefix.Add(mMinorPropDic[mPrefixPropList.value]);

            // 属性值
            prefix.Add(prefixPropValue);
        }

        // 词缀属性
        LPCArray minorProp1 = LPCArray.Empty;
        int minor1PropValue = 0;
        if (int.TryParse(mMinorPropValue1.value, out minor1PropValue))
        {
            if (!mMinorPropDic.ContainsKey(mMinorPropList1.value))
                minor1PropValue = 0;

            // 属性id
            minorProp1.Add(mMinorPropDic[mMinorPropList1.value]);

            // 属性值
            minorProp1.Add(minor1PropValue);
        }

        // 词缀属性
        LPCArray minorProp2 = LPCArray.Empty;
        int minor2PropValue = 0;
        if (int.TryParse(mMinorPropValue2.value, out minor2PropValue))
        {
            if (!mMinorPropDic.ContainsKey(mMinorPropList2.value))
                minor2PropValue = 0;

            // 属性id
            minorProp2.Add(mMinorPropDic[mMinorPropList2.value]);

            // 属性值
            minorProp2.Add(minor2PropValue);
        }

        // 词缀属性
        LPCArray minorProp3 = LPCArray.Empty;
        int minor3PropValue = 0;
        if (int.TryParse(mMinorPropValue3.value, out minor3PropValue))
        {
            if (!mMinorPropDic.ContainsKey(mMinorPropList3.value))
                minor3PropValue = 0;

            // 属性id
            minorProp3.Add(mMinorPropDic[mMinorPropList3.value]);

            // 属性值
            minorProp3.Add(minor3PropValue);
        }

        // 词缀属性
        LPCArray minorProp4 = LPCArray.Empty;
        int minor4PropValue = 0;
        if (int.TryParse(mMinorPropValue4.value, out minor4PropValue))
        {
            if (!mMinorPropDic.ContainsKey(mMinorPropList4.value))
                minor4PropValue = 0;

            // 属性id
            minorProp4.Add(mMinorPropDic[mMinorPropList4.value]);

            // 属性值
            minorProp4.Add(minor4PropValue);
        }

        if (mainPropValue > 0)
            prop.Add(EquipConst.MAIN_PROP, new LPCArray(mainProp));

        if (prefixPropValue > 0)
            prop.Add(EquipConst.PREFIX_PROP, new LPCArray(prefix));

        LPCArray minorProp = LPCArray.Empty;

        if (minor1PropValue > 0)
            minorProp.Add(minorProp1);

        if (minor2PropValue > 0)
            minorProp.Add(minorProp2);

        if (minor3PropValue > 0)
            minorProp.Add(minorProp3);

        if (minor4PropValue > 0)
            minorProp.Add(minorProp4);

        if (minorProp.Count != 0)
            prop.Add(EquipConst.MINOR_PROP, minorProp);

        // 没有选择装备类型或者套装
        if (!mEquipTypeDic.ContainsKey(mEquipTypeList.value) || !mSuitDic.ContainsKey(mSuitList.value))
            return;

        int classId = EquipMgr.GetClassId(mSuitDic[mSuitList.value], mEquipTypeDic[mEquipTypeList.value]);

        para.Add("class_id", classId);

        if (prop.Count == 0)
            return;

        para.Add("prop", prop);

        // 通知服务器克隆宠物
        Operation.CmdAdminCloneEquip.Go(1, para);
    }
}
