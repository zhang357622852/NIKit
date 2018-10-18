/// <summary>
/// ArenaBattleListWnd.cs
/// Created by fengsc 2016/09/26
/// 竞技场排位战对战列表窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ArenaBattleListWnd : WindowBase<ArenaBattleListWnd>
{
    #region 成员变量

    // 列表格子对象;
    public GameObject mItem;

    public GameObject mWrapContent;

    // 刷新列表按钮
    public GameObject mRefershListBtn;
    public UILabel mRefershListBtnLb;

    // 当前连胜次数
    public UILabel mWinsAmount;

    // 连胜buff剩余的有效时间
    public UILabel mTimer;

    // 下一次胜利的连胜次数
    public UILabel mNextWinsAmount;

    // 刷新提示
    public UILabel mRefreshTips;

    // 刷新计时器
    public UILabel mRefreshTime;

    public GameObject mArenaWnd;

    /// <summary>
    /// The m sroll view.
    /// </summary>
    public UIScrollView mSrollView;

    // 是否开启倒计时
    bool mEnableCountDown = false;

    // 上一次更新的时间
    float mLastUpdateTime = 0;

    // 剩余时间
    int mRemainTime = 0;

    // 缓存创建的复用格子;
    Dictionary<string, GameObject> mItemDic = new Dictionary<string, GameObject>();

    // 对战列表数据;
    LPCArray mBattleList = new LPCArray();

    // 格子限定个数
    int mLimitAmonut = 10;

    // 剩余刷新时间
    int mRefreshRemainTime = 0;

    string mInstanceId = "arena";

    #endregion

    void Awake()
    {
        CreateObject();
    }

    void OnEnable()
    {
        // 注册事件
        RegisterEvent();

        // 整理位置
        mSrollView.ResetPosition();

        // 绘制窗口
        Redraw();

        // 刷新连胜buff
        RefreshWinsBuff();
    }

    void Start()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mRefershListBtn).onClick = OnClickRefreshBtn;

        mRefershListBtnLb.text = LocalizationMgr.Get("RankingBattleWnd_1");

        mRefreshTips.text = LocalizationMgr.Get("RankingBattleWnd_23");
    }

    void Update()
    {
        if (mEnableCountDown)
        {
            if (Time.realtimeSinceStartup > mLastUpdateTime + 1)
            {
                mLastUpdateTime = Time.realtimeSinceStartup;

                // 每秒钟调用一次
                CountDown();
            }
        }
    }

    /// <summary>
    /// 列表刷新倒计时
    /// </summary>
    void RefreshCountDown()
    {
        if (mRefreshRemainTime < 60)
        {
            // 取消调用
            CancelInvoke("RefreshCountDown");

            mRefreshTime.text = TimeMgr.ConvertTimeToChineseTimer(0, false);

            return;
        }

        // 剩余时间
        mRefreshTime.text = TimeMgr.ConvertTimeToChineseTimer(mRefreshRemainTime, false);

        mRefreshRemainTime--;
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册消息回调
        // 关注MSG_GET_ARENA_OPPONENT_DEFENSE_DATA消息
        MsgMgr.RegisterDoneHook("MSG_GET_ARENA_OPPONENT_DEFENSE_DATA", "ArenaBattleListWnd", OnMsgGetArenaOpponentDefenseData);

        // 注册字段变化的事件
        if (ME.user != null)
        {
            ME.user.dbase.RegisterTriggerField("ArenaBattleListWnd",
                new string[] { "arena_opponent" }, new CallBack(OnArenaOpponentFieldChange));
        }
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDisable()
    {
        // 取消调用
        CancelInvoke("RefreshCountDown");

        // 移除消息监听
        MsgMgr.RemoveDoneHook("MSG_GET_ARENA_OPPONENT_DEFENSE_DATA", "ArenaBattleListWnd");

        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 解注册事件
        ME.user.dbase.RemoveTriggerField("ArenaBattleListWnd");
    }

    /// <summary>
    /// 请求对手防御信息,消息回调
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    void OnMsgGetArenaOpponentDefenseData(string cmd, LPCValue para)
    {
        // 获取防守信息
        LPCMapping args = para.AsMapping;
        if (args == null)
            return;

        LPCMapping defenseData = args.GetValue<LPCMapping>("opponent_data");
        if (defenseData == null)
            return;

        // 进入到选择战斗界面
        GameObject wnd = WindowMgr.OpenWnd(SelectFighterWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (wnd == null)
        {
            LogMgr.Trace("SelectFighterWnd窗口创建失败");
            return;
        }

        // 窗口绑定对象
        wnd.GetComponent<SelectFighterWnd>().Bind(mArenaWnd.name, mInstanceId, defenseData);
    }

    /// <summary>
    /// 倒计时
    /// </summary>
    void CountDown()
    {
        if (mRemainTime < 0)
        {
            mEnableCountDown = false;

            // 刷新连胜buff
            RefreshWinsBuff();

            return;
        }

        mTimer.text = string.Format(LocalizationMgr.Get("RankingBattleWnd_17"), TimeMgr.ConvertTime(mRemainTime));

        mRemainTime--;
    }

    /// <summary>
    /// 字段变化的回调
    /// </summary>
    void OnArenaOpponentFieldChange(object para, params object[] _params)
    {
        // 刷新连胜buff
        RefreshWinsBuff();

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 刷新列表按钮点击事件
    /// </summary>
    void OnClickRefreshBtn(GameObject go)
    {
        // 打开对战列表刷新界面
        WindowMgr.OpenWnd(ArenaRefreshBattleWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 刷新连胜buff
    /// </summary>
    void RefreshWinsBuff()
    {
        // 玩家对象不存在
        // 窗口对象已经析构
        if (ME.user == null || this == null)
            return;

        // 获取玩家竞技场连胜数据
        LPCValue v = ME.user.Query<LPCValue>("arena_record");
        LPCMapping arenaRecord = LPCMapping.Empty;

        if (v != null && v.IsMapping)
            arenaRecord = v.AsMapping;

        // 连胜次数
        int winTimes = arenaRecord.GetValue<int>("win_times");

        // 连胜buff结束的时间
        int endTime = arenaRecord.GetValue<int>("end_time");

        // 当前的连胜次数
        mWinsAmount.text = string.Format(LocalizationMgr.Get("RankingBattleWnd_16"), winTimes, ArenaMgr.GetArenaBuffDesc(winTimes));

        // 连胜buff的剩余时间
        mRemainTime = Mathf.Max(0, endTime - TimeMgr.GetTime());

        if (mRemainTime <= 0)
        {
            mTimer.text = string.Format(LocalizationMgr.Get("RankingBattleWnd_17"), LocalizationMgr.Get("RankingBattleWnd_21"));

            mEnableCountDown = false;
        }
        else
            mEnableCountDown = true;

        CsvRow csvRow = ArenaMgr.ArenaBuffCsv.FindByKey(ArenaMgr.GetBuffWinsTimes(winTimes));

        if (csvRow == null)
            return;

        // 下一次有连胜buff的次数
        int nextWinTimes = csvRow.Query<int>("next_win_times");

        if (nextWinTimes > 0)
            mNextWinsAmount.text = string.Format(LocalizationMgr.Get("RankingBattleWnd_18"), nextWinTimes, ArenaMgr.GetArenaBuffDesc(nextWinTimes));
        else
            mNextWinsAmount.text = LocalizationMgr.Get("RankingBattleWnd_22");

        if (! mNextWinsAmount.gameObject.activeSelf)
            mNextWinsAmount.gameObject.SetActive(true);
    }

    /// <summary>
    /// 创建一批基础格子
    /// </summary>
    void CreateObject()
    {
        mItem.SetActive(false);

        mItemDic.Clear();

        for (int i = 0; i < mLimitAmonut; i++)
        {
            // 实例化列表格子
            GameObject clone = Instantiate(mItem);
            clone.transform.SetParent(mWrapContent.transform);
            clone.transform.localScale = Vector3.one;

            // 初始化坐标
            clone.transform.localPosition = Vector3.zero;

            string name = "battle_item_" + i;
            clone.name = name;

            // 缓存格子对象
            mItemDic.Add(name, clone);

            clone.SetActive(false);

            clone.transform.localPosition = new Vector3(
                clone.transform.localPosition.x,
                - 110 * i,
                clone.transform.localPosition.x);
        }
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        if (mItemDic.Count < 1)
            return;

        LPCMapping arenaOpponent = ME.user.Query<LPCMapping>("arena_opponent");

        if (arenaOpponent == null)
            return;

        LPCValue isOverdue = arenaOpponent.GetValue<LPCValue>("is_overdue");
        if (isOverdue != null && isOverdue.AsInt == 1)
        {
            // 提示列表自动刷新
            DialogMgr.ShowSingleBtnDailog(
                new CallBack(DialogCallBack),
                LocalizationMgr.Get("RankingBattleWnd_24"),
                string.Empty,
                string.Empty,
                true,
                WindowMgr.GetWindow(ArenaWnd.WndType).transform
            );

            return;
        }

        // 开始倒计时
        StarCountDown();

        mBattleList = arenaOpponent.GetValue<LPCArray>("opponent");

        int index = 0;
        for (int i = 0; i < mBattleList.Count; i++)
        {
            if (i + 1 > mBattleList.Count)
                break;

            if (! mItemDic.ContainsKey("battle_item_" + i))
                continue;

            GameObject go = mItemDic["battle_item_" + i];

            LPCMapping data = mBattleList[i].AsMapping;
            if (data == null || data.Count == 0)
            {
                go.SetActive(false);
                continue;
            }

            go.SetActive(true);

            go.GetComponent<ArenaBattleItemWnd>().Bind(data);
            index++;
        }

        for (int i = mBattleList.Count; i < mLimitAmonut; i++)
        {
            if (! mItemDic.ContainsKey("battle_item_" + i))
                continue;

            mItemDic["battle_item_" + i].SetActive(false);
        }
    }

    /// <summary>
    /// 开始倒计时
    /// </summary>
    void StarCountDown()
    {
        LPCMapping arenaOpponent = ME.user.Query<LPCMapping>("arena_opponent");

        if (arenaOpponent == null)
            return;

        // 挑战列表匹配时间
        int matchTime = arenaOpponent.GetValue<int>("match_time");

        // 自动刷新列表时间间隔（+59是是因为倒计时不足一分钟的时候显示为0的还等待一分钟）
        int maxOpponentTime = GameSettingMgr.GetSettingInt("arena_opponent_auto_refresh_interval") + 59;

        // 圣域刷新时间
        mRefreshRemainTime = Mathf.Max(maxOpponentTime - Mathf.Max(TimeMgr.GetServerTime() - matchTime, 0), 0);

        // 每秒钟检测一次
        InvokeRepeating("RefreshCountDown", 0, 1.0f);
    }

    /// <summary>
    /// 弹框按钮点击回调
    /// </summary>
    void DialogCallBack(object para, params object[] _params)
    {
        // 请求服务器刷新列表
        ArenaMgr.RequestArenaBattleList(ArenaConst.ARENA_MATCH_TYPE_OVERDUE);
    }
}
