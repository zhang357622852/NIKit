/// <summary>
/// ShowUserPetWnd.cs
/// Created by fengsc 2017/02/16
/// 选择战斗界面显示玩家包裹宠物信息
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class PlayerPetWnd : WindowBase<PlayerPetWnd>
{
    #region 成员变量

    // 初始位置.
    Vector2 mInitPos = new Vector2(0, 0);

    // 每个格子的间隔
    Vector2 mItemSpace = new Vector2(6, 6);

    // 每个格子的大小;
    Vector2 mItemSize = new Vector2(110, 110);

    // 列数
    int mColumnNum = 4;

    // 行数
    int mRowNum = 5;

    // 宠物列表的父级
    public GameObject mContainer;

    // 宠物格子数量
    private int containerSize = 0;

    public UIScrollView ScrollView;

    // 重复利用ScrollView下的列表的控件
    public UIWrapContent petcontent;

    public FriendPetWnd mFriendPetWnd;

    // 玩家宠物列表标题
    public UILabel mPlayerTitle;

    public SelectFighterWnd mSelectFighterWnd;

    public GameObject mItemPetWnd;

    Dictionary<int, int> mIndexMap = new Dictionary<int, int>();

    // 存储玩家宠物信息
    [HideInInspector]
    public List<Property> mPetData = new List<Property>();

    [HideInInspector]
    public List<string> mPets = new List<string>();

    public Dictionary<string, GameObject> mItems = new Dictionary<string, GameObject>();

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start ()
    {
        RegisterEvent();

        InitPetData();

        InitPlayerAllPetGrid();

        InitLable();
    }

    void OnDestroy()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 移除属性字段关注回调
        ME.user.dbase.RemoveTriggerField("SelectFighterWnd");

        // 删除玩家包裹变化回调
        ME.user.baggage.eventCarryChange -= BaggageChange;
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLable()
    {
        mPlayerTitle.text = LocalizationMgr.Get("SelectFighterWnd_4");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 添加滑动列表时的回调;
        petcontent.onInitializeItem = UpdateItem;

        ME.user.baggage.eventCarryChange += BaggageChange;

        // 关注包裹格子变化
        ME.user.dbase.RegisterTriggerField("SelectFighterWnd", new string[] { "container_size" }, new CallBack(OnBagContainerSizeChange));
    }

    /// <summary>
    /// 包裹格子数量变化回调
    /// </summary>
    void OnBagContainerSizeChange(object para, params object[] param)
    {
        // 窗口没有显示，不处理
        if (gameObject == null ||
            !gameObject.activeSelf ||
            !gameObject.activeInHierarchy)
            return;

        // 初始化宠物数据
        InitPetData();

        // 填充数据
        foreach (KeyValuePair<int, int> kv in mIndexMap)
            FillData(kv.Key, kv.Value);
    }

    /// <summary>
    /// 包裹格子变化的回调
    /// </summary>
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
            if (!ContainerConfig.IS_PET_POS(pos[i]))
                continue;

            // 初始化宠物数据
            InitPetData();

            // 填充数据
            foreach (KeyValuePair<int, int> kv in mIndexMap)
                FillData(kv.Key, kv.Value);
            return;
        }
    }

    /// <summary>
    /// 初始化宠物数据
    /// </summary>
    void InitPetData()
    {
        // 获取副本配置信息
        LPCMapping instanceInfo = InstanceMgr.GetInstanceInfo(mSelectFighterWnd.mInstanceId);
        if (instanceInfo == null)
            return;

        //获取玩家宠物数据, 过滤掉材料宠物;
        mPetData = UserMgr.GetUserPets(ME.user, InstanceMgr.IsSelectStorePet(mSelectFighterWnd.mInstanceId), false, instanceInfo.GetValue<int>("limit_level"));

        // 对宠物按指定方式进行排序
        mPetData = BaggageMgr.SortPetInBag(mPetData, MonsterConst.SORT_BY_STAR);

        for (int i = 0; i < mPetData.Count; i++)
        {
            if (mPetData[i] == null)
                continue;
            mPets.Add(mPetData[i].GetRid());
        }

        int Row = mRowNum;

        containerSize = ME.user.baggage.ContainerSize[ContainerConfig.POS_PET_GROUP].AsInt;

        // 此处包裹中的东西数量有可能比包裹容量大
        if (mPetData.Count > containerSize)
            containerSize = mPetData.Count;

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

        petcontent.enabled = true;
    }

    /// <summary>
    /// 实例化玩家所有的宠物格子
    /// </summary>
    void InitPlayerAllPetGrid()
    {
        if (mItemPetWnd.activeSelf)
            mItemPetWnd.SetActive(false);

        // 生成格子，只生成这么多格子，动态复用
        for (int i = 0; i < mRowNum; i++)
        {
            string name = "item_" + i.ToString();
            GameObject rowItemOb;

            if (mContainer.transform.Find(name) != null)
                rowItemOb = mContainer.transform.Find(name).gameObject;
            else
            {
                //创建空物体;
                rowItemOb = new GameObject();

                rowItemOb.name = name;

                rowItemOb.transform.parent = mContainer.transform;

                rowItemOb.transform.localPosition = new Vector3(mInitPos.x, mInitPos.y - i * (mItemSize.y + mItemSpace.y), 0);

                rowItemOb.transform.localScale = Vector3.one;
            }

            for (int j = 0; j < mColumnNum; j++)
            {
                string posName;
                GameObject posWnd;

                posName = string.Format("pet_item_{0}_{1}", i, j);
                if (mItems.ContainsKey(posName))
                    mItems.Remove(posName);

                float x, y;

                //创建一个宠物格子对象;
                posWnd = Instantiate(mItemPetWnd);
                posWnd.transform.SetParent(rowItemOb.transform);
                posWnd.transform.localPosition = Vector3.zero;
                posWnd.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                posWnd.name = posName;

                x = (mItemSize.x + mItemSpace.x) * j;
                y = 0f;
                posWnd.transform.localPosition = new Vector3(x, y, 0);
                posWnd.SetActive(true);

                mItems.Add(posName, posWnd);

                DragDropPetItem drag = posWnd.AddComponent<DragDropPetItem>();
                drag.restriction = UIDragDropItem.Restriction.Horizontal;

                drag.SetCallBack(new CallBack(mSelectFighterWnd.CallBack));

                // 设置格子的标签
                posWnd.tag = "UserGrid";

                // 注册点击事件
                UIEventListener.Get(posWnd).onClick = ClickSelectPet;

                //注册长按事件
                UIEventListener.Get(posWnd).onPress = mSelectFighterWnd.OnPressShowPetInfo;
            }
        }
    }


    /// <summary>
    /// 设置滑动列表时复用宠物格子更改数据
    /// </summary>
    void UpdateItem(GameObject go, int wrapIndex, int realIndex)
    {
        // 将index与realindex对应关系记录下来
        if (!mIndexMap.ContainsKey(wrapIndex))
            mIndexMap.Add(wrapIndex, realIndex);
        else
            mIndexMap[wrapIndex] = realIndex;

        // 填充数据
        FillData(wrapIndex, realIndex);
    }

    /// <summary>
    /// 填充数据
    /// </summary>
    void FillData(int wrapIndex, int realIndex)
    {
        if (mPetData == null || mPetData.Count == 0)
            return;

        for (int i = 0; i < mColumnNum; i++)
        {
            string name = string.Format("pet_item_{0}_{1}", wrapIndex, i);
            if (!mItems.ContainsKey(name))
                continue;

            GameObject petWnd = mItems[name];

            PetItemWnd petItem = petWnd.GetComponent<PetItemWnd>();
            if (petItem == null)
                continue;

            // 计算索引，通过索引拿到对应的宠物数据;
            int dataIndex = System.Math.Abs(realIndex) * mColumnNum + i;

            if (dataIndex < mPetData.Count)
            {
                petItem.ShowMaxLevel(true);

                petItem.ShowLeaderSkill(true);

                // 设置宠物数据;
                petItem.SetBind(mPetData[dataIndex]);

                mSelectFighterWnd.arrPet.Add(petWnd.GetComponent<PetItemWnd>());

                if (mSelectFighterWnd.mSelectRidList.Count >= mSelectFighterWnd.amount)
                    petItem.ShowCover(true);
                else
                    petItem.ShowCover(false);

                if (!mSelectFighterWnd.mSelectRidList.Contains(mPetData[dataIndex].GetRid()))
                {
                    UIEventListener.Get(petWnd).onClick = ClickSelectPet;

                    petItem.SetSelected(false, false);
                }
                else
                {
                    // 恢复选中状态;
                    petItem.SetSelected(true);
                    petItem.ShowCover(false);
                    UIEventListener.Get(petWnd).onClick = ClickCancelSelectPet;
                }
            }
            else if (mPetData.Count < containerSize && dataIndex < containerSize)
            {
                //去掉选中效果;
                petItem.SetSelected(false);
                petItem.ShowCover(false);
                petItem.SetBind(null);
                petItem.SetIcon(null);
            }
            else
            {
                //去掉选中效果;
                petItem.SetSelected(false);
                petItem.ShowCover(false);
                petItem.SetBind(null);
                petItem.SetIcon("addpet");
            }
        }
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 刷新数据
    /// </summary>
    public void RefreshData()
    {
        foreach (KeyValuePair<int, int> kv in mIndexMap)
            FillData(kv.Key, kv.Value);
    }

    /// <summary>
    /// 点击选择宠物
    /// </summary>
    public void ClickSelectPet(GameObject go)
    {
        if (mSelectFighterWnd.amount < 1)
            return;

        // 拿到点击的宠物对象;
        Property proItem = go.GetComponent<PetItemWnd>().item_ob;
        if (proItem == null)
        {
            // 通关兰达平原普通所有副本
            if (! GuideMgr.IsGuided(4))
            {
                DialogMgr.Notify(LocalizationMgr.Get("GuideWnd_1"));

                return;
            }

            // 尝试升级包裹
            BaggageMgr.TryUpgradeBaggage(ME.user, ContainerConfig.POS_PET_GROUP);

            return;
        }

        if (mSelectFighterWnd.amount == mSelectFighterWnd.mSelectRidList.Count)
        {
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("SelectFighterWnd_16"),
                string.Empty,
                string.Empty,
                true,
                WindowMgr.GetWindow(SelectFighterWnd.WndType).transform
            );
            return;
        }

        if (! InstanceMgr.IsAllowSamePet(mSelectFighterWnd.mInstanceId) &&
            mSelectFighterWnd.mSelectClassIdList.Contains(proItem.GetClassID()))
        {
            // 通天之塔不能使用相同的使魔
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("SelectFighterWnd_40"),
                string.Empty,
                string.Empty,
                true,
                WindowMgr.GetWindow(SelectFighterWnd.WndType).transform
            );
            return;
        }

        for (int i = 0; i < mSelectFighterWnd.mPlayerSelectList.Count; i++)
        {
            // 已经有玩家数据
            if (mSelectFighterWnd.mPlayerSelectList[i].item_ob != null)
                continue;

            mSelectFighterWnd.mPlayerSelectList[i].ShowMaxLevel(true);

            if (i == 0)
                mSelectFighterWnd.mPlayerSelectList[i].ShowLeaderSkill(true);

            // 玩家阵容显示选择的宠物;
            mSelectFighterWnd.mPlayerSelectList[i].SetBind(proItem);

            // 设置宠物格子标签
            mSelectFighterWnd.SetFormationPetItemTag(mSelectFighterWnd.mPlayerSelectList[i].gameObject);
            break;
        }

        // 刷新队长技能描述
        mSelectFighterWnd.RefreshLeaderDesc();

        // 设置选中;
        mSelectFighterWnd.SetPetSelectState(go);

        // 保存选中的Rid;
        if (mSelectFighterWnd.mSelectRidList != null)
        {
            mSelectFighterWnd.mSelectRidList.Add(proItem.GetRid());

            mSelectFighterWnd.mSelectClassIdList.Add(proItem.GetClassID());
        }

        if (mSelectFighterWnd.amount == mSelectFighterWnd.mSelectRidList.Count)
        {
            // 刷新数据
            mFriendPetWnd.RefreshData();
            RefreshData();
        }

        mSelectFighterWnd.UpdateShadow();

        if (mSelectFighterWnd.mIsLoopFight)
            mSelectFighterWnd.OnClickLoopFightBtn();

        UIEventListener.Get(go).onClick -= ClickSelectPet;

        // 选择宠物后，添加取消选择宠物点击事件;
        UIEventListener.Get(go).onClick = ClickCancelSelectPet;
    }

    /// <summary>
    /// 点击取消选择宠物
    /// </summary>
    public void ClickCancelSelectPet(GameObject go)
    {
        // 获取点击的宠物的数据;
        Property proPet = go.GetComponent<PetItemWnd>().item_ob;
        if (proPet == null)
            return;

        // 设置选中;
        mSelectFighterWnd.SetPetSelectState(go);

        // 移除保存的;
        if (mSelectFighterWnd.mSelectRidList != null &&
            mSelectFighterWnd.mSelectRidList.Count > 0)
        {
            mSelectFighterWnd.mSelectRidList.Remove(proPet.GetRid());

            mSelectFighterWnd.mSelectClassIdList.Remove(proPet.GetClassID());
        }

        if (mSelectFighterWnd.amount == mSelectFighterWnd.mSelectRidList.Count + 1)
        {
            // 刷新数据
            mFriendPetWnd.RefreshData();
            RefreshData();
        }

        for (int i = 0; i < mSelectFighterWnd.mPlayerSelectList.Count; i++)
        {
            // 玩家阵容中此位置没有数据;
            if (mSelectFighterWnd.mPlayerSelectList[i].item_ob == null)
                continue;

            if (!mSelectFighterWnd.mPlayerSelectList[i].item_ob.GetRid().Equals(proPet.GetRid()))
                continue;

            mSelectFighterWnd.mPlayerSelectList[i].ShowMaxLevel(false);

            mSelectFighterWnd.mPlayerSelectList[i].SetBind(null);

            // 设置宠物格子标签
            mSelectFighterWnd.SetFormationPetItemTag(mSelectFighterWnd.mPlayerSelectList[i].gameObject);

            // 设置队长技能效果描述;
            if (i == 0 && mSelectFighterWnd.mFormation.ContainsKey(FormationConst.RAW_NONE))
                mSelectFighterWnd.LeaderSkillEffectDesc(null);
        }

        // 刷新队长技能描述
        mSelectFighterWnd.RefreshLeaderDesc();

        mSelectFighterWnd.UpdateShadow();

        if (mSelectFighterWnd.mIsLoopFight)
            mSelectFighterWnd.OnClickLoopFightBtn();

        UIEventListener.Get(go).onClick -= ClickCancelSelectPet;

        // 取消选择宠物后，添加选择宠物点击事件
        UIEventListener.Get(go).onClick = ClickSelectPet;
    }

    #endregion
}
