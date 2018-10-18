/// <summary>
/// ViewUserWnd.cs
/// Created by lic 2016-12-9
/// 访问界面
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ViewUserWnd : WindowBase<ViewUserWnd>
{
    #region 成员变量

    public UIScrollView ScrollView;
    public UIWrapContent petcontent;
    public GameObject mCloseBtn;

    public GameObject mPetToolTip;

    public GameObject mCloseWndBtn;
    public UILabel mCloseWndBtnLb;

    // 玩家头像
    public UITexture mIcon;

    public GameObject mViewDefencebtn;
    public UILabel mViewDefencebtnLb;

    public UILabel mUserNameLb;

    public UISprite mRankIcon;

    public UISprite[] mStars;

    public GameObject mPetItem;

    public TweenScale mTweenScale;

    #endregion

    #region 私有变量

    // 每5个item为一行
    const int mColumnNum = 5;

    // 此处是指预先创建多少行
    const int mRowNum = 5;

    private List<Property> petData = new List<Property>();

    private List<Property> equipData = new List<Property>();

    private string selectedRid = string.Empty;

    // 当前显示数据的index与实际数据的对应关系
    private Dictionary<int, int> indexMap = new Dictionary<int, int>();

    // 当前访问的玩家
    private Property mUser;

    LPCArray mDefenceList = LPCArray.Empty;

    // name与OB映射
    private Dictionary<string, GameObject> mPosObMap = new Dictionary<string, GameObject>();

    private string sharePet = string.Empty;

    #endregion

    #region 内部函数

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        petcontent.onInitializeItem = OnUpdateItem;

        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;
        UIEventListener.Get(mCloseWndBtn).onClick = OnCloseBtn;

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        // 初始化本地化文本
        mCloseWndBtnLb.text = LocalizationMgr.Get("ViewUserWnd_3");
        mViewDefencebtnLb.text = LocalizationMgr.Get("ViewUserWnd_2");

        mPetItem.SetActive (false);
    }

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        //初始化窗口
        InitWnd();

        // 创建格子
        CreatePos();
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        // 注销事件
        EventMgr.UnregisterEvent("VisitBaggageWnd");

        // 析构掉临时创建的宠物对象
        for (int i = 0; i < petData.Count; i++)
        {
            if (petData[i] == null)
                continue;

            // 析构物件对象
            petData[i].Destroy();
        }

        // 析构掉临时创建的装备对象
        for (int i = 0; i < equipData.Count; i++)
        {
            if (equipData[i] == null)
                continue;

            // 析构物件对象
            equipData[i].Destroy();
        }

        // 析构临时玩家对象
        // 这个对象由FriendViewWnd界面去销毁
        mUser = null;
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    void OnCloseBtn(GameObject ob)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 装备信息数据
    /// </summary>
    public void SetUserData(int eventId, MixedValue para)
    {
        // 如果窗口绑定对象已经不存在
        if (mUser == null)
            return;

        // 转换数据格式
        LPCMapping args = para.GetValue<LPCMapping>().GetValue<LPCMapping>("data");

        // 如果不是窗口绑定玩家id，不处理
        if (! string.Equals(mUser.GetRid(), args.GetValue<string>("rid")))
            return;

        // 获取防守宠物列表
        mDefenceList = args.GetValue<LPCArray>("defense_troop");

        // 获取该玩家的包裹宠物信息
        LPCArray pets = args.GetValue<LPCArray>("baggage_pet_list");
        if (pets == null)
            return;

        // 获取玩家的共享宠物
        LPCValue v = mUser.Query<LPCValue>("share_pet");
        Property shareOb = null;

        foreach (LPCValue item in pets.Values)
        {
            if (item == null)
                continue;

            // TODO: 收集异常数据
            if (!item.IsMapping)
            {
                LogMgr.Error("异常的宠物数据 : {0}， pet_amount = {1}， user_rid = {2}, user_name = {3}",
                    item.GetDescription(), pets.Count, mUser.GetRid(), mUser.GetName());
                continue;
            }

            LPCMapping dbase = LPCValue.Duplicate(item).AsMapping;

            string newRid = Rid.New();

            dbase.Add("rid", newRid);

            // 克隆宠物对象
            Property ob = PropertyMgr.CreateProperty(dbase);

            if (ob == null)
                continue;

            // 将道具载入包裹中
            DoPropertyLoaded(ob);

            // 刷新宠物属性
            PropMgr.RefreshAffect(ob);

            // 判断是否是共享使魔
            if (v != null &&
                item.AsMapping.GetValue<string>("rid").Equals(v.AsString))
            {
                sharePet = newRid;
                shareOb = ob;
                continue;
            }

            // 添加到petData
            petData.Add(ob);
        }

        // 按照星级排序
        petData = BaggageMgr.SortPetInBag(petData, MonsterConst.SORT_BY_STAR);

        // 保持将使魔放在第一位
        if (shareOb != null)
            petData.Insert(0, shareOb);

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 载入宠物的附属道具
    /// </summary>
    private void DoPropertyLoaded(Property owner)
    {
        // 获取角色的附属道具
        LPCArray propertyList = owner.Query<LPCArray>("property_list");

        // 角色没有附属装备信息
        if (propertyList == null ||
            propertyList.Count == 0)
            return;

        // 转换Container
        Container container = owner as Container;
        Property proOb;

        // 遍历各个附属道具
        foreach (LPCValue data in propertyList.Values)
        {
            LPCMapping dbase = LPCValue.Duplicate(data).AsMapping;

            dbase.Add("rid", Rid.New());

            // 构建对象
            proOb = PropertyMgr.CreateProperty(dbase, true);
            equipData.Add(proOb);

            // 构建对象失败
            if (proOb == null)
                continue;

            // 将道具载入包裹中
            container.LoadProperty(proOb, dbase["pos"].AsString);
        }
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
            rowItemOb.name = string.Format("item_", i);
            rowItemOb.transform.parent = petcontent.transform;
            rowItemOb.transform.localPosition = new Vector3(0, - i * 114, 0);
            rowItemOb.transform.localScale = Vector3.one;

            for(int j = 0; j < mColumnNum;j++)
            {
                GameObject posWnd = Instantiate (mPetItem) as GameObject;
                posWnd.transform.parent = rowItemOb.transform;
                posWnd.name = string.Format("visit_pet_item_{0}_{1}", i, j);
                posWnd.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                posWnd.transform.localPosition = new Vector3(114 * j, 0f, 0f);

                mPosObMap.Add (string.Format("visit_pet_item_{0}_{1}", i, j), posWnd);

                posWnd.SetActive(true);

                // 注册点击事件
                UIEventListener.Get(posWnd).onClick = OnBaggageItemClicked;
            }
        }
    }

    /// <summary>
    /// 刷新宠物数据.
    /// </summary>
    private void InitPetData()
    {
        // 包裹中无宠物
        if (petData.Count == 0)
        {
            selectedRid = string.Empty;
            ItemSelected(null);
        }

        int Row = mRowNum;

        int containerSize = petData.Count;

        int containerRow = containerSize % mColumnNum == 0 ?
            containerSize / mColumnNum : containerSize / mColumnNum + 1;

        // 多显示一行用来显示添加格子按钮
        if (containerRow > mRowNum)
            Row = containerRow;

        // 从0开始
        petcontent.maxIndex = 0;

        petcontent.minIndex = -(Row - 1);

        petcontent.enabled = true;
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        UIEventListener.Get(mViewDefencebtn).onClick = OnClickViewDefenceBtn;

        // 刷新宠物数据数据
        InitPetData();

        // 填充数据
        foreach (KeyValuePair<int, int> kv in indexMap)
            FillPetData(kv.Key, kv.Value);
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

        FillPetData(index, realIndex);
    }

    /// <summary>
    /// 填充数据
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="realIndex">Real index.</param>
    private void FillPetData(int index, int realIndex)
    {
        for (int i = 0; i < mColumnNum; i++)
        {
            GameObject petWnd = mPosObMap [string.Format ("visit_pet_item_{0}_{1}", index, i)];

            if (petWnd == null)
                continue;

            PetItemWnd item = petWnd.GetComponent<PetItemWnd>();

            int dataIndex = System.Math.Abs(realIndex) * mColumnNum + i;

            if (dataIndex < petData.Count)
            {
                item.SetBind(petData[dataIndex]);

                item.SetLock(PET_BAGGAGE_STATE_ICON.Call(petData[dataIndex]));

                if (string.IsNullOrEmpty(selectedRid))
                {
                    // 最开始默认选中第一个
                    if (dataIndex == 0)
                        ItemSelected(petWnd);
                }
                else
                {
                    if (petData[dataIndex].GetRid().Equals(selectedRid))
                        item.SetSelected(true, false);
                    else
                        item.SetSelected(false, false);
                }
            }
            else
            {
                item.SetLock("");
                item.SetSelected(false, false);
                item.SetBind(null);
                item.SetIcon(null);
            }
        }
    }

    /// <summary>
    /// 设置包裹格子选中
    /// </summary>
    private void ItemSelected(GameObject ob)
    {
        // 当前包裹无宠物
        if (petData.Count == 0 && ob == null)
        {
            mPetToolTip.GetComponent<OthersPetToolTipWnd>().SetBind(null, string.Empty);
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
            foreach (Transform child in petcontent.transform)
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

        item.SetSelected(true, false);

        // 刷新显示pettooltip
        mPetToolTip.GetComponent<OthersPetToolTipWnd>().SetBind(item_ob, sharePet);

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
            return;

        // 如果点击的是已标记宠物，不响应
        if (selectedRid.Equals(ob.GetComponent<PetItemWnd>().item_ob.GetRid()))
            return;

        ItemSelected(ob);
    }

    /// <summary>
    /// 防守阵容查看按钮点击事件
    /// </summary>
    void OnClickViewDefenceBtn(GameObject go)
    {
        // 创建防守宠物查看界面
        GameObject wnd = WindowMgr.OpenWnd(DefenceDeployViewWnd.WndType);

        // 窗口创建失败
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<DefenceDeployViewWnd>().Bind(mDefenceList, mUser.GetName());
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(Property user)
    {
        // 创建玩家对象
        mUser = user;
        if (mUser == null)
            return;

        // 重置分享宠物
        sharePet = string.Empty;

        // 监听玩家详细信息事件
        EventMgr.RegisterEvent("VisitBaggageWnd", EventMgrEventType.EVENT_SEARCH_BAGGAGE_INFO_SUCC, SetUserData);

        // 显示玩家的等级和姓名
        mUserNameLb.text = string.Format(LocalizationMgr.Get("ViewUserWnd_1"), mUser.GetLevel(), mUser.GetName());

        // 加载玩家头像
        LPCValue iconValue = mUser.Query<LPCValue>("icon");
        if (iconValue != null && iconValue.IsString)
            mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconValue.AsString));
        else
            mIcon.mainTexture = null;

        for (int i = 0; i < mStars.Length; i++)
        {
            mStars[i].spriteName = "arena_star_bg";
            mStars[i].gameObject.SetActive(true);
        }

        mRankIcon.spriteName = "ordinary_icon";

        // 获取玩家竞技场排名数据
        LPCValue arenaTop = mUser.Query<LPCValue>("arena_top");
        if (arenaTop != null && arenaTop.IsMapping)
        {
            // 获取竞技场阶位
            int step = ArenaMgr.GetStepByScoreAndRank(arenaTop.AsMapping.GetValue<int>("rank"), arenaTop.AsMapping.GetValue<int>("score"));
            // 获取配置表数据
            CsvRow row = ArenaMgr.TopBonusCsv.FindByKey(step);

            if (row == null)
                return;

            mRankIcon.spriteName = row.Query<string>("rank_icon");

            // 星级
            int star = row.Query<int>("star");

            for (int i = 0; i < star; i++)
                mStars[i].spriteName = row.Query<string>("star_name");
        }

        // 向服务器请求包裹数据
        Operation.CmdSearchBaggageInfo.Go(DomainAddress.GenerateDomainAddress("c@" + mUser.GetRid(), "u", 0));
    }

    #endregion
}
