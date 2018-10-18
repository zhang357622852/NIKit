/// <summary>
/// EquipWnd.cs
/// Created by lic 7/5/2016
/// 宠物装备界面
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class EquipWnd : WindowBase<EquipWnd>
{
    #region 公共字段

    public Vector2 mInitPos = new Vector2(0, 0);
    public Vector2 mItemSpace = new Vector2(15, 15);
    public Vector2 mItemSize = new Vector2(89, 89);

    // 每几个item为一行
    public int mColumnNum = 4;

    // 创建格子的列数
    public int mRowNum = 4;

    public GameObject Container;
    public UIScrollView ScrollView;
    public UIWrapContent petcontent;

    // 本地化文字
    public UILabel mBatchSell;
    public UILabel mMultipleSell;
    public UILabel mEquipFilter;

    public GameObject mBatchSellBtn;
    public GameObject mMultipleSellBtn;
    public GameObject EquipFilterBtn;

    public UISprite mEquipFilterBg;

    // 筛选
    public UILabel mEquipNumber;

    public GameObject mEquipItem;

    public UISpriteAnimation mNewTips;

    public FilterWnd mFilterWnd;

    #endregion

    // 包裹格子的总数量
    private int limitNumber = 0;

    // 玩家的未穿上的装备数据
    private List<Property> equipData = new List<Property>();

    // 当前的宠物对象
    private Property petItem_ob = null;

    private string mSelectRid = string.Empty;

    // 当前显示数据的index与实际数据的对应关系
    private Dictionary<int, int> indexMap = new Dictionary<int, int>();

    // 起始位置
    private Dictionary<GameObject,Vector3> rePosition = new Dictionary<GameObject,Vector3>();

    // 格子
    private Dictionary<string, GameObject> mPetPosObMap = new Dictionary<string, GameObject> ();

    // 勾选的装备列表
    private Dictionary<string, int> mEquipList = new Dictionary<string, int>();

    int EquipViewBtnState = 0;

    // 装备筛选条件
    private LPCMapping mCondition = LPCMapping.Empty;

    // 是否是筛选装备
    private bool mIsFilter = false;

    // Use this for initialization
    void Start()
    {
        //初始化窗口
        InitWnd();

        // 初始化窗口
        RedrawData();

        // 创建格子
        CreatePos();

        UIEventListener.Get(mBatchSellBtn).onClick += OnBatchSellClicked;
        UIEventListener.Get(mMultipleSellBtn).onClick += OnMultipleSellClicked;
        UIEventListener.Get(EquipFilterBtn).onClick += OnEquipFilterClicked;

        petcontent.onInitializeItem = OnUpdateItem;
    }

    void OnEnable()
    {
        // 注册事件
        RegisterEvent();

        // 检测新物品提示
        DoCheckNewTips();
    }

    void OnDisable()
    {
        // 解注册
        MsgMgr.RemoveDoneHook("MSG_SELL_ITEM", "EquipWnd");

        EventMgr.UnregisterEvent(gameObject.name);

        // 玩家不存在
        if (ME.user == null)
            return;

        // 取消注册玩家装备道具事件
        ME.user.baggage.eventCarryChange -= BaggageChange;
    }

    #region 内部方法

    /// <summary>
    /// 注册窗口事件
    /// </summary>
    private void RegisterEvent()
    {
        // 注册玩家装备道具事件
        ME.user.baggage.eventCarryChange += BaggageChange;

        EquipViewBtnState = 1;

        // 初始化按钮的状态
        SetBtnState(true);

        // 关注装备出售的消息MSG_SELL_ITEM
        MsgMgr.RegisterDoneHook("MSG_SELL_ITEM", "EquipWnd", OnMsgSellEquip);

        // 注册新物品信息被清除
        EventMgr.RegisterEvent(gameObject.name, EventMgrEventType.EVENT_CLEAR_NEW, ClearNewInfo);
        EventMgr.RegisterEvent(gameObject.name, EventMgrEventType.EVENT_CLOSE_EQUIP_STRENTHEN, OnCloseEquipStrengthenEvent);
    }

    void OnCloseEquipStrengthenEvent(int eventId, MixedValue para)
    {
        // 刷新窗口
        Redraw(false);
    }

    void ClearNewInfo(int eventId, MixedValue para)
    {
        // 检测新物品提示
        DoCheckNewTips();
    }

    /// <summary>
    /// 检测新物品提示
    /// </summary>
    void DoCheckNewTips()
    {
        if (ME.user == null)
        {
            mNewTips.gameObject.SetActive(false);
            return;
        }

        List<Property> equips = BaggageMgr.GetItemsByPage(ME.user, ContainerConfig.POS_ITEM_GROUP);

        if (BaggageMgr.HasNewItem(equips))
        {
            mNewTips.gameObject.SetActive(true);

            mNewTips.ResetToBeginning();
        }
        else
        {
            mNewTips.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 装备出售成功消息回调
    /// </summary>
    void OnMsgSellEquip(string cmd, LPCValue para)
    {
        SetBtnState(true);

        // 清空选择状态
        ClearSelectState();

        // 清空装备缓存列表
        mEquipList.Clear();

        EquipViewBtnState = 1;

        CheckItemIsAllEmpety();
    }

    /// <summary>
    /// 当前列表是否为空
    /// </summary>
    /// <returns><c>true</c>, if item is all empety was checked, <c>false</c> otherwise.</returns>
    private void CheckItemIsAllEmpety()
    {
        int minIndex = System.Math.Abs(indexMap[0]);

        foreach (int value in indexMap.Values)
        {
            if(System.Math.Abs(value) < minIndex)
                minIndex = System.Math.Abs(value);
        }

        if (minIndex * mColumnNum >= equipData.Count)
            Redraw(true);
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        // 本地化文字
        mEquipFilter.text = LocalizationMgr.Get("EquipWnd_4");
        mBatchSell.text = LocalizationMgr.Get("EquipWnd_2");
        mMultipleSell.text = LocalizationMgr.Get("EquipWnd_3");


        mEquipItem.SetActive (false);

        mNewTips.GetComponent<UISpriteAnimation>().namePrefix = ConfigMgr.IsCN ? "cnew" : "new";
    }

    /// <summary>
    /// 创建宠物格子
    /// </summary>
    private void CreatePos()
    {
        // 生成格子，只生成这么多格子，动态复用
        for(int i = 0; i < mRowNum; i++)
        {
            GameObject rowItemOb = new GameObject();
            rowItemOb.name = string.Format("item_{0}", i);
            rowItemOb.transform.parent = Container.transform;
            rowItemOb.transform.localPosition = new Vector3(mInitPos.x, mInitPos.y - i*(mItemSize.y + mItemSpace.y), 0);
            rowItemOb.transform.localScale = Vector3.one;

            rePosition.Add(rowItemOb, rowItemOb.transform.localPosition);

            for(int j = 0; j < mColumnNum; j++)
            {
                GameObject posWnd = Instantiate (mEquipItem) as GameObject;
                posWnd.transform.parent = rowItemOb.transform;
                posWnd.name = string.Format("equip_item_{0}_{1}", i, j);
                posWnd.transform.localScale = Vector3.one;
                posWnd.transform.localPosition = new Vector3((mItemSize.x + mItemSpace.x) * j, 0f, 0);

                posWnd.SetActive(true);

                mPetPosObMap.Add (string.Format ("equip_item_{0}_{1}", i, j), posWnd);

                // 注册点击事件
                UIEventListener.Get(posWnd).onClick = OnEquipItemClicked;
            }
        }
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    private void RedrawData()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 装备筛选
        if (mIsFilter)
        {
            if (mCondition == null)
                mCondition = LPCMapping.Empty;

            // 次要属性
            LPCArray minorProp = mCondition.GetValue<LPCArray>(EquipConst.MINOR_PROP);
            if (minorProp == null)
                minorProp = LPCArray.Empty;

            // 筛选装备
            equipData = BaggageMgr.GetItemsByCustom(ME.user, mCondition);

            int reverse = mCondition.GetValue<int>("reverse");

            // 对装备数据进行排序
            equipData = BaggageMgr.SortEquipsAttribInBag(equipData, minorProp);

            if (minorProp.Count == 1)
            {
                // 反转排序
                if (reverse == 1 && minorProp[0].AsInt != -1)
                    equipData.Reverse();
            }
            else if (minorProp.Count == 2)
            {
                // 反转排序
                if (reverse == 1 && (minorProp[0].AsInt != -1 || minorProp[1].AsInt != -1))
                    equipData.Reverse();
            }
        }
        else
        {
            // 取得当前所有的装备数据
            // 对装备数据进行排序
            equipData = BaggageMgr.SortEquipsInBag(BaggageMgr.GetItemsByPage(ME.user, ContainerConfig.POS_ITEM_GROUP));
        }

        // 取得玩家格子数量
        limitNumber = ME.user.baggage.ContainerSize[ContainerConfig.POS_ITEM_GROUP].AsInt;

        mEquipNumber.text = string.Format("{0}/{1}", equipData.Count, limitNumber);

        int maxSize = GameSettingMgr.GetSettingInt("max_equip_baggage_size");

        // 取得生成格子的总行数
        int rowNum = (equipData.Count%mColumnNum == 0) ? equipData.Count/mColumnNum:(equipData.Count/mColumnNum + 1);

        if(rowNum < mRowNum)
            rowNum = mRowNum;

        int maxRow = maxSize % mColumnNum == 0 ?
            maxSize / mColumnNum : maxSize / mColumnNum + 1;

        // 已达到最大格子数量，不显示添加格子按钮
        if (rowNum >= maxRow)
            rowNum = rowNum - 1;

        // 从0开始
        petcontent.maxIndex = 0;

        petcontent.minIndex = - rowNum;
    }

    /// <summary>
    /// 相应ScrowView拖动事件,PetData在此阶段数据不变，可以复用
    /// </summary>
    /// <param name="ob">Ob.</param>
    /// <param name="index">Index.</param>
    /// <param name="realindex">Realindex.</param>
    private void OnUpdateItem(GameObject ob, int index, int realindex)
    {
        // 将index与realindex对应关系记录下来
        if(!indexMap.ContainsKey(index))
            indexMap.Add(index, realindex);
        else
            indexMap[index] = realindex;

        FillData(index, realindex);
    }

    /// <summary>
    /// 装备格子被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnEquipItemClicked(GameObject ob)
    {
        // 取得gameobject上绑定的item
        EquipItemWnd item = ob.GetComponent<EquipItemWnd>();

        if (item.ItemOb == null)
            return;

        // 清除新物品标识
        BaggageMgr.ClearNewField(item.ItemOb);

        item.SetNewTips(item.ItemOb);

        if (item.IsSelected)
        {
            item.SetSelected(false);
            ResetEquipSelect();
            return;
        }

        // 标记当前已选中
        mSelectRid = item.ItemOb.GetRid();

        // 刷新选中
        RedrawSelect();

        GameObject wnd = WindowMgr.GetWindow(EquipViewWnd.WndType + "_UnEquip");

        if (wnd == null)
        {
            wnd = WindowMgr.CreateWindow("EquipViewWnd_UnEquip", EquipViewWnd.PrefebResource);

            if (wnd == null)
                return;

            WindowMgr.AddToOpenWndList(wnd, WindowOpenGroup.SINGLE_OPEN_WND);

            Vector3 pos = wnd.transform.localPosition;

            wnd.transform.localPosition = new Vector3(pos.x, -132, pos.z);

            GameObject bagWnd = WindowMgr.GetWindow(BaggageWnd.WndType);

            if (bagWnd != null)
                wnd.transform.SetParent(bagWnd.transform);
        }

        wnd.SetActive(true);
        EquipViewWnd evw = wnd.GetComponent<EquipViewWnd>();

        if (evw == null)
            return;

        string petRid = string.Empty;
        if (petItem_ob != null)
            petRid = petItem_ob.GetRid();

        evw.Bind(item.ItemOb.GetRid(), petRid, new CallBack(OnCloseEquipView));

        evw.SetState(EquipViewBtnState);
    }

    /// <summary>
    /// 刷新装备查看界面
    /// </summary>
    public void RefreshEquipViewWnd()
    {
        GameObject wnd = WindowMgr.GetWindow(EquipViewWnd.WndType + "_UnEquip");

        // 装备不存在或者装备隐藏的状态下不刷新
        if (wnd == null || !wnd.activeSelf)
            return;

        EquipViewWnd evw = wnd.GetComponent<EquipViewWnd>();
        if (evw == null)
            return;

        string petRid = string.Empty;

        if (petItem_ob != null)
            petRid = petItem_ob.GetRid();

        // 装备对象
        Property equipOb = evw.ob;

        // 装备对象不存在,重置装备选择
        if (equipOb == null)
        {
            ResetEquipSelect();

            return;
        }

        evw.Bind(equipOb.GetRid(), petRid, new CallBack(OnCloseEquipView));

        evw.SetState(EquipViewBtnState);
    }

    /// <summary>
    /// 关闭装备悬浮回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="_params">Parameters.</param>
    public void OnCloseEquipView(object para, params object[] _params)
    {
        // 当前窗口不存在了
        if (WindowMgr.GetWindow("BaggageWnd") == null)
            return;

        mSelectRid = string.Empty;

        RedrawSelect();
    }

    /// <summary>
    /// 包裹变化回调
    /// </summary>
    /// <param name="pos">Position.</param>
    void BaggageChange(string[] pos)
    {
        // 延迟刷新计时器
        MergeExecuteMgr.DispatchExecute(DoDelayedRefresh);
    }

    /// <summary>
    /// 执行延迟刷新处理
    /// </summary>
    void DoDelayedRefresh()
    {
        // 窗口没有显示，不处理
        if (gameObject == null ||
            ! gameObject.activeSelf ||
            ! gameObject.activeInHierarchy)
            return;

        // 检查new标识
        DoCheckNewTips();

        // 重置mSelectRid
        mSelectRid = string.Empty;

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 装备批量出售格子被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnBatchSellClicked(GameObject ob)
    {
        // 重置选中状态
        ResetEquipSelect();

        RedrawSelect();

        WindowMgr.OpenWnd(EquipBatchSellWnd.WndType, WindowMgr.GetWindow(BaggageWnd.WndType).transform, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 装备多选出售按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnMultipleSellClicked(GameObject ob)
    {
        GameObject baggage = WindowMgr.GetWindow("BaggageWnd");

        // 包裹界面不存在
        if (baggage == null)
            return;

        // 打开多选出售窗口
        GameObject wnd = WindowMgr.OpenWnd(MultipleSellWnd.WndType, baggage.transform, WindowOpenGroup.SINGLE_OPEN_WND);

        // 打开窗口失败
        if (wnd == null)
            return;

        MultipleSellWnd script = wnd.GetComponent<MultipleSellWnd>();

        // 获取脚本失败
        if (script == null)
            return;

        script.SetCallBack(new CallBack(OnCloseCallBack));

        // 检查new标识
        DoCheckNewTips();

        // 设置按钮的状态
        SetBtnState(false);
    }

    /// <summary>
    /// MultipleSellWnd窗口关闭回调
    /// </summary>
    void OnCloseCallBack(object para, params object[] param)
    {
        // 刷新数据
        foreach (int key in indexMap.Keys)
            FillData(key, indexMap[key]);

        // 检查new标识
        DoCheckNewTips();
    }

    /// <summary>
    /// 装备筛选按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
     void OnEquipFilterClicked(GameObject ob)
    {
        // 重置选中状态
        ResetEquipSelect();

        RedrawSelect();

        EquipViewBtnState = 1;

        // 打开装备筛选窗口
        if (mFilterWnd.gameObject.activeSelf)
        {
            mFilterWnd.gameObject.SetActive(false);

            mFilterWnd.mIsCurClose = true;

            mEquipFilterBg.color = new Color(1.0f, 1.0f, 1.0f);

            OnFliterWndCloseCallBack(null);
        }
        else
        {
            float rgb = 141.0f / 255;

            mEquipFilterBg.color = new Color(rgb, rgb, rgb);

            mFilterWnd.gameObject.SetActive(true);

            // 绑定数据
            mFilterWnd.Bind(new CallBack(OnRefreshCondition), new CallBack(OnFliterWndCloseCallBack));

            // 主动请求刷新
            mFilterWnd.DoConditionCallBack();
        }
    }

    /// <summary>
    /// 刷新筛选条件回调
    /// </summary>
    void OnRefreshCondition(object para, params object[] param)
    {
        // 筛选条件
        mCondition = param[0] as LPCMapping;

        // 重置mSelectRid
        mSelectRid = string.Empty;

        // 标识装备筛选
        mIsFilter = true;

        // 刷新装备选择
        ResetEquipSelect();

        // 重绘窗口
        Redraw(true);
    }

    /// <summary>
    /// 装备筛选关闭回调
    /// </summary>
    void OnFliterWndCloseCallBack(object para, params object[] param)
    {
        mIsFilter = false;

        // 重置mSelectRid
        mSelectRid = string.Empty;

        mEquipFilterBg.color = new Color(1.0f, 1.0f, 1.0f);

        // 刷新装备选择
        ResetEquipSelect();

        // 重绘窗口
        Redraw(true);
    }

    /// <summary>
    /// 点击多选出售时设置按钮的状态
    /// </summary>
    void SetBtnState(bool isCancel)
    {
        GameObject wnd = WindowMgr.GetWindow(EquipViewWnd.WndType + "_UnEquip");
        if (isCancel)
        {
            if (wnd != null)
                wnd.GetComponent<EquipViewWnd>().SetState(EquipViewBtnState);
        }
        else
        {
            if (wnd != null)
                wnd.GetComponent<EquipViewWnd>().SetState(EquipViewBtnState);
        }
    }

    /// <summary>
    /// 自定义选择回调
    /// </summary>
    void CustomSelectedHook(object para, object[] expara)
    {
        Redraw(true);
    }

    /// <summary>
    /// 填充数据
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="realIndex">Real index.</param>
    private void FillData(int index, int realIndex)
    {
        for(int i = 0 ; i < mColumnNum; i++)
        {
            GameObject equipWnd = mPetPosObMap[string.Format("equip_item_{0}_{1}", index, i)];

            if (equipWnd == null)
                continue;

            EquipItemWnd item = equipWnd.GetComponent<EquipItemWnd>();

            if (item == null)
                continue;

            int dataIndex =  System.Math.Abs(realIndex)*mColumnNum + i;

            if (dataIndex < equipData.Count)
            {
                item.SetBind(equipData[dataIndex]);

                item.SetNewTips(equipData[dataIndex]);

                if (equipData[dataIndex].GetRid().Equals(mSelectRid))
                    item.SetSelected(true);
                else
                    item.SetSelected(false);

                if (mEquipList.ContainsKey(equipData[dataIndex].GetRid()))
                    item.SetCheck(true);
                else
                    item.SetCheck(false);
            }
            else
            {
                item.SetSelected(false);
                item.SetCheck(false);
                item.SetBind(null);
                item.SetNewTips(null);
            }
        }
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw(bool resetPosition = false)
    {
        RedrawData();
        if (resetPosition)
        {
            foreach (GameObject item in rePosition.Keys)
            {
                item.transform.localPosition = rePosition[item];
            }

            // 整理位置, 回到初始位置
            ScrollView.transform.localPosition = new Vector3(-5, -33, 0);

            ScrollView.panel.clipOffset = Vector2.zero;

            if (indexMap != null)
                indexMap.Clear();

            for(int i = 0; i < mRowNum; i++)
            {
                indexMap.Add(i, -i);
            }
        }

        // 填充数据
        foreach (KeyValuePair<int, int> kv in indexMap)
        {
            FillData(kv.Key, kv.Value);
        }
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void RedrawSelect()
    {
        foreach (KeyValuePair<int, int> kv in indexMap)
        {
            for(int i = 0 ; i < mColumnNum; i++)
            {
                GameObject equipWnd = mPetPosObMap[string.Format("equip_item_{0}_{1}", kv.Key, i)];

                if (equipWnd == null)
                    continue;

                EquipItemWnd item = equipWnd.GetComponent<EquipItemWnd>();

                if (item == null)
                    continue;

                int dataIndex =  System.Math.Abs(kv.Value)*mColumnNum + i;

                if (dataIndex < equipData.Count)
                {
                    if (equipData[dataIndex].GetRid().Equals(mSelectRid))
                        item.SetSelected(true);
                    else
                        item.SetSelected(false);

                    if (mEquipList.ContainsKey(equipData[dataIndex].GetRid()))
                        item.SetCheck(true);
                    else
                        item.SetCheck(false);
                }
                else
                {
                    item.SetSelected(false);
                    item.SetCheck(false);
                }

            }
        }
    }

    /// <summary>
    /// 清空所有的状态
    /// </summary>
    private void ClearSelectState()
    {
        foreach (KeyValuePair<int, int> kv in indexMap)
        {
            for (int i = 0 ; i < mColumnNum; i++)
            {
                GameObject equipWnd = mPetPosObMap[string.Format("equip_item_{0}_{1}", kv.Key, i)];

                if (equipWnd == null)
                    continue;

                EquipItemWnd item = equipWnd.GetComponent<EquipItemWnd>();

                item.SetCheck(false);

                item.SetSelected(false);
            }
        }
    }

    /// <summary>
    /// 重置当前装备选中状态
    /// </summary>
    void ResetEquipSelect()
    {
        mSelectRid = string.Empty;

        // 关闭悬浮窗口
        GameObject equipViewWnd = WindowMgr.GetWindow("EquipViewWnd_UnEquip");

        if (equipViewWnd != null && equipViewWnd.activeInHierarchy)
            WindowMgr.HideWindow(equipViewWnd);
    }

    #endregion


    /// <summary>
    /// 设置当前宠物对象
    /// </summary>
    public void SetBind(Property ob, bool isRedraw = true)
    {
        // 重置绑定对象
        petItem_ob = ob;

        if (isRedraw)
        {
            // 刷新数据
            ResetEquipSelect();

            Redraw();
        }
    }

    /// <summary>
    /// 指引点击装备格子
    /// </summary>
    public void GuideClickEquipItem(string equipItemName)
    {
        GameObject item = null;

        if (! mPetPosObMap.TryGetValue(equipItemName, out item))
            return;

        OnEquipItemClicked(item);
    }
}
