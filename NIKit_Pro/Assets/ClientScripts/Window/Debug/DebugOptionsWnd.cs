/// <summary>
/// DebugOptionsWindow.cs
/// Created by xuhd Sec/04/2014
/// 调试选项窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LPC;
using System;

public class DebugOptionsWnd : MonoBehaviour
{
    #region 公共字段

    public TweenPosition tp;
    // 动画组件
    public bool mIsOpen = false;
    // 是否打开
    public GameObject mOptionItem;
    // 选项预设
    public Transform mGrid;
    // 选项挂载点
    public GameObject mSecondOptionWnd;
    // 通用二级选项窗口prefab
    public GameObject mThirdOptionWnd;
    // 通用三级选项窗口prefab

    #endregion

    #region 私有字段

    private Vector3 mTop = new Vector3(-380, 700, 0);
    // 位置动画的顶端
    private Vector3 mBottom = new Vector3(-380, 300, 0);
    // 位置动画的底端
    private bool mInTween = false;
    // 是否在动画中
    private List<OptionItem> mOptionItemList;
    // 选项列表
    private GameObject mCloneEquipWnd;
    // 克隆装备窗口
    private GameObject mCommonSubWnd;
    // 打开的通用子窗口
    private GameObject mThirdSubWnd;
    // 打开的通用三级子窗口
    private GameObject mCloneItemWnd;
    // 克隆道具窗口
    private GameObject mBlockAccountWnd;
    // 封禁窗口
    private GameObject mBlockTopWnd;
    // 屏蔽上榜窗口
    private GameObject mVersionTipWnd;
    // 版本提示窗口

    // 选项与回调事件的映射
    private Dictionary<int, OptionItemWnd.ItemClickedDelegate> mOptionCallbackDict = new Dictionary<int, OptionItemWnd.ItemClickedDelegate>();

    #endregion

    // Use this for initialization
    void Start()
    {
        mOptionItem.SetActive(false);
    }

    void OnDisable()
    {
        // 关闭这个窗口时，其二级窗口也要关闭
        if (mCommonSubWnd != null)
            Destroy(mCommonSubWnd);

        // 关闭这个窗口时，其三级窗口也要关闭
        if (mCommonSubWnd != null)
            Destroy(mThirdSubWnd);
    }

    #region 添加选项

    /// <summary>
    /// 在这里添加选项列表
    /// </summary>
    private void InitItemList()
    {
        if (mOptionItemList != null)
            mOptionItemList.Clear();

        mOptionItemList = new List<OptionItem>();

        mOptionItemList.Add(new OptionItem("玩家数据设置", OnPlayerDataSetting));

        mOptionItemList.Add(new OptionItem("战斗调试设置", OnCombatDebug));

        mOptionItemList.Add(new OptionItem("装备道具设置", OnEquipItemSetting));

        mOptionItemList.Add(new OptionItem("GM功能菜单", OnGameManageMenu));

        mOptionItemList.Add(new OptionItem("重载战斗文件", OnReLoadCombatFile));

        mOptionItemList.Add(new OptionItem("屏蔽技能消耗", IgnoreSkillConsume));

        mOptionItemList.Add(new OptionItem("设置攻方无敌状态", OnAttackInvincible));
        mOptionItemList.Add(new OptionItem("设置守方无敌状态", OnDefenceInvincible));

        mOptionItemList.Add(new OptionItem("设置攻方无视控制状态", OnAttackIgnoreControl));
    }

    #endregion

    #region 升级

    private void OnUpgrade(GameObject btn)
    {
#if UNITY_EDITOR

        int level = ME.user.Query<int>("level");

        if (level >= GameSettingMgr.GetSettingInt("max_user_level"))
        {
            DialogMgr.Notify("玩家等级达到上限");
            return;
        }

        // 计算玩家升级所需经验
        int costExp = StdMgr.GetUserStdExp(level + 1);

        // 通知服务器玩家升级
        Operation.CmdAdminUpgrade.Go(costExp);
#endif
    }

    #endregion

    #region 升到最大等级

    private void OnUpgradeToMaxLevel(GameObject btn)
    {
#if UNITY_EDITOR

        // 玩家最大等级
        int maxLevel = GameSettingMgr.GetSettingInt("max_user_level");

        int level = ME.user.Query<int>("level");

        if (level >= GameSettingMgr.GetSettingInt("max_user_level"))
        {
            DialogMgr.Notify("玩家等级达到上限");
            return;
        }

        // 计算玩家升级所需经验
        int costExp = 0;

        // 累计升级到满级需要的经验
        for (int i = level; i <= maxLevel; i++)
            costExp += StdMgr.GetUserStdExp(MaxAttrib.GetMaxAttrib("max_user_level"));

        // 通知服务器玩家升级
        Operation.CmdAdminUpgrade.Go(costExp);
#endif
    }

    #endregion

    #region 普通副本全开

    private void OnOpenNormalInstance(GameObject btn)
    {
#if UNITY_EDITOR
        // 通知服务器普通副本全开
        Operation.CmdAdminOpenAllInstance.Go(1);
#endif
    }

    #endregion

#region 普通副本全关

    private void OnCloseNormalInstance(GameObject btn)
    {
        #if UNITY_EDITOR
        // 通知服务器普通副本全开
        Operation.CmdAdminOpenAllInstance.Go(0);
        #endif
    }

#endregion

#region 完成指引

    private void OnFinishGuide(GameObject btn)
    {
#if UNITY_EDITOR
        // 完成指引
        Operation.CmdAdminFinishGuide.Go();
#endif
    }

#endregion

