/// <summary>
/// FastStrengthenWnd.cs
/// Created by fengsc 2017/12/21
/// 快速强化窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class FastStrengthenWnd : WindowBase<FastStrengthenWnd>
{
    // 窗口标题
    public UILabel mTitle;

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    // 确认按钮
    public GameObject mConfirmBtn;
    public UILabel mConfirmBtnLb;

    // 再来一次按钮
    public UISprite mAgainBtn;
    public UILabel mAgainBtnLb;

    public GameObject mGrid;

    public GameObject mItem;

    public TweenAlpha mTweenAlpha;

    public UILabel mLimitTips;

    // 装备对象
    Property mEquipOb;

    // 缓存装备属性
    Dictionary<int, Dictionary<int, int>> mCacheProp = new Dictionary<int, Dictionary<int, int>>();

    // 附加属性
    private LPCArray mMinorProp = LPCArray.Empty;

    float mLineHeight = 44f;

    // 本次强化或者新增加的属性
    LPCArray mProp = LPCArray.Empty;

    // 装备强化结果
    int mResult = 0;

    // 强化次数
    int mCurIntensifyCount = 0;

    // 是否金钱不足
    bool mIsNoMoney = false;

    // 强化前的装备等级
    [HideInInspector]
    public int mBeforeRank = 0;

    bool IsShowMaxLevelDesc = false;

    List<GameObject> mItemList = new List<GameObject>();

    int mMaxEquipIntensify = 0;

    int mLimitRank = 0;

    void Awake()
    {
        // 注册动画回调
        EventDelegate.Add(mTweenAlpha.onFinished, OnTweenAlphaFinish);
    }

    // Use this for initialization
    void Start ()
    {
        mConfirmBtnLb.text = LocalizationMgr.Get("FastStrengthenWnd_2");

        mAgainBtnLb.text = LocalizationMgr.Get("FastStrengthenWnd_10");

        // 装备最大强化次数
        mMaxEquipIntensify = GameSettingMgr.GetSettingInt("max_equip_intensify");

        mLimitRank = GameSettingMgr.GetSettingInt("limit_equip_intensify_rank");

        // 显示装备强化次数限制
        RefreshLimitData();

        // 每天零点刷新一次
        InvokeRepeating("RefreshLimitData", (float) Game.GetZeroClock(1), 86400);
    }

    /// <summary>
    /// 显示装备强化次数限制
    /// </summary>
    void ShowLimitTips()
    {
        // 开启版署模式累计许愿次数
        if (ME.user.QueryTemp<int>("gapp_world") != 1)
        {
            mLimitTips.gameObject.SetActive(false);
            return;
        }

        LPCMapping limitData = LPCMapping.Empty;

        LPCValue v = OptionMgr.GetLocalOption(ME.user, "limit_equip_intensify");
        if (v != null && v.IsMapping)
            limitData = v.AsMapping;

        mLimitTips.text = string.Format(LocalizationMgr.Get("EquipStrengthenWnd_18"), mLimitRank, limitData.GetValue<int>("amount"), mMaxEquipIntensify);

        mLimitTips.gameObject.SetActive(true);
    }

    void RefreshLimitData()
    {
        // 开启版署模式累计许愿次数
        if (ME.user.QueryTemp<int>("gapp_world") == 1)
        {
            LPCMapping limitData = LPCMapping.Empty;

            LPCValue v = OptionMgr.GetLocalOption(ME.user, "limit_equip_intensify");
            if (v != null && v.IsMapping)
                limitData = v.AsMapping;

            // 重置数据
            if (!TimeMgr.IsSameDay(TimeMgr.GetServerTime(), limitData.GetValue<int>("refresh_time")))
                OptionMgr.SetLocalOption(ME.user, "limit_equip_intensify", LPCValue.Create(LPCMapping.Empty));
        }

        // 限制数据
        ShowLimitTips();
    }

    bool CheckLimit()
    {
        // 开启版署模式累计许愿次数
        if (ME.user.QueryTemp<int>("gapp_world") != 1)
            return true;

        LPCMapping limitData = LPCMapping.Empty;

        LPCValue v = OptionMgr.GetLocalOption(ME.user, "limit_equip_intensify");
        if (v != null && v.IsMapping)
            limitData = v.AsMapping;

        // 重置数据
        if (!TimeMgr.IsSameDay(TimeMgr.GetServerTime(), limitData.GetValue<int>("refresh_time")))
        {
            OptionMgr.SetLocalOption(ME.user, "limit_equip_intensify", LPCValue.Create(LPCMapping.Empty));

            return true;
        }

        if (limitData.GetValue<int>("amount") >= mMaxEquipIntensify)
            return false;

        return true;
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("FastStrengthenWnd");
        EventMgr.UnregisterEvent("FastStrengthenWnd_NO_MONEY");

        MsgMgr.RemoveDoneHook("MSG_LOGIN_NOTIFY_OK", "FastStrengthenWnd");

        CancelInvoke("RefreshLimitData");
    }

    /// <summary>
    /// tween动画回调
    /// </summary>
    void OnTweenAlphaFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口显示完成，通知服务器强化装备
        SendMessage();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mConfirmBtn).onClick = OnClickCloseBtn;

        // 注册装备强化事件
        EventMgr.RegisterEvent("FastStrengthenWnd", EventMgrEventType.EVENT_EQUIP_STRENGTHEN, OnEquipStrengthen);

        // 注册装备强化金钱事件
        EventMgr.RegisterEvent("FastStrengthenWnd_NO_MONEY", EventMgrEventType.EVENT_EQUIP_INTENSIY_NO_MONEY, OnIntensifyEquipNoMoney);

        // 注册快速强化动画播放完成事件
        EventMgr.RegisterEvent("FastStrengthenWnd", EventMgrEventType.EVENT_FAST_INTENFISY_COUNT_LIMIT, OnCountMaxLimit);

    }

    /// <summary>
    /// 动画播放完成事件回调
    /// </summary>
    void OnCountMaxLimit(int eventId, MixedValue para)
    {
        if(IsShowMaxLevelDesc)
            return;

        IsShowMaxLevelDesc = true;

        // 重新绘制窗口
        Redraw();
    }

    /// <summary>
    /// 装备强化金钱不足事件回调
    /// </summary>
    void OnIntensifyEquipNoMoney(int eventId, MixedValue para)
    {
        mIsNoMoney = true;

        // 重绘窗口
        Redraw();

        // 解注册事件
        EventMgr.UnregisterEvent("FastStrengthenWnd_NO_MONEY");
    }

    /// <summary>
    /// 装备强化消息回调
    /// </summary>
    void OnEquipStrengthen(int eventId, MixedValue para)
    {
        if (para == null)
            return;

        LPCMapping map = para.GetValue<LPCMapping>();

        if (map == null)
            return;

        // 强化结果
        mResult = map.GetValue<int>("result");

        // 重绘窗口
        Redraw();

        ShowLimitTips();
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 再来一次按钮点击事件
    /// </summary>
    void OnClickAgainBtn(GameObject go)
    {
        if (! CheckLimit())
        {
            DialogMgr.Notify(LocalizationMgr.Get("EquipStrengthenWnd_19"));

            return;
        }

        // 当前装备的强化等级
        int rank = mEquipOb.Query<int>("rank");

        if (rank >= GameSettingMgr.GetSettingInt("equip_intensify_limit_level"))
        {
            // 装备强化等级达到上限
            DialogMgr.Notify(LocalizationMgr.Get("EquipStrengthenWnd_8"));

            return;
        }

        for (int i = 0; i < mItemList.Count; i++)
            mItemList[i].SetActive(false);

        // 重置强化次数
        mCurIntensifyCount = 0;

        // 通知服务器强化装备
        SendMessage();

        // 设置按钮状态为不可用
        SetAgainBtnState(false);
    }

    /// <summary>
    /// 重绘窗口
    /// </summary>
    void Redraw()
    {
        // 获取装备属性
        GetEquipProp();

        Dictionary<int, int> prop = new Dictionary<int, int>();

        prop = mCacheProp[EquipConst.MINOR_PROP];

        // 是否有新增属性
        bool isAdd = false;

        // 次要属性
        foreach (LPCValue item in mMinorProp.Values)
        {
            if (item == null || !item.IsArray)
                continue;

            int value = 0;

            // 不包含该属性id(新增属性)
            if (!prop.TryGetValue(item.AsArray[0].AsInt, out value))
            {
                mProp = item.AsArray;

                isAdd = true;

                break;
            }

            // 属性值没变
            if (value == item.AsArray[1].AsInt)
                continue;

            // 属性增强
            mProp = item.AsArray;

            break;
        }

        // 缓存装备属性
        CacheProp();

        // 累计强化次数
        mCurIntensifyCount++;

        // 获取缓存的item
        GameObject clone = mItemList[mCurIntensifyCount - 1];
        if (clone == null)
            return;

        clone.SetActive(true);

        // 绑定数据
        clone.GetComponent<FastStrengthenItem>().Bind(mEquipOb, mProp, mCurIntensifyCount, mResult, isAdd, mIsNoMoney, mBeforeRank, CheckLimit());

        mProp = LPCArray.Empty;
    }

    /// <summary>
    /// 缓存一批GameObejct待用
    /// </summary>
    void CreatedGameObject()
    {
        mItem.SetActive(false);

        for (int i = 0; i < GameSettingMgr.GetSettingInt("auto_intensify_count"); i++)
        {
            GameObject clone = Instantiate(mItem);
            if (clone == null)
                return;

            clone.transform.SetParent(mGrid.transform);

            clone.transform.localScale = Vector3.one;

            // 计算item的位置
            clone.transform.localPosition = new Vector3(
                mItem.transform.localPosition.x,
                (0 - i) * mLineHeight,
                mItem.transform.localPosition.z);

            mItemList.Add(clone);
        }
    }

    /// <summary>
    /// 缓存装备属性
    /// </summary>
    void CacheProp()
    {
        // 缓存装备次要属性
        Dictionary<int, int> minorProp = new Dictionary<int, int>();

        foreach (LPCValue item in mMinorProp.Values)
            minorProp.Add(item.AsArray[0].AsInt, item.AsArray[1].AsInt);

        mCacheProp[EquipConst.MINOR_PROP] = minorProp;

    }

    /// <summary>
    /// 获取装备属性
    /// </summary>
    void GetEquipProp()
    {
        LPCMapping equipProp = mEquipOb.Query<LPCMapping>("prop");

        if (equipProp == null)
            return;

        // 附加属性
        mMinorProp = equipProp.GetValue<LPCArray>(EquipConst.MINOR_PROP);
        if (mMinorProp == null)
            mMinorProp = LPCArray.Empty;
    }

    /// <summary>
    /// 检测装备强化
    /// </summary>
    bool CheckIntensify()
    {
        // 强化等级
        int rank = mEquipOb.GetRank();

        if (mCurIntensifyCount >= GameSettingMgr.GetSettingInt("auto_intensify_count"))
        {
            SetAgainBtnState(true);
            return false;
        }

        // 自动强化次数达到上限或者已达到最大强化等级
        if (mBeforeRank + 1 > GameSettingMgr.GetSettingInt("equip_intensify_limit_level"))
        {
            // 抛出事件
            EventMgr.FireEvent(EventMgrEventType.EVENT_FAST_INTENFISY_COUNT_LIMIT, null);

            return false;
        }

        CsvRow row = BlacksmithMgr.IntensifyCsv.FindByKey(rank + 1);

        // 获取强化脚本编号
        int scriptNo = row.Query<int>("cost_script");

        LPCMapping args = new LPCMapping();

        args.Add("star", mEquipOb.Query<int>("star"));
        args.Add("rank", rank + 1);

        // 获取装备强化的消耗
        object ret = ScriptMgr.Call(scriptNo, args);

        if (ret == null)
        {
            LogMgr.Trace("没有获取到计算信息");
            return false;
        }

        LPCMapping costMap = ret as LPCMapping;

        string filed = FieldsMgr.GetFieldInMapping(costMap);

        if (costMap == null || string.IsNullOrEmpty(filed))
            return false;

        // 金钱不足
        if (ME.user.Query<int>(filed) < costMap.GetValue<int>(filed))
        {
            // 同步抛出事件
            EventMgr.FireEvent(EventMgrEventType.EVENT_EQUIP_INTENSIY_NO_MONEY, null, true);

            return false;
        }

        return true;
    }

    /// <summary>
    /// 设置再来一次按钮的状态
    /// </summary>
    void SetAgainBtnState(bool isActive)
    {
        if (isActive)
        {
            UIEventListener.Get(mAgainBtn.gameObject).onClick = OnClickAgainBtn;

            mAgainBtn.color = new Color(1f, 1f, 1f);
        }
        else
        {
            UIEventListener.Get(mAgainBtn.gameObject).onClick -= OnClickAgainBtn;

            float rgb = 125f / 255f;

            mAgainBtn.color = new Color(rgb, rgb, rgb);
        }
    }

    /// <summary>
    /// 刷新装备描述
    /// </summary>
    public void RefreshEquipDesc()
    {
        // 获取装备短描述;
        string shortDesc = mEquipOb.Short();

        // 获取颜色标签
        string colorLabel = ColorConfig.GetColor(mEquipOb.GetRarity());

        // 强化等级
        int rank = mEquipOb.GetRank();

        if (rank > 0)
            mTitle.text = string.Format("[{0}]+{1}{2}[-]", colorLabel, rank + "  ", shortDesc);
        else
            mTitle.text = string.Format("[{0}]{1}[-]", colorLabel, shortDesc);
    }

    /// <summary>
    /// 发送消息通知服务器执行强化操作
    /// </summary>
    public void SendMessage()
    {
        mBeforeRank = mEquipOb.GetRank();

        if (!CheckIntensify())
            return;

        if (!CheckLimit())
        {
            SetAgainBtnState(true);

            return;
        }

        // 构建参数
        LPCMapping cmdAgrs = new LPCMapping();

        cmdAgrs.Add("rid", mEquipOb.GetRid());
        Operation.CmdBlacksmithAction.Go("intensify", cmdAgrs);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(Property equipOb)
    {
        if (equipOb == null)
            return;

        // 装备对象
        mEquipOb = equipOb;

        mBeforeRank = mEquipOb.GetRank();

        // 创建预留使用的GameObject
        CreatedGameObject();

        // 注册事件
        RegisterEvent();

        // 获取装备属性
        GetEquipProp();

        // 缓存装备属性
        CacheProp();

        SetAgainBtnState(false);

        RefreshEquipDesc();
    }
}
