/// <summary>
/// LevelBonusWnd.cs
/// Created by lic 11/14/2017
/// 等级奖励窗口
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class LevelBonusWnd : WindowBase<LevelBonusWnd>
{
    #region 成员变量

    public GameObject[] mLevelBonusItems;

    public GameObject mReceiveBtn;
    public UILabel mReceiveLb;
    public GameObject mReceiveBtnCover;

    public GameObject mCloseBtn;
    public UILabel mTitle;

    public GameObject mPetModel;

    public GameObject mDialog;
    public UILabel mDialogLb;

    #endregion

    #region 私有变量

    Dictionary<int, GameObject> mBonusItemsDic = new Dictionary<int, GameObject>();

    int mReceiveLevel = -1;

    // 窗口模型使用觉醒的光九尾，写死
    const int modelId = 3124;

    // 某型点击次数
    int sumClickTimes = 0;
    int clickTimes = 0;

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        //初始化窗口
        InitWnd();

        // 刷新界面
        Redraw();
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        Coroutine.StopCoroutine("SyncAutoShowDialog");
        Coroutine.StopCoroutine("SyncShowDialog");

        // 取消消息关注
        MsgMgr.RemoveDoneHook("MSG_RECEIVE_LEVEL_BONUS", "LevelBonusWnd");

        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 移除属性字段关注回调
        ME.user.dbase.RemoveTriggerField("LevelBonusWnd");
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        List<CsvRow> bonusList = CommonBonusMgr.GetBonusList(CommonBonusMgr.LEVEL_BONUS);

        if (bonusList == null || bonusList.Count == 0)
            return;

        for (int i = 0; i < mLevelBonusItems.Length; i++)
        {
            if (i >= bonusList.Count)
            {
                mLevelBonusItems[i].SetActive(false);
                continue;
            }

            mLevelBonusItems[i].SetActive(true);

            int level = bonusList[i].Query<int>("id");

            mLevelBonusItems[i].GetComponent<LevelBonusItem>().BindData(bonusList[i].Query<LPCArray>("bonus_list"), level);

            if (!mBonusItemsDic.ContainsKey(level))
                mBonusItemsDic.Add(level, mLevelBonusItems[i]);
        }

        RedrawSelect();
    }

    /// <summary>
    /// 刷新奖励选中
    /// </summary>
    private void RedrawSelect()
    {
        mReceiveLevel = -1;

        foreach (int level in mBonusItemsDic.Keys)
        {
            GameObject bounsItem = mBonusItemsDic[level];

            bounsItem.GetComponent<LevelBonusItem>().SetSelect(false);

            if (CommonBonusMgr.IsReceivedLevleBonus(ME.user, level))
            {
                bounsItem.GetComponent<LevelBonusItem>().SetState(RECEIVE_STATE.RECEIVED);
                continue;
            }

            if (CommonBonusMgr.CanReceiveLevelBonus(ME.user, level))
            {
                if (mReceiveLevel == -1 ||
                    ( mReceiveLevel != -1 && level < mReceiveLevel))
                    mReceiveLevel = level;

                bounsItem.GetComponent<LevelBonusItem>().SetState(RECEIVE_STATE.RECEIVING);
                continue;
            }

            bounsItem.GetComponent<LevelBonusItem>().SetState(RECEIVE_STATE.UNCOMPLETE);
        }

        if (mReceiveLevel == -1)
        {
            mReceiveBtnCover.SetActive(true);
            return;
        }

        mReceiveBtnCover.SetActive(false);
        mBonusItemsDic[mReceiveLevel].GetComponent<LevelBonusItem>().SetSelect(true);
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        // 本地化文字
        mTitle.text = LocalizationMgr.Get("LevelBonusWnd_1");
        mReceiveLb.text = LocalizationMgr.Get("LevelBonusWnd_2");

        mDialog.SetActive(false);
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;
        UIEventListener.Get(mReceiveBtn).onClick = OnReceiveBtn;

        foreach (GameObject item in mLevelBonusItems)
            UIEventListener.Get(item).onClick = OnBonusItemBtn;

        gameObject.GetComponent<TweenScale>().AddOnFinished(OnTweenFinished);

        // 关注消息
        MsgMgr.RegisterDoneHook("MSG_RECEIVE_LEVEL_BONUS", "LevelBonusWnd", OnMsgReceiveLevelBonus);

        if (ME.user == null)
            return; 

        ME.user.dbase.RegisterTriggerField("LevelBonusWnd", new string[] {"level_bonus"}, new CallBack (OnLevelBonusChange));
    }

    /// <summary>
    /// TweenAlpha结束的回调
    /// </summary>
    private void OnTweenFinished()
    {
        ShowModel();

        Coroutine.DispatchService(SyncAutoShowDialog(), "SyncAutoShowDialog");
    }

    /// <summary>
    /// 播放小光效
    /// </summary>
    /// <returns>The wait sever.</returns>
    IEnumerator SyncAutoShowDialog()
    {
        yield return new WaitForSeconds(0.5f);

        LPCMapping extraPara = LPCMapping.Empty;
        extraPara.Add("show_times", 0);

        string tips = SystemTipsMgr.FetchTip("level_bonus", extraPara);

        if (string.IsNullOrEmpty(tips))
            yield break;

        Coroutine.DispatchService(SyncShowDialog(tips, 2f), "SyncShowDialog");

        yield return new WaitForSeconds(1.5f);

        extraPara = LPCMapping.Empty;
        extraPara.Add("show_times", 1);

        tips = SystemTipsMgr.FetchTip("level_bonus", extraPara);

        if (string.IsNullOrEmpty(tips))
            yield break;

        Coroutine.StopCoroutine("SyncShowDialog");
        Coroutine.DispatchService(SyncShowDialog(tips, 2f), "SyncShowDialog");
    }

    /// <summary>
    /// 领取按钮点击事件
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnReceiveBtn(GameObject ob)
    {
        // 没有需要领取的奖励
        if (mReceiveLevel == -1)
            return;

        // 通知服务器
        Operation.CmdReceiveLevelBonus.Go(mReceiveLevel);
    }

    /// <summary>
    /// 领取按钮点击事件
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnBonusItemBtn(GameObject ob)
    {
        LevelBonusItem item = ob.GetComponent<LevelBonusItem>();

        if (item == null)
            return;

        // 已经是选中状态
        if (item.IsSelected)
            return;

        // 非待领取状态
        if (item.State != RECEIVE_STATE.RECEIVING)
            return;

        int level = item.Level;

        if (!mBonusItemsDic.ContainsKey(level))
            return;

        // 如果之前有选中，则将之前的选中去掉
        if (mReceiveLevel != -1 && mBonusItemsDic.ContainsKey(mReceiveLevel))
            mBonusItemsDic[mReceiveLevel].GetComponent<LevelBonusItem>().SetSelect(false);

        mBonusItemsDic[level].GetComponent<LevelBonusItem>().SetSelect(true);

        mReceiveLevel = level;
    }

    /// <summary>
    /// 等级奖励变化回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="_params">Parameters.</param>
    void OnLevelBonusChange(object para, params object[] _params)
    {
        RedrawSelect();
    }

    /// <summary>
    /// 消息回调
    /// </summary>
    void OnMsgReceiveLevelBonus(string cmd, LPCValue para)
    {
        LPCMapping args = para.AsMapping;
        int level = args.GetValue<int>("level");

        // 弹出领取奖励弹框
        DialogMgr.ShowSingleBtnDailog(
            null,
            LocalizationMgr.Get("LevelBonusWnd_4"),
            string.Format(LocalizationMgr.Get("LevelBonusWnd_3"), level),
            string.Empty,
            true,
            this.transform
        );
    }

    /// <summary>
    /// 显示模型
    /// </summary>
    void ShowModel()
    {
        // 获取窗口绑定的模型窗口组件
        ModelWnd pmc = mPetModel.GetComponent<ModelWnd>();

        // 没有绑定模型窗口组件
        if (pmc == null)
            return;

        // 异步载入模型
        pmc.LoadModelSync(modelId, 2, LayerMask.NameToLayer("UI"), new CallBack(OnClickModel));
    }

    /// <summary>
    /// 点击模型回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="_params">Parameters.</param>
    void OnClickModel(object para, params object[] _params)
    {
        // 累计点击次数, 防止越界
        if (sumClickTimes != ConstantValue.MAX_VALUE)
            sumClickTimes++;

        // 累计当期点击次数
        clickTimes++;

        LPCMapping extraPara = LPCMapping.Empty;
        extraPara.Add("sum_times", sumClickTimes);
        extraPara.Add("times", clickTimes);

        // 抽取一条信息
        string tips = SystemTipsMgr.FetchTip("level_bonus", extraPara);
        if (string.IsNullOrEmpty(tips))
            return;

        // 重置累计点击次数
        clickTimes = 0;

        mDialog.GetComponent<TweenAlpha>().ResetToBeginning();
        mDialog.GetComponent<TweenAlpha>().enabled = false;

        Coroutine.StopCoroutine("SyncAutoShowDialog");
        Coroutine.StopCoroutine("SyncShowDialog");
        Coroutine.DispatchService(SyncShowDialog(tips, 2f), "SyncShowDialog");
    }

    /// <summary>
    /// 播放小光效
    /// </summary>
    /// <returns>The wait sever.</returns>
    IEnumerator SyncShowDialog(string tips, float showTime)
    {
        mDialog.SetActive(true);
        mDialog.GetComponent<TweenAlpha>().ResetToBeginning();
        mDialogLb.text = tips;

        yield return new WaitForSeconds(showTime);

        mDialog.GetComponent<TweenAlpha>().PlayForward();
    }

    #endregion
}
