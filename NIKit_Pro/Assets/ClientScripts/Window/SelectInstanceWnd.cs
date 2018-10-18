/// <summary>
/// SelectInstanceWnd.cs
/// Created by fengsc 2016/07/16
///副本选择界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class SelectInstanceWnd : WindowBase<SelectInstanceWnd>
{
    #region 成员变量

    /// <summary>
    ///关闭按钮
    /// </summary>
    public GameObject mCloseBtn;

    /// <summary>
    ///副本列表
    /// </summary>
    public GameObject mInstacneList;

    /// <summary>
    ///副本列表克隆体
    /// </summary>
    public GameObject mItem;

    public UILabel mEasyLabel;

    public UILabel mNormalLabel;

    public UILabel mHardLabel;

    public GameObject mNormalLock;

    public GameObject mHardLock;

    public GameObject mHelpTipsBtn;

    /// <summary>
    ///展开信息栏的按钮
    /// </summary>
    public GameObject mArrowsBtn;

    /// <summary>
    ///掉落物品信息栏
    /// </summary>
    public GameObject mDrapOut;

    /// <summary>
    ///副本列表排序组件
    /// </summary>
    public UIGrid mGrid;

    public UIScrollView mScrollView;

    public GameObject mMask;

    /// <summary>
    ///掉落信息栏宠物列表排序组件
    /// </summary>
    public UIGrid mPetGrid;

    public GameObject mPetListItem;

    /// <summary>
    ///套装效果图片
    /// </summary>
    public UITexture mSuitTypeIcon;

    /// <summary>
    ///套装描述信息
    /// </summary>
    public UILabel mSuitTypeDesc;

    /// <summary>
    ///套装名称
    /// </summary>
    public UILabel mSuitName;

    private int mMapId = 0;

    private bool IsClick = true;

    private Vector3 normalPos = new Vector3(0, 0, 0);
    private Vector3 hardPos = new Vector3(0, 0, 0);

    public UIToggle hardToggle;
    public UIToggle normalToggle;
    public UIToggle easyToggle;

    public GameObject normalCheckMark;
    public GameObject hardCheckMark;

    public TweenScale mTweenScale;

    /// <summary>
    /// 副本难度   
    /// </summary>
    int difficulty = 0;

    // 副本列表的数量
    int mItemAmount = 15;

    Dictionary<string, GameObject> mItemList = new Dictionary<string, GameObject>();

    // 展示掉落临时克隆的item(每次创建或窗口关闭时需要析构掉)
    Property mPetItem = null;

    // 奖励窗口列表
    List<GameObject> mClearanceBonusWndList = new List<GameObject>();

    #endregion

    #region 内部函数

    void Awake()
    {
        // 创建窗口控件
        CreateGameObject(0, mItemAmount);
    }

    void OnEnable()
    {
        TweenScale scale = mInstacneList.GetComponent<TweenScale>();
        TweenAlpha alpha = mInstacneList.GetComponent<TweenAlpha>();

        // 没有获取到组件;
        if (scale == null || alpha == null)
            return;

        scale.ResetToBeginning();
        scale.enabled = true;

        alpha.ResetToBeginning();
        alpha.enabled = true;
    }

    // Use this for initialization
    void Start()
    {
        // 注册控件事件
        RegisterEvent();

        // 初始化本地化文本
        InitLabel();
    }

    /// <summary>
    ///初始化本地化文本
    /// </summary>
    void InitLabel()
    {
        mEasyLabel.text = LocalizationMgr.Get("SelectInstanceWnd_2");
        mNormalLabel.text = LocalizationMgr.Get("SelectInstanceWnd_3");
        mHardLabel.text = LocalizationMgr.Get("SelectInstanceWnd_4");
    }

    /// <summary>
    ///注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(easyToggle.gameObject).onClick = OnClickEasyBtn;
        UIEventListener.Get(normalToggle.gameObject).onClick = OnClickNormalBtn;
        UIEventListener.Get(hardToggle.gameObject).onClick = OnClickHardBtn;
        UIEventListener.Get(mHelpTipsBtn).onClick = OnClickHelpTipsBtn;
        UIEventListener.Get(mMask).onClick = OnClickMask;
        UIEventListener.Get(mArrowsBtn).onClick = OnClickArrowsBtn;

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, new EventDelegate.Callback(OnTweenScaleFinish));

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    void OnDisable()
    {
        // 从打开列表中移除
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        // 初始化相机位置
        ResetCameraPosition();

        // 临时对象不存在
        if (mPetItem == null)
            return;

        // 析构掉临时创建的克隆对象
        mPetItem.Destroy();
    }

    void OnTweenScaleFinish()
    {
        mDrapOut.SetActive(true);

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    ///初始化可掉落物品信息
    /// </summary>
    void InitDrapOutGoodsInfo()
    {
        if (mMapId <= 0)
            return;

        //根据地图ID获取地图配置信息;
        CsvRow mapConfig = MapMgr.GetMapConfig(mMapId);

        //该地图没有配置信息;
        if (mapConfig == null)
            return;

        // 获取该地图的奖励数据;
        LPCMapping bonusData = MapMgr.GetMapClearanceBonus(mMapId);

        LPCArray petData = bonusData.GetValue<LPCArray>("pet_id");

        int suit_id = bonusData.GetValue<int>("suit_id");

        if (petData == null || suit_id == 0)
            return;

        mPetListItem.SetActive(false);

        int childId = 0;
        GameObject itemWnd;

        // 填充数据
        for (int i = 0; i < petData.Count; i++)
        {
            // 如果没有窗口就创建
            if (childId >= mPetGrid.transform.childCount)
            {
                // 将新的消息添加到列表的末尾
                itemWnd = Instantiate(mPetListItem);
                itemWnd.transform.SetParent(mPetGrid.transform);
                itemWnd.transform.localScale = Vector3.one;
                itemWnd.name = petData[i].AsString;
            } else
            {
                itemWnd = mPetGrid.transform.GetChild(childId).gameObject;
            }

            // 显示图标
            if (! itemWnd.activeSelf)
                itemWnd.SetActive(true);

            // 绑定数据
            itemWnd.GetComponent<DrapOutPetItemWnd>().Bind(petData[i].AsInt);

            // 设置点击事件
            UIEventListener.Get(itemWnd).onClick = OnClickPetItem;

            // childId++
            childId++;
        }

        // 隐藏多余的图标
        for (int i = childId; i < mPetGrid.transform.childCount; i++)
        {
            // 获取子控件
            itemWnd = mPetGrid.transform.GetChild(i).gameObject;

            // 如果本身不是显示状态
            if (itemWnd == null || ! itemWnd.activeSelf)
                continue;

            // 隐藏控件
            itemWnd.SetActive(false);
        }

        //启用排序组件;
        mPetGrid.Reposition();

        CsvFile suitCsv = EquipMgr.SuitTemplateCsv;

        CsvRow suitData = suitCsv.FindByKey(suit_id);

        //套装类型图标;
        mSuitTypeIcon.mainTexture = EquipMgr.GetSuitTexture(suitData.Query<int>("suit_id"));

        //套装类型名称;
        string suitName = LocalizationMgr.Get(suitData.Query<string>("name"));
        mSuitName.text = string.Format("{0}{1}{2}", suitName, suitData.Query<int>("sub_count"), LocalizationMgr.Get("SelectInstanceWnd_5"));

        //获取套装属性;
        LPCArray propList = suitData.Query<LPCArray>("props");

        //没有套装属性
        if (propList == null)
        {
            mSuitTypeDesc.text = string.Empty;
            return;
        }

        string propDesc = string.Empty;

        //遍历各个套装属性;
        foreach (LPCValue prop in propList.Values)
            propDesc += PropMgr.GetPropDesc(prop.AsArray, EquipConst.SUIT_PROP);

        //套装描述;
        mSuitTypeDesc.text = propDesc;
    }

    /// <summary>
    /// 创建一批副本列表
    /// </summary>
    void CreateGameObject(int starIndex, int endIndex)
    {
        mItem.SetActive(false);
        for (int i = starIndex; i < endIndex; i++)
        {
            GameObject clone = Instantiate(mItem);

            // 设置父级;
            clone.transform.SetParent(mGrid.transform);

            clone.transform.localPosition = Vector3.zero;

            clone.transform.localScale = Vector3.one;

            clone.name = "instacne" + i;

            clone.SetActive(false);

            mItemList[clone.name] = clone;
        }

        mGrid.Reposition();
    }

    /// <summary>
    ///根据难度获取副本列表
    /// </summary>
    void GetInstanceList(int difficulty)
    {
        // 没有该范围内的地图ID;
        if (mMapId <= 0)
            return;

        // 根据地图ID获取副本列表
        List<string> instanceData = InstanceMgr.GetDifficultyInstanceByMapId(mMapId, difficulty);

        //该地图没有副本数据;
        if (instanceData == null || instanceData.Count <= 0)
            return;

        // 显示奖励信息
        ShowClearanceBonus(instanceData[instanceData.Count - 1], mMapId);

        if (mItemList.Count < mItemAmount)
        {
            CreateGameObject(mItemList.Count, mItemAmount);
        }

        // 创建初始不够的item
        if (instanceData.Count > mItemAmount)
        {
            CreateGameObject(mItemList.Count, instanceData.Count);
        }

        // 通关数量
        float clearanceCount = 0f;

        int lockAmount = 0;

        for (int i = 0; i < instanceData.Count; i++)
        {
            if (!mItemList.ContainsKey("instacne" + i))
                continue;

            // 累计当前难度副本通关数量
            if (InstanceMgr.IsClearanced(ME.user, instanceData[i]))
                clearanceCount++;

            // 累计等级未解锁的副本数量
            if (!InstanceMgr.IsUnLockLevel(ME.user, instanceData[i]))
                lockAmount++;

            GameObject item = mItemList["instacne" + i];

            item.SetActive(true);

            //绑定数据;
            item.GetComponent<InstanceItemWnd>().Bind(instanceData[i], lockAmount);
        }

        // 隐藏多余的item
        for (int i = instanceData.Count; i < mItemAmount; i++)
        {
            if (!mItemList.ContainsKey("instacne" + i))
                continue;

            mItemList["instacne" + i].SetActive(false);
        }

        mGrid.repositionNow = true;

        float count = 0f;

        if (clearanceCount < instanceData.Count - 1)
            count = clearanceCount + 1.5f;
        else if (clearanceCount < instanceData.Count)
            count = clearanceCount + 1.0f;
        else
            count = clearanceCount;

        mScrollView.ResetPosition();

        Vector3 pos = new Vector3(mScrollView.panel.cachedTransform.localPosition.x,
            mScrollView.panel.cachedTransform.localPosition.y + count * mGrid.cellHeight - mScrollView.panel.GetViewSize().y,
            mScrollView.panel.cachedTransform.localPosition.z);

        if (clearanceCount < 3)
            return;

        // 设置滑动位置
        SpringPanel.Begin(mScrollView.panel.cachedGameObject, pos, 10f);
    }

    /// <summary>
    /// 显示副本通关奖励信息
    /// </summary>
    /// <param name="instanceId">Instance identifier.</param>
    void ShowClearanceBonus(string instanceId, int mapId)
    {
        LPCArray bonusList = InstanceMgr.GetAssignTaskBonus(ME.user, instanceId);

        // 先将窗口全部隐藏
        foreach (GameObject wnd in mClearanceBonusWndList)
            wnd.SetActive(false);

        for (int i = 0; i < bonusList.Count; i++)
        {
            GameObject wnd;
            // 需要单独创建奖励窗口
            if (i >= mClearanceBonusWndList.Count)
            {
                wnd = WindowMgr.CreateWindow(string.Format("{0}_{1}", ClearanceBonusWnd.WndType, i), ClearanceBonusWnd.PrefebResource, transform);
                wnd.transform.localPosition = new Vector3(-125.4f, -232.8f + 170f*i, 0f);

                if (wnd == null)
                {
                    LogMgr.Error("找不到ClearanceBonusWnd的预置");
                    return;
                }

                mClearanceBonusWndList.Add(wnd);
            }
            else
                wnd = mClearanceBonusWndList[i];

            wnd.SetActive(true);
            wnd.GetComponent<ClearanceBonusWnd>().BindData(mapId, bonusList[i].AsMapping);
        }
    }

    /// <summary>
    ///点击掉落宠物格子事件回调
    /// </summary>
    void OnClickPetItem(GameObject go)
    {
        DrapOutPetItemWnd item = go.GetComponent<DrapOutPetItemWnd>();

        if (item == null)
            return;

        int classId = item.ClassId;

        if (classId <= 0)
            return;

        if (mPetItem != null)
            mPetItem.Destroy();

        LPCMapping dBase = LPCMapping.Empty;
        dBase.Add("rid", Rid.New());
        dBase.Add("class_id", classId);

        mPetItem = PropertyMgr.CreateProperty(dBase);

        if (mPetItem == null)
            return;

        GameObject petWnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (petWnd == null)
            return;

        PetSimpleInfoWnd petSimpleInfoWnd = petWnd.GetComponent<PetSimpleInfoWnd>();

        petSimpleInfoWnd.Bind(mPetItem, true);
        petSimpleInfoWnd.ShowBtn(true);
    }

    /// <summary>
    ///箭头点击事件回调
    /// </summary>
    void OnClickArrowsBtn(GameObject go)
    {
        TweenPosition tweenPos = mDrapOut.GetComponent<TweenPosition>();

        if (tweenPos == null)
            return;

        if (IsClick)
        {
            tweenPos.PlayForward();
            IsClick = false;
        }
        else
        {
            tweenPos.PlayReverse();
            IsClick = true;
        }
    }

    /// <summary>
    /// 初始化场景相机的动画
    /// </summary>
    void ResetCameraPosition()
    {
        Camera sceneCamera = SceneMgr.SceneCamera;
        if (sceneCamera == null)
            return;

        Vector3 fromPos = SceneMgr.SceneCameraFromPos;

        SceneCamera control = sceneCamera.GetComponent<SceneCamera>();

        if (control != null)
            control.MoveCamera(sceneCamera.transform.position, fromPos);

        // 缓存场景相机的位置
        SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
        SceneMgr.SceneCameraToPos = fromPos;
    }

    /// <summary>
    ///关闭按钮点击事件回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        GameObject wnd = WindowMgr.OpenWnd("MainWnd");

        if (wnd == null)
            return;

        MainWnd mainWnd = wnd.GetComponent<MainWnd>();

        if (mainWnd == null)
            return;

        WindowMgr.ShowWindow(wnd);

        mainWnd.ShowMainUIBtn(false);

        // 关闭窗口;
        WindowMgr.HideWindow(gameObject);
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

        MainWnd mainWnd = wnd.GetComponent<MainWnd>();
        WindowMgr.ShowWindow(wnd);
        mainWnd.ShowMainUIBtn(false);
    }

    /// <summary>
    /// Saves the option.
    /// </summary>
    void SaveOption()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        LPCValue v = OptionMgr.GetOption(ME.user, "instance_difficulty");
        LPCMapping data = LPCMapping.Empty;

        if (v != null && v.IsMapping)
            data = v.AsMapping;

        data.Add(mMapId, difficulty);

        // 缓存副本难度到本地
        OptionMgr.SetOption(ME.user, "instance_difficulty", LPCValue.Create(data));
    }

    /// <summary>
    ///easy选项按钮点击事件回调
    /// </summary>
    void OnClickEasyBtn(GameObject go)
    {
        if (difficulty.Equals(InstanceConst.INSTANCE_DIFFICULTY_EASY))
            return;

        difficulty = InstanceConst.INSTANCE_DIFFICULTY_EASY;

        OptionsEffect();

        GetInstanceList(difficulty);

        // 保存难度选择
        SaveOption();
    }

    /// <summary>
    ///normal选项按钮点击事件回调
    /// </summary>
    void OnClickNormalBtn(GameObject go)
    {
        // 该难度副本未解锁不做以下操作;
        if (!InstanceMgr.DifficultyIsUnlock(ME.user, mMapId, InstanceConst.INSTANCE_DIFFICULTY_NORMAL))
        {
            LogMgr.Trace("Normal:该难度的副本未解锁");
            return;
        }

        if (difficulty.Equals(InstanceConst.INSTANCE_DIFFICULTY_NORMAL))
            return;

        difficulty = InstanceConst.INSTANCE_DIFFICULTY_NORMAL;

        OptionsEffect();

        GetInstanceList(difficulty);

        // 保存难度选择
        SaveOption();
    }

    /// <summary>
    ///hard选项按钮点击事件回调
    /// </summary>
    void OnClickHardBtn(GameObject go)
    {
        // 该难度副本未解锁不做以下操作;
        if (!InstanceMgr.DifficultyIsUnlock(ME.user, mMapId, InstanceConst.INSTANCE_DIFFICULTY_HARD))
        {
            LogMgr.Trace("Hard:该难度的副本未解锁");
            return;
        }

        if (difficulty.Equals(InstanceConst.INSTANCE_DIFFICULTY_HARD))
            return;

        difficulty = InstanceConst.INSTANCE_DIFFICULTY_HARD;

        OptionsEffect();

        GetInstanceList(difficulty);

        // 保存难度选择
        SaveOption();
    }

    /// <summary>
    ///帮助信息按钮点击事件
    /// </summary>
    void OnClickHelpTipsBtn(GameObject go)
    {
        // 获取帮助信息界面;
        GameObject wnd = WindowMgr.OpenWnd(HelpWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (wnd == null)
            return;

        wnd.GetComponent<HelpWnd>().Bind(HelpConst.SUIT_ID);
    }

    /// <summary>
    ///开关选中效果
    /// </summary>
    void OptionsEffect()
    {
        // 没有获取到组件;
        if (easyToggle == null || normalToggle == null || hardToggle == null)
            return;

        int notOpions = -1;
        int Options = 3;

        if (easyToggle.value)
        {
            // 设置选中按钮的label的alpha值;
            SetLabelAlpha(255, 150, 150);

            // 设置选中按钮的label的y轴的偏移
            LocalPositionOffset(Options, notOpions, notOpions);
        }
        if (normalToggle.value)
        {
            SetLabelAlpha(150, 255, 150);

            LocalPositionOffset(notOpions, Options, notOpions);
        }
        if (hardToggle.value)
        {
            SetLabelAlpha(150, 150, 255);

            LocalPositionOffset(notOpions, notOpions, Options);
        }
    }

    /// <summary>
    ///设置字体的alpha值
    /// </summary>
    void SetLabelAlpha(float easyAlpha, float normalAlpha, float hardAlpha)
    {
        // 设置字体的alpha值;
        mEasyLabel.color = new Color(mEasyLabel.color.r,
            mEasyLabel.color.g,
            mEasyLabel.color.b,
            easyAlpha / 255f);

        mNormalLabel.color = new Color(mNormalLabel.color.r,
            mNormalLabel.color.g,
            mNormalLabel.color.b,
            normalAlpha / 255f);

        mHardLabel.color = new Color(mHardLabel.color.r,
            mHardLabel.color.g,
            mHardLabel.color.b,
            hardAlpha / 255f);
    }

    /// <summary>
    ///设置label相对位置的y轴偏移
    /// </summary>
    void LocalPositionOffset(int easyOffset, int noarmalOffset, int hardOffset)
    {
        mEasyLabel.transform.localPosition = new Vector3(mEasyLabel.transform.localPosition.x,
            easyOffset,
            mEasyLabel.transform.localPosition.z);

        mNormalLabel.transform.localPosition = new Vector3(mNormalLabel.transform.localPosition.x,
            noarmalOffset,
            mNormalLabel.transform.localPosition.z);

        mHardLabel.transform.localPosition = new Vector3(mHardLabel.transform.localPosition.x,
            hardOffset,
            mHardLabel.transform.localPosition.z);
    }

    /// <summary>
    ///判断正常或者较难的难度副本是否解锁
    /// </summary>
    void JudgeNormalAndHardIsUnLock()
    {
        // 判断normal难度的副本是否解锁;
        if (InstanceMgr.DifficultyIsUnlock(ME.user, mMapId, InstanceConst.INSTANCE_DIFFICULTY_NORMAL))
        {
            mNormalLock.SetActive(false);
            normalToggle.enabled = true;
            normalCheckMark.SetActive(true);
        }
        else
        {
            normalToggle.enabled = false;

            mNormalLock.SetActive(true);

            normalCheckMark.SetActive(false);
        }

        // 判断hard难度的副本是否解锁;
        if (InstanceMgr.DifficultyIsUnlock(ME.user, mMapId, InstanceConst.INSTANCE_DIFFICULTY_HARD))
        {
            mHardLock.SetActive(false);
            hardToggle.enabled = true;
            hardCheckMark.SetActive(true);
        }
        else
        {
            hardToggle.enabled = false;
            mHardLock.SetActive(true);
            hardCheckMark.SetActive(false);
        }
    }

    /// <summary>
    ///初始化label的位置
    /// </summary>
    void InitToggleLabelPos()
    {
        normalPos = mNormalLabel.transform.localPosition;
        hardPos = mHardLabel.transform.localPosition;

        if (InstanceMgr.DifficultyIsUnlock(ME.user, mMapId, InstanceConst.INSTANCE_DIFFICULTY_NORMAL))
            mNormalLabel.transform.localPosition = new Vector3(0, normalPos.y, normalPos.z);
        else
            mNormalLabel.transform.localPosition = new Vector3(11, normalPos.y, normalPos.z);

        if (InstanceMgr.DifficultyIsUnlock(ME.user, mMapId, InstanceConst.INSTANCE_DIFFICULTY_HARD))
            mHardLabel.transform.localPosition = new Vector3(0, hardPos.y, hardPos.z);
        else
            mHardLabel.transform.localPosition = new Vector3(15, hardPos.y, hardPos.z);

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

    #endregion

    #region 外部接口

    public void Bind(int mapId, Vector3 clickPos)
    {
        mMapId = mapId;

        if (mapId < 1)
        {
            LogMgr.Trace("地图id不存在");
            return;
        }

        // 移动相机
        MoveCamera(clickPos);

        JudgeNormalAndHardIsUnLock();

        // 获取本地缓存的难度信息
        LPCValue v = OptionMgr.GetOption(ME.user, "instance_difficulty");

        int diff = 0;
        if (v == null || ! v.IsMapping)
        {
            diff = InstanceConst.INSTANCE_DIFFICULTY_EASY;
        }
        else
        {
            if (! v.AsMapping.ContainsKey(mapId))
                diff = InstanceConst.INSTANCE_DIFFICULTY_EASY;
            else
                diff = v.AsMapping.GetValue<int>(mapId);
        }

        // 重置difficulty
        difficulty = 0;

        mMask.SetActive(true);

        InitDrapOutGoodsInfo();

        InitToggleLabelPos();

        switch (diff)
        {
            case InstanceConst.INSTANCE_DIFFICULTY_EASY:
                easyToggle.Set(true);
                normalToggle.Set(false);
                hardToggle.Set(false);
                OnClickEasyBtn(easyToggle.gameObject);
                break;
            case InstanceConst.INSTANCE_DIFFICULTY_NORMAL:
                normalToggle.Set(true);
                easyToggle.Set(false);
                hardToggle.Set(false);
                OnClickNormalBtn(normalToggle.gameObject);
                break;
            case InstanceConst.INSTANCE_DIFFICULTY_HARD:
                hardToggle.Set(true);
                easyToggle.Set(false);
                normalToggle.Set(false);
                OnClickHardBtn(hardToggle.gameObject);
                break;
            default:
                break;
        }

        // 初始化选中的效果;
        OptionsEffect();
    }

    #endregion
}