    #region 停止伤害浮动

    private void OnIgnoreDamageFloatingExchange(GameObject btn)
    {
        int floating = ME.user.QueryTemp<int>("ignore_damage_floating_exchange");

        if (floating == 1)
        {
            ME.user.SetTemp("ignore_damage_floating_exchange", LPCValue.Create(0));
            DialogMgr.Notify("开启伤害浮动！");
        }
        else
        {
            ME.user.SetTemp("ignore_damage_floating_exchange", LPCValue.Create(1));
            DialogMgr.Notify("关闭伤害浮动！");
        }
    }

    #endregion

    #region 重载技能

    private void OnReLoadCombatFile(GameObject item)
    {
#if UNITY_EDITOR

        // 重新载入skill_action文件
        CombatActionMgr.ReloadSkillActionData();

        // 删除原来的旧文件
        File.Delete(ConfigMgr.ETC_PATH + "/skill.bytes");
        File.Delete(ConfigMgr.ETC_PATH + "/skill.bytes.meta");

        File.Delete(ConfigMgr.ETC_PATH + "/status.bytes");
        File.Delete(ConfigMgr.ETC_PATH + "/status.bytes.meta");

        // 重新载入技能配置表信息
        CsvFileMgr.Save(Application.dataPath + "/../../server/server_scripts/etc/gs/skill.csv", true);
        CsvFileMgr.Save(Application.dataPath + "/LocalSet/status.csv", false);

        // 刷新资源
        UnityEditor.AssetDatabase.Refresh();

        // 重新初始化技能状态
        SkillMgr.Init();
        StatusMgr.Init();
#endif

        Debug.Log("战斗相关文件重载成功！");
    }

    #endregion

    #region 屏蔽技能消耗

    private void IgnoreSkillConsume(GameObject go)
    {
        int ignoreCost = ME.user.QueryTemp<int>("ignore_cost");
        int ignoreCd = ME.user.QueryTemp<int>("ignore_cd");

        // 开启关闭技能消耗
        if (ignoreCost == 1 && ignoreCd == 1)
        {
            ME.user.SetTemp("ignore_cost", LPCValue.Create(0));
            ME.user.SetTemp("ignore_cd", LPCValue.Create(0));
            DialogMgr.Notify("关闭技能消耗！");
        }
        else
        {
            ME.user.SetTemp("ignore_cost", LPCValue.Create(1));
            ME.user.SetTemp("ignore_cd", LPCValue.Create(1));
            DialogMgr.Notify("开启技能消耗！");
        }
    }

    #endregion

    #region 无敌状态

    private void OnAttackInvincible(GameObject go)
    {
        int attackInvincible = ME.user.QueryTemp<int>("attack_invincible");

        // 开启关闭攻方无敌
        if (attackInvincible == 1)
        {
            ME.user.SetTemp("attack_invincible", LPCValue.Create(0));
            DialogMgr.Notify("关闭攻方无敌！");
        }
        else
        {
            ME.user.SetTemp("attack_invincible", LPCValue.Create(1));
            DialogMgr.Notify("开启攻方无敌！");
        }
    }

    private void OnDefenceInvincible(GameObject go)
    {
        int invincible = ME.user.QueryTemp<int>("defence_invincible");

        // 开启关闭防守方无敌
        if (invincible == 1)
        {
            ME.user.SetTemp("defence_invincible", LPCValue.Create(0));
            DialogMgr.Notify("关闭防守方无敌！");
        }
        else
        {
            ME.user.SetTemp("defence_invincible", LPCValue.Create(1));
            DialogMgr.Notify("开启防守方无敌！");
        }
    }

    private void OnAttackIgnoreControl(GameObject go)
    {
        int ignoreCtrl = ME.user.QueryTemp<int>("attack_ignore_ctrl");

        // 开启关闭攻方免疫控制状态
        if (ignoreCtrl == 1)
        {
            ME.user.SetTemp("attack_ignore_ctrl", LPCValue.Create(0));
            DialogMgr.Notify("关闭攻方免疫控制状态！");
        }
        else
        {
            ME.user.SetTemp("attack_ignore_ctrl", LPCValue.Create(1));
            DialogMgr.Notify("开启攻方免疫控制状态！");
        }
    }

    private void OnSwitchPostion(GameObject go)
    {
        int switchPostion = ME.user.QueryTemp<int>("switch_postion");

        // 设置变换位置标识
        ME.user.SetTemp("switch_postion", (switchPostion == 0 ? LPCValue.Create(1) : LPCValue.Create(0)));
    }

    private void OnControlEnemy(GameObject go)
    {
        int controlEnemy = ME.user.QueryTemp<int>("control_enemy");

        // 控制敌方怪物
        ME.user.SetTemp("control_enemy", (controlEnemy == 0 ? LPCValue.Create(1) : LPCValue.Create(0)));
    }

    private void OnChangeShowDebugInfo(GameObject go)
    {
        int showDebugInfo = ME.user.QueryTemp<int>("show_debug_info");

        // 控制敌方怪物
        ME.user.SetTemp("show_debug_info", (showDebugInfo == 0 ? LPCValue.Create(1) : LPCValue.Create(0)));

        // 获取场上的所有战斗对象
        List<Property> combatObjects = RoundCombatMgr.GetPropertyList();

        if (combatObjects.Count == 0)
            return;

        foreach (Property ob in combatObjects)
            ob.Actor.ShowValueWnd(showDebugInfo == 0);
    }

