/// <summary>
/// ShowPetsWnd.cs
/// Created by lic 2016-6-20
/// 宠物contain界面
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LPC;

public class ShowPetsWnd : WindowBase<ShowPetsWnd>
{
    #region 成员变量

    public Vector2 mInitPos = new Vector2(-171, 172);
    public Vector2 mItemSpace = new Vector2(6, 6);
    public Vector2 mItemSize = new Vector2(64, 64);

    public GameObject Container;
    public UIScrollView ScrollView;
    public UIWrapContent petcontent;

    public GameObject[] mSortBtn;
    public UISprite mShowAllPet;

    public GameObject mPetToolTip;

    //本地化label文字
    public UILabel mSortLevelLb;
    public UILabel mSortStarLb;
    public UILabel mSortAttributeLb;
    public UILabel mSortLaterstLb;
    public UILabel mAllPetsLb;

    public GameObject mPetItem;

    public GameObject mManualNewTips;

    #endregion

    #region 私有变量

    // 每5个item为一行
    const int mColumnNum = 5;

    // 此处是指预先创建多少行
    const int mRowNum = 6;

    private List<Property> petData = new List<Property>();

    // 包裹格子数(注意：包裹中宠物数量可能大于包裹容量，此处取两者最大值)
    private int containerSize = 0;

    private string selectedRid = string.Empty;

    // 当前选择的排序方式
    int mSelectSort = 0;

    // 当前显示数据的index与实际数据的对应关系
    private Dictionary<int, int> indexMap = new Dictionary<int, int>();

    // name与OB映射
    private Dictionary<string, GameObject> mPosObMap = new Dictionary<string, GameObject>();

    // 起始位置
    private Dictionary<GameObject,Vector3> rePosition = new Dictionary<GameObject,Vector3>();

    #endregion

    #region 内部函数

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        // 关注MSG_LOCK_PET消息
        MsgMgr.RegisterDoneHook("MSG_LOCK_PET", "ShowPetsWnd_LockPet", OnMsgLockPet);
        MsgMgr.RegisterDoneHook("MSG_RECEIVE_MANUAL_BONUS", "ShowPetsWnd", OnMsgReceiveManulaBonus);

        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 注册玩家装备道具事件
        ME.user.baggage.eventCarryChange += BaggageChange;

