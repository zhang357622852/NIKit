/// <summary>
/// FilterWnd.cs
/// Created by fengsc 2018/08/22
/// 装备筛选窗口
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;
using System.Collections.Generic;
using System;
using System.Linq;

public class FilterWnd : WindowBase<FilterWnd>
{
    #region 成员变量

    // 窗口标题
    public UILabel mTitle;

    // 关闭按钮
    public GameObject mCloseBtn;

    // 主属性下拉菜单
    public UIPopupList mMainPropsBtn;

    // 副属性1下拉菜单
    public UIPopupList mMinorPropsBtn1;

    // 副属性2下拉菜单
    public UIPopupList mMinorPropsBtn2;

    // 是否显示新装备选择按钮
    public UIToggle mSelectNewBtn;
    public UILabel mSelectNewLb;

    // 是否显示强化过的装备选择按钮
    public UIToggle mSelectIntensifyBtn;
    public UILabel mSelectIntensifyLb;

    // 反转排序按钮
    public GameObject mReverseSortBtn;
    public GameObject mReverseSortSelect;
    public GameObject mReverseSortArrow;

    public UIGrid mSuitGrid;

    // 套装格子
    public GameObject mSuitItem;

    // 套装全选按钮
    public UIToggle mSuitAllSelectBtn;
    public UILabel mSuitAllSelectLb;

    // 星级选择
    public UIToggle[] mStarToggles;

    // 星级全选按钮
    public UIToggle mStarAllSelectBtn;
    public UILabel mStarAllSelectLb;

    public UIWidget[] mStars;

    // 装备部位选择
    public UIToggle[] mTypeToggles;
    public UILabel[] mTypeToggleLbs;

    // 装备部位全选按钮
    public UIToggle mTypeAllSelectBtn;
    public UILabel mTypeAllSelectLb;

    // 当前选择的主属性
    private int mSelectMainProp = -1;

    // 当前选择的次要属性1
    private int mSelectMinorProp1 = -1;

    // 当前选择的次要属性2
    private int mSelectMinorProp2 = -1;

    // 当前选择的套装列表
    private LPCArray mSelectSuits = LPCArray.Empty;

    // 当前选择的星级列表
    private LPCArray mSelectStars = LPCArray.Empty;

    // 当前选择的装备部位列表
    private LPCArray mSelectTypes = LPCArray.Empty;

    private Dictionary<string, int> mMainPropDic = new Dictionary<string, int>();

    private Dictionary<string, int> mMinorPropDic = new Dictionary<string, int>();

    private Dictionary<int, List<Property>> mSuitEquipDic = new Dictionary<int, List<Property>>();

    private List<EquipSuitItemWnd> mSuitItems = new List<EquipSuitItemWnd>();

    // 委托变量
    private CallBack mConditionCallBack;

    private int mIsReverse = 0;

    private bool mIsCancel = true;

    private CallBack mCloseCb;

    [HideInInspector]
    public bool mIsCurClose = false;

    // 是否只显示新装备
    private int mOnlyShowNewEquip = 0;

    // 是否隐藏强化过的装备
    private int mHideIntensifyEquip = 0;

    string mId;

    #endregion

    // Use this for initialization
    void Awake ()
    {
        // 初始化本地化文本
        InitLable();

        // 清空下拉列表
        mMainPropsBtn.Clear();
        mMinorPropsBtn1.Clear();
        mMinorPropsBtn2.Clear();

        // 绘制窗口
        Redraw();

        // 注册事件
        RegisterEvent();

        mId = Game.GetUniqueName("FilterWnd");
    }

    void OnEnable()
    {
        RefreshStarsAlpha();

        RefreshTypeAlpha();

        RefreshData();

        // 执行选中默认选项
        if (!mIsCurClose)
        {
            mSelectNewBtn.Set(false);

            mSelectIntensifyBtn.Set(false);

            SelectDefaultOption();
        }

        EventMgr.RegisterEvent(mId, EventMgrEventType.EVENT_CLEAR_NEW, ClearNewInfo);

        // 包裹变化事件
        if (ME.user != null)
            ME.user.baggage.eventCarryChange += BaggageChange;
    }

