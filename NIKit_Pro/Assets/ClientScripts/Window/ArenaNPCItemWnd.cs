/// <summary>
/// ArenaNPCItemWnd.cs
/// Create by fengsc 2016/10/10
/// npc格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ArenaNPCItemWnd : WindowBase<ArenaNPCItemWnd>
{
    #region 成员变量

    // npc头像
    public UITexture mIcon;
    // npc名字
    public UILabel mName;
    // npc等级
    public UILabel mLevel;
    // 重新准备文本显示
    public UILabel mAgainReadyLb;
    // 下次战斗倒计时
    public UILabel mTimer;
    // 是否可以挑战提示
    public UILabel mIsBattleTips;
    // 战斗胜利文本
    public UILabel mBattleWinLb;
    // 奖励物品图标
    public UISprite[] mRewardGoodsIcon;
    // 奖励物品数量
    public UILabel[] mRewradGoodsAmount;
    // 战斗按钮
    public GameObject mBattleBtn;
    public UILabel mBattleBtnLb;

    // 进入副本的消耗
    public UILabel mCost;
    public UISprite mCostIcon;
    public UISprite mItemBg;

    //遮罩
    public GameObject mWndItemMask;

    public GameObject mArenaWnd;


    LPCMapping mNpcData = new LPCMapping();

    // 副本id
    string mInstanceId = string.Empty;

    int timer = 0;

    bool mIsCountDown = false;

    float mLastTime = 0;

    #endregion

    #region 内部函数

    void Update()
    {
        if (mIsCountDown)
        {
            if (Time.realtimeSinceStartup > mLastTime + 1)
            {
                mLastTime = Time.realtimeSinceStartup;
                CountDown();
            }
        }
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
        // 没有数据
        if (mNpcData.Count < 1)
            return;

        int isUnLocked = mNpcData.GetValue<int>("is_unlocked");

        // npc不在冷却时间内
        if (JudgeIsChallenge())
        {
            string desc = string.Empty;

            desc = isUnLocked == 1 ? LocalizationMgr.Get("ArenaNPCItemWnd_2") : LocalizationMgr.Get("ArenaNPCItemWnd_3");

            // 可以挑战
            mIsBattleTips.text = desc;

            SetBtnStateAndBgColor(255f, true);

            SetTips(true);
        }
        else
        {
            // 设置按钮的状态和颜色
            SetBtnStateAndBgColor(180f, false);

            // 开启倒计时
            mIsCountDown = true;

            SetTips(false);
        }

        if (isUnLocked != 1)
        {
            // npc没有解锁
            SetBtnStateAndBgColor(180f, false);

            mName.gameObject.SetActive(false);
            mLevel.gameObject.SetActive(false);
        }
        else
        {
            mName.gameObject.SetActive(true);
            mLevel.gameObject.SetActive(true);
        }

        // 获取npc头像名称
        // 加载玩家头像;
        string iconName = mNpcData.GetValue<string>("icon");
        string resPath = string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconName);
        Texture2D iconRes = ResourceMgr.LoadTexture(resPath);

        if (iconRes != null)
            mIcon.mainTexture = iconRes;

        mIcon.gameObject.SetActive(true);

        // 获取npc等级
        int mNpcLevel = mNpcData.GetValue<int>("level");

        // npc等级显示
        mLevel.text = string.Format(LocalizationMgr.Get("RankingBattleWnd_12"), mNpcLevel);

        // 获取npc的名称
        mName.text = LocalizationMgr.Get(mNpcData.GetValue<string>("name"));

        LPCMapping bonusData = mNpcData.GetValue<LPCMapping>("bonus");

        if (bonusData == null)
            return;

        int index = 0;
        foreach (string key in bonusData.Keys)
        {
            mRewardGoodsIcon[index].spriteName = key;
            mRewradGoodsAmount[index].text = bonusData[key].AsString;
            index++;
        }

        // 获取消耗
        LPCMapping costMap = InstanceMgr.GetInstanceCostMap(ME.user, mInstanceId);

        string fields = FieldsMgr.GetFieldInMapping(costMap);

        mCostIcon.spriteName = fields;

        mCost.text = costMap.GetValue<int>(fields).ToString();
    }

    /// <summary>
    /// 设置提示信息
    /// </summary>
    void SetTips(bool isActive)
    {
        mIsBattleTips.gameObject.SetActive(isActive);
        mTimer.gameObject.SetActive(!isActive);
        mWndItemMask.SetActive(!isActive);
        mAgainReadyLb.gameObject.SetActive(!isActive);
    }

    /// <summary>
    /// 倒计时
    /// </summary>
    void CountDown()
    {
        if (timer < 0)
        {
            SetTips(true);

            // 重绘窗口
            Redraw();

            mIsCountDown = false;
            return;
        }

        mTimer.text = TimeMgr.ConvertTime(timer, true);

        timer--;
    }

    /// <summary>
    /// 判断能否挑战
    /// </summary>
    bool JudgeIsChallenge()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return false;

        // 上次战斗胜利距离现在的时间间隔
        int interval = mNpcData.GetValue<int>("cd_time") - TimeMgr.GetServerTime();

        // 计算cd剩余时间
        timer = Mathf.Max(interval, 0);
        return timer > 0 ? false : true;
    }

    /// <summary>
    /// 设置按钮的状态和item的背景色
    /// </summary>
    void SetBtnStateAndBgColor(float rgb, bool isRegister)
    {
        float value = rgb / 255;

        Color color = new Color(value, value, value);

        mItemBg.color = color;
        mBattleBtn.GetComponent<UISprite>().color = color;

        if (isRegister)
            UIEventListener.Get(mBattleBtn).onClick = OnClickBattleBtn;
        else
            UIEventListener.Get(mBattleBtn).onClick -= OnClickBattleBtn;
    }

    /// <summary>
    /// 战斗按钮点击事件
    /// </summary>
    void OnClickBattleBtn(GameObject go)
    {
        if (string.IsNullOrEmpty(mInstanceId))
            return;

        // 不允许挑战
        if (!JudgeIsChallenge())
            return;

        // 获取副本配置信息
        List<CsvRow> list = InstanceMgr.GetInstanceFormation(mInstanceId);

        LPCArray defenceList = new LPCArray();

        foreach (CsvRow item in list)
        {
            LPCMapping para = new LPCMapping();

            // 调用脚本参数计算怪物class_id;
            int classIdScript = item.Query<int>("class_id_script");
            int classId = (int) ScriptMgr.Call(classIdScript, ME.user.GetLevel(),
                item.Query<LPCValue>("class_id_args"));

            para.Add("rid", Rid.New());
            para.Add("class_id", classId);

            // 获取始化参数;
            int initScript = item.Query<int>("init_script");
            LPCMapping initArgs = ScriptMgr.Call(initScript, ME.user.GetLevel(),
                item.Query<LPCValue>("init_script_args"), para) as LPCMapping;

            // 获取始化参数
            para.Append(initArgs);

            // 添加列表
            defenceList.Add(para);
        }

        LPCMapping defenceData = new LPCMapping();
        defenceData.Add("defense_list", defenceList);

        // 打开选择战斗界面
        GameObject wnd = WindowMgr.OpenWnd(SelectFighterWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
        {
            LogMgr.Trace("SelectFighterWnd窗口创建失败");
            return;
        }

        // 窗口绑定数据
        wnd.GetComponent<SelectFighterWnd>().Bind(mArenaWnd.name, mInstanceId, defenceData);

        WindowMgr.DestroyWindow(mArenaWnd.name);
    }

    /// <summary>
    /// 初始化本地花文本
    /// </summary>
    void InitLocalText()
    {
        mAgainReadyLb.text = LocalizationMgr.Get("ArenaNPCItemWnd_1");
        mBattleWinLb.text = LocalizationMgr.Get("ArenaNPCItemWnd_4");
        mBattleBtnLb.text = LocalizationMgr.Get("ArenaNPCItemWnd_5");
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data, string instanceId)
    {
        // 注册事件
        RegisterEvent();

        mNpcData = data;

        mInstanceId = instanceId;

        if (mNpcData == null || mNpcData.Count < 1)
            return;

        Redraw();
    }

    /// <summary>
    /// 指引点击战斗按钮
    /// </summary>
    public void GuideClickBattle()
    {
        OnClickBattleBtn(mBattleBtn);
    }

    #endregion
}