        // 关注字段变化
        ME.user.dbase.RegisterTriggerField("ShowPetsWnd_SharePet", new string[] { "share_pet" }, new CallBack(OnFiledsChange));
        ME.user.dbase.RegisterTriggerField("ShowPetsWnd_manual_data", new string[] { "manual_data" }, new CallBack(OnManualDataFiledsChange));
        ME.user.dbase.RegisterTriggerField("ShowPetsWnd_Upgrade_Baggage", new string[] { "container_size" }, new CallBack(OnContainerSizeChange));
    }

    /// <summary>
    /// MSG_RECEIVE_MANUAL_BONUS 消息回调
    /// </summary>
    void OnMsgReceiveManulaBonus(string cmd, LPCValue para)
    {
        // 刷新图鉴按钮“新”提示
        RefreshManulaNewTips();
    }

    /// <summary>
    /// 消息回调
    /// </summary>
    void OnMsgLockPet(string cmd, LPCValue para)
    {
        // 刷新界面
        Redraw();
    }

    /// <summary>
    /// 字段变化事件
    /// </summary>
    void OnFiledsChange(object para, params object[] param)
    {
        // 刷新界面
        Redraw();
    }

    void OnManualDataFiledsChange(object para, params object[] param)
    {
        // 刷新图鉴按钮“新”提示
        RefreshManulaNewTips();
    }

    void OnContainerSizeChange(object para, params object[] param)
    {
        // 刷新界面
        Redraw();
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        mManualNewTips.GetComponent<UISpriteAnimation>().namePrefix = ConfigMgr.IsCN ? "cnew" : "new";

        // 本地化文字
        mSortLevelLb.text = LocalizationMgr.Get("ShowPetsWnd_1");
        mSortStarLb.text = LocalizationMgr.Get("ShowPetsWnd_2");
        mSortAttributeLb.text = LocalizationMgr.Get("ShowPetsWnd_3");
        mSortLaterstLb.text = LocalizationMgr.Get("ShowPetsWnd_4");
        mAllPetsLb.text = LocalizationMgr.Get("ShowPetsWnd_5");

        // 设置初始排序方式（最开始默认为等级排序的方式）
        mSelectSort = BaggageMgr.GetMonsterSortType() >= mSortBtn.Length ?
            MonsterConst.SORT_BY_LEVEL : BaggageMgr.GetMonsterSortType();

        mSortBtn[mSelectSort].GetComponent<UIToggle>().Set(true);

        mPetItem.SetActive (false);
    }

    void RegisterWndEvent()
    {
        petcontent.onInitializeItem = OnUpdateItem;
        UIEventListener.Get(mShowAllPet.gameObject).onClick = OnShowAllPetsBtn;

        foreach (GameObject btn in mSortBtn)
            UIEventListener.Get(btn).onClick = OnSortBtn;
    }

    // Use this for initialization
    void Awake()
    {
        // 注册事件
        RegisterWndEvent();

        // 初始化窗口
        InitWnd();

        // 创建格子
        CreatePos();
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
        ScrollView.ResetPosition();

        // 重新初始化indexMap
        if (indexMap != null)
        {
            indexMap.Clear();
            for (int i = 0; i < mRowNum; i++)
                indexMap.Add(i, -i);
        }
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    void OnEnable()
    {
        // 注册事件
        RegisterEvent();

        // 重置当前选择rid
        selectedRid = string.Empty;

        // 重置ScrollView面板位置
        ResetScrollView();

        Redraw();

        // 刷新新使魔图鉴提示
        RefreshManulaNewTips();

        // 通关兰达平原普通所有副本
        if (! GuideMgr.IsGuided(4))
        {
            float rgb = 125f / 255f;
            mShowAllPet.color = new Color(rgb, rgb, rgb, rgb);
        }
        else
        {
            mShowAllPet.color = new Color(1, 1, 1, 1);
        }
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    void OnDisable()
    {
        // 取消消息关注
        MsgMgr.RemoveDoneHook("MSG_LOCK_PET", "ShowPetsWnd_LockPet");
        MsgMgr.RemoveDoneHook("MSG_RECEIVE_MANUAL_BONUS", "ShowPetsWnd");

        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 取消注册玩家装备道具事件
        ME.user.baggage.eventCarryChange -= BaggageChange;
        ME.user.dbase.RemoveTriggerField("ShowPetsWnd_SharePet");
        ME.user.dbase.RemoveTriggerField("ShowPetsWnd_Upgrade_Baggage");
        ME.user.dbase.RemoveTriggerField("ShowPetsWnd_manual_data");
    }

    /// <summary>
    /// 创建宠物格子
    /// </summary>
    private void CreatePos()
    {
        // 生成格子，只生成这么多格子，动态复用
        for (int i = 0; i < mRowNum; i++)
        {
            GameObject rowItemOb = new GameObject();
            rowItemOb.name = string.Format("item_{0}", i);
            rowItemOb.transform.parent = Container.transform;
            rowItemOb.transform.localPosition = new Vector3(mInitPos.x, mInitPos.y - i * 115, 0);
            rowItemOb.transform.localScale = Vector3.one;

            rePosition.Add(rowItemOb, rowItemOb.transform.localPosition);

            for(int j = 0; j < mColumnNum; j++)
            {
                GameObject posWnd = Instantiate (mPetItem) as GameObject;
                posWnd.transform.parent = rowItemOb.transform;
                posWnd.name = string.Format("baggage_pet_item_{0}_{1}", i, j);
                posWnd.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                posWnd.transform.localPosition = new Vector3((mItemSize.x + mItemSpace.x) * j, 0f, 0f);

                mPosObMap.Add (string.Format("baggage_pet_item_{0}_{1}", i, j), posWnd);

                posWnd.SetActive(true);

                // 注册点击事件
                UIEventListener.Get(posWnd).onClick = OnBaggageItemClicked;
            }
        }
    }

    /// <summary>
    /// 刷新宠物数据.
    /// </summary>
    private void InitData()
    {
        // 取得当前所有的宠物数据
        petData = BaggageMgr.GetItemsByPage(ME.user, ContainerConfig.POS_PET_GROUP);

        // 对宠物按指定方式进行排序
        petData = BaggageMgr.SortPetInBag(petData, BaggageMgr.GetMonsterSortType ());

        // 包裹中无宠物
        if (petData.Count == 0)
        {
            selectedRid = string.Empty;
            ItemSelected(null);
        }

        int Row = mRowNum;

        // 取得玩家宠物格子数量
        containerSize = ME.user.baggage.ContainerSize[ContainerConfig.POS_PET_GROUP].AsInt;

        // 此处包裹中的东西数量有可能比包裹容量大
        if (petData.Count > containerSize)
            containerSize = petData.Count;

        // 取得包裹扩充最大容量（如果包裹达到最大容量，不像是扩充包裹“+”标识）
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

        // 重置面板
        petcontent.maxIndex = 0;
        petcontent.minIndex = -(Row - 1);
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        // 刷新数据
        InitData();

        // 填充数据
        foreach (KeyValuePair<int, int> kv in indexMap)
        {
            FillData(kv.Key, kv.Value);
        }
    }

    /// <summary>
    ///设置滑动列表时复用宠物格子更改数据
    /// </summary>
    void OnUpdateItem(GameObject go, int index, int realIndex)
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
            GameObject petWnd = mPosObMap[string.Format("baggage_pet_item_{0}_{1}", index, i)];

            if (petWnd == null)
                continue;

            PetItemWnd item = petWnd.GetComponent<PetItemWnd>();

            int dataIndex = System.Math.Abs(realIndex) * mColumnNum + i;

            if (dataIndex < petData.Count)
            {
                item.SetBind(petData[dataIndex]);

                item.SetNewPetTips(petData[dataIndex]);

                item.SetLock(PET_BAGGAGE_STATE_ICON.Call(petData[dataIndex]));

                if (string.IsNullOrEmpty(selectedRid))
                {
                    // 最开始默认选中第一个
                    if (dataIndex == 0)
                    {
                        ItemSelected(petWnd);

                        // 删除新物品标识
                        BaggageMgr.ClearNewField(item.item_ob);

                        item.SetNewPetTips(item.item_ob);
                    }
                }
                else
                {
                    if (petData[dataIndex].GetRid().Equals(selectedRid))
                        item.SetSelected(true, false);
                    else
                        item.SetSelected(false, false);
                }
            }
            else if (dataIndex >= petData.Count && dataIndex < containerSize)
            {
                item.SetLock("");
                item.SetSelected(false, false);
                item.SetBind(null);
                item.SetIcon(null);
                item.SetNewPetTips(null);
            }
            else
            {
                item.SetLock("");
                item.SetSelected(false, false);
                item.SetBind(null);
                item.SetIcon("addpet");
                item.SetNewPetTips(null);
            }
        }
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
        for (int i = 0; i < pos.Length; i++)
        {
            if (ContainerConfig.IS_PET_POS(pos[i]))
            {
                Redraw();
                return;
            }
        }
    }

    /// <summary>
    /// 刷新图鉴按钮提示
    /// </summary>
    void RefreshManulaNewTips()
    {
        // 通关兰达平原普通所有副本
        if (! GuideMgr.IsGuided(4))
            return;

        int amount = 0;

        foreach (int element in MonsterConst.sortElement.Keys)
            amount += ManualMgr.GetNewTipsByElement(ME.user, element);

        if (amount > 0)
            mManualNewTips.SetActive(true);
        else
            mManualNewTips.SetActive(false);
    }

    /// <summary>
    /// 设置包裹格子选中
    /// </summary>
    /// <param name="wnd">Window.</param>
    /// <param name="is_selected">If set to <c>true</c> is_selected.</param>
    private void ItemSelected(GameObject ob)
    {
        // 当前包裹无宠物
        if (petData.Count == 0 && ob == null)
        {
            mPetToolTip.GetComponent<PetToolTipWnd>().SetBind(null);
            return;
        }

        PetItemWnd item = ob.GetComponent<PetItemWnd>();
        if (item == null)
            return;

        Property item_ob = item.item_ob;
        if (item_ob == null)
            return;

        // 如果之前有选中，需要先取消之前选中状态
        if (!string.IsNullOrEmpty(selectedRid))
        {
            foreach (Transform child in Container.transform)
            {
                foreach (Transform petWnd in child)
                {
                    Property pet_ob = petWnd.GetComponent<PetItemWnd>().item_ob;

                    if (pet_ob == null)
                        continue;

                    if (pet_ob.GetRid().Equals(selectedRid))
                        petWnd.GetComponent<PetItemWnd>().SetSelected(false, false);
                }
            }
        }

        // 重新标记选中
        selectedRid = item_ob.GetRid();

        // 设置选中状态
        item.SetSelected(true, false);

        // 刷新显示pettooltip
        mPetToolTip.GetComponent<PetToolTipWnd>().SetBind(item_ob);
    }

    /// <summary>
    /// 包裹格子被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnBaggageItemClicked(GameObject ob)
    {
        // 取得gameobject上绑定的item
        PetItemWnd item = ob.GetComponent<PetItemWnd>();

        if (item == null)
            return;

        if (item.item_ob == null)
        {
            // 通关兰达平原普通所有副本
            if (! GuideMgr.IsGuided(4))
            {
                DialogMgr.Notify(LocalizationMgr.Get("GuideWnd_1"));

                return;
            }

            BaggageMgr.TryUpgradeBaggage(ME.user, ContainerConfig.POS_PET_GROUP);

            return;
        }

        // 如果点击的是已标记宠物，不响应
        if (selectedRid.Equals(ob.GetComponent<PetItemWnd>().item_ob.GetRid()))
            return;

        ItemSelected(ob);

        // 删除新物品标识
        BaggageMgr.ClearNewField(item.item_ob);

        item.SetNewPetTips(item.item_ob);
    }

    /// <summary>
    /// 宠物图鉴按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnShowAllPetsBtn(GameObject ob)
    {
        // 通关兰达平原普通所有副本
        if (! GuideMgr.IsGuided(4))
        {
            DialogMgr.Notify(LocalizationMgr.Get("GuideWnd_1"));

            return;
        }

        //打开使魔魔图鉴窗口，重置使魔装备的选中状态和隐藏装备信息界面
        mPetToolTip.GetComponent<PetToolTipWnd>().ResetEquipSelect();

        // 打开使魔图鉴窗口
        WindowMgr.OpenWnd(PetManualWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 排序方式被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnSortBtn(GameObject ob)
    {
        // 通关兰达平原普通所有副本
        if (! GuideMgr.IsGuided(4))
        {
            DialogMgr.Notify(LocalizationMgr.Get("GuideWnd_1"));

            UIToggle toggle = ob.GetComponent<UIToggle>();
            if (toggle == null)
                return;

            toggle.Set(false);

            UIToggle toggle0 = mSortBtn[0].GetComponent<UIToggle>();
            if (toggle0 == null)
                return;

            toggle0.Set(true);

            return;
        }

        for (int i = 0; i < mSortBtn.Length; i++)
        {
            if (ob != mSortBtn[i])
                continue;

            // 点击为当前所选不响应
            if (i == mSelectSort)
                continue;

            mSelectSort = i;

            // 设置宠物排序方式
            BaggageMgr.SetMonsterSortType(i);

            Redraw();

            break;
        }

        mSortBtn[mSelectSort].GetComponent<UIToggle>().Set(true);
    }

    /// <summary>
    /// 指引宠物排序
    /// </summary>
    string GuideSort(Property ob)
    {
        return string.Format("@{0:D2}{1:D2}{2:D2}", ob.GetStar(), ob.GetLevel(), ob.GetRank());
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 指引选择宠物
    /// </summary>
    public void GuideSelectPet(string itemName)
    {
        GameObject item = null;

        if (!mPosObMap.TryGetValue(itemName, out item))
            return;

        OnBaggageItemClicked(item);
    }

    /// <summary>
    /// 选择指定宠物
    /// </summary>
    public void SelectPet(int classId)
    {
        List<Property> selectList = new List<Property>();

        for (int i = 0; i < petData.Count; i++)
        {
            if (petData[i].GetClassID() != classId)
                continue;

            // 缓存符合条件的宠物对象
            selectList.Add(petData[i]);
        }

        // 宠物排序
        IEnumerable<Property> ItemQuery = from ob in selectList orderby GuideSort(ob) descending select ob;

        Property selectOb = null;

        foreach (Property item in ItemQuery)
        {
            selectOb = item;
            break;
        }

        // 如果之前有选中，需要先取消之前选中状态
        if (!string.IsNullOrEmpty(selectedRid))
        {
            foreach (Transform child in Container.transform)
            {
                foreach (Transform petWnd in child)
                {
                    // 获取脚本对象
                    PetItemWnd script = petWnd.GetComponent<PetItemWnd>();

                    // 绑定的宠物对象
                    Property pet_ob = script.item_ob;

                    if (pet_ob == null)
                        continue;

                    if (selectOb.Equals(pet_ob))
                        script.SetSelected(true, false);

                    if (pet_ob.GetRid().Equals(selectedRid))
                        script.SetSelected(false, false);
                }
            }

        }

        // 重新标记选中
        selectedRid = selectOb.GetRid();

        // 刷新显示pettooltip
        mPetToolTip.GetComponent<PetToolTipWnd>().SetBind(selectOb);
    }

    #endregion
}
