/// <summary>
/// SpecialGuideWnd.cs
/// Created by fengsc 2017/11/06
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class SpecialGuideWnd : WindowBase<SpecialGuideWnd>
{
    // 上一个关卡
    public GameObject mLastLevelBtn;
    public UILabel mLastLevelBtnLb;

    // 强化装备按钮
    public GameObject mStrengthenBtn;
    public UILabel mStrengthenBtnLb;

    // 北镜圣域按钮
    public GameObject mDungeonsBtn;
    public UILabel mDungeonsBtnLb;

    // 上一个关卡描述
    public UILabel mLastLevelDesc;

    // 强化装备描述
    public UILabel mStrengthenDesc;

    // 北镜圣域描述
    public UILabel mDungeonsDesc;

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();

        // 初始化文本
        InitLabel();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mLastLevelBtn).onClick = OnClickLastLevelBtn;
        UIEventListener.Get(mStrengthenBtn).onClick = OnClickStrengthenBtn;
        UIEventListener.Get(mDungeonsBtn).onClick = OnClickDungeonsBtn;
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLabel()
    {
        mLastLevelBtnLb.text = LocalizationMgr.Get("SpecialGuideWnd_1");
        mStrengthenBtnLb.text = LocalizationMgr.Get("SpecialGuideWnd_2");
        mDungeonsBtnLb.text = LocalizationMgr.Get("SpecialGuideWnd_3");
        mLastLevelDesc.text = LocalizationMgr.Get("SpecialGuideWnd_4");
        mStrengthenDesc.text = LocalizationMgr.Get("SpecialGuideWnd_5");
        mDungeonsDesc.text = LocalizationMgr.Get("SpecialGuideWnd_6");
    }

    /// <summary>
    /// 上一个关卡按钮点击事件
    /// </summary>
    void OnClickLastLevelBtn(GameObject go)
    {
        if (ME.user == null)
            return;

        LPCMapping instance = ME.user.Query<LPCMapping>("instance");
        if (instance == null)
            return;

        // 当前副本id
        string instanceId = instance.GetValue<string>("id");

        LPCMapping instanceCofig = InstanceMgr.GetInstanceInfo(instanceId);
        if (instanceCofig == null)
            return;

        CsvRow mapConfig = MapMgr.GetMapConfig(instanceCofig.GetValue<int>("map_id"));
        if (mapConfig == null)
            return;

        int mapType = mapConfig.Query<int>("map_type");

        if (mapType.Equals(MapConst.ARENA_MAP) || mapType.Equals(MapConst.ARENA_REVENGE_MAP) || mapType.Equals(MapConst.ARENA_NPC_MAP))
        {
            DialogMgr.Notify(LocalizationMgr.Get("SpecialGuideWnd_7"));

            WindowMgr.DestroyWindow(gameObject.name);

            return;
        }

        // 前置副本id
        string preInstance = InstanceMgr.GetPreInstanceId(instanceId);

        // 没有前置副本直接选择当前副本
        if (string.IsNullOrEmpty(preInstance))
            preInstance = instanceId;

        // 打开世界地图场景
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(DoSelectAgainFight, preInstance));
    }

    /// <summary>
    /// 打开反击界面
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="param">Parameter.</param>
    private void DoSelectAgainFight(object para, object[] param)
    {
        // 离开副本
        DoLeaveInstance();

        //获得选择战斗窗口
        GameObject wnd = WindowMgr.OpenWnd(SelectFighterWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        string preInstance = para as string;

        // 地图类型
        int mapType = InstanceMgr.GetMapTypeByInstanceId(preInstance);

        string wndName = string.Empty;
        if (mapType.Equals(MapConst.DUNGEONS_MAP_2) || mapType.Equals(MapConst.SECRET_DUNGEONS_MAP))
        {
            wndName = DungeonsWnd.WndType;
        }
        else if(mapType.Equals(MapConst.TOWER_MAP))
        {
            wndName = TowerWnd.WndType;
        }
        else
        {
            wndName = SelectInstanceWnd.WndType;
        }

        if (mapType.Equals(MapConst.TOWER_MAP))
        {
        }
        else if (mapType.Equals(MapConst.SECRET_DUNGEONS_MAP))
        {
        }
        else
        {
            // 设置本次通关的副本ID
            wnd.GetComponent<SelectFighterWnd>().Bind(wndName, preInstance, null, false);
        }
    }

    /// <summary>
    /// 离开副本
    /// </summary>
    private void DoLeaveInstance()
    {
        //离开副本;
        InstanceMgr.LeaveInstance(ME.user);

        // 销毁自己
        WindowMgr.DestroyWindow(FightSettlementWnd.WndType);

        WindowMgr.DestroyWindow(ArenaFightSettlementWnd.WndType);
    }

    /// <summary>
    /// 强化装备按钮点击事件
    /// </summary>
    void OnClickStrengthenBtn(GameObject go)
    {
        SceneMgr.LoadScene("Main", SceneConst.SCENE_MAIN_CITY, new CallBack(OnOpenMainCity));
    }

    /// <summary>
    /// 打开场景回调打开包裹界面
    /// </summary>
    private void OnOpenMainCity(object para, object[] param)
    {
        // 离开副本
        DoLeaveInstance();

        // 打开主界面
        GameObject wnd = WindowMgr.OpenMainWnd();
        if (wnd == null)
            return;

        // 设置主界面打开方式
        wnd.GetComponent<MainWnd>().ShowMainUIBtn(true);

        // 打开包裹界面
        WindowMgr.OpenWnd(BaggageWnd.WndType);
    }

    /// <summary>
    /// 北镜圣域按钮点击事件
    /// </summary>
    void OnClickDungeonsBtn(GameObject go)
    {
        // 打开世界地图场景
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(OpenDungeonsWnd));
    }

    /// <summary>
    /// 打开北镜圣域
    /// </summary>
    void OpenDungeonsWnd(object para, object[] param)
    {
        // 离开副本
        DoLeaveInstance();

        // 打开好友地下城
        // 相机移动的目标位置
        Vector3 targetPos = new Vector3(-4.25f, 10.86f, -15f);

        // 创建地下城窗口
        GameObject wnd = WindowMgr.OpenWnd(DungeonsWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<DungeonsWnd>().Bind(string.Empty, 0, targetPos);
    }

    /// <summary>
    /// 指引点击上一个关卡按钮
    /// </summary>
    public void GuideClickLastLevelBtn()
    {
        OnClickLastLevelBtn(mLastLevelBtn);
    }

    /// <summary>
    /// 指引点击强化按钮
    /// </summary>
    public void GuideClickStrengthenBtn()
    {
        OnClickStrengthenBtn(mStrengthenBtn);
    }

    /// <summary>
    /// 指引点击北镜圣域按钮
    /// </summary>
    public void GuideClickDungeonsBtn()
    {
        OnClickDungeonsBtn(mDungeonsBtn);
    }
}