    /// <summary>
    /// 战斗加速
    /// </summary>
    private void OnCombatSpeedUp(GameObject go)
    {
        if (ME.user == null)
            return;

        LPCValue time_scale = ME.user.QueryTemp<LPCValue>("time_scale");

        if (time_scale == null)
        {
            ME.user.SetTemp("time_scale", LPCValue.Create(10));

            TimeMgr.TimeScale = 10;

            DialogMgr.Notify("开启加速");
        }
        else
        {
            ME.user.DeleteTemp("time_scale");

            TimeMgr.TimeScale = 1;

            DialogMgr.Notify("关闭加速");
        }
    }


    #endregion

    #region 清除怪物

    private void OnDoDie(GameObject btn)
    {
        // 收集目标
        List<Property> entityList = RoundCombatMgr.GetPropertyList(CampConst.CAMP_TYPE_DEFENCE);

        // 没有实体
        if (entityList.Count == 0)
            return;

        // 清除怪物
        foreach (Property ob in entityList)
        {
            // 对象不存在
            if (ob == null)
                continue;

            // 如果怪物已经死亡不处理
            if (ob.CheckStatus("DIED"))
                continue;

            // 执行死亡操作
            (ob as Char).DoDie();
        }

        // 结束战斗
        RoundCombatMgr.EndRoundCombat(true);

        LPCMapping mRoundActions = LPCMapping.Empty;
        mRoundActions.Add(1, LPCArray.Empty);

        // 抛出胜利事件
        LPCMapping ePara = new LPCMapping();
        ePara.Add("camp_id", CampConst.CAMP_TYPE_ATTACK);
        ePara.Add("round_type", RoundCombatConst.ROUND_TYPE_INSTANCE);
        ePara.Add("round_actions", mRoundActions);
        EventMgr.FireEvent(EventMgrEventType.EVENT_ROUND_COMBAT_END, MixedValue.NewMixedValue<LPCMapping>(ePara));
    }

    #endregion

    #region 清除攻方宠物

    void OnClearUserPet(GameObject go)
    {
        // 收集目标
        List<Property> userList = RoundCombatMgr.GetPropertyList(CampConst.CAMP_TYPE_ATTACK);

        // 没有实体
        if (userList.Count == 0)
            return;

        // 清除宠物
        foreach (Property ob in userList)
        {
            // 对象不存在
            if (ob == null)
                continue;

            // 如果怪物已经死亡不处理
            if (ob.CheckStatus("DIED"))
                continue;

            // 执行死亡操作
            (ob as Char).DoDie();
        }

        // 结束战斗
        RoundCombatMgr.EndRoundCombat(true);

        LPCMapping mRoundActions = LPCMapping.Empty;
        mRoundActions.Add(1, LPCArray.Empty);

        // 抛出胜利事件
        LPCMapping ePara = new LPCMapping();
        ePara.Add("camp_id", CampConst.CAMP_TYPE_DEFENCE);
        ePara.Add("round_type", RoundCombatConst.ROUND_TYPE_INSTANCE);
        ePara.Add("round_actions", mRoundActions);
        EventMgr.FireEvent(EventMgrEventType.EVENT_ROUND_COMBAT_END, MixedValue.NewMixedValue<LPCMapping>(ePara));
    }

    #endregion

    #region 克隆道具

    private void OnCloneItem(GameObject item)
    {
        Debug.Log("克隆道具被点击");

        OptionItemWnd itemWnd = item.GetComponent<OptionItemWnd>();
        if (mCommonSubWnd != null && mCommonSubWnd == itemWnd.GetSubWnd())
        {
            Destroy(mCommonSubWnd);
            return;
        }

        GameObject wndOb;
        if (mCloneItemWnd == null)
        {
            // 加载窗口对象实例
            wndOb = ResourceMgr.Load("Assets/Prefabs/Window/Debug/DebugCloneItemWnd.prefab") as GameObject;

            if (wndOb == null)
            {
                LogMgr.Trace("加载DebugCloneItemWnd失败");
                return;
            }

            GameObject wnd = GameObject.Instantiate(wndOb, wndOb.transform.localPosition, Quaternion.identity) as GameObject;
            wnd.name = wndOb.name;

            // 挂到UIRoot下
            Transform ts = wnd.transform;
            ts.parent = WindowMgr.UIRoot;
            ts.localPosition = wndOb.transform.localPosition;
            ts.localScale = Vector3.one;
            mCloneItemWnd = wnd;
            mCloneItemWnd.SetActive(true);
        }
        else
            Destroy(mCloneItemWnd);
    }

    /// <summary>
    /// 克隆装备
    /// </summary>
    private void OnCloneEquip(GameObject go)
    {
        OptionItemWnd itemWnd = go.GetComponent<OptionItemWnd>();
        if (mCommonSubWnd != null && mCommonSubWnd == itemWnd.GetSubWnd())
        {
            Destroy(mCommonSubWnd);
            return;
        }

        GameObject wndOb;
        // 加载窗口对象实例
        wndOb = ResourceMgr.Load("Assets/Prefabs/Window/Debug/DebugCloneEquipWnd.prefab") as GameObject;

        if (wndOb == null)
        {
            LogMgr.Trace("DebugCloneEquipWnd");
            return;
        }

        GameObject wnd = GameObject.Instantiate(wndOb, wndOb.transform.localPosition, Quaternion.identity) as GameObject;
        wnd.name = wndOb.name;

        // 挂到UIRoot下
        Transform ts = wnd.transform;
        ts.parent = WindowMgr.UIRoot;
        ts.localPosition = wndOb.transform.localPosition;
        ts.localScale = Vector3.one;
        mCloneItemWnd = wnd;
        mCloneItemWnd.SetActive(true);
    }

