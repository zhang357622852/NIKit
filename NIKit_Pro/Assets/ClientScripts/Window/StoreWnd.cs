
/// <summary>
/// StoreWnd.cs
/// Created by lic 2016-6-20
/// 仓库界面
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class StoreWnd : WindowBase<StoreWnd>
{
    #region 成员变量

    public UILabel mTitle;

    public UILabel mTip;

    public GameObject mCloseBtn;

    public GameObject[] mSortBtn;

    public UISprite mStoreBtn;

    public UISprite mTakeBtn;

    public UILabel mSortLevelLb;
    public UILabel mSortStarLb;
    public UILabel mSortAttributeLb;
    public UILabel mSortLaterstLb;

    public UIScrollView mBaggScro;
    public UIScrollView mStoreScro;

    public Transform mBaggTf;
    public Transform mStoreTf;

    public GameObject mPetItem;

    public TweenScale mTweenScale;

    #endregion

    #region 私有变量

    // 预创格子行数(包裹与仓库一致)
    const int mRowNum = 7;

    // 格子列数(包裹与仓库一致)
    const int mColumnNum = 4;

    int mSelectSort = 0;

    List<string> mBagSelects = new List<string>();

    List<string> mStoreSelects = new List<string>();

    List<Property> mBagData = new List<Property>();

    List<Property> mStoreData = new List<Property>();

    // 当前显示数据的index与实际数据的对应关系
    Dictionary<int, int> bagIndexMap = new Dictionary<int, int>();

    // 当前显示数据的index与实际数据的对应关系
    Dictionary<int, int> storeIndexMap = new Dictionary<int, int>();

    // 起始位置
    Dictionary<GameObject, Vector3> bagRePosition = new Dictionary<GameObject, Vector3>();

    // 起始位置
    Dictionary<GameObject, Vector3> storeRePosition = new Dictionary<GameObject, Vector3>();

    Dictionary<string, GameObject> petObMap = new Dictionary<string, GameObject>();

    // 当前容量
    int bagContainerSize = 0;

    int storeContainerSize = 0;

    // 悬浮面板宠物rid
    string pressRid = string.Empty;

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        //初始化窗口
        InitWnd();

        // 创建格子
        CreatePos();

        // 初始化窗口
        InitBagData();

        // 初始化数据
        InitStoreData();

        // 刷新存入，取出按钮
        RedrawBtn();
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    void OnDestroy()
    {
        // 取消注册玩家仓库变化事件
        EventMgr.UnregisterEvent("StoreWnd");

        // 玩家对象不存在
        if (ME.user != null)
        {
            // 移除属性字段关注回调
            ME.user.dbase.RemoveTriggerField("StoreWnd");

            // 取消注册玩家包裹事件
            ME.user.baggage.eventCarryChange -= BaggageChange;
        }

        petObMap.Clear ();
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        // 本地化文字
        mSortLevelLb.text = LocalizationMgr.Get("ShowPetsWnd_1");
        mSortStarLb.text = LocalizationMgr.Get("ShowPetsWnd_2");
        mSortAttributeLb.text = LocalizationMgr.Get("ShowPetsWnd_3");
        mSortLaterstLb.text = LocalizationMgr.Get("ShowPetsWnd_4");
        mTip.text = LocalizationMgr.Get("StoreWnd_2");
        mTitle.text = LocalizationMgr.Get("StoreWnd_1");

        // 设置初始排序方式
        mSelectSort = BaggageMgr.GetMonsterSortType() >= mSortBtn.Length ?
            0 : BaggageMgr.GetMonsterSortType();

        mSortBtn[mSelectSort].GetComponent<UIToggle>().Set(true);

        mPetItem.SetActive (false);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        // 注册玩家装备道具事件
        ME.user.baggage.eventCarryChange += BaggageChange;

        mBaggTf.GetComponent<UIWrapContent>().onInitializeItem = OnBagUpdateItem;

        mStoreTf.GetComponent<UIWrapContent>().onInitializeItem = OnStoreUpdateItem;

        foreach (GameObject btn in mSortBtn)
            UIEventListener.Get(btn).onClick = OnSortBtn;

        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;
        UIEventListener.Get(mStoreBtn.gameObject).onClick = OnStoreBtn;
        UIEventListener.Get(mTakeBtn.gameObject).onClick = OnTakeBtn;

        // 关注包裹格子变化(包括仓库page和宠物page)
        ME.user.dbase.RegisterTriggerField("StoreWnd", new string[] { "container_size" }, new CallBack(OnBagContainerSizeChange));

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// 动画播放回调
    /// </summary>
    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 创建宠物格子
    /// </summary>
    void CreatePos()
    {
        //只生成这么多格子，动态复用
        for (int i = 0; i < mRowNum; i++)
        {
            GameObject baggageItem = CreateColumnItem(string.Format("storewnd_baggage_item_{0}",
                                             i), mBaggTf, i);

            GameObject storeItem = CreateColumnItem(string.Format("storewnd_store_item_{0}", i),
                                       mStoreTf, i);

            // 记录原始位置
            bagRePosition.Add(baggageItem, baggageItem.transform.localPosition);

            storeRePosition.Add(storeItem, storeItem.transform.localPosition);

            for (int j = 0; j < mColumnNum; j++)
            {
                GameObject baggagePet = CreatePetItem(string.Format("storewnd_baggage_pet_{0}_{1}", i, j),
                                            baggageItem.transform, j);

                GameObject storePet = CreatePetItem(string.Format("storewnd_store_pet_{0}_{1}", i, j),
                                          storeItem.transform, j);

                // 注册包裹宠物格子点击事件
                UIEventListener.Get(baggagePet).onClick = OnBaggageItem;

                // 注册仓库宠物格子点击事件
                UIEventListener.Get(storePet).onClick = OnStoreItem;

                // 注册长按事件
                UIEventListener.Get(baggagePet).onPress = OnPressShowPetInfo;
                UIEventListener.Get(storePet).onPress = OnPressShowPetInfo;
            }

        }
    }

    /// <summary>
    /// 创建行item
    /// </summary>
    /// <param name="name">Name.</param>
    /// <param name="tf">Tf.</param>
    /// <param name="index">Index.</param>
    GameObject CreateColumnItem(string name, Transform tf, int index)
    {
        GameObject ob = new GameObject();
        ob.name = name;
        ob.transform.parent = tf;
        ob.transform.localPosition = new Vector3(0, -index * 115, 0);
        ob.transform.localScale = Vector3.one;

        ob.SetActive(true);

        return ob;
    }

    /// <summary>
    /// 创建行item
    /// </summary>
    /// <param name="name">Name.</param>
    /// <param name="tf">Tf.</param>
    /// <param name="index">Index.</param>
    GameObject CreatePetItem(string name, Transform tf, int index)
    {
        GameObject item = Instantiate (mPetItem) as GameObject;
        item.transform.parent = tf;
        item.name = name;
        item.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        item.transform.localPosition = new Vector3(114f * index, 0f, 0f);
        item.SetActive(true);

        petObMap.Add (name, item);

        return item;
    }

    /// <summary>
    /// Inits the baggage data.
    /// </summary>
    void InitBagData()
    {
        mBagData = BaggageMgr.GetItemsByPage(ME.user, ContainerConfig.POS_PET_GROUP);

        // 对宠物排序
        mBagData = BaggageMgr.SortPetInBag(mBagData, BaggageMgr.GetMonsterSortType ());

        int Row = mRowNum;

        bagContainerSize = ME.user.baggage.ContainerSize[ContainerConfig.POS_PET_GROUP].AsInt;

        // 此处包裹中的东西数量有可能比包裹容量大
        if (mBagData.Count > bagContainerSize)
            bagContainerSize = mBagData.Count;

        int maxSize = GameSettingMgr.GetSettingInt("max_pet_baggage_size");

        int containerRow = bagContainerSize % mColumnNum == 0 ?
            bagContainerSize / mColumnNum : bagContainerSize / mColumnNum + 1;

        // 多显示一行用来显示添加格子按钮
        if (containerRow >= mRowNum)
            Row = containerRow + 1;

        int maxRow = maxSize % mColumnNum == 0 ?
            maxSize / mColumnNum : maxSize / mColumnNum + 1;

        // 已达到最大格子数量，不显示添加格子按钮
        if (containerRow >= maxRow)
            Row = containerRow;

        // 从0开始
        mBaggTf.GetComponent<UIWrapContent>().maxIndex = 0;

        mBaggTf.GetComponent<UIWrapContent>().minIndex = -(Row - 1);
    }

    /// <summary>
    /// Inits the store data.
    /// </summary>
    void InitStoreData()
    {       
        mStoreTf.GetComponent<UIWrapContent>().enabled = true;

        mStoreData = BaggageMgr.GetItemsByPage(ME.user, ContainerConfig.POS_STORE_GROUP);

        // 对宠物排序
        mStoreData = BaggageMgr.SortPetInBag(mStoreData, BaggageMgr.GetMonsterSortType ());

        int Row = mRowNum;

        storeContainerSize = ME.user.baggage.ContainerSize[ContainerConfig.POS_STORE_GROUP].AsInt;

        // 仓库容量可能比仓库内的宠物数量少
        if (storeContainerSize < mStoreData.Count)
            storeContainerSize = mStoreData.Count;

        int maxSize = GameSettingMgr.GetSettingInt("max_store_baggage_size");

        int containerRow = storeContainerSize % mColumnNum == 0 ?
            storeContainerSize / mColumnNum : storeContainerSize / mColumnNum + 1;

        // 多显示一行用来显示添加格子按钮
        if (containerRow >= mRowNum)
            Row = containerRow + 1;

        int maxRow = maxSize % mColumnNum == 0 ?
            maxSize / mColumnNum : maxSize / mColumnNum + 1;

        // 已达到最大格子数量，不显示添加格子按钮
        if (containerRow >= maxRow)
            Row = containerRow;

        // 从0开始
        mStoreTf.GetComponent<UIWrapContent>().maxIndex = 0;

        mStoreTf.GetComponent<UIWrapContent>().minIndex = -(Row - 1);
    }

    /// <summary>
    ///设置滑动列表时复用宠物格子更改数据
    /// </summary>
    void OnBagUpdateItem(GameObject go, int index, int realIndex)
    {
        // 将index与realindex对应关系记录下来
        if (!bagIndexMap.ContainsKey(index))
            bagIndexMap.Add(index, realIndex);
        else
            bagIndexMap[index] = realIndex;

        FillBagData(index, realIndex);
    }

    /// <summary>
    ///设置滑动列表时复用宠物格子更改数据
    /// </summary>
    void OnStoreUpdateItem(GameObject go, int index, int realIndex)
    {
        // 将index与realindex对应关系记录下来
        if (!storeIndexMap.ContainsKey(index))
            storeIndexMap.Add(index, realIndex);
        else
            storeIndexMap[index] = realIndex;

        FillStoreData(index, realIndex);
    }

    /// <summary>
    /// 填充包裹数据
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="realIndex">Real index.</param>
    void FillBagData(int index, int realIndex)
    {
        for (int i = 0; i < mColumnNum; i++)
        {
            GameObject wnd = petObMap [string.Format ("storewnd_baggage_pet_{0}_{1}", index, i)];

            if (wnd == null)
                continue;

            PetItemWnd item = wnd.GetComponent<PetItemWnd>();

            item.SetSelected(false);

            int dataIndex = System.Math.Abs(realIndex) * mColumnNum + i;

            if (dataIndex < mBagData.Count)
            {
                item.SetBind(mBagData[dataIndex]);

                item.SetLock(STORE_PET_SHOW_ICON.Call(item.item_ob), STORE_PET_SHOW_ICON_COVER.Call(item.item_ob));

                foreach (string rid in mBagSelects)
                {
                    if (mBagData[dataIndex].GetRid().Equals(rid))
                    {
                        item.SetSelected(true);
                        break;
                    }
                }
            }
            else if (mBagData.Count < bagContainerSize && dataIndex < bagContainerSize)
            {
                item.SetLock("");
                item.SetBind(null);
                item.SetIcon(null);
            }
            else
            {
                item.SetLock("");
                item.SetBind(null);
                item.SetIcon("addpet");
            }
        }
    }

    /// <summary>
    /// 填充仓库数据
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="realIndex">Real index.</param>
    void FillStoreData(int index, int realIndex)
    {
        for (int i = 0; i < mColumnNum; i++)
        {
            GameObject wnd = petObMap [string.Format("storewnd_store_pet_{0}_{1}", index, i)];

            if (wnd == null)
                continue;

            PetItemWnd item = wnd.GetComponent<PetItemWnd>();

            item.SetSelected(false);

            int dataIndex = System.Math.Abs(realIndex) * mColumnNum + i;

            if (dataIndex < mStoreData.Count)
            {
                item.SetBind(mStoreData[dataIndex]);

                item.SetLock(TAKE_PET_SHOW_ICON.Call(item.item_ob));

                foreach (string rid in mStoreSelects)
                {
                    if (mStoreData[dataIndex].GetRid().Equals(rid))
                    {
                        item.SetSelected(true);
                        break;
                    }
                }
            }
            else if (mStoreData.Count < storeContainerSize && dataIndex < storeContainerSize)
            {
                item.SetLock("");
                item.SetBind(null);
                item.SetIcon(null);
            }
            else
            {
                item.SetLock("");
                item.SetBind(null);
                item.SetIcon("addpet");
            }
        }
    }

    /// <summary>
    /// 玩家包裹变化回调
    /// </summary>
    /// <param name="pos">Position.</param>
    void BaggageChange(string[] pos)
    {
        // 判断是否是宠物
        for (int i = 0; i < pos.Length; i++)
        {
            if (ContainerConfig.IS_PET_POS(pos[i]))
                RedrawBaggage();

            if (ContainerConfig.IS_STORE_POS(pos[i]))
                RedrawStore();
        }
    }

    /// <summary>
    /// 刷新按钮
    /// </summary>
    void RedrawBtn()
    {
        mStoreBtn.alpha = mBagSelects.Count == 0 ?
            0.3f : 1.0f;

        mTakeBtn.alpha = mStoreSelects.Count == 0 ?
            0.3f : 1.0f;
    }

    /// <summary>
    /// 包裹格子数量变化回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="param">Parameter.</param>
    void OnBagContainerSizeChange(object para, params object[] param)
    {
        // 刷新界面
        RedrawBaggage();

        // 刷新仓库变化
        RedrawStore();
    }

    /// <summary>
    /// 刷新包裹
    /// </summary>
    void RedrawBaggage()
    {
        InitBagData();

        // 填充数据
        foreach (KeyValuePair<int, int> kv in bagIndexMap)
            FillBagData(kv.Key, kv.Value);
    }

    /// <summary>
    /// 刷新仓库
    /// </summary>
    void RedrawStore()
    {
        InitStoreData();

        // 填充数据
        foreach (KeyValuePair<int, int> kv in storeIndexMap)
        {
            FillStoreData(kv.Key, kv.Value);
        }
    }

    /// <summary>
    /// 包裹格子被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnBaggageItem(GameObject ob)
    {
        // 取得gameobject上绑定的item
        PetItemWnd item = ob.GetComponent<PetItemWnd>();

        if (item.item_ob == null)
        {
            // 尝试升级宠物页面
            BaggageMgr.TryUpgradeBaggage(ME.user, ContainerConfig.POS_PET_GROUP);
            return;
        }

        string desc = CAN_STORE_PET_DESC.Call(item.item_ob);

        // 检测该魔灵能否作为存入仓库
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

        // 当前格子已经被选中
        if (item.isSelected)
        {
            mBagSelects.Remove(item.item_ob.GetRid());
            item.SetSelected(false);
            RedrawBtn();
            return;
        }

        // 检测仓库格子数量
        if (ContainerMgr.GetFreePosCount(ME.user as Container, ContainerConfig.POS_STORE_GROUP) < mBagSelects.Count + 1)
        {
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("StoreWnd_3"),
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
            return;
        }
           
        mBagSelects.Add(item.item_ob.GetRid());
        item.SetSelected(true);
        RedrawBtn();
    }

    /// <summary>
    /// 仓库格子被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnStoreItem(GameObject ob)
    {
        // 取得gameobject上绑定的item
        PetItemWnd item = ob.GetComponent<PetItemWnd>();

        if (item.item_ob == null)
        {
            // 尝试升级宠物页面
            BaggageMgr.TryUpgradeBaggage(ME.user, ContainerConfig.POS_STORE_GROUP);

            return;
        }

        // 当前格子已经被选中
        if (item.isSelected)
        {
            mStoreSelects.Remove(item.item_ob.GetRid());
            item.SetSelected(false);
            RedrawBtn();
            return;
        }

        if (ContainerMgr.GetFreePosCount(ME.user as Container, ContainerConfig.POS_PET_GROUP) < mStoreSelects.Count + 1)
        {
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("StoreWnd_4"),
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
            return;
        }

        mStoreSelects.Add(item.item_ob.GetRid());
        item.SetSelected(true);
        RedrawBtn();
    }

    /// <summary>
    ///长按显示宠物信息
    /// </summary>
    private void OnPressShowPetInfo(GameObject go, bool isPress)
    {
        //玩家正在滑动宠物列表;
        if (mBaggScro.isDragging)
            return;

        if (mStoreScro.isDragging)
            return;

        //手指抬起时
        if (!isPress)
        {
            pressRid = string.Empty;

            return;
        }

        // 没有宠物;
        if (go.GetComponent<PetItemWnd>().item_ob == null)
            return;

        pressRid = go.GetComponent<PetItemWnd>().item_ob.GetRid();

        CancelInvoke("ShowPetInfo");

        // 0.5秒后显示宠物信息界面;
        Invoke("ShowPetInfo", 0.4f);
    }

    /// <summary>
    ///显示宠物信息
    /// </summary>
    void ShowPetInfo()
    {
        //玩家正在滑动宠物列表;
        if (mBaggScro.isDragging)
            return;

        if (mStoreScro.isDragging)
            return;

        // 没有选择对象
        if (string.IsNullOrEmpty(pressRid))
            return;

        GameObject wnd = WindowMgr.OpenWnd("PetInfoWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return;

        wnd.GetComponent<PetInfoWnd>().Bind(pressRid, ME.user.GetName(), ME.user.GetLevel());

    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        WindowMgr.OpenWnd ("MainWnd");

        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 取出仓库按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnTakeBtn(GameObject ob)
    {
        // 执行take操作
        if (!BaggageMgr.StoreToBaggage(ME.user, ContainerConfig.POS_PET_GROUP, mStoreSelects))
            return;

        // 清空数据重绘按钮
        mStoreSelects.Clear();
        RedrawBtn();
    }

    /// <summary>
    /// 存入仓库按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnStoreBtn(GameObject ob)
    {
        // 执行store操作
        if (!BaggageMgr.BaggageToStore(ME.user, mBagSelects))
            return;

        // 清空数据重绘按钮
        mBagSelects.Clear();
        RedrawBtn();
    }

    /// <summary>
    /// 排序方式被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnSortBtn(GameObject ob)
    {
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

            RedrawBaggage();

            RedrawStore();

            break;
        }

        mSortBtn[mSelectSort].GetComponent<UIToggle>().Set(true);
    }

    #endregion

}
