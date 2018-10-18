/// <summary>
/// MultipleSellWnd.cs
/// Created by fengsc 2018/08/22
/// 装备多选出售窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class MultipleSellWnd : WindowBase<MultipleSellWnd>
{
    #region 成员变量

    public Vector2 mInitPos = new Vector2(0, 0);
    public Vector2 mItemSpace = new Vector2(15, 15);
    public Vector2 mItemSize = new Vector2(89, 89);

    // 窗口标题
    public UILabel mTitle;

    // 关闭按钮
    public GameObject mCloseBtn;

    // 选择提示
    public UILabel mSelectTips;

    // 选择数量
    public UILabel mSelectAmount;

    // 全选按钮
    public GameObject mAllSelectBtn;
    public UILabel mAllSelectLb;

    // 全选提示
    public UILabel mAllSelectTips;

    // 智能全选按钮
    public GameObject mIntelligentSelectBtn;
    public UILabel mIntelligentSelectLb;

    // 出售按钮
    public GameObject mSellBtn;
    public UILabel mSellLb;

    // 装备基础格子
    public GameObject mEquipItem;

    public UIWrapContent mUIWrapContent;

    public UIScrollView ScrollView;

    // 每几个item为一行
    public int mColumnNum = 5;

    // 创建格子的列数
    public int mRowNum = 7;

    // 筛选窗口
    public GameObject mFilterWnd;

    public TweenScale mTweenScale;

    public TweenAlpha mTweenAlpha;

    private LPCMapping mCondition;

    // 起始位置
    private Dictionary<GameObject,Vector3> rePosition = new Dictionary<GameObject,Vector3>();

    // 格子
    private Dictionary<string, GameObject> mPetPosObMap = new Dictionary<string, GameObject>();

    // 当前显示数据的index与实际数据的对应关系
    private Dictionary<int, int> indexMap = new Dictionary<int, int>();

    // 玩家的未穿上的装备数据
    private List<Property> equipData = new List<Property>();

    // 装备选择列表
    private List<string> mSelectRid = new List<string>();

    private string mCurViewRid = string.Empty;

    private CallBack mCb;

    #endregion

    // Use this for initialization
    void Awake ()
    {
        // 初始化本地化文本
        InitLable();

        CreatePos();

        // 注册事件
        RegisterEvent();
    }

    void OnEnable()
    {
        // 重新播放缩放动画
        mTweenScale.enabled = true;
        mTweenScale.ResetToBeginning();

        // 重新播放渐变动画
        mTweenAlpha.enabled = true;
        mTweenAlpha.ResetToBeginning();

        // 绑定数据
        mFilterWnd.GetComponent<FilterWnd>().Bind(new CallBack(RefreshFilterEquip), null);

        // 执行条件回调，主动要求刷新
        mFilterWnd.GetComponent<FilterWnd>().DoConditionCallBack();

        // 关注装备出售的消息MSG_SELL_ITEM
        MsgMgr.RegisterDoneHook("MSG_SELL_ITEM", "MultipleSellWnd", OnMsgSellEquip);

        // 玩家不存在
        if (ME.user == null)
            return;

        // 取消注册玩家装备道具事件
        ME.user.baggage.eventCarryChange += BaggageChange;
    }

    void OnDisable()
    {
        // 移除窗口缓存
        WindowMgr.RemoveOpenWnd(this.gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        MsgMgr.RemoveDoneHook("MSG_SELL_ITEM", "MultipleSellWnd");

        // 玩家不存在
        if (ME.user == null)
            return;

        // 注册玩家装备道具事件
        ME.user.baggage.eventCarryChange -= BaggageChange;
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLable()
    {
        mTitle.text = LocalizationMgr.Get("MultipleSellWnd_0");
        mSelectTips.text = LocalizationMgr.Get("MultipleSellWnd_1");
        mAllSelectLb.text = LocalizationMgr.Get("MultipleSellWnd_3");
        mAllSelectTips.text = LocalizationMgr.Get("MultipleSellWnd_4");
        mIntelligentSelectLb.text = LocalizationMgr.Get("MultipleSellWnd_5");
        mSellLb.text = LocalizationMgr.Get("MultipleSellWnd_6");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mAllSelectBtn).onClick = OnClickAllSelectBtn;
        UIEventListener.Get(mIntelligentSelectBtn).onClick = OnClickIntelligentSelectBtn;
        UIEventListener.Get(mSellBtn).onClick = OnClickSellBtn;

        mUIWrapContent.onInitializeItem = OnUpdateItem;

        float scale = Game.CalcWndScale();

        mTweenScale.to = new Vector3(scale, scale, scale);

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);
    }

    /// <summary>
    /// 装备出售成功消息回调
    /// </summary>
    void OnMsgSellEquip(string cmd, LPCValue para)
    {
        // 清空查看列表
        if (mSelectRid.Contains(mCurViewRid))
        {
            mCurViewRid = string.Empty;

            // 关闭装备查看窗口
            WindowMgr.DestroyWindow(EquipViewWnd.WndType);
        }

        // 清空选择列表
        mSelectRid.Clear();

        CheckItemIsAllEmpety();
    }

    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(this.gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
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
            rowItemOb.transform.parent = mUIWrapContent.transform;
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

                if(!posWnd.activeSelf)
                    posWnd.SetActive(true);

                mPetPosObMap.Add (string.Format ("equip_item_{0}_{1}", i, j), posWnd);

                // 注册点击事件
                UIEventListener.Get(posWnd).onClick = OnEquipItemClicked;
            }
        }

        mEquipItem.SetActive(false);
    }

    void RedrawData()
    {
        // 次要属性
        LPCArray minorProp = mCondition.GetValue<LPCArray>(EquipConst.MINOR_PROP);

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
        else if(minorProp.Count == 2)
        {
            // 反转排序
            if (reverse == 1 && (minorProp[0].AsInt != -1 || minorProp[1].AsInt != -1))
                equipData.Reverse();
        }

        mSelectAmount.text = string.Format(LocalizationMgr.Get("MultipleSellWnd_2"), mSelectRid.Count);

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
        mUIWrapContent.maxIndex = 0;

        mUIWrapContent.minIndex = - rowNum;

        mUIWrapContent.enabled = true;
    }

    void Redraw(bool resetPosition = false)
    {
        RedrawData();

        if (resetPosition)
        {
            foreach (GameObject item in rePosition.Keys)
            {
                item.transform.localPosition = rePosition[item];
            }

            // 整理位置
            ScrollView.ResetPosition();

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
    /// 刷新筛选的装备
    /// </summary>
    void RefreshFilterEquip(object para, params object[] param)
    {
        // 筛选条件
        mCondition = param[0] as LPCMapping;

        // 条件改变重置选择列表
        mSelectRid.Clear();

        WindowMgr.DestroyWindow(EquipViewWnd.WndType);

        // 刷新页面
        Redraw(true);
    }

    /// <summary>
    /// 刷新选中状态
    /// </summary>
    void RedrawSelect()
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
                    if (mSelectRid.Contains(equipData[dataIndex].GetRid()))
                        item.SetCheck(true);
                    else
                        item.SetCheck(false);

                    if (equipData[dataIndex].GetRid().Equals(mCurViewRid))
                        item.SetSelected(true);
                    else
                        item.SetSelected(false);
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
    /// 相应ScrowView拖动事件,PetData在此阶段数据不变，可以复用
    /// </summary>
    /// <param name="ob">Ob.</param>
    /// <param name="index">Index.</param>
    /// <param name="realindex">Realindex.</param>
    void OnUpdateItem(GameObject ob, int index, int realindex)
    {
        // 将index与realindex对应关系记录下来
        if(!indexMap.ContainsKey(index))
            indexMap.Add(index, realindex);
        else
            indexMap[index] = realindex;

        FillData(index, realindex);
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

                string rid = item.ItemOb.GetRid();

                item.SetCheck(mSelectRid.Contains(rid));

                item.SetSelected(rid.Equals(mCurViewRid));
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
    /// 包裹变化回调
    /// </summary>
    /// <param name="pos">Position.</param>
    void BaggageChange(string[] pos)
    {
#if UNITY_EDITOR
        // 延迟刷新计时器
        MergeExecuteMgr.DispatchExecute(DoDelayedRefresh);
#endif
    }

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
        else
            Redraw(false);
    }

    void DoDelayedRefresh()
    {
        // 窗口没有显示，不处理
        if (gameObject == null ||
            ! gameObject.activeSelf ||
            ! gameObject.activeInHierarchy)
            return;

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 装备格子被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnEquipItemClicked(GameObject go)
    {
        // 取得gameobject上绑定的script
        EquipItemWnd script = go.GetComponent<EquipItemWnd>();

        Property ob = script.ItemOb;

        if (ob == null)
            return;

        // 清除新物品标识
        BaggageMgr.ClearNewField(ob);

        script.SetNewTips(ob);

        string rid = ob.GetRid();

        if (script.mIsCheck)
        {
            mSelectRid.Remove(rid);
        }
        else
        {
            if (!mSelectRid.Contains(rid))
                mSelectRid.Add(rid);
        }

        mCurViewRid = rid;

        // 刷新选中状态
        RedrawSelect();

        mSelectAmount.text = string.Format(LocalizationMgr.Get("MultipleSellWnd_2"), mSelectRid.Count);

        // 打开装备查看界面
        GameObject wnd = WindowMgr.OpenWnd(EquipViewWnd.WndType, transform, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            wnd = WindowMgr.GetWindow(EquipViewWnd.WndType);

        if (wnd == null)
            return;

        // 调整窗口位置
        wnd.transform.localPosition = new Vector3(-265, -134);

        wnd.GetComponent<UIPanel>().depth = 1050;

        EquipViewWnd View = wnd.GetComponent<EquipViewWnd>();
        if (View == null)
            return;

        // 绑定数据
        View.Bind(rid, string.Empty, new CallBack(OnViewCallBack), false);

        // 隐藏按钮
        View.SetBtnActive(false);
    }

    /// <summary>
    /// 装备查看窗口关闭回调
    /// </summary>
    void OnViewCallBack(object para, params object[] param)
    {
        mCurViewRid = string.Empty;

        // 刷新选中
        RedrawSelect();
    }

    /// <summary>
    /// 窗口关闭按钮点击回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        mFilterWnd.GetComponent <FilterWnd>().mIsCurClose = true;

        if (mCb != null)
            mCb.Go();

        // 关闭当前窗口
        WindowMgr.HideWindow(gameObject);
    }

    /// <summary>
    /// 全选按钮点击回调
    /// </summary>
    void OnClickAllSelectBtn(GameObject go)
    {
        DialogMgr.ShowSimpleDailog(new CallBack(OnAllSelectCallBack),
            LocalizationMgr.Get("MultipleSellWnd_8"),
            LocalizationMgr.Get("MultipleSellWnd_7")
        );
    }

    /// <summary>
    /// 确认框点击回调
    /// </summary>
    void OnAllSelectCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        mSelectRid.Clear();

        // 缓存需要选中的装备
        foreach (Property ob in equipData)
        {
            if (ob == null)
                continue;

            mSelectRid.Add(ob.GetRid());
        }

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 智能全选按钮点击回调
    /// </summary>
    void OnClickIntelligentSelectBtn(GameObject go)
    {
        // 智能全选
        List<Property> equips = BaggageMgr.GetItemsByCustom(ME.user, equipData);

        // 没有符合条件的装备
        if (equips.Count == 0)
        {
            DialogMgr.ShowSimpleSingleBtnDailog(
                null,
                LocalizationMgr.Get("MultipleSellWnd_9"),
                LocalizationMgr.Get("MultipleSellWnd_5")
            );

            return;
        }

        // 清空缓存列表
        mSelectRid.Clear();

        // 缓存需要选中的装备
        foreach (Property ob in equips)
        {
            if (ob == null)
                continue;

            mSelectRid.Add(ob.GetRid());
        }

        // 重新绘制窗口
        Redraw();
    }

    /// <summary>
    /// 出售按钮的点击回调
    /// </summary>
    void OnClickSellBtn(GameObject go)
    {
        LPCMapping para = LPCMapping.Empty;

        LPCMapping price = LPCMapping.Empty;

        string fields = string.Empty;

        foreach (string rid in mSelectRid)
        {
            if (string.IsNullOrEmpty(rid))
                continue;

            if (! para.ContainsKey(rid))
                para.Add(rid, 1);

            // 装备对象
            Property ob = Rid.FindObjectByRid(rid);
            if (ob == null)
                continue;

            // 获取装备的出售价格
            LPCMapping sell = PropertyMgr.GetSellPrice(ob);
            if (sell == null || sell.Count == 0)
                continue;

            fields = FieldsMgr.GetFieldInMapping(sell);

            if (price.ContainsKey(fields))
                price[fields].AsInt += sell[fields].AsInt;
            else
                price.Add(fields, sell[fields].AsInt);
        }

        if (mSelectRid.Count > 0)
        {
            fields = FieldsMgr.GetFieldInMapping(price);

            string iconDesc = string.Format(LocalizationMgr.Get("EquipViewWnd_8"), FieldsMgr.GetFieldIcon(fields), 46, 46);
            string desc = string.Format(LocalizationMgr.Get("MultipleSellWnd_10"), price.GetValue<int>(fields), iconDesc);
            DialogMgr.ShowDailog(
                new CallBack(OnConfirmSellCallBack, para),
                desc,
                string.Empty,
                LocalizationMgr.Get("MultipleSellWnd_11"),
                string.Empty,
                true,
                this.transform
            );
        }
        else
        {
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("EquipWnd_8"),
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
        }
    }

    void OnConfirmSellCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        LPCMapping data = LPCMapping.Empty;

        data.Add("rid", para as LPCMapping);

        // 通知服务器出售装备
        Operation.CmdSellItem.Go(data);
    }

    public void ClearData()
    {
        // 清空数据
        mFilterWnd.GetComponent<FilterWnd>().ClearData();
    }

    public void SetCallBack(CallBack cb)
    {
        mCb = cb;
    }
}
