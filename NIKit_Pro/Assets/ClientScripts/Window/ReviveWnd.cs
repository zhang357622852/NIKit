/// <summary>
/// Created bu fengsc 2016/08/05
/// 副本复活窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ReviveWnd : WindowBase<ReviveWnd>
{
    #region 成员变量

    public UILabel mTitle;

    public UILabel mDesc1;

    public UILabel mPrice;

    public UILabel mDesc2;

    public GameObject mReviveBtn;
    public UILabel mResLabel;
    public UILabel mResPrice;

    public GameObject mCancelBtn;
    public UILabel mCancelLabel;

    public GameObject mMask;

    public GameObject mNormal;

    public GameObject mRoundTips;

    public UILabel mTips;

    LPCMapping data = new LPCMapping();

    //当前副本id
    string instanceId = string.Empty;

    //复活需要的消耗;
    LPCMapping cost = LPCMapping.Empty;

    //复活的次数;
    int reviveTimes = 0;

    #endregion

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mReviveBtn).onClick = OnClickReviveBtn;
        UIEventListener.Get(mCancelBtn).onClick = OnClickCancelBtn;
        UIEventListener.Get(mMask).onClick = OnClickCancelBtn;
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    void OnEnable()
    {
        // 监听msg_revive_pet回调
        MsgMgr.RegisterDoneHook("MSG_REVIVE_PET", "revive_wnd", OnMsgRevivePet);
    }

    /// <summary>
    /// OnDisable
    /// </summary>
    void OnDisable()
    {
        // 开始协程
        Coroutine.DispatchService(RemoveDoneHook());
    }

    /// <summary>
    /// Removes the done hook.
    /// </summary>
    private IEnumerator RemoveDoneHook()
    {
        // 等待一帧
        yield return null;

        //移除msg_revive_pet事件
        MsgMgr.RemoveDoneHook("MSG_REVIVE_PET", "revive_wnd");
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLabel()
    {
        mTitle.text = LocalizationMgr.Get("FightSettlementWnd_8");
        mDesc1.text = LocalizationMgr.Get("FightSettlementWnd_10");
        mDesc2.text = LocalizationMgr.Get("FightSettlementWnd_11");
        mResLabel.text = LocalizationMgr.Get("FightSettlementWnd_7");
        mCancelLabel.text = LocalizationMgr.Get("FightSettlementWnd_9");
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        if (string.IsNullOrEmpty(instanceId))
            return;

        LPCMapping map = InstanceMgr.GetInstanceInfo(instanceId);

        if (map == null)
            return;

        //获取实际的复活开销
        if (map["revive_cost"].IsMapping)
        {
            cost = map["revive_cost"].AsMapping;
        }
        else if (map["revive_cost"].IsInt)
        {
            //获取本地数据;
            reviveTimes = InstanceMgr.GetReviveTimes(ME.user) + 1;

            //脚本编号
            int scriptNo = map["revive_cost"].AsInt;

            //通过脚本计算开销;
            cost = ScriptMgr.Call(scriptNo, reviveTimes) as LPCMapping;
        }

        string fields = FieldsMgr.GetFieldInMapping(cost);

        int amount = cost.GetValue<int>(fields);

        // 剩余战场回合
        int remainRound = GameSettingMgr.GetSettingInt("max_combat_rounds") - InstanceMgr.GetRoundCount(ME.user);

        if (remainRound <= InstanceConst.FIRST_STAGE)
        {
            // 显示回合提示
            mNormal.SetActive(false);
            mRoundTips.SetActive(true);

            mTips.text = string.Format(LocalizationMgr.Get("FightSettlementWnd_22"), remainRound);
        }
        else
        {
            mPrice.text = amount.ToString();

            mNormal.SetActive(true);
            mRoundTips.SetActive(false);
        }

        mResPrice.text = amount.ToString();
    }

    /// <summary>
    /// 复活消息回调
    /// </summary>
    void OnMsgRevivePet(string cmd, LPCValue para)
    {
        // 销毁本窗口
        WindowMgr.DestroyWindow(gameObject.name);

        GameObject wnd = WindowMgr.OpenWnd(CombatWnd.WndType);
        if (wnd == null)
            return;

        Transform go = wnd.transform.Find("SkillInfoWnd");
        if (go == null)
            return;

        // 显示SkillInfoWnd
        go.gameObject.SetActive(true);
    }

    /// <summary>
    /// 复活按钮点击事件
    /// </summary>
    void OnClickReviveBtn(GameObject go)
    {
        if (ME.user == null)
        {
            LogMgr.Trace("玩家对象不存在");
            return;
        }

        string fields = FieldsMgr.GetFieldInMapping(cost);

        int amount = ME.user.Query<int>(fields);

        // 玩家消耗不足
        if (amount < cost.GetValue<int>(fields))
        {
            DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("FightSettlementWnd_13"), FieldsMgr.GetFieldName(fields)));
            return;
        }

        //通知服务器更新;
        Operation.CmdRevivePet.Go();

        UIEventListener.Get(mReviveBtn).onClick -= OnClickReviveBtn;
    }

    /// <summary>
    /// 取消按钮点击事件
    /// </summary>
    void OnClickCancelBtn(GameObject go)
    {
        // 没有绑定数据
        if (data == null)
            return;

        bool result = true;
        if (data.GetValue<int>("result") == 0)
            result = false;

        //通知服务器更新;
        Operation.CmdInstanceClearance.Go(
            data.GetValue<string>("rid"),
            result,
            data.GetValue<LPCMapping>("dropBonus"),
            data.GetValue<LPCMapping>("roundActions"),
            data.GetValue<int> ("aliveAmount"),
            data.GetValue<int> ("killAmount"),
            data.GetValue<int> ("crossTimes"),
            data.GetValue<int> ("remainAmount"),
            data.GetValue<int> ("clearanceTime"),
            AutoCombatMgr.AutoCombat,
            data.GetValue<int> ("roundTimes")
         );

        //销毁当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// Bind the specified map.
    /// </summary>
    /// <param name="map">Map.</param>
    public void Bind(LPCMapping map)
    {
        data = map;

        if (map == null)
            return;

        instanceId = data.GetValue<string>("instanceId");

        InitLabel();

        RegisterEvent();

        Redraw();
    }
}
