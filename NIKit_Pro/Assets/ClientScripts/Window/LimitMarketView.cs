/// <summary>
/// LimitMarketView.cs
/// Created by fengsc 2017/12/11
/// 限时商城物品查看窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class LimitMarketView : WindowBase<LimitMarketView>
{
    // 查看窗口背景
    public UISprite mBg;

    // NGUI表格排序组件
    public UITable mTable;

    // 显示商品查看基础格子
    public GameObject mItem;

    public GameObject mMask;

    GameObject mTarget;

    // 上下边距
    int mTopBottomBorder = 17;

    int mIconWidth = 80;

    List<GameObject> mGoList = new List<GameObject>();

    bool mIsOpen = false;

    void Start()
    {
        // 注册点击事件
        UIEventListener.Get(mMask).onClick = OnClickMaskBtn;

        if (ME.user == null)
            return;

        // 关注字段变化
        ME.user.dbase.RegisterTriggerField("LimitMarketView", new string[]{"dynamic_gift", "intensify_gift", "limit_buy_data"}, new CallBack(OnFieldsChange));
    }

    void OnDestroy()
    {
        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField("LimitMarketView");
    }

    /// <summary>
    /// 字段变化回调
    /// </summary>
    void OnFieldsChange(object para, params object[] param)
    {
        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mItem.SetActive(false);

        if (ME.user == null)
            return;

        LPCArray allLimitList = LPCArray.Empty;

        LPCArray dynamicGift = MarketMgr.GetLimitMarketList(ME.user);
        if (dynamicGift == null)
            dynamicGift = LPCArray.Empty;

        bool isExistWeekendGift = false;

        foreach (LPCValue id in dynamicGift.Values)
        {
            // 过滤掉不需要显示的动态礼包
            if (!MarketMgr.IsShow(ME.user, id.AsInt))
                continue;

            // 筛选周末特惠礼包，多个周末特惠礼包，入口只要一个
            if (MarketMgr.IsWeekendGift(id.AsInt))
            {
                if (isExistWeekendGift)
                    continue;
                else
                    isExistWeekendGift = true;
            }

            allLimitList.Add(id.AsInt);
        }

        // 元素强化限时礼包列表
        LPCArray gift =  MarketMgr.GetLimitStrengthList(ME.user);

        if (gift != null)
        {
            for (int i = 0; i < gift.Count; i++)
            {
                int classId = gift[i].AsMapping["class_id"].AsInt;

                // 过滤掉不需要显示的动态礼包
                if (!MarketMgr.IsShow(ME.user, classId))
                    continue;

                allLimitList.Add(classId);
            }
        }

        if (allLimitList == null || allLimitList.Count == 0)
        {
            if (this != null)
                WindowMgr.DestroyWindow(gameObject.name);

            return;
        }

        int amount = allLimitList.Count - mGoList.Count;

        for (int i = 0; i < amount; i++)
        {
            GameObject go = Instantiate(mItem);

            go.transform.SetParent(mTable.transform);

            go.transform.localScale = Vector3.one;

            go.transform.localPosition = Vector3.zero;

            go.SetActive(true);

            mGoList.Add(go);
        }

        if (amount < 0)
        {
            // 隐藏多余的基础格子
            for (int i = allLimitList.Count; i < mGoList.Count; i++)
            {
                Destroy(mGoList[i]);
                mGoList.RemoveAt(i);
            }
        }

        // 获取显示商品列表
        for (int i = 0; i < allLimitList.Count; i++)
        {
            GameObject go = mGoList[i];
            if (!go.activeSelf)
                go.SetActive(true);

            LimitMarketItem script = go.GetComponent<LimitMarketItem>();
            if (script == null)
                continue;

            // 绑定数据
            script.Bind(allLimitList[i].AsInt);

            // 注册点击事件
            UIEventListener.Get(go).onClick = OnClickItem;
        }

        // 排序子物体
        mTable.Reposition();

        UITexture bg = mTarget.GetComponent<UITexture>();
        if (bg == null)
            return;

        float xOffset = 8.0f;

        Vector3 worldTarget = transform.parent.InverseTransformPoint(mTarget.transform.position);
        this.transform.localPosition = new Vector3(
            worldTarget.x - bg.width *0.5f - xOffset,
            worldTarget.y + bg.height * 0.5f,
            worldTarget.z);

        int maskWidth = 0;
        int maskHeight = 0;
        int tempWidth = 0;
        int group = 0;
        for (int i = 0; i < mGoList.Count; i++)
        {
            if (group == (i / 3))
                tempWidth += mGoList[i].GetComponent<LimitMarketItem>().GetItemWidth();
            else
            {
                group = (i / 3);
                maskWidth = Mathf.Max(maskWidth, tempWidth);
                tempWidth = mGoList[i].GetComponent<LimitMarketItem>().GetItemWidth();
            }
        }

        maskWidth = Mathf.Max(maskWidth, tempWidth) + ((group > 0 ? 3 : mGoList.Count) * 24) + 26;
        maskHeight = (mIconWidth + mTopBottomBorder * 2 + 30) * (group + 1);

        mBg.width = (int) maskWidth;

        mBg.height = (int) maskHeight;
    }

    /// <summary>
    /// mask按钮点击事件
    /// </summary>
    void OnClickMaskBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 显示商品基础格子点击事件回调
    /// </summary>
    void OnClickItem(GameObject go)
    {
        if (mIsOpen)
            return;

        mIsOpen = true;

        LimitMarketItem script = go.GetComponent<LimitMarketItem>();
        if (script == null)
        {
            mIsOpen = false;

            return;
        }

        // 限时商品id
        int classId = script.mClassId;
        if (classId == 0)
        {
            mIsOpen = false;

            return;
        }

        // 打开商城界面
        if (MarketMgr.IsWeekendGift(classId))
        {
            // 周末特惠礼包
            List<int> giftList = MarketMgr.GetValidWeekendGifts(ME.user);

            if (giftList.Count < 2)
            {
                mIsOpen = false;

                return;
            }

            GameObject wnd = WindowMgr.OpenWnd(WeekendGiftWnd.WndType);
            if (wnd == null)
            {
                mIsOpen = false;

                return;
            }

            wnd.GetComponent<WeekendGiftWnd>().BindData(classId);
        }
        else
        {
            GameObject wnd = WindowMgr.OpenWnd(LimitGiftBagWnd.WndType);
            if (wnd == null)
            {
                mIsOpen = false;

                return;
            }

            wnd.GetComponent<LimitGiftBagWnd>().Bind(classId);
        }

        mIsOpen = false;
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(GameObject go)
    {
        mTarget = go;

        // 绘制窗口
        Redraw();
    }
}