    /// <summary>
    /// 克隆自定义装备
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnCloneCustomEquip(GameObject go)
    {
        OptionItemWnd itemWnd = go.GetComponent<OptionItemWnd>();
        if (mCommonSubWnd != null && mCommonSubWnd == itemWnd.GetSubWnd())
        {
            Destroy(mCommonSubWnd);
            return;
        }

        GameObject wndOb;
        // 加载窗口对象实例
        wndOb = ResourceMgr.Load("Assets/Prefabs/Window/Debug/DebugCustomEquipWnd.prefab") as GameObject;

        if (wndOb == null)
        {
            LogMgr.Trace("DebugCustomEquipWnd");
            return;
        }

        GameObject wnd = GameObject.Instantiate(wndOb, wndOb.transform.localPosition, Quaternion.identity) as GameObject;
        wnd.name = wndOb.name;

        // 挂到UIRoot下
        Transform ts = wnd.transform;
        ts.parent = WindowMgr.UIRoot;
        ts.localPosition = wndOb.transform.localPosition;
        ts.localScale = Vector3.one;
        mCloneItemWnd = wnd;
        mCloneItemWnd.SetActive(true);
    }

    /// <summary>
    /// 克隆宠物窗口
    /// </summary>
    private void OnClonePet(GameObject go)
    {
        OptionItemWnd itemWnd = go.GetComponent<OptionItemWnd>();
        if (mCommonSubWnd != null && mCommonSubWnd == itemWnd.GetSubWnd())
        {
            Destroy(mCommonSubWnd);
            return;
        }

        GameObject wndOb;
        // 加载窗口对象实例
        wndOb = ResourceMgr.Load("Assets/Prefabs/Window/Debug/DebugClonePetWnd.prefab") as GameObject;

        if (wndOb == null)
        {
            LogMgr.Trace("DebugCloneEquipWnd");
            return;
        }

        GameObject wnd = GameObject.Instantiate(wndOb, wndOb.transform.localPosition, Quaternion.identity) as GameObject;
        wnd.name = wndOb.name;

        // 挂到UIRoot下
        Transform ts = wnd.transform;
        ts.parent = WindowMgr.UIRoot;
        ts.localPosition = wndOb.transform.localPosition;
        ts.localScale = Vector3.one;
        mCloneItemWnd = wnd;
        mCloneItemWnd.SetActive(true);
    }

    #endregion

    #region 清除道具

    private void OnClearBaggage(GameObject item)
    {
        Debug.Log("清除道具被点击");

        OptionItemWnd itemWnd = item.GetComponent<OptionItemWnd>();
        if (mCommonSubWnd != null && mCommonSubWnd == itemWnd.GetSubWnd())
        {
            Destroy(mCommonSubWnd);
            return;
        }

        GameObject wndOb;
        if (mCloneItemWnd == null)
        {
            // 加载窗口对象实例
            wndOb = ResourceMgr.Load("Assets/Prefabs/Window/Debug/DebugClearBaggage.prefab") as GameObject;

            if (wndOb == null)
            {
                LogMgr.Trace("加载DebugClearBaggage失败");
                return;
            }

            GameObject wnd = GameObject.Instantiate(wndOb, wndOb.transform.localPosition, Quaternion.identity) as GameObject;
            wnd.name = wndOb.name;

            // 挂到UIRoot下
            Transform ts = wnd.transform;
            ts.parent = WindowMgr.UIRoot;
            ts.localPosition = wndOb.transform.localPosition;
            ts.localScale = Vector3.one;
            mCloneItemWnd = wnd;
            mCloneItemWnd.SetActive(true);
        }
        else
            Destroy(mCloneItemWnd);
    }

    #endregion

    #region 兑换码功能开关

    private void OnRedeemKeySwith(GameObject item)
    {
        // 不在游戏中不让使用该功能
        if (!ME.isInGame)
            return;
        
        List<OptionItem> itemList = new List<OptionItem>();
        itemList.Add(new OptionItem("开启兑换码功能", OnOpenRedeemKey));
        itemList.Add(new OptionItem("关闭兑换码功能", OnCloseRedeemKey));
        itemList.Add(new OptionItem("获取兑换码key", OnGetRedeemKey));

        // 创建下一级子选项窗口
        SetThirdLevelOptions(item, itemList);
    }

    #endregion

    #region 发送邮件

    private void OnSendExpress(GameObject item)
    {
        // 不在游戏中不让使用该功能
        if (!ME.isInGame)
            return;

        List<OptionItem> itemList = new List<OptionItem>();
        itemList.Add(new OptionItem("发送给指定玩家", OnSendToSpecify));
        itemList.Add(new OptionItem("发送给全部玩家", OnSendToAll));

        // 创建下一级子选项窗口
        SetThirdLevelOptions(item, itemList);
    }

    /// <summary>
    /// 发送给指定玩家
    /// </summary>
    private void OnSendToSpecify(GameObject ob)
    {
        #if UNITY_EDITOR

        GameObject inputWnd = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(GetPrefabPath("Prefabs/Window/Debug/DebugSendMailWnd.prefab"));
        if (inputWnd == null)
        {
            LogMgr.Trace("DebugSendMailWnd.prefab加载不到");
            return;
        }

        GameObject instance = Instantiate(inputWnd) as GameObject;
        instance.name = inputWnd.name;

        instance.transform.parent = WindowMgr.UIRoot;
        instance.transform.localScale = Vector3.one;
        instance.transform.localPosition = inputWnd.transform.localPosition;

        instance.GetComponent<DebugSendMailWnd>().InitWnd("发送给指定玩家", false, OnSendMailCallback);
        instance.SetActive(true);

        #endif
    }