    void OnDisable()
    {
        mIsCancel = false;

        EventMgr.UnregisterEvent(mId);

        if (ME.user != null)
            ME.user.baggage.eventCarryChange -= BaggageChange;
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLable()
    {
        mSuitAllSelectLb.text = LocalizationMgr.Get("FilterWnd_18");
        mStarAllSelectLb.text = LocalizationMgr.Get("FilterWnd_18");
        mTypeAllSelectLb.text = LocalizationMgr.Get("FilterWnd_18");
        mSelectNewLb.text = LocalizationMgr.Get("FilterWnd_17");
        mSelectIntensifyLb.text = LocalizationMgr.Get("FilterWnd_19");

        if (mTitle != null)
            mTitle.text = LocalizationMgr.Get("FilterWnd_22");

        for (int i = 0; i < mTypeToggleLbs.Length; i++)
            mTypeToggleLbs[i].text = EquipConst.EquipTypeToName[i];
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 监听下拉菜单value的变化
        EventDelegate.Add(mMainPropsBtn.onChange, OnMainPropsListValueChange);
        EventDelegate.Add(mMinorPropsBtn1.onChange, OnMinorPropsListValueChange1);
        EventDelegate.Add(mMinorPropsBtn2.onChange, OnMinorPropsListValueChange2);

        UIEventListener.Get(mSuitAllSelectBtn.gameObject).onClick = OnClickAllSelectSuitToggle;
        UIEventListener.Get(mStarAllSelectBtn.gameObject).onClick = OnClickAllStar;
        UIEventListener.Get(mTypeAllSelectBtn.gameObject).onClick = OnClickAllType;

        // 绑定UIToggle onChange 回调
        for (int i = 0; i < EquipConst.EquipStarList.Count; i++)
        {
            UIEventListener.Get(mStarToggles[i].gameObject).onClick = OnClickStarToggle;
            mStarToggles[i].gameObject.GetComponent<UIEventListener>().parameter = EquipConst.EquipStarList[i];
        }

        // 绑定UIToggle onChange 回调
        for (int i = 0; i < EquipConst.EquipTypeList.Count; i++)
        {
            UIEventListener.Get(mTypeToggles[i].gameObject).onClick = OnClickTypeToggle;

            mTypeToggles[i].gameObject.GetComponent<UIEventListener>().parameter = EquipConst.EquipTypeList[i];
        }

        // 注册按钮点击事件
        UIEventListener.Get(mReverseSortBtn).onClick = OnClickReversSortBtn;

        if (mCloseBtn != null)
            UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        UIEventListener.Get(mSelectNewBtn.gameObject).onClick = OnClickSelectNewBtn;
        UIEventListener.Get(mSelectIntensifyBtn.gameObject).onClick = OnClickSelectIntensifyBtn;
    }

    void ClearNewInfo(int eventId, MixedValue para)
    {
        // 刷新数据
        RefreshData();
    }

    void BaggageChange(string[] pos)
    {
        mIsCancel = false;

        // 延迟刷新计时器
        MergeExecuteMgr.DispatchExecute(DoDelayedRefresh);
    }

    /// <summary>
    /// 选中默认选项
    /// </summary>
    void SelectDefaultOption()
    {
        // 选中所有星级
        for (int i = 0; i < mStarToggles.Length; i++)
        {
            mStarToggles[i].Set(true);
            DoClickStarToggle(mStarToggles[i].gameObject);
        }

        mStarAllSelectBtn.Set(true);

        // 选中所有装备部位
        for (int i = 0; i < mTypeToggles.Length; i++)
        {
            mTypeToggles[i].Set(true);
            DoClickTypeToggle(mTypeToggles[i].gameObject);
        }

        mTypeAllSelectBtn.Set(true);
    }

    void DoDelayedRefresh()
    {
        RefreshData();
    }

    /// <summary>
    /// 关闭按钮
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 执行回调
        if (mCloseCb != null)
            mCloseCb.Go();

        mIsCurClose = true;

        // 关闭当前窗口
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mMainPropDic = PropType.MainPropNameToId;

        mMinorPropDic = PropType.MinorPropNameToId;

        // 初始化下拉列表
        foreach (string propName in mMainPropDic.Keys)
            mMainPropsBtn.AddItem(propName);

        // 默认选中第一个
        mMainPropsBtn.value = mMainPropsBtn.items[0];

        foreach (string propName in mMinorPropDic.Keys)
            mMinorPropsBtn1.AddItem(propName);

        // 默认选中第一个
        mMinorPropsBtn1.value = mMinorPropsBtn1.items[0];

        foreach (string propName in mMinorPropDic.Keys)
            mMinorPropsBtn2.AddItem(propName);

        // 默认选中第一个
        mMinorPropsBtn2.value = mMinorPropsBtn2.items[0];

        if (mSuitItem == null)
            return;

        mSuitEquipDic = BaggageMgr.GetSuitByPage(ME.user, ContainerConfig.POS_ITEM_GROUP);

        // 初始化套装格子
        foreach (CsvRow row in EquipMgr.SuitTemplateCsv.rows)
        {
            GameObject item = Instantiate(mSuitItem);
            if (item == null)
                continue;

            if (!item.activeSelf)
                item.SetActive(true);

            // 设置父级
            item.transform.SetParent(mSuitGrid.transform);

            // 初始化位置
            item.transform.localPosition = Vector3.zero;

            // 初始化scale
            item.transform.localScale = Vector3.one;

            EquipSuitItemWnd script = item.GetComponent<EquipSuitItemWnd>();
            if (script == null)
                return;

            script.CallBack = OnSuitCallBack;


            mSuitItems.Add(script);
        }

        // 激活排序组件
        mSuitGrid.Reposition();

        // 隐藏套装模板格子
        mSuitItem.SetActive(false);
    }

