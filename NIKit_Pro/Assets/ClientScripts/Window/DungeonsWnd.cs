/// <summary>
/// DungeonsWnd.cs
/// Created by fengsc 2016/12/29
/// 地下城窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class DungeonsWnd : WindowBase<DungeonsWnd>
{
    // 窗口关闭按钮
    public GameObject mCloseBtn;

    public UIGrid mDungeonListGrid;
    public UIGrid mInstanceGrid;

    // 掉落物品查看按钮
    public UILabel mDrapOutViewBtn;

    // 任务奖励查看按钮
    public UILabel mRewardViewBtn;

    // 地下城挑战完成的数量
    public UILabel mFinishChallengeAmount;

    // 所有挑战完成提示文字
    public UILabel mFinishTips;

    // 地下城列表基础格子
    public GameObject mDungeonsItem;

    // 副本列表基础格子
    public GameObject mInstanceItem;

    //  窗口标题
    public UILabel mWndTitle;
    public UILabel mWndTitleShadow;

    // 地下城名称
    public UILabel mDungeonsTitle;

    public UILabel mDungeonsInstanceTips;

    public UIScrollView mDungeonsScrollView;

    public UIScrollView mInstanceScrollView;

    public GameObject mMask;

    public TweenScale mTweenScale;
    public TweenAlpha mTweenAlpha;

    // 当前选择DungeonsItemWnd目标对象
    DungeonsItemWnd mSelectItemWnd = null;

    // 选中的副本id
    int mSelectMapId = 0;
    LPCMapping mExtraPara = LPCMapping.Empty;

    // 副本格子缓存列表
    List<GameObject> mInstanceItemList = new List<GameObject>();

    // 地下城格子缓存列表
    List<GameObject> mDungeonsItemList = new List<GameObject>();

    string mInstanceId = string.Empty;

    // 默认选择副本id
    int mDefaultSelectMapId = 11;

    // Use this for initialization
    void Awake()
    {
        // 初始化本地化事件
        InitLocaText();

        // 注册事件
        RegisterEvent();

        // 创建一批副本列表基础格子缓存
        CreatedInstanceGameObject();
    }

    /// <summary>
    /// OnEnable
    /// </summary>
    void OnEnable()
    {
        // 播放地下城音乐
        GameSoundMgr.PlayBgmMusic("dungeonsWnd");

        // 关注字段变化
        if (ME.user != null)
            ME.user.dbase.RegisterTriggerField(DungeonsWnd.WndType,
                new string[] {"dynamic_map"}, new CallBack(OnDynamicMapChange));

        // 没有获取到组件;
        if (mTweenScale == null || mTweenAlpha == null)
            return;

        mTweenScale.ResetToBeginning();
        mTweenScale.enabled = true;

        mTweenAlpha.ResetToBeginning();
        mTweenAlpha.enabled = true;
    }

    /// <summary>
    /// OnDisable
    /// </summary>
    void OnDisable()
    {
        // 重置当前选择mSelectItemWnd
        mSelectItemWnd = null;

        // 播放世界地图音乐
        GameSoundMgr.PlayBgmMusic("WorldMap");

        // 从正在打开列表中移除
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        // 取消调用
        CancelInvoke("RefreshData");

        // 重置相机位置
        ResetCameraPosition();

        // 取消字段变化监听
        if (ME.user != null)
            ME.user.dbase.RemoveTriggerField(DungeonsWnd.WndType);
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        // 析构InstanceItem
        for (int i = 0; i < mInstanceItemList.Count; i++)
            Destroy(mInstanceItemList[i]);

        mInstanceItemList.Clear();

        // 析构DungeonsItem
        for (int i = 0; i < mDungeonsItemList.Count; i++)
            Destroy(mDungeonsItemList[i]);

        mDungeonsItemList.Clear();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mDrapOutViewBtn.gameObject).onClick = OnClickDrapOutViewBtn;
        UIEventListener.Get(mRewardViewBtn.gameObject).onClick = OnClickRewardViewBtn;
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mMask).onClick = OnClickMask;

        // 注册动画播放回调
        if (mTweenScale != null)
        {
            EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

            float scale = Game.CalcWndScale();
            mTweenScale.to = new Vector3(scale, scale, scale);
        }
    }

    /// <summary>
    /// 动画播放完成回调
    /// </summary>
    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 字段变化回调
    /// </summary>
    void OnDynamicMapChange(object para, params object[] param)
    {
        // 重置数据
        mSelectMapId = mDefaultSelectMapId;
        mInstanceId = string.Empty;
        mExtraPara = LPCMapping.Empty;

        // 重置当前选择mSelectItemWnd
        mSelectItemWnd = null;

        // 刷新窗口
        RefreshData();
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLocaText()
    {
        mWndTitle.text = LocalizationMgr.Get("DungeonsWnd_1");
        mWndTitleShadow.text  = LocalizationMgr.Get("DungeonsWnd_1");
        mDrapOutViewBtn.text = LocalizationMgr.Get("DungeonsWnd_3");
        mRewardViewBtn.text = LocalizationMgr.Get("DungeonsWnd_13");
        mFinishTips.text = LocalizationMgr.Get("DungeonsWnd_14");
        mDungeonsInstanceTips.text = LocalizationMgr.Get("DungeonsWnd_16");
    }

    /// <summary>
    /// Gets the dungeons item.
    /// </summary>
    private GameObject GetDungeonsItem(int index)
    {
        // 如果已经超出了范围
        if (index > mDungeonsItemList.Count)
        {
            GameObject itemGo = Instantiate(mDungeonsItem).gameObject;
            itemGo.transform.SetParent(mDungeonListGrid.transform);
            itemGo.transform.localPosition = Vector3.zero;
            itemGo.transform.localScale = Vector3.one;
            itemGo.name = mDungeonsItemList.Count.ToString();

            // 初始化隐藏控件
            itemGo.SetActive(false);

            // 注册按钮点击事件
            UIEventListener.Get(itemGo).onClick = OnClickDungeonsItem;

            // 添加到列表中
            mDungeonsItemList.Add(itemGo);
        }

        // 返回对应位置的对象
        return mDungeonsItemList[index];
    }

    /// <summary>
    /// 绘制地下城列表
    /// </summary>
    void RedrawDungeonsList()
    {
        // 玩家独享不存在
        if (ME.user == null)
            return;

        int index = 0;
        int selectId = 0;
        int totalCount = 0;
        int mapId;
        GameObject dungeonsItem;

        // 绘制精英副本
        LPCArray petDungeonsList = InstanceMgr.GetPetDungeonsList(ME.user);
        foreach (LPCValue data in petDungeonsList.Values)
        {
            // 精英副本地下城的地图id
            mapId = 25;

            // 获取一个控件
            dungeonsItem = GetDungeonsItem(index);
            if (dungeonsItem == null)
                return;

            // 绑定数据
            dungeonsItem.GetComponent<DungeonsItemWnd>().Bind(mapId, data.AsMapping);

            // 显示控件
            dungeonsItem.SetActive(true);

            // 模拟点击事件
            if (mSelectMapId == mapId &&
                string.Equals(data.AsMapping.GetValue<string>("dynamic_id"), mExtraPara.GetValue<string>("dynamic_id")))
            {
                selectId = index;
                OnClickDungeonsItem(dungeonsItem);
            }

            // index++
            index++;

            // 累计totalCount
            totalCount++;
        }

        // 没有选择到精英副本
        if (mSelectMapId == 25 && mSelectItemWnd == null)
        {
            // 重置数据
            mSelectMapId = mDefaultSelectMapId;
            mInstanceId = string.Empty;
            mExtraPara = LPCMapping.Empty;
        }

        // 绘制秘密地下城
        LPCArray secretDungeonsList = InstanceMgr.GetSecretDungeonsList(ME.user);
        if (secretDungeonsList.Count != 0)
        {
            // 秘密地下城的地图id
            mapId = 19;

            // 获取一个控件
            dungeonsItem = GetDungeonsItem(index);
            if (dungeonsItem == null)
                return;

            // 绑定数据
            dungeonsItem.GetComponent<DungeonsItemWnd>().Bind(mapId, LPCMapping.Empty);

            // 显示控件
            dungeonsItem.SetActive(true);

            // 模拟点击事件
            if (mSelectMapId == mapId)
            {
                selectId = index;
                OnClickDungeonsItem(dungeonsItem);
            }

            // index++
            index++;

            // 累计totalCount
            totalCount++;
        }

        // 没有选择到精英副本
        if (mSelectMapId == 19 && mSelectItemWnd == null)
        {
            // 重置数据
            mSelectMapId = mDefaultSelectMapId;
            mInstanceId = string.Empty;
            mExtraPara = LPCMapping.Empty;
        }

        // 绘制固定地下城
        Dictionary<int, CsvRow> mDungeonsFixedMapList = MapMgr.GetDungeonsFixedMap(ME.user);
        foreach (CsvRow data in mDungeonsFixedMapList.Values)
        {
            if (data == null)
                continue;

            // 获取地图编号
            mapId = data.Query<int>("rno");

            // 获取一个控件
            dungeonsItem = GetDungeonsItem(index);
            if (dungeonsItem == null)
                continue;

            LPCMapping args = data.Query<LPCMapping>("unlock_args");
            if (args == null)
                continue;

            LPCMapping dynamicMap = LPCMapping.Empty;

            // 添加副本倒计时关闭信息
            if (args.ContainsKey("wday"))
            {
                LPCArray array = GameSettingMgr.GetSetting<LPCArray>("dungeons_all_open_date");
                int curTime = TimeMgr.GetServerTime();
                int indexOf = array.IndexOf(Game.GetWeekDay(curTime));

                if (indexOf < 0)
                    indexOf = 0;
                else
                    indexOf = array.Count - indexOf - 1;

                // 添加参数
                dynamicMap.Add("end_time", (int) Game.GetZeroClock(1) + curTime + indexOf * 86400);
            }

            // 绑定数据
            dungeonsItem.GetComponent<DungeonsItemWnd>().Bind(mapId, dynamicMap);

            // 显示控件
            dungeonsItem.SetActive(true);

            // 模拟点击事件
            if (mSelectMapId == mapId)
            {
                selectId = index;
                OnClickDungeonsItem(dungeonsItem);
            }

            // index++
            index++;

            // 累计totalCount
            totalCount++;
        }

        // 隐藏多余控件
        for (int i = index; i < mDungeonsItemList.Count; i++)
            mDungeonsItemList[i].SetActive(false);

        // 重置mDungeonListGrid排序
        mDungeonListGrid.repositionNow = true;

        // 如果一个面板能够显示
        if (selectId <= 3)
            return;

        Vector3 pos = Vector3.zero;
        if (selectId >= (totalCount - 1))
        {
            pos = new Vector3(mDungeonsScrollView.panel.cachedTransform.localPosition.x,
                mDungeonsScrollView.panel.cachedTransform.localPosition.y + totalCount * mDungeonListGrid.cellHeight - mDungeonsScrollView.panel.GetViewSize().y,
                mDungeonsScrollView.panel.cachedTransform.localPosition.z);
        }
        else
        {
            pos = new Vector3(mDungeonsScrollView.panel.cachedTransform.localPosition.x,
                mDungeonsScrollView.panel.cachedTransform.localPosition.y + (selectId + 1) * mDungeonListGrid.cellHeight - mDungeonsScrollView.panel.GetViewSize().y,
                mDungeonsScrollView.panel.cachedTransform.localPosition.z);
        }

        // 设置滑动位置
        SpringPanel.Begin(mDungeonsScrollView.panel.cachedGameObject, pos, 10f);
    }

    /// <summary>
    /// 创建一批副本列表基础格子
    /// </summary>
    void CreatedInstanceGameObject()
    {
        for (int i = 0; i < 20; i++)
        {
            // 实例化副本列表格子
            GameObject go = Instantiate(mInstanceItem).gameObject;
            if (go == null)
                continue;

            go.transform.SetParent(mInstanceGrid.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.name = i.ToString();
            go.SetActive(false);

            mInstanceItemList.Add(go);

            GameObject item = Instantiate(mDungeonsItem).gameObject;
            if (item == null)
                continue;

            item.transform.SetParent(mDungeonListGrid.transform);
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = Vector3.one;
            item.name = i.ToString();

            item.SetActive(false);

            // 注册按钮点击事件
            UIEventListener.Get(item).onClick = OnClickDungeonsItem;

            // 添加到mDungeonsItemList列表中
            mDungeonsItemList.Add(item);
        }
    }

    /// <summary>
    /// 填充副本列表数据
    /// </summary>
    void FillInstanceData()
    {
        // 没有mSelectDungeonsItemWnd
        if (mSelectItemWnd == null)
            return;

        // 根据地图id获取副本列表
        List<string> instanceList = InstanceMgr.GetInstanceByMapId(mSelectItemWnd.mMapId);
        if (instanceList == null)
            return;

        // 地图配置信息
        CsvRow mapConfig = MapMgr.GetMapConfig(mSelectItemWnd.mMapId);

        int mapType = 0;

        int listCount = 0;

        int clearanceCount = 0;

        int instanceIndex = 0;

        int count = 0;

        Vector3 targetPos = Vector3.zero;

        if (mapConfig != null)
            mapType = mapConfig.Query<int>("map_type");

        if (mapType.Equals(MapConst.SECRET_DUNGEONS_MAP))
        {
            clearanceCount = 0;

            instanceIndex = 0;

            int index = 0;

            // 秘密地下城列表
            LPCArray secretDungeonsList = InstanceMgr.GetSecretDungeonsList(ME.user);
            foreach (LPCValue item in secretDungeonsList.Values)
            {
                LPCMapping config = InstanceMgr.GetInstanceInfo(instanceList[0]);

                if (config == null || config.Count < 1)
                    continue;

                if (mInstanceId.Equals(config.GetValue<string>("instance_id")))
                    instanceIndex = clearanceCount;

                if (index + 1 > mInstanceItemList.Count)
                {
                    // 实例化副本列表格子
                    GameObject itemGo = Instantiate(mInstanceItem).gameObject;
                    if (itemGo == null)
                        continue;

                    itemGo.transform.SetParent(mInstanceGrid.transform);
                    itemGo.transform.localPosition = Vector3.zero;
                    itemGo.transform.localScale = Vector3.one;
                    itemGo.name = index.ToString();
                    itemGo.SetActive(false);

                    mInstanceItemList.Add(itemGo);
                }

                // 实例化副本列表格子
                GameObject go = mInstanceItemList[index];

                if (go == null)
                    continue;

                // 绑定数据
                go.GetComponent<DungeondInstanceItemWnd>().Bind(config, item.AsMapping);

                go.SetActive(true);

                index++;
            }

            listCount = secretDungeonsList.Count;
        }
        else
        {
            clearanceCount = 0;

            instanceIndex = 0;

            // 遍历副本列表， 给副本列表格子填充数据
            for (int i = 0; i < instanceList.Count; i++)
            {
                // 根据副本id获取副本配置信息
                LPCMapping config = InstanceMgr.GetInstanceInfo(instanceList[i]);

                if (config == null || config.Count < 1)
                    continue;

                string instanceId = config.GetValue<string>("instance_id");

                if (InstanceMgr.IsClearanced(ME.user, instanceId, mExtraPara))
                    clearanceCount++;

                if (mInstanceId.Equals(instanceId))
                    instanceIndex = clearanceCount;

                if (i + 1 > mInstanceItemList.Count)
                {
                    // 实例化副本列表格子
                    GameObject itemGo = Instantiate(mInstanceItem).gameObject;
                    if (itemGo == null)
                        continue;

                    itemGo.transform.SetParent(mInstanceGrid.transform);
                    itemGo.transform.localPosition = Vector3.zero;
                    itemGo.transform.localScale = Vector3.one;
                    itemGo.name = i.ToString();
                    itemGo.SetActive(false);

                    mInstanceItemList.Add(itemGo);
                }

                // 实例化副本列表格子
                GameObject go = mInstanceItemList[i];

                if (go == null)
                    continue;

                // 绑定数据
                go.GetComponent<DungeondInstanceItemWnd>().Bind(config, mSelectItemWnd.mExtraPara);

                go.SetActive(true);
            }

            listCount = instanceList.Count;

            count = (instanceIndex == 0 ? clearanceCount : instanceIndex);

            // 目标位置
            targetPos = new Vector3(mInstanceScrollView.transform.localPosition.x,
                mInstanceScrollView.transform.localPosition.y + (count == listCount ? count : count + 1) * mInstanceGrid.cellHeight - mInstanceScrollView.panel.GetViewSize().y,
                mInstanceScrollView.transform.localPosition.z);
        }

        mInstanceGrid.repositionNow = true;

        for (int i = listCount; i < mInstanceItemList.Count; i++)
            mInstanceItemList[i].SetActive(false);

        if (count <= 4)
            return;

        SpringPanel.Begin(mInstanceScrollView.gameObject, targetPos, 10f);
    }

    /// <summary>
    /// 窗口关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.HideWindow(gameObject);

        // 获取主城界面
        GameObject wnd = WindowMgr.OpenWnd(MainWnd.WndType);

        if (wnd == null)
            return;

        // 显示主城界面
        WindowMgr.ShowWindow(wnd);

        wnd.GetComponent<MainWnd>().ShowMainUIBtn(false);
    }

    /// <summary>
    ///点击mask关闭副本选择界面
    /// </summary>
    void OnClickMask(GameObject go)
    {
        // 关闭窗口;
        WindowMgr.HideWindow(gameObject);

        GameObject wnd = WindowMgr.OpenWnd("MainWnd");

        // 获取主界面窗口失败,不做以下操作;
        if (wnd == null)
            return;

        WindowMgr.ShowWindow(wnd);

        wnd.GetComponent<MainWnd>().ShowMainUIBtn(false);
    }

    /// <summary>
    ///初始化场景相机的动画
    /// </summary>
    void ResetCameraPosition()
    {
        Vector3 fromPos = SceneMgr.SceneCameraFromPos;

        Camera sceneCamera = SceneMgr.SceneCamera;
        if (sceneCamera == null)
            return;

        SceneCamera control = sceneCamera.GetComponent<SceneCamera>();

        if (control != null)
            control.MoveCamera(sceneCamera.transform.position, fromPos);

        // 缓存场景相机的位置
        SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        SceneMgr.SceneCameraToPos = fromPos;
    }

    /// <summary>
    /// 掉落物品查看按钮点击事件
    /// </summary>
    void OnClickDrapOutViewBtn(GameObject go)
    {
        // 当前没有选择目标对象
        if (mSelectItemWnd == null)
            return;

        // 创建掉落物品查看窗口
        GameObject wnd = WindowMgr.OpenWnd(DungeonsDropGoodsWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (wnd == null)
            return;

        // 绑定地图id
        wnd.GetComponent<DungeonsDropGoodsWnd>().Bind(mSelectItemWnd.mMapId, mSelectItemWnd.mExtraPara);
    }

    /// <summary>
    /// 奖励查看按钮点击事件
    /// </summary>
    void OnClickRewardViewBtn(GameObject go)
    {
        // 打开DungeonsRewardWnd
        WindowMgr.OpenWnd("DungeonsRewardWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 地下城基础格子点击事件
    /// </summary>
    void OnClickDungeonsItem(GameObject go)
    {
        // 获取点击DungeonsItemWnd
        DungeonsItemWnd script = go.GetComponent<DungeonsItemWnd>();

        // 控件不存在
        if (script == null)
            return;

        // 目标没有发生变化
        if (mSelectItemWnd == script)
            return;

        // 记录当前选择DungeonsItemWnd
        mSelectItemWnd = script;

        if (mDungeonsInstanceTips.gameObject.activeSelf)
            mDungeonsInstanceTips.gameObject.SetActive(false);

        mInstanceScrollView.ResetPosition();

        CsvRow data = MapMgr.GetMapConfig(mSelectItemWnd.mMapId);

        if (data != null)
            mDungeonsTitle.text = string.Format(LocalizationMgr.Get("DungeonsWnd_2"),
                LocalizationMgr.Get(data.Query<string>("name")));

        if (mDungeonsItemList == null || mDungeonsItemList.Count < 1)
            return;

        foreach (GameObject item in mDungeonsItemList)
        {
            Transform icon = item.transform.Find("icon");
            UILabel name = item.transform.Find("name").GetComponent<UILabel>();
            GameObject itemCheckmark = item.transform.Find("Checkmark").gameObject;

            GameObject time = item.transform.Find("time").gameObject;

            if (icon == null || name == null || itemCheckmark == null)
                continue;

            if (item.Equals(go))
            {
                itemCheckmark.SetActive(true);
                icon.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                icon.transform.localPosition = new Vector3(-239,
                    icon.transform.localPosition.y,
                    icon.transform.localPosition.z);
                name.fontSize = 32;

                time.transform.localPosition = new Vector3(57, -3, 0);
            }
            else
            {
                icon.localScale = Vector3.one;
                icon.transform.localPosition = new Vector3(-248,
                    icon.transform.localPosition.y,
                    icon.transform.localPosition.z);
                itemCheckmark.SetActive(false);
                name.fontSize = 30;

                time.transform.localPosition = new Vector3(26, 0, 0);
            }
        }

        // 刷新紅點提示
        RefreshRedPoint();

        // 填充副本列表数据
        FillInstanceData();
    }

    /// <summary>
    ///  刷新紅點提示的位置
    /// </summary>
    void RefreshRedPoint()
    {
        // 当前没有选择节点
        if (mSelectItemWnd == null)
            return;

        // 遍历全部节点
        for (int i = 0; i < mDungeonsItemList.Count; i++)
        {
            GameObject item = mDungeonsItemList[i];
            if (item == null)
                continue;

            // 获取DungeonsItemWnd
            DungeonsItemWnd script = item.GetComponent<DungeonsItemWnd>();
            if (script == null)
                continue;

            if (mSelectItemWnd != script)
                script.mRedPointTips.transform.localPosition = new Vector3(-198.2f, 33.8f, 0);
            else
                script.mRedPointTips.transform.localPosition = new Vector3(-173.4f, 43.9f, 0);
        }
    }

    /// <summary>
    /// 地图窗口点击移动相机
    /// </summary>
    private void MoveCamera(Vector3 position)
    {
        Vector3 targetPos = new Vector3(position.x, position.y, -15);

        SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
        if (control != null)
            control.MoveCamera(SceneMgr.SceneCamera.transform.position, targetPos);

        // 缓存场景相机的位置
        SceneMgr.SceneCameraToPos = targetPos;
        SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
    }

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(string instanceId, int mapId, Vector3 clickPos, LPCMapping extraPara)
    {
        // 缓存mExtraPara
        mExtraPara = extraPara;

        // 地图未解锁
        if (! MapMgr.IsUnlocked(ME.user, mapId))
        {
            instanceId = string.Empty;

            mapId = -1;

            // 缓存mExtraPara
            mExtraPara = LPCMapping.Empty;
        }

        // 重置当前选择mSelectItemWnd
        mSelectItemWnd = null;

        // 记录副本id
        mInstanceId = instanceId;

        // 获取
        if (mapId <= 0)
            mSelectMapId = mDefaultSelectMapId;
        else
            mSelectMapId = mapId;

        // 移动相机
        MoveCamera(clickPos);

        // 刷新数据
        RefreshData();
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(string instanceId, int mapId, Vector3 clickPos)
    {
        Bind(instanceId, mapId, clickPos, LPCMapping.Empty);
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    public void RefreshData()
    {
        mDungeonsScrollView.ResetPosition();

        // 绘制地下城列表
        RedrawDungeonsList();

        // 填充当前选择的副本数据
        FillInstanceData();

        mDungeonsInstanceTips.gameObject.SetActive(false);

        mInstanceId = string.Empty;
    }

    #endregion
}