    /// <summary>
    /// 发送给全部玩家
    /// </summary>
    private void OnSendToAll(GameObject ob)
    {
        #if UNITY_EDITOR

        GameObject inputWnd = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(GetPrefabPath("Prefabs/Window/Debug/DebugSendMailWnd.prefab"));
        if (inputWnd == null)
        {
            LogMgr.Trace("DebugSendMailWnd.prefab加载不到");
            return;
        }

        GameObject instance = Instantiate(inputWnd) as GameObject;
        instance.name = inputWnd.name;

        instance.transform.parent = WindowMgr.UIRoot;
        instance.transform.localScale = Vector3.one;
        instance.transform.localPosition = inputWnd.transform.localPosition;

        instance.GetComponent<DebugSendMailWnd>().InitWnd("发送给全部玩家", true, OnSendMailCallback);
        instance.SetActive(true);

        #endif
    }

    /// <summary>
    /// 发送邮件回调
    /// </summary>
    private void OnSendMailCallback(LPCMapping para)
    {
        if (para == null)
        {
            LogMgr.Trace("返回参数为null或不含值");
            return;
        }

        // 发送消息
        Operation.CmdAdminExpressSend.Go(para["addressee"].AsString, para["expire"].AsInt, 
            para["title"].AsString, para["message"].AsString,
            para["belonging_list"].AsArray);
    }

    #endregion

    #region 聊天禁言

    /// <summary>
    /// 增加金币
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnSetForbidChat(GameObject go)
    {
        #if UNITY_EDITOR

        GameObject inputWnd = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(GetPrefabPath("Prefabs/Window/Debug/DebugForbidChat.prefab"));
        if (inputWnd == null)
        {
            LogMgr.Trace("DebugForbidChat.prefab加载不到");
            return;
        }

        GameObject instance = Instantiate(inputWnd) as GameObject;
        instance.name = inputWnd.name;

        instance.transform.parent = WindowMgr.UIRoot;
        instance.transform.localScale = Vector3.one;
        instance.transform.localPosition = inputWnd.transform.localPosition;

        // 激活窗口
        instance.SetActive(true);

        #endif
    }

    #endregion

    #region 玩家数据设置

    private void OnPlayerDataSetting(GameObject item)
    {
        Debug.Log("玩家数据设置被点击");

        List<OptionItem> itemList = new List<OptionItem>();
        itemList.Add(new OptionItem("添加玩家属性", OnAddAttrib));
        itemList.Add(new OptionItem("升级", OnUpgrade));
        itemList.Add(new OptionItem("升到最大等级", OnUpgradeToMaxLevel));
        itemList.Add(new OptionItem("副本全开", OnOpenNormalInstance));
        itemList.Add(new OptionItem("副本全关", OnCloseNormalInstance));
        itemList.Add(new OptionItem("完成指引", OnFinishGuide));
        itemList.Add(new OptionItem("开启版署模式", OpenGappWorld));
        itemList.Add(new OptionItem("关闭版署模式", CloseGappWorld));
        itemList.Add(new OptionItem("开启/关闭付费", OpenOrClosePay));

        // 创建二级子选项窗口
        SetSecondLevelOptions(item, itemList);
    }

    /// <summary>
    /// 开启付费
    /// </summary>
    private void OpenOrClosePay(GameObject item)
    {
#if UNITY_EDITOR

        if(ME.user == null)
            return;

        if (ME.user.QueryTemp<int>("close_pay") == 1)
            Operation.CmdAdminClosePay.Go(0);
        else
            Operation.CmdAdminClosePay.Go(1);
#endif
    }


    /// <summary>
    /// 开启版署模式
    /// </summary>
    private void OpenGappWorld(GameObject item)
    {
        #if UNITY_EDITOR

        Operation.CmdAdminSetGappWrold.Go(true);

        #endif
    }

    /// <summary>
    /// 关闭版署模式
    /// </summary>
    private void CloseGappWorld(GameObject item)
    {
        #if UNITY_EDITOR

        Operation.CmdAdminSetGappWrold.Go(false);

        #endif
    }

    /// <summary>
    /// 关闭社交功能
    /// </summary>
    private void OnCloseSocail(GameObject go)
    {
        // 关闭社交功能
        Operation.CmdAdminSetSocialFunction.Go(false);
    }

    /// <summary>
    /// 开启社交功能
    /// </summary>
    private void OnOpenSocail(GameObject go)
    {
        // 开启社交功能
        Operation.CmdAdminSetSocialFunction.Go(true);
    }

    /// <summary>
    /// 开启兑换码功能
    /// </summary>
    private void OnOpenRedeemKey(GameObject ob)
    {
        // 开启兑换码功能
        Operation.CmdAdminSetRedeemKeyFunction.Go(true);
    }

    /// <summary>
    /// 关闭兑换码功能
    /// </summary>
    private void OnCloseRedeemKey(GameObject ob)
    {
        // 关闭兑换码功能
        Operation.CmdAdminSetRedeemKeyFunction.Go(false);
    }

    private void OnGetRedeemKey(GameObject ob)
    {
        int is_redeem_key_on = ME.user.QueryTemp<int>("is_redeem_key_on");
        DialogMgr.Notify("is_redeem_key_on: " + is_redeem_key_on);
    }