    /// <summary>
    /// 点击回调
    /// </summary>
    void OnSuitCallBack(int suitId, bool isSelect)
    {
        if (isSelect)
        {
            // 添加套装缓存
            if (mSelectSuits.IndexOf(suitId) == -1)
                mSelectSuits.Add(suitId);
        }
        else
        {
            // 移除套装缓存
            if (mSelectSuits.IndexOf(suitId) != -1)
                mSelectSuits.Remove(suitId);
        }

        // 取消全选选项，  但不执行回调
        if (mIsCancel && mSuitAllSelectBtn.value)
            mSuitAllSelectBtn.Set(false, false);

        // 执行回调
        DoConditionCallBack();
    }

    /// <summary>
    /// 套装全选按钮点击回调
    /// </summary>
    void OnClickAllSelectSuitToggle(GameObject go)
    {
        mIsCancel = false;

        // 全部选中/取消
        for (int i = 0; i < mSuitItems.Count; i++)
            mSuitItems[i].SetSelect(mSuitAllSelectBtn.value);

        mIsCancel = true;
    }

    /// <summary>
    /// UIPopupList value变化回调
    /// </summary>
    void OnMainPropsListValueChange()
    {
        int propId;

        if (!mMainPropDic.TryGetValue(mMainPropsBtn.value, out propId))
            return;

        // 当前选择的主属性
        mSelectMainProp = propId;

        // 执行回调
        DoConditionCallBack();

        mMinorPropsBtn1.Clear();
        mMinorPropsBtn2.Clear();

        foreach (string propName in mMinorPropDic.Keys)
        {
            int id = mMinorPropDic[propName];

            if (id == -1)
            {
                mMinorPropsBtn1.AddItem(propName);
                mMinorPropsBtn2.AddItem(propName);

                continue;
            }

            if (id == mSelectMainProp)
            {
                mMinorPropsBtn1.RemoveItem(propName);

                mMinorPropsBtn2.RemoveItem(propName);
            }

            if (id != mSelectMinorProp2 && id != mSelectMainProp && mMinorPropsBtn1.items.IndexOf(propName) == -1)
                mMinorPropsBtn1.AddItem(propName);

            if (id != mSelectMinorProp1 && id != mSelectMainProp && mMinorPropsBtn2.items.IndexOf(propName) == -1)
                mMinorPropsBtn2.AddItem(propName);
        }

        mMinorPropsBtn1.Set(mMinorPropsBtn1.items[0], false);
        mMinorPropsBtn2.Set(mMinorPropsBtn2.items[0], false);
    }

