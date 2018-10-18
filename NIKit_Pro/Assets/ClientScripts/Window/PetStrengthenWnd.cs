/// <summary>
/// PetStrengthenWnd.cs
/// Created by fengsc 2016/07/20
///宠物强化窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class PetStrengthenWnd : WindowBase<PetStrengthenWnd>
{
    #region 成员变量

    public float expSliderPlayTime = 2f;
    public Vector2 mInitPos = new Vector2(0, 0);
    public Vector2 mItemSpace = new Vector2(6, 6);
    public Vector2 mItemSize = new Vector2(110, 110);
    public GameObject mGrid;
    public GameObject closeBtn;
    public UIScrollView mPetListScrollView;
    public UIWrapContent petcontent;
    public GameObject mStrengthenPet;
    //材料宠物 左边5个
    public CustomPetItemWnd[] mMaterialPets;
    //主宠物星星
    public GameObject[] mStars;
    public UISprite mElement;
    public UITexture mIcon;
    public UILabel mLevelAndName;
    public UISlider mExpSlider;
    public UILabel mExpTips;
    public UILabel mExpPercent;
    public UILabel mLevel;
    public UILabel mLevelTips;
    public UILabel mLiftLevel;
    public UILabel mPower;
    public UILabel mPowerTips;
    public UILabel mLiftPower;
    public UILabel mAttack;
    public UILabel mAttackTips;
    public UILabel mLiftAttack;
    public UILabel mDefence;
    public UILabel mDefenceTips;
    public UILabel mStarTips;
    public UISprite[] mLeftStars;
    public UISprite[] mRightStars;
    public UILabel mLiftDefence;
    public GameObject mPetStrengthenToggle;
    public UILabel mPetStrengthenLabel;
    public GameObject mStrengthenRewardToggle;
    public UILabel mRewardLabel;
    public GameObject mUpgradeBtn;
    public UILabel mUpgardeLabel;
    public UILabel mUpgradeCostLab;
    public UISprite mUpgradeCostIcon;
    public GameObject mStarLiftBtn;
    public UILabel mStarLiftLabel;
    public UILabel mStarLiftGoldLab;
    public UISprite mStarLiftCostIcon;
    public UILabel mPanelTitle;
    public GameObject mStrengthenPanel;
    public GameObject mRewardPanel;
    public GameObject mStrengthenScrollVivew;
    public UIScrollView mRewardScrollVivew;
    public UILabel mSelectPetTip;
    public GameObject mPetInfoDetails;
    public GameObject mWhitemask;
    public UILabel mAddFactorTip;
    public UISpriteAnimation mStarBtnAnima;
    public UISpriteAnimation[] mMaterialAnima;
    public UISpriteAnimation mStrPetAnima;
    public GameObject mUpgradeBtnCover;
    public GameObject mUpStarBtnCover;
    public GameObject mWaitCover;
    public GameObject[] mStarEffects;
    public GameObject mStarupEfect;
    public GameObject mStarupEffectLb;
    public UILabel[] mLevelUpTips;
    public GameObject mLevelUpBtn;
    //召唤卷升级的页签icon
    public UITexture mMaterialPageIcon;
    //使魔材料页签按钮
    public GameObject mMaterialBtn;
    //召唤卷升级页签按钮
    public GameObject mReelBtn;
    public GameObject mMaterialsPart;
    public GameObject mReelPart;
    //卷轴icon
    public UITexture mReelIcon;
    //卷轴数量label
    public UILabel mReelNumLab;
    //使用{0}个{1}提升经验
    public UILabel mReelDesLab;
    public UISlider mReelSlider;
    public GameObject mReelSliderThumb;

    public GameObject mPetStrWnd;
    public GameObject mResearchRewardWnd;

    public GameObject mPetItem;

    public TweenScale mTweenScale;
    public TweenAlpha mTweenAlpha;

    public UILabel mStarup;
    public UILabel mShadow;

    // 分享按钮
    public GameObject mShareBtn;
    public UILabel mShareLb;

    #endregion

    #region 私有变量

    const int mColumnNum = 4;
    const int mRowNum = 7;

    // 存储玩家宠物信息
    private List<Property> arrPetData = new List<Property>();

    // 存储选择宠物的Rid
    private List<string> mSelectRidList = new List<string>();

    // 加强宠物的rid
    private string mStrengthenPetRid = string.Empty;

    // 悬浮面板宠物rid
    string pressRid = string.Empty;

    // 宠物格子数量
    private int containerSize = 0;

    private List<float> star_x = new List<float>();

    // 强化需要的材料宠物最大数量
    private const int amount = 5;

    // 升级前经验条进度
    private float preUpgradePrecent = 0f;

    // 升级前等级
    private int preUpgradeLev = 0;

    // 当前显示数据的index与实际数据的对应关系
    private Dictionary<int, int> indexMap = new Dictionary<int, int>();

    // name与posOb的对应关系
    private Dictionary<string, CustomPetItemWnd> mPosObMap = new Dictionary<string, CustomPetItemWnd>();

    // 起始位置
    private Dictionary<GameObject,Vector3> rePosition = new Dictionary<GameObject,Vector3>();

    // 升级（星）后克隆宠物对象
    private Property strClonePet = null;

    // 立即结束
    public bool ImmediEnd = false;

    private bool mIsShowDialog = false;

    private float mLastTime = 0f;

    private MaterialPageType mCurMaterialPageType = MaterialPageType.None;
    private enum MaterialPageType
    {
        None = 0,
        /// <summary>
        /// 使魔材料
        /// </summary>
        Pet,
        /// <summary>
        /// 属性道具-目前是召唤卷
        /// </summary>
        PROPERTY,
    }

    //使魔升级属性道具列表
    private LPCArray mMaterialList = LPCArray.Empty;
    private LPCMapping mMaterialMap = LPCMapping.Empty;
    //每个召唤卷轴提升的经验值
    private int mPerItemExp;

    #endregion

    #region 内部函数

    void Awake()
    {
        //注册事件;
        RegisterEvent();

        // 记录下星星的原始位置
        for (int i = 0; i < mStars.Length; i++)
            star_x.Add(mStars[i].transform.localPosition.x);

        // 对强化格子做显示定义
        for (int i = 0; i < mMaterialPets.Length; i++)
        {
            mMaterialPets[i].ShowLeaderSkill(false);
            mMaterialPets[i].ShowMaxLevel(true);
            mMaterialPets[i].SetIcon("addpet");
        }

        //实例化玩家宠物格子
        InitPlayerAllPetGrid();

        //初始化label内容;
        SetLabelContent();

        //初始化使魔升级吞噬属性道具列表
        List<int> materialList = ItemMgr.GetPetUpgradeMaterials();
        if (materialList.Count <= 0)
            return;

        for (int i = 0; i < materialList.Count; i++)
        {
            LPCMapping materialMap = LPCMapping.Empty;
            materialMap.Add("class_id", materialList[i]);
            materialMap.Add("amount", 0);
            mMaterialList.Add(materialMap);
        }

        mMaterialMap = mMaterialList[0].AsMapping;

        mPerItemExp = GetPropertyMaterialExp();

        //初始化召唤卷升级icon
        string path = string.Format("Assets/Art/UI/Icon/item/{0}.png", ItemMgr.GetClearIcon(materialList[0]));
        mReelIcon.mainTexture = ResourceMgr.LoadTexture(path);
        mMaterialPageIcon.mainTexture = ResourceMgr.LoadTexture(path);

    }

    /// <summary>
    /// Resets the scroll view.
    /// </summary>
    private void ResetScrollView()
    {
        // 重新设置item的初始位置
        foreach (GameObject item in rePosition.Keys)
        {
            item.transform.localPosition = rePosition[item];
        }

        // 整理位置
        mPetListScrollView.ResetPosition();

        // 重新初始化indexMap
        if (indexMap != null)
        {
            indexMap.Clear();
            for (int i = 0; i < mRowNum; i++)
                indexMap.Add(i, -i);
        }
    }

    void OnEnable()
    {
        // 注册回调事件
        EventMgr.RegisterEvent("PetStrengthenWnd", EventMgrEventType.EVENT_PET_UPGRADE, OnPetUpgrade);
        EventMgr.RegisterEvent("PetStrengthenWnd", EventMgrEventType.EVENT_PET_STARUP, OnPetStarup);
        MsgMgr.RegisterDoneHook("MSG_LOGIN_NOTIFY_OK", "PetStrengthenWnd", WhenLoginOk);

        if (ME.user != null)
        {
            ME.user.baggage.eventCarryChange += BaggageChange;
            ME.user.dbase.RegisterTriggerField("PetStrengthenWnd", new string[] { "container_size" , "stone_unknown", }, new CallBack(OnBagContainerSizeChange));
        }

        // 显示mPetStrWnd
        mPetStrWnd.SetActive (true);

        mShareBtn.SetActive(false);

        // 重绘窗口
        // 这个东西一定要放在Tween之前，否则会影响ScrollView位置
        ReseetWnd();

        // 重新播放缩放动画
        mTweenScale.enabled = true;
        mTweenScale.ResetToBeginning();

        // 重新播放渐变动画
        mTweenAlpha.enabled = true;
        mTweenAlpha.ResetToBeginning();

        // 主要是由于UIToggle在OnEnable和OnDisable添加列表的操作
        mPetStrengthenToggle.GetComponent<UIToggle>().Set(true);
        mStrengthenRewardToggle.GetComponent<UIToggle>().Set(false);

        //默认开启使魔材料
        OnMaterialPage(null);

        RefreshShareBtn();
    }

    /// <summary>
    /// 登陆成功回调
    /// </summary>
    private void WhenLoginOk(string cmd, LPCValue para)
    {
        mWaitCover.SetActive(false);
    }

    void RefreshShareBtn()
    {
        if (!GuideMgr.IsGuided(GuideMgr.SHARE_SHOW_GUIDE_GROUP) || !ShareMgr.IsOpenShare())
        {
            mShareBtn.SetActive(false);

            return;
        }

        Property ob = Rid.FindObjectByRid(mStrengthenPetRid);
        if (ob == null)
        {
            mShareBtn.SetActive(false);
            return;
        }

        if (ob.GetStar() < GameSettingMgr.GetSetting<int>("share_pet_star"))
        {
            mShareBtn.SetActive(false);
        }
        else
        {
            mShareBtn.SetActive(true);
        }
    }

    void OnDisable()
    {
        // 显示mPetStrWnd面板
        mPetStrWnd.SetActive (true);
        mResearchRewardWnd.SetActive (false);

        // 析构临时宠物
        if (strClonePet != null)
            strClonePet.Destroy();

        // 清除界面缓存资源
        arrPetData.Clear();
        mSelectRidList.Clear();
        mStrengthenPetRid = string.Empty;

        // 从正在打开列表中移除
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        MsgMgr.RemoveDoneHook("MSG_LOGIN_NOTIFY_OK", "PetStrengthenWnd");

        // 注销事件
        EventMgr.UnregisterEvent("PetStrengthenWnd");

        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 移除属性字段关注回调
        ME.user.dbase.RemoveTriggerField("PetStrengthenWnd");

        // 取消注册玩家装备道具事件
        ME.user.baggage.eventCarryChange -= BaggageChange;

        //初始材料页签状态
        mCurMaterialPageType = MaterialPageType.None;
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void ReseetWnd()
    {
        // 重置滚动面板
        ResetScrollView();

        // 刷新宠物数据;
        RedrawPetsPanel();

        // 初始化强化信息界面
        RedrawStrPet();

        // 初始化消耗按钮
        RedrawCostBtn();

        mPetItem.SetActive (false);

        // 没有研究奖励隐藏按钮
        List<int> taskData = TaskMgr.GetTasksData (ME.user, TaskConst.RESEARCH_TASK, false);
        mStrengthenRewardToggle.SetActive(taskData.Count == 0 ? false:true);

        // 重绘整个窗口
        Redraw();
    }

    /// <summary>
    ///初始化本地化文本
    /// </summary>
    private void SetLabelContent()
    {
        mPanelTitle.text = LocalizationMgr.Get("PetStrengthenWnd_2");
        mUpgardeLabel.text = LocalizationMgr.Get("PetStrengthenWnd_3");
        mStarLiftLabel.text = LocalizationMgr.Get("PetStrengthenWnd_4");
        mPetStrengthenLabel.text = LocalizationMgr.Get("PetStrengthenWnd_5");
        mRewardLabel.text = LocalizationMgr.Get("PetStrengthenWnd_6");
        mExpTips.text = LocalizationMgr.Get("PetStrengthenWnd_7");
        mLevelTips.text = LocalizationMgr.Get("PetStrengthenWnd_8");
        mAttackTips.text = LocalizationMgr.Get("PetStrengthenWnd_9");
        mDefenceTips.text = LocalizationMgr.Get("PetStrengthenWnd_10");
        mSelectPetTip.text = LocalizationMgr.Get("PetStrengthenWnd_11");
        mStarTips.text = LocalizationMgr.Get("PetStrengthenWnd_20");
        mStarup.text = LocalizationMgr.Get("PetStrengthenWnd_36");
        mShadow.text = LocalizationMgr.Get("PetStrengthenWnd_36");
        mShareLb.text = LocalizationMgr.Get("PetStrengthenWnd_46");

        for (int i = 0; i < mLevelUpTips.Length; i++)
            mLevelUpTips[i].text = LocalizationMgr.Get("PetStrengthenWnd_28");

        mLevelUpBtn.GetComponent<UILabel>().text = LocalizationMgr.Get("PetStrengthenWnd_29");
    }

    /// <summary>
    ///注册事件
    /// </summary>
    void RegisterEvent()
    {
        //注册按钮的点击事件;
        UIEventListener.Get(closeBtn).onClick += ClickCloseUI;
        UIEventListener.Get(mUpgradeBtn).onClick = OnClickUpgradeBtn;
        UIEventListener.Get(mStarLiftBtn).onClick = OnClickStarLiftBtn;
        UIEventListener.Get(mPetStrengthenToggle).onClick = OnClickPetStrengthen;
        UIEventListener.Get(mStrengthenRewardToggle).onClick = OnClickStrengthenReward;
        UIEventListener.Get(mStrengthenPet).onClick = OnClickCancelStrenthen;
        UIEventListener.Get(mLevelUpBtn).onClick = OnLevelUpHelpBtn;
        UIEventListener.Get(mWaitCover).onClick = OnWaitCoverClick;
        UIEventListener.Get(mMaterialBtn).onClick = OnMaterialPage;
        UIEventListener.Get(mReelBtn).onClick = OnReelPage;
        UIEventListener.Get(mShareBtn).onClick = OnClickShareBtn;

        //添加宠物的点击事件，点击取消选择宠物;
        for (int i = 0; i < mMaterialPets.Length; i++)
            UIEventListener.Get(mMaterialPets[i].gameObject).onClick = OnClickMaterial;

        //添加滑动列表时的回调;
        petcontent.onInitializeItem = UpdateItem;

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// 分享按钮点击回调
    /// </summary>
    void OnClickShareBtn(GameObject go)
    {
        Property ob = Rid.FindObjectByRid(mStrengthenPetRid);
        if (ob == null)
            return;

        if (ob.GetStar() < GameSettingMgr.GetSetting<int>("share_pet_star"))
            return;

        GameObject wndGo = WindowMgr.OpenWnd(ShareOperateWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wndGo == null)
            return;

        wndGo.GetComponent<ShareOperateWnd>().BindData(ShareOperateWnd.ShareOperateType.PetUpStar, ob);
    }

    /// <summary>
    /// tween动画播放完后回调
    /// </summary>
    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 包裹变化回调
    /// </summary>
    /// <param name="pos">Position.</param>
    void BaggageChange(string[] pos)
    {
        // 窗口没有显示，不处理
        if (gameObject == null ||
            !gameObject.activeSelf ||
            !gameObject.activeInHierarchy)
            return;

        // 判断是否是宠物
        bool isChangeed = false;

        // 遍历改变的包裹格子
        for (int i = 0; i < pos.Length; i++)
        {
            // 不是宠物包裹
            if (! ContainerConfig.IS_PET_POS(pos[i]))
                continue;

            // 标识改变需要重绘窗口
            isChangeed = true;
            break;
        }

        // 宠物包裹没有发生变化
        if (! isChangeed)
            return;

        // 延迟到下一帧调用, 汇总宠物包裹面板
        MergeExecuteMgr.DispatchExecute(RedrawPetsPanel);
    }

    /// <summary>
    /// 包裹格子数量变化回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="param">Parameter.</param>
    void OnBagContainerSizeChange(object para, params object[] param)
    {
        // 窗口没有显示，不处理
        if (gameObject == null ||
            !gameObject.activeSelf ||
            !gameObject.activeInHierarchy)
            return;

        // 刷新界面
        RedrawPetsPanel();
    }

    /// <summary>
    ///实例化玩家所有的宠物格子
    /// </summary>
    void InitPlayerAllPetGrid()
    {
        // 生成格子，只生成这么多格子，动态复用
        for (int i = 0; i < mRowNum; i++)
        {
            GameObject rowItemOb = new GameObject();
            rowItemOb.name = string.Format("strengthenItem_{0}", i);
            rowItemOb.transform.parent = mGrid.transform;
            rowItemOb.transform.localPosition = new Vector3(mInitPos.x, mInitPos.y - i * 115, 0);
            rowItemOb.transform.localScale = Vector3.one;

            rePosition.Add(rowItemOb, rowItemOb.transform.localPosition);

            for(int j = 0; j < mColumnNum;j++)
            {
                GameObject posWnd = Instantiate (mPetItem) as GameObject;
                posWnd.transform.parent = rowItemOb.transform;
                posWnd.name = string.Format("petStrengthen_item_{0}_{1}", i, j);
                posWnd.transform.localScale = new Vector3 (0.9f, 0.9f, 0.9f);
                posWnd.transform.localPosition = new Vector3((mItemSize.x + mItemSpace.x) * j, 0f, 0);

                posWnd.SetActive(true);

                // 自定义宠物格子显示
                CustomPetItemWnd itemWnd = posWnd.GetComponent<CustomPetItemWnd>();
                itemWnd.ShowLeaderSkill(false);
                itemWnd.ShowMaxLevel(true);
                mPosObMap.Add (string.Format ("petStrengthen_item_{0}_{1}", i, j), itemWnd);

                UIEventListener.Get(posWnd).onClick = ClickSelectPet;
                UIEventListener.Get(posWnd).onPress = OnPressShowPetInfo;
            }
        }
    }

    /// <summary>
    ///初始化宠物数据
    /// </summary>
    void InitPetData()
    {
        //获取玩家当前所有的宠物数据;
        arrPetData = BaggageMgr.GetItemsByPage(ME.user, ContainerConfig.POS_PET_GROUP);

        // 对宠物数据按等级进行排序
        arrPetData = BaggageMgr.SortPetInBag(arrPetData, MonsterConst.SORT_BY_STAR);

        int Row = mRowNum;

        containerSize = ME.user.baggage.ContainerSize[ContainerConfig.POS_PET_GROUP].AsInt;

        // 此处包裹中的东西数量有可能比包裹容量大
        if (arrPetData.Count > containerSize)
            containerSize = arrPetData.Count;

        int maxSize = GameSettingMgr.GetSettingInt("max_pet_baggage_size");

        int containerRow = containerSize % mColumnNum == 0 ?
            containerSize / mColumnNum : containerSize / mColumnNum + 1;

        // 多显示一行用来显示添加格子按钮
        if (containerRow >= mRowNum)
            Row = containerRow + 1;

        int maxRow = maxSize % mColumnNum == 0 ?
            maxSize / mColumnNum : maxSize / mColumnNum + 1;

        // 已达到最大格子数量，不显示添加格子按钮
        if (containerRow >= maxRow)
            Row = containerRow;

        petcontent.maxIndex = 0;

        petcontent.minIndex = -(Row - 1);
    }

    /// <summary>
    ///设置滑动列表时复用宠物格子更改数据
    /// </summary>
    void UpdateItem(GameObject go, int index, int realIndex)
    {
        // 将index与realindex对应关系记录下来
        if (!indexMap.ContainsKey(index))
            indexMap.Add(index, realIndex);
        else
            indexMap[index] = realIndex;

        FillData(index, realIndex);
    }

    /// <summary>
    /// 填充数据
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="realIndex">Real index.</param>
    private void FillData(int index, int realIndex)
    {
        for (int i = 0; i < mColumnNum; i++)
        {
            // 获取CustomPetItemWnd组件
            CustomPetItemWnd petItem = mPosObMap[string.Format("petStrengthen_item_{0}_{1}", index, i)];
            if (petItem == null)
                continue;

            //计算索引，通过索引拿到对应的宠物数据;
            int dataIndex = System.Math.Abs(realIndex) * mColumnNum + i;
            if (dataIndex < arrPetData.Count)
            {
                //设置宠物数据;
                petItem.SetBind(arrPetData[dataIndex]);

                if (!string.IsNullOrEmpty(mStrengthenPetRid))
                {
                    // 共享，锁定，防御宠物,召唤卷轴升级显示黑色蒙版
                    if (PetMgr.IsSharePet(arrPetData[dataIndex].GetRid())
                        || PetMgr.IsLockPet(arrPetData[dataIndex])
                        || PetMgr.IsDefenceTroop(ME.user, arrPetData[dataIndex])
                        || PetMgr.IsGuidePet(ME.user, arrPetData[dataIndex].GetRid())
                        || mCurMaterialPageType == MaterialPageType.PROPERTY)
                        petItem.ShowCover(true);
                    else
                        petItem.ShowCover(false);
                }
                else
                {
                    petItem.SetLock("");
                }

                petItem.SetSelected(false);

                // 要强化宠物(非强化材料)不显示lock
                if (!string.IsNullOrEmpty(mStrengthenPetRid) &&
                    arrPetData[dataIndex].GetRid().Equals(mStrengthenPetRid))
                {
                    petItem.SetSelected(true);
                    petItem.SetLock("");
                    continue;
                }

                for (int j = 0; j < mSelectRidList.Count; j++)
                {
                    if (arrPetData[dataIndex].GetRid().Equals(mSelectRidList[j]))
                        petItem.SetSelected(true);
                }
            }
            else if (arrPetData.Count < containerSize && dataIndex < containerSize)
            {
                petItem.SetSelected(false);
                petItem.SetBind(null);
                petItem.SetIcon(null);
                petItem.SetLock("");
            }
            else
            {
                petItem.SetSelected(false);
                petItem.SetBind(null);
                petItem.SetIcon("addpet");
                petItem.SetLock("");
            }
        }
    }

    /// <summary>
    ///点击关闭窗口按钮
    /// </summary>
    private void ClickCloseUI(GameObject go)
    {
        // 打开主窗口
        WindowMgr.OpenWnd("MainWnd");

        // 隐藏窗口
        WindowMgr.HideWindow(gameObject);
    }

    /// <summary>
    /// 刷新界面
    /// </summary>
    private void Redraw()
    {
        RedrawMaterialsPanel();
        RedrawStrPet();
        RedrawPetsSelect();
        RedrawCostBtn();
    }

    /// <summary>
    /// 刷新召唤卷升级
    /// </summary>
    private void RedrawReel()
    {
        if (ME.user == null || mMaterialMap.Count <= 0)
            return;

        int ownReelCounts = UserMgr.GetAttribItemAmount(ME.user, mMaterialMap["class_id"].AsInt);
        mReelDesLab.text = string.Format(LocalizationMgr.Get("PetStrengthenWnd_42"), mMaterialMap["amount"].AsInt);
        mReelNumLab.text = ownReelCounts.ToString();
        mReelSlider.value = 0f;
        //改变数目颜色
        if (mMaterialMap["amount"].AsInt > 0)
            mReelNumLab.color = ColorConfig.ParseToColor("ff7f7f");
        else
            mReelNumLab.color = ColorConfig.ParseToColor("e4e4e4");
    }

    /// <summary>
    /// 刷新宠物面板
    /// </summary>
    private void RedrawPetsPanel()
    {
        // 窗口已经析构，不在重绘
        if (this == null)
            return;

        // 刷新数据
        InitPetData();

        // 填充数据
        foreach (KeyValuePair<int, int> kv in indexMap)
        {
            FillData(kv.Key, kv.Value);
        }
    }

    /// <summary>
    /// 刷新宠物列表选择状态
    /// </summary>
    private void RedrawPetsSelect()
    {
        Property item_ob;

        // 取消强化材料对应到宠物列表中的选中状态
        foreach (CustomPetItemWnd itemWnd in mPosObMap.Values)
        {
            // 对象不存在
            if (itemWnd == null)
                continue;

            // 窗口没有绑定对象
            item_ob = itemWnd.item_ob;
            if (item_ob == null)
                continue;

            // 当前选中宠物为空
            if (string.IsNullOrEmpty(mStrengthenPetRid))
            {
                itemWnd.SetSelected(false);
                itemWnd.SetLock("");
                continue;
            }

            if (item_ob.GetRid().Equals(mStrengthenPetRid))
            {
                itemWnd.SetSelected(true);
            }
            else
            {
                itemWnd.SetSelected(false);
                for (int j = 0; j < mSelectRidList.Count; j++)
                {
                    if (item_ob.GetRid().Equals(mSelectRidList[j]))
                    {
                        itemWnd.SetSelected(true);

                        break;
                    }
                }

                // 共享，锁定，防御宠物,召唤卷升级显示黑色蒙版
                if (PetMgr.IsSharePet(item_ob.GetRid())
                    || PetMgr.IsLockPet(item_ob)
                    || PetMgr.IsDefenceTroop(ME.user, item_ob)
                    || PetMgr.IsGuidePet(ME.user, item_ob.GetRid())
                    || mCurMaterialPageType == MaterialPageType.PROPERTY)
                    itemWnd.ShowCover(true);
                else
                    itemWnd.ShowCover(false);
            }
        }
    }

    /// <summary>
    /// 刷新强化材料窗口
    /// </summary>
    private void RedrawMaterialsPanel()
    {
        // 获取临时克隆宠物
        if (strClonePet != null)
            strClonePet.Destroy();
        strClonePet = GetStrClonePet();

        // 获取升星所需材料的位置列表
        List<int> materialPosList = PetsmithMgr.GetMaterialStarPos(mStrengthenPetRid);

        // 收集材料列表
        List<string> mMaterialRidList = new List<string>();
        foreach (string rid in mSelectRidList)
            mMaterialRidList.Add(rid);

        for (int i = 0; i < mMaterialPets.Length; i++)
        {
            Property material_ob = mMaterialPets[i].item_ob;

            // 当前材料框不为为空并且材料列表中包含该材料框中的宠物
            if (material_ob != null && mMaterialRidList.Contains(material_ob.GetRid()))
                mMaterialRidList.Remove(material_ob.GetRid());
            else
            {
                mLevelUpTips[i].gameObject.SetActive(false);

                // 如果强化材料为不为空
                if (!string.IsNullOrEmpty(mStrengthenPetRid))
                {
                    Property str_ob = Rid.FindObjectByRid(mStrengthenPetRid);

                    // 满级材料的空材料框的icon与不满级的不一样
                    if (str_ob != null && MonsterMgr.IsMaxLevel(str_ob))
                    {
                        if (i < materialPosList.Count)
                            mMaterialPets[i].SetIcon(string.Format("strengthen_{0}", materialPosList[i]));
                        else
                            mMaterialPets[i].SetIcon("forbid");
                    }
                    else
                        mMaterialPets[i].SetIcon("addpet");

                }
                else
                    mMaterialPets[i].SetIcon("addpet");

                // 空格子或者不在材料列表中直接空
                mMaterialPets[i].SetBind(null);
            }
        }

        Property add_pet = null;
        Property pet_ob = null;

        // 填充mMaterialRidList剩余的的宠物数据(此处要保证填入的是第一个空的材料格子)
        for (int i = 0; i < mMaterialRidList.Count; i++)
        {
            pet_ob = Rid.FindObjectByRid(mStrengthenPetRid);
            add_pet = Rid.FindObjectByRid(mMaterialRidList[i]);
            if (add_pet == null)
                continue;

            Property material_pet = null;
            for (int j = 0; j < mMaterialPets.Length; j++)
            {
                material_pet = mMaterialPets[j].item_ob;
                if (material_pet != null)
                    continue;

                // 是宠物升级材料要显示“升级材料提示”
                if (MonsterMgr.IsSkillLevelUpMaterial(pet_ob, add_pet))
                    mLevelUpTips[j].gameObject.SetActive(true);

                mMaterialPets[j].SetBind(add_pet);
                break;
            }
        }
    }

    /// <summary>
    /// 获取属性材料mapping
    /// </summary>
    /// <returns></returns>
    private LPCMapping GetPropertyMaterialMap()
    {
        if (mMaterialList.Count <= 0)
            return null;

        return mMaterialList[0].AsMapping;
    }

    /// <summary>
    /// 获取属性材料经验
    /// </summary>
    /// <returns></returns>
    private int GetPropertyMaterialExp()
    {
        LPCMapping materialMap = GetPropertyMaterialMap();

        if (materialMap == null)
            return 0;

        CsvRow csv = ItemMgr.GetRow(materialMap["class_id"].AsInt);
        if (csv == null)
            return 0;

        int scriptId = csv.Query<int>("exp_script");
        if (scriptId <= 0)
            return 0;

        return (int)ScriptMgr.Call(scriptId, csv.Query<int>("exp_arg"));

    }

    /// <summary>
    /// 升级clone宠物
    /// </summary>
    /// <param name="addExp">1.0吞噬使魔 2.大于0吞噬卷轴</param>
    /// <returns></returns>
    private Property GetStrClonePet(int addExp = 0)
    {
        if (string.IsNullOrEmpty(mStrengthenPetRid))
            return null;

        Property pet_ob = Rid.FindObjectByRid(mStrengthenPetRid);
        if (pet_ob == null)
            return null;

        LPCMapping debase = new LPCMapping();
        Property clonePet = null;

        // 满级宠物
        if (MonsterMgr.IsMaxLevel(pet_ob))
        {
            // 宠物已达最大星级
            if (MonsterMgr.IsMaxStar(pet_ob))
            {
                debase.Add("exp", 0);
            }
            else
            {
                debase.Add("level", 1);
                debase.Add("star", pet_ob.GetStar() + 1);
                debase.Add("exp", 0);
            }

            // clone宠物对象
            clonePet = PropertyMgr.DuplicateProperty(pet_ob, debase);
        }
        else
        {
            // 材料列表为空不克隆宠物
            if (mSelectRidList.Count == 0 && addExp <= 0)
                return null;

            // 获取
            int add_exp = 0;
            if (addExp > 0)
                add_exp = addExp;
            else
                add_exp = PetsmithMgr.GetAddExp(mStrengthenPetRid, mSelectRidList);

            // 复制数据
            debase.Add("exp", pet_ob.Query<int>("exp") + add_exp);

            // clone宠物对象
            clonePet = PropertyMgr.DuplicateProperty(pet_ob, debase);

            // 尝试升级
            PetMgr.TryLevelUp(clonePet);

            // 执行升级后处理
            MonsterMgr.DoUpgrade(clonePet);
        }

        return clonePet;
    }

    /// <summary>
    /// 刷新要强化的宠物信息
    /// </summary>
    /// <param name="pet_ob">Pet ob.</param>
    private void RedrawStrPet()
    {
        Property pet_ob = null;

        if (!string.IsNullOrEmpty(mStrengthenPetRid))
            pet_ob = Rid.FindObjectByRid(mStrengthenPetRid);

        RedrawStrPetInfo(pet_ob);
        RedrawPetAttributeWnd(pet_ob);
    }

    /// <summary>
    /// 重绘强化宠物信息
    /// </summary>
    private void RedrawStrPetInfo(Property pet_ob)
    {
        // 若宠物为空
        if (pet_ob == null)
        {
            for (int i = 0; i < mStars.Length; i++)
                mStars[i].SetActive(false);

            mLevelAndName.gameObject.SetActive(false);
            mElement.gameObject.SetActive(false);

            string resPath = string.Format("Assets/Art/UI/Icon/monster/emptypet.png");
            mIcon.mainTexture = ResourceMgr.LoadTexture(resPath);
        }
        else
        {
            int classId = pet_ob.GetClassID();

            //获取宠物元素的图标;
            mElement.spriteName = PetMgr.GetElementIconName(MonsterMgr.GetElement(pet_ob.GetClassID()));
            mElement.gameObject.SetActive(true);

            //获取宠物的星级;
            int star = pet_ob.GetStar();

            int count = star < mStars.Length ? star : mStars.Length;

            //根据是否觉醒设置星级图标的类型;
            string starName = PetMgr.GetStarName(pet_ob.GetRank());

            int offset = (mStars.Length - count) * 10;

            for (int i = 0; i < count; i++)
            {
                mStars[i].GetComponent<UISprite>().spriteName = starName;

                mStars[i].SetActive(true);

                mStars[i].transform.localPosition = new Vector3(star_x[i] + offset,
                    mStars[i].transform.localPosition.y,
                    mStars[i].transform.localPosition.z);
            }

            string lvAndName;

            //设置宠物的等级和名称;
            if (!MonsterMgr.IsMaxLevel(pet_ob))
                lvAndName = string.Format(LocalizationMgr.Get("PetStrengthenWnd_1"),
                    PetMgr.GetAwakeColor(pet_ob.GetRank()), pet_ob.GetLevel(), pet_ob.Short());
            else
                lvAndName = string.Format(LocalizationMgr.Get("PetStrengthenWnd_21"),
                    PetMgr.GetAwakeColor(pet_ob.GetRank()), pet_ob.GetLevel(), pet_ob.Short());

            mLevelAndName.text = lvAndName;
            mLevelAndName.gameObject.SetActive(true);

            mIcon.mainTexture = MonsterMgr.GetTexture(classId, pet_ob.GetRank());
        }
    }

    /// <summary>
    /// 刷新宠物属性面板
    /// </summary>
    private void RedrawPetAttributeWnd(Property pet_ob)
    {
        mAddFactorTip.gameObject.SetActive(false);

        if (pet_ob == null)
        {
            mSelectPetTip.gameObject.SetActive(true);
            mPetInfoDetails.SetActive(false);
            return;
        }

        mSelectPetTip.gameObject.SetActive(false);
        mPetInfoDetails.SetActive(true);

        Property strMonster = null;

        if (strClonePet == null)
            strMonster = pet_ob;
        else
            strMonster = strClonePet;

        string colour = string.Empty;

        // 显示升级的经验条
        if (!MonsterMgr.IsMaxLevel(pet_ob))
        {
            mStarTips.gameObject.SetActive(false);
            mExpTips.gameObject.SetActive(true);

            int exp = strMonster.Query<int>("exp");

            int LevelExp = StdMgr.GetPetStdExp(strMonster.GetLevel() + 1, strMonster.GetStar());

            // 等级提升的进度条百分比
            float percent = strMonster.Query<int>("level") - pet_ob.Query<int>("level");
            if (LevelExp > 0)
                percent += exp / (float)LevelExp;

            mExpPercent.text = string.Format("{0:N1}{1}", percent * 100, "%");
            mExpSlider.value = percent;

            colour = "[FFDD8AFF]";
        }
        else // 显示升星的星级
        {
            mStarTips.gameObject.SetActive(true);
            mExpTips.gameObject.SetActive(false);

            //根据是否觉醒设置星级图标的类型;
            string starName = PetMgr.GetStarName(pet_ob.GetRank());

            for (int i = 0; i < mStars.Length; i++)
            {
                if (i < pet_ob.GetStar())
                {
                    mLeftStars[i].spriteName = starName;
                    mLeftStars[i].gameObject.SetActive(true);
                }
                else
                    mLeftStars[i].gameObject.SetActive(false);

                if (i < strMonster.GetStar())
                {
                    mRightStars[i].spriteName = starName;
                    mRightStars[i].gameObject.SetActive(true);
                }
                else
                    mRightStars[i].gameObject.SetActive(false);
            }

            colour = "[FC7E7FFF]";
        }

        // 强化后属性
        mLiftPower.text = string.Format("{0}{1}", colour, strMonster.QueryAttrib("max_hp"));
        mLiftAttack.text = string.Format("{0}{1}", colour, strMonster.QueryAttrib("attack"));
        mLiftDefence.text = string.Format("{0}{1}", colour, strMonster.QueryAttrib("defense"));
        mLiftLevel.text = string.Format("{0}{1}", colour, strMonster.Query<int>("level"));

        // 强化前属性
        mPower.text = pet_ob.QueryAttrib("max_hp").ToString();
        mAttack.text = pet_ob.QueryAttrib("attack").ToString();
        mDefence.text = pet_ob.QueryAttrib("defense").ToString();
        mLevel.text = pet_ob.Query<int>("level").ToString();
    }

    /// <summary>
    /// Determines whether this instance can upgrade skill the specified ob materialList.
    /// </summary>
    /// <returns><c>true</c> if this instance can upgrade skill the specified ob materialList; otherwise, <c>false</c>.</returns>
    /// <param name="ob">Ob.</param>
    /// <param name="materialList">Material list.</param>
    private bool CanUpgradeSkill(Property ob, List<Property> materialList)
    {
        // 没有宠物技能
        if (ob == null)
            return false;

        // 获取当前强化对象技能
        LPCArray skills = ob.Query<LPCArray>("skills");
        if (skills == null || skills.Count == 0)
            return false;

        // 判断当前技能是否已经满级了
        int upgradeTimes = 0;
        foreach(LPCValue mks in skills.Values)
        {
            // 转换数据格式
            LPCArray data = mks.AsArray;

            // 判断技能是否已经达到了等级上限
            if (data[1].AsInt >= SkillMgr.GetSkillMaxLevel(data[0].AsInt))
                continue;

            // 增加可以升级次数
            upgradeTimes++;
        }

        // 不能升级技能
        if (upgradeTimes == 0)
            return false;

        // 遍历可以提升技能等级次数
        foreach (Property materialOb in materialList)
        {
            // 不能升级技能
            if (! MonsterMgr.IsSkillLevelUpMaterial(ob, materialOb))
                continue;

            // 有材料可以提升使魔技能
            return true;
        }

        // 没有技能可以升级
        return false;
    }

    /// <summary>
    /// 判断是否可以升星
    /// </summary>
    private bool CanStarUp(Property ob, List<Property> materialList)
    {
        // 没有宠物技能
        if (ob == null)
            return false;

        Dictionary<int, int> materialMap = new Dictionary<int, int>();
        int star;

        // 遍历可以提升技能等级次数
        foreach (Property materialOb in materialList)
        {
            // 获取材料星级
            star = materialOb.GetStar();

            // 统计各个星级材料数量
            if (!materialMap.ContainsKey(star))
                materialMap.Add(star, 1);
            else
                materialMap[star] += 1;
        }

        // 获取照料消耗
        LPCMapping costMap = PetsmithMgr.GetStarUpMaterialCost(ob);
        foreach(int tStar in costMap.Keys)
        {
            if (!materialMap.ContainsKey(tStar) ||
                materialMap[tStar] != costMap.GetValue<int>(tStar))
                return false;
        }

        // 可以升星
        return true;
    }

    /// <summary>
    /// 刷新消耗按钮
    /// </summary>
    private void RedrawCostBtn()
    {
        // 无要强化宠物
        if (string.IsNullOrEmpty(mStrengthenPetRid))
        {
            mUpgradeCostLab.text = "0";
            mStarLiftGoldLab.text = "0";
            mUpgradeCostIcon.spriteName = "money";
            mStarLiftCostIcon.spriteName = "money";
            mUpgardeLabel.text = LocalizationMgr.Get("PetStrengthenWnd_3");
            mStarBtnAnima.gameObject.SetActive(false);
            mUpStarBtnCover.SetActive(false);
            mUpgradeBtnCover.SetActive(false);

            return;
        }

        Property pet_ob = Rid.FindObjectByRid(mStrengthenPetRid);
        if (pet_ob == null)
            return;

        // 取得材料列表
        List<Property> material_list = new List<Property>();
        foreach (string material_rid in mSelectRidList)
        {
            Property material_ob = Rid.FindObjectByRid(material_rid);
            material_list.Add(material_ob);
        }

        // 取得升级消耗
        LPCMapping costMap = mCurMaterialPageType == MaterialPageType.PROPERTY ? CALC_PET_UPGRADE_ITEM_COST.Call(mMaterialList) :
            CALC_PET_UPGRADE_COST.Call(pet_ob, material_list);

        if (costMap == null)
            return;

        string costField = FieldsMgr.GetFieldInMapping(costMap);

        mUpgradeCostLab.text = costMap[costField].AsString;
        mUpgradeCostIcon.spriteName = FieldsMgr.GetFieldIcon(costField);

        // 是否已满级
        if (MonsterMgr.IsMaxLevel(pet_ob))
        {
            // 开启升星按钮动画
            if (MonsterMgr.IsMaxStar(pet_ob))
            {
                mStarBtnAnima.gameObject.SetActive(false);
                mUpStarBtnCover.SetActive(true);
            }
            else
            {
                // 隐藏mUpStarBtnCover
                mUpStarBtnCover.SetActive(false);

                // 显示光效效果
                if (!mStarBtnAnima.gameObject.activeInHierarchy)
                {
                    mStarBtnAnima.gameObject.SetActive(true);
                    mStarBtnAnima.enabled = true;
                    mStarBtnAnima.ResetToBeginning();
                }
            }

            // 如果可以升星不允许点击升级按钮
            if (CanStarUp(pet_ob, material_list))
            {
                // 屏蔽升级技能
                mUpgradeBtnCover.SetActive(true);
            }
            else
            {
                // 判断是否可以提升技能
                if (CanUpgradeSkill(pet_ob, material_list))
                    mUpgradeBtnCover.SetActive(false);
                else
                    mUpgradeBtnCover.SetActive(true);
            }

            // 获取升星消耗
            CsvRow data = PetsmithMgr.StarUpCsv.FindByKey(pet_ob.GetStar());
            int script_no = data.Query<int>("cost_script");
            if (script_no > 0)
                costMap = (LPCMapping)ScriptMgr.Call(script_no, pet_ob, material_list, data.Query<LPCMapping>("cost_args"));

            costField = FieldsMgr.GetFieldInMapping(costMap);

            mStarLiftGoldLab.text = costMap[costField].AsString;
            mUpgradeCostIcon.spriteName = FieldsMgr.GetFieldIcon(costField);

            // 显示技能提升
            mUpgardeLabel.text = LocalizationMgr.Get("PetStrengthenWnd_15");
            return;
        }

        // 取消mUpgradeBtnCover
        mUpgradeBtnCover.SetActive(false);
        mUpStarBtnCover.SetActive(true);
        mStarBtnAnima.gameObject.SetActive(false);

        // 显示等级提升
        mUpgardeLabel.text = LocalizationMgr.Get("PetStrengthenWnd_3");
    }

    /// <summary>
    ///点击选择宠物
    /// </summary>
    private void ClickSelectPet(GameObject go)
    {
        // 拿到点击的宠物数据;
        CustomPetItemWnd petItem = go.GetComponent<CustomPetItemWnd>();

        Property item_ob = petItem.item_ob;
        if (item_ob == null)
            return;

        string rid = item_ob.GetRid();

        string desc = string.Empty;

        // 没有选择要加强的宠物
        if (string.IsNullOrEmpty(mStrengthenPetRid))
        {
            desc = PET_CAN_STRENGTHEN.Call(item_ob);

            // 检测该魔灵能否作为材料
            if (!string.IsNullOrEmpty(desc))
            {
                // 显示单选提示框
                DialogMgr.ShowSingleBtnDailog(
                    null,
                    desc,
                    string.Empty,
                    string.Empty,
                    true,
                    this.transform
                );

                return;
            }

            mStrengthenPetRid = rid;
            mSelectRidList.Clear();

            Property pet_ob = Rid.FindObjectByRid(mStrengthenPetRid);
            if (!MonsterMgr.IsMaxLevel(pet_ob))
            {
                int LevelExp = StdMgr.GetPetStdExp(pet_ob.GetLevel() + 1, pet_ob.GetStar());

                // 将强化宠物的经验条和状态记录下来
                preUpgradePrecent = pet_ob.Query<int>("exp") / (float)LevelExp;
                preUpgradeLev = pet_ob.GetLevel();
            }

            Redraw();

            RefreshShareBtn();

            return;
        }

        // 取消当前选中要强化的宠物
        if (rid.Equals(mStrengthenPetRid))
        {
            mStrengthenPetRid = string.Empty;

            OnMaterialPage(null);

            RefreshShareBtn();

            return;
        }

        //如果是使用召唤卷升级的话，是不可以点击吞噬使魔的，就只可以点击取消主使魔
        if (mCurMaterialPageType == MaterialPageType.PROPERTY)
            return;

        //取消选中材料宠物
        if (petItem.isSelected)
        {
            if (mSelectRidList.Contains(rid))
                mSelectRidList.Remove(rid);

            Redraw();

            return;
        }

        // 如果是最大等级，需要检测材料数量
        if (MonsterMgr.IsMaxLevel(Rid.FindObjectByRid(mStrengthenPetRid)))
        if (mSelectRidList.Count >= PetsmithMgr.GetMaterialStarPos(mStrengthenPetRid).Count)
            return;

        // 材料已达到最大数量，不处理
        if (mSelectRidList.Count >= mMaterialPets.Length)
            return;

        desc = PET_STRENGTHEN_MATERIAL_STATE_DESC.Call(item_ob);

        // 检测该魔灵能否作为材料
        if (!string.IsNullOrEmpty(desc))
        {
            // 显示单选提示框
            DialogMgr.ShowSingleBtnDailog(
                null,
                desc,
                string.Empty,
                string.Empty,
                true,
                this.transform
            );

            return;
        }

        // 判断该宠物是否穿戴装备
        int count = (item_ob as Container).baggage.GetFreePosCount(ContainerConfig.POS_EQUIP_GROUP);
        if (count != GameSettingMgr.GetSettingInt("max_equip_amount"))
        {
            // 弹出提示框该宠物有穿戴装备
            DialogMgr.ShowDailog(
                new CallBack(OnDailog, rid),
                LocalizationMgr.Get("PetStrengthenWnd_35"),
                LocalizationMgr.Get("PetStrengthenWnd_34"),
                string.Empty,
                string.Empty,
                true,
                this.transform
            );

            return;
        }

        if (!mSelectRidList.Contains(rid))
            mSelectRidList.Add(rid);

        Redraw();
    }

    private void OnDailog(object para, params object[] _params)
    {
        if (!(bool)_params[0])
            return;

        string rid = para as string;

        if (!mSelectRidList.Contains(rid))
            mSelectRidList.Add(rid);

        Redraw();
    }

    /// <summary>
    /// 取消强化宠物
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnClickCancelStrenthen(GameObject go)
    {
        // 如果强化宠物为空，不响应
        if (string.IsNullOrEmpty(mStrengthenPetRid))
            return;

        mStrengthenPetRid = string.Empty;

        OnMaterialPage(null);

        RefreshShareBtn();
    }

    /// <summary>
    /// 取消强化材料
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnClickMaterial(GameObject go)
    {
        if (go.GetComponent<CustomPetItemWnd>() == null)
            return;

        // 数据为空不响应
        if (go.GetComponent<CustomPetItemWnd>().item_ob == null)
            return;

        //获取点击物体的Rid;
        string rid = go.GetComponent<CustomPetItemWnd>().item_ob.GetRid();

        if (mSelectRidList.Contains(rid))
            mSelectRidList.Remove(rid);

        Redraw();
    }

    /// <summary>
    ///长按显示宠物信息
    /// </summary>
    private void OnPressShowPetInfo(GameObject go, bool isPress)
    {
        //玩家正在滑动宠物列表;
        if (mPetListScrollView.isDragging)
            return;

        //手指抬起时
        if (!isPress)
        {
            pressRid = string.Empty;

            return;
        }

        //没有宠物;
        if (go.GetComponent<CustomPetItemWnd>().item_ob == null)
            return;

        pressRid = go.GetComponent<CustomPetItemWnd>().item_ob.GetRid();

        CancelInvoke("ShowPetInfo");

        //0.5秒后显示宠物信息界面;
        Invoke("ShowPetInfo", 0.4f);
    }

    /// <summary>
    ///显示宠物信息
    /// </summary>
    private void ShowPetInfo()
    {
        // 当前处于滑动列表中
        if (mPetListScrollView.isDragging)
            return;

        // 没有选择对象
        if (string.IsNullOrEmpty(pressRid))
            return;

        //获得宠物信息窗口
        GameObject wnd = WindowMgr.GetWindow("PetInfoWnd");

        // 窗口对象不存在则创建一个
        if (wnd == null)
            wnd = WindowMgr.CreateWindow("PetInfoWnd", PetInfoWnd.PrefebResource);

        // 创建窗口失败
        if (wnd == null)
        {
            LogMgr.Trace("打开PetInfoWnd窗口失败。");
            return;
        }

        // 消息窗口
        WindowMgr.ShowWindow(wnd);
        wnd.GetComponent<PetInfoWnd>().Bind(pressRid, ME.user.GetName(), ME.user.GetLevel());

    }

    /// <summary>
    /// 宠物升级消息回调
    /// </summary>
    private void OnPetUpgrade(int eventId, MixedValue para)
    {
        if (para == null)
            return;

        LPCMapping map = para.GetValue<LPCMapping>();

        if (map == null)
            return;

        // 播放升级音效
        GameSoundMgr.PlayGroupSound("upgrade", Game.GetUniqueName("upgrade"));

        float add_factor = map.GetValue<float>("add_factor");

        LPCArray level_up_skills = map.GetValue<LPCArray>("level_up_skills");

        Property pet_ob = Rid.FindObjectByRid(mStrengthenPetRid);
        if (pet_ob == null)
            return;

        // 播放进度条动画
        Coroutine.DispatchService(UpgradeAnimaCoroutine(pet_ob, add_factor, level_up_skills));
    }

    /// <summary>
    /// 宠物升星消息回调
    /// </summary>
    private void OnPetStarup(int eventId, MixedValue para)
    {
        if (para == null)
            return;

        LPCMapping map = para.GetValue<LPCMapping>();

        if (map == null)
            return;

        // 播放升级音效
        GameSoundMgr.PlayGroupSound("starup", Game.GetUniqueName("starup"));

        LPCArray level_up_skills = map.GetValue<LPCArray>("level_up_skills");

        Property pet_ob = Rid.FindObjectByRid(mStrengthenPetRid);
        if (pet_ob == null)
            return;

        Coroutine.DispatchService(SkipStarupAnima(pet_ob, level_up_skills), "SkipStarupAnima");

        // 播放进度条动画
        Coroutine.DispatchService(StarupAnimaCoroutine(pet_ob, level_up_skills), "StarupAnimaCoroutine");
    }

    /// <summary>
    /// 跳过升星动画
    /// </summary>
    private IEnumerator SkipStarupAnima(Property pet_ob, LPCArray level_up_skills)
    {
        while (true)
        {
            if (!ImmediEnd)
            {
                yield return null;

                continue;
            }

            if (Time.realtimeSinceStartup < mLastTime + 0.6f)
            {
                yield return null;

                continue;
            }

            // 结束指定协程
            Coroutine.StopCoroutine("StarupAnimaCoroutine");

            mSelectRidList.Clear();

            RedrawMaterialsPanel();
            RedrawPetsPanel();
            RedrawStrPetInfo(pet_ob);

            for (int i = 0; i < mMaterialAnima.Length; i++)
                mMaterialAnima[i].gameObject.SetActive(false);

            mWhitemask.GetComponent<TweenAlpha>().SetCurrentValueToEnd();

            mStarupEfect.GetComponent<TweenAlpha>().SetCurrentValueToEnd();

            mStarupEffectLb.GetComponent<TweenAlpha>().SetCurrentValueToEnd();

            // 播放强化宠物动画
            mStrPetAnima.ResetToBeginning();

            mStrPetAnima.gameObject.SetActive(false);

            int stars = pet_ob.Query<int>("star");

            // 索引越界
            if (stars < 2)
            {
                // 打印log，上传异常数据
                LogMgr.Error(pet_ob.QueryEntireDbase()._GetDescription(3));

                stars = 2;
            }
            else if (stars > 6)
            {
                // 打印log，上传异常数据
                LogMgr.Error(pet_ob.QueryEntireDbase()._GetDescription(3));

                stars = 6;
            }

            // 播放升星动画
            foreach (Transform star in mStarEffects[stars - 2].transform)
            {
                star.GetComponent<TweenAlpha>().SetCurrentValueToEnd();
                star.GetComponent<TweenScale>().SetCurrentValueToEnd();
            }

            for (int i = 0; i < mStarEffects.Length; i++)
                mStarEffects[i].SetActive(false);

            mStarupEffectLb.SetActive(false);

            RedrawCostBtn();
            RedrawPetAttributeWnd(pet_ob);

            if (!mIsShowDialog)
            {
                // 弹出探索奖励界面
                if (TaskMgr.HasNewBonus)
                {
                    LPCMapping bonus = TaskMgr.ReceiveCacheBonus();

                    if (bonus.GetValue<int>("is_first_receive") == 1)
                    {
                        GameObject wnd = WindowMgr.OpenWnd("ResearchBonusInfoWnd");
                        wnd.GetComponent<ResearchBonusInfoWnd>().BindData(bonus.GetValue<LPCArray>("bonus_data")[0].AsMapping);
                    }
                }

                for (int i = 0; i < level_up_skills.Count; i++)
                {
                    Dictionary<string, object> args = new Dictionary<string, object>()
                    {
                        { "skill_id", level_up_skills[i].AsMapping.GetValue<int>("skill_id") },
                        { "level", level_up_skills[i].AsMapping.GetValue<int>("level") },
                    };

                    WindowTipsMgr.AddWindow("SkillLevelUpWnd", args, new CallBack(OnShowFinishTipsWndowCb));
                }

                // 关闭界面上的遮罩
                if (level_up_skills.Count < 1)
                    mWaitCover.SetActive(false);
            }
            else
            {
                // 关闭界面上的遮罩
                mWaitCover.SetActive(false);
            }

            // 刷新分享按钮
            RefreshShareBtn();

            ImmediEnd = false;

            mIsShowDialog = false;

            mLastTime = 0f;

            // 结束协程
            Coroutine.StopCoroutine("SkipStarupAnima");
            Coroutine.StopCoroutine("StarupAnimaCoroutine");

            yield break;
        }
    }

    /// <summary>
    /// 窗口全部显示完成回调
    /// </summary>
    void OnShowFinishTipsWndowCb(object para, params object[] param)
    {
        // 关闭界面上的遮罩
        mWaitCover.SetActive(false);
    }

    /// <summary>
    /// 升星动画
    /// </summary>
    /// <returns>The anima coroutine.</returns>
    private IEnumerator StarupAnimaCoroutine(Property pet_ob, LPCArray level_up_skills)
    {
        yield return new WaitForSeconds(0.3f);

        mSelectRidList.Clear();

        RedrawMaterialsPanel();
        RedrawPetsPanel();
        RedrawStrPetInfo(pet_ob);

        for (int i = 0; i < mMaterialAnima.Length; i++)
            mMaterialAnima[i].gameObject.SetActive(false);

        mWhitemask.GetComponent<TweenAlpha>().enabled = true;
        mWhitemask.GetComponent<TweenAlpha>().ResetToBeginning();

        yield return new WaitForSeconds(0.1f);

        // 播放强化宠物动画
        mStrPetAnima.gameObject.SetActive(true);
        mStrPetAnima.enabled = true;
        mStrPetAnima.ResetToBeginning();

        yield return new WaitForSeconds(0.2f);

        int stars = pet_ob.Query<int>("star");

        TweenAlpha tweenAlpha = mStarupEfect.GetComponent<TweenAlpha>();

        tweenAlpha.ResetToBeginning();

        for (int i = 0; i < mStarEffects.Length; i++)
        {
            if (i == stars - 2)
                mStarEffects[i].SetActive(true);
            else
                mStarEffects[i].SetActive(false);
        }

        // 播放升星动画
        string starName = PetMgr.GetStarName(pet_ob.GetRank());
        foreach (Transform star in mStarEffects[stars - 2].transform)
        {
            star.GetComponent<TweenAlpha>().enabled = true;
            star.GetComponent<TweenAlpha>().ResetToBeginning();

            star.GetComponent<UISprite>().spriteName = starName;

            star.GetComponent<TweenScale>().enabled = true;
            star.GetComponent<TweenScale>().ResetToBeginning();
        }

        mStarupEffectLb.SetActive(true);
        mStarupEffectLb.GetComponent<TweenAlpha>().enabled = true;
        mStarupEffectLb.GetComponent<TweenAlpha>().ResetToBeginning();

        mStarupEffectLb.GetComponent<TweenScale>().enabled = true;
        mStarupEffectLb.GetComponent<TweenScale>().ResetToBeginning();

        mStarupEffectLb.GetComponent<TweenScale>().AddOnFinished(OnScaleFinish);

        yield return new WaitForSeconds(2.0f);

        tweenAlpha.enabled = true;

        RedrawCostBtn();
        RedrawPetAttributeWnd(pet_ob);

        yield return new WaitForSeconds(1.0f);

        // 弹出探索奖励界面
        if (TaskMgr.HasNewBonus)
        {
            LPCMapping bonus = TaskMgr.ReceiveCacheBonus ();

            if (bonus.GetValue<int>("is_first_receive") == 1)
            {
                GameObject wnd = WindowMgr.OpenWnd ("ResearchBonusInfoWnd");
                wnd.GetComponent<ResearchBonusInfoWnd> ().BindData (bonus.GetValue<LPCArray>("bonus_data")[0].AsMapping);
            }
        }

        for (int i = 0; i < level_up_skills.Count; i++)
        {
            Dictionary<string, object> args = new Dictionary<string, object>()
            {
                {"skill_id", level_up_skills[i].AsMapping.GetValue<int>("skill_id")},
                {"level", level_up_skills[i].AsMapping.GetValue<int>("level")},
            };

            WindowTipsMgr.AddWindow("SkillLevelUpWnd", args, new CallBack(OnShowFinishTipsWndowCb));
        }

        // 关闭界面上的遮罩
        if (level_up_skills.Count < 1)
            mWaitCover.SetActive(false);

        mIsShowDialog = false;

        // 结束协程
        Coroutine.StopCoroutine("SkipStarupAnima");
        Coroutine.StopCoroutine("StarupAnimaCoroutine");
    }

    void OnScaleFinish()
    {
        RefreshShareBtn();
    }

    /// <summary>
    /// 升级动画
    /// </summary>
    /// <returns>The anima coroutine.</returns>
    /// <param name="pet_ob">Pet ob.</param>
    /// <param name="add_exp">Add exp.</param>
    private IEnumerator UpgradeAnimaCoroutine(Property pet_ob, float add_factor, LPCArray level_up_skills)
    {
        yield return new  WaitForSeconds(0.3f);

        for (int i = 0; i < mMaterialAnima.Length; i++)
            mMaterialAnima[i].gameObject.SetActive(false);

        mSelectRidList.Clear();

        RedrawMaterialsPanel();
        RedrawPetsPanel();
        RedrawStrPetInfo(pet_ob);
        if (mCurMaterialPageType == MaterialPageType.PROPERTY)
        {
            bool isRefreshReel = false;

            //单召唤卷轴消耗后小于最低要求的数目需要跳转到使魔材料分页
            int ownReelCounts = UserMgr.GetAttribItemAmount(ME.user, mMaterialMap["class_id"].AsInt);
            int limitNum = GameSettingMgr.GetSetting<int>("pet_upgrade_count_limit");
            if (ownReelCounts < limitNum)
            {
                OnMaterialPage(null);
                isRefreshReel = true;
            }

            if (!string.IsNullOrEmpty(mStrengthenPetRid))
            {
                //如果强化的宠物满级了，需要跳转到使魔材料分页
                Property strenPet = Rid.FindObjectByRid(mStrengthenPetRid);
                if (strenPet != null)
                {
                    if (MonsterMgr.IsMaxLevel(strenPet))
                    {
                        OnMaterialPage(null);
                        isRefreshReel = true;
                    }
                }
            }

            if (!isRefreshReel)
            {
                mMaterialMap["amount"] = LPCValue.Create(0);
                RedrawReel();
            }
        }

        mWhitemask.GetComponent<TweenAlpha>().enabled = true;
        mWhitemask.GetComponent<TweenAlpha>().ResetToBeginning();

        yield return new WaitForSeconds(1.0f);

        // 播放强化宠物动画
        mStrPetAnima.gameObject.SetActive(true);
        mStrPetAnima.enabled = true;
        mStrPetAnima.ResetToBeginning();

        // 重置立即结束标志
        ImmediEnd = false;
        float current_precent = 0f;

        // 满级宠物升级不需要播放动画
        if (preUpgradeLev < MonsterMgr.GetMaxLevel(pet_ob))
        {
            // 显示强化因子
            if (add_factor > 1.0f)
            {
                mAddFactorTip.gameObject.SetActive(true);
                mAddFactorTip.text = string.Format(LocalizationMgr.Get("PetStrengthenWnd_19"), add_factor);
            }

            if (pet_ob.GetLevel() < MonsterMgr.GetMaxLevel(pet_ob))
            {
                // 取得升级后宠物进度百分比
                int LevelExp = StdMgr.GetPetStdExp(pet_ob.GetLevel() + 1, pet_ob.GetStar());
                current_precent = pet_ob.Query<int>("exp") / (float)LevelExp;
            }

            // 取得总的经验条加成
            float sum_precent = (pet_ob.GetLevel() - preUpgradeLev) + (current_precent - preUpgradePrecent);

            float slider_value = preUpgradePrecent;
            float acceSpeed = -2f * sum_precent / Mathf.Pow(expSliderPlayTime, 2.0f);
            float curr_speed = 2f * sum_precent / expSliderPlayTime;
            float precent = 0f;

            while (sum_precent > 0f && !(precent < 0f))
            {
                if (ImmediEnd)
                {
                    if (sum_precent > 1f)
                        sum_precent -= Mathf.Floor(sum_precent);

                    acceSpeed = 0f;

                    curr_speed = 1f;
                }

                precent = curr_speed * Time.unscaledDeltaTime + acceSpeed * Mathf.Pow(Time.unscaledDeltaTime, 2.0f) / 2f;

                if (precent < 0f)
                {
                    break;
                }

                slider_value += precent;

                curr_speed += Time.unscaledDeltaTime * acceSpeed;

                sum_precent -= precent;

                if (slider_value > 0.999f)
                    slider_value = 0f;

                // 经验条赋值
                mExpSlider.value = slider_value;
                mExpPercent.text = string.Format("{0:N1}{1}", slider_value * 100, "%");

                yield return null;
            }
        }

        RedrawCostBtn();
        RedrawPetAttributeWnd(pet_ob);

        // 将强化宠物的经验条和状态记录下来
        preUpgradePrecent = current_precent;
        preUpgradeLev = pet_ob.GetLevel();

        if (!ImmediEnd)
            yield return new WaitForSeconds(1.0f);

        // 显示技能等级升级
        for (int i = 0; i < level_up_skills.Count; i++)
        {
            Dictionary<string, object> args = new Dictionary<string, object>()
            {
                {"skill_id", level_up_skills[i].AsMapping.GetValue<int>("skill_id")},
                {"level", level_up_skills[i].AsMapping.GetValue<int>("level")},
            };

            WindowTipsMgr.AddWindow("SkillLevelUpWnd", args, new CallBack(OnShowFinishTipsWndowCb));
        }

        // 关闭遮罩
        if (level_up_skills.Count < 1)
            mWaitCover.SetActive(false);

        ImmediEnd = false;

        EventMgr.FireEvent(EventMgrEventType.EVENT_UPGREDE_ANIMATION_FINISH, null);

        yield break;
    }

    /// <summary>
    ///等级提升按钮点击事件
    /// </summary>
    private void OnClickUpgradeBtn(GameObject go)
    {
        // 如果当前不允许点击
        if (mUpgradeBtnCover.activeInHierarchy)
            return;

        // 选择要强化的宠物
        if (string.IsNullOrEmpty(mStrengthenPetRid))
        {
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("PetStrengthenWnd_12"),
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
            return;
        }

        Property pet_ob = Rid.FindObjectByRid(mStrengthenPetRid);
        if (pet_ob == null)
            return;

        //吞噬使魔为材料
        if (mCurMaterialPageType == MaterialPageType.Pet)
        {
            // 选择材料宠物
            if (mSelectRidList == null || mSelectRidList.Count == 0)
            {
                DialogMgr.ShowSingleBtnDailog(
                    null,
                    LocalizationMgr.Get("PetStrengthenWnd_13"),
                    string.Empty,
                    string.Empty,
                    true,
                    this.transform
                );
                return;
            }

            List<Property> material_list = new List<Property>();
            foreach (string rid in mSelectRidList)
            {
                Property material_ob = Rid.FindObjectByRid(rid);
                if (material_ob != null)
                    material_list.Add(material_ob);
            }

            string desc = CHECK_PET_CAN_UPGRADE.Call(pet_ob, material_list);
            if (!string.IsNullOrEmpty(desc))
            {
                DialogMgr.ShowSingleBtnDailog(
                    null,
                    desc,
                    string.Empty,
                    string.Empty,
                    true,
                    this.transform
                );
                return;
            }

            LPCMapping cost_map = CALC_PET_UPGRADE_COST.Call(pet_ob, material_list);
            if (!PetsmithMgr.CheckMoneyEnough(cost_map))
                return;

            // 如果满级需要检测有没有技能材料
            if (MonsterMgr.IsMaxLevel(pet_ob))
            {
                // 如果没有技能材料
                if (!PetsmithMgr.HasLevelUpMaterial(pet_ob, material_list))
                {
                    DialogMgr.ShowSingleBtnDailog(
                        null,
                        string.Format(LocalizationMgr.Get("PetStrengthenWnd_25"), pet_ob.Short()),
                        string.Empty,
                        string.Empty,
                        true,
                        this.transform
                    );
                    return;
                }

                // 如果宠物技能全部都满级了
                if (SkillMgr.CheckSkillsFullLevel(pet_ob))
                {
                    // 给出提示
                    DialogMgr.ShowDailog(
                        new CallBack(DoStrengthenPet, "upgrade"),
                        LocalizationMgr.Get("PetStrengthenWnd_24"),
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        true,
                        this.transform
                    );
                    return;
                }
            }
        }
        else
        {
            if (mMaterialMap.Count <= 0)
                return;

            if (mMaterialMap["amount"].AsInt <= 0)
            {
                //请先放入卷轴材料
                DialogMgr.ShowSingleBtnDailog(
                    null,
                    LocalizationMgr.Get("PetStrengthenWnd_43"),
                    string.Empty,
                    string.Empty,
                    true,
                    this.transform
                );
                return;
            }

            //消耗的钱是否足够
            LPCMapping cost_map = CALC_PET_UPGRADE_ITEM_COST.Call(mMaterialList);
            if (!PetsmithMgr.CheckMoneyEnough(cost_map))
                return;
        }

        // 指引不弹提示框
        if (GuideMgr.IsGuiding())
        {
            DoStrengthenPet("upgrade", null);
            return;
        }

        if (mCurMaterialPageType == MaterialPageType.Pet)
        {
            // 给出提示
            DialogMgr.ShowDailog(
                new CallBack(DoStrengthenPet, "upgrade"),
                LocalizationMgr.Get("PetStrengthenWnd_16"),
                string.Empty,
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
        }
        else if (mCurMaterialPageType == MaterialPageType.PROPERTY)
        {
            if (mMaterialMap.Count <= 0)
                return;

            // 给出提示:需要消耗 {0} 张「{1}」进行升级
            DialogMgr.ShowDailog(
                new CallBack(DoStrengthenPet, "upgrade"),
                string.Format(LocalizationMgr.Get("PetStrengthenWnd_45"), mMaterialMap["amount"].AsInt, ItemMgr.GetName(mMaterialMap["class_id"].AsInt)),
                string.Empty,
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
        }

    }

    /// <summary>
    ///星级提升按钮点击事件
    /// </summary>
    private void OnClickStarLiftBtn(GameObject go)
    {
        // 如果当前不允许点击
        if (mUpStarBtnCover.activeInHierarchy)
            return;

        // 选择要强化的宠物
        if (string.IsNullOrEmpty(mStrengthenPetRid))
        {
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("PetStrengthenWnd_12"),
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
            return;
        }

        Property pet_ob = Rid.FindObjectByRid(mStrengthenPetRid);
        if (pet_ob == null)
            return;

        // 宠物还没满级
        if (!MonsterMgr.IsMaxLevel(pet_ob))
        {
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("PetStrengthenWnd_14"),
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
            return;
        }

        // 宠物已达最大星级
        if (MonsterMgr.IsMaxStar(pet_ob))
        {
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("PetStrengthenWnd_30"),
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
            return;
        }

        // 选择材料宠物
        if (mSelectRidList == null || mSelectRidList.Count == 0)
        {
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("PetStrengthenWnd_13"),
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
            return;
        }

        List<Property> material_list = new List<Property>();
        foreach (string rid in mSelectRidList)
        {
            Property material_ob = Rid.FindObjectByRid(rid);
            if (material_ob != null)
                material_list.Add(material_ob);
        }

        // 检测消耗材料是否足够
        CsvRow data = PetsmithMgr.StarUpCsv.FindByKey(pet_ob.GetStar());

        LPCMapping cost_args = data.Query<LPCMapping>("material_cost_args");

        string cost_str = string.Empty;
        foreach (int star in cost_args.Keys)
        {
            cost_str += string.Format(LocalizationMgr.Get("PetStrengthenWnd_23"), cost_args.GetValue<int>(star), star);
        }
        string matNotEnoughDesc = string.Format(LocalizationMgr.Get("PetStrengthenWnd_22"), cost_str, pet_ob.GetStar() + 1);

        List<int> star_cost = PetsmithMgr.GetMaterialStarPos(mStrengthenPetRid);
        if (material_list.Count != star_cost.Count)
        {
            DialogMgr.ShowSingleBtnDailog(
                null,
                matNotEnoughDesc,
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
            return;
        }

        foreach (Property item in material_list)
        {
            if (!star_cost.Contains(item.GetStar()))
            {
                DialogMgr.ShowSingleBtnDailog(
                    null,
                    matNotEnoughDesc,
                    string.Empty,
                    string.Empty,
                    true,
                    this.transform
                );
                return;
            }

            star_cost.Remove(item.GetStar());
        }

        // 检测能否强化
        int checkScriptNo = data.Query<int>("check_script");
        if (checkScriptNo > 0)
        {
            string desc = (string)ScriptMgr.Call(checkScriptNo, pet_ob, material_list);
            if (!string.IsNullOrEmpty(desc))
            {
                DialogMgr.ShowSingleBtnDailog(
                    null,
                    desc,
                    string.Empty,
                    string.Empty,
                    true,
                    this.transform
                );
                return;
            }
        }

        LPCMapping costMoneymap = new LPCMapping();

        int script_no = data.Query<int>("cost_script");

        if (script_no > 0)
            costMoneymap = (LPCMapping)ScriptMgr.Call(script_no, pet_ob, material_list, data.Query<LPCMapping>("cost_args"));

        if (!PetsmithMgr.CheckMoneyEnough(costMoneymap))
            return;

        // 如果有技能材料
        if (PetsmithMgr.HasLevelUpMaterial(pet_ob, material_list))
        {
            DialogMgr.ShowDailog(
                new CallBack(DoStrengthenPet, "starup"),
                string.Format(LocalizationMgr.Get("PetStrengthenWnd_26"), pet_ob.GetStar() + 1),
                string.Empty,
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
            return;
        }

        // 给出提示
        DialogMgr.ShowDailog(
            new CallBack(DoStrengthenPet, "starup"),
            string.Format(LocalizationMgr.Get("PetStrengthenWnd_27"), LocalizationMgr.Get(pet_ob.Query<string>("name")), pet_ob.GetStar() + 1),
            string.Empty,
            string.Empty,
            string.Empty,
            true,
            this.transform
        );
    }

    /// <summary>
    /// 帮助按钮点击
    /// </summary>
    void OnLevelUpHelpBtn(GameObject go)
    {
        // 获取历史排名窗口
        GameObject wnd = WindowMgr.GetWindow(HelpWnd.WndType);

        if (wnd == null)
            wnd = WindowMgr.CreateWindow(HelpWnd.WndType, HelpWnd.PrefebResource);

        if (wnd == null)
        {
            LogMgr.Trace("HelpWnd窗口创建失败");
            return;
        }

        WindowMgr.ShowWindow(wnd);

        wnd.GetComponent<HelpWnd>().Bind(HelpConst.PET_STRENGTH_ID);
    }

    /// <summary>
    /// 使魔材料页签
    /// </summary>
    /// <param name="go"></param>
    private void OnMaterialPage(GameObject go)
    {
        mCurMaterialPageType = MaterialPageType.Pet;
        mMaterialBtn.GetComponent<PageCtrl>().SetSelected(true);
        mReelBtn.GetComponent<PageCtrl>().SetSelected(false);

        mMaterialsPart.SetActive(true);
        mReelPart.SetActive(false);

        mUpgradeBtn.transform.localPosition = new Vector3(-378f, mUpgradeBtn.transform.localPosition.y, 0f);
        mStarLiftBtn.SetActive(true);

        //清理
        mSelectRidList.Clear();
        Redraw();

        //初始化滑动条
        if (mMaterialMap.Count <= 0)
            return;

        mMaterialMap["amount"] = LPCValue.Create(0);
        RedrawReel();
    }

    /// <summary>
    /// 召唤卷升级页签
    /// </summary>
    /// <param name="go"></param>
    private void OnReelPage(GameObject go)
    {
        if (ME.user == null)
            return;

        if (mMaterialMap.Count <= 0)
            return;

        int classId = mMaterialMap["class_id"].AsInt;

        //您需要达到{0}级才可以使用该功能。
        if (ME.user.GetLevel() < GameSettingMgr.GetSetting<int>("pet_upgrade_use_item_lv_limit"))
        {
            DialogMgr.Notify(string.Format(LocalizationMgr.Get("PetStrengthenWnd_37"), GameSettingMgr.GetSetting<int>("pet_upgrade_use_item_lv_limit")));
            return;
        }

        //要拥有「{0}」{1}个以上才可以使用该功能。
        int ownNum = UserMgr.GetAttribItemAmount(ME.user, classId);
        int limitNum = GameSettingMgr.GetSetting<int>("pet_upgrade_count_limit");
        if (ownNum < limitNum)
        {
            DialogMgr.Notify(string.Format(LocalizationMgr.Get("PetStrengthenWnd_38"), ItemMgr.GetName(classId), limitNum));
            return;
        }

        //您需要选择一个未满级的使魔才可以使用该功能。
        if (string.IsNullOrEmpty(mStrengthenPetRid))
        {
            DialogMgr.Notify(LocalizationMgr.Get("PetStrengthenWnd_39"));
            return;
        }

        Property petProp = Rid.FindObjectByRid(mStrengthenPetRid);
        if (petProp != null)
        {
            if (MonsterMgr.IsMaxLevel(petProp))
            {
                //该使魔已满级，无法使用该功能。
                DialogMgr.Notify(LocalizationMgr.Get("PetStrengthenWnd_40"));
                return;
            }
        }
        else
        {
            //该使魔不存在
            DialogMgr.Notify(LocalizationMgr.Get("PetStrengthenWnd_41"));
            return;
        }

        mCurMaterialPageType = MaterialPageType.PROPERTY;
        mMaterialBtn.GetComponent<PageCtrl>().SetSelected(false);
        mReelBtn.GetComponent<PageCtrl>().SetSelected(true);

        mMaterialsPart.SetActive(false);
        mReelPart.SetActive(true);

        //按钮
        mUpgradeBtn.transform.localPosition = new Vector3(-246f, mUpgradeBtn.transform.localPosition.y, 0f);
        mStarLiftBtn.SetActive(false);

        //清理
        mSelectRidList.Clear();
        Redraw();
    }

    /// <summary>
    /// 覆盖被点击
    /// </summary>
    void OnWaitCoverClick(GameObject go)
    {
        if (ImmediEnd)
            return;

        ImmediEnd = true;

        mLastTime = Time.realtimeSinceStartup;
    }

    /// <summary>
    /// 确定强化
    /// </summary>
    public void DoStrengthenPet(object para, object[] expara)
    {
        if (expara != null)
        {
            bool isOk = (bool)expara[0];

            EventMgr.FireEvent(EventMgrEventType.EVENT_STRENGTHEN_PET_DIALOG_CLICK, MixedValue.NewMixedValue<bool>(isOk));

            if (!isOk)
                return;
        }

        LPCArray material_list = new LPCArray();
        if (mCurMaterialPageType == MaterialPageType.Pet)
        {
            foreach (string rid in mSelectRidList)
                material_list.Add(rid);
        }
        else if (mCurMaterialPageType == MaterialPageType.PROPERTY)
        {
            material_list = mMaterialList;
        }

        LPCMapping arg = new LPCMapping();
        arg.Add("type", mCurMaterialPageType == MaterialPageType.Pet ? (int)MaterialPageType.Pet : (int)MaterialPageType.PROPERTY);
        arg.Add("rid", mStrengthenPetRid);
        arg.Add("material_list", material_list);

        bool actionResult = PetsmithMgr.DoAction(ME.user, para as string, arg);

        if (!actionResult)
            return;

        mUpgradeBtnCover.SetActive(true);
        mUpStarBtnCover.SetActive(true);
        mWaitCover.SetActive(true);

        // 播放光效
        for (int i = 0; i < mMaterialPets.Length; i++)
        {
            Property pet_ob = mMaterialPets[i].item_ob;

            if (pet_ob == null)
                continue;

            if (i >= mMaterialAnima.Length)
                continue;

            mMaterialAnima[i].gameObject.SetActive(true);
            mMaterialAnima[i].enabled = true;
            mMaterialAnima[i].ResetToBeginning();
        }
    }

    /// <summary>
    /// 使魔强化按钮点击事件
    /// </summary>
    private void OnClickPetStrengthen(GameObject go)
    {
        // 隐藏mResearchRewardWnd
        mResearchRewardWnd.SetActive (false);

        // 显示mPetStrWnd面板
        bool isActive = mPetStrWnd.activeSelf;
        mPetStrWnd.SetActive (true);

        // 重置ScrollVivew位置
        if (!isActive)
            ReseetWnd();
    }

    /// <summary>
    ///强化奖励按钮点击事件
    /// </summary>
    private void OnClickStrengthenReward(GameObject go)
    {
        // 隐藏mPetStrWnd
        mPetStrWnd.SetActive (false);

        // 显示mResearchRewardWnd
        bool isActive = mResearchRewardWnd.activeSelf;
        mResearchRewardWnd.SetActive (true);

        // 重置mRewardScrollVivew位置
        if (! isActive)
            mRewardScrollVivew.ResetPosition();
    }

    #endregion

    #region 外部函数
    /// <summary>
    /// 召唤卷升级slider
    /// </summary>
    public void OnReelSlider()
    {
        if (ME.user == null)
            return;

        if (string.IsNullOrEmpty(mStrengthenPetRid))
            return;

        Property strengPet = Rid.FindObjectByRid(mStrengthenPetRid);
        if (strengPet == null)
            return;

        if (mMaterialMap.Count <= 0)
            return;

        int maxExp = StdMgr.GetPetExpsToMaxLV(strengPet.Query<int>("level") + 1, strengPet.GetStar());
        int expSpan = maxExp - strengPet.Query<int>("exp");
        int maxReelCounts = Mathf.CeilToInt(1f * expSpan / mPerItemExp);
        int ownReelCounts = UserMgr.GetAttribItemAmount(ME.user, mMaterialMap["class_id"].AsInt);
        int curMaxReelCounts = ownReelCounts > maxReelCounts ? maxReelCounts : ownReelCounts;
        int curReelCount  = Mathf.FloorToInt(mReelSlider.value * curMaxReelCounts);
        mMaterialMap["amount"] = LPCValue.Create(curReelCount);

        mReelDesLab.text = string.Format(LocalizationMgr.Get("PetStrengthenWnd_42"), curReelCount);
        mReelNumLab.text = (ownReelCounts - curReelCount).ToString();

        //改变数目颜色
        if (curReelCount > 0)
            mReelNumLab.color = ColorConfig.ParseToColor("ff7f7f");
        else
            mReelNumLab.color = ColorConfig.ParseToColor("e4e4e4");

        // 获取临时克隆宠物
        if (strClonePet != null)
            strClonePet.Destroy();
        strClonePet = GetStrClonePet(curReelCount * mPerItemExp);

        RedrawPetAttributeWnd(strengPet);
        RedrawCostBtn();

        //LogMgr.Trace("==============" + mReelSlider.value);
    }

    /// <summary>
    /// 指引选择强化的宠物
    /// </summary>
    public void GuideSelectStrengthenPet(string itemName)
    {
        CustomPetItemWnd petItem = null;
        if (! mPosObMap.TryGetValue(itemName, out petItem) ||
            petItem == null)
            return;

        // 拿到点击的宠物数据;
        Property item_ob = petItem.item_ob;
        if (item_ob == null)
            return;

        string rid = item_ob.GetRid();

        // 同时点击多次不做处理
        if (rid.Equals(mStrengthenPetRid))
            return;

        ClickSelectPet(petItem.gameObject);
    }

    /// <summary>
    /// 指引点击升级按钮
    /// </summary>
    public void GuideOnClickUpgradeBtn()
    {
        OnClickUpgradeBtn(mUpgradeBtn);
    }

    #endregion
}