    /// <summary>
    /// 添加玩家属性
    /// </summary>
    private void OnAddAttrib(GameObject go)
    {
        #if UNITY_EDITOR

        GameObject inputWnd = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(GetPrefabPath("Prefabs/Window/Debug/DebugCommonInputWnd.prefab"));
        if (inputWnd == null)
        {
            LogMgr.Trace("DebugCommonInputWnd.prefab加载不到");
            return;
        }

        GameObject instance = Instantiate(inputWnd) as GameObject;
        instance.name = inputWnd.name;

        instance.transform.parent = WindowMgr.UIRoot;
        instance.transform.localScale = Vector3.one;
        instance.transform.localPosition = inputWnd.transform.localPosition;

        instance.GetComponent<DebugCommonInputWnd>().InitWnd("需要增加的属性数据:", OnAddAttribCallBack);
        instance.SetActive(true);

        #endif
    }

    /// <summary>
    /// 属性添加按钮点击回调
    /// </summary>
    private void OnAddAttribCallBack(LPCMapping para)
    {
        if (para == null || para.Count == 0)
            return;

        if (!para.ContainsKey("value") || !para["value"].IsString)
            return;

        LPCValue v = LPCRestoreString.RestoreFromString(para["value"].AsString);
        if (v == null || !v.IsMapping)
        {
            LogMgr.Trace(string.Format("数据格式不正确: {0}", para["value"].AsString));
            return;
        }

        // 通知服务器添加属性
        Operation.CmdAdminAddAttrib.Go(v.AsMapping);
    }

    #endregion

    #region 战斗调试

    private void OnCombatDebug(GameObject item)
    {
        List<OptionItem> itemList = new List<OptionItem>();
        itemList.Add(new OptionItem("停止伤害浮动", OnIgnoreDamageFloatingExchange));
        itemList.Add(new OptionItem("清除怪物", OnDoDie));
        itemList.Add(new OptionItem("清除攻方宠物", OnClearUserPet));
        itemList.Add(new OptionItem("攻守双方换位", OnSwitchPostion));
        itemList.Add(new OptionItem("控制敌方怪物", OnControlEnemy));
        itemList.Add(new OptionItem("打开/关闭宠物数值显示", OnChangeShowDebugInfo));
        itemList.Add(new OptionItem("战斗加速", OnCombatSpeedUp));

        // 创建二级子选项窗口
        SetSecondLevelOptions(item, itemList);
    }

    #endregion

    #region 装备道具设置

    private void OnEquipItemSetting(GameObject item)
    {
        List<OptionItem> itemList = new List<OptionItem>();
        itemList.Add(new OptionItem("通用克隆道具", OnCloneItem));
        itemList.Add(new OptionItem("克隆装备", OnCloneEquip));
        itemList.Add(new OptionItem("克隆自定义装备", OnCloneCustomEquip));
        itemList.Add(new OptionItem("克隆宠物", OnClonePet));
        itemList.Add(new OptionItem("清除包裹", OnClearBaggage));

        // 创建二级子选项窗口
        SetSecondLevelOptions(item, itemList);
    }

    #endregion

    #region 游戏管理菜单

    private void OnGameManageMenu(GameObject item)
    {
        List<OptionItem> itemList = new List<OptionItem>();
        itemList.Add(new OptionItem("GM封号", OnBlockUser));
        itemList.Add(new OptionItem("GM屏蔽上榜", OnBlockTop));
        itemList.Add(new OptionItem("兑换码功能", OnRedeemKeySwith));
        itemList.Add(new OptionItem("发送邮件", OnSendExpress));
        itemList.Add(new OptionItem("聊天禁言", OnSetForbidChat));
        itemList.Add(new OptionItem("开启活动", AddActivity));
        itemList.Add(new OptionItem("关闭活动", RemoveActivity));
        itemList.Add(new OptionItem("开启新功能", OpenNewFunction));

        // 创建二级子选项窗口
        SetSecondLevelOptions(item, itemList);
    }

    /// <summary>
    /// GM封号
    /// </summary>
    private void OnBlockUser(GameObject item)
    {
        // 不在游戏中不让使用该功能
        if (!ME.isInGame)
            return;

        OptionItemWnd itemWnd = item.GetComponent<OptionItemWnd>();
        if (mCommonSubWnd != null && mCommonSubWnd == itemWnd.GetSubWnd())
        {
            Destroy(mCommonSubWnd);
            return;
        }

        GameObject wndOb;
        if (mBlockAccountWnd == null)
        {
            // 加载窗口对象实例
            wndOb = ResourceMgr.Load("Assets/Prefabs/Window/Debug/BlockAccountWnd.prefab") as GameObject;

            if (wndOb == null)
            {
                LogMgr.Trace("加载BlockAccountWnd失败");
                return;
            }

            GameObject wnd = GameObject.Instantiate(wndOb, wndOb.transform.localPosition, Quaternion.identity) as GameObject;
            wnd.name = wndOb.name;

            // 挂到UIRoot下
            Transform ts = wnd.transform;
            ts.parent = WindowMgr.UIRoot;
            ts.localPosition = wndOb.transform.localPosition;
            ts.localScale = Vector3.one;
            mBlockAccountWnd = wnd;
            mBlockAccountWnd.SetActive(true);
        }
        else
            Destroy(mBlockAccountWnd);
    }