    /// <summary>
    /// UIPopupList value变化回调
    /// </summary>
    void OnMinorPropsListValueChange1()
    {
        int propId;

        if (!mMinorPropDic.TryGetValue(mMinorPropsBtn1.value, out propId))
            return;

        // 当前选择的副属性1
        mSelectMinorProp1 = propId;

        // 执行回调
        DoConditionCallBack();

        mMinorPropsBtn2.Clear();
        // mMinorPropsBtn2 下拉列表中相同的属性
        foreach (string propName in mMinorPropDic.Keys)
        {
            int id = mMinorPropDic[propName];

            if (id == -1)
            {
                mMinorPropsBtn2.AddItem(propName);

                continue;
            }


            if (id == mSelectMinorProp1)
                mMinorPropsBtn2.RemoveItem(propName);

            if (id != mSelectMainProp && id != mSelectMinorProp1 && mMinorPropsBtn2.items.IndexOf(propName) == -1)
                mMinorPropsBtn2.AddItem(propName);
        }

        mMinorPropsBtn2.Set(mMinorPropsBtn2.items[0], false);

        mMainPropsBtn.Clear();

        // 移除mMainPropsBtn 下拉列表中相同的属性
        foreach (string propName in mMainPropDic.Keys)
        {
            int id = mMainPropDic[propName];

            if (id == -1)
            {
                mMainPropsBtn.AddItem(propName);

                continue;
            }

            if (id == mSelectMinorProp1)
                mMainPropsBtn.RemoveItem(propName);

            if (id != mSelectMinorProp2 && id != mSelectMinorProp1 && mMainPropsBtn.items.IndexOf(propName) == -1)
                mMainPropsBtn.AddItem(propName);
        }

        mMainPropsBtn.Set(mMainPropsBtn.items[0], false);
    }

    /// <summary>
    /// UIPopupList value变化回调
    /// </summary>
    void OnMinorPropsListValueChange2()
    {
        int propId;

        if (!mMinorPropDic.TryGetValue(mMinorPropsBtn2.value, out propId))
            return;

        // 当前选择的副属性2
        mSelectMinorProp2 = propId;

        // 执行回调
        DoConditionCallBack();

        mMinorPropsBtn1.Clear();

        // mMinorPropsBtn1 下拉列表中相同的属性
        foreach (string propName in mMinorPropDic.Keys)
        {
            int id = mMinorPropDic[propName];

            if (id == -1)
            {
                mMinorPropsBtn1.AddItem(propName);

                continue;
            }

            if (id == mSelectMinorProp2)
                mMinorPropsBtn1.RemoveItem(propName);

            if (id != mSelectMainProp && id != mSelectMinorProp2 && mMinorPropsBtn1.items.IndexOf(propName) == -1)
                mMinorPropsBtn1.AddItem(propName);
        }

        // 选中第一个
        mMinorPropsBtn1.Set(mMinorPropsBtn1.items[0], false);


        mMainPropsBtn.Clear();
        // 移除mMainPropsBtn 下拉列表中相同的属性
        foreach (string propName in mMainPropDic.Keys)
        {
            int id = mMainPropDic[propName];

            if (id == -1)
            {
                mMainPropsBtn.AddItem(propName);

                continue;
            }

            if (id == mSelectMinorProp2 && mSelectMinorProp2 != -1)
                mMainPropsBtn.RemoveItem(propName);

            if (id != mSelectMinorProp1 && id != mSelectMinorProp2 && mMainPropsBtn.items.IndexOf(propName) == -1)
                mMainPropsBtn.AddItem(propName);
        }

        mMainPropsBtn.Set(mMainPropsBtn.items[0], false);
    }

