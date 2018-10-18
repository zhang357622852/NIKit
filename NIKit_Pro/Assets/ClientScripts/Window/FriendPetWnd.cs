/// <summary>
/// FriendPetWnd.cs
/// Created by fengsc 2017/02/16
/// 选择战斗界面显示好友包裹宠物信息
/// 
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class FriendPetWnd : WindowBase<FriendPetWnd>
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

    // 显示格子行数
    int mRowNum = 3;

    // 宠物列表的父级
    public GameObject mContainer;

    // 宠物格子数量
    private int containerSize = 0;

    public UIScrollView ScrollView;

    // 重复利用ScrollView下的列表的控件
    public UIWrapContent petcontent;
    public PlayerPetWnd mPlayerPetWnd;

    // 玩家宠物列表标题
    public UILabel mFriendTitle;

    // 添加好友提示
    public UILabel mAddFriendTips;

    public UILabel mTips;

    public SelectFighterWnd mSelectFighterWnd;

    public GameObject mItem;

    Dictionary<int, int> mIndexMap = new Dictionary<int, int>();

    // 存储玩家宠物信息
    [HideInInspector]
    public List<Property> mPetData = new List<Property>();

    [HideInInspector]
    public List<string> mPets = new List<string>();

    [HideInInspector]
    public Dictionary<string, LPCMapping> mData = new Dictionary<string, LPCMapping>();

    // 缓存好友宠物基础格子
    Dictionary<string, GameObject> mPetItems = new Dictionary<string, GameObject>();

    // 最大出战数量
    [HideInInspector]
    public int mMaxFightAmount = 0;

    // 出战的数量
    [HideInInspector]
    public int mFightAmount = 0;

    // 好友共享宠物的使用时间
    int mUseTime = 0;

    // 好友共享宠物的使用列表
    LPCArray mUseList = LPCArray.Empty;

    // 好友数量
    int mFriendAmount = 0;

    // 缓存的装备道具
    private List<Property> mCacheEquipOb = new List<Property>();

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start ()
    {
        InitLable();

        Redraw();
    }

    void OnDestroy()
    {
        for (int i = 0; i < mPetData.Count; i++)
        {
            if (mPetData == null)
                continue;

            // 析构物件对象
            mPetData[i].Destroy();
        }

        // 析构掉临时创建的装备对象
        for (int i = 0; i < mCacheEquipOb.Count; i++)
        {
            if (mCacheEquipOb[i] == null)
                continue;

            // 析构物件对象
            mCacheEquipOb[i].Destroy();
        }

        // 解注册事件
        EventMgr.UnregisterEvent(FriendPetWnd.WndType);

        if (ME.user == null)
            return;

        // 移除字段关注
        ME.user.dbase.RemoveTriggerField(FriendPetWnd.WndType);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLable()
    {
        mAddFriendTips.text  = LocalizationMgr.Get("SelectFighterWnd_9");
        mTips.text = LocalizationMgr.Get("SelectFighterWnd_37");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 添加滑动列表时的回调;
        petcontent.onInitializeItem = UpdateItem;

        // 关注字段变化
        ME.user.dbase.RegisterTriggerField(FriendPetWnd.WndType, new string[]{"share_pet_use_time", "share_pet_use_list"}, new CallBack(OnFieldChange));

        // 监听好友操作结果事件
        EventMgr.RegisterEvent(FriendPetWnd.WndType, EventMgrEventType.EVENT_FRIEND_OPERATE_DONE, OnFriendOperateDone);

        // 监听好友更新事件
        EventMgr.RegisterEvent(FriendPetWnd.WndType, EventMgrEventType.EVENT_FRIEND_NOTIFY_LIST, OnFriendNotifyList);
    }

    void OnFriendOperateDone(int eventId, MixedValue para)
    {
        LPCMapping map = para.GetValue<LPCMapping>();

        if (map == null)
            return;

        // 操作结果
        int result = map.GetValue<int>("result");

        // 操作类型
        string oper = map.GetValue<string>("oper");

        LPCMapping extraData = map.GetValue<LPCMapping>("extra_data");

        if ((oper.Equals("agree") || oper.Equals("remove")) && result.Equals(FriendConst.ERESULT_OK))
        {
            SetFriendBaggageInfo();
            for (int i = 0; i < mSelectFighterWnd.mPlayerSelectList.Count; i++)
            {
                Property ob = mSelectFighterWnd.mPlayerSelectList[i].item_ob;

                if (ob == null)
                    continue;

                if (!ob.GetRid().Equals(extraData.GetValue<string>("rid")))
                    continue;

                mSelectFighterWnd.mSelectRidList.Remove(ob.GetRid());

                mSelectFighterWnd.mSelectClassIdList.Remove(ob.GetClassID());

                mSelectFighterWnd.mPlayerSelectList[i].SetBind(null);
            }
            mSelectFighterWnd.UpdateShadow();

            // 刷新队长技能描述
            mSelectFighterWnd.RefreshLeaderDesc();

            mFriendAmount = FriendMgr.FriendList.Count;
        }
    }

    void OnFriendNotifyList(int eventId, MixedValue para)
    {
        SetFriendBaggageInfo();
        mFriendAmount = FriendMgr.FriendList.Count;
    }

    /// <summary>
    /// 字段变化回调
    /// </summary>
    void OnFieldChange(object para, params object[] param)
    {
        LPCValue v = ME.user.Query<LPCValue>("share_pet_use_time");
        if (v != null && v.IsInt)
            mUseTime = v.AsInt;
        else
            mUseTime = 0;

        LPCValue list = ME.user.Query<LPCValue>("share_pet_use_list");
        if (list != null && list.IsArray)
            mUseList = list.AsArray;
        else
            mUseList = LPCArray.Empty;

        foreach (KeyValuePair<int, int> item in mIndexMap)
        {
            FillData(item.Key, item.Value);
        }
    }

    void Redraw()
    {
        // 最大出战数量
        mMaxFightAmount = GameSettingMgr.GetSettingInt("max_friend_fight_amount");

        mFriendAmount = FriendMgr.FriendList.Count;

        mFriendTitle.text = LocalizationMgr.Get("SelectFighterWnd_5");

        LPCMapping instanceInfo = InstanceMgr.GetInstanceInfo(mSelectFighterWnd.mInstanceId);

        if (instanceInfo != null && instanceInfo.GetValue<int>("show_friend_share_pet") != 1)
        {
            if (!mTips.gameObject.activeSelf)
                mTips.gameObject.SetActive(true);

            if (mAddFriendTips.gameObject.activeSelf)
                mAddFriendTips.gameObject.SetActive(false);

            return;
        }

        RegisterEvent();

        mTips.gameObject.SetActive(false);

        // 设置好友包裹数据
        SetFriendBaggageInfo();
    }

    /// <summary>
    /// 载入宠物的附属道具
    /// </summary>
    private void DoPropertyLoaded(Property owner)
    {
        // 获取角色的附属道具
        LPCArray propertyList = owner.Query<LPCArray>("properties");

        // 角色没有附属装备信息
        if (propertyList == null ||
            propertyList.Count == 0)
            return;

        // 转换Container
        Container container = owner as Container;
        LPCMapping dbase = LPCMapping.Empty;
        Property proOb;

        // 遍历各个附属道具
        foreach (LPCValue data in propertyList.Values)
        {
            // 转换数据格式
            dbase = data.AsMapping;

            // 重置一下rid
            dbase.Add("rid", Rid.New());

            // 构建对象
            proOb = PropertyMgr.CreateProperty(dbase, true);

            // 构建对象失败
            if (proOb == null)
                continue;

            mCacheEquipOb.Add(proOb);

            // 将道具载入包裹中
            container.LoadProperty(proOb, dbase["pos"].AsString);
        }
    }

    /// <summary>
    /// 玩家包裹数据
    /// </summary>
    void SetFriendBaggageInfo()
    {
        // 获取玩家好友列表
        LPCArray friendList = FriendMgr.FriendList;

        if (friendList == null || friendList.Count == 0)
        {
            // 没有好友情况下的显示
            ShowNoFriendData();
            return;
        }

        List<Property> pets = new List<Property>();

        for (int i = 0; i < friendList.Count; i++)
        {
            // 克隆宠物对象
            LPCMapping sharePet = friendList[i].AsMapping.GetValue<LPCMapping>("share_pet");

            // 该玩家没有共享宠物
            if (sharePet == null || sharePet.Count == 0)
                continue;

            // 还原宠物数据
            Property ob = PropertyMgr.CreateProperty(sharePet, true);
            if (ob == null)
                continue;

            // 还原角色装备
            DoPropertyLoaded(ob);

            // 刷新宠物属性
            PropMgr.RefreshAffect(ob);

            // 好友数据
            LPCMapping data = LPCMapping.Empty;
            data.Add("name", friendList[i].AsMapping.GetValue<string>("name"));
            data.Add("level", friendList[i].AsMapping.GetValue<int>("level"));
            data.Add("rid", friendList[i].AsMapping.GetValue<string>("rid"));
            if (!mData.ContainsKey(ob.GetRid()))
                mData.Add(ob.GetRid(), data);
            else
                mData[ob.GetRid()] = data;

            pets.Add(ob);
        }

        // 显示好友共享宠物数据
        ShowFriendPetData(pets);

        // 设置格子复用控件
        SetWrapContent(mRowNum);
    }

    /// <summary>
    ///  显示好友共享宠物数据
    /// </summary>
    void ShowFriendPetData(List<Property> pets)
    {
        mAddFriendTips.gameObject.SetActive(false);
        InitFriendPetGrid(mRowNum, mColumnNum);

        mUseTime = 0;

        // 好友共享宠物使用数据
        LPCValue v = ME.user.Query<LPCValue>("share_pet_use_time");
        if (v != null && v.IsInt)
            mUseTime = v.AsInt;

        mUseList = LPCArray.Empty;

        LPCValue list = ME.user.Query<LPCValue>("share_pet_use_list");
        if (list != null && list.IsArray)
            mUseList = list.AsArray;

        mPetData.Clear();

        // 已使用的好友宠物
        List<Property> usePet = new List<Property>();

        List<Property> petList = new List<Property>();

        for (int i = 0; i < pets.Count; i++)
        {
            Property ob = pets[i];
            if (ob == null)
                continue;

            mSelectFighterWnd.AddCacheOb(ob);

            // 已经使用过的好友共享宠物保持在列表末端
            if (IsUserSharePet(ob.GetRid()))
            {
                if (! usePet.Contains(ob))
                    usePet.Add(ob);
            }
            else
            {
                if (! petList.Contains(ob))
                    petList.Add(ob);
            }
        }

        petList = BaggageMgr.SortPetInBag(petList, MonsterConst.SORT_BY_STAR);

        usePet = BaggageMgr.SortPetInBag(usePet, MonsterConst.SORT_BY_STAR);

        mPetData.AddRange(petList);

        mPetData.AddRange(usePet);

        for (int i = 0; i < mPetData.Count; i++)
        {
            if (mPetData[i] == null)
                continue;

            mPets.Add(mPetData[i].GetRid());
        }
    }

    /// <summary>
    /// 显示没有好友情况下的数据
    /// </summary>
    void ShowNoFriendData()
    {
        foreach (KeyValuePair<string, GameObject> item in mPetItems)
            Destroy(item.Value);

        // 清空字典
        mPetItems.Clear();

        // 克隆两个空格子
        mAddFriendTips.gameObject.SetActive(true);
        InitFriendPetGrid(1, 2);

        int index = 0;
        foreach (KeyValuePair<string, GameObject> item in mPetItems)
        {
            item.Value.SetActive(true);
            PetItemWnd petItem = item.Value.GetComponent<PetItemWnd>();
            petItem.SetSelected(false);
            petItem.SetBind(null);

            GameObject name = item.Value.transform.Find("name").gameObject;

            UILabel namelable = null;
            if (name != null)
            {
                namelable = name.GetComponent<UILabel>();
            }

            if (index + 1 == mPetItems.Count)
            {
                petItem.SetIcon("addpet");

                // 显示添加好友
                if (namelable != null)
                    namelable.text = LocalizationMgr.Get("SelectFighterWnd_8");

                UIEventListener.Get(item.Value).onClick = OnClickAddFriend;
            }
            else
            {
                petItem.SetIcon(null);

                // 显示没有好友
                if (namelable != null)
                    namelable.text = LocalizationMgr.Get("SelectFighterWnd_7");
            }
            index++;
        }
    }

    /// <summary>
    /// 设置格子复用控件
    /// </summary>
    void SetWrapContent(int rowNum)
    {
        int maxAmount = GameSettingMgr.GetSettingInt("max_friend_pet_amount");

        int size = mFriendAmount % mColumnNum == 0 ? mFriendAmount + mColumnNum : mColumnNum - mFriendAmount % mColumnNum + mColumnNum + mFriendAmount;

        containerSize = mFriendAmount >= maxAmount ? maxAmount : size + mColumnNum;

        int containerRow = containerSize % mColumnNum == 0 ?
            containerSize / mColumnNum : containerSize / mColumnNum + 1;

        // 多显示一行用来显示添加格子按钮
        if (containerRow >= mRowNum)
            rowNum = containerRow + 1;

        int maxRow = containerSize % mColumnNum == 0 ?
            containerSize / mColumnNum : containerSize / mColumnNum + 1;

        // 已达到最大格子数量，不显示添加格子按钮
        if (containerRow >= maxRow)
            rowNum = containerRow;

        petcontent.maxIndex = 0;

        petcontent.minIndex = -(rowNum - 1);

        petcontent.enabled = true;
    }

    /// <summary>
    /// 实例化玩家所有的宠物格子
    /// </summary>
    void InitFriendPetGrid(int rowNum, int columnNum)
    {
        // 生成格子，只生成这么多格子，动态复用
        for (int i = 0; i < rowNum; i++)
        {
            string name = "item_" + i.ToString();
            GameObject rowItemOb;

            if (mContainer.transform.Find(name) != null)
                rowItemOb = mContainer.transform.Find(name).gameObject;
            else
            {
                // 创建空物体;
                rowItemOb = new GameObject();

                rowItemOb.name = name;

                rowItemOb.transform.parent = mContainer.transform;

                rowItemOb.transform.localPosition = new Vector3(mInitPos.x, mInitPos.y - i * (mItemSize.y + mItemSpace.y), 0);

                rowItemOb.transform.localScale = Vector3.one;
            }

            for (int j = 0; j < columnNum; j++)
            {
                string posName = string.Format("friend_pet_item_{0}_{1}", i, j);
                if (mPetItems.ContainsKey(posName) && mPetItems[posName] != null)
                    continue;

                // 删除数据
                mPetItems.Remove(posName);

                // 窗口包裹格
                GameObject posWnd = Instantiate(mItem);

                float x, y;

                x = (mItemSize.x + mItemSpace.x) * j;

                y = 0f;

                // 初始化transform
                posWnd.transform.SetParent(rowItemOb.transform);
                posWnd.transform.localPosition = new Vector3(x, y, 0);
                posWnd.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                posWnd.name = posName;

                posWnd.SetActive(false);
                DragDropPetItem drag = posWnd.AddComponent<DragDropPetItem>();
                drag.restriction = UIDragDropItem.Restriction.Horizontal;

                drag.SetCallBack(new CallBack(mSelectFighterWnd.CallBack));

                posWnd.tag = "FriendGrid";

                // 添加缓存列表
                mPetItems.Add(posName, posWnd);

                // 注册点击事件
                UIEventListener.Get(posWnd).onClick = ClickSelectPet;

                // 注册长按事件
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
    /// 加载好友界面
    /// </summary>
    void OnClickAddFriend(GameObject go)
    {
        // 通关兰达平原普通所有副本
        if (! GuideMgr.IsGuided(4))
        {
            DialogMgr.Notify(LocalizationMgr.Get("GuideWnd_1"));

            return;
        }

        mSelectFighterWnd.CancelLoopFight();

        GameObject friendWnd = WindowMgr.OpenWnd(FriendWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (friendWnd == null)
            return;

        friendWnd.GetComponent<FriendWnd>().Bind(false);
    }

    /// <summary>
    /// 填充数据
    /// </summary>
    void FillData(int wrapIndex, int realIndex)
    {
        if (FriendMgr.FriendList == null || FriendMgr.FriendList.Count == 0)
            return;
        for (int i = 0; i < mColumnNum; i++)
        {
            string key = string.Format("friend_pet_item_{0}_{1}", wrapIndex, i);
            if (! mPetItems.ContainsKey(key))
                continue;

            GameObject petWnd = mPetItems[key];
            if (petWnd == null)
                continue;

            PetItemWnd petItem = petWnd.GetComponent<PetItemWnd>();
            if (petItem == null)
                continue;

            // 计算索引，通过索引拿到对应的宠物数据;
            int dataIndex = System.Math.Abs(realIndex) * mColumnNum + i;

            petWnd.SetActive(true);

            GameObject nameGo = petWnd.transform.Find("name").gameObject;
            UILabel lable = null;
            if (nameGo != null)
            {
                lable = nameGo.GetComponent<UILabel>();
                lable.gameObject.SetActive(true);
            }

            if (dataIndex < mPetData.Count)
            {
                petItem.ShowMaxLevel(true);

                petItem.ShowLeaderSkill(true);

                // 设置宠物数据;
                petItem.SetBind(mPetData[dataIndex]);

                // 获取玩家名称
                LPCMapping friendData = mData[mPetData[dataIndex].GetRid()];

                if (lable != null)
                {
                    lable.text = friendData.GetValue<string>("name");
                }

                mSelectFighterWnd.arrPet.Add(petWnd.GetComponent<PetItemWnd>());

                if (mSelectFighterWnd.mSelectRidList.Count >= mSelectFighterWnd.amount)
                {
                    if (IsUserSharePet(mPetData[dataIndex].GetRid()))
                    {
                        // TODO 随后替换已使用的图标
                        petItem.ShowCover(true);
//                        petItem.ShowCover(false);
                    }
                    else
                    {
                        petItem.ShowCover(true);
                    }
                }
                else
                {
                    if (IsUserSharePet(mPetData[dataIndex].GetRid()))
                    {
                        // TODO 随后替换已使用的的图标
                        petItem.ShowCover(true);
                        //                        petItem.ShowCover(false);
                    }
                    else
                    {
                        petItem.ShowCover(false);
                    }
                }

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
                // 去掉选中效果;
                petItem.SetSelected(false);
                petItem.SetBind(null);
                petItem.ShowCover(false);
                petItem.SetIcon(null);

                if (lable != null)
                {
                    // 没有好友
                    lable.text = LocalizationMgr.Get("SelectFighterWnd_7");
                }
            }
            else
            {
                petWnd.SetActive(false);
            }
        }
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 是否使用该共享宠物
    /// </summary>
    public bool IsUserSharePet(string petRid)
    {
        if (string.IsNullOrEmpty(petRid))
            return false;

        if (!mData.ContainsKey(petRid))
            return false;

        string userRid = mData[petRid].GetValue<string>("rid");

        // 今天已经使用过
        if (Game.IsSameDay(mUseTime, TimeMgr.GetServerTime()) &&
            mUseList.IndexOf(userRid) != -1)
            return true;
        else
            return false;
    }


    /// <summary>
    /// 点击选择宠物
    /// </summary>
    public void ClickSelectPet(GameObject go)
    {
        // 通关兰达平原普通所有副本
        if (! GuideMgr.IsGuided(4))
        {
            DialogMgr.Notify(LocalizationMgr.Get("GuideWnd_1"));

            return;
        }

        // 拿到点击的宠物对象;
        Property proItem = go.GetComponent<PetItemWnd>().item_ob;

        if (proItem == null)
            return;

        if (IsUserSharePet(proItem.GetRid()))
        {
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("SelectFighterWnd_17"),
                string.Empty,
                string.Empty,
                true,
                WindowMgr.GetWindow(SelectFighterWnd.WndType).transform
            );
            return;
        }

        if (mFightAmount <= mMaxFightAmount &&
            mSelectFighterWnd.amount == mSelectFighterWnd.mSelectRidList.Count)
        {
            // 栏位已满
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
        else if(mFightAmount >= mMaxFightAmount ||
            mSelectFighterWnd.amount == mSelectFighterWnd.mSelectRidList.Count)
        {
            // 好友共享宠物只能选择一个
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("SelectFighterWnd_15"),
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

            if (i == 0)
                mSelectFighterWnd.mPlayerSelectList[i].ShowLeaderSkill(true);

            mSelectFighterWnd.mPlayerSelectList[i].ShowMaxLevel(true);

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
            mSelectFighterWnd.mSelectClassIdList.Add(proItem.GetClassID());
            mSelectFighterWnd.mSelectRidList.Add(proItem.GetRid());
        }

        mFightAmount++;
        if (mSelectFighterWnd.amount == mSelectFighterWnd.mSelectRidList.Count)
        {
            // 刷新数据
            mPlayerPetWnd.RefreshData();
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
            mPlayerPetWnd.RefreshData();
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
            break;
        }

        // 刷新队长技能描述
        mSelectFighterWnd.RefreshLeaderDesc();

        mFightAmount--;
        mSelectFighterWnd.UpdateShadow();

        if (mSelectFighterWnd.mIsLoopFight)
            mSelectFighterWnd.OnClickLoopFightBtn();

        UIEventListener.Get(go).onClick -= ClickCancelSelectPet;

        //取消选择宠物后，添加选择宠物点击事件
        UIEventListener.Get(go).onClick = ClickSelectPet;
    }

    public void RefreshData()
    {
        foreach (KeyValuePair<int, int> kv in mIndexMap)
            FillData(kv.Key, kv.Value);
    }

    #endregion
}
