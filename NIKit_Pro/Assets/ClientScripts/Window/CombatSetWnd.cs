/// <summary>
/// CombatSetWnd.cs
/// Created by tanzy 2016/05/10
/// CombatSetWnd的子窗口，玩家操作主要集中在该部分
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// CombatWnd的子窗口，玩家操作主要集中在该部分
/// </summary>
public class CombatSetWnd : WindowBase<CombatSetWnd>
{
    // 放弃按钮
    public GameObject mGiveUpBtn;
    public UILabel mGiveUpBtnLb;

    // 增益效果查看按钮
    public UIToggle mEffectBtn;
    public UILabel mEffectBtnLb;

    // 首领战策略按钮
    public UIToggle mPloyBtn;
    public UILabel mPloyBtnLb;

    public GameObject mEffectView;

    public TweenScale mEffectTS;

    public UITable mTable;

    public GameObject mItem;

    public UILabel mNoEffectTips;

    // 策略选择面板
    public GameObject mPloySelect;
    public TweenScale mPloySelectTS;

    // 手动策略按钮
    public UIToggle mManualOpreationBtn;
    public UILabel mManualOpreationBtnLb;

    // 自动战斗
    public UIToggle mAutoCombatBtn;
    public UILabel mAutoCombatBtnLb;

    // 顺序攻击
    public UIToggle mOderAttackBtn;
    public UILabel mOderAttackBtnLb;

    // 顺序选择面板
    public GameObject mOderSelect;
    public TweenScale mOderSelectTS;

    public GameObject mPosBtn;

    public GameObject mNormal;

    public GameObject mPlayback;

    // 退出回放按钮
    public GameObject mQuitPlayback;
    public UILabel mQuitPlaybackLb;

    [HideInInspector]
    public bool mIsopen = false;

    private int mCurrnetAtkOder = 0;

    private int mCurrnetPos = 0;

    private Dictionary<int, UILabel> mOderDic = new Dictionary<int, UILabel>();

    private List<GameObject> mPosBtnList = new List<GameObject>();

    private List<UILabel> mPosList = new List<UILabel>();

    private List<UILabel> mPosBtnLbList = new List<UILabel>();

    LPCMapping mCombatPloy = LPCMapping.Empty;

    // 副本攻击可选列表
    LPCArray mAtkSelectList = LPCArray.Empty;

    // 当前副本id
    string mInstanceId = string.Empty;

    GameObject mWnd;

    int mIsPlayback = 0;

    void Awake()
    {
        if (ME.user == null)
            return;

        LPCMapping instance = ME.user.Query<LPCMapping>("instance");

        // 副本id
        mInstanceId = instance.GetValue<string>("id");

        mIsPlayback = instance.GetValue<int>("playback");
    }

    void OnEnable()
    {
        // 初始化数据
        InitData();

        mIsopen = true;

        mEffectView.SetActive(false);

        mPloySelect.SetActive(false);

        mOderSelect.SetActive(false);

        mEffectBtn.optionCanBeNone = false;
        mPloyBtn.optionCanBeNone = false;
    }

    void OnDisable()
    {
        mIsopen = false;

        mCurrnetAtkOder = 0;

        mEffectBtn.optionCanBeNone = true;
        mPloyBtn.optionCanBeNone = true;

        mEffectBtn.value = false;

        mPloyBtn.value = false;

        mManualOpreationBtn.optionCanBeNone = true;
        mAutoCombatBtn.optionCanBeNone = true;
        mOderAttackBtn.optionCanBeNone = true;

        mManualOpreationBtn.value = false;
        mAutoCombatBtn.value = false;
        mOderAttackBtn.value = false;

        SetToggleValue(false);

        // 刷新设置窗口
        GameObject wnd = WindowMgr.GetWindow(CombatWnd.WndType);
        if (wnd == null)
            return;

        // 刷新设置窗口
        wnd.GetComponent<CombatWnd>().RedrawSetWnd();
    }