    /// <summary>
    /// 是否显示新装备选项按钮点击回调
    /// </summary>
    void OnClickSelectNewBtn(GameObject go)
    {
        mOnlyShowNewEquip = mSelectNewBtn.value ? 1 : 0;

        // 执行回调
        DoConditionCallBack();
    }

    /// <summary>
    /// 是否显示强化过的装备选项按钮点击回调
    /// </summary>
    void OnClickSelectIntensifyBtn(GameObject go)
    {
        mHideIntensifyEquip = mSelectIntensifyBtn.value ? 1 : 0;

        // 执行回调
        DoConditionCallBack();
    }

    /// <summary>
    /// 星级选项点击回调
    /// </summary>
    void OnClickStarToggle(GameObject go)
    {
        DoClickStarToggle(go);

        if (mStarAllSelectBtn.value)
            mStarAllSelectBtn.Set(false);
    }

    void DoClickStarToggle(GameObject go)
    {
        int star = (int) go.GetComponent<UIEventListener>().parameter;

        if (go.GetComponent<UIToggle>().value)
        {
            // 添加缓存
            if(mSelectStars.IndexOf(star) == -1)
                mSelectStars.Add(star);
        }
        else
        {
            // 移除缓存
            if(mSelectStars.IndexOf(star) != -1)
                mSelectStars.Remove(star);
        }

        RefreshStarsAlpha();

        // 执行回调
        DoConditionCallBack();
    }

    void RefreshStarsAlpha()
    {
        for (int i = 0; i < mStarToggles.Length; i++)
        {
            if (mStarToggles[i].value)
                mStars[i].alpha = 1.0f;
            else
                mStars[i].alpha = 0.5f;
        }
    }

    /// <summary>
    /// 星级全选按钮点击
    /// </summary>
    void OnClickAllStar(GameObject go)
    {
        // 全部选中/取消星级选项
        for (int i = 0; i < EquipConst.EquipStarList.Count; i++)
        {
            mStarToggles[i].Set(mStarAllSelectBtn.value);

            DoClickStarToggle(mStarToggles[i].gameObject);
        }
    }

    void OnClickTypeToggle(GameObject go)
    {
        DoClickTypeToggle(go);

        if (mTypeAllSelectBtn.value)
            mTypeAllSelectBtn.Set(false);
    }

    void DoClickTypeToggle(GameObject go)
    {
        int type = (int) go.GetComponent<UIEventListener>().parameter;

        if (go.GetComponent<UIToggle>().value)
        {
            // 添加缓存
            if(mSelectTypes.IndexOf(type) == -1)
                mSelectTypes.Add(type);
        }
        else
        {
            // 移除缓存
            if(mSelectTypes.IndexOf(type) != -1)
                mSelectTypes.Remove(type);
        }

        RefreshTypeAlpha();

        // 执行回调
        DoConditionCallBack();
    }

    void RefreshTypeAlpha()
    {
        for (int i = 0; i < mTypeToggles.Length; i++)
        {
            if (mTypeToggles[i].value)
                mTypeToggleLbs[i].alpha = 1.0f;
            else
                mTypeToggleLbs[i].alpha = 0.5f;
        }
    }

    /// <summary>
    /// 装备类型全选按钮点击
    /// </summary>
    void OnClickAllType(GameObject go)
    {
        // 全部选中/取消装备类型选项
        for (int i = 0; i < EquipConst.EquipTypeList.Count; i++)
        {
            mTypeToggles[i].Set(mTypeAllSelectBtn.value);

            DoClickTypeToggle(mTypeToggles[i].gameObject);
        }
    }

