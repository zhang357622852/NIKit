/// <summary>
/// DropSoulInfoWnd.cs
/// Created by lic 11/17/2017
/// 魂石掉落窗口
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class DropSoulInfoWnd : WindowBase<DropSoulInfoWnd>
{
    #region 成员变量

    public UITexture mBg;
    public UILabel mTitle;
    public UILabel mTime;
    public UILabel mDesc;
    public UILabel mBtnLb;

    public GameObject mTimeIcon;

    public GameObject mMaterialItem;

    public GameObject mGoBtn;
    public GameObject mCover;

    public GameObject mMaterialViewWnd;

    #endregion

    #region 私有变量

    private int mMapId;

    // 开放时间（开放时间为-1表示一直开放）
    private int mOpenday = -1;

    private float mLastTime = 0f;

    private Vector3 mTimePos;

    #endregion

    #region 内部函数

    void Awake()
    {
        mTimePos = new Vector3(mTime.transform.localPosition.x, mTime.transform.localPosition.y, mTime.transform.localPosition.z);
    }

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        //初始化窗口
        InitWnd();
    }

    void Update()
    {
        // 一直开放时间不处理
        if (mOpenday == -1)
            return;
        
        if (Time.realtimeSinceStartup > mLastTime + 1)
        {
            mLastTime = Time.realtimeSinceStartup;
            UpdateTime();
        }
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        mMaterialItem.SetActive(false);

        mTitle.text = string.Format("[{0}]{1}[-]", GetTitleColor(mMapId), GetTitle(mMapId));

        mBg.color = ColorConfig.HexToColor(GetBgColor(mMapId));

        string group = GetGroup(mMapId);

        List<int> itemList = ItemMgr.GetClassIdByGroup(group);

        GameObject item;

        for (int i = 0; i < itemList.Count; i++)
        {
            item = Instantiate (mMaterialItem) as GameObject;
            item.transform.parent = transform;
            item.name = string.Format("item_{0}", i);
            item.transform.localPosition = new Vector3(mMaterialItem.transform.localPosition.x - (itemList.Count - i - 1)*60f, mMaterialItem.transform.localPosition.y, mMaterialItem.transform.localPosition.z);
            item.transform.localScale = new Vector3(1f, 1f, 1f);

            item.SetActive(true);

            // 构建参数
            LPCMapping data = new LPCMapping();
            data.Add("icon", itemList[i]);

            item.GetComponent<AwakeMaterialItemWnd>().Bind(data, itemList[i]);

            UIEventListener.Get(item).onPress = OnClickMaterialItem;
        }

        CsvRow config = MapMgr.GetMapConfig(mMapId);

        if (config == null)
            return;

        LPCMapping args = config.Query<LPCMapping>("unlock_args");
        if (args == null)
            return;

        mOpenday = args.ContainsKey("wday") ? args.GetValue<int>("wday") : -1;

        if (mOpenday == -1)
            RedrawTime(true);
    }

    /// <summary>
    /// 材料格子点击事件
    /// </summary>
    /// <param name="go">Go.</param>
    /// <param name="isPress">If set to <c>true</c> is press.</param>
    void OnClickMaterialItem(GameObject go, bool isPress)
    {
        AwakeMaterialViewWnd viewWnd = mMaterialViewWnd.GetComponent<AwakeMaterialViewWnd>();
        if(isPress)
            viewWnd.ShowView(go.GetComponent<AwakeMaterialItemWnd>().mClassId);
        else
            viewWnd.HideView();
    }

    /// <summary>
    /// 刷新时间显示
    /// </summary>
    private void UpdateTime()
    {
        int curTime = TimeMgr.GetServerTime();

        int day = Game.GetWeekDay(curTime);

        // 计算开放时间
        if (!MapMgr.IsUnlocked(ME.user, mMapId))
        {
            int days = (mOpenday > day) ? (mOpenday - day) : (7 - day + mOpenday);

            int startTime = (days - 1) * 86400 + (int)Game.GetZeroClock(1);

            RedrawTime(false, false, startTime);

            return;
        }

        LPCArray array = GameSettingMgr.GetSetting<LPCArray>("dungeons_all_open_date");

        int indexOf = array.IndexOf(day);

        int endTime = (int) Game.GetZeroClock(1) + ((indexOf == -1) ? 0:array.Count - indexOf - 1) * 86400;

        RedrawTime(false, true, endTime);
    }

    /// <summary>
    /// 刷新时间显示
    /// </summary>
    /// <param name="isAllOpen">If set to <c>true</c> is all open.</param>
    /// <param name="isOpenState">If set to <c>true</c> is open state.</param>
    /// <param name="time">Time.</param>
    private void RedrawTime(bool isAllOpen,bool isOpenState = false, int time = 0)
    {
        string timeDesc;

        if (time >= 86400)
        {
            int day = time / 86400;
            
            timeDesc = string.Format("{0} {1} {2} {3}", day, LocalizationMgr.Get("TimeMgr_1"), (time - day * 86400) / 3600, LocalizationMgr.Get("TimeMgr_8"));
        }
        else if (time > 60)
            timeDesc = TimeMgr.ConvertTimeToChineseTimer(time, false);
        else
        {
            if(time >= 10)
                timeDesc = string.Format("{0} {1}", time, LocalizationMgr.Get("TimeMgr_4"));
            else
                timeDesc = string.Format("0{0} {1}", time, LocalizationMgr.Get("TimeMgr_4"));
        }

        if (isAllOpen)
        {
            mTime.text = LocalizationMgr.Get("DropSoulInfoWnd_10");
            mTime.transform.localPosition = new Vector3(mTimePos.x + 28f, mTimePos.y, mTimePos.z);
            mTimeIcon.SetActive(false);
            mCover.SetActive(false);
            return;
        }

        if (!isOpenState)
        {
            mTime.text = string.Format(LocalizationMgr.Get("DropSoulInfoWnd_9"), timeDesc);
            mTime.transform.localPosition = new Vector3(mTimePos.x + 28f, mTimePos.y, mTimePos.z);
            mTimeIcon.SetActive(false);
            mCover.SetActive(true);
            return;
        }

        mTime.text = timeDesc;
        mTime.transform.localPosition = mTimePos;
        mTimeIcon.SetActive(true);
        mCover.SetActive(false);
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        // 本地化文字
        mDesc.text = LocalizationMgr.Get("DropSoulInfoWnd_8");
        mBtnLb.text = LocalizationMgr.Get("DropSoulInfoWnd_7");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(mGoBtn).onClick = OnGoToBtn;
    }

    /// <summary>
    /// 跳转按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnGoToBtn(GameObject ob)
    {
        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(OnEnterMainCityScene));

        WindowMgr.DestroyWindow(DropSoulWnd.WndType);

        // 关闭包裹界面
        WindowMgr.HideWindow(BaggageWnd.WndType);
    }

    /// <summary>
    /// 打开主城回调
    /// </summary>
    private void OnEnterMainCityScene(object para, object[] param)
    {
        if(InstanceMgr.IsInInstance(ME.user))
        {
            //离开副本;
            InstanceMgr.LeaveInstance(ME.user);

            // 销毁战斗结算界面
            WindowMgr.DestroyWindow(FightSettlementWnd.WndType);
        }

        // 相机移动的目标位置
        Vector3 targetPos = new Vector3(-4.25f, 10.86f, -15f);

        // 创建地下城窗口
        GameObject wnd = WindowMgr.OpenWnd(DungeonsWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<DungeonsWnd>().Bind(string.Empty, mMapId, targetPos);
    }

    /// <summary>
    /// 根据mapid获取描述
    /// </summary>
    /// <returns>The title.</returns>
    /// <param name="MapId">Map identifier.</param>
    private string GetTitle(int mapId)
    {
        switch (mapId)
        {
            case 12: 
                return LocalizationMgr.Get("DropSoulInfoWnd_1");
            case 13: 
                return LocalizationMgr.Get("DropSoulInfoWnd_2");
            case 14: 
                return LocalizationMgr.Get("DropSoulInfoWnd_3");
            case 15: 
                return LocalizationMgr.Get("DropSoulInfoWnd_4");
            case 16: 
                return LocalizationMgr.Get("DropSoulInfoWnd_5");
            case 17: 
                return LocalizationMgr.Get("DropSoulInfoWnd_6");
            default : 
                return "";
        }
    }

    /// <summary>
    /// 根据地图id获取组别
    /// </summary>
    /// <returns>The group.</returns>
    /// <param name="mapId">Map identifier.</param>
    private string GetGroup(int mapId)
    {
        switch (mapId)
        {
            case 12: 
                return ItemConst.SOUL_M;
            case 13: 
                return ItemConst.SOUL_F;
            case 14: 
                return ItemConst.SOUL_S;
            case 15: 
                return ItemConst.SOUL_W;
            case 16: 
                return ItemConst.SOUL_L;
            case 17: 
                return ItemConst.SOUL_D;
            default : 
                return "";
        }
    }

    /// <summary>
    /// 根据地图id获取组别
    /// </summary>
    /// <returns>The group.</returns>
    /// <param name="mapId">Map identifier.</param>
    private string GetBgColor(int mapId)
    {
        switch (mapId)
        {
            case 12: 
                return "FF4AA8FF";
            case 13: 
                return "FF2138FF";
            case 14: 
                return "FFE621FF";
            case 15: 
                return "21C8FFFF";
            case 16: 
                return "FFFBDDFF";
            case 17: 
                return "E230FFFF";
            default : 
                return "FFFFFFFF";
        }
    }

    /// <summary>
    /// 根据地图id获取组别
    /// </summary>
    /// <returns>The group.</returns>
    /// <param name="mapId">Map identifier.</param>
    private string GetTitleColor(int mapId)
    {
        switch (mapId)
        {
            case 12: 
                return "ff48b4";
            case 13: 
                return "ff3a3a";
            case 14: 
                return "ffe63a";
            case 15: 
                return "3acaff";
            case 16: 
                return "fef8cf";
            case 17: 
                return "b70bff";
            default : 
                return "FFFFFFFF";
        }
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="data">Data.</param>
    public void BindData(int mapId)
    {
        mMapId = mapId;

        Redraw();
    }

    #endregion
}