    /// <summary>
    /// 屏蔽上榜
    /// </summary>
    private void OnBlockTop(GameObject item)
    {
        // 不在游戏中不让使用该功能
        if (!ME.isInGame)
            return;

        OptionItemWnd itemWnd = item.GetComponent<OptionItemWnd>();
        if (mCommonSubWnd != null && mCommonSubWnd == itemWnd.GetSubWnd())
        {
            Destroy(mCommonSubWnd);
            return;
        }

        GameObject wndOb;
        if (mBlockTopWnd == null)
        {
            // 加载窗口对象实例
            wndOb = ResourceMgr.Load("Assets/Prefabs/Window/Debug/BlockTopWnd.prefab") as GameObject;

            if (wndOb == null)
            {
                LogMgr.Trace("加载BlockTopWnd失败");
                return;
            }

            GameObject wnd = GameObject.Instantiate(wndOb, wndOb.transform.localPosition, Quaternion.identity) as GameObject;
            wnd.name = wndOb.name;

            // 挂到UIRoot下
            Transform ts = wnd.transform;
            ts.parent = WindowMgr.UIRoot;
            ts.localPosition = wndOb.transform.localPosition;
            ts.localScale = Vector3.one;
            mBlockTopWnd = wnd;
            mBlockTopWnd.SetActive(true);
        }
        else
            Destroy(mBlockTopWnd);
    }

    /// <summary>
    /// 添加活动
    /// </summary>
    private void AddActivity(GameObject item)
    {
        #if UNITY_EDITOR

        GameObject inputWnd = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(GetPrefabPath("Prefabs/Window/Debug/DebugCommonInputWnd.prefab"));
        if (inputWnd == null)
        {
            LogMgr.Trace("DebugCommonInputWnd.prefab加载不到");
            return;
        }

        GameObject instance = Instantiate(inputWnd) as GameObject;
        instance.name = inputWnd.name;

        instance.transform.parent = WindowMgr.UIRoot;
        instance.transform.localScale = Vector3.one;
        instance.transform.localPosition = inputWnd.transform.localPosition;

        instance.GetComponent<DebugCommonInputWnd>().InitWnd("请输入活动数据:", OnAddActivityCallBack);
        instance.SetActive(true);

        #endif
    }

    /// <summary>
    /// 添加活动回调
    /// </summary>
    private void OnAddActivityCallBack(LPCMapping para)
    {
        if (para == null || para.Count == 0)
            return;

        if (!para.ContainsKey("value") || !para["value"].IsString)
            return;

        LPCMapping data = LPCRestoreString.RestoreFromString(para["value"].AsString).AsMapping;

        if (data == null || data.Count == 0)
            return;

        // 通知服务器添加活动
        Operation.CmdAdminAddActivity.Go(data);
    }

    /// <summary>
    /// 移除活动
    /// </summary>
    private void RemoveActivity(GameObject item)
    {
        #if UNITY_EDITOR

        GameObject inputWnd = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(GetPrefabPath("Prefabs/Window/Debug/DebugCommonInputWnd.prefab"));
        if (inputWnd == null)
        {
            LogMgr.Trace("DebugCommonInputWnd.prefab加载不到");
            return;
        }

        GameObject instance = Instantiate(inputWnd) as GameObject;
        instance.name = inputWnd.name;

        instance.transform.parent = WindowMgr.UIRoot;
        instance.transform.localScale = Vector3.one;
        instance.transform.localPosition = inputWnd.transform.localPosition;

        instance.GetComponent<DebugCommonInputWnd>().InitWnd("请输入需要关闭的活动ID:", OnRemoveActivityCallBack);
        instance.SetActive(true);

        #endif
    }

    /// <summary>
    /// 移除活动回调
    /// </summary>
    private void OnRemoveActivityCallBack(LPCMapping para)
    {
        if (para == null || para.Count == 0)
            return;

        if (!para.ContainsKey("value") || !para["value"].IsString)
            return;

        // 通知服务器移除活动
        Operation.CmdAdminRemoveActivity.Go(para.GetValue<string>("value"));
    }

    /// <summary>
    /// 开启新功能
    /// </summary>
    private void OpenNewFunction(GameObject item)
    {
        #if UNITY_EDITOR

        GameObject inputWnd = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(GetPrefabPath("Prefabs/Window/Debug/DebugCommonInputWnd.prefab"));
        if (inputWnd == null)
        {
            LogMgr.Trace("DebugCommonInputWnd.prefab加载不到");
            return;
        }

        GameObject instance = Instantiate(inputWnd) as GameObject;
        instance.name = inputWnd.name;

        instance.transform.parent = WindowMgr.UIRoot;
        instance.transform.localScale = Vector3.one;
        instance.transform.localPosition = inputWnd.transform.localPosition;

        instance.GetComponent<DebugCommonInputWnd>().InitWnd("请输入需要开启的新功能名:", OnOpenNewFunctionCallBack);
        instance.SetActive(true);

        #endif
    }

