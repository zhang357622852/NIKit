/// <summary>
/// ShareOperateEquipIntensifyWnd.cs
/// Created by zhangwm 2018/07/09
/// 分享操作界面-装备强化+12以上
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class ShareOperateEquipIntensifyWnd : WindowBase<ShareOperateEquipIntensifyWnd>
{
    #region 成员变量
    public UILabel mIntensifyLab;
    public UILabel mMainAttribLab;
    public UILabel mPrefixAttribLab;
    public UIGrid mMinorGrid;
    public GameObject mMinorAttribGo;
    public UILabel mSuitDesLab;
    public UILabel mSuitDesValueLab;

    public UILabel mIntensifySucLab;
    public UILabel mEquipNameLab;
    public EquipItemWnd mEquipItemWnd;

    private Property mEquipPro = null;
    //记录旧的附加属性，与现在的附加属性对比，才能知道哪个是新增属性和增量
    private Dictionary<int, int> mMinorProDic = null;
    #endregion

    private void Start()
    {
        // 初始化文本
        InitText();
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    private void InitText()
    {
        mIntensifySucLab.text = LocalizationMgr.Get("EquipStrengthenWnd_6");
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    private void Redraw()
    {
        if (mEquipPro == null || mMinorProDic == null)
            return;

        // 获取强化等级
        int rank = mEquipPro.Query<int>("rank");

        //装备的属性数据
        LPCMapping equipProMap = mEquipPro.Query<LPCMapping>("prop");
        if (equipProMap == null)
            return;

        LPCArray mainProp = equipProMap.GetValue<LPCArray>(EquipConst.MAIN_PROP);// 主属性
        LPCArray minorProp = equipProMap.GetValue<LPCArray>(EquipConst.MINOR_PROP); // 附加属性
        LPCArray prefixProp = equipProMap.GetValue<LPCArray>(EquipConst.PREFIX_PROP);// 获取词缀属性

        //强化等级 +12
        mIntensifyLab.text = string.Format("+{0}", rank);

        string propValue = string.Empty;
        //主属性
        if (mainProp != null)
        {
            LPCArray mainAddProArr = LPCArray.Empty;
            int value = 0;
            foreach (LPCValue item in mainProp.Values)
            {
                propValue += PropMgr.GetPropDesc(item.AsArray, EquipConst.MAIN_PROP);

                value = FetchPropMgr.GetMainPropIntensifyValue(mEquipPro, item.AsArray[0].AsInt, 0);
                mainAddProArr.Add(item.AsArray[0].AsInt);
                mainAddProArr.Add(value);
            }
            mMainAttribLab.text = propValue;
            //主属性-增值
            Transform tran = mMainAttribLab.transform.Find("addAttrib");
            if (tran != null)
                tran.GetComponent<UILabel>().text = string.Format("↑{0}", PropMgr.GetPropValueDesc(mainAddProArr, 0));
        }

        //措词属性
        propValue = string.Empty;
        if (prefixProp != null)
        {
            foreach (LPCValue item in prefixProp.Values)
                propValue += PropMgr.GetPropDesc(item.AsArray, EquipConst.PREFIX_PROP);

        }
        mPrefixAttribLab.text = propValue;

        //附加属性
        mMinorAttribGo.SetActive(true);
        if (minorProp != null)
        {
            foreach (LPCValue item in minorProp.Values)
            {
                GameObject go = NGUITools.AddChild(mMinorGrid.gameObject, mMinorAttribGo);
                go.GetComponent<UILabel>().text = PropMgr.GetPropDesc(item.AsArray, EquipConst.MINOR_PROP);
                //附加属性-增值
                Transform tran = go.transform.Find("addAttrib");
                if (tran != null)
                    tran.GetComponent<UILabel>().text = string.Empty;

                if (mMinorProDic.ContainsKey(item.AsArray[0].AsInt))
                {
                    int addValue = item.AsArray[1].AsInt - mMinorProDic[item.AsArray[0].AsInt];
                    if (addValue > 0)
                    {
                        LPCArray minorAddProArr = LPCArray.Empty;
                        minorAddProArr.Add(item.AsArray[0].AsInt);
                        minorAddProArr.Add(addValue);
                        if (tran != null)
                            tran.GetComponent<UILabel>().text = string.Format("↑{0}", PropMgr.GetPropValueDesc(minorAddProArr, 0));
                    }
                }
            }
        }
        mMinorAttribGo.SetActive(false);
        mMinorGrid.Reposition();

        //套装属性
        CsvFile csv = EquipMgr.SuitTemplateCsv;
        propValue = string.Empty;
        if (csv != null)
        {
            CsvRow row = csv.FindByKey(mEquipPro.Query<int>("suit_id"));
            if (row != null)
            {
                mSuitDesLab.text = string.Format("{0}{1}:", row.Query<int>("sub_count"), LocalizationMgr.Get("EquipViewWnd_1"));

                LPCArray props = row.Query<LPCArray>("props");
                if (props != null)
                {
                    //获取套装描述信息;
                    foreach (LPCValue item in props.Values)
                        propValue += PropMgr.GetPropDesc(item.AsArray, EquipConst.SUIT_PROP);

                    mSuitDesValueLab.text = propValue;
                }
            }
        }

        //装备描述
        mEquipNameLab.text = string.Format("+{0} {1}", rank, mEquipPro.Short());

        //装备item
        mEquipItemWnd.SetBind(mEquipPro, false);
    }

    #region 外部
    public void BindData(Property pro, Dictionary<int, int> minorProDic)
    {
        if (pro == null || minorProDic == null)
            return;

        mEquipPro = pro;
        mMinorProDic = minorProDic;

        Redraw();
    }
    #endregion
}