    /// <summary>
    /// 点击反转排序按钮
    /// </summary>
    void OnClickReversSortBtn(GameObject go)
    {
        if (mReverseSortSelect.activeSelf)
        {
            mIsReverse = 0;

            mReverseSortSelect.SetActive(false);

            mReverseSortArrow.transform.localScale = new Vector3(1, -1, 1);
        }
        else
        {
            mIsReverse = 1;

            mReverseSortSelect.SetActive(true);

            mReverseSortArrow.transform.localScale = new Vector3(1, 1, 1);
        }

        // 执行回调
        DoConditionCallBack();
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(params CallBack[] cbs)
    {
        mConditionCallBack = cbs[0] as CallBack;

        mCloseCb = cbs[1] as CallBack;
    }

    /// <summary>
    /// 清除数据
    /// </summary>
    public void ClearData()
    {
        // 重置选项
        // 当前选择的主属性
        mSelectMainProp = -1;

        // 当前选择的次要属性1
        mSelectMinorProp1 = -1;

        // 当前选择的次要属性2
        mSelectMinorProp2 = -1;

        // 当前选择的套装列表
        mSelectSuits = LPCArray.Empty;

        // 当前选择的星级列表
        mSelectStars = LPCArray.Empty;

        // 当前选择的装备部位列表
        mSelectTypes = LPCArray.Empty;

        mIsReverse = 0;

        mIsCancel = true;

        mIsCurClose = false;

        mOnlyShowNewEquip = 0;

        mHideIntensifyEquip = 0;

        // 默认选中第一个
        if (gameObject.activeSelf)
            mMainPropsBtn.value = mMainPropsBtn.items[0];

        // 默认选中第一个
        if (gameObject.activeSelf)
            mMinorPropsBtn1.value = mMinorPropsBtn1.items[0];

        // 默认选中第一个
        if (gameObject.activeSelf)
            mMinorPropsBtn2.value = mMinorPropsBtn2.items[0];

        // 取消全部选中
        mTypeAllSelectBtn.Set(false);
        foreach (UIToggle toggle in mTypeToggles)
            toggle.Set(false);

        mStarAllSelectBtn.Set(false);
        foreach (UIToggle toggle in mStarToggles)
            toggle.Set(false);

        mSuitAllSelectBtn.Set(false);
        foreach (EquipSuitItemWnd item in mSuitItems)
            item.SetSelect(false);

        if (mConditionCallBack != null)
        {
            DoConditionCallBack();

            mConditionCallBack = null;
        }

        if (mCloseCb != null)
        {
            mCloseCb.Go();

            mCloseCb = null;
        }
    }

    /// <summary>
    /// 执行条件回调
    /// </summary>
    public void DoConditionCallBack()
    {
        // 装备筛选条件
        LPCMapping condition = LPCMapping.Empty;

        LPCArray mianProp = LPCArray.Empty;

        if (mSelectMainProp != -1)
            mianProp.Add(mSelectMainProp);

        // 选择的主要属性
        condition.Add(EquipConst.MAIN_PROP, mianProp);

        // 选择的次要属性

        LPCArray minor = LPCArray.Empty;

        if (mSelectMinorProp1 != -1)
            minor.Add(mSelectMinorProp1);

        if (mSelectMinorProp2 != -1)
            minor.Add(mSelectMinorProp2);

        condition.Add(EquipConst.MINOR_PROP, minor);

        // 选择的套装列表
        condition.Add("suits", mSelectSuits);

        // 选择的星级列表
        condition.Add("stars", mSelectStars);

        // 选择的装备类型列表
        condition.Add("types", mSelectTypes);

        // 排序是否倒转
        condition.Add("reverse", mIsReverse);

        // 是否只显示新装备
        condition.Add("only_new", mOnlyShowNewEquip);

        // 是否隐藏强化过的装备
        condition.Add("hide_intensify", mHideIntensifyEquip);

        // 执行回调
        if (mConditionCallBack != null)
            mConditionCallBack.Go(condition);
    }

    public void RefreshData()
    {
        mSuitEquipDic = BaggageMgr.GetSuitByPage(ME.user, ContainerConfig.POS_ITEM_GROUP);

        CsvRow[] rows = EquipMgr.SuitTemplateCsv.rows;

        for (int i = 0; i < rows.Length; i++)
        {
            List<Property> equipList = new List<Property>();

            if (! mSuitEquipDic.TryGetValue(rows[i].Query<int>("suit_id"), out equipList))
                equipList = new List<Property>();

            // 绑定数据
            mSuitItems[i].BindData(rows[i], equipList, false);
        }
    }
}