    /// <summary>
    /// 新功能开启回调
    /// </summary>
    private void OnOpenNewFunctionCallBack(LPCMapping para)
    {
        if (para == null || para.Count == 0)
            return;

        if (!para.ContainsKey("value") || !para["value"].IsString)
            return;

        // 通知服务器开启新功能
        Operation.CmdAdminOpenNewFunction.Go(para.GetValue<string>("value"));
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 打开窗口
    /// </summary>
    public void OpenWnd()
    {
        InitItemList();

        // 加载选项列表
        LoadOptionView();

        // 播放动画
        PlayAnimation();
    }

    /// <summary>
    /// 隐藏窗口
    /// </summary>
    public void HideWnd()
    {

        if (mSecondOptionWnd != null)
            Destroy(mCommonSubWnd);

        if (mThirdOptionWnd != null)
            Destroy(mThirdSubWnd);

        // 播放动画
        PlayAnimation();
    }

    #endregion

    #region 内部函数

    /// <summary>
    /// 播放开关动画
    /// </summary>
    private void PlayAnimation()
    {
        // 正在动画中，不响应
        if (mInTween)
            return;

        mInTween = true;
        if (mIsOpen)
        {
            tp.from = mBottom;
            tp.to = mTop;
        }
        else
        {
            tp.from = mTop;
            tp.to = mBottom;
        }

        tp.AddOnFinished(OnTweenFinish);
        mIsOpen = !mIsOpen;
        tp.enabled = true;
        tp.ResetToBeginning();
    }

    /// <summary>
    /// 动画结束
    /// </summary>
    private void OnTweenFinish()
    {
        mInTween = false;
        mGrid.GetComponent<UIGrid>().Reposition();
    }

    private void LoadOptionView()
    {
        UIGrid grid = mGrid.GetComponent<UIGrid>();

        // 先清除原先的选项
        List<Transform> childList = grid.GetChildList();
        if (childList.Count > 0)
        {
            foreach (Transform tf in childList)
            {
                GameObject go = tf.gameObject;
                Destroy(go);
            }

            grid.repositionNow = true;
        }

        // 清空回调
        mOptionCallbackDict.Clear();

        GameObject item;
        if (mOptionItemList != null && mOptionItemList.Count > 0)
        {
            for (int i = 0; i < mOptionItemList.Count; ++i)
            {
                item = Instantiate(mOptionItem) as GameObject;
                item.name = mOptionItemList[i].OptionName;
                item.GetComponent<OptionItemWnd>().SetOptionName(mOptionItemList[i].OptionName);
                item.GetComponent<OptionItemWnd>().AddOnClickDelegate(OnOptionClicked);

                // 登记回调
                mOptionCallbackDict.Add(item.GetInstanceID(), mOptionItemList[i].Callback);

                Transform ts = item.transform;
                ts.parent = mGrid;
                ts.localPosition = Vector3.zero;
                ts.localScale = Vector3.one;
                ts.gameObject.SetActive(true);
            }

            grid.Reposition();

            int totalCount = mOptionItemList.Count;
            UIPanel panel = gameObject.GetComponent<UIPanel>();
            panel.SetRect(0, -totalCount * (25 + totalCount / 4), 200, totalCount * (80 + totalCount));
        }
    }

    /// <summary>
    /// 选项被选中
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnOptionClicked(GameObject go)
    {
        // 关闭其他已打开的二级选项窗口
        if (mCommonSubWnd != null && mCommonSubWnd != go.GetComponent<OptionItemWnd>().GetSubWnd())
            Destroy(mCommonSubWnd);

        // 关闭其他已打开的二级选项窗口
        if (mThirdSubWnd != null && mThirdSubWnd != go.GetComponent<OptionItemWnd>().GetSubWnd())
            Destroy(mThirdSubWnd);

        mOptionCallbackDict[go.GetInstanceID()](go);
    }

    /// <summary>
    /// 设置二级选项
    /// </summary>
    private void SetSecondLevelOptions(GameObject item, List<OptionItem> itemList)
    {
        // 析构三级菜单
        if (mThirdSubWnd != null)
            Destroy(mThirdSubWnd);

        OptionItemWnd itemWnd = item.GetComponent<OptionItemWnd>();
        if (mCommonSubWnd != null)
        {
            // 析构三级菜单
            Destroy(mCommonSubWnd);

            // 关闭窗口
            if (mCommonSubWnd == itemWnd.GetSubWnd())
                return;
        }

        // 创建二级子选项窗口
        GameObject subWnd = Instantiate(mSecondOptionWnd) as GameObject;
        subWnd.name = mSecondOptionWnd.name;

        Transform ts = subWnd.transform;
        ts.parent = WindowMgr.UIRoot;
        ts.localScale = mSecondOptionWnd.transform.localScale;
        ts.localPosition = mSecondOptionWnd.transform.localPosition;

        SecondOptionWnd sow = subWnd.GetComponent<SecondOptionWnd>();
        sow.OpenSecondOptionWnd(itemList);

        // 注册为当前按钮的子窗口
        itemWnd.RegisterSubWnd(subWnd);

        // 记录下当前打开的子窗口
        mCommonSubWnd = subWnd;
    }

    /// <summary>
    /// 设置三级选项
    /// </summary>
    private void SetThirdLevelOptions(GameObject item, List<OptionItem> itemList)
    {
        OptionItemWnd itemWnd = item.GetComponent<OptionItemWnd>();
        if (mThirdSubWnd != null)
        {
            // 析构三级菜单
            Destroy(mThirdSubWnd);

            // 关闭窗口
            if (mThirdSubWnd == itemWnd.GetSubWnd())
                return;
        }

        // 创建三级子选项窗口
        GameObject subWnd = Instantiate(mThirdOptionWnd) as GameObject;
        subWnd.name = mThirdOptionWnd.name;

        Transform ts = subWnd.transform;
        ts.parent = WindowMgr.UIRoot;
        ts.localScale = mThirdOptionWnd.transform.localScale;
        ts.localPosition = mThirdOptionWnd.transform.localPosition;

        SecondOptionWnd sow = subWnd.GetComponent<SecondOptionWnd>();
        sow.OpenSecondOptionWnd(itemList);

        // 注册为当前按钮的子窗口
        itemWnd.RegisterSubWnd(subWnd);

        // 记录下当前打开的子窗口
        mThirdSubWnd = subWnd;
    }

    private string GetPrefabPath(string path)
    {
#if UNITY_EDITOR
        return string.Format("Assets/{0}", path);
#else
        return path.Replace(".prefab", string.Empty);
#endif
    }

    #endregion
}