    void Start()
    {
        mWnd = this.gameObject;

        // 初始化文本
        InitText();

        // 注册事件
        RegisterEvent();

        if (mIsPlayback == 1)
        {
            mPlayback.SetActive(true);

            mNormal.SetActive(false);
        }
        else
        {
            CreatedSelectObject();

            mPlayback.SetActive(false);

            mNormal.SetActive(true);
        }
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mGiveUpBtnLb.text = LocalizationMgr.Get("CombatSetWnd_3");
        mEffectBtnLb.text = LocalizationMgr.Get("CombatSetWnd_1");
        mPloyBtnLb.text = LocalizationMgr.Get("CombatSetWnd_7");
        mManualOpreationBtnLb.text = LocalizationMgr.Get("CombatSetWnd_8");
        mAutoCombatBtnLb.text = LocalizationMgr.Get("CombatSetWnd_9");
        mOderAttackBtnLb.text = LocalizationMgr.Get("CombatSetWnd_10");
        mNoEffectTips.text = LocalizationMgr.Get("CombatSetWnd_11");
        mQuitPlaybackLb.text = LocalizationMgr.Get("CombatWnd_8");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mGiveUpBtn).onClick = OnClickGiveUp;
        UIEventListener.Get(mEffectBtn.gameObject).onClick = OnEffectBtnChange;
        UIEventListener.Get(mPloyBtn.gameObject).onClick = OnPloyBtnChange;
        UIEventListener.Get(mManualOpreationBtn.gameObject).onClick = OnManualOpreationBtnChange;
        UIEventListener.Get(mAutoCombatBtn.gameObject).onClick = OnAutoCombatBtnChange;
        UIEventListener.Get(mOderAttackBtn.gameObject).onClick = OnOderAttackBtnChange;
        UIEventListener.Get(mQuitPlayback).onClick = OnClickQuitPlayback;
    }

    /// <summary>
    /// 创建可选列表格子
    /// </summary>
    void CreatedSelectObject()
    {
        float y = 82;

        if (mAtkSelectList.Count > 3)
            y = (mAtkSelectList.Count - 3) * 76 + 82;

        mPosBtn.SetActive(false);

        for (int i = 0; i < mAtkSelectList.Count; i++)
        {
            GameObject clone = Instantiate(mPosBtn);

            clone.transform.SetParent(mPosBtn.transform.parent);

            clone.transform.localScale = Vector3.one;

            clone.transform.localPosition = new Vector3(
                mPosBtn.transform.localPosition.x,
                y - i * 76,
                mPosBtn.transform.localPosition.z);

            clone.SetActive(true);

            UIEventListener.Get(clone).onClick = OnClickPosBtn;

            mPosBtnList.Add(clone);

            Transform posTrans = clone.transform.Find("pos");
            if (posTrans == null)
                continue;

            UILabel pos = posTrans.GetComponent<UILabel>();
            if (pos == null)
                continue;

            mPosList.Add(pos);

            Transform posLbTran = clone.transform.Find("posLb");
            if (posLbTran == null)
                continue;

            UILabel posLb = posLbTran.GetComponent<UILabel>();
            if (posLb == null)
                continue;

            mPosBtnLbList.Add(posLb);
        }
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    void InitData()
    {
        if (ME.user == null)
            return;

        mAtkSelectList = InstanceMgr.GetAtkSelectList(mInstanceId);

        // 获取缓存的战斗策略
        mCombatPloy = AutoCombatSelectTypeMgr.GetSelectMap(ME.user, mInstanceId);
    }

    /// <summary>
    /// 获取队长宠物
    /// </summary>
    Property GetLeaderPet(List<Property> pets)
    {
        if (pets == null || pets.Count == 0)
            return null;

        for (int i = 0; i < pets.Count; i++)
        {
            Property ob = pets[i];
            if (ob == null)
                continue;

            if (ob.Query<int>("is_leader") == 1)
                return ob;
        }

        return null;
    }

    /// <summary>
    /// 增益效果按钮点击事件
    /// </summary>
    void OnEffectBtnChange(GameObject go)
    {
        if (mEffectView.activeSelf)
            return;

        for (int i = 0; i < mTable.transform.childCount; i++)
            Destroy(mTable.transform.GetChild(i).gameObject);

        mItem.SetActive(false);

        mTable.repositionNow = true;

        ShowOption(true, false);

        mEffectTS.ResetToBeginning();
        mEffectTS.PlayForward();

        // 获取队长宠物对象
        Property leaderPet = GetLeaderPet(RoundCombatMgr.GetPropertyList(CampConst.CAMP_TYPE_ATTACK));
        if (leaderPet == null)
        {
            mNoEffectTips.gameObject.SetActive(true);

            return;
        }

        // 获取角色的队长技能
        LPCMapping skills = SkillMgr.GetEffectctiveLeaderSkill(leaderPet);

        if (skills == null || skills.Count == 0)
        {
            mNoEffectTips.gameObject.SetActive(true);

            return;
        }

        mNoEffectTips.gameObject.SetActive(false);

        foreach (int skillId in skills.Keys)
        {
            GameObject item = Instantiate(mItem);
            item.transform.SetParent(mTable.transform);
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = Vector3.one;

            UITexture icon = item.transform.Find("icon").GetComponent<UITexture>();

            if (icon != null)
                icon.mainTexture = SkillMgr.GetTexture(skillId);

            UILabel title = item.transform.Find("title").GetComponent<UILabel>();

            if (title != null)
            {
                CsvRow row = SkillMgr.GetSkillInfo(skillId);

                // 队长技能名称
                title.text = string.Format(LocalizationMgr.Get("CombatSetWnd_4"), LocalizationMgr.Get(row.Query<string>("name")));
            }

            UILabel desc = item.transform.Find("desc").GetComponent<UILabel>();

            if (desc != null)
            {
                desc.text = SkillMgr.GetLeaderSkillDesc(leaderPet);
            }

            item.SetActive(true);
        }
    }

    /// <summary>
    /// 首领战策略按钮点击事件
    /// </summary>
    void OnPloyBtnChange(GameObject go)
    {
        if (mPloySelect.activeSelf)
            return;

        ShowOption(false, false);

        if (mAtkSelectList == null || mAtkSelectList.Count == 0)
            mOderAttackBtn.gameObject.SetActive(false);

        mPloySelectTS.ResetToBeginning();
        mPloySelectTS.PlayForward();

        // 设置默认选项
        SetDefaultOption();
    }

    /// <summary>
    /// 手动操作按钮点击事件
    /// </summary>
    void OnManualOpreationBtnChange(GameObject go)
    {
        ShowOption(false, false);

        SetToggleValue(false);

        mCombatPloy = LPCMapping.Empty;
        mCombatPloy.Add(InstanceConst.MANUAL_OPREATION, 1);

        // 缓存副本攻击策略
        AutoCombatSelectTypeMgr.SetSelectMap(ME.user, mInstanceId, mCombatPloy);

        // 获取防守方战斗列表
        List<Property> list = RoundCombatMgr.GetPropertyList(CampConst.CAMP_TYPE_DEFENCE);
        if (list == null || list.Count == 0)
            return;

        for (int i = 0; i < list.Count; i++)
        {
            Property ob = list[i];

            if (ob == null)
                continue;

            LPCValue selectType = ob.Query<LPCValue>("auto_combat_select_type");

            // 手动操作, 最后一批次如果是自动战斗则取消
            if (selectType != null && selectType.AsString.Equals(InstanceConst.BOSS_TYPE))
            {
                // 取消自动战斗
                if (AutoCombatMgr.IsAutoCombat())
                    AutoCombatMgr.SetAutoCombat(false, InstanceMgr.GetLoopFightByInstanceId(mInstanceId));
            }

        }
    }

    /// <summary>
    /// 自动战斗按钮点击事件
    /// </summary>
    void OnAutoCombatBtnChange(GameObject go)
    {
        ShowOption(false, false);

        mCombatPloy = LPCMapping.Empty;
        mCombatPloy.Add(InstanceConst.AUTO_COMBAT, 1);

        // 缓存副本攻击策略
        AutoCombatSelectTypeMgr.SetSelectMap(ME.user, mInstanceId, mCombatPloy);

        SetToggleValue(false);
    }

    /// <summary>
    /// 顺序攻击按钮点击事件
    /// </summary>
    void OnOderAttackBtnChange(GameObject go)
    {
        if (mOderSelect.activeSelf)
            return;

        ShowOption(false, true);

        mOderSelectTS.ResetToBeginning();
        mOderSelectTS.PlayForward();

        if (!mCombatPloy.ContainsKey(InstanceConst.ATTACK_ODER))
        {
            mCombatPloy = LPCMapping.Empty;
            mCombatPloy.Add(InstanceConst.ATTACK_ODER, LPCArray.Empty);
        }

        // 显示可选列表
        ShowSelectList();
    }

    void OnClickPosBtn(GameObject go)
    {
        int index = mPosBtnList.IndexOf(go);
        if (index < 0)
            return;

        mCurrnetPos = index + 1;

        UIToggle toggle = go.GetComponent<UIToggle>();
        if (toggle == null)
            return;

        if (! toggle.value)
        {
            CancelAttackOder(mPosList[index].text);
            return;
        }

        ShowOption(false, true);

        mCurrnetAtkOder++;

        mPosList[index].text = mCurrnetAtkOder.ToString();

        AddAttackOder(mPosList[index]);
    }

    /// <summary>
    /// 退出回放
    /// </summary>
    void OnClickQuitPlayback(GameObject go)
    {
        if (mIsPlayback != 1)
            return;

        // 提示弹框
        DialogMgr.ShowDailog(new CallBack(OnQuitPlayback), LocalizationMgr.Get("CombatSetWnd_12"));
    }

    /// <summary>
    /// 退出回放回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="param">Parameter.</param>
    void OnQuitPlayback(object para, params object[] param)
    {
        if (mWnd == null)
            return;

        if (!(bool) param[0])
            return;

        // 如果战斗没有开始不允许放弃战斗
        if (! RoundCombatMgr.IsRoundCombatRunning())
        {
            // 给出提示信息
            DialogMgr.Notify(LocalizationMgr.Get("CombatSetWnd_14"));
            return;
        }

        // 恢复
        TimeMgr.DoContinueCombatLogic("CombatSetPause");

        // 结束战斗
        RoundCombatMgr.EndCombat(RoundCombatConst.END_TYPE_GIVEUP);
    }

    /// <summary>
    /// 放弃按钮点击事件
    /// </summary>
    void OnClickGiveUp(GameObject ob)
    {
        // 指引没有完成
        if (! GuideMgr.IsGuided(4))
        {
            DialogMgr.Notify(LocalizationMgr.Get("GuideWnd_1"));

            return;
        }

        // 提示弹框
        DialogMgr.ShowDailog(new CallBack(OnGiveUpCallBack), LocalizationMgr.Get("CombatSetWnd_5"));
    }

    /// <summary>
    /// 放弃战斗回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="param">Parameter.</param>
    void OnGiveUpCallBack(object para, params object[] param)
    {
        // 窗口对象不存在
        if (mWnd == null)
            return;

        if (!(bool) param[0])
            return;

        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 如果战斗没有开始不允许放弃战斗
        if (! RoundCombatMgr.IsRoundCombatRunning())
        {
            // 给出提示信息
            DialogMgr.Notify(LocalizationMgr.Get("CombatSetWnd_13"));
            return;
        }

        // 副本id
        string instanceId = ME.user.Query<string>("instance/id");
        if (InstanceMgr.GetLoopFightByInstanceId(instanceId))
        {
            // 循环战斗清除指定地图的自动战斗标识
            AutoCombatMgr.RemoveAutoCombatByMapId(InstanceMgr.GetMapIdByInstanceId(instanceId));

            // 结束自动循环
            InstanceMgr.SetLoopFight(instanceId, false);
        }

        // 恢复
        TimeMgr.DoContinueCombatLogic("CombatSetPause");

        // 结束战斗
        RoundCombatMgr.EndCombat(RoundCombatConst.END_TYPE_GIVEUP);
    }


    /// <summary>
    /// 设置开关的值
    /// </summary>
    void SetToggleValue(bool value)
    {
        for (int i = 0; i < mPosBtnList.Count; i++)
        {
            GameObject go = mPosBtnList[i];
            if (go == null)
                continue;

            UIToggle toggle = go.GetComponent<UIToggle>();
            if (toggle == null)
                continue;

            toggle.value = value;
        }
    }

    /// <summary>
    /// 显示自动策略列表
    /// </summary>
    void ShowOption(bool showEffectView, bool showOder)
    {
        mEffectView.SetActive(showEffectView);

        mPloySelect.SetActive(!showEffectView);

        mOderSelect.SetActive(showOder);
    }

    /// <summary>
    /// 显示可选列表
    /// </summary>
    void ShowSelectList()
    {
        // 显示缓存的选择项
        LPCArray oderData = mCombatPloy.GetValue<LPCArray>(InstanceConst.ATTACK_ODER);
        if (oderData.Count > 0)
        {
            mOderDic.Clear();
        }
        else
            mCurrnetAtkOder = 0;

        for (int i = 0; i < mAtkSelectList.Count; i++)
        {
            if (i + 1 > mPosBtnList.Count)
                continue;

            GameObject posBtn = mPosBtnList[i];
            if (posBtn == null)
                continue;

            posBtn.gameObject.SetActive(true);

            if (i + 1 > mPosBtnLbList.Count)
            {
                posBtn.gameObject.SetActive(true);
                continue;
            }

            string type = mAtkSelectList[i].AsString;

            CsvRow data = AutoCombatSelectTypeMgr.GetSelectTypeConfig(type);
            if (data == null)
            {
                posBtn.gameObject.SetActive(false);
                continue;
            }

            mPosBtnLbList[i].text = LocalizationMgr.Get(data.Query<string>("name"));

            if (i + 1 > mPosList.Count)
            {
                posBtn.gameObject.SetActive(false);
                continue;
            }

            // 攻击顺序
            if (oderData.IndexOf(type) == -1)
            {
                mPosList[i].text = string.Empty;
                continue;
            }

            UIToggle toggle = posBtn.GetComponent<UIToggle>();
            if (toggle == null)
                continue;

            toggle.value = true;

            int atkOder = oderData.IndexOf(type) + 1;

            // 显示列表的攻击顺序
            mPosList[i].text = atkOder.ToString();

            mCurrnetAtkOder = Mathf.Max(atkOder, mCurrnetAtkOder);

            mOderDic.Add(atkOder, mPosList[i]);
        }
    }

    /// <summary>
    /// 设置攻击顺序
    /// </summary>
    void CancelAttackOder(string text)
    {
        int number = 0;

        if (!int.TryParse(text, out number))
            return;

        int cur = mCurrnetAtkOder;

        // 界面的显示逻辑
        for (int i = number; i <= cur; i++)
        {
            if (!mOderDic.ContainsKey(i))
                continue;

            UILabel label = mOderDic[i];
            if (label == null)
                continue;

            if (mOderDic.ContainsKey(i))
                mOderDic.Remove(i);

            label.text = string.Empty;

            UIToggle tg = label.transform.parent.GetComponent<UIToggle>();
            if (tg != null)
                tg.value = false;

            // 顺序攻击
            if (!mCombatPloy.ContainsKey(InstanceConst.ATTACK_ODER))
            {
                mCurrnetAtkOder--;
                return;
            }

            // 取消攻击顺序
            LPCArray para = LPCArray.Empty;

            para = mCombatPloy.GetValue<LPCArray>(InstanceConst.ATTACK_ODER);

            // 移除怪物类型
            para.RemoveAt(mCurrnetAtkOder - 1);

            mCombatPloy.Add(InstanceConst.ATTACK_ODER, para);

            mCurrnetAtkOder--;
        }

        // 缓存副本攻击策略
        AutoCombatSelectTypeMgr.SetSelectMap(ME.user, mInstanceId, mCombatPloy);
    }

    /// <summary>
    /// 添加攻击循序
    /// </summary>
    void AddAttackOder(UILabel label)
    {
        // 用于处理界面的显示逻辑
        if (mOderDic.ContainsKey(mCurrnetAtkOder))
            mOderDic[mCurrnetAtkOder] = label;
        else
            mOderDic.Add(mCurrnetAtkOder, label);

        // 用于添加实际的攻击逻循序
        LPCArray para = LPCArray.Empty;

        // 顺序攻击
        if (mCombatPloy.ContainsKey(InstanceConst.ATTACK_ODER))
            para = mCombatPloy.GetValue<LPCArray>(InstanceConst.ATTACK_ODER);

        // value : 怪物类型
        string type = mAtkSelectList[mCurrnetPos - 1].AsString;

        // 列表中包含该类型
        if (para.IndexOf(type) >= 0)
            para.Remove(type);

        para.Add(type);

        mCombatPloy.Add(InstanceConst.ATTACK_ODER, para);

        // 缓存副本攻击策略
        AutoCombatSelectTypeMgr.SetSelectMap(ME.user, mInstanceId, mCombatPloy);
    }

    /// <summary>
    /// 自动战斗策略设置默认选项
    /// </summary>
    void SetDefaultOption()
    {
        for (int i = 0; i < mPosList.Count; i++)
            mPosList[i].text = string.Empty;

        string fields = FieldsMgr.GetFieldInMapping(mCombatPloy);

        switch (fields)
        {
            // 手动操作
            case InstanceConst.MANUAL_OPREATION:
                mManualOpreationBtn.value = true;
                mAutoCombatBtn.value = false;
                mOderAttackBtn.value = false;

                OnManualOpreationBtnChange(mManualOpreationBtn.gameObject);
                break;

            // 自动战斗
            case InstanceConst.AUTO_COMBAT:
                mManualOpreationBtn.value = false;
                mAutoCombatBtn.value = true;
                mOderAttackBtn.value = false;

                OnAutoCombatBtnChange(mAutoCombatBtn.gameObject);
                break;

            // 攻击顺序
            case InstanceConst.ATTACK_ODER:
                mManualOpreationBtn.value = false;
                mAutoCombatBtn.value = false;
                mOderAttackBtn.value = true;

                OnOderAttackBtnChange(mOderAttackBtn.gameObject);
                break;

            // 默认战斗策略
            default:
                mManualOpreationBtn.value = false;
                mAutoCombatBtn.value = true;
                mOderAttackBtn.value = false;

                OnAutoCombatBtnChange(mAutoCombatBtn.gameObject);
                break;
        }

        mManualOpreationBtn.optionCanBeNone = false;
        mAutoCombatBtn.optionCanBeNone = false;
        mOderAttackBtn.optionCanBeNone = false;
    }
}


