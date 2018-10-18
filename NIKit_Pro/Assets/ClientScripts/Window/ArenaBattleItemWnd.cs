/// <summary>
/// ArenaBattleItemWnd.cs
/// Created by fengsc 2016/09/26
/// 竞技场排位战列表基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class ArenaBattleItemWnd : WindowBase<ArenaBattleItemWnd>
{
    #region 成员变量

    public UITexture mIcon;
    // 玩家头像
    public UILabel mLevel;
    // 等级
    public UILabel mName;
    // 姓名
    public UILabel mScore;
    // 竞技场积分
    public UILabel mGoodsAmount;
    // 奖励物品数量
    public UILabel mRewardGoodsLb;

    public GameObject mBattleBtn;

    public GameObject mWndItemMask;
    // 战斗按钮
    public UILabel mCost;
    // 竞技场战斗消耗
    public UILabel mBattleBtnLb;

    public UILabel mVictoryOrFailed;
    // 胜利

    public UISprite mCostIcon;

    LPCMapping mBattleData = new LPCMapping();

    string mInstanceId = "arena";

    LPCMapping instanceData = new LPCMapping();

    #endregion

    // Use this for initialization
    void Start()
    {
        // 初始化本地化文本
        InitLocalText();

        UIEventListener.Get(mBattleBtn).onClick = OnClickBattleBtn;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        //加载玩家头像;
        LPCValue iconValue = mBattleData.GetValue<LPCValue>("icon");
        if (iconValue != null && iconValue.IsString)
            mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconValue.AsString));
        else
            mIcon.mainTexture = null;

        // 玩家等级
        mLevel.text = string.Format(LocalizationMgr.Get("RankingBattleWnd_12"), mBattleData.GetValue<int>("level"));

        // 竞技场积分
        mScore.text = mBattleData.GetValue<int>("score").ToString();

        // 玩家名称
        mName.text = mBattleData.GetValue<string>("name");

        // 获取副本的配置信息
        instanceData = InstanceMgr.GetInstanceInfo(mInstanceId);

        // 是否挑战成功
        LPCValue challenged = mBattleData.GetValue<LPCValue>("challenged");
        if (challenged != null && challenged.AsInt == 1)
            SetItemState(true);
        else
            SetItemState(false);

        if (instanceData == null)
            return;

        // 挑战胜利的奖励
        LPCMapping bonus = InstanceMgr.GetInstanceClearanceBonus(mInstanceId);

        if(bonus == null)
            return;

        string fields = FieldsMgr.GetFieldInMapping(bonus);

        mGoodsAmount.text = LocalizationMgr.Get("RankingBattleWnd_11") + bonus.GetValue<int>(fields).ToString();

        // 获取竞技场战斗消耗
        LPCMapping battleCost = InstanceMgr.GetInstanceCostMap(ME.user, mInstanceId);

        string battleCostField = FieldsMgr.GetFieldInMapping(battleCost);

        mCost.text = battleCost.GetValue<int>(battleCostField).ToString();

        mCostIcon.spriteName = battleCostField;
    }

    void SetItemState(bool isEnable)
    {
        UISprite bg = gameObject.transform.Find("bg").GetComponent<UISprite>();

        if (bg == null)
            return;

        float value = 0;

        if (isEnable)
        {
            value = 150f/ 255;
            mVictoryOrFailed.text = LocalizationMgr.Get("RankingBattleWnd_9");
        }
        else
        {
            value = 255f/ 255;
            mVictoryOrFailed.text = LocalizationMgr.Get("RankingBattleWnd_10");
        }

        bg.color = new Color(value, value, value);

        mBattleBtn.SetActive(!isEnable);
        mWndItemMask.SetActive(isEnable);

        mVictoryOrFailed.gameObject.SetActive(isEnable);

        gameObject.transform.Find("reward").gameObject.SetActive(!isEnable);
    }

    /// <summary>
    /// 战斗按钮点击事件
    /// </summary>
    void OnClickBattleBtn(GameObject go)
    {
        // 竞技场结算中,进入失败
        if (ArenaMgr.IsSettlement())
        {
            DialogMgr.Notify(LocalizationMgr.Get("ArenaWnd_10"));

            return;
        }

        if (mBattleData == null || mBattleData.Count < 1)
            return;

        string rid = mBattleData.GetValue<string>("rid");

        if (string.IsNullOrEmpty(rid))
            return;

        // 向服务器请求被挑战者的防御列表
        Operation.CmdGetArenaOpponentDefenseData.Go(rid);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLocalText()
    {
        mRewardGoodsLb.text = LocalizationMgr.Get("RankingBattleWnd_7");
        mBattleBtnLb.text = LocalizationMgr.Get("RankingBattleWnd_8");
    }

    /// <summary>
    /// 绑定列表数据
    /// </summary>
    public void Bind(LPCMapping data)
    {
        mBattleData = data;

        Redraw();
    }
}
