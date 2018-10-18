/// <summary>
/// TowerRaidersPetWnd.cs
/// Created by fengsc 2017/08/25
/// 通天之塔攻略使魔界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class TowerRaidersPetWnd : WindowBase<TowerRaidersPetWnd>
{
    #region 成员变量

    // 窗口标题
    public UILabel mTitle;

    // 攻略使魔基础格子
    public GameObject mItem;

    // 排序控件
    public UIGrid mGrid;

    // 无排名数据
    public UILabel mDesc;

    public UIScrollView mScrollView;

    // 当前选择的难度
    private int mDifficulty;

    // boss层
    private int mBossLayer = 0;

    // 使魔攻略排名数据
    private LPCMapping mTopData = LPCMapping.Empty;

    private List<GameObject> mItemList = new List<GameObject>();

    #endregion

    #region 内部接口

    void Awake()
    {
        // 注册事件
        RegisterEvent();

        // 创建一批缓存的基础格子
        CreatedGameObject();
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("TowerRaidersPetWnd");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 监听通天塔滑动事件
        EventMgr.RegisterEvent("TowerRaidersPetWnd", EventMgrEventType.EVENT_TOWER_SLIDE, OnTowerSlideEvent);

        // TODO:监听排行榜数据更新事件
        EventMgr.RegisterEvent("TowerRaidersPetWnd", EventMgrEventType.EVENT_GET_TOWER_PET_TOP_LIST, OnTowerPetTopEvent);
    }

    /// <summary>
    /// 获取宠物排行榜数据回调
    /// </summary>
    void OnTowerPetTopEvent(int eventId, MixedValue para)
    {
        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 通天塔滑动事件回调
    /// </summary>
    void OnTowerSlideEvent(int eventId, MixedValue para)
    {
        int bossLayer = para.GetValue<int>();
        if (bossLayer == mBossLayer)
            return;

        mBossLayer = bossLayer;

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 创建一批GameObject
    /// </summary>
    void CreatedGameObject()
    {
        mItem.SetActive(false);
        for (int i = 0; i < GameSettingMgr.GetSettingInt("tower_pet_top_show_max_pieces"); i++)
        {
            GameObject go = Instantiate(mItem);
            if (go == null)
                continue;

            go.transform.SetParent(mGrid.transform);

            go.transform.localPosition = Vector3.zero;

            go.transform.localScale = Vector3.one;

            UIEventListener.Get(go).onClick = OnClickItem;

            mItemList.Add(go);
        }
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 重置滑动位置
        mScrollView.ResetPosition();

        mTitle.text = string.Format(LocalizationMgr.Get("TowerRaidersPetWnd_2"), mBossLayer + 1);

        for (int i = 0; i < mItemList.Count; i++)
            mItemList[i].SetActive(false);

        // 根据难度获取对应层数的排名数据
        mTopData = TowerMgr.GetPetTopListByLayer(mDifficulty, mBossLayer);
        if (mTopData == null || mTopData.Count == 0)
        {
            mDesc.gameObject.SetActive(true);
            return;
        }

        mDesc.text = LocalizationMgr.Get("TowerRaidersPetWnd_1");

        mDesc.gameObject.SetActive(false);

        int sumTimes = mTopData.GetValue<int>("sum_times");

        // 排名数据
        LPCArray topData = mTopData.GetValue<LPCArray>("top_data");
        if (topData == null || topData.Count == 0)
        {
            mDesc.gameObject.SetActive(true);
            return;
        }

        for (int i = 0; i < topData.Count; i++)
        {
            LPCMapping top = topData[i].AsMapping;
            if (top == null)
                continue;

            GameObject go = mItemList[i];
            if (go == null)
                continue; 

            go.SetActive(true);

            TowerRaidersPetItemWnd script = go.GetComponent<TowerRaidersPetItemWnd>();
            if (script == null)
                continue;

            // 绑定数据
            script.Bind(topData, i + 1, sumTimes);
        }

        for (int j = topData.Count; j < mItemList.Count; j++)
        {
            // 隐藏多余的基础格子
            mItemList[j].SetActive(false);
        }

        mGrid.Reposition();
    }

    /// <summary>
    /// 基础格子点击事件
    /// </summary>
    void OnClickItem(GameObject go)
    {
        TowerRaidersPetItemWnd script = go.GetComponent<TowerRaidersPetItemWnd>();
        if (script == null)
            return;

        Property ob = script.mPetOb;
        if (ob == null)
            return;

        // 简要使魔信息弹框
        GameObject petWnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (petWnd == null)
            return;

        PetSimpleInfoWnd petSimpleInfoWnd = petWnd.GetComponent<PetSimpleInfoWnd>();
        if (petSimpleInfoWnd == null)
            return;

        petSimpleInfoWnd.Bind(ob, true);
        petSimpleInfoWnd.ShowBtn(true);
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(int difficulty, int bossLayer)
    {
        mDifficulty = difficulty;

        mBossLayer = bossLayer;

        // 请求排行榜数据
        if (TowerMgr.RequestPetTopList(mDifficulty))
            return;

        // 重绘窗口
        Redraw();
    }

    #endregion
}
