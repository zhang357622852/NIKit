/// <summary>
/// RecommendGangWnd.cs
/// Created by fengsc 2018/01/29
/// 推荐公会界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class RecommendGangWnd : WindowBase<RecommendGangWnd>
{
    #region 成员变量

    public UILabel mConditionTips;

    public UILabel mTips1;
    public UILabel mTips2;
    public UILabel mTips3;

    // 段位要求
    public GameObject mArenaRankBtn;
    public UILabel mArenaRankBtnLb;

    public GameObject mStars;

    public UISprite[] mArenaRankStars;

    // 是否需要申请条件
    public GameObject mApplicationConditionBtn;
    public UILabel mApplicationConditionBtnLb;

    public GameObject mSelectRankWnd;

    public GameObject mCloseSelectRankWnd;

    public GameObject mApplicationConditionWnd;

    public GameObject mCloseApplicationConditionWnd;

    // 段位条件基础格子
    public GameObject[] mRankConditionItem;

    // 不需要审核
    public GameObject mNoCheckBtn;
    public UILabel mNoCheckLb;

    // 需要审核
    public GameObject mNeedCheckBtn;
    public UILabel mNeedCheckLb;

    public GameObject mRefuseJionBtn;
    public UILabel mRefuseJionLb;

    // 推荐公会基础格子
    public GameObject mGangItemWnd;

    // 物体复用组件
    public UIWrapContent mUIWrapContent;

    // 没有数据提示
    public UILabel mNoRecommendTips;

    public UIScrollView mUIScrollView;

    public GangInfoWnd mGangInfoWnd;

    // 滑动条
    public UIScrollBar mUIScrollBar;

    // 载入更多公户列表按钮
    public UILabel mLoadGangListBtn;

    Dictionary<string, GameObject> mItems = new Dictionary<string, GameObject>();

    Dictionary<int, int> mIndexMap = new Dictionary<int, int>();

    // 推荐列表
    LPCArray mRecommendList = LPCArray.Empty;

    // 每行显示10个宠物格子,实例化十个元素进行复用
    private int mRowAmonut = 6;

    int mCheckCondition = -1;

    int mStep = -1;

    string mRelationTag = string.Empty;

    bool mIsLoad = false;

    #endregion

    // Use this for initialization
    void Awake ()
    {
        // 创建缓存的基础格子
        CreatedGameObject();

        // 初始化文本
        InitText();

        // 注册事件
        RegisterEvent();
    }

    void OnEnable()
    {
        // 绘制窗口
        Redraw();
    }

    void OnDestroy()
    {
        // 移除消息关注
        MsgMgr.RemoveDoneHook("MSG_NOTIFY_GANG_SUMMARY", "RecommendGangWnd");

        EventMgr.UnregisterEvent("RecommendGangWnd");
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mConditionTips.text = LocalizationMgr.Get("GangWnd_52");
        mTips1.text = LocalizationMgr.Get("CreateGuildWnd_12");
        mTips2.text = LocalizationMgr.Get("CreateGuildWnd_13");
        mTips3.text = LocalizationMgr.Get("GangWnd_53");
        mArenaRankBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_14");
        mApplicationConditionBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_14");
        mNoCheckLb.text = LocalizationMgr.Get("CreateGuildWnd_17");
        mNeedCheckLb.text = LocalizationMgr.Get("CreateGuildWnd_18");
        mRefuseJionLb.text = LocalizationMgr.Get("CreateGuildWnd_19");
        mLoadGangListBtn.text = LocalizationMgr.Get("GangWnd_57");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mNoCheckBtn).onClick = OnClickNoCheckBtn;
        UIEventListener.Get(mNeedCheckBtn).onClick = OnClickNeedCheckBtn;
        UIEventListener.Get(mRefuseJionBtn).onClick = OnClickRefuseJionBtn;
        UIEventListener.Get(mCloseSelectRankWnd).onClick = OnClickCloseSelectRankWnd;
        UIEventListener.Get(mCloseApplicationConditionWnd).onClick = OnClickCloseApplicationCondition;
        UIEventListener.Get(mArenaRankBtn).onClick = OnClickSelectRankConditinBtn;
        UIEventListener.Get(mApplicationConditionBtn).onClick = OnClickSelectCheckCondition;
        UIEventListener.Get(mLoadGangListBtn.gameObject).onClick = OnClickLoadGangList;

        for (int i = 0; i < mRankConditionItem.Length; i++)
            UIEventListener.Get(mRankConditionItem[i]).onClick = OnClickRankConditionItem;

        mUIWrapContent.onInitializeItem = UpdateItem;

        EventDelegate.Add(mUIScrollBar.onChange, OnScrollBarChange);

        // 关注MSG_NOTIFY_GANG_SUMMARY
        MsgMgr.RegisterDoneHook("MSG_NOTIFY_GANG_SUMMARY", "RecommendGangWnd", OnNotifyGangSummary);
    }

    /// <summary>
    /// 滑动条变化回调
    /// </summary>
    void OnScrollBarChange()
    {
        RefreshLoadGangListBtn();
    }

    /// <summary>
    /// 刷新按状态
    /// </summary>
    void RefreshLoadGangListBtn()
    {
        if (mUIScrollBar.value > 0.99f)
        {
            mLoadGangListBtn.gameObject.SetActive(true);
        }
        else
        {
            mLoadGangListBtn.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 获取公会列表消息回调
    /// </summary>
    void OnNotifyGangSummary(string cmd, LPCValue para)
    {
        if (!para.IsMapping)
            return;

        LPCMapping data = para.AsMapping;
        if (!data.ContainsKey("gang_list"))
            return;

        LPCArray list = data.GetValue<LPCArray>("gang_list");

        if (mIsLoad)
            mRecommendList.Append(list);
        else
            mRecommendList = list;

        // 刷新公会列表数据
        RefreshGangData();
    }

    /// <summary>
    /// UIWrapContent更新回调
    /// </summary>
    void UpdateItem(GameObject go, int wrapIndex, int realIndex)
    {
        // 将index与realindex对应关系记录下来
        if (!mIndexMap.ContainsKey(wrapIndex))
            mIndexMap.Add(wrapIndex, realIndex);
        else
            mIndexMap[wrapIndex] = realIndex;

        // 填充数据
        FillData(wrapIndex, realIndex);
    }

    /// <summary>
    /// 填充数据
    /// </summary>
    void FillData(int wrapIndex, int realIndex)
    {
        // 没有获取到推荐列表
        if (mRecommendList == null || mRecommendList.Count == 0)
            return;

        int index = Mathf.Abs(realIndex);

        if (index + 1 > mRecommendList.Count)
            return;

        GameObject item = mItems["gang_item_" + wrapIndex];

        if (item == null)
            return;

        if (!item.activeSelf)
            item.SetActive(true);

        GangItemWnd script = item.GetComponent<GangItemWnd>();
        if (script == null)
            return;

        LPCMapping data = mRecommendList[index].AsMapping;

        // 绑定数据
        script.Bind(data);

        string tempRelationTag = data.GetValue<string>("relation_tag");

        if (index == 0 && string.IsNullOrEmpty(mRelationTag))
        {
            mGangInfoWnd.Bind(data);

            mGangInfoWnd.gameObject.SetActive(true);

            mRelationTag = tempRelationTag;
        }

        if (mRelationTag == tempRelationTag)
            script.Select(true);
        else
            script.Select(false);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 段位条件列表
        List<int> rankCondition = CALC_CREATE_GUILD_RANK_CONDITION.Call();

        for (int i = 0; i < mRankConditionItem.Length; i++)
        {
            ConditionItemWnd script = mRankConditionItem[i].GetComponent<ConditionItemWnd>();
            if (script == null)
                continue;

            // 绑定数据
            script.Bind(rankCondition[i]);
        }

        // 默认筛选的条件
        mStep = -1;

        mCheckCondition = (int) CHECK_CONDITION.NEED_CHECK;

        RefreshRankCondition();

        RefreshCheckCondition();

        mRecommendList = LPCArray.Empty;

        // 获取公会列表
        GangMgr.NotifyGangSummary(mStep, mCheckCondition, 0);
    }

    /// <summary>
    /// 刷新公会数据
    /// </summary>
    void RefreshGangData()
    {
        mNoRecommendTips.gameObject.SetActive(false);

        RefreshLoadGangListBtn();

        if (!mIsLoad)
        {
            mIndexMap.Clear();

            for (int i = 0; i < mRowAmonut; i++)
                mIndexMap.Add(i, -i);

            mRelationTag = string.Empty;
        }

        if (mRecommendList.Count < mRowAmonut)
        {
            for (int i = 0; i < mRecommendList.Count; i++)
            {
                GameObject item = mItems["gang_item_" + i];

                item.SetActive(true);

                GangItemWnd script = item.GetComponent<GangItemWnd>();
                if (script == null)
                    continue;

                LPCMapping data = mRecommendList[i].AsMapping;

                // 绑定数据
                script.Bind(data);

                string tempRelationTag = data.GetValue<string>("relation_tag");

                if (i == 0 && string.IsNullOrEmpty(mRelationTag))
                {
                    mGangInfoWnd.Bind(data);

                    mGangInfoWnd.gameObject.SetActive(true);

                    mRelationTag = tempRelationTag;
                }

                if (mRelationTag == tempRelationTag)
                    script.Select(true);
                else
                    script.Select(false);
            }

            for (int i = mRecommendList.Count; i < mItems.Count; i++)
                mItems["gang_item_" + i].SetActive(false);

            if (mRecommendList.Count == 0)
            {
                mNoRecommendTips.gameObject.SetActive(true);

                mGangInfoWnd.Bind(LPCMapping.Empty);

                mGangInfoWnd.gameObject.SetActive(true);
            }

            return;
        }

        foreach (GameObject item in mItems.Values)
            item.SetActive(true);

        mUIWrapContent.minIndex = -(mRecommendList.Count > mRowAmonut ? mRecommendList.Count : mRowAmonut - 1);
        mUIWrapContent.maxIndex = 0;

        mUIWrapContent.enabled = true;

        if (mIndexMap.Count != 0)
        {
            // 填充数据
            foreach(KeyValuePair<int, int> kv in mIndexMap)
                FillData(kv.Key, kv.Value);
        }

        if (mIsLoad)
            return;

        mIsLoad = false;

        // 重置滑动区域
        mUIScrollView.ResetPosition();
    }

    /// <summary>
    /// 创建一批缓存GameObject
    /// </summary>
    void CreatedGameObject()
    {
        mGangItemWnd.SetActive(false);

        for (int i = 0; i < mRowAmonut; i++)
        {
            GameObject item = Instantiate(mGangItemWnd);
            item.transform.SetParent(mUIWrapContent.transform);
            item.transform.localScale = Vector3.one;
            item.transform.localPosition = new Vector3(0, -i * mUIWrapContent.itemSize, 0);
            item.name = "gang_item_" + i;

            // 缓存
            mItems.Add(item.name, item);

            UIEventListener.Get(item).onClick = OnClickGangItemWnd;
        }
    }

    /// <summary>
    /// 刷新审核条件显示
    /// </summary>
    void RefreshCheckCondition()
    {
        switch (mCheckCondition)
        {
            case (int) CHECK_CONDITION.NO_CHECK:
                mApplicationConditionBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_17");
                break;

            case (int) CHECK_CONDITION.NEED_CHECK:
                mApplicationConditionBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_18");
                break;

            case (int) CHECK_CONDITION.REFUSE_JION:
                mApplicationConditionBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_19");
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 刷新段位条件显示
    /// </summary>
    void RefreshRankCondition()
    {
        mArenaRankBtnLb.gameObject.SetActive(false);

        mStars.SetActive(false);

        if (mStep == -1)
        {
            mArenaRankBtnLb.gameObject.SetActive(true);
            mArenaRankBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_20");
            return;
        }

        mStars.SetActive(true);

        CsvRow row = ArenaMgr.TopBonusCsv.FindByKey(mStep);
        if (row == null)
            return;

        for (int i = 0; i < mArenaRankStars.Length; i++)
            mArenaRankStars[i].spriteName = "arena_star_bg";

        for (int i = 0; i < row.Query<int>("star"); i++)
            mArenaRankStars[i].spriteName = row.Query<string>("star_name");
    }

    /// <summary>
    /// 载入更多公会列表按钮点击事件回调
    /// </summary>
    void OnClickLoadGangList(GameObject go)
    {
        mLoadGangListBtn.gameObject.SetActive(false);

        mIsLoad = true;

        // 获取公会列表
        GangMgr.NotifyGangSummary(mStep, mCheckCondition, mRecommendList.Count);
    }

    /// <summary>
    /// 公会格子点击事件
    /// </summary>
    void OnClickGangItemWnd(GameObject go)
    {
        GangItemWnd script = go.GetComponent<GangItemWnd>();
        if (script == null)
            return;

        LPCMapping data = script.mGangData;

        string tempRelationTag = data.GetValue<string>("relation_tag");

        if (mRelationTag == tempRelationTag)
            return;

        mRelationTag = tempRelationTag;

        // 刷新选中状态
        foreach (GameObject item in mItems.Values)
        {
            GangItemWnd itemScript = item.GetComponent<GangItemWnd>();
            if (script == null)
                continue;

            if (mRelationTag == itemScript.mGangData.GetValue<string>("relation_tag"))
                itemScript.Select(true);
            else
                itemScript.Select(false);
        }

        // 绑定数据
        mGangInfoWnd.Bind(data);

        mGangInfoWnd.gameObject.SetActive(true);
    }

    /// <summary>
    /// 段位条件基础格子点击事件
    /// </summary>
    void OnClickRankConditionItem(GameObject go)
    {
        ConditionItemWnd script = go.GetComponent<ConditionItemWnd>();
        if (script == null)
            return;

        mStep = script.mStep;

        mSelectRankWnd.SetActive(false);

        // 刷新段位条件显示
        RefreshRankCondition();

        // 获取公会列表
        GangMgr.NotifyGangSummary(mStep, mCheckCondition, 0);
    }

    /// <summary>
    /// 审核条件按钮点击事件回调
    /// </summary>
    void OnClickSelectCheckCondition(GameObject go)
    {
        mSelectRankWnd.SetActive(false);
        mApplicationConditionWnd.SetActive(true);

        // 默认选项
        if (mCheckCondition < 0)
            mCheckCondition = (int) CHECK_CONDITION.NO_CHECK;

        switch (mCheckCondition)
        {
            case (int) CHECK_CONDITION.NO_CHECK:
                mNoCheckBtn.GetComponent<UIToggle>().Set(true);
                break;

            case (int) CHECK_CONDITION.NEED_CHECK:
                mNeedCheckBtn.GetComponent<UIToggle>().Set(true);
                break;

            case (int) CHECK_CONDITION.REFUSE_JION:
                mRefuseJionBtn.GetComponent<UIToggle>().Set(true);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 段位选择按钮点击事件回调
    /// </summary>
    void OnClickSelectRankConditinBtn(GameObject go)
    {
        mSelectRankWnd.SetActive(true);
        mApplicationConditionWnd.SetActive(false);

        // 默认选项
        if (mStep < -1)
            mStep = -1;

        for (int i = 0; i < mRankConditionItem.Length; i++)
        {
            ConditionItemWnd script = mRankConditionItem[i].GetComponent<ConditionItemWnd>();
            if (script == null)
                continue;

            if (script.mStep != mStep)
                continue;

            mRankConditionItem[i].GetComponent<UIToggle>().Set(true);
        }
    }

    /// <summary>
    /// 申请条件窗口关闭点击回调
    /// </summary>
    void OnClickCloseApplicationCondition(GameObject go)
    {
        mApplicationConditionWnd.SetActive(false);

        RefreshCheckCondition();
    }

    /// <summary>
    /// 直接加入按钮点击回调
    /// </summary>
    void OnClickNoCheckBtn(GameObject go)
    {
        mCheckCondition = (int) CHECK_CONDITION.NO_CHECK;

        mApplicationConditionWnd.SetActive(false);

        // 刷新审核条件显示
        RefreshCheckCondition();

        // 获取公会列表
        GangMgr.NotifyGangSummary(mStep, mCheckCondition, 0);
    }

    /// <summary>
    /// 需要审核按钮点击回调
    /// </summary>
    void OnClickNeedCheckBtn(GameObject go)
    {
        mCheckCondition = (int) CHECK_CONDITION.NEED_CHECK;

        mApplicationConditionWnd.SetActive(false);

        // 刷新审核条件显示
        RefreshCheckCondition();

        // 获取公会列表
        GangMgr.NotifyGangSummary(mStep, mCheckCondition, 0);
    }

    /// <summary>
    /// 拒绝加入按钮点击回调
    /// </summary>
    void OnClickRefuseJionBtn(GameObject go)
    {
        mCheckCondition = (int) CHECK_CONDITION.REFUSE_JION;

        mApplicationConditionWnd.SetActive(false);

        // 刷新审核条件显示
        RefreshCheckCondition();

        // 获取公会列表
        GangMgr.NotifyGangSummary(mStep, mCheckCondition, 0);
    }

    /// <summary>
    /// 段位选择窗口
    /// </summary>
    void OnClickCloseSelectRankWnd(GameObject go)
    {
        mSelectRankWnd.SetActive(false);

        RefreshRankCondition();
    }
}
