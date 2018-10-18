using UnityEngine;
using System.Collections;
using LPC;

public class ArenaRevengeItemWnd : WindowBase<ArenaRevengeItemWnd>
{
    #region 成员变量

    public UISprite mBg;
    public UITexture mIcon;
    public UILabel mLevel;
    public UILabel mName;
    public UILabel mScore;
    public UILabel mResultAndTime;
    public GameObject mBattleBtn;
    public UILabel mCostNum;
    public UILabel mBattleBtnLb;
    public UISprite mCostIcon;
    public UILabel mRevengeResult;
    public UILabel mRewardLb;
    public UISprite mRewardIcon;
    public UILabel mRewardNum;
    public GameObject mReward;
    public GameObject mArenaWnd;

    LPCMapping mRevengeData = new LPCMapping();

    // 副本名称
    string mInstanceId = "arena_revenge";

    // 每条反击记录的唯一id
    string mCookie = string.Empty;

    #endregion

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        // 初始化本地化文本
        InitLocalText();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mBattleBtn).onClick = OnClickBattleBtn;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        if(mRevengeData == null)
            return;

        // 记录反击的唯一id
        mCookie = mRevengeData.GetValue<string>("cookie");

        LPCMapping top_data = mRevengeData.GetValue<LPCMapping>("opponent_value");

        // 加载玩家头像;
        string iconName = string.Empty;

        LPCValue v = top_data.GetValue<LPCValue>("icon");
        if (v != null && v.IsString)
            iconName = v.AsString;

        string resPath = string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconName);
        Texture2D iconRes = ResourceMgr.LoadTexture(resPath);

        if (iconRes != null)
            mIcon.mainTexture = iconRes;

        int level = 0;

        LPCValue levelLpc = top_data.GetValue<LPCValue>("level");
        if (levelLpc != null && levelLpc.IsInt)
            level = levelLpc.AsInt;

        // 玩家等级
        mLevel.text = string.Format(LocalizationMgr.Get("RankingBattleWnd_12"), level);

        string name = string.Empty;

        LPCValue nameLpc = top_data.GetValue<LPCValue>("name");
        if (nameLpc != null && nameLpc.IsString)
            name = nameLpc.AsString;

        // 玩家名称
        mName.text = name;

        // 防守结果
        if(mRevengeData.GetValue<int>("result") == 0)
            mResultAndTime.text = string.Format(LocalizationMgr.Get("DefenceRecordWnd_5"), "(" ,
                TimeMgr.ConvertTimeToSimpleChinese(mRevengeData.GetValue<int>("time")), ")");
        else
            mResultAndTime.text = string.Format(LocalizationMgr.Get("DefenceRecordWnd_6"), "(",
                TimeMgr.ConvertTimeToSimpleChinese(mRevengeData.GetValue<int>("time")), ")");

        // 竞技场积分
        string score = mRevengeData.GetValue<int>("score").ToString();
        if(mRevengeData.GetValue<int>("score") >= 0)
            score = string.Format("+{0}", score);
        
        mScore.text = string.Format(LocalizationMgr.Get("DefenceRecordWnd_4"), score);

        if(mRevengeData.GetValue<int>("revenged") > 0)
        {
            mReward.SetActive(false);
            mBattleBtn.SetActive(false);
            mRevengeResult.gameObject.SetActive(true);

            // 已反击将背景置灰
            mBg.color = new Color(150f/255f, 150f/255f, 150f/255f, 1f);

            // 显示反击结果
            mRevengeResult.text = mRevengeData.GetValue<int>("revenged") == 1 ?
                LocalizationMgr.Get("DefenceRecordWnd_3"):LocalizationMgr.Get("DefenceRecordWnd_2");

            return;
        }

        // 已反击将背景置灰
        mBg.color = new Color(255/255f, 255/255f, 255/255f, 1f);

        mReward.SetActive(true);
        mBattleBtn.SetActive(true);
        mRevengeResult.gameObject.SetActive(false);

        // 挑战胜利的奖励
        LPCMapping bonus = InstanceMgr.GetInstanceClearanceBonus(mInstanceId);

        if(bonus != null)
        {
            string bonusField = FieldsMgr.GetFieldInMapping(bonus);

            mRewardIcon.spriteName = FieldsMgr.GetFieldIcon(bonusField);
            mRewardNum.text = string.Format("+{0}", bonus.GetValue<int>(bonusField));
        }
        else
            mReward.SetActive(false);

        // 获取竞技场战斗消耗
        LPCMapping battleCost = InstanceMgr.GetInstanceCostMap(ME.user, mInstanceId);

        if(battleCost != null)
        {
            string costField = FieldsMgr.GetFieldInMapping(battleCost);

            mCostIcon.spriteName = FieldsMgr.GetFieldIcon(costField);
            mBattleBtnLb.text = LocalizationMgr.Get("DefenceRecordWnd_7");
            mCostNum.text = battleCost.GetValue<int>(costField).ToString();
        }
        else
            mCostNum.text = "0";
    }

    /// <summary>
    /// 战斗按钮点击事件
    /// </summary>
    void OnClickBattleBtn(GameObject go)
    {
        if (mRevengeData == null)
            return;

        // 竞技场结算中，进入失败
        if (ArenaMgr.IsSettlement())
        {
            DialogMgr.Notify(LocalizationMgr.Get("ArenaWnd_10"));

            return;
        }

        // 向服务器请求被挑战者的防御列表
        Operation.CmdGetArenaOpponentDefenseData.Go(mCookie, true);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLocalText()
    {
        mRewardLb.text = LocalizationMgr.Get("DefenceRecordWnd_1");
    }

    #region 公共函数

    /// <summary>
    /// 绑定列表数据
    /// </summary>
    public void Bind(LPCMapping data)
    {
        mRevengeData = data;

        Redraw();
    }

    #endregion
}
