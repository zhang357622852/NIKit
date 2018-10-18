/// <summary>
/// DungeonsDropGoodsWnd.cs
/// Created by fengsc 2017/01/06
/// 地下城掉落物品查看窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class DungeonsDropGoodsWnd : WindowBase<DungeonsDropGoodsWnd>
{
    // 窗口标题
    public UILabel mTitle;

    // 关闭按钮
    public GameObject mCloseBtn;

    // 不规则排序组件
    public UITable mTable;

    public GameObject mItem;

    public GameObject mTypeItem;

    public GameObject mDescItem;

    // 确认按钮点击事件
    public GameObject mConfirmBtn;

    public GameObject mMask;

    public Property mPropOb = null;

    public TweenAlpha mTweenAlpha;

    public TweenScale mTweenScale;

    // 地图id
    int mMapId = 0;

    // 附加参数
    LPCMapping mExtraPara = LPCMapping.Empty;

    // Use this for initialization
    void Start ()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mMask).onClick = OnClickCloseBtn;
        UIEventListener.Get(mConfirmBtn).onClick = OnClickCloseBtn;

        if (mTweenAlpha == null || mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

        // 播放动画
        mTweenScale.PlayForward();

        mTweenAlpha.PlayForward();

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
        if (mPropOb != null)
            mPropOb.Destroy();
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
        // 获取地图的配置信息
        CsvRow mapConfig = MapMgr.GetMapConfig(mMapId);

        if (mapConfig == null)
            return;

        // 界面标题
        mTitle.text = string.Format(LocalizationMgr.Get("DungeonsDropGoodsWnd_3"),
            LocalizationMgr.Get(mapConfig.Query<string>("name")));

        // 获取地图的通关奖励数据
        LPCMapping clearanceBonus = MapMgr.GetMapClearanceBonus(mMapId, mExtraPara);

        if (clearanceBonus == null)
            return;

        // 排序组件
        UIGrid grid = null;
        UILabel mDropTitle = null;

        foreach (string key in clearanceBonus.Keys)
        {
            if (key == null)
                continue;

            GameObject typeItem = null;

            if (key.Equals("desc"))
            {
                typeItem = Instantiate(mDescItem);
            }
            else
            {
                typeItem = Instantiate(mTypeItem);

                grid = typeItem.transform.Find("Grid").GetComponent<UIGrid>();
            }

            typeItem.transform.SetParent(mTable.transform);
            typeItem.transform.localPosition = Vector3.zero;
            typeItem.transform.localScale = Vector3.one;
            mDropTitle = typeItem.transform.Find("title").GetComponent<UILabel>();
            typeItem.SetActive(true);

            if (key.Equals("suit_id"))
            {
                mDropTitle.text = LocalizationMgr.Get("DungeonsDropGoodsWnd_1");

                foreach (LPCValue value in clearanceBonus.GetValue<LPCArray>(key).Values)
                {
                    if (value == null || !value.IsInt)
                        continue;

                    GameObject go = Instantiate(mItem).gameObject;

                    go.transform.SetParent(grid.transform);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localScale = Vector3.one;
                    go.SetActive(true);

                    LPCMapping data = LPCMapping.Empty;
                    data.Add("suit_id", value.AsInt);

                    // 绑定数据
                    go.GetComponent<DungeonsDropGoodsItemWnd>().Bind(data);

                    UIEventListener.Get(go).onClick = OnClickItem;
                }

                // 排序子控件
                grid.repositionNow = true;
            }
            if (key.Equals("item_id"))
            {
                mDropTitle.text = LocalizationMgr.Get("DungeonsDropGoodsWnd_2");
                foreach (LPCValue value in clearanceBonus.GetValue<LPCArray>(key).Values)
                {
                    if (value == null || !value.IsInt)
                        continue;

                    GameObject go = Instantiate(mItem).gameObject;

                    go.transform.SetParent(grid.transform);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localScale = Vector3.one;
                    go.SetActive(true);

                    int classId = value.AsInt;

                    LPCMapping data = LPCMapping.Empty;
                    data.Add("class_id", classId);

                    DungeonsDropGoodsItemWnd script = go.GetComponent<DungeonsDropGoodsItemWnd>();
                    if (script == null)
                        continue;

                    // 绑定数据
                    script.Bind(data);

                    if (MonsterMgr.IsMonster(classId))
                    {
                        script.ShowSub(true);
                    }
                    else
                    {
                        script.ShowSub(false);
                    }

                    UIEventListener.Get(go).onClick = OnClickItem;
                }

                // 排序子控件
                grid.repositionNow = true;
            }
            if (key.Equals("desc"))
            {
                mDropTitle.text = LocalizationMgr.Get("DungeonsDropGoodsWnd_5");

                LPCArray array = clearanceBonus.GetValue<LPCArray>(key);
                if (array == null)
                    continue;

                UILabel desc = typeItem.transform.Find("desc").GetComponent<UILabel>();

                for (int i = 0; i < array.Count; i++)
                    desc.text += LocalizationMgr.Get(array[i].AsString);
            }
        }

        mTable.repositionNow = true;
    }

    /// <summary>
    /// 物品格子点击事件
    /// </summary>
    void OnClickItem(GameObject go)
    {
        DungeonsDropGoodsItemWnd script = go.GetComponent<DungeonsDropGoodsItemWnd>();

        if (script == null)
            return;

        LPCMapping data = script.mData;

        if (data == null)
            return;

        if (data.ContainsKey("suit_id"))
        {
            GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

            if (wnd == null)
                return;

            RewardItemInfoWnd itemOb = wnd.GetComponent<RewardItemInfoWnd>();

            if (itemOb == null)
                return;

            itemOb.SetSuitData(data, true, false, LocalizationMgr.Get("DungeonsDropGoodsWnd_4"));
            itemOb.SetMask(true);
        }
        else
        {
            LPCMapping para = LPCMapping.Empty;
            para.Add("class_id", data.GetValue<int>("class_id"));
            para.Add("rid", Rid.New());

            if (mPropOb != null)
                mPropOb.Destroy();
            
            mPropOb = PropertyMgr.CreateProperty(para);

            if (MonsterMgr.IsMonster(mPropOb))
            {
                GameObject petWnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (petWnd == null)
                    return;

                PetSimpleInfoWnd petSimpleInfoWnd = petWnd.GetComponent<PetSimpleInfoWnd>();
                if (petSimpleInfoWnd == null)
                    return;

                petSimpleInfoWnd.Bind(mPropOb, true);
                petSimpleInfoWnd.ShowBtn(true);
            }
            else if (ItemMgr.IsItem(mPropOb))
            {
                GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

                if (wnd == null)
                    return;

                RewardItemInfoWnd itemOb = wnd.GetComponent<RewardItemInfoWnd>();

                if (itemOb == null)
                    return;

                itemOb.SetPropData(mPropOb, true, false, LocalizationMgr.Get("DungeonsDropGoodsWnd_4"));
                itemOb.SetMask(true);
            }
        }
    }

    /// <summary>
    /// 窗口关闭界面
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 销毁当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(int mapId, LPCMapping para)
    {
        if (mapId < 0)
            return;

        mMapId = mapId;

        mExtraPara = para;

        Redraw();
    }
}
